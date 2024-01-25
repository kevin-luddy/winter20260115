// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView
{
    using System.Collections.ObjectModel;
    using System.Linq;
    using GenBOE.ActionLogic.Common;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using IES.Common;
    using IES.Common.classes;

    [WBSOrCLINValidation(CLINId = "ClinID", WBSId = "WbsID", ErrorMessage = "Please select either a CLIN or WBS (or both)")]
    public class ManageBOEModelView : PersistedDataModelView
    {
        public ManageBOEModelView()
        {
            this.BoeID = -1;
            this.Deleted = false;
            this.State = BOEState.None;
            this.Authors = new Collection<int>();
            this.SubcontractorAuthors = new Collection<int>();
            this.Approvers = new Collection<int>();
            this.MaterialString = "No";
            this.AuthorsDisplayNames = new Collection<string>();
            this.ApproversDisplayNames = new Collection<string>();
            this.TotalHours = 0;
            this.TotalCost = 0;
        }

        public ManageBOEModelView(BoeDTO inBoeDTO, WbsDTO inWbsDTO, ClinDTO inClinDTO, Collection<UserDTO> inUserDTOs,
                                  Collection<PermissionsDTO> boeAuthors, Collection<PermissionsDTO> boeSubcontractorAuthors,Collection<PermissionsDTO> boeApprovers, Collection<BoeTaskElementDTO> taskElements, decimal totalCostTravel)
            : this()
        {
            Collection<int> ApproverIDs = new Collection<int>();
            Collection<int> AuthorIDs = new Collection<int>();
            Collection<int> SubcontractorAuthorIDs = new Collection<int>();

            if (inBoeDTO != null)
            {
                this.BoeID = inBoeDTO.Id;
                this.BOETitle = inBoeDTO.Title;
                this.WbsID = inBoeDTO.WBSID;
                this.WbsDisplayName = (inWbsDTO != null) ? inWbsDTO.WbsString : string.Empty;
                this.PaddedWbsName = (inWbsDTO != null) ? inWbsDTO.WbsPaddedNumber : string.Empty;
                this.PaddedClinName = (inClinDTO != null) ? inClinDTO.ClinPaddedNumber : string.Empty;
                this.State = inBoeDTO.State;
                this.ClinID = inBoeDTO.CLINID;
                this.ClinDisplayName = (inClinDTO != null) ? inClinDTO.ClinString: string.Empty;
                this.WorkspaceID = inBoeDTO.WorkspaceID;
                this.UpdateDate = inBoeDTO.UpdateDate;

                this.BoeXrefID = inBoeDTO.WCBID.Value;

                this.MaterialString = inBoeDTO.isMaterial ? "Yes" : "No";
                this.isMaterial = inBoeDTO.isMaterial;
                this.IsMultiClinWbs = inBoeDTO.IsMultiClinWbs;

                this.TotalCost = totalCostTravel;

                // Calculate totals for the rest of the task elements.
                if (taskElements != null && taskElements.Any())
                {
                    foreach (BoeTaskElementDTO taskElement in taskElements)
                    {
                        this.TotalCost += taskElement.TotalCost ?? 0;
                        this.TotalHours += taskElement.TotalHours ?? 0;
                    }
                }

                // get the Author IDs
                if (boeAuthors != null && boeAuthors.Any())
                {
                    foreach (PermissionsDTO boeAuthor in boeAuthors)
                    {
                        AuthorIDs.Add(boeAuthor.ETIUserId);
                    }
                    this.Authors = AuthorIDs;

                }

                if (boeSubcontractorAuthors != null && boeSubcontractorAuthors.Any())
                {
                    foreach (PermissionsDTO boeSubcontractorAuthor in boeSubcontractorAuthors)
                    {
                        SubcontractorAuthorIDs.Add(boeSubcontractorAuthor.ETIUserId);
                    }
                    this.SubcontractorAuthors = SubcontractorAuthorIDs;
                }

				// Create a combined list of Author and SubcontractorAuthor display names.
				// Append "(Sub)" to each subcontractor author in the collection.
				System.Collections.Generic.IEnumerable<string> authorDisplayNames = (from x in inUserDTOs
                                          where this.Authors.Contains(x.UserID)
                                          select x.DisplayName);
				System.Collections.Generic.IEnumerable<string> subcontractorAuthorDisplayNames = (from x in inUserDTOs
                                                       where this.SubcontractorAuthors.Contains(x.UserID)
                                                       select x.DisplayName + CommonConstants.SUBCONTRACTOR_AUTHOR_SUFFIX);
                this.AuthorsDisplayNames = new Collection<string>((authorDisplayNames.Union(subcontractorAuthorDisplayNames)
                    .OrderBy(x => x).Distinct().ToList()));

                // get the Approver IDs
                if (boeApprovers != null && boeApprovers.Any())
                {
                    foreach (PermissionsDTO boeApprover in boeApprovers)
                    {
                        ApproverIDs.Add(boeApprover.ETIUserId);
                    }
                    this.Approvers = ApproverIDs;

                    this.ApproversDisplayNames = new Collection<string>((from x in inUserDTOs
                                                                    where this.Approvers.Contains(x.UserID)
                                                                    orderby x.DisplayName
                                                                    select x.DisplayName).Distinct().ToList());
                }
            }
        }

        public int BoeID { get; set; }
        public int BoeXrefID { get; set; }
        public string BOETitle { get; set; }

        public int? WbsID { get; set; }
        public string WbsDisplayName{ get; set; }
        public string PaddedWbsName { get; set; }

        public int? ClinID { get; set; }
        public string ClinDisplayName { get; set; }
        public string PaddedClinName { get; set; }

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

        public Collection<int> Authors { get; set; }
        public Collection<int> SubcontractorAuthors { get; set; }
        public Collection<string> AuthorsDisplayNames { get; set; }

        public Collection<int> Approvers { get; set; }
        public Collection<string> ApproversDisplayNames { get; set; }
               
        public int WorkspaceID { get; set; }

        public bool Deleted { get; set; }

        public string MaterialString { get; set; }

        public bool isMaterial { get; set; }

        public bool IsMultiClinWbs { get; set; }

        /// <summary>
        /// The total number of hours (or equivalent persons) for all Task Elements in the BOE.
        /// </summary>
        public decimal TotalHours { get; set; }

        /// <summary>
        /// The total cost (in dollars) for all Task Elements in the BOE.
        /// </summary>
        public decimal TotalCost { get; set; }
    }
}
