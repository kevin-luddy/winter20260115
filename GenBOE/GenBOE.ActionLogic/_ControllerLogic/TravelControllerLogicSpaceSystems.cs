// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ControllerLogic
{
    using GenBOE.DataBridge.DTO;
    using System;

    public class TravelControllerLogicSpaceSystems : TravelControllerLogic
    {
        public TravelControllerLogicSpaceSystems(
             ITravelDTODataLoader inTravelDataLoader,
             ICustomFieldValueDTODataLoader customFieldValueLoader
            )
            : base(inTravelDataLoader, customFieldValueLoader)
        {
            // nothing to do here
        }

        /// <summary>
        /// Gets a <see cref="Boolean"/> indicating if the segment help should be shown for SSC
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
