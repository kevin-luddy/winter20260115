// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------
using GenBOE.Dtos;
using GenBOE.Objects;

namespace GenBOE.ActionLogic.ModelView
{
    /// <summary>
    /// Dependency data
    /// </summary>
    public class VariableDependencyData
    {
        public IVariableDTO Variable { get; set; }
        public FullWorkspace WorkspaceFull { get; set; }
    }
}
