using System.Diagnostics.CodeAnalysis;
using System;

namespace GenBOE.Dtos
{
    [ExcludeFromCodeCoverage]
    [Serializable()]
    public class FieldTypeModelView
    {
        public FieldTypeModelView()
        {
            FieldTypeID = 0;
            FieldTypeName = string.Empty;
        }

        public int FieldTypeID { get; set; }

        public string FieldTypeName { get; set; }
    }
}
