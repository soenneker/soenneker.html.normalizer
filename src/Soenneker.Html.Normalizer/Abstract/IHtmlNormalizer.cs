using System.Threading;
using System.Threading.Tasks;
using Soenneker.Html.Normalizer.Models;
using Soenneker.Html.Normalizer.Options;

namespace Soenneker.Html.Normalizer.Abstract;

/// <summary>
/// Produces deterministic HTML suitable for comparison, change detection, and hashing.
/// </summary>
public interface IHtmlNormalizer
{
    /// <summary>
    /// Normalizes a document or fragment using conservative defaults or the supplied options.
    /// </summary>
    /// <param name="html">Rendered page HTML to inspect.</param>
    /// <param name="options">Options to configure for the html normalizer.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task whose result is the text returned by normalize.</returns>
    ValueTask<string> Normalize(string? html, HtmlNormalizationOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Normalizes HTML and computes its lowercase hexadecimal XXH3 hash.
    /// </summary>
    /// <param name="html">Rendered page HTML to inspect.</param>
    /// <param name="options">Options to configure for the html normalizer.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task whose result is the requested HTML Normalization Result.</returns>
    ValueTask<HtmlNormalizationResult> NormalizeAndHash(string? html, HtmlNormalizationOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Computes a lowercase hexadecimal XXH3 hash of a value without normalizing it.
    /// </summary>
    /// <param name="value">Text whose XXH3 hash should be computed.</param>
    /// <returns>The resulting text.</returns>
    string ComputeHash(string value);
}
