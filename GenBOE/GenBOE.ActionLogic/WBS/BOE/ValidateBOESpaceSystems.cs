// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
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
            IOffloadRatesDTOLoader offloadRatesDTOLoader
            )
            : base(
            inVariableSelectBOEtoSumCalculation,
            inBOECommentsResponsesValidator, 
            inTripDTODataLoader, 
            inMiscTravelRateDTOLoader, 
            inLocationDTODataLoader,  
            offloadRatesDTOLoader)
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
        /// <param name="sourcesOfData">The string to test</param>
        /// <returns>Always returns True</returns>
        protected override bool IsSourcesOfDataValid(string sourcesOfData)
        {
            // always return true since the field isn't required
            return true;
        }

        /// <summary>
        /// Determines whether Description is valid.
        /// </summary>
        /// <param name="inBOE">The boe.</param>
        /// <returns>
        ///   <c>true</c> if description is valid; otherwise, <c>false</c>.
        /// </returns>
        protected override bool IsDescriptionValid(BoeDTO inBOE)
        {
            if (inBOE == null)
            {
                throw new ArgumentNullException(nameof(inBOE));
            }
            // validate description
            return !string.IsNullOrEmpty(inBOE.Description);
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
