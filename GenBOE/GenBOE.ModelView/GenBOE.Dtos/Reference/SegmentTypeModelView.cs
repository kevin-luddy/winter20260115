using System;
using System.Diagnostics.CodeAnalysis;

namespace GenBOE.Dtos
{
    [ExcludeFromCodeCoverage]
    [Serializable()]
    public class SegmentTypeModelView
    {
        public SegmentTypeModelView()
        {
            SegmentTypeID = 0;
            SegmentTypeName = string.Empty;
        }

        public int SegmentTypeID { get; set; }
        public string SegmentTypeName { get; set; }
    }
}
