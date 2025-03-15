using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Northwind.Mvc.TagHelpers;
// [HtmlTargetElement("email", TagStructure = TagStructure.WithoutEndTag)] 
public class EmailTagHelper: TagHelper
{
    private const string EmailDomain = "contoso.com";

    // Can be passed via <email mail-to="..." />. 
    // PascalCase gets translated into kebab-case.
    [HtmlAttributeName("recipient")]
    public string MailTo { get; set; }
    // public string SendTo { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "a"; // Replaces <email> with <a> tag

        var address = MailTo + "@" + EmailDomain;
        // var address = SendTo + "@" + EmailDomain;
        output.Attributes.SetAttribute("href", "mailto:" + address);
        output.Content.SetContent(address);
    }
}
