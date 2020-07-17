// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace RPM.DataBridge.DataLoaders
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using GenTRAC.Models;
    using IES.Common;
    using RPM.DataBridge.Models;

    /// <summary>
    /// The data loader for Proposal data.
    /// </summary>
    public class ProposalDataMVDataLoader
    {
        /// <summary>
        /// The logger.
        /// </summary>
        private Logger log;

        /// <summary>
        /// Constructor
        /// </summary>
        public ProposalDataMVDataLoader()
        {
            this.log = new Logger(typeof(ProposalDataMVDataLoader));
        }

        /// <summary>
        /// Populates Lines of Business short names.
        /// </summary>
        /// <returns>List of all Lines of Business as strings from the database.</returns>
        public ICollection<string> GetLoBShortNames()
        {
            List<string> toReturn = null;

            using (StopwatchTimer sw = new StopwatchTimer(this.log))
            {
                using (genTRACEntities ptme = new genTRACEntities())
                {
                    ptme.Database.CommandTimeout = 360;

                    toReturn = (from l in ptme.LineOfBusinessLUs
                                select l.LineOfBusinessName).ToList();
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Populates Lines of Business for Proposal Data Model View
        /// </summary>
        /// <returns>List of all Lines of Business as strings from the database</returns>
        public ICollection<string> GetLoBs()
        {
            List<string> toReturn = null;

            using (StopwatchTimer sw = new StopwatchTimer(this.log))
            {
                using (genTRACEntities ptme = new genTRACEntities())
                {
                    ptme.Database.CommandTimeout = 360;

                    toReturn = (from l in ptme.LineOfBusinessLUs
                                where l.IsActive
                                select l.LineOfBusinessName).ToList();
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Gets the contract types.
        /// </summary>
        /// <returns>List of active Contract Types.</returns>
        public ICollection<string> GetContractTypes()
        {
            List<string> toReturn = null;

            using (StopwatchTimer sw = new StopwatchTimer(this.log))
            {
                using (genTRACEntities ptme = new genTRACEntities())
                {
                    ptme.Database.CommandTimeout = 360;

                    toReturn = (from l in ptme.ContractTypeLUs
                                where l.IsActive
                                select l.ContractType).ToList();
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Populates Program Areas for Proposal Data Model View
        /// </summary>
        /// <returns>List of Program Area Model Views containing Program Area data from the database</returns>
        public ICollection<ProgramAreaModelView> GetProgramAreaShortNames()
        {
            List<ProgramAreaModelView> toReturn = null;

            using (StopwatchTimer sw = new StopwatchTimer(this.log))
            {
                using (genTRACEntities ptme = new genTRACEntities())
                {
                    ptme.Database.CommandTimeout = 360;

                    toReturn = (from p in ptme.ProgramAreaLUs
                                where p.IsActive
                                select new ProgramAreaModelView
                                {
                                    ProgramArea = p.ProgramAreaName,
                                    LineOfBusiness = p.LineOfBusinessLU.LineOfBusinessName
                                }).ToList();
                }
            }

            return toReturn;
        }
        
        /// <summary>
         /// Populates Program Areas for Proposal Data Model View
         /// </summary>
         /// <returns>List of Program Area Model Views containing Program Area data from the database</returns>
        public ICollection<ProgramAreaModelView> GetProgramAreas()
        {
            List<ProgramAreaModelView> toReturn = null;

            using (StopwatchTimer sw = new StopwatchTimer(this.log))
            {
                using (genTRACEntities ptme = new genTRACEntities())
                {
                    ptme.Database.CommandTimeout = 360;

                    toReturn = (from p in ptme.ProgramAreaLUs
                                where p.IsActive
                                select new ProgramAreaModelView
                                {
                                    ProgramArea = p.ProgramAreaName,
                                    LineOfBusiness = p.LineOfBusinessLU.LineOfBusinessName
                                }).ToList();
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Populates Proposal information for the Metrics model view.
        /// </summary>
        /// <param name="year">The year to retrieve the Model over.</param>
        /// <returns>Metrics Model Views containing Proposal data from the database.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        public ICollection<MetricsModelView> GetMetricsModelView(int year)
        {
            List<MetricsModelView> toReturn = null;
            DateTime currentYearStart = new DateTime(year, 1, 1);
            DateTime currentYearEnd = new DateTime(year, 12, 31);
            using (StopwatchTimer sw = new StopwatchTimer(this.log))
            {
                using (genTRACEntities ptme = new genTRACEntities())
                {
                    ptme.Database.CommandTimeout = 360;

                    toReturn = (from p in ptme.genTracDatas 
                                join pc in ptme.ProposalChecklists on p.genTracProposalID equals pc.ProposalID
                                where (p.ProposalStatus == "Submitted" || p.ProposalStatus == "Completed") && p.CreatedDate.HasValue && p.CreatedDate.Value >= currentYearStart && p.CreatedDate.Value <= currentYearEnd
                                select new MetricsModelView
                                {
                                    Contact = p.ContractsLead,
                                    SVal = p.SubmittedValue,
                                    Profit = pc.ProfitFee,
                                    LOB = p.LineOfBusinessName,
                                    PA = p.ProgramAreaName,
                                    PropType = p.ProposalType,
                                    Pricer = p.LeadEstimator,
                                    ContType = p.ContractType,
                                    ProposalStartDate = p.CreatedDate.Value
                                }).ToList();
                }
            }

            // format Dates and Values
            foreach (MetricsModelView proposal in toReturn)
            {
                proposal.Month = proposal.ProposalStartDate.Month;

                if (proposal.SVal.HasValue)
                {
                    proposal.SVal /= 1000000m;
                }

                if (proposal.Profit.HasValue)
                {
                    proposal.Profit /= 1000000m;
                }

                if (!string.IsNullOrWhiteSpace(proposal.ContType) && proposal.ContType.Contains(","))
                {
                    proposal.ContType = "Hybrid";
                }
                else
                {
                    switch (proposal.ContType)
                    {
                        case "Cost Plus Award Fee":
                        case "Cost Plus Fixed Fee":
                        case "Cost Plus Incentive Fee":
                            proposal.ContType = "CP";
                            break;
                        case "Firm Fixed Price":
                        case "Fixed Price Award Fee":
                        case "Fixed Price Incentive":
                        case "Fixed Price Incentive Award":
                        case "Fixed Price Incentive Fee":
                        case "Fixed Price Level of Effort":
                            proposal.ContType = "FP";
                            break;
                        case "IWTA-FWP":
                        case "IWTA-C":
                        case "IWTA-FCC":
                        case "IWTA-P":
                            proposal.ContType = "IWTA";
                            break;
                        case "Time and Material Level of Effort":
                        case "Time and Material Labor Hour":
                        case "Time and Materials":
                            proposal.ContType = "T&M";
                            break;
                        case "Indefinite Delivery / Indefinite Quantity (IDIQ)":
                            proposal.ContType = "IDIQ";
                            break;
                        default:
                            proposal.ContType = "Other";
                            break;
                    }
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Populates Proposal information for the Proposal Data Model View.
        /// </summary>
        /// <returns>Proposal Model Views containing Proposal data from the database.</returns>
        public ICollection<ProposalModelView> GetProposalModelView()
        {
            List<ProposalModelView> toReturn = null;

            using (StopwatchTimer sw = new StopwatchTimer(this.log))
            {
                using (genTRACEntities ptme = new genTRACEntities())
                {
                    ptme.Database.CommandTimeout = 360;
                    toReturn = (from p in ptme.genTracDatas
                                select new ProposalModelView
                                {
                                    Title = p.ProposalTitle,
                                    Lead = p.LeadEstimator,   // LeadEstimator
                                    EVal = p.EstimatedValue, 
                                    SVal = p.SubmittedValue, 
                                    LOB = p.LineOfBusinessName,
                                    BoeTool = p.BOETool,
                                    PA = p.ProgramAreaName,
                                    Cust = p.Customer,
                                    CusType = p.CustomerType,
                                    PropType = p.ProposalType,
                                    PricingTool = p.PricingTool,
                                    Price1 = p.AdditionalEstimatingResource1,
                                    Price2 = p.AdditionalEstimatingResource2,
                                    Cost = p.CostVolumeLead,
                                    ContType = p.ContractType,
                                    Ref = p.TrackingNumber,
                                    Status = p.ProposalStatus,
                                    ProposalStartDate = p.ProposalStartDate,
                                    ProposalEndDate = p.ProposalEndDate,
                                    CreatedDate = p.CreatedDate,
                                    SubmittalDate = p.SubmittalDate,
                                    EoC = p.ElementsOfCost,
                                    IWTA = p.IWTA,
                                    PrmSub = p.PrimeOrSub,
                                    PrgName = p.ProgramName,
                                    ContLdr = p.ContractsLead,
                                    RFP = p.RFPNumber,
                                    Mgr = p.Manager,
                                    IndpRev = p.IndependentReviewer,
                                    CSA = p.CoverSheetApprover,
                                    PV = p.PricingVerifier,
                                    ML = p.MaterialLead,
                                    SL = p.SubcontractLead
                                }).ToList();
                }
            }

            // format Dates and Values
            foreach (ProposalModelView proposal in toReturn)
            {
                // strip out whitespace values for PrmSub
                if (string.IsNullOrWhiteSpace(proposal.PrmSub))
                {
                    proposal.PrmSub = null;
                }

                if (proposal.ProposalStartDate.HasValue)
                {
                    proposal.StDate = proposal.ProposalStartDate.Value.ToString("yyyy-MM-dd");
                }

                if (proposal.ProposalEndDate.HasValue)
                {
                    proposal.EndDate = proposal.ProposalEndDate.Value.ToString("yyyy-MM-dd");
                }

                if (proposal.CreatedDate.HasValue)
                {
                    proposal.CrDate = proposal.CreatedDate.Value.ToString("yyyy-MM-dd");
                }

                if (proposal.SubmittalDate.HasValue)
                {
                    proposal.SubDate = proposal.SubmittalDate.Value.ToString("yyyy-MM-dd");
                }

                if (proposal.EVal.HasValue)
                {
                    proposal.EVal /= 1000000m;
                }

                if (proposal.SVal.HasValue)
                {
                    proposal.SVal /= 1000000m;
                }
            }

            return toReturn;
        }
    }
}
