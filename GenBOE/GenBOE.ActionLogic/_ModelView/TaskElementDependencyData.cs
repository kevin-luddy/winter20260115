// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView
{
    using GenBOE.DataBridge.DTO;
    using GenBOE.Objects;

    public class TaskElementDependencyData
    {
        public int TaskElementID { get; set; }
        public BoeTaskElementDTO TaskElement { get; set; }
        public int BoeID { get; set; }
        public FullBoe BoeFull { get; set; }
        public FullWorkspace WorkspaceFull { get; set; }
    }
}
