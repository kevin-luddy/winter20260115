// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using IES.Common;
    using GenBOE.Models;
    using GenBOE.Dtos;

    /// <summary>
    /// WBS DTO Data Loader
    /// </summary>
    public class WbsDTODataLoader : DataLoader<WbsDTO>, IWbsDTODataLoader
    {
        #region Constants

        readonly static private char WBS_PAD_KEY = '0';
        readonly static private char WBS_SEPARATER_VALUE_KEY = '?';
        readonly static private int WBS_PAD_LENGTH = 5;

        #endregion Constants

        /// <summary>
        /// Default Constructor
        /// </summary>
        public WbsDTODataLoader()
        {
            this.Log = new Logger(typeof(WbsDTODataLoader));
        }

        #region Retrieves

        /// <summary>
        /// Gets a collection of WbsDTOs by ids.
        /// </summary>
        /// <param name="ids">ids of WbsDTOs to retrieve</param>
        /// <returns>Collection of WbsDTOs</returns>
        [DbQuery]
        public override ICollection<WbsDTO> GetByIds(ICollection<int> ids)
        {
            List<WbsDTO> toReturn = new List<WbsDTO>();

            using (StopwatchTimer sw = new StopwatchTimer(this.Log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    toReturn = (from w in gbe.WorkBreakdownStructures
                                where ids.Contains(w.WBSID)
                                select new WbsDTO()
                                {
                                    Id = w.WBSID,
                                    WbsNumber = w.DisplayedWBSNumber,
                                    WbsPaddedNumber = w.WBSNumber,
                                    WbsTitle = w.WBSTitle,
                                    WorkspaceID = w.WorkspaceID.Value,
                                    UpdateDate = w.UpdateDT,

                                    ClinIDsIEnum = w.WBS_CLIN_BOE_XREF.Where(b => b.CLINID.HasValue).Select(b => b.CLINID.Value).Distinct().OrderBy(x => x),
                                    ClinsInUseIEnum = w.WBS_CLIN_BOE_XREF.Where(x => x.CLINID.HasValue && x.BOEID.HasValue).Select(x => x.CLINID.Value).OrderBy(x => x),

                                    inUse = w.WBS_CLIN_BOE_XREF.Any(x => x.BOEID.HasValue)
                                }).ToList();
                }

                toReturn.ForEach(x =>
                {
                    x.Level = x.WbsNumber.Count(z => z == '.');

                    x.ClinIDs = x.ClinIDsIEnum.ToCollection(); x.ClinIDsIEnum = null;
                    x.ClinsInUse = x.ClinsInUseIEnum.ToCollection(); x.ClinsInUseIEnum = null;
                });
            }

            return toReturn;
        }

        /// <summary>
        /// Gets a collection of WbsDTOs by Workspace Id.
        /// </summary>
        /// <param name="workspaceId">Id of the workspace</param>
        /// <returns>Collection of WbsDTOs</returns>
        [DbQuery]
        virtual public ICollection<WbsDTO> GetByWorkspaceId(int workspaceId)
        {
            List<WbsDTO> toReturn = new List<WbsDTO>();

            using (StopwatchTimer sw = new StopwatchTimer(this.Log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    toReturn = (from w in gbe.WorkBreakdownStructures
                                where w.WorkspaceID == workspaceId
                                select new WbsDTO()
                                {
                                    Id = w.WBSID,
                                    WbsNumber = w.DisplayedWBSNumber,
                                    WbsPaddedNumber = w.WBSNumber,
                                    WbsTitle = w.WBSTitle,
                                    WorkspaceID = w.WorkspaceID.Value,
                                    UpdateDate = w.UpdateDT,

                                    ClinIDsIEnum = w.WBS_CLIN_BOE_XREF.Where(b => b.CLINID.HasValue).Select(b => b.CLINID.Value).Distinct().OrderBy(x => x),
                                    ClinsInUseIEnum = w.WBS_CLIN_BOE_XREF.Where(x => x.CLINID.HasValue && x.BOEID.HasValue).Select(x => x.CLINID.Value).OrderBy(x => x),

                                    inUse = w.WBS_CLIN_BOE_XREF.Any(x => x.BOEID.HasValue)
                                }).ToList();
                }

                toReturn.ForEach(x =>
                {
                    x.Level = x.WbsNumber.Count(z => z == '.');

                    x.ClinIDs = x.ClinIDsIEnum.ToCollection(); x.ClinIDsIEnum = null;
                    x.ClinsInUse = x.ClinsInUseIEnum.ToCollection(); x.ClinsInUseIEnum = null;
                });
            }

            return toReturn;
        }

        /// <summary>
        /// Get unique WBS
        /// </summary>
        /// <param name="inWbsNumber">wbs number</param>
        /// <param name="inWorkspaceID">workspace iD</param>
        /// <returns>true if unique, false if not</returns>
        [DbQuery]
        virtual public bool IsWbsNumberUnique(string inWbsNumber, int inWorkspaceID, int? inWbsID)
        {
            bool isUnique = true;

            // need to pad this number 
            string paddedWBS = this.PadWBSNumber(inWbsNumber);

            using (StopwatchTimer sw = new StopwatchTimer(this.Log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    if (!inWbsID.HasValue)
                    {
                        isUnique = !gbe.WorkBreakdownStructures.Any(m => m.WBSNumber == paddedWBS && m.WorkspaceID == inWorkspaceID);
                    }
                    else
                    {
                        isUnique = !gbe.WorkBreakdownStructures.Any(m => m.WBSNumber == paddedWBS && m.WorkspaceID == inWorkspaceID && m.WBSID != inWbsID.Value);
                    }
                }
            }

            return isUnique;
        }

        /// <summary>
        /// Get the task variable IDS associated with the WBS ID
        /// </summary>
        /// <param name="inWbsID">WBS ID</param>
        /// <returns>task variable IDs</returns>
        [DbQuery]
        virtual public ICollection<int> GetTaskVariableIdsById(int inWbsID)
        {
            ICollection<int> toReturn = new Collection<int>();

            using (StopwatchTimer sw = new StopwatchTimer(this.Log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    toReturn = (from s in gbe.SumOfBOE_OrdinaryVariableXREF
                                where s.WBSID == inWbsID
                                select s.OrdinaryVariableID).ToList();
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Get the workspace variable IDs associated with the WBS ID
        /// </summary>
        /// <param name="inWbsID">WBS ID</param>
        /// <returns>workspace variable IDs</returns>
        [DbQuery]
        virtual public ICollection<int> GetWorkspaceVariableIdsById(int inWbsID)
        {
            ICollection<int> toReturn = new Collection<int>();

            using (StopwatchTimer sw = new StopwatchTimer(this.Log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    toReturn = (from s in gbe.SumOfBOE_WorkspaceVariableXREF
                                where s.WBSID == inWbsID
                                select s.WorkspaceVariableID).ToList();
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Check if the WBS is already tied to a Material BOE
        /// </summary>
        /// <param name="inWbsID">WBS id</param>
        /// <returns>bool</returns>
        [DbQuery]
        virtual public bool IsWbsTiedToMaterialBoe(int inBoeID, int inWbsID)
        {
            bool IsWbsTiedToMaterialBoe = false;

            using (StopwatchTimer sw = new StopwatchTimer(this.Log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    IsWbsTiedToMaterialBoe = (from m in gbe.WBS_CLIN_BOE_XREF
                                              from b in gbe.BOEs
                                              where m.WBSID == inWbsID 
                                                && m.BOEID == b.BOEID 
                                                && b.IsMaterial
                                                && m.BOEID != inBoeID
                                              select 1).Any();
                }
            }

            return IsWbsTiedToMaterialBoe;
        }

        /// <summary>
        /// Check if the WBS and CLIN is already tied to a Material BOE
        /// </summary>
        /// <param name="inWbsID">WBS id</param>
        /// <param name="inClinID">CLIN id</param>
        /// <returns>bool</returns>
        [DbQuery]
        virtual public bool IsWbsTiedToMaterialBoeAndClin(int inBoeID, int inWbsID, int inClinID)
        {
            bool IsWbsTiedToMaterialBoe = false;

            using (StopwatchTimer sw = new StopwatchTimer(this.Log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    IsWbsTiedToMaterialBoe =  (from m in gbe.WBS_CLIN_BOE_XREF
                                               from b in gbe.BOEs
                                               from w in gbe.WorkBreakdownStructures
                                               where m.WBSID == inWbsID &&
                                                   m.CLINID == inClinID &&
                                                   m.WBSID == w.WBSID &&
                                                   m.BOEID == b.BOEID &&
                                                   m.BOEID != inBoeID &&
                                                   b.IsMaterial
                                               select 1).Any();
                }
            }

            return IsWbsTiedToMaterialBoe;
        }

        /// <summary>
        /// Gets a collection of all WBS that are parents of the specified WBS in the workspace
        /// </summary>
        /// <param name="workspaceId">Workspace Id</param>
        /// <param name="wbsNumber">The WBS number to find parents of</param>
        /// <returns>A collection of WBS elements, ordered by Level</returns>
        [DbQuery]
        virtual public ICollection<WbsDTO> GetAllParentWbs(int workspaceId, string wbsNumber)
        {
            List<WbsDTO> toReturn = new List<WbsDTO>();

            using (StopwatchTimer sw = new StopwatchTimer(this.Log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    toReturn = (from w in gbe.WorkBreakdownStructures
                                where w.WorkspaceID == workspaceId && wbsNumber.StartsWith(w.WBSNumber)
                                select new WbsDTO()
                                {
                                    Id = w.WBSID,
                                    WbsNumber = w.DisplayedWBSNumber,
                                    WbsPaddedNumber = w.WBSNumber,
                                    WbsTitle = w.WBSTitle,
                                    WorkspaceID = w.WorkspaceID.Value,
                                    UpdateDate = w.UpdateDT,

                                    ClinIDsIEnum = w.WBS_CLIN_BOE_XREF.Where(b => b.CLINID.HasValue).Select(b => b.CLINID.Value).Distinct().OrderBy(x => x),
                                    ClinsInUseIEnum = w.WBS_CLIN_BOE_XREF.Where(x => x.CLINID.HasValue && x.BOEID.HasValue).Select(x => x.CLINID.Value).OrderBy(x => x),

                                    inUse = w.WBS_CLIN_BOE_XREF.Any(x => x.BOEID.HasValue)
                                }).ToList();
                }

                toReturn.ForEach(x =>
                {
                    x.Level = x.WbsNumber.Count(z => z == '.');

                    x.ClinIDs = x.ClinIDsIEnum.ToCollection(); x.ClinIDsIEnum = null;
                    x.ClinsInUse = x.ClinsInUseIEnum.ToCollection(); x.ClinsInUseIEnum = null;
                });
            }

            // need to do the filtration, because we could not add the + '.' into the LINQ to SQL (it's not capable of handling that).. But, at least it filtered the data down 
            // to mostly needed (only the original item is pulled extra, which this will remove)
            // then we sort..
            toReturn = toReturn.Where(x => wbsNumber.StartsWith(x.WbsNumber + '.', StringComparison.CurrentCultureIgnoreCase)).OrderBy(x => x.Level, SortOrder.Ascending).ToList();

            return toReturn;
        }

        /// <summary>
        /// Gets a collection of all WBS that are children of the specified WBS in the workspace
        /// </summary>
        /// <param name="workspaceId">Workspace Id</param>
        /// <param name="wbsNumber">The WBS number to find children of</param>
        /// <returns>A collection of WBS elements, ordered by Level</returns>
        [DbQuery]
        virtual public ICollection<WbsDTO> GetAllChildWbs(int workspaceId, string wbsNumber)
        {
            List<WbsDTO> toReturn;

            using (StopwatchTimer sw = new StopwatchTimer(this.Log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    toReturn = (from w in gbe.WorkBreakdownStructures
                                where w.WorkspaceID == workspaceId 
                                    && w.WBSNumber.StartsWith(wbsNumber)
                                select new WbsDTO()
                                {
                                    Id = w.WBSID,
                                    WbsNumber = w.DisplayedWBSNumber,
                                    WbsPaddedNumber = w.WBSNumber,
                                    WbsTitle = w.WBSTitle,
                                    WorkspaceID = w.WorkspaceID.Value,
                                    UpdateDate = w.UpdateDT,

                                    ClinIDsIEnum = w.WBS_CLIN_BOE_XREF.Where(b => b.CLINID.HasValue).Select(b => b.CLINID.Value).Distinct().OrderBy(x => x),
                                    ClinsInUseIEnum = w.WBS_CLIN_BOE_XREF.Where(x => x.CLINID.HasValue && x.BOEID.HasValue).Select(x => x.CLINID.Value).OrderBy(x => x),

                                    inUse = w.WBS_CLIN_BOE_XREF.Any(x => x.BOEID.HasValue)
                                }).ToList();
                }

                toReturn.ForEach(x =>
                {
                    x.Level = x.WbsNumber.Count(z => z == '.');

                    x.ClinIDs = x.ClinIDsIEnum.ToCollection(); x.ClinIDsIEnum = null;
                    x.ClinsInUse = x.ClinsInUseIEnum.ToCollection(); x.ClinsInUseIEnum = null;
                });
            }

            // need to do the filtration, because we could not add the + '.' into the LINQ to SQL (it's not capable of handling that).. But, at least it filtered the data down 
            // to mostly needed (only the original item is pulled extra, which this will remove)
            // then we sort..
            toReturn = toReturn.Where(x => x.WbsNumber.StartsWith(wbsNumber + '.', StringComparison.CurrentCultureIgnoreCase)).OrderBy(x => x.Level, SortOrder.Ascending).ToList();

            return toReturn;
        }

        /// <summary>
        /// Gets BOE IDs for BOEs that belong to the Wbs and all of it's descendants
        /// </summary>
        /// <param name="wbsId">Wbs Id</param>
        /// <returns>A collection of Boe Ids</returns>
        [DbQuery]
        virtual public ICollection<int> GetBoeIdsForWbsIdWithNesting(int wbsId)
        {
            ICollection<int> result = new Collection<int>(); ;

            using (StopwatchTimer sw = new StopwatchTimer(this.Log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    result = (from originalWbs in gbe.WorkBreakdownStructures
                              from wbs in gbe.WorkBreakdownStructures
                              from xref in gbe.WBS_CLIN_BOE_XREF
                              where originalWbs.WBSID == wbsId
                                  && (wbs.WBSID == originalWbs.WBSID
                                          || (wbs.WorkspaceID == originalWbs.WorkspaceID
                                              && wbs.WBSNumber.StartsWith(originalWbs.WBSNumber + ".")))
                                  && xref.WBSID == wbs.WBSID
                                  && xref.BOEID.HasValue
                              select xref.BOEID.Value).ToList();
                }
            }

            return result;
        }

        /// <summary>
        /// Takes in a WBS unPadded number and return an padded WBS number
        /// </summary>
        /// <returns>padded WBS number</returns>
        public string PadWBSNumber(string unPaddedWBSNum)
        {
            if (string.IsNullOrEmpty(unPaddedWBSNum))
            {
                throw new ArgumentNullException(nameof(unPaddedWBSNum));
            }

            string toReturn = null;

            string[] splitWBS = unPaddedWBSNum.Split('.');

            foreach (string word in splitWBS)
            {
                string pad = null;

                // only pad if it starts with a #.
                if (!string.IsNullOrEmpty(word) && Char.IsDigit(word, 0))
                {
                    char[] letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz".ToCharArray();
                    int trailingLettersIndex = word.IndexOfAny(letters);
                    string trailingLetters = trailingLettersIndex >= 0 ? word.Substring(trailingLettersIndex) : string.Empty;
                    string numericalValue = trailingLettersIndex >= 0 ? word.Substring(0, trailingLettersIndex) : word;

                    for (int i = 0; i < WBS_PAD_LENGTH - numericalValue.Length; i++)
                    {
                        pad = pad + WBS_PAD_KEY;
                    }

                    pad = pad + numericalValue + WBS_SEPARATER_VALUE_KEY + trailingLetters + WBS_SEPARATER_VALUE_KEY;

                    toReturn = toReturn + pad + '.';
                }
                else
                {
                    toReturn = toReturn + word + '.';
                }
            }

            toReturn = toReturn.TrimEnd('.');

            return toReturn;
        }

        #endregion

        #region Commits

        /// <summary>
        /// Remaps the Task and Workspace variables from a WBS to a BOE
        /// </summary>
        /// <param name="wbsId">Wbs Id</param>
        /// <param name="boeId">Boe Id</param>
        virtual public void RemapTaskAndWorkspaceVariablesFromWbsToBoe(int wbsId, int boeId)
        {
            using (StopwatchTimer sw = new StopwatchTimer(this.Log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    gbe.updateSumOfBOEs(wbsId, boeId);
                }
            }
        }

        /// <summary>
        /// Upsert a WbsDTO. if successfully inserted The dtoToUpsert.Id is updated to have the new id.
        /// </summary>
        /// <param name="dtoToUpsert">dto to upsert</param>
        /// <returns>id of upserted entry</returns>
        protected override int? Upsert(WbsDTO dtoToUpsert)
        {
            if (dtoToUpsert == null)
            {
                throw new ArgumentNullException(nameof(dtoToUpsert));
            }

            int? toReturn = null;
            string clins = null;

            using (StopwatchTimer sw = new StopwatchTimer(this.Log))
            {
                // Need to get the string of CLINs associated 
                if (dtoToUpsert.ClinIDs != null && dtoToUpsert.ClinIDs.Count > 0)
                {
                    clins = string.Join(",", dtoToUpsert.ClinIDs.Select(x => x).ToArray()) + ","; // need trailing , for sproc
                }

                using (GenBoeEntities gbm = new GenBoeEntities())
                {
                    toReturn = gbm.upsertWBSElement(
                        dtoToUpsert.Id,
                        this.PadWBSNumber(dtoToUpsert.WbsNumber),
                        dtoToUpsert.WbsNumber,
                        dtoToUpsert.WbsTitle,
                        clins,// string of CLINs
                        dtoToUpsert.WorkspaceID,
                        dtoToUpsert.UpdateDate).FirstOrDefault();

                    dtoToUpsert.Id = toReturn ?? 0;
                }
            }

            return toReturn;
        }

        /// <summary>
        /// delete the selected WBS
        /// </summary>
        /// <param name="dtoToDelete">the element to delete</param>
        /// <returns>id of the deleted item</returns>
        protected override int? Delete(WbsDTO dtoToDelete)
        {
            int? toReturn = null;

            if (dtoToDelete == null)
            {
                throw new ArgumentNullException(nameof(dtoToDelete));
            }

            using (StopwatchTimer sw = new StopwatchTimer(this.Log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    toReturn = gbe.deleteWorkBreakdownStructure(dtoToDelete.Id, dtoToDelete.UpdateDate);
                }
            }

            return toReturn;
        }

        #endregion

        #region Private
        /// <summary>
        /// Sets the WBS level and clins in use properties for the given WBS dtos.
        /// </summary>
        /// <param name="wbsDtos">WBS dtos.</param>
        /// <param name="xrefVals">Clin/Wbs/Boe xrefs.</param>
        private void SetWbsLevelAndClinsInUse(ref ICollection<WbsDTO> wbsDtos, ICollection<WBS_CLIN_BOE_XREF> xrefVals)
        {
            if (wbsDtos != null && wbsDtos.Any())
            {
                foreach (WbsDTO wbs in wbsDtos)
                {
                    wbs.Level = this.CalculateLevel(wbs.WbsNumber);

                    Collection<int> clinsInUse = new Collection<int>();

                    var clinsForWbsXref = xrefVals.Where(x => x.WBSID == wbs.Id);

                    foreach (var row in clinsForWbsXref)
                    {
                        if (row.CLINID.HasValue && row.BOEID.HasValue)
                        {
                            clinsInUse.Add(row.CLINID.Value);
                        }

                        if (row.BOEID.HasValue)
                        {
                            wbs.inUse = true;
                        }
                    }

                    wbs.ClinsInUse = clinsInUse;
                    wbs.ClinIDs = xrefVals.Where(b => b.WBSID == wbs.Id && b.CLINID.HasValue).Select(b => b.CLINID.Value).Distinct().ToCollection();
                }
            }
        }
        /// <summary>
        /// Takes in a WBS Padded number and return the level it is.
        /// </summary>
        /// <returns>calculated level of the padded WBS number</returns>
        private int CalculateLevel(string paddedWBSNum)
        {
            int toReturn = -1;

            toReturn = paddedWBSNum.Split('.').Length - 1;

            return toReturn;
        }
        #endregion
    }
}