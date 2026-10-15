using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace WebApplication1.Resources;

public sealed class LocalizedModelBinding(IStringLocalizer<SharedResource> localizer) : IConfigureOptions<MvcOptions>
{
    public void Configure(MvcOptions options)
    {
        var messages = options.ModelBindingMessageProvider;
        messages.SetAttemptedValueIsInvalidAccessor((_, _) => localizer["InvalidField"].Value);
        messages.SetNonPropertyAttemptedValueIsInvalidAccessor(_ => localizer["InvalidField"].Value);
        messages.SetUnknownValueIsInvalidAccessor(_ => localizer["InvalidField"].Value);
        messages.SetNonPropertyUnknownValueIsInvalidAccessor(() => localizer["InvalidField"].Value);
        messages.SetValueIsInvalidAccessor(_ => localizer["InvalidField"].Value);
        messages.SetValueMustNotBeNullAccessor(_ => localizer["RequiredField"].Value);
        messages.SetMissingBindRequiredValueAccessor(_ => localizer["RequiredField"].Value);
        messages.SetMissingKeyOrValueAccessor(() => localizer["RequiredField"].Value);
        messages.SetMissingRequestBodyRequiredValueAccessor(() => localizer["RequiredField"].Value);
        messages.SetValueMustBeANumberAccessor(_ => localizer["NumberField"].Value);
        messages.SetNonPropertyValueMustBeANumberAccessor(() => localizer["NumberField"].Value);
    }
}
