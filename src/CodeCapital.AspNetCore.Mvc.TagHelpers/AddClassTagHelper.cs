using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Collections.Generic;
using System.Linq;

namespace CodeCapital.AspNetCore.Mvc.TagHelpers
{
    /// <summary>
    /// Conditional add class, asp-class-myclass, myclass will be added if the conditions is true
    /// </summary>
    [HtmlTargetElement(Attributes = AspClassPrefix + "*")]
    public class AddClassTagHelper : TagHelper
    {
        private const string AspClassPrefix = "asp-class-";

        [HtmlAttributeName(DictionaryAttributePrefix = AspClassPrefix)]
        public IDictionary<string, bool> Condition { get; set; } = new Dictionary<string, bool>();

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (Condition.Count == 0) return;

            //foreach (var condition in Condition)
            //{
            //    if (!condition.Value) continue;

            //    output.Attributes.TryGetAttribute("class", out var attributeClass);

            //    if (attributeClass is null)
            //        output.Attributes.SetAttribute("class", condition.Key);
            //    else
            //    {
            //        var items = attributeClass.Value.ToString()?.Split(' ').ToList();

            //        if (items is null || items.Contains(condition.Key)) continue;

            //        items.Add(condition.Key);

            //        output.Attributes.SetAttribute("class", string.Join(" ", items));
            //    }
            //}

            var items = Condition.Where(w => w.Value).Select(s => s.Key).ToList();

            if (items.Count == 0) return;

            output.Attributes.TryGetAttribute("class", out var attributeClass);

            if (attributeClass is null)
                output.Attributes.SetAttribute("class", string.Join(" ", items));
            else
            {
                var currentItems = attributeClass.Value.ToString()?.Split(' ').ToList() ?? new List<string>();

                output.Attributes.SetAttribute("class", string.Join(" ", currentItems.Union(items)));
            }
        }
    }
}
