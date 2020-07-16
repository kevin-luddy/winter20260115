// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ControllerLogic
{
    using GenBOE.ActionLogic.ModelView.BOE;
    using IES.Common.classes;

    public class BOEMaterialControllerLogicMST : BOEMaterialControllerLogic
    {
        /// <summary>
        /// Populates properties with company specific data
        /// </summary>
        /// <param name="theModel">the <see cref="MaterialDetailsModelView"/> to populate</param>
        public override void PopulateCompanySpecificProperties(MaterialDetailsModelView theModel)
        {
            if (theModel != null)
            {
                // The requirements are identical to Space Systems so we can re-use the text here
                theModel.MOQTextLabel = CommonConstants.BOE_MOQ_TEXT_LABEL_SPACE_SYSTEMS;
            }
        }
    }
}
