using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Threading.Tasks;

namespace CodeCapital.AspNetCore.Mvc.TagHelpers
{
    /// <summary>
    /// If the conditions is true, the tag will be rendered
    /// </summary>
    [HtmlTargetElement(Attributes = "asp-is-authorized")]
    public class IsAuthorized : TagHelper
    {
        private readonly IAuthorizationService _authorizationService;
        private HttpRequest Request => ViewContext.HttpContext.Request;

        public IsAuthorized(IAuthorizationService authorizationService) => _authorizationService = authorizationService;

        [ViewContext] public ViewContext ViewContext { get; set; } = null!;

        [HtmlAttributeName("asp-is-authorized")]
        public string Policy { get; set; } = string.Empty;

        //public override void Process(TagHelperContext context, TagHelperOutput output)
        //{
        //    output.SuppressOutput();
        //}

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var authorizationResult = await _authorizationService.AuthorizeAsync(Request.HttpContext.User, Policy);

            if (!authorizationResult.Succeeded) output.SuppressOutput();
        }
    }
}
