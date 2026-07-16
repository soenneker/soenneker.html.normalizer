namespace Soenneker.Html.Normalizer.Models;

/// <summary>
/// Contains normalized HTML and its lowercase hexadecimal XXH3 hash.
/// </summary>
public sealed record HtmlNormalizationResult(string Html, string Hash);
