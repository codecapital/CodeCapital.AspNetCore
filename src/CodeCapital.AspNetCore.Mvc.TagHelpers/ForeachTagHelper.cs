using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeCapital.AspNetCore.Mvc.TagHelpers
{
    /// <summary>
    /// Iterate the collections
    /// </summary>
    [HtmlTargetElement(Attributes = "asp-foreach")]
    public class ForeachTagHelper : TagHelper
    {
        [HtmlAttributeName("asp-foreach")] public IEnumerable<object> IterateOver { get; set; } = new List<object>();

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (!IterateOver.Any()) return;

            var content = output.Content;
            var tag = $"<{output.TagName} class='list-group-item justify-content-between'>{{0}}</{output.TagName}>";
            output.TagName = "";

            // extract property item.GetType().GetProperty("OrderD1ate").GetValue(item, null)

            foreach (var item in IterateOver)
            {
                var childContent = await output.GetChildContentAsync(false);
                output.Content.AppendHtml(String.Format(tag, childContent.GetContent()));
            }
        }
    }
}
