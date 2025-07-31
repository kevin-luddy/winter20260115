// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.WBS.BOE
{
    using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Linq;
	using GenBOE.ActionLogic.Common;
	using GenBOE.ActionLogic.Common.Calculations;
	using GenBOE.ActionLogic.ModelView;
	using GenBOE.ActionLogic.Validation;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
	using GenBOE.Objects;
	using IES.Common;
	using IES.Common.classes;
	using IES.Common.Exceptions;

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

			// UCOT validation (Space only) - if there are multiple MOQ types assigned to a task and one of those MOQ Types is Historical/Comparative/Analogous, add a warning
			MultiMOQTypeResult multiMoqResult = MultiMOQTypeUtility.DoTasksHaveMultipleMOQTypes(inBOE, ws.CreationDate, ws.Shortname);
			if (multiMoqResult.DoMultiMOQTypesExist)
			{
				foreach (string task in multiMoqResult.Tasks)
				{
					toReturn.BOEHeaderMsgs.Add(string.Format(ValidationConstants.TASK_WITH_MULTI_MOQ_TYPES_UCOT, task));
				}
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
