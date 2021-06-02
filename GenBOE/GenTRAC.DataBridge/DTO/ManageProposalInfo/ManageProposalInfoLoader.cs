// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.DataBridge.DTO
{
    using System.Linq;
    using GenTRAC.Models;
    using IES.Common;

    /// <summary>
    /// Manage proposal info loader
    /// </summary>
    public class ManageProposalInfoLoader : IManageProposalInfoLoader
    {
        /// <summary>
        /// Logger
        /// </summary>
        protected Logger Log { get; set; }

        /// <summary>
        /// Default Constructor
        /// </summary>
        public ManageProposalInfoLoader()
        {
            this.Log = new Logger(typeof(ManageProposalInfoLoader));
        }

        /// <summary>
        /// Saves proposal info from the Admin Management page
        /// </summary>
        /// <param name="dtoToSave">Dto to save</param>
        /// <returns>Id of the updated Proposal</returns>
        public int? SaveProposalInfo(ManageProposalInfoDto dtoToSave)
        {
            int? toReturn = null;

            using (StopwatchTimer sw = new StopwatchTimer("ManageProposalInfoLoader.SaveProposalInfo", this.Log))
            {
                if (dtoToSave != null)
                {
                    // save
                    using (genTRACEntities dbModel = new genTRACEntities())
                    {
                        toReturn = dbModel.updateProposalInformation(
                            dtoToSave.ProposalId,
                            dtoToSave.UpdateDate,
                            (int)dtoToSave.NewProposalStatus,
                            dtoToSave.EstimatingSubmitsToContractsDate,
                            dtoToSave.TotalPrice,
                            dtoToSave.ChecklistSubmittedDatePricer,
                            dtoToSave.ChecklistSubmittedDatePeer,
                            dtoToSave.Comments).FirstOrDefault();
                    }
                }
            }

            return toReturn;
        }
    }
}
