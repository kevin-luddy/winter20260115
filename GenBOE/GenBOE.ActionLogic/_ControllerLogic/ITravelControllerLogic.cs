// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic
{
    using GenBOE.ActionLogic.ModelView;
    using GenBOE.Objects;
    using System;
    using System.Collections.Generic;
    public interface ITravelControllerLogic
    {
        /// <summary>
        /// Gets a <see cref="Boolean"/> indicating if the segment help should be shown
        /// </summary>
        /// <value>a <see cref="Boolean"/> indicating if the segment help should be shown</value>
        Boolean ShowSegmentHelpLink { get; }


        /// <summary>
        /// Saves the travel elements with a new order.
        /// </summary>
        /// <param name="boeObject">full boe object.</param>
        /// <param name="theModelView">Collection of user changes to the order.</param>
        void ReOrderTaskElementOrder(FullBoe boeObject, TaskElementOrderCollection theModelView);

        /// <summary>
        /// Creates duplicates of a collection of Travel Task elements.
        /// </summary>
        /// <param name="duplicateRequest">Collection of travel task IDs (key) and the number of times (value) they should be duplicated.</param>
        /// <param name="inSourceBOE">Full BOE containing the Travel Tasks.</param>
        void DuplicateTravelTaskElements(Dictionary<int, int> duplicateRequest, FullBoe inSourceBOE);
    }
}
