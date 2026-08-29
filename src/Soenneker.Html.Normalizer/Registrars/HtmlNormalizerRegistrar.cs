using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Soenneker.AngleSharp.Parser.Registrars;
using Soenneker.Html.Normalizer.Abstract;
namespace Soenneker.Html.Normalizer.Registrars;
/// <summary>
/// Registers the HTML normalizer and its parser dependency.
/// </summary>
public static class HtmlNormalizerRegistrar
{
    /// <summary>
    /// Registers HTML Normalizer with a singleton lifetime.
    /// </summary>
    /// <param name="services">Service collection that receives the registration.</param>
    /// <returns>The same service collection, so additional registrations can be chained.</returns>
    public static IServiceCollection AddHtmlNormalizerAsSingleton(this IServiceCollection services)
    {
        services.AddAngleSharpParserAsSingleton()
                .TryAddSingleton<IHtmlNormalizer, HtmlNormalizer>();
        return services;
    }

    /// <summary>
    /// Registers HTML Normalizer with a scoped lifetime.
    /// </summary>
    /// <param name="services">Service collection that receives the registration.</param>
    /// <returns>The same service collection, so additional registrations can be chained.</returns>
    public static IServiceCollection AddHtmlNormalizerAsScoped(this IServiceCollection services)
    {
        services.AddAngleSharpParserAsScoped()
                .TryAddScoped<IHtmlNormalizer, HtmlNormalizer>();
        return services;
    }
}
