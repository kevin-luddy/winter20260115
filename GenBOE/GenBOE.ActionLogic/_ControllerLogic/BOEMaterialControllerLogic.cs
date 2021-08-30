// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ControllerLogic
{
    using GenBOE.ActionLogic.ModelView.BOE;
    using IES.Common.classes;

    public class BOEMaterialControllerLogic : IBOEMaterialControllerLogic
    {
        /// <summary>
        /// Populates properties with company specific data
        /// </summary>
        /// <param name="theModel">the <see cref="MaterialDetailsModelView"/> to populate</param>
        public virtual void PopulateCompanySpecificProperties(MaterialDetailsModelView theModel)
        {
            if (theModel != null)
            {
                theModel.MOQTextLabel = CommonConstants.BOE_MOQ_TEXT_LABEL;
            }
        }
    }
}
