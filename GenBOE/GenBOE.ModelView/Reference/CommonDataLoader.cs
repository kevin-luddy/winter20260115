// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.Common
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using GenBOE.Dtos;
    using GenBOE.Models;
    using IES.Common;
    using IES.Common.classes;
    using IES.Common.PickList;

    /// <summary>
    /// This class loads up Lookup/Reference table values via LINQ.  The values
    /// should ideally be cached to any added overhead from the load via LINQ 
    /// should be negligible the first time and directly from cache after that.
    /// </summary>
    public class CommonDataLoader : ICommonDataLoader
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public CommonDataLoader()
        { }

        /// <summary>
        /// Gets the Spread Curves via LINQ
        /// </summary>
        /// <returns>all the Spread Curve data</returns>
        [DbQuery]
        public virtual Collection<SpreadCurveModelView> GetSpreadCurve()
        {
            Collection<SpreadCurveModelView> toReturn = null;
            using (GenBoeEntities gbe = new GenBoeEntities())
            {

                IEnumerable<SpreadCurveModelView> resultLinq = from s in gbe.SpreadCurveLUs
                                                                where s.SpreadCurveID != (int) SpreadCurves.Load &&
                                                                 s.SpreadCurveID != (int)SpreadCurves.Level
                                                               select new SpreadCurveModelView
                                                           {
                                                               SpreadCurveID = (SpreadCurves)s.SpreadCurveID,
                                                               SpreadCurveName = s.SpreadCurve
                                                           };
                toReturn = new Collection<SpreadCurveModelView>(resultLinq.ToArray());
            }
            return toReturn;
        }

        /// <summary>
        /// Gets the Project Map Spread Curves via LINQ
        /// </summary>
        /// <returns>all the Spread Curve data</returns>
        [DbQuery]
        public virtual Collection<SpreadCurveModelView> GetProjectMapSpreadCurve()
        {
            Collection<SpreadCurveModelView> toReturn = null;
            using (GenBoeEntities gbe = new GenBoeEntities())
            {

                IEnumerable<SpreadCurveModelView> resultLinq = from s in gbe.SpreadCurveLUs
                    where s.SpreadCurveID == (int)SpreadCurves.Level ||
                          s.SpreadCurveID == (int)SpreadCurves.DiscreteCost ||
                          s.SpreadCurveID == (int)SpreadCurves.DiscreteHours ||
                          s.SpreadCurveID == (int)SpreadCurves.SpreadCurve3
                    select new SpreadCurveModelView
                    {
                        SpreadCurveID = (SpreadCurves)s.SpreadCurveID,
                        SpreadCurveName = s.SpreadCurve
                    };
                toReturn = new Collection<SpreadCurveModelView>(resultLinq.ToArray());
            }
            return toReturn;
        }

        /// <summary>
        /// Get the list of system Roles via LINQ
        /// </summary>
        /// <returns>list of roles</returns>
        [DbQuery]
        public virtual Collection<RoleModelView> GetRoles()
        {
            Collection<RoleModelView> toReturn = null;
            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                IEnumerable<RoleModelView> resultLinq = from r in gbe.RoleLUs
                                                        select new RoleModelView
                                                        {
                                                            RoleID = r.RoleID,
                                                            RoleName = r.RoleName
                                                        };
                toReturn = new Collection<RoleModelView>(resultLinq.ToArray());
            }
            return toReturn;
        }

        /// <summary>
        /// Get the list of Sort By via LINQ
        /// </summary>
        /// <returns>list of roles</returns>
        [DbQuery]
        public virtual Collection<SortByModelView> GetSortBy()
        {
            Collection<SortByModelView> toReturn = null;
            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                IEnumerable<SortByModelView> resultLinq = from r in gbe.SortByLUs
                                                        select new SortByModelView
                                                        {
                                                            SortByID = r.SortByID,
                                                            SortBy = r.SortBy
                                                        };
                toReturn = new Collection<SortByModelView>(resultLinq.ToArray());
            }
            return toReturn;
        }

        /// <summary>
        /// Get the list of WorkspaceStates via LINQ
        /// </summary>
        /// <returns>list of WorkspaceStates</returns>
        [DbQuery]
        public virtual Collection<WorkspaceStateModelView> GetWorkspaceStates()
        {
            Collection<WorkspaceStateModelView> toReturn = null;
            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                IEnumerable<WorkspaceStateModelView> resultLinq = from r in gbe.WorkspaceStateLUs
                                                                  select new WorkspaceStateModelView
                                                                  {
                                                                      WorkspaceStateID = r.WorkspaceStateID,
                                                                      WorkspaceState = r.WorkspaceState
                                                                  };
                toReturn = new Collection<WorkspaceStateModelView>(resultLinq.ToArray());
            }
            return toReturn;
        }

        /// <summary>
        /// Get the collection of SikorskyLegacyResources via LINQ.
        /// </summary>
        /// <returns>Collection of SikorskyLegacyResources.</returns>
        [DbQuery]
        public Collection<SikorskyLegacyResourceDTO> GetSikorskyLegacyResources()
        {
            Collection<SikorskyLegacyResourceDTO> toReturn = null;
            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                IEnumerable<SikorskyLegacyResourceDTO> resultLinq = from r in gbe.SikorskyLegacyResources
                                                                    orderby r.ResourceID
                                                                    select new SikorskyLegacyResourceDTO
                                                                    {
                                                                        LegacyID = r.ID,
                                                                        LegacyResourceID = r.ResourceID.ToUpper(),
                                                                        LegacyResourceName = r.Name
                                                                    };
                toReturn = new Collection<SikorskyLegacyResourceDTO>(resultLinq.ToArray());
            }

            return toReturn;
        }

        /// <summary>
        /// Saves the system email.
        /// </summary>
        /// <param name="email">The email to save.</param>
        /// <exception cref="System.ArgumentNullException">email</exception>
        [DbQuery]
        public virtual void SaveSystemEmail(EmailModelDomain email)
        {
            if (email == null)
            {
                throw new ArgumentNullException(nameof(email));
            }

            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                gbe.updateSystemEmail((int)email.EmailType, email.DefaultOn, email.Forced);
            }
        }

        /// <summary>
        /// Get the list of ElementsOfCost via LINQ
        /// </summary>
        /// <returns>list of ElementOfCostTypeModelView</returns>
        [DbQuery]
        public virtual Collection<ElementOfCostTypeModelView> GetElementOfCostTypes()
        {
            Collection<ElementOfCostTypeModelView> toReturn = null;
            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                IEnumerable<ElementOfCostTypeModelView> resultLinq = from r in gbe.CostElementLUs
                                                                     where r.CostElement != "None"
                                                                     select new ElementOfCostTypeModelView
                                                                  {
                                                                      ElementOfCostId = r.CostElementID,
                                                                      ElementOfCostName = r.CostElement
                                                                  };
                toReturn = new Collection<ElementOfCostTypeModelView>(resultLinq.ToArray());
            }
            return toReturn;
        }

        /// <summary>
        /// Gets a list of selected contract types for a particular Workspace
        /// </summary>
        /// <param name="inWorkspaceId">the Workspace ID</param>
        /// <returns>A collection of contract types</returns>
        public virtual ICollection<PickListDto> GetSelectedContractTypes(int inWorkspaceId)
        {
            ICollection<PickListDto> toReturn = null;

            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                IEnumerable<PickListDto> resultsLinq = from p in gbe.WorkspaceContractTypeXREFs
                                                                 join c in gbe.ContractTypeLUs on p.ContractTypeID equals c.ContractTypeID
                                                                 where p.WorkspaceID == inWorkspaceId
                                                                 select new PickListDto
                                                                 {
                                                                     Id = c.ContractTypeID,
                                                                     Text = c.ContractType
                                                                 };
                toReturn = resultsLinq.ToList();
            }
            return toReturn;
        }


        /// <summary>
        /// Get the list of ProposalStates via LINQ
        /// </summary>
        /// <returns>list of ProposalStateTypes</returns>
        [DbQuery]
        public virtual Collection<ProposalStatusTypeModelView> GetProposalStatusTypes()
        {
            Collection<ProposalStatusTypeModelView> toReturn = null;
            
            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                
                IEnumerable<ProposalStatusTypeModelView> resultLinq = from r in gbe.ProposalStatusLUs
                                                            select new ProposalStatusTypeModelView
                                                            {
                                                                ProposalStateID = r.ProposalStatusID,
                                                                ProposalStateType = r.ProposalStatus
                                                            };
                toReturn = new Collection<ProposalStatusTypeModelView>(resultLinq.ToArray());
            }

            return toReturn;
        }

        /// <summary>
        /// Get the list of BOEStates via LINQ
        /// </summary>
        /// <returns>list of BOEStates</returns>
        [DbQuery]
        public virtual Collection<BOEStateModelView> GetBOEStates()
        {
            Collection<BOEStateModelView> toReturn = null;
            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                IEnumerable<BOEStateModelView> resultLinq = from r in gbe.BOEStateLUs
                                                            select new BOEStateModelView
                                                            {
                                                                BOEStateID = r.BOEStateID,
                                                                BOEState = r.BOEState
                                                            };
                toReturn = new Collection<BOEStateModelView>(resultLinq.ToArray());
            }
            return toReturn;
        }

        /// <summary>
        /// Get the MOQ Types via LINQ
        /// </summary>
        /// <returns>List of MOQ Types</returns>
        [DbQuery]
        public virtual Collection<MOQTypeModelView> GetMOQTypes()
        {
            Collection<MOQTypeModelView> toReturn = null;

            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                IEnumerable<MOQTypeModelView> resultLinq = from m in gbe.MOQTypeLUs
                                                           select new MOQTypeModelView
                                                           {
                                                               MOQTypeID = m.MOQTypeID,
                                                               MOQTypeName = m.MOQType
                                                           };
                toReturn = new Collection<MOQTypeModelView>(resultLinq.ToArray());
            }

            return toReturn;
        }

        /// <summary>
        /// GetMSTTravelModes
        /// </summary>
        /// <returns>List of usages</returns>
        [DbQuery]
        public virtual Collection<EnumTypeModelView> GetMSTTravelModes()
        {
            Collection<EnumTypeModelView> toReturn = null;

            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                IEnumerable<EnumTypeModelView> resultLinq = from m in gbe.MSTTravelModeLUs
                                                            select new EnumTypeModelView
                                                            {
                                                                EnumTypeID = m.MSTTravelModeID,
                                                                EnumTypeName = m.MSTTravelMode
                                                            };
                toReturn = new Collection<EnumTypeModelView>(resultLinq.OrderBy(x => x.EnumTypeID).ToArray());
            }

            return toReturn;
        }

        /// <summary>
        /// Get the emails for the system, this includes subject and body
        /// </summary>
        /// <returns>The email objects</returns>
        [DbQuery]
        public virtual Collection<EmailModelDomain> GetEmails()
        {
            Collection<EmailModelDomain> toReturn = null;
            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                IEnumerable<EmailModelDomain> resultLinq = from m in gbe.EmailLUs
                                                           select new EmailModelDomain
                                                           {
                                                               EmailType = (EmailTypes)m.EmailID,
                                                               Body = m.Body,
                                                               Subject = m.Subject,
                                                               DefaultOn = m.DefaultOn,
                                                               Forced = m.ForcedOn ?? false,
                                                               Recipient = m.Recipient,
                                                               Category = m.Category,
                                                               Trigger = m.Trigger
                                                           };
                toReturn = new Collection<EmailModelDomain>(resultLinq.ToArray());
            }

            return toReturn;
        }

        /// <summary>
        /// Get a list of Reports
        /// </summary>
        /// <returns>A collection of reports</returns>
        [DbQuery]
        public virtual Collection<ReportDTO> GetReports()
        {
            Collection<ReportDTO> toReturn = null;

            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                // Grab the reports from the DB
                IEnumerable<ReportDTO> resultsLinq = (from r in gbe.ReportLUs
                                                     select new ReportDTO
                                                     {
                                                         ReportID = r.ReportID,
                                                         ReportName = r.ReportName,
                                                         Description = r.Description
                                                      }).ToList();

                // If there are results, we'll add them to the return collection after setting report
                // types explicitly
                if (resultsLinq.Any())
                {
                    toReturn = new Collection<ReportDTO>();

                    foreach (ReportDTO report in resultsLinq)
                    {
                        // Set the report type explicitly
                        switch (report.ReportID)
                        {
                            case (int)Reports.BOEStatus:
                            case (int)Reports.BOEActivity:
                            case (int)Reports.WorkspaceActivity:
                            case (int)Reports.BoeDiscrepancy:
                            case (int)Reports.ValidateAllBOE:
                                report.ReportType = ReportType.View;
                                break;

                            case (int)Reports.CategoryClinSummary:
                            case (int)Reports.ClinCategorySummary:
                            case (int)Reports.ProjectClinCostSummary:
                            case (int)Reports.CostByClinResActYr:
                            case (int)Reports.CostByClinActYr:
                            case (int)Reports.BOESummaryReport:
                            case (int)Reports.ByPricingCode:
                            case (int)Reports.ByCatPricingCode:
                            case (int)Reports.OffloadCostByYear:
                            case (int)Reports.OffloadDetailedReport:
                            case (int)Reports.OffloadCostSummary:
                            case (int)Reports.StaffingCurves:
                            case (int)Reports.Rps:
                            case (int)Reports.Prp:
                            case (int)Reports.Ram:
                            case (int)Reports.PreVsPostOffloadTotals:
                            case (int)Reports.CostByClinResActYrFlat:
                                report.ReportType = ReportType.SSRS;
                                break;

                            case (int)Reports.AllBOEs:
                            case (int)Reports.StandardReports:
                            case (int)Reports.WbsBoeReport:
                                report.ReportType = ReportType.Export;
                                break;
                        }

                        // Add the report to the return collection
                        toReturn.Add(report);
                    }
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Get the Field Types via LINQ
        /// </summary>
        /// <returns>List of Field Types</returns>
        [DbQuery]
        public virtual Collection<FieldTypeModelView> GetFieldTypes()
        {
            Collection<FieldTypeModelView> toReturn = null;

            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                IEnumerable<FieldTypeModelView> resultLinq = from f in gbe.FieldLUs
                                                             select new FieldTypeModelView
                                                             {
                                                                 FieldTypeID = f.FieldID,
                                                                 FieldTypeName = f.FieldName
                                                             };
                toReturn = new Collection<FieldTypeModelView>(resultLinq.ToArray());
            }

            return toReturn;
        }

        /// Get the Resource Name string value given the ID
        /// </summary>
        /// <param name="inResourceID">the Resource ID</param>
        /// <returns>Resource name</returns>
        [DbQuery]
        public virtual string getResourceName(int inResourceID)
        {
            string toReturn = string.Empty;
            using (GenBoeEntities gbe = new GenBoeEntities())
            {
				string resultLinq = (from l in gbe.Resources
                                  where l.ResourceID == inResourceID
                                  select l.ResourceName).First();
                toReturn = resultLinq;
            }
            return toReturn;
        }

        /// <summary>
        /// Get all ProPricer Fields
        /// </summary>
        /// <returns>List of fields</returns>
        [DbQuery]
        public virtual Collection<EnumTypeModelView> GetProPricerFields(bool isProjectMapType)
        {
            Collection<EnumTypeModelView> toReturn;
            
            //Return Custom ProjectMap ProPricer Fields if workspace is project map type
            if (isProjectMapType)
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    IEnumerable<EnumTypeModelView> resultLinq = from p in gbe.ProPricerFieldLUs.Where(x => x.ProPricerCompanyID == 4)
                                                                select new EnumTypeModelView
                                                                {
                                                                    EnumTypeID = p.ProPricerFieldID,
                                                                    EnumTypeName = p.ProPricerField
                                                                };
                    toReturn = new Collection<EnumTypeModelView>(resultLinq.ToArray());
                }
            }
            //Non-project map type fields
            else
            {
                int companyID = (int)SystemConfiguration.Instance().CompanyMode;

                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    IEnumerable<EnumTypeModelView> resultLinq = from p in gbe.ProPricerFieldLUs.Where(x => x.ProPricerCompanyID == companyID || x.ProPricerCompanyID == 0)
                                                                select new EnumTypeModelView
                                                                {
                                                                    EnumTypeID = p.ProPricerFieldID,
                                                                    EnumTypeName = p.ProPricerField
                                                                };
                    toReturn = new Collection<EnumTypeModelView>(resultLinq.ToArray());
                }
            }
            return toReturn;
        }


        /// <summary>
        /// Get segment types
        /// </summary>
        /// <returns></returns>
        [DbQuery]
        public virtual Collection<SegmentTypeModelView> GetSegmentTypes()
        {
            Collection<SegmentTypeModelView> toReturn = null;

            using (GenBoeEntities gbe = new GenBoeEntities())
            {
      
                IEnumerable<SegmentTypeModelView> resultLinq = from s in gbe.SegmentLUs
                                                            select new SegmentTypeModelView
                                                            {
                                                                SegmentTypeID = s.SegmentID,
                                                                SegmentTypeName = s.Segment
                                                            };
                toReturn = new Collection<SegmentTypeModelView>(resultLinq.ToArray());
            }

            return toReturn;
        }

        /// <summary>
        /// Get rate types
        /// </summary>
        /// <returns>Collection of rate types</returns>
        public virtual Collection<RateTypeModelView> GetRateTypes()
        {
            Collection<RateTypeModelView> toReturn = null;

            using (GenBoeEntities gbe = new GenBoeEntities())
            {

                IEnumerable<RateTypeModelView> resultLinq =
                    from s in gbe.RateTypeLUs
                    select new RateTypeModelView
                    {
                        RateTypeID = s.RateTypeID,
                        RateTypeName = s.RateType
                    };
                
                toReturn = new Collection<RateTypeModelView>(resultLinq.ToArray());
            }

            return toReturn;
        }

        /// <summary>
        /// get the other direct cost (ODC) Spread Curves
        /// </summary>
        /// <returns></returns>
        [DbQuery]
        public virtual Collection<OtherDirectCostSpreadCurveModelView> GetODCSpreadCurve()
        {
            Collection<OtherDirectCostSpreadCurveModelView> toReturn = null;
            using (GenBoeEntities gbe = new GenBoeEntities())
            {

                IEnumerable<OtherDirectCostSpreadCurveModelView> resultLinq = from s in gbe.SpreadCurveLUs
                                                               where s.SpreadCurveID == (int) SpreadCurves.DiscreteCost ||
                                                               s.SpreadCurveID == (int) SpreadCurves.Load ||
                                                               s.SpreadCurveID == (int) SpreadCurves.Level
                                                                              select new OtherDirectCostSpreadCurveModelView
                                                               {
                                                                   SpreadCurveID = (SpreadCurves)s.SpreadCurveID,
                                                                   SpreadCurveName = s.SpreadCurve
                                                               };
                toReturn = new Collection<OtherDirectCostSpreadCurveModelView>(resultLinq.ToArray());
            }
            return toReturn;
        }

        /// <summary>
        /// Get Sum Variable Resource types
        /// </summary>
        /// <returns>collection of sum variable resource types</returns>
        [DbQuery]
        public virtual Collection<SumVariableResourceTypeModelView> GetSumVariableResourceTypes()
        {
            Collection<SumVariableResourceTypeModelView> toReturn = null;

            using (GenBoeEntities gbe = new GenBoeEntities())
            {

                IEnumerable<SumVariableResourceTypeModelView> resultLinq = from s in gbe.SumVariableResourceTypeLUs
                                                             select new SumVariableResourceTypeModelView
                                                             {
                                                                 SumVariableResourceTypeID = s.SumVariableResourceTypeID,
                                                                 SumVariableResourceTypeName = s.SumVariableResourceType
                                                             };
                toReturn = new Collection<SumVariableResourceTypeModelView>(resultLinq.ToArray());
            }

            return toReturn;
        }
    }
}
