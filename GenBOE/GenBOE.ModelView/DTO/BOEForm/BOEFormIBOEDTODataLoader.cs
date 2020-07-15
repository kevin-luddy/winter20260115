// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
    using System.Collections.Generic;
    using System.Linq;
    using IES.Common;
    using GenBOE.Dtos;
    using GenBOE.Models;

    /// <summary>
    /// The IBOE INL Forms Data Loader.
    /// </summary>
    /// <seealso cref="GenBOE.DataBridge.DTO.IBOEFormIBOEDTODataLoader" />
    public class BOEFormIBOEDTODataLoader : DataLoader<BOEFormIBOEDTO>, IBOEFormIBOEDTODataLoader
    {
        private Logger _log = new Logger(typeof(BOEFormIBOEDTODataLoader));

        /// <summary>
        /// Gets BOE Forms for a workspace.
        /// </summary>
        /// <param name="wsId">Workspace Id</param>
        /// <returns>BOE Forms used by Workspace.</returns>
        [DbQuery]
        public ICollection<BOEFormIBOEDTO> GetByWorkspaceId(int wsId)
        {
            List<BOEFormIBOEDTO> toReturn = new List<BOEFormIBOEDTO>();
            List<BOEFormIBOEDTO> iboes;
            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    // get the full BOE form data so we can validate
                    iboes = (from b in gbe.BOEFormIBOEs
                                where b.WorkspaceID == wsId
                                orderby b.IBOEFormID
                                select new BOEFormIBOEDTO
                                {
                                    Approver = b.Approver,
                                    ApproverPhone = b.ApproverPhone,
                                    BasisAndRationale = b.BasisAndRationale,
                                    BusinessArea = b.BusinessArea,
                                    Description = b.Description,
                                    FormName = b.FormName,
                                    Id = b.IBOEFormID,
                                    Poc = b.Poc,
                                    PocPhone = b.PocPhone,
                                    ProposalDate = b.ProposalDate,
                                    ProposalTitle = b.ProposalTitle,
                                    Revision = b.Revision,
                                    Version = b.FormVersion,
                                    UpdateDate = b.UpdateDT,
                                    ResourceIdsIEnum = gbe.BOEFormIBOEResourcesXREFs.Where(r => r.IBOEFormID == b.IBOEFormID).Select(r => r.ResourceID),
                                    ClinContractTypesIEnum = gbe.BOEFormIBOECLINsXREFs.Where(c => c.IBOEFormID == b.IBOEFormID)
                                        .Select(c => new BoeFormClinContractTypeDTO
                                        {
                                            ClinId = c.ClinID,
                                            ContractType = c.ContractType
                                        }),
                                    WorkspaceId = b.WorkspaceID
                                }).ToList();
                }
                foreach(BOEFormIBOEDTO dto in iboes)
                {
                    dto.ResourceIds = dto.ResourceIdsIEnum.ToList();
                    dto.ResourceIdsIEnum = null;

                    dto.ClinContractTypes = dto.ClinContractTypesIEnum.ToList();
                    dto.ClinContractTypesIEnum = null;

                    toReturn.Add(dto);
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Retrieves the latest version number of the form.
        /// </summary>
        /// <returns>Latest version number of the form.</returns>
        public int GetCurrentFormVersion()
        {
            return 1;
        }

        /// <summary>
        /// Returns a collection of IBOE BOE Form DTOs based on the Collection of Ids.
        /// </summary>
        /// <param name="ids">Capture Ids.</param>
        /// <returns>The matching DTOs.</returns>
        [DbQuery]
        public override ICollection<BOEFormIBOEDTO> GetByIds(ICollection<int> ids)
        {
            List<BOEFormIBOEDTO> toReturn = null;

            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    // get the basic BOE data from the sprocResults
                    toReturn = (from b in gbe.BOEFormIBOEs
                                where ids.Contains(b.IBOEFormID)
                                orderby b.IBOEFormID
                                select new BOEFormIBOEDTO
                                {
                                    Approver = b.Approver,
                                    ApproverPhone = b.ApproverPhone,
                                    BasisAndRationale = b.BasisAndRationale,
                                    BusinessArea = b.BusinessArea,
                                    Description = b.Description,
                                    FormName = b.FormName,
                                    Id = b.IBOEFormID,
                                    Poc = b.Poc,
                                    PocPhone = b.PocPhone,
                                    ProposalDate = b.ProposalDate,
                                    ProposalTitle = b.ProposalTitle,
                                    Revision = b.Revision,
                                    Version = b.FormVersion,
                                    UpdateDate = b.UpdateDT,
                                    ResourceIdsIEnum = gbe.BOEFormIBOEResourcesXREFs.Where(r => r.IBOEFormID == b.IBOEFormID).Select(r => r.ResourceID),
                                    ClinContractTypesIEnum  = gbe.BOEFormIBOECLINsXREFs.Where(c => c.IBOEFormID == b.IBOEFormID)
                                        .Select(c => new BoeFormClinContractTypeDTO
                                        {
                                            ClinId =  c.ClinID,
                                            ContractType = c.ContractType
                                        }),
                                    WorkspaceId = b.WorkspaceID
                                }).ToList();
                }

                // post processing of resourceIDs
                foreach(BOEFormIBOEDTO iboe in toReturn)
                {
                    iboe.ResourceIds = iboe.ResourceIdsIEnum.ToList();
                    iboe.ResourceIdsIEnum = null;

                    iboe.ClinContractTypes = iboe.ClinContractTypesIEnum.ToList();
                    iboe.ClinContractTypesIEnum = null;
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Upserts an IBOE BOE Form.
        /// </summary>
        /// <param name="dtoToUpsert">Dto to upsert.</param>
        /// <returns>Id of the saved capture.</returns>
        protected override int? Upsert(BOEFormIBOEDTO dtoToUpsert)
        {
            using (StopwatchTimer sw = new StopwatchTimer(Log))
            {
                int? toReturn = null;

                if (dtoToUpsert != null)
                {

                    // assemble CSV lists of ids for resources
                    string resourceList = null;
                    if (dtoToUpsert.ResourceIds != null && dtoToUpsert.ResourceIds.Any())
                    {
                        resourceList = string.Join(",", dtoToUpsert.ResourceIds.Select(i => i.ToString()));
                    }

                    // assemble CSV lists of Clin Contract Types
                    string clinList = string.Empty;
                    if (dtoToUpsert.ClinContractTypes != null && dtoToUpsert.ClinContractTypes.Any())
                    {
                        clinList = string.Join(",", dtoToUpsert.ClinContractTypes.Where(c => c.ContractType > 0).Select(c => c.ClinId.ToString() + ":" + ((int)c.ContractType).ToString()));
                    }

                    using (GenBoeEntities gbm = new GenBoeEntities())
                    {
                        toReturn = gbm.upsertBOEFormIBOE(
                                    dtoToUpsert.Id,
                                    dtoToUpsert.UpdateDate,
                                    dtoToUpsert.WorkspaceId,
                                    dtoToUpsert.FormName,
                                    dtoToUpsert.Description,
                                    dtoToUpsert.BasisAndRationale,
                                    dtoToUpsert.ProposalTitle,
                                    dtoToUpsert.ProposalDate,
                                    dtoToUpsert.Poc,
                                    dtoToUpsert.PocPhone,
                                    dtoToUpsert.Approver,
                                    dtoToUpsert.ApproverPhone,
                                    dtoToUpsert.BusinessArea,
                                    dtoToUpsert.Version,
                                    resourceList,
                                    clinList,
                                    dtoToUpsert.Revision).FirstOrDefault().Value;


                        BOEFormIBOE iboeEntity;
                        if ((iboeEntity = gbm.BOEFormIBOEs.FirstOrDefault(b => b.IBOEFormID == dtoToUpsert.Id)) != null)
                        {
                            dtoToUpsert.UpdateDate = iboeEntity.UpdateDT;  // refresh the update-date to match the DB value
                        }
                    }
                }

                return toReturn;
            }
        }

        /// <summary>
        /// Deletes an IBOE BOE Form.
        /// </summary>
        /// <param name="dtoToDelete">Capture to delete.</param>
        /// <returns>Id of the deleted item.</returns>
        protected override int? Delete(BOEFormIBOEDTO dtoToDelete)
        {
            int? toReturn = null;

            if (dtoToDelete != null)
            {
                using (StopwatchTimer sw = new StopwatchTimer(this.Log))
                {
                    using (GenBoeEntities gbe = new GenBoeEntities())
                    {
                        gbe.deleteBOEFormIBOE(dtoToDelete.Id, dtoToDelete.UpdateDate);
                    }
                }

                toReturn = dtoToDelete.Id;
            }

            return toReturn;
        }

        /// <summary>
        /// Gets a list of currently used resources inside a workspace, excluding the boeForm passed in.
        /// </summary>
        /// <param name="wsId">Workspace Id.</param>
        /// <param name="boeFormId">The boe Form Id to not include when finding the in-use resources.</param>
        /// <returns>A list of resource ids used in a workspace.</returns>
        [DbQuery]
        public ICollection<int> CurrentlyUsedResources(int wsId, int boeFormId)
        {
            List<int> toReturn = new List<int>();
            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    // get the basic BOE data from the sprocResults
                    toReturn = (from x in gbe.BOEFormIBOEResourcesXREFs
                                join b in gbe.BOEFormIBOEs on x.IBOEFormID equals b.IBOEFormID
                                where b.WorkspaceID == wsId && b.IBOEFormID != boeFormId
                                select x.ResourceID).Distinct().ToList();
                }
            }
            return toReturn;
        }
    }
}
