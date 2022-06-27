// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ControllerLogic
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using GenBOE.ActionLogic;
    using GenBOE.ActionLogic.BLL;
    using GenBOE.ActionLogic.BOETransitions;
    using GenBOE.ActionLogic.Common.Calculations;
    using GenBOE.ActionLogic.Common.Email;
    using GenBOE.ActionLogic.CopyBOE;
    using GenBOE.ActionLogic.IO.Export;
    using GenBOE.ActionLogic.IO.Import;
    using GenBOE.ActionLogic.IESSAPClient;
    using GenBOE.ActionLogic.ModelView.BOE;
    using GenBOE.ActionLogic.Validation;
    using GenBOE.ActionLogic.WBS;
    using GenBOE.ActionLogic.WBS.BOE;
    using GenBOE.DataBridge.Common.Interfaces;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common;
    using IES.Common.classes;

    public class BOEControllerLogicSpaceSystems : BOEControllerLogic
    {
        #region Protected Properties and Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        public BOEControllerLogicSpaceSystems(
            IBOESummary inBOESummary,
            IUserDTODataLoader inUserLoader,
            IActiveDirectoryUtilities inActiveDirectoryUtil,
            IPermissionsDTODataLoader inPermissionsLoader,
            IFullObjectFactory inFactory,
            IBOEExporter inBOEExporter,
            IBOECustomExporter inBoeCustomExporter,
            IGenBOEControllerLogic inGenBOEControllerLogic,
            IBoeMediator inBoeMediator,
            IValidationHelper inValidationHelper,
            IBOECommentDTODataLoader inBoeCommentLoader,
            IBoeEmailer inEmailer,
            IBoeTaskElementMediator inBoeTaskElementMediator,
            IWorkspaceVariableDTODataLoader inWorkspaceVariableLoader,
            IBOEStateMachine inBOEStateMachine,
            IVariableSelectBOEtoSumCalculation inVariableSelectBOEtoSumCalculation,
            IBOELaborControllerLogic inBOELaborControllerLogic,
            IValidateBOE inValidateBOE,
            ISecurityInformation inSecurityInformation,
            IBOESearchDTODataLoader inBoeSearchLoader,
            ISecurityAccess inSecurityAccess,
            IBoeTaskElementRecalculation inBoeTaskElementRecalculation,
            IBOEImporter inBOEImporter,
            IVariableCircularReferenceChecker inVariableCircularReferenceChecker,
            IConflictBOE inConflictBOE,
            INestedWBSUtilities inNestedWBSUtilities,
            IProjectMapDataLoader projectMapLoader,
            RMSZoneTravelRatesFeesDataLoader zoneTravelRatesFeesDataLoader,
            IRteTemplateDataLoader rteTemplateDataLoader,
            IMoqTypeDataLoader moqTypeDataLoader,
            IBoeApproverResponseDTODataLoader boeApproverResponseLoader,
            IESSAPClient iesSapClient,
            ITokenService tokenService
            )
            : base(inBOESummary, inUserLoader, inActiveDirectoryUtil,
            inPermissionsLoader, inFactory, inBOEExporter, inBoeCustomExporter, inGenBOEControllerLogic,
            inBoeMediator, inValidationHelper, inBoeCommentLoader, inEmailer, inBoeTaskElementMediator, inWorkspaceVariableLoader, inBOEStateMachine,
            inVariableSelectBOEtoSumCalculation, inBOELaborControllerLogic, inValidateBOE, inSecurityInformation, inBoeSearchLoader, inSecurityAccess,
            inBoeTaskElementRecalculation, inBOEImporter, inVariableCircularReferenceChecker, inConflictBOE, inNestedWBSUtilities, projectMapLoader, zoneTravelRatesFeesDataLoader,
            rteTemplateDataLoader, moqTypeDataLoader, boeApproverResponseLoader, iesSapClient, tokenService)
        {
        }

        #endregion

        #region Get Actions

        /// <summary>
        /// Get BOE Header Model View
        /// </summary>
        /// <param name="boe">The boe.</param>
        /// <returns></returns>
        public override IBOEHeaderModelView GetCreateBOEHeaderMV(BoeDTO boe, ICollection<RTECustomTemplateQuestionAnswerModelView> answers)
        {
            return new BOEHeaderSpaceModelView(boe, answers);
        }

        #endregion Get Actions

        /// <summary>
        /// Populates properties with company specific data
        /// </summary>
        /// <param name="theModel">the <see cref="BOEAdvancedSearchModelView"/> to populate</param>
        public override void PopulateCompanySpecificProperties(BOEAdvancedSearchModelView theModel)
        {
            if (theModel != null)
            {
                // requirements are identical to Space Systems so we can reuse the text here
                theModel.LabelLeadPricer = CommonConstants.LABEL_TEXT_LEAD_PRICER_SSC;
                theModel.ShowRFP = false;
            }
        }

        /// <summary>
        /// Gets company specific header information for the manage Boe page.
        /// </summary>
        /// <returns>Header Info for Manage Boe</returns>
        public override string GetCompanySpecificManageBoeHeaderInfo
        {
            get
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Validates the BOE Level Custom Fields being saved for a BOE.
        /// </summary>
        /// <param name="ws">Workspace containing the BOE</param>
        /// <param name="boe">BOE containing the header</param>
        /// <param name="inBOEHeader">Modelview for the BOE Header</param>
        public override void ValidateBOEHeaderCustomFields(FullWorkspace ws, FullBoe boe, IBOEHeaderModelView inBOEHeader)
        {
            if (boe == null)
            {
                throw new ArgumentNullException(nameof(boe));
            }

            // boe.ShowDescriptionAndSources is set correctly in the above method (SaveEditBoeHeader) before the base of that method calls this one.
            // only need to validate custom fields if they are shown to the user
            base.ValidateBOEHeaderCustomFields(ws, boe, inBOEHeader);
        }
    }
}
