// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ControllerLogic
{
    using GenBOE.ActionLogic.ModelView.BOE;

    public interface IBOEMaterialControllerLogic
    {
        /// <summary>
        /// Populates properties with company specific data
        /// </summary>
        /// <param name="theModel">the <see cref="MaterialDetailsModelView"/> to populate</param>
        void PopulateCompanySpecificProperties(MaterialDetailsModelView theModel);
    }
}
