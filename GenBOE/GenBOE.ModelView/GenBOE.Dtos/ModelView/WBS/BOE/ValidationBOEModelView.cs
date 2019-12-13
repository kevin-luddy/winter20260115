using System;
using System.Linq;
using System.Diagnostics.CodeAnalysis;
using System.Collections.ObjectModel;

using IES.Common;

namespace GenBOE.Dtos
{
    /// <summary>
    /// This model view is used to return any validation messages that need to be shown
    /// to a user after the "Validate" button has been selected
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ValidationBOEModelView
    {
        public ValidationBOEModelView()
        {
            BOEName = "";
            BOEID = -1;
            BOEHeaderMsgs = new Collection<string>();
            BOECustomFieldValidationMessages = new Collection<string>();
            Tasks = new Collection<ValidationBOETasks>();
            BOECommentandApprovals = new Collection<string>();
            Costs = new Collection<ValidationBOETasks>();
            Materials = new Collection<ValidationBOETasks>();
            Travels = new Collection<ValidationBOETasks>();

        }
        //the ID of the BOE that is being validated.
        public int BOEID { get; set; }

        //The name of the BOE that is being Validated.
        public String BOEName { get; set; }

        // this is a collection of tasks that have been validated
        public Collection<ValidationBOETasks> Tasks { get; set; }

        // this is a collection of items contain in the boe header that have been validated
        public Collection<string> BOEHeaderMsgs { get; set; }

        // this is a collection of boe custom fields that have been validated
        public Collection<string> BOECustomFieldValidationMessages { get; set; }

        public Collection<string> BOECommentandApprovals { get; set; }

        // this is a collection of costs that have been validated
        public Collection<ValidationBOETasks> Costs { get; set; }

        // this is a collection of materials that have been validated
        public Collection<ValidationBOETasks> Materials { get; set; }

        // this is a collection of travels that have been validated
        public Collection<ValidationBOETasks> Travels { get; set; }

        /// <summary>
        /// this should be updated when you add anymore types of validation Collections to this MV. 
        /// </summary>
        public Boolean isValid
        {
            get
            {
                if (BOEHeaderMsgs.Any()
                    || Tasks.Any()
                    || BOECommentandApprovals.Any()
                    || BOECustomFieldValidationMessages.Any()
                    || Costs.Any()
                    || Materials.Any()
                    || Travels.Any())
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        /// <summary>
        /// If the BOE state was changed as a result of the validation, then this will be set to the new state.. If not, it will be null.
        /// </summary>
        public BOEState? NewState { get; set; }
    }
}
