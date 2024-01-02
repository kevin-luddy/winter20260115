using System.Diagnostics.CodeAnalysis;
using System;

namespace GenBOE.Dtos
{
    [ExcludeFromCodeCoverage]
    [Serializable()]
    public class MOQTypeModelView
    {
        public MOQTypeModelView()
        {
            MOQTypeID = 0;
            MOQTypeName = string.Empty;
        }

        public int MOQTypeID { get; set; }

        public string MOQTypeName { get; set; }
    }
}
