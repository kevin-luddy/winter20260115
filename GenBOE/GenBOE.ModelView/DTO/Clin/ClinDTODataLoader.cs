// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text;
    using IES.Common;
    using GenBOE.Dtos;
    using GenBOE.Models;

    public class ClinDTODataLoader : DataLoader<ClinDTO>, IClinDTODataLoader
    {
        #region Static 'Constants' used for Clin Padding

        readonly static private string CLIN_PAD_KEY = "0";
        readonly static private char CLIN_SEPARATER_VALUE_KEY = '?';
        readonly static private int CLIN_PAD_LENGTH = 20;

        #endregion

        /// <summary>
        /// Default constructor
        /// </summary>
        public ClinDTODataLoader() 
        {
            this.Log = new Logger(typeof(ClinDTODataLoader));
        }

        #region Retrieves

        /// <summary>
        /// Returns a collection of CLIN DTOs based on the Collection of Ids
        /// </summary>
        /// <param name="ids">Capture Ids</param>
        /// <returns>The matching DTOs</returns>
        [DbQuery]
        public override ICollection<ClinDTO> GetByIds(ICollection<int> ids)
        {
            List<ClinDTO> toReturn = new List<ClinDTO>();

            if (ids != null)
            {
                using (StopwatchTimer sw = new StopwatchTimer(Log))
                {
                    using (GenBoeEntities dbModel = new GenBoeEntities())
                    {
                        toReturn = dbModel.CLINs.Where(c => ids.Contains(c.CLINID)).Select(
                            c => new ClinDTO 
                            { 
                                Id = c.CLINID,
                                ClinNumber = c.DisplayedCLINNumber,
                                ClinPaddedNumber = c.CLINNumber,
                                ClinTitle = c.CLINTitle,
                                StartDate = c.CLINStartDate,
                                EndDate = c.CLINEndDate,
                                UpdateDate = c.UpdateDT,
                                WorkspaceID = c.WorkspaceID,
                                ContractType = c.ContractTypeID == null ? Constants.CONTRACT_TYPE_NOT_SET : c.ContractTypeID.Value,

                                InUse = c.WBS_CLIN_BOE_XREF.Any()
                            }).ToList();


                        toReturn.ToList().ForEach(w =>
                        {
                            w.StartDate = w.StartDate.Normalize();
                            w.EndDate = w.EndDate.Normalize();
                        });
                    }
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Get the number of BOEs associated with a particular CLIN ID
        /// </summary>
        /// <param name="clinID">CLIN ID</param>
        /// <returns>the number of BOEs associated with the CLIN </returns>
        [DbQuery]
        virtual public int GetBoeCountByClinID(int clinID)
        {
            int toReturn = 0;

            using (StopwatchTimer sw = new StopwatchTimer(Log))
            {
                using (GenBoeEntities dbModel = new GenBoeEntities())
                {
                    toReturn = dbModel.WBS_CLIN_BOE_XREF.Where(x => x.CLINID == clinID).Count();
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Gets the Clin DTOs
        /// </summary>
        /// <param name="wsId">the WorkSpace to get ClinS</param>
        /// <returns>list of Clin DTOs</returns>
        [DbQuery]
        virtual public Collection<ClinDTO> GetByWorkspaceId(int wsId)
        {
            Collection<ClinDTO> toReturn = new Collection<ClinDTO>();

            using (StopwatchTimer sw = new StopwatchTimer(Log))
            {
                using (GenBoeEntities dbModel = new GenBoeEntities())
                {
                    toReturn = dbModel.CLINs.Where(c => c.WorkspaceID == wsId).Select(
                            c => new ClinDTO
                            {
                                Id = c.CLINID,
                                ClinNumber = c.DisplayedCLINNumber,
                                ClinPaddedNumber = c.CLINNumber,
                                ClinTitle = c.CLINTitle,
                                StartDate = c.CLINStartDate,
                                EndDate = c.CLINEndDate,
                                UpdateDate = c.UpdateDT,
                                WorkspaceID = c.WorkspaceID,
                                ContractType = c.ContractTypeID == null ? Constants.CONTRACT_TYPE_NOT_SET : c.ContractTypeID.Value,

                                InUse = c.WBS_CLIN_BOE_XREF.Any()
                            }).ToCollection();


                    toReturn.ToList().ForEach(w =>
                    {
                        w.StartDate = w.StartDate.Normalize();
                        w.EndDate = w.EndDate.Normalize();
                    });
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Gets a single collection of task variable IDs for a clin ID
        /// </summary>
        /// <param name="clinID"></param>
        /// <returns></returns>
        [DbQuery]
        virtual public Collection<int> GetTaskVariableIDsByClinID(int clinID)
        {
            Collection<int> toReturn = new Collection<int>();

            using (StopwatchTimer sw = new StopwatchTimer(Log))
            {
                using (GenBoeEntities dbModel = new GenBoeEntities())
                {
                    toReturn = dbModel.SumOfBOE_OrdinaryVariableXREF.Where(x => x.CLINID.HasValue && x.CLINID == clinID).Select(x => x.OrdinaryVariableID).ToCollection();
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Gets a single collection of workspace variable IDs for a clin ID
        /// </summary>
        /// <param name="clindId"></param>
        /// <returns></returns>
        [DbQuery]
        virtual public Collection<int> GetWorkspaceVariableIDsByClinID(int clinId)
        {
            Collection<int> toReturn = new Collection<int>();

            using (StopwatchTimer sw = new StopwatchTimer(Log))
            {
                using (GenBoeEntities dbModel = new GenBoeEntities())
                {
                    toReturn = dbModel.SumOfBOE_WorkspaceVariableXREF.Where(x => x.CLINID.HasValue && x.CLINID == clinId).Select(x => x.WorkspaceVariableID).ToCollection();
                }
            }
        
            return toReturn;
        }

        #endregion

        #region Commits

        /// <summary>
        /// Upserts a Clin
        /// </summary>
        /// <param name="dtoToUpsert">Dto to upsert</param>
        /// <returns>Id of the saved capture</returns>
        protected override int? Upsert(ClinDTO dtoToUpsert)
        {
            int? toReturn = null;

            using (StopwatchTimer sw = new StopwatchTimer(Log))
            {
                if (dtoToUpsert != null)
                {
                    // save
                    using (GenBoeEntities dbModel = new GenBoeEntities())
                    {

                        toReturn = dbModel.upsertCLIN(dtoToUpsert.Id, PadClinNumber(dtoToUpsert.ClinNumber),
                            dtoToUpsert.ClinTitle, dtoToUpsert.StartDate, dtoToUpsert.EndDate, dtoToUpsert.WorkspaceID, dtoToUpsert.UpdateDate, dtoToUpsert.ClinNumber, dtoToUpsert.ContractType != Constants.CONTRACT_TYPE_NOT_SET ? (int?)dtoToUpsert.ContractType : null).FirstOrDefault();

                        dtoToUpsert.Id = toReturn ?? dtoToUpsert.Id;
                    }
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Deletes a capture
        /// </summary>
        /// <param name="dtoToDelete">Capture to delete.</param>
        /// <returns>Id of the deleted item.</returns>
        protected override int? Delete(ClinDTO dtoToDelete)
        {
            int? toReturn = null;

            using (StopwatchTimer sw = new StopwatchTimer(Log))
            {
                if (dtoToDelete != null)
                {
                    using (GenBoeEntities dbModel = new GenBoeEntities())
                    {
                        toReturn = dbModel.deleteCLIN(dtoToDelete.Id, dtoToDelete.UpdateDate);
                    }
                }
            }

            return toReturn;
        }

        #endregion

        /// <summary>
        /// Take in a CLIN unPadded number and return a padded CLIN number
        /// </summary>
        /// <returns></returns>
        public string PadClinNumber(string unPaddedCLINNum)
        {
            string toReturn = null;
            string wordValue;

            if (unPaddedCLINNum != null)
            {
                string[] splitCLIN = unPaddedCLINNum.Split('.');

                foreach (string word in splitCLIN)
                {
                    wordValue = word;

                    if (wordValue.Length == 0)
                    {
                        wordValue = "0";
                    }

                    // only pad if it starts with a #.
                    if (Char.IsDigit(wordValue, 0))
                    {
                        char[] letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz".ToCharArray();
                        int trailingLettersIndex = wordValue.IndexOfAny(letters);
                        string trailingLetters = trailingLettersIndex >= 0 ? wordValue.Substring(trailingLettersIndex) : string.Empty;
                        string numericalValue = trailingLettersIndex >= 0 ? wordValue.Substring(0, trailingLettersIndex) : wordValue;
                        StringBuilder pad = new StringBuilder();

                        if (numericalValue.Length <= CLIN_PAD_LENGTH)
                        {
                            pad.Insert(0, CLIN_PAD_KEY, CLIN_PAD_LENGTH - numericalValue.Length);
                            pad.Append(numericalValue).Append(CLIN_SEPARATER_VALUE_KEY).Append(trailingLetters).Append(CLIN_SEPARATER_VALUE_KEY);

                            toReturn = toReturn + pad.ToString() + '.';
                        }
                        else
                        {
                            // number is too long, why are the inserting a number that is more than 20 digits?
                            toReturn = toReturn + wordValue + '.';
                        }
                    }
                    else
                    {
                        toReturn = toReturn + wordValue + '.';
                    }
                }

                toReturn = toReturn.TrimEnd('.');
            }
            return toReturn;
        }
    }
}