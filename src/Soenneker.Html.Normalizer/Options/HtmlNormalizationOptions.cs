using System.Collections.Generic;

namespace Soenneker.Html.Normalizer.Options;

/// <summary>
/// Controls which non-content artifacts are removed and how HTML is canonicalized.
/// </summary>
public sealed class HtmlNormalizationOptions
{
    /// <summary>
    /// Gets or sets whether HTML comment nodes are removed.
    /// </summary>
    /// <remarks>
    /// Enabled by default because comments commonly contain build metadata, render markers, or diagnostics that do not affect indexable content.
    /// </remarks>
    public bool RemoveComments { get; set; } = true;

    /// <summary>
    /// Gets or sets whether all <c>style</c> elements are removed.
    /// </summary>
    /// <remarks>
    /// Enabled by default so generated CSS, font subsets, and runtime style injection do not change the normalized hash.
    /// Inline <c>style</c> attributes are not affected.
    /// </remarks>
    public bool RemoveStyleElements { get; set; } = true;

    /// <summary>
    /// Gets or sets whether script elements are removed unless their trimmed <c>type</c> attribute is <c>application/ld+json</c>.
    /// </summary>
    /// <remarks>
    /// Enabled by default. JSON-LD is retained because it can contain meaningful structured search-index content.
    /// </remarks>
    public bool RemoveNonJsonLdScriptElements { get; set; } = true;

    /// <summary>
    /// Gets or sets whether <c>nonce</c> attributes are removed from all elements.
    /// </summary>
    /// <remarks>
    /// Enabled by default because content-security-policy nonces are normally unique per response.
    /// </remarks>
    public bool RemoveNonceAttributes { get; set; } = true;

    /// <summary>
    /// Gets or sets whether attributes whose names begin with <c>_bl_</c> are removed.
    /// </summary>
    /// <remarks>
    /// Enabled by default. Blazor generates these attributes for element references, and their values are not stable between renders.
    /// </remarks>
    public bool RemoveBlazorElementReferenceAttributes { get; set; } = true;

    /// <summary>
    /// Gets or sets whether <c>id</c> attributes are removed from all elements.
    /// </summary>
    /// <remarks>
    /// Enabled by default because component libraries commonly generate a new identifier on every render. Normalized HTML is intended for
    /// comparison and hashing, so preserving functional fragment targets is not required.
    /// </remarks>
    public bool RemoveIdAttributes { get; set; } = true;

    /// <summary>
    /// Gets or sets whether attributes that reference element IDs are removed.
    /// </summary>
    /// <remarks>
    /// Enabled by default. This includes common HTML and ARIA ID-reference attributes such as <c>for</c>, <c>headers</c>,
    /// <c>aria-labelledby</c>, <c>aria-describedby</c>, and <c>aria-controls</c>.
    /// </remarks>
    public bool RemoveIdReferenceAttributes { get; set; } = true;

    /// <summary>
    /// Removes whitespace-only text nodes outside whitespace-sensitive elements such as
    /// <c>pre</c>, <c>textarea</c>, <c>script</c>, and <c>style</c>.
    /// </summary>
    /// <remarks>
    /// Enabled by default to prevent formatting and indentation changes between elements from changing the normalized hash.
    /// Disable this when whitespace between inline elements must be treated as significant.
    /// </remarks>
    public bool RemoveInterElementWhitespace { get; set; } = true;

    /// <summary>
    /// Sorts ordinary HTML attributes by name. Namespaced attributes are left in their original order.
    /// </summary>
    /// <remarks>
    /// Enabled by default because HTML attribute order is not semantically significant. Sorting makes equivalent elements serialize identically.
    /// </remarks>
    public bool SortAttributes { get; set; } = true;

    /// <summary>
    /// Gets or sets whether CRLF and CR line endings are converted to LF after serialization.
    /// </summary>
    /// <remarks>Enabled by default so platform-specific line endings do not affect the normalized hash.</remarks>
    public bool NormalizeLineEndings { get; set; } = true;

    /// <summary>
    /// Gets or sets whether leading and trailing whitespace is removed from the final serialized value.
    /// </summary>
    /// <remarks>Enabled by default.</remarks>
    public bool TrimResult { get; set; } = true;

    /// <summary>
    /// Gets the CSS selectors for elements that should be removed before serialization.
    /// </summary>
    /// <remarks>
    /// Selectors are evaluated in insertion order. An invalid selector causes normalization to fail with the parser's selector exception.
    /// The collection is empty by default.
    /// </remarks>
    public IList<string> RemoveSelectors { get; } = new List<string>();

    /// <summary>
    /// Gets the additional attribute names to remove from every element.
    /// </summary>
    /// <remarks>Matching is case-insensitive. The collection is empty by default.</remarks>
    public ISet<string> RemoveAttributes { get; } = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the ordered replacements applied after DOM serialization.
    /// </summary>
    /// <remarks>
    /// Use replacements for application-generated identifiers, timestamps, request values, or other unstable text that cannot be addressed by
    /// removing an element or attribute. Each replacement receives the output of the preceding replacement.
    /// </remarks>
    public IList<HtmlNormalizationReplacement> Replacements { get; } = new List<HtmlNormalizationReplacement>();
}
