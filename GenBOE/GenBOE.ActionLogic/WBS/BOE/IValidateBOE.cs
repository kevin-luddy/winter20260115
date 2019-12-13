// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.WBS.BOE
{
    using GenBOE.Dtos;
    using GenBOE.Objects;

    /// <summary>
    /// IValidate Interface
    /// </summary>
    public interface IValidateBOE
    {
        /// <summary>
        /// Validate all BOE data (BOE Header, task element details, labor type, and labor spreads)
        /// when the user selects the Validate button
        /// </summary>
        /// <param name="inBOE">the BOE DTO to validate</param>
        /// <param name="ws">Full WS</param>
        /// <returns>all possible validation messages</returns>
        ValidationBOEModelView ValidateBOE_OnValidateBtnClick(FullBoe inBOE, FullWorkspace ws);

        /// <summary>
        /// Validate all BOE data (BOE Header, task element details, labor type, and labor spreads) in the workspace
        /// </summary>
        /// <param name="ws">Full Ws</param>
        /// <returns>Validation data</returns>
        ValidationAllBOEModelView ValidateAllBOEs(FullWorkspace ws);
    }
}