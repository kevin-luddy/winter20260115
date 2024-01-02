using System;
using System.Diagnostics.CodeAnalysis;

namespace GenBOE.Dtos
{
    [ExcludeFromCodeCoverage]
    [Serializable()]

    public class SortByModelView
    {
        public SortByModelView()
        {
            SortByID = 0;
            SortBy = String.Empty;
        }

        public int SortByID { get; set; }

        public string SortBy { get; set; }
    }
}
