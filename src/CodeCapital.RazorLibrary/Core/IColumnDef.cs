using Microsoft.AspNetCore.Components;

namespace CodeCapital.RazorLibrary.Core
{
    public interface IColumnDef
    {
        public RenderFragment ChildContent { get; }

        public SortStatus SortStatus { get; set; }

        public string? SortBy { get; set; }

        public void ResetSort();
    }
}
