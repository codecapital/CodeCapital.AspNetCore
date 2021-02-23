﻿using System.Collections.Generic;

namespace CodeCapital.RazorLibrary.Core
{
    public interface ITable
    {
        public List<IColumnDef> Columns { get; set; }
        public void AddColumn(IColumnDef column);
        public void SortBy(IColumnDef column);
    }
}
