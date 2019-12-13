// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ControllerLogic
{
    using GenBOE.ActionLogic.ModelView;
    using GenBOE.Objects;
    using IES.Common.classes;

    /// <summary>
    /// Class to encapsulate ODC logic
    /// </summary>
    public class BOEOtherDirectCostControllerLogic : IBOEOtherDirectCostControllerLogic
    {
        #region Protected Properties and Constructor

      /// <summary>
      /// Constructor for the BOEOtherDirectCostControllerLogic
      /// </summary>
        public BOEOtherDirectCostControllerLogic()
        {
        }

        #endregion

        #region Get Actions

        /// <summary>
        /// Populates properties with company specific data
        /// </summary>
        /// <param name="theModel">the <see cref="ODCElementDetailsModelView"/> to populate</param>
        public virtual void PopulateCompanySpecificProperties(ODCElementDetailsModelView theModel)
        {
            if (theModel != null)
            {
                theModel.MOQTextLabel = CommonConstants.BOE_MOQ_TEXT_LABEL;
            }
        }

        #endregion Get Actions
    }
}
