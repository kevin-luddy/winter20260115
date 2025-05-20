// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.WBS.BOE
{
    using System;
    using System.Collections.ObjectModel;
    using GenBOE.ActionLogic.Common.Calculations;
    using GenBOE.ActionLogic.Validation;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
	using GenBOE.Objects;
	using IES.Common;
	using IES.Common.classes;

    public class ValidateBOESpaceSystems : ValidateBOE
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public ValidateBOESpaceSystems(
            IVariableSelectBOEtoSumCalculation inVariableSelectBOEtoSumCalculation,
            BOECommentsResponsesValidator inBOECommentsResponsesValidator,
            ITripDTODataLoader inTripDTODataLoader,
            IMiscTravelRateDTOLoader inMiscTravelRateDTOLoader,
            ILocationDTODataLoader inLocationDTODataLoader,
            IOffloadRatesDTOLoader offloadRatesDTOLoader,
            IRteTemplateDataLoader rteTemplateDataLoader)
            : base(
            inVariableSelectBOEtoSumCalculation,
            inBOECommentsResponsesValidator, 
            inTripDTODataLoader, 
            inMiscTravelRateDTOLoader, 
            inLocationDTODataLoader,  
            offloadRatesDTOLoader, 
            rteTemplateDataLoader)
        {

        }

        public override ValidationBOEModelView ValidateBOE_OnValidateBtnClick(FullBoe inBOE, FullWorkspace ws)
        {
            ValidationBOEModelView toReturn = base.ValidateBOE_OnValidateBtnClick(inBOE, ws);

            // validate BOE title
            if (String.IsNullOrEmpty(inBOE.Title))
            {
                toReturn.BOEHeaderMsgs.Add(BoeDTO.BOE_TITLE_REQUIRED);
            }

			// UCOT validation (Space only) - if there are multiple MOQ types assigned to a task and one of those MOQ Types is one of the following, add a warning
			if (Utilities.IsUCOTEnabledForSystem && SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.SpaceSystems)
			{
				//IEnumerable<MOQType> selectedMOQTypes = inBOE.MoqTypeSelections.Select(x => x.SelectedMOQType);
				//if (inBOE.MoqTypeSelections.Count > 1 && (selectedMOQTypes.Any(x => x == MOQType.Historical) || selectedMOQTypes.Any(x => x == MOQType.Comparative)
				//	|| selectedMOQTypes.Any(x => x == MOQType.AnalogousRelationships)))
				//{
				//	toReturn.BOEHeaderMsgs.Add($"UCOT is not calculated for Task -test- because it has multiple MOQ Types.");
				//}
				foreach (BoeTaskElementDTO task in inBOE.TaskElements)
				{
					if (task != null)
					{
						toReturn.BOEHeaderMsgs.Add("");
					}
				}
				// Group tasks by IDs, especially if they have multiple MOQ Types
				//inBOE.MoqTypeSelections.GroupBy(x => x.SelectedMOQType).Select(x => new BoeTaskElementDTO()
				//{
				//	TaskTitle = 
				//});
			}

			return toReturn;
        }

        /// <summary>
        /// Adds the company specific MOQ Text label to the Error string
        /// </summary>
        /// <param name="MOQTextErrorMessage">The error text to add the MOQ label to</param>
        /// <returns>The formatted <see cref="String"/></returns>
        protected override string FormatMOQTextErrorMessage(string MOQTextErrorMessage)
        {
            return String.Format(MOQTextErrorMessage, CommonConstants.BOE_MOQ_TEXT_LABEL_SPACE_SYSTEMS);
        }

        /// <summary>
        /// Tests if the Source of Data field is valid for SSC
        /// </summary>
        /// <returns>Always returns True</returns>
        protected override bool IsSourcesOfDataValid(BoeDTO boe, int wsId)
        {
            // always return true since the field isn't required
            return true;
        }

        /// <summary>
        /// This function will validate the BOE Custom Fields.
        /// </summary>
        /// <param name="workspace">The Workspace used during validation.</param>
        /// <param name="boe">The BOE used to validate against.</param>
        /// <returns>A collection of validation messages.</returns>
        protected override Collection<string> ValidateBOECustomFields(FullWorkspace workspace, FullBoe boe)
        {
            if (boe == null)
            {
                throw new ArgumentNullException(nameof(boe));
            }

            Collection<string> toReturn  = base.ValidateBOECustomFields(workspace, boe);

            return toReturn;
        }
    }
}
