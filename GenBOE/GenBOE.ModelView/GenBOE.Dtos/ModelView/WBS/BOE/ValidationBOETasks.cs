using System.Diagnostics.CodeAnalysis;
using System.Collections.ObjectModel;

namespace GenBOE.Dtos
{
    [ExcludeFromCodeCoverage]
    public class ValidationBOETasks
    {
        public ValidationBOETasks()
        {
            TaskMessage = string.Empty;
            TaskElementDetails = new ValidationBOETaskElementDetails();
            LaborTypes = new Collection<ValidationBOELaborType>();
            TaskId = -1;
        }

        // the task Message is used on the Validation PopUp
        // if there is any missing data within a task, this string will say Task: <TaskTitle>
        // If there is no missing data, this string will be empty
        public string TaskMessage { get; set; }

        //the id for the taskelemnent, used so we can link back to the task on the validateallboe method.
        public int TaskId { get; set; }

        // the task element details to validate
        public ValidationBOETaskElementDetails TaskElementDetails { get; set; }

        // a collection of labor types to validate
        public Collection<ValidationBOELaborType> LaborTypes { get; set; }
    }
}
