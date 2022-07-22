using Microsoft.AspNetCore.Razor.TagHelpers;

namespace CodeCapital.AspNetCore.Mvc.TagHelpers
{
    /// <summary>
    /// If the conditions is true or not null, the tag will be rendered
    /// </summary>
    [HtmlTargetElement(Attributes = "asp-if")]
    public class IfTagHelper : TagHelper
    {
        [HtmlAttributeName("asp-if")]
        public object? Condition { get; set; }

        public bool Negate { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var state = Condition == null
                        || Condition is bool condition && !condition
                        || Condition is string stringCondition && string.IsNullOrWhiteSpace(stringCondition);

            if (Negate) state = !state;

            if (state) output.SuppressOutput();
        }
    }
}