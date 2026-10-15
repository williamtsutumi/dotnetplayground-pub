using Microsoft.AspNetCore.Razor.TagHelpers;

namespace WebApplication1.TagHelpers;

[HtmlTargetElement("button", Attributes = "data-preview")]
[HtmlTargetElement("button", Attributes = "data-blocked")]
[HtmlTargetElement("button", Attributes = "disabled")]
public class BlockedButtonTagHelper(Microsoft.Extensions.Localization.IStringLocalizer<SharedResource> localizer) : TagHelper
{
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var classes = output.Attributes["class"]?.Value?.ToString() ?? "";
        output.Attributes.SetAttribute("class", classes + " blocked-control");
        output.Attributes.SetAttribute("disabled", "disabled");
        output.Attributes.SetAttribute("aria-disabled", "true");
        output.Attributes.SetAttribute("title", localizer["BlockedControlUnavailableTitle"].Value);
        output.PostContent.AppendHtml("<span class=\"blocked-overlay\" aria-hidden=\"true\"><svg viewBox=\"0 0 100 100\" preserveAspectRatio=\"none\"><path d=\"M0 100L100 0\" /></svg><span>");
        output.PostContent.Append(localizer["BlockedControlLabel"].Value);
        output.PostContent.AppendHtml("</span></span>");
    }
}
