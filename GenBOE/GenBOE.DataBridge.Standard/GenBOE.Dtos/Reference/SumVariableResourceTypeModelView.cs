using System;
using System.Diagnostics.CodeAnalysis;

namespace GenBOE.Dtos
{
    [ExcludeFromCodeCoverage]
    [Serializable()]
    public class SumVariableResourceTypeModelView
    {
        public SumVariableResourceTypeModelView()
        {
            SumVariableResourceTypeID = 0;
            SumVariableResourceTypeName = string.Empty;
        }

        public int SumVariableResourceTypeID { get; set; }
        public string SumVariableResourceTypeName { get; set; }

    }
}
