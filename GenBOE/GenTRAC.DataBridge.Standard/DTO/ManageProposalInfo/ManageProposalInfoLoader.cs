// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.DataBridge.DTO
{
    using System.Linq;
    using GenTRAC.Models;
    using IES.Standard;

    /// <summary>
    /// Manage proposal info loader
    /// </summary>
    public class ManageProposalInfoLoader : IManageProposalInfoLoader
    {
        /// <summary>
        /// Logger
        /// </summary>
        protected ILogger Log { get; private set; }

        /// <summary>
        /// Default Constructor
        /// </summary>
        public ManageProposalInfoLoader(ILogger logger)
		{
			this.Log = logger;
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
