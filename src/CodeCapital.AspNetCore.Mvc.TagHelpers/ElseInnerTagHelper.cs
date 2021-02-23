using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Threading.Tasks;

// ReSharper disable MustUseReturnValue

namespace CodeCapital.AspNetCore.Mvc.TagHelpers
{
    /// <summary>
    /// If the conditions is false, the content will be rendered but parent tag removed
    /// </summary>
    [Obsolete("Use instead <asp-if condition=\"\" negate> IfElementTagHelper")]
    [HtmlTargetElement(Attributes = "asp-else-inner")]
    public class ElseInnerTagHelper : TagHelper
    {
        [HtmlAttributeName("asp-else-inner")]
        public bool Condition { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (Condition) output.SuppressOutput();
            else
            {
                var content = await output.GetChildContentAsync();
                output.TagName = "";
                output.Content.SetHtmlContent(content.GetContent().Trim());
            }
        }
    }
}