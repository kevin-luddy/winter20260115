using System.Diagnostics.CodeAnalysis;
using System.Collections.ObjectModel;

namespace GenBOE.Dtos
{
    [ExcludeFromCodeCoverage]
    public class ValidationBOELaborType
    {
        public ValidationBOELaborType()
        {
            LaborTypeHeader = string.Empty;
            LaborTypeValidationMsgs = new Collection<string>();
        }

        // the Labor Type Header will be defined as Labor Type: if there is missing data for a labor type.
        // If there is no missing data, it will be an empty string
        public string LaborTypeHeader { get; set; }

        // a collection of validation messages if needed
        public Collection<string> LaborTypeValidationMsgs { get; set; }
    }


}
