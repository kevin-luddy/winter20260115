// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------
using System.Collections.Generic;

namespace GenBOE.ActionLogic.ModelView
{
    public class VariableDependencyResolutionData
    {
        public IList<VariableDependencyData> Variables { get; set; }
        public IList<TaskElementDependencyData> TaskElements { get; set; }
    }
}
