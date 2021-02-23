using Microsoft.AspNetCore.Razor.TagHelpers;
using System;

namespace CodeCapital.AspNetCore.Mvc.TagHelpers
{
    /// <summary>
    /// If the conditions is false, the tag will be rendered
    /// </summary>
    [Obsolete("Use instead negate attribute")]
    [HtmlTargetElement(Attributes = "asp-else")]
    public class ElseTagHelper : TagHelper
    {
        [HtmlAttributeName("asp-else")]
        public bool Condition { get; set; }

        // The null option added, needs to be tested
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (Condition) output.SuppressOutput();
        }
    }
}
