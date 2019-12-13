// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2016 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using GenBOE.Dtos;
using GenBOE.Objects;

namespace GenBOE.ActionLogic.ModelView.BOE
{
    public class BOEMobileModelView
    {
        public BOEMobileModelView()
        {
            BOEComments = new Collection<BOEComment>();
        }

        public BOEMobileModelView(FullBoe inFullBOE, IDictionary<int,BOEStateModelView> boeStateDictionary)
        {
            if (inFullBOE == null)
            {
                throw new ArgumentNullException("inFullBOE");
            }
            if (boeStateDictionary == null)
            {
                throw new ArgumentNullException("boeStateDictionary");
            }

            BOEId = inFullBOE.Id;
            Title = inFullBOE.Title;

            BOEStateModelView boeStateModelview = null;
            if (boeStateDictionary.TryGetValue((int)inFullBOE.State, out boeStateModelview))
            {
                State = boeStateModelview.BOEState;
            }
            else
            {
                throw new ArgumentException(
                    "Dictionary does not contain a key for the boe state value of " + (int)inFullBOE.State, 
                    "boeStateDictionary");
            }

            Description = inFullBOE.Description;

            if (inFullBOE.Clin != null)
            {
                CLINID = inFullBOE.Clin.Id;
                CLINText = inFullBOE.Clin.ClinString;
            }

            if (inFullBOE.Wbs != null)
            {
                WBSID = inFullBOE.Wbs.Id;
                WBSText = inFullBOE.Wbs.WbsString;
            }
            
            WorkspaceID = inFullBOE.WorkspaceID;           
            BOELastUpdate = inFullBOE.UpdateDate;
            BOEComments = new Collection<BOEComment>();

            if (inFullBOE.Workspace != null)
            {
                WorkspaceName = inFullBOE.Workspace.WorkspaceName;
                WorkspaceShortName = inFullBOE.Workspace.Shortname;
            }
        }

        // the BOE Id
        public int BOEId { get; set; }

        // the BOE Title
        public string Title { get; set; }

        // the BOE state
        public string State { get; set; }

        // the BOE description
        public string Description { get; set; }


        // WBS ID associated with the BOE
        public int? WBSID { get; set; }

        public string WBSText { get; set; }


        // CLIN ID associated with the BOE
        public int? CLINID { get; set; }

        public string CLINText { get; set; }

        // Workspace ID
        public int WorkspaceID { get; set; }

        public string WorkspaceShortName { get; set; }

        public string WorkspaceName { get; set; }

        public DateTime BOELastUpdate { get; set; }

        public DateTime BOEApprovalLastUpdate { get; set; }

        public long BOEApprovalLastUpdateInt { get; set; }

        public Collection<BOEComment> BOEComments { get; set; }
        
    }
}