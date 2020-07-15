// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ControllerLogic
{
    using GenBOE.ActionLogic.ModelView;
    using IES.Common.classes;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Objects;

    /// <summary>
    /// Class to encapsulate ODC logic
    /// </summary>
    public class BOEOtherDirectCostControllerLogicSpaceSystems : BOEOtherDirectCostControllerLogic
    {
        #region Protected Properties and Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        public BOEOtherDirectCostControllerLogicSpaceSystems()
            : base()
        {

        }

        #endregion

        /// <summary>
        /// Populates properties with company specific data
        /// </summary>
        /// <param name="theModel">the <see cref="ODCElementDetailsModelView"/> to populate</param>
        public override void PopulateCompanySpecificProperties(ODCElementDetailsModelView theModel)
        {
            if (theModel != null)
            {
                theModel.MOQTextLabel = CommonConstants.BOE_MOQ_TEXT_LABEL_SPACE_SYSTEMS;
            }
        }
    }
}
