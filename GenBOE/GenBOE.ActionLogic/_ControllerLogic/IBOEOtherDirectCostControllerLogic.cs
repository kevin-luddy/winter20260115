// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic
{
    using GenBOE.ActionLogic.ModelView;

    public interface IBOEOtherDirectCostControllerLogic
    {
        /// <summary>
        /// Populates properties with company specific data
        /// </summary>
        /// <param name="theModel">the <see cref="ODCElementDetailsModelView"/> to populate</param>
        void PopulateCompanySpecificProperties(ODCElementDetailsModelView theModel);
    }
}
