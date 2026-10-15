using System.Globalization;
using Microsoft.Extensions.Localization;

namespace WebApplication1.Resources;

// Preserve the existing resource filenames while exposing the typed localizer.
public sealed class UsuarioLocalizer(IStringLocalizerFactory factory) : IStringLocalizer<SharedResource>
{
    private IStringLocalizer Current => factory.Create(
        CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "pt" ? "resource_ptbr" : "resource_en",
        typeof(SharedResource).Assembly.GetName().Name!);

    public LocalizedString this[string name] => Current[name];
    public LocalizedString this[string name, params object[] arguments] => Current[name, arguments];
    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => Current.GetAllStrings(includeParentCultures);
}
