[![](https://img.shields.io/nuget/v/soenneker.html.normalizer.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.html.normalizer/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.html.normalizer/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.html.normalizer/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.html.normalizer.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.html.normalizer/)

# ![](https://user-images.githubusercontent.com/4441470/224455560-91ed3ee7-f510-4041-a8d2-3fc093025112.png) Soenneker.Html.Normalizer
### Deterministic, configurable HTML normalization for hashing and change detection

## Installation

```
dotnet add package Soenneker.Html.Normalizer
```

## Usage

```csharp
using Soenneker.Html.Normalizer.Models;

HtmlNormalizationResult result = await normalizer.NormalizeAndHash(renderedHtml);

Console.WriteLine(result.Html);
Console.WriteLine(result.Hash);
```

The default profile removes common runtime artifacts while preserving indexable content:

- HTML comments
- `<style>` elements
- scripts other than `application/ld+json`
- `nonce` attributes
- Blazor `_bl_*` element-reference attributes
- element IDs and common HTML/ARIA attributes that reference them
- formatting-only inter-element whitespace

It also normalizes line endings and sorts ordinary HTML attributes. Hashing uses XXH3 through
[`Soenneker.Hashing.XxHash`](https://www.nuget.org/packages/Soenneker.Hashing.XxHash), producing a lowercase 64-bit hexadecimal hash for fast,
non-adversarial change detection.

## Application-specific normalization

```csharp
using System.Text.RegularExpressions;
using Soenneker.Html.Normalizer.Options;

var options = new HtmlNormalizationOptions();

options.RemoveSelectors.Add("[data-runtime-only]");
options.RemoveAttributes.Add("data-request-id");
options.Replacements.Add(new HtmlNormalizationReplacement(
    "session-[a-f0-9]{32}",
    "session-id",
    RegexOptions.IgnoreCase | RegexOptions.CultureInvariant));

string normalized = await normalizer.Normalize(renderedHtml, options);
```

## Dependency injection

```csharp
using Soenneker.Html.Normalizer.Registrars;

services.AddHtmlNormalizerAsSingleton();
// or services.AddHtmlNormalizerAsScoped();
```

Browser readiness is intentionally outside this package. Wait for hydration or another application-specific readiness signal before capturing and normalizing rendered HTML.
