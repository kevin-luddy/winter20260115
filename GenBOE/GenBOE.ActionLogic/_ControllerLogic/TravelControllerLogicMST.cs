// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ControllerLogic
{
    using GenBOE.DataBridge.DTO;
    using System;

    public class TravelControllerLogicMST : TravelControllerLogic
    {
        public TravelControllerLogicMST(
             ITravelDTODataLoader inTravelDataLoader,
             ICustomFieldValueDTODataLoader customFieldValueLoader
            ) :base(inTravelDataLoader, customFieldValueLoader)
        {
            // nothing to do here
        }

        /// <summary>
        /// Gets a <see cref="Boolean"/> indicating if the segment help should be shown for MST
        /// </summary>
        /// <value>a <see cref="Boolean"/> indicating if the segment help should be shown</value>
        public override Boolean ShowSegmentHelpLink
        {
            get
            {
                return false;
            }
        }
    }
}
