// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.Validation
{
    using IES.Common;
    using IES.Common.Exceptions;

    public class ValidationFactory
    {
        private static ValidationFactory instance;

        private Logger _log = new Logger(typeof(ValidationFactory));

        private Validator _workspaceUniqueNameValidator;
        private Validator _workspaceUniqueShortnameValidator;
        private Validator _workspaceCostVolumeLeadNotGroupValidator;
        private Validator _boeTaskIdUniqueValidator;
        private Validator _BOEDateRangeValidator;
        private Validator _wbsUniqueNumberValidator;
        private Validator _resourceUniqueIDValidator;
        private Validator _performingOrgUniqueIDValidator;
        private Validator _wbsRenumberValidator;
        private Validator _boeWBSMoveValidator;
        private Validator _boeCLINMoveValidator;
        private Validator _boeLaborCostElementExistsValidator;
        private Validator _boeMaterialElementExistsValidator;
        private Validator _resourceUniqueDescValidator;
        private Validator _workspaceCostVolumeLeadNotSubcontractorValidator;


        public ValidationFactory(Validator inWorkspaceUniqueNameValidator,
            Validator inWorkspaceUniqueShortnameValidator,
            Validator inWorkspaceCostVolumeLeadNotGroupValidator,
            Validator inBoeTaskIDUniqueValidator,
            Validator inBOEDateRangeValidator,
            Validator inWbsUniqueNumberValidator,
            Validator inResourceUniqueIDValidator,
            Validator inPerformingOrgUniqueIDValidator,
            Validator inWBSRenumberValidator,
            Validator inBOEWBSMoveValidator,
            Validator inBOECLINMoveValidator,
            Validator inBoeLaborCostElementExistsValidator,
            Validator inBoeMaterialElementExistsValidator,
            Validator inResourceUniqueDescValidator,
            Validator inWorkspaceCostVolumeLeadNotSubcontractorValidator)
        {
            this._workspaceUniqueNameValidator = inWorkspaceUniqueNameValidator;
            this._workspaceUniqueShortnameValidator = inWorkspaceUniqueShortnameValidator;
            this._workspaceCostVolumeLeadNotGroupValidator = inWorkspaceCostVolumeLeadNotGroupValidator;
            this._boeTaskIdUniqueValidator = inBoeTaskIDUniqueValidator;
            this._BOEDateRangeValidator = inBOEDateRangeValidator;
            this._wbsUniqueNumberValidator = inWbsUniqueNumberValidator;
            this._resourceUniqueIDValidator = inResourceUniqueIDValidator;
            this._performingOrgUniqueIDValidator = inPerformingOrgUniqueIDValidator;
            this._wbsRenumberValidator = inWBSRenumberValidator;
            this._boeWBSMoveValidator = inBOEWBSMoveValidator;
            this._boeCLINMoveValidator = inBOECLINMoveValidator;
            this._boeLaborCostElementExistsValidator = inBoeLaborCostElementExistsValidator;
            this._boeMaterialElementExistsValidator = inBoeMaterialElementExistsValidator;
            this._resourceUniqueDescValidator = inResourceUniqueDescValidator;
            this._workspaceCostVolumeLeadNotSubcontractorValidator = inWorkspaceCostVolumeLeadNotSubcontractorValidator;

            instance = this;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        public static ValidationFactory Instance
        {
            get
            {
                if (instance == null)
                {
                    throw new ValidationException("The Validation Factory was not successfully created.");
                }
                return instance;
            }
        }

        public virtual Validator getValidator(ValidationType type)
        {
            Validator toReturn = null;

            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                switch (type)
                {
                    case ValidationType.WorkspaceUniqueName:
                        toReturn = this._workspaceUniqueNameValidator;
                        break;
                    case ValidationType.WorkspaceUniqueShortname:
                        toReturn = this._workspaceUniqueShortnameValidator;
                        break;
                    case ValidationType.IsUserNotGroup:
                        toReturn = this._workspaceCostVolumeLeadNotGroupValidator;
                        break;
                    case ValidationType.BoeTaskIDUnique:
                        toReturn = this._boeTaskIdUniqueValidator;
                        break;
                    case ValidationType.DateRangeValidator:
                        toReturn = this._BOEDateRangeValidator;
                        break;
                    case ValidationType.WBSUniqueNumber:
                        toReturn = this._wbsUniqueNumberValidator;
                        break;
                    case ValidationType.ResourceUniqueID:
                        toReturn = this._resourceUniqueIDValidator;
                        break;
                    case ValidationType.PerformingOrgUniqueID:
                        toReturn = this._performingOrgUniqueIDValidator;
                        break;
                    case ValidationType.WBSRenumber:
                        toReturn = this._wbsRenumberValidator;
                        break;
                    case ValidationType.BOEWBSMove:
                        toReturn = this._boeWBSMoveValidator;
                        break;
                    case ValidationType.BOECLINMove:
                        toReturn = this._boeCLINMoveValidator;
                        break;
                    case ValidationType.BoeLaborCostElementExists:
                        toReturn = this._boeLaborCostElementExistsValidator;
                        break;
                    case ValidationType.BoeMaterialElementExists:
                        toReturn = this._boeMaterialElementExistsValidator;
                        break;
                    case ValidationType.ResourceUniqueDesc:
                        toReturn = this._resourceUniqueDescValidator;
                        break;
                    case ValidationType.IsUserNotSubcontractor:
                        toReturn = this._workspaceCostVolumeLeadNotSubcontractorValidator;
                        break;
                    default:
                        break;
                }
            }

            return toReturn;
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1027:MarkEnumsWithFlags", Justification = "Allows non-consecutive numbering of enum values")]
    public enum ValidationType
    {
        None = 0,
        WorkspaceUniqueName = 1,
        WorkspaceUniqueShortname = 2,
        IsUserNotGroup = 3,
        BoeTaskIDUnique = 4,
        WorkspaceVariableUniqueName = 5,
        DateRangeValidator = 6,
        WBSUniqueNumber = 7,
        ResourceUniqueID = 8,
        PerformingOrgUniqueID = 9,
        WBSRenumber = 10,
        BOEWBSMove = 11,
        BOECLINMove = 12,
        BoeLaborCostElementExists = 14,
        BoeMaterialElementExists = 15,
        ResourceUniqueDesc = 16,
        IsUserNotSubcontractor = 20
    }
}
