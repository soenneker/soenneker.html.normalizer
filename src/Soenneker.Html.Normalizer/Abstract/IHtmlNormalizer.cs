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
    ValueTask<string> Normalize(string? html, HtmlNormalizationOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Normalizes HTML and computes its lowercase hexadecimal XXH3 hash.
    /// </summary>
    ValueTask<HtmlNormalizationResult> NormalizeAndHash(string? html, HtmlNormalizationOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Computes a lowercase hexadecimal XXH3 hash of a value without normalizing it.
    /// </summary>
    string ComputeHash(string value);
}
