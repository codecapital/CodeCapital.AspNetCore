using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Threading.Tasks;

// ReSharper disable MustUseReturnValue

namespace CodeCapital.AspNetCore.Mvc.TagHelpers
{
    /// <summary>
    /// If the conditions is true, the content will be rendered but parent tag removed
    /// </summary>
    [Obsolete("Use instead <asp-if condition=\"\"> IfElementTagHelper")]
    [HtmlTargetElement(Attributes = "asp-if-inner")]
    public class IfInnerTagHelper : TagHelper
    {
        [HtmlAttributeName("asp-if-inner")]
        public bool Condition { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (!Condition) output.SuppressOutput();
            else
            {
                var content = await output.GetChildContentAsync();
                output.TagName = "";
                output.Content.SetHtmlContent(content.GetContent().Trim());
            }
        }
    }
}