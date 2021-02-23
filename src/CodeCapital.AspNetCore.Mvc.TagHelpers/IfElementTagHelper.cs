using Microsoft.AspNetCore.Razor.TagHelpers;

namespace CodeCapital.AspNetCore.Mvc.TagHelpers
{
    /// <summary>
    /// If the conditions is true or not null, the tag will be rendered
    /// </summary>
    [HtmlTargetElement("asp-if")]
    public class IfElementTagHelper : TagHelper
    {
        [HtmlAttributeName("condition")]
        public object? Condition { get; set; }

        public bool Negate { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = null;

            var state = Condition == null
                        || Condition is bool condition && !condition
                        || Condition is string stringCondition && string.IsNullOrWhiteSpace(stringCondition);

            if (Negate) state = !state;

            if (state) output.SuppressOutput();
        }
    }
}