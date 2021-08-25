// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView.Workspace
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using IES.Common;
    using IES.Common.classes;

    [ExcludeFromCodeCoverage]
    public class HomeWorkspaceModelView
    {
        public HomeWorkspaceModelView()
        {
            BOEID = 0;
            BOETitle = string.Empty;
            WBSText = String.Empty;
            CLINText = string.Empty;
            AuthorsName = new Collection<string>();
            AuthorOrderName = string.Empty;
            Approvers = new Collection<HomeWorkspaceGridApproverModelView>();
            ApproverOrderName = string.Empty;
            State = BOEState.None;
            isMaterial = "No";
            StartDate = DateTime.MinValue.ToString("MM/yyyy");
            EndDate = DateTime.MinValue.ToString("MM/yyyy");
            UpdateDate = DateTime.MinValue.Ticks.ToString();
        }

        public HomeWorkspaceModelView(BoeDTO inBoeDTO, WbsDTO inWbsDTO, ClinDTO inClinDTO, ICollection<string> inAuthors, ICollection<HomeWorkspaceGridApproverModelView> inApprovers)
            : this()
        {
            if (inBoeDTO == null)
            {
                throw new ArgumentNullException(nameof(inBoeDTO));
            }

            BOEID = inBoeDTO.Id;
            BOETitle = inBoeDTO.Title;
            WBSText = inWbsDTO != null ? inWbsDTO.WbsString : CommonConstants.Unassigned_WBS_Display_Text;
            CLINText = inClinDTO != null ? inClinDTO.ClinString : CommonConstants.Unassigned_CLIN_Display_Text;
            AuthorsName = inAuthors;
            AuthorOrderName = inAuthors.Any() ? inAuthors.First() : string.Empty;
            Approvers = inApprovers;
            ApproverOrderName = inApprovers.Any() ? inApprovers.First().DisplayName : string.Empty;
            State = inBoeDTO.State;
            isMaterial = inBoeDTO.isMaterial ? "Yes" : "No";
            StartDate = inBoeDTO.StartDate.ToString("MM/yyyy");
            EndDate = inBoeDTO.EndDate.ToString("MM/yyyy");
            UpdateDate = inBoeDTO.UpdateDate.Ticks.ToString();
        }        

        public int? BOEID { get; set; }
        public string BOETitle { get; set; }
        public string WBSText { get; set; }
        public string CLINText { get; set; }
        public ICollection<string> AuthorsName { get; set; }
        public string AuthorOrderName { get; set; }
        public ICollection<HomeWorkspaceGridApproverModelView> Approvers { get; set; }
        public string ApproverOrderName { get; set; }
        public BOEState State { get; set; }
        public string Status
        {
            get
            {
                if (this.State == BOEState.None)
                {
                    return string.Empty;
                }
                else if (this.State == BOEState.DraftLocked)
                {
                    return BOEState.Draft.GetDescription();
                }
                else
                {
                    return this.State.GetDescription();
                }
            }
        }
        public string StateLockedSortKey
        {
            get
            {
                string text;
                if (this.State == BOEState.Draft)
                {
                    text = "A";
                }
                else if (this.State == BOEState.DraftLocked)
                {
                    text = "B";
                }
                else
                {
                    text = string.Empty;
                }
                return text;
            }
        }
        public string isMaterial { get; set; }
        public String StartDate { get; set; }
        public String EndDate { get; set; }
        public string UpdateDate { get; set; }
    }
}
