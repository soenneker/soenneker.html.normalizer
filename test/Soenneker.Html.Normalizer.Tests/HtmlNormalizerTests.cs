using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Soenneker.Html.Normalizer.Abstract;
using Soenneker.Html.Normalizer.Models;
using Soenneker.Html.Normalizer.Options;
using Soenneker.Tests.HostedUnit;

namespace Soenneker.Html.Normalizer.Tests;

[ClassDataSource<Host>(Shared = SharedType.PerTestSession)]
public sealed class HtmlNormalizerTests : HostedUnitTest
{
    private readonly IHtmlNormalizer _normalizer;

    public HtmlNormalizerTests(Host host) : base(host)
    {
        _normalizer = Resolve<IHtmlNormalizer>(true);
    }

    [Test]
    public async Task Runtime_artifacts_produce_the_same_normalized_html()
    {
        const string first = """
            <main nonce="first" _bl_123="">
              <!-- render marker -->
              <style>.first { color: red; }</style>
              <script>window.runtimeId = 'first';</script>
              <h1 class="title" id="hero">Leadping</h1>
            </main>
            """;
        const string second = """
            <main _bl_456="" nonce="second"><style>.second { color: blue; }</style>
              <script>window.runtimeId = 'second';</script><!-- another marker -->
              <h1 id="hero" class="title">Leadping</h1></main>
            """;

        string normalizedFirst = await _normalizer.Normalize(first);
        string normalizedSecond = await _normalizer.Normalize(second);

        await Assert.That(normalizedFirst).IsEqualTo(normalizedSecond);
    }

    [Test]
    public async Task JsonLd_and_indexable_content_are_preserved()
    {
        const string original = """
            <html><head><script type="application/ld+json">{"name":"Original"}</script></head>
            <body><a href="/pricing">Original content</a></body></html>
            """;
        const string changed = """
            <html><head><script type="application/ld+json">{"name":"Changed"}</script></head>
            <body><a href="/contact">Changed content</a></body></html>
            """;

        string normalizedOriginal = await _normalizer.Normalize(original);
        string normalizedChanged = await _normalizer.Normalize(changed);

        await Assert.That(normalizedOriginal).Contains("application/ld+json");
        await Assert.That(normalizedOriginal).IsNotEqualTo(normalizedChanged);
    }

    [Test]
    public async Task Custom_replacements_normalize_application_generated_values()
    {
        var options = new HtmlNormalizationOptions();
        options.Replacements.Add(new HtmlNormalizationReplacement(
            "session-[a-f0-9]{32}", "session-id", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant));

        const string first = "<div data-session=\"session-342082340f974064841b23af31f8abf4\">Content</div>";
        const string second = "<div data-session=\"session-972d5276a61546eea574e1373ef559d6\">Content</div>";

        await Assert.That(await _normalizer.Normalize(first, options))
                    .IsEqualTo(await _normalizer.Normalize(second, options));
    }

    [Test]
    public async Task Randomized_ids_and_their_references_are_removed_by_default()
    {
        const string first = """
            <section id="panel-342082340f974064841b23af31f8abf4" aria-labelledby="title-342082340f974064841b23af31f8abf4">
              <h2 id="title-342082340f974064841b23af31f8abf4">Cookies</h2>
              <label for="input-342082340f974064841b23af31f8abf4">Choice</label>
              <input id="input-342082340f974064841b23af31f8abf4">
            </section>
            """;
        const string second = """
            <section id="panel-972d5276a61546eea574e1373ef559d6" aria-labelledby="title-972d5276a61546eea574e1373ef559d6">
              <h2 id="title-972d5276a61546eea574e1373ef559d6">Cookies</h2>
              <label for="input-972d5276a61546eea574e1373ef559d6">Choice</label>
              <input id="input-972d5276a61546eea574e1373ef559d6">
            </section>
            """;

        await Assert.That(await _normalizer.Normalize(first)).IsEqualTo(await _normalizer.Normalize(second));
    }

    [Test]
    public async Task Selectors_and_attributes_can_be_removed()
    {
        var options = new HtmlNormalizationOptions();
        options.RemoveSelectors.Add("[data-runtime]");
        options.RemoveAttributes.Add("data-request-id");

        const string html = "<main data-request-id=\"abc\"><span data-runtime>noise</span><p>Content</p></main>";
        string normalized = await _normalizer.Normalize(html, options);

        await Assert.That(normalized).IsEqualTo("<main><p>Content</p></main>");
    }

    [Test]
    public async Task Normalize_and_hash_returns_a_repeatable_xxhash3_hash()
    {
        HtmlNormalizationResult first = await _normalizer.NormalizeAndHash("<main id='content' class='page'>Leadping</main>");
        HtmlNormalizationResult second = await _normalizer.NormalizeAndHash("<main class=\"page\" id=\"content\">Leadping</main>");

        await Assert.That(first.Html).IsEqualTo(second.Html);
        await Assert.That(first.Hash).IsEqualTo(second.Hash);
        await Assert.That(first.Hash.Length).IsEqualTo(16);
    }
}
