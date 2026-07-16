using System;
using System.Text.RegularExpressions;

namespace Soenneker.Html.Normalizer.Options;

/// <summary>
/// Replaces a generated or otherwise unstable value in serialized HTML.
/// </summary>
public sealed class HtmlNormalizationReplacement
{
    /// <summary>
    /// Gets the regular expression matched against serialized HTML.
    /// </summary>
    public Regex Pattern { get; }

    /// <summary>
    /// Gets the replacement string passed to <see cref="Regex.Replace(string, string)"/>.
    /// </summary>
    public string Replacement { get; }

    /// <summary>
    /// Initializes a replacement using an existing regular expression.
    /// </summary>
    /// <param name="pattern">The expression matched against serialized HTML.</param>
    /// <param name="replacement">The replacement string. Regular-expression substitution syntax is supported.</param>
    public HtmlNormalizationReplacement(Regex pattern, string replacement)
    {
        Pattern = pattern ?? throw new ArgumentNullException(nameof(pattern));
        Replacement = replacement ?? throw new ArgumentNullException(nameof(replacement));
    }

    /// <summary>
    /// Initializes a replacement and compiles its regular expression.
    /// </summary>
    /// <param name="pattern">The regular-expression pattern matched against serialized HTML.</param>
    /// <param name="replacement">The replacement string. Regular-expression substitution syntax is supported.</param>
    /// <param name="regexOptions">
    /// Additional expression options. <see cref="RegexOptions.Compiled"/> is always added. The default is
    /// <see cref="RegexOptions.CultureInvariant"/>.
    /// </param>
    public HtmlNormalizationReplacement(string pattern, string replacement,
        RegexOptions regexOptions = RegexOptions.CultureInvariant) : this(
        new Regex(pattern, regexOptions | RegexOptions.Compiled), replacement)
    {
    }
}
