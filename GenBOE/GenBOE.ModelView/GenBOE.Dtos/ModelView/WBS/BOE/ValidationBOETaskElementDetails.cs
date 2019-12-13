using System.Diagnostics.CodeAnalysis;
using System.Collections.ObjectModel;

namespace GenBOE.Dtos
{
    [ExcludeFromCodeCoverage]
    public class ValidationBOETaskElementDetails
    {
        public ValidationBOETaskElementDetails()
        {
            TaskElementDetailsHeader = string.Empty;
            TaskElementDetailValidationMessages = new Collection<string>();
        }

        // the Task Element Details Header will either say "Task Element Details" or be empty if no 
        // validation needs to occur
        public string TaskElementDetailsHeader { get; set; }

        // this is a collection of the validation messages if needed
        public Collection<string> TaskElementDetailValidationMessages { get; set; }
    }
}
