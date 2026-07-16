using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Html;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Soenneker.AngleSharp.Parser.Abstract;
using Soenneker.AngleSharp.Parser.Enums;
using Soenneker.Extensions.Task;
using Soenneker.Extensions.ValueTask;
using Soenneker.Hashing.XxHash;
using Soenneker.Html.Normalizer.Abstract;
using Soenneker.Html.Normalizer.Models;
using Soenneker.Html.Normalizer.Options;

namespace Soenneker.Html.Normalizer;

/// <inheritdoc cref="IHtmlNormalizer"/>
public sealed class HtmlNormalizer : IHtmlNormalizer
{
    private static readonly HashSet<string> WhitespaceSensitiveElements = new(StringComparer.OrdinalIgnoreCase)
    {
        "pre", "textarea", "script", "style"
    };

    private static readonly HashSet<string> IdReferenceAttributes = new(StringComparer.OrdinalIgnoreCase)
    {
        "for",
        "form",
        "headers",
        "list",
        "popovertarget",
        "aria-activedescendant",
        "aria-controls",
        "aria-describedby",
        "aria-details",
        "aria-errormessage",
        "aria-flowto",
        "aria-labelledby",
        "aria-owns"
    };

    private readonly IAngleSharpParser _angleSharpParser;

    public HtmlNormalizer(IAngleSharpParser angleSharpParser)
    {
        _angleSharpParser = angleSharpParser;
    }

    public async ValueTask<string> Normalize(string? html, HtmlNormalizationOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(html))
            return string.Empty;

        options ??= new HtmlNormalizationOptions();
        string input = StripBom(html);
        string normalized = LooksLikeDocument(input)
            ? await NormalizeDocument(input, options, cancellationToken).NoSync()
            : await NormalizeFragment(input, options, cancellationToken).NoSync();

        foreach (HtmlNormalizationReplacement replacement in options.Replacements)
            normalized = replacement.Pattern.Replace(normalized, replacement.Replacement);

        if (options.NormalizeLineEndings)
            normalized = normalized.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n');

        return options.TrimResult ? normalized.Trim() : normalized;
    }

    public async ValueTask<HtmlNormalizationResult> NormalizeAndHash(string? html,
        HtmlNormalizationOptions? options = null, CancellationToken cancellationToken = default)
    {
        string normalized = await Normalize(html, options, cancellationToken).NoSync();
        return new HtmlNormalizationResult(normalized, ComputeHash(normalized));
    }

    public string ComputeHash(string value) => XxHash3Util.Hash(value);

    private async ValueTask<string> NormalizeDocument(string html, HtmlNormalizationOptions options,
        CancellationToken cancellationToken)
    {
        HtmlParser parser = await _angleSharpParser.Get(cancellationToken).NoSync();
        IHtmlDocument document = await parser.ParseDocumentAsync(html, cancellationToken).NoSync();
        NormalizeDom(document, options, cancellationToken);
        return Serialize(document, html.Length);
    }

    private async ValueTask<string> NormalizeFragment(string html, HtmlNormalizationOptions options,
        CancellationToken cancellationToken)
    {
        HtmlParser parser = await _angleSharpParser.Get(AngleSharpContextType.Fast, cancellationToken).NoSync();
        IHtmlDocument document = await parser.ParseDocumentAsync("<body></body>", cancellationToken).NoSync();
        IElement context = document.Body ?? document.CreateElement("body");
        INodeList nodes = parser.ParseFragment(html, context);

        foreach (INode node in nodes.ToArray())
            context.AppendChild(node);

        NormalizeDom(context, options, cancellationToken);

        var builder = new StringBuilder(Math.Max(html.Length + 64, 256));
        using var writer = new StringWriter(builder, CultureInfo.InvariantCulture);

        foreach (INode node in context.ChildNodes)
            node.ToHtml(writer, HtmlMarkupFormatter.Instance);

        return builder.ToString();
    }

    private static void NormalizeDom(INode root, HtmlNormalizationOptions options, CancellationToken cancellationToken)
    {
        if (root is IParentNode parent)
        {
            if (options.RemoveStyleElements)
                RemoveElements(parent.QuerySelectorAll("style"));

            if (options.RemoveNonJsonLdScriptElements)
            {
                RemoveElements(parent.QuerySelectorAll("script").Where(static script =>
                    !string.Equals(script.GetAttribute("type")?.Trim(), "application/ld+json",
                        StringComparison.OrdinalIgnoreCase)));
            }

            foreach (string selector in options.RemoveSelectors)
            {
                cancellationToken.ThrowIfCancellationRequested();
                RemoveElements(parent.QuerySelectorAll(selector));
            }
        }

        NormalizeNode(root, options, preserveWhitespace: false, cancellationToken);
    }

    private static void NormalizeNode(INode node, HtmlNormalizationOptions options, bool preserveWhitespace,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (node is IElement element)
        {
            preserveWhitespace |= WhitespaceSensitiveElements.Contains(element.LocalName);
            NormalizeAttributes(element, options);
        }

        foreach (INode child in node.ChildNodes.ToArray())
        {
            if (options.RemoveComments && child.NodeType == NodeType.Comment)
            {
                node.RemoveChild(child);
                continue;
            }

            if (options.RemoveInterElementWhitespace && !preserveWhitespace && child is IText text &&
                string.IsNullOrWhiteSpace(text.Data))
            {
                node.RemoveChild(child);
                continue;
            }

            NormalizeNode(child, options, preserveWhitespace, cancellationToken);
        }
    }

    private static void NormalizeAttributes(IElement element, HtmlNormalizationOptions options)
    {
        foreach (IAttr attribute in element.Attributes.ToArray())
        {
            bool remove = options.RemoveAttributes.Contains(attribute.Name) ||
                          options.RemoveNonceAttributes &&
                          attribute.Name.Equals("nonce", StringComparison.OrdinalIgnoreCase) ||
                          options.RemoveBlazorElementReferenceAttributes &&
                          attribute.Name.StartsWith("_bl_", StringComparison.OrdinalIgnoreCase) ||
                          options.RemoveIdAttributes &&
                          attribute.Name.Equals("id", StringComparison.OrdinalIgnoreCase) ||
                          options.RemoveIdReferenceAttributes &&
                          IdReferenceAttributes.Contains(attribute.Name);

            if (remove)
                element.RemoveAttribute(attribute.Name);
        }

        if (!options.SortAttributes || element.Attributes.Length < 2 ||
            element.Attributes.Any(static attribute => attribute.NamespaceUri is not null))
            return;

        var attributes = element.Attributes.Select(static attribute => (attribute.Name, attribute.Value))
                                .OrderBy(static attribute => attribute.Name, StringComparer.Ordinal).ToArray();

        foreach ((string name, _) in attributes)
            element.RemoveAttribute(name);

        foreach ((string name, string value) in attributes)
            element.SetAttribute(name, value);
    }

    private static void RemoveElements(IEnumerable<IElement> elements)
    {
        foreach (IElement element in elements.ToArray())
            element.Remove();
    }

    private static string Serialize(INode node, int originalLength)
    {
        var builder = new StringBuilder(Math.Max(originalLength + 64, 256));
        using var writer = new StringWriter(builder, CultureInfo.InvariantCulture);
        node.ToHtml(writer, HtmlMarkupFormatter.Instance);
        return builder.ToString();
    }

    private static bool LooksLikeDocument(string html) =>
        html.Contains("<!DOCTYPE", StringComparison.OrdinalIgnoreCase) ||
        html.Contains("<html", StringComparison.OrdinalIgnoreCase) ||
        html.Contains("<head", StringComparison.OrdinalIgnoreCase) ||
        html.Contains("<body", StringComparison.OrdinalIgnoreCase);

    private static string StripBom(string value) => value.Length > 0 && value[0] == '\uFEFF' ? value[1..] : value;
}
