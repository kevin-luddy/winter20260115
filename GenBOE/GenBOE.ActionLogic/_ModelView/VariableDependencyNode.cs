// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView
{
    public enum VertexType { BOE, TaskElement, Variable }

    /// <summary>
    /// For the dependency resolution algorithm
    /// </summary>
    public class VariableDependencyNode
    {
        public VertexType Vertex { get; set; }
        public int ID { get; set; }
    }
}
