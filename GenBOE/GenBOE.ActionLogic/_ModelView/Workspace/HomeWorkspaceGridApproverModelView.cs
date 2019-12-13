// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView.Workspace
{
    using System;
    using IES.Common;

    public class HomeWorkspaceGridApproverModelView
    {
        public HomeWorkspaceGridApproverModelView(ApproverReponseType repsonse, bool? responded, string name)
        {
            ApproverResponse = repsonse;
            ApproverResponded = responded;
            DisplayName = name;
        }  

        public ApproverReponseType ApproverResponse { get; set; }
        public bool? ApproverResponded { get; set; }
        public String DisplayName { get; set; }
    }
}

