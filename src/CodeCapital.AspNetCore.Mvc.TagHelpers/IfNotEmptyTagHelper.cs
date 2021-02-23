using Microsoft.AspNetCore.Razor.TagHelpers;
using System;

namespace CodeCapital.AspNetCore.Mvc.TagHelpers
{
    /// <summary>
    /// If the conditions is true, the tag will be rendered
    /// </summary>
    [HtmlTargetElement(Attributes = "asp-if-not-empty")]
    [Obsolete("Use instead asp-if", true)]
    public class IfNotEmptyTagHelper : TagHelper
    {
        [HtmlAttributeName("asp-if-not-empty")]
        public string Condition { get; set; } = string.Empty;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (string.IsNullOrWhiteSpace(Condition)) output.SuppressOutput();
        }
    }
}
