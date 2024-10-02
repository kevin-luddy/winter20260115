// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic
{
    using System.Collections.Generic;
    using System.IO;
    using GenBOE.ActionLogic.ModelView.BOE;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common;
    using IES.Common.Exceptions;
    using IES.Common.PickList;

    public interface IBOEFormControllerLogic
    {
        /// <summary>
        /// Exports a BOE Form based on its Id
        /// </summary>
        /// <param name="workspace">The workspace for the BOE Form.</param>
        /// <param name="boeFormId">BOE Form Id</param>
        /// <param name="boeFormType">BOE Form Type that you wish to export</param>
        /// <param name="isPortionMarkingEnabled">True if portion marking is enabled; False otherwise.</param>
        /// <param name="contractTypes">The contract types.</param>
        /// <returns>Returns the path of the actual file, and the suggested filename.</returns>
        string[] ExportBOEFormReport(FullWorkspace workspace, int boeFormId, BOEFormType boeFormType, bool isPortionMarkingEnabled, ICollection<PickListDto> contractTypes);

		/// <summary>
		/// Exports a BOE Form based on its Id to a Stream
		/// </summary>
		/// <param name="workspace">The workspace for the BOE Form.</param>
		/// <param name="boeFormId">BOE Form Id</param>
		/// <param name="boeFormType">BOE Form Type that you wish to export</param>
		/// <param name="isPortionMarkingEnabled">True if portion marking is enabled; False otherwise.</param>
		/// <param name="contractTypes">The contract types.</param>
		/// <returns>Returns the Stream containing the export.</returns>
		Stream ExportBOEFormReportAsStream(FullWorkspace workspace, int boeFormId, BOEFormType boeFormType, bool isPortionMarkingEnabled, ICollection<PickListDto> contractTypes);

		/// <summary>
		/// Retrieves a boe form.
		/// </summary>
		/// <param name="workspaceId">The id of the workspace</param>
		/// <param name="boeFormId">The id of the boe form.</param>
		/// <returns>IBOE Model View for the BOE Form.</returns>
		/// <param name="proposalTitleAndRfpNumber">Proposal Title And RFP Number from the workspace.</param>
		BOEFormIBOEModelView GetIBOEForm(int workspaceId, int boeFormId, string proposalTitleAndRfpNumber);

        /// <summary>
        /// Retrieves a boe form.
        /// </summary>
        /// <param name="workspaceId">The id of the workspace</param>
        /// <param name="boeFormId">The id of the boe form.</param>
        /// <returns>PBOE Model View for the BOE Form.</returns>
        /// <param name="proposalTitleAndRfpNumber">Proposal Title And RFP Number from the workspace.</param>
        BOEFormPBOEModelView GetPBOEForm(int workspaceId, int boeFormId, string proposalTitleAndRfpNumber);

        /// <summary>
        /// Retrieves summaries of forms by workspace
        /// </summary>
        /// <param name="workspace">A workspace to retrieve boe forms against.</param>
        /// <returns>A collection of boeforms for the workspace.</returns>
        ICollection<BOEFormModelView> GetSummaryForms(FullWorkspace workspace);

        /// <summary>
        /// Validates the specified IBOE and PBOE Form T&amp;M resources.
        /// </summary>
        /// <param name="validationErrors">output parameter - collection of error messages</param>
        /// <param name="resourceIdsWithValidTMRates">output parameter - list of Resource IDs that have valid T&amp;M rates.</param>
        /// <param name="workspace">A workspace to validate boe forms against.</param>
        /// <param name="iboeFormIds">IBOE form IDs to validate</param>
        /// <param name="pboeFormIds">PBOE form IDs to validate</param>
        void ValidateBOEFormsTMResources(ICollection<ValidationMessage> validationErrors,
            ICollection<int> resourceIdsWithValidTMRates, FullWorkspace workspace, ICollection<int> iboeFormIds,
            ICollection<int> pboeFormIds);

        /// <summary>
        /// Validates the pboe.
        /// </summary>
        /// <param name="boeForm">The BOE form.</param>
        /// <param name="validationMessages">The validation messages.</param>
        /// <param name="proposalTitleAndRfpNumber">Proposal Title And RFP Number from the workspace. When present, no need to validate Proposal Title.</param>
        void ValidatePBOE(BOEFormPBOEDTO boeForm, ICollection<ValidationMessage> validationMessages, string proposalTitleAndRfpNumber = null);

        /// <summary>
        /// Validates the iboe.
        /// </summary>
        /// <param name="boeForm">The BOE form.</param>
        /// <param name="validationMessages">The validation messages.</param>
        /// <param name="proposalTitleAndRfpNumber">Proposal Title And RFP Number from the workspace. When present, no need to validate Proposal Title.</param>
        void ValidateIBOE(BOEFormIBOEDTO boeForm, ICollection<ValidationMessage> validationMessages, string proposalTitleAndRfpNumber = null);

        /// <summary>
        /// Saves the IBOE Form.
        /// </summary>
        /// <param name="modelview">ModelView of the form.</param>
        /// <param name="workspaceId">The workspace Id.</param>
        void SaveBOEFormIBOE(BOEFormIBOEModelView modelview, int workspaceId);

        /// <summary>
        /// Saves the PBOE Form.
        /// </summary>
        /// <param name="modelview">ModelView of the form.</param>
        /// <param name="workspaceId">The workspace Id.</param>
        void SaveBOEFormPBOE(BOEFormPBOEModelView modelview, int workspaceId);

        /// <summary>
        /// Deletes the IBOE form.
        /// </summary>
        /// <param name="boeFormId">The id of the form to delete.</param>
        void DeleteBOEFormIBOE(int boeFormId);

        /// <summary>
        /// Deletes the PBOE form.
        /// </summary>
        /// <param name="boeFormId">The id of the form to delete.</param>
        void DeleteBOEFormPBOE(int boeFormId);

        /// <summary>
        /// Gets current/latest version of form
        /// </summary>
        /// <param name="boeFormType">The type of the boe form.</param>
        int GetCurrentFormVersion(BOEFormType boeFormType);

        /// <summary>
        /// Gets a list of in-use resources for a BOEFormType.
        /// </summary>
        /// <param name="boeFormType">The type of the boe form.</param>
        /// <param name="wsId">Workspace Id.</param>
        /// <param name="boeFormId">The boe Form Id to not include when finding the in-use resources.</param>
        /// <returns>A list of ids that are currently in-use.</returns>
        ICollection<int> GetInUseResources(BOEFormType boeFormType, int wsId, int boeFormId);

        /// <summary>
        /// Converts ModelView to DTO
        /// </summary>
        /// <param name="modelview">The modelview to convert.</param>
        /// <param name="workspaceId">The workspace Id.</param>
        BOEFormIBOEDTO ConvertModelViewToDto(BOEFormIBOEModelView modelview, int workspaceId);

        /// <summary>
        /// Converts ModelView to DTO
        /// </summary>
        /// <param name="modelview">The modelview to convert.</param>
        /// <param name="workspaceId">The workspace Id.</param>
        BOEFormPBOEDTO ConvertModelViewToDto(BOEFormPBOEModelView modelview, int workspaceId);
    }
}
