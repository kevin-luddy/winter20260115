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
    using System.Linq.Expressions;
    using System.Transactions;
    using IES.Common;
    using GenBOE.MSTMetricModel;
    using GenBOE.Models;
    using GenBOE.Dtos;

    /// <summary>
    /// MST Metric Data Loader Class.
    /// </summary>
    public class MSTMetricLoader : IMSTMetricLoader
    {
        /// <summary>
        /// Public Constructor
        /// </summary>
        public MSTMetricLoader()
        {
        }

        /// <summary>
        /// Gets search criteria for searchin MST metrics.
        /// </summary>
        /// <returns>Metric search criteria.</returns>
        [DbQuery(4)]
        public MSTMetricSearchCriteriaDTO GetMSTMetricSearchCriteria()
        {
            MSTMetricSearchCriteriaDTO toReturn = new MSTMetricSearchCriteriaDTO();

            using (BI_Datamart_PMMEntities mstEntities = new BI_Datamart_PMMEntities())
            {
                toReturn.Programs = (from b in mstEntities.vw_BOE_Data
                                     select new Program
                                     {
                                         Id = b.PMM_Program_ID,
                                         ProgramName = b.Program_Name
                                     }).Distinct().OrderBy(p => p.ProgramName).ToCollection<Program>();

                toReturn.MeasureFunctions = (from b in mstEntities.vw_BOE_Data
                                             select new MeasureFunction
                                             {
                                                 Id = b.Function_Acronym,
                                                 MeasureFunctionName = b.Function_Description
                                             }).Distinct().OrderBy(m => m.MeasureFunctionName).ToCollection<MeasureFunction>();

                toReturn.MeasureQualifiers = (from b in mstEntities.vw_BOE_Data
                                              select new MeasureQualifier
                                              {
                                                  Id = b.Measure_Qualifier,
                                                  MeasureQualifierDescription = b.Measure_Qualifier
                                              }).Where(m => m.Id != null).Distinct().OrderBy(m => m.MeasureQualifierDescription).ToCollection<MeasureQualifier>();

                toReturn.MeasureNames = (from m in mstEntities.vw_BOE_Definition
                                         select new Measure
                                         {
                                             Id = m.BOE_Definition_ID,
                                             MeasureName = m.Measure_Name
                                         }).Distinct().OrderBy(m => m.MeasureName).ToCollection<Measure>();

                // Get a distinct set objects by the MeasureName.
                toReturn.MeasureNames = toReturn.MeasureNames.GroupBy(m => m.MeasureName).Select(m => m.First()).ToCollection<Measure>();

                toReturn.DataSources = (from b in mstEntities.vw_BOE_Data
                                        select new DataSource
                                        {
                                            Id = b.Data_Source_ID,
                                            DataSourceName = b.Data_Source
                                        }).Distinct().OrderBy(d => d.DataSourceName).ToCollection<DataSource>();

                return toReturn;
            }
        }

        /// <summary>
        /// Searches MST metrics using the given search criteria.
        /// </summary>
        /// <param name="searchDTO">Search criteria.</param>
        /// <returns>Search result details for the given criteria.</returns>
        public ICollection<MSTMetricDetailsDTO> SearchMSTMetrics(MSTMetricSearchDTO searchDTO)
        {
            ICollection<MSTMetricDetailsDTO> searchResults = new Collection<MSTMetricDetailsDTO>();

            Expression<Func<MSTMetricDetailsDTO, bool>> programIdFilter;
            Expression<Func<MSTMetricDetailsDTO, bool>> measureIdFilter;
            Expression<Func<MSTMetricDetailsDTO, bool>> measureQualifierIdFilter;
            Expression<Func<MSTMetricDetailsDTO, bool>> measureFunctionIdFilter;
            Expression<Func<MSTMetricDetailsDTO, bool>> dataSourceIdFilter;
            Expression<Func<MSTMetricDetailsDTO, bool>> searchStringFilter;

            // Search on Program Id. A select value of 0 is select all.
            if (searchDTO.ProgramId != null && searchDTO.ProgramId.Value != 0)
            {
                programIdFilter = p => p.ProgramId == searchDTO.ProgramId;
            }
            else
            {
                programIdFilter = p => true; // return all if filter not set
            }

            // Search on Measure Name Id.  A select value of 0 is select all.
            if (searchDTO.MeasureId != null && searchDTO.MeasureId.Value != 0)
            {
                measureIdFilter = m => m.MeasureId == searchDTO.MeasureId;
            }
            else
            {
                measureIdFilter = m => true; // return all if filter not set
            }

            // Search on Measure Qualifier Id.  A select value of 0 is select all.
            if (!string.IsNullOrEmpty(searchDTO.MeasureQualifierId) && searchDTO.MeasureQualifierId != "ALL")
            {
                measureQualifierIdFilter = mq => mq.MeasureQualifierId == searchDTO.MeasureQualifierId;
            }
            else
            {
                measureQualifierIdFilter = mq => true; // return all if filter not set or ALL
            }

            // Search on measure function Id. A null or empty function Id is select all.
            if (!string.IsNullOrEmpty(searchDTO.MeasureFunctionId) && searchDTO.MeasureFunctionId != "All")
            {
                measureFunctionIdFilter = mf => mf.MeasureFunctionId == searchDTO.MeasureFunctionId;
            }
            else
            {
                measureFunctionIdFilter = mf => true; // return all if filter not set or ALL
            }

            // Search on Data Source Id.  A select value of 0 is select all.
            if (searchDTO.DataSourceId != null && searchDTO.DataSourceId.Value != 0)
            {
                dataSourceIdFilter = ds => ds.DataSourceId == searchDTO.DataSourceId;
            }
            else
            {
                dataSourceIdFilter = ds => true; // return all if filter not set
            }

            // OR all searchable fields containing the search string. A string value of "ALL" is select all.
            if (!string.IsNullOrEmpty(searchDTO.SearchFor) && searchDTO.SearchFor != "All")
            {
                searchStringFilter = s => (s.Comment != null && s.Comment.Contains(searchDTO.SearchFor)) ||
                                                         (s.MeasureDescription != null && s.MeasureDescription.Contains(searchDTO.SearchFor)) ||
                                                         (s.MeasureQualifierId != null && s.MeasureQualifierId.Contains(searchDTO.SearchFor)) ||
                                                         (s.ProgramDescription != null && s.ProgramDescription.Contains(searchDTO.SearchFor));
            }
            else
            {
                searchStringFilter = s => true; // return all if filter not set or All
            }

            using (BI_Datamart_PMMEntities mstEntities = new BI_Datamart_PMMEntities())
            {
                // Get everything and narrow the list down by search criteria;
                searchResults = (from b in mstEntities.vw_BOE_Data
                                 join m in mstEntities.vw_BOE_Definition on b.Measure_Name equals m.Measure_Name
                                 select new MSTMetricDetailsDTO
                                 {
                                     BaseMeasure1Data = b.BaseMeas1_Data,
                                     BaseMeasure1Name = m.BaseMeas1_Name,
                                     BaseMeasure2Data = b.BaseMeas2_Data,
                                     BaseMeasure2Name = m.BaseMeas2_Name,
                                     BaseMeasure3Data = b.BaseMeas3_Data,
                                     BaseMeasure3Name = m.BaseMeas3_Name,
                                     BaseMeasure4Data = b.BaseMeas4_Data,
                                     BaseMeasure4Name = m.BaseMeas4_Name,
                                     BaseMeasure5Data = b.BaseMeas5_Data,
                                     BaseMeasure5Name = m.BaseMeas5_Name,
                                     Comment = b.Comment,
                                     ContractNumber = b.Contract_Number,
                                     EndDate = b.Measure_End_Date,
                                     Equation = m.Equation,
                                     Id = b.BOE_Data_ID,
                                     MeasureData = b.Measure_Data,
                                     MeasureDescription = m.Meas_Description,
                                     MeasureFunction = b.Function_Description,
                                     MeasureFunctionId = b.Function_Acronym,
                                     MeasureName = b.Measure_Name,
                                     MeasureId = m.BOE_Definition_ID,
                                     DataSourceId = b.Data_Source_ID,
                                     DataSource = b.Data_Source,
                                     ProgramName = b.Program_Name,
                                     ProgramId = b.PMM_Program_ID,
                                     ProgramDescription = b.Program_Description,
                                     ScopeName = b.Scope_Name,
                                     StartDate = b.Measure_Start_Date,
                                     WorkPackages = b.Work_Packages,
                                     MeasureQualifier = b.Measure_Qualifier,
                                     MeasureQualifierId = b.Measure_Qualifier
                                 }).Where(programIdFilter)
                                 .Where(measureIdFilter)
                                 .Where(measureQualifierIdFilter)
                                 .Where(measureFunctionIdFilter)
                                 .Where(dataSourceIdFilter)
                                 .Where(searchStringFilter)
                                 .OrderBy(m => m.MeasureName)
                                 .Take(1000) // limit to first 1000 results
                                 .ToCollection<MSTMetricDetailsDTO>();
            }

            return searchResults;
        }

        /// <summary>
        /// Gets a list of metric details from the PMM database for the given ids. NOTE: This data comes from PMM not the genBoe database.
        /// </summary>
        /// <param name="ids">Metric Ids.</param>
        /// <returns>Metric details for the given list of Ids.</returns>
        public ICollection<MSTMetricDetailsDTO> GetByIds(ICollection<int> ids)
        {
            ICollection<MSTMetricDetailsDTO> results = new Collection<MSTMetricDetailsDTO>();

            if (ids.Any())
            {
                // Create a new transaction with isolation level ReadUncommitted.  Should not be a problem because this is just a query of a view. This 
                // is required because an exception is thrown in the scope of a CopyBoe transaction because it thinks the MST PPM DB being queried has 
                // changed in the scope of the outer transaction.
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.RequiresNew, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
                {
                    using (BI_Datamart_PMMEntities mstEntities = new BI_Datamart_PMMEntities())
                    {
                        results = (from b in mstEntities.vw_BOE_Data
                                   join m in mstEntities.vw_BOE_Definition on b.Measure_Name equals m.Measure_Name
                                   where ids.Contains(b.BOE_Data_ID)
                                   select new MSTMetricDetailsDTO
                                   {
                                       BaseMeasure1Data = b.BaseMeas1_Data,
                                       BaseMeasure1Name = m.BaseMeas1_Name,
                                       BaseMeasure2Data = b.BaseMeas2_Data,
                                       BaseMeasure2Name = m.BaseMeas2_Name,
                                       BaseMeasure3Data = b.BaseMeas3_Data,
                                       BaseMeasure3Name = m.BaseMeas3_Name,
                                       BaseMeasure4Data = b.BaseMeas4_Data,
                                       BaseMeasure4Name = m.BaseMeas4_Name,
                                       BaseMeasure5Data = b.BaseMeas5_Data,
                                       BaseMeasure5Name = m.BaseMeas5_Name,
                                       Comment = b.Comment,
                                       ContractNumber = b.Contract_Number,
                                       EndDate = b.Measure_End_Date,
                                       Equation = m.Equation,
                                       Id = b.BOE_Data_ID,
                                       PMMMeasureId = b.BOE_Data_ID,
                                       MeasureQualifier = b.Measure_Qualifier,
                                       MeasureData = b.Measure_Data,
                                       MeasureFunction = b.Function_Description,
                                       MeasureFunctionId = b.Function_Acronym,
                                       MeasureName = b.Measure_Name,
                                       MeasureId = m.BOE_Definition_ID,
                                       DataSourceId = b.Data_Source_ID,
                                       DataSource = b.Data_Source,
                                       ProgramName = b.Program_Name,
                                       ProgramId = b.PMM_Program_ID,
                                       ScopeName = b.Scope_Name,
                                       StartDate = b.Measure_Start_Date,
                                       WorkPackages = b.Work_Packages,
                                       BusinessArea = b.Business_Area,
                                       LineOfBusiness = b.LOB,
                                       MeasureCategoryName = m.Meas_Category,
                                       MeasureDescription = m.Meas_Description,
                                       MeasureGroupName = m.Meas_Group,
                                       MeasureLink = b.URL,
                                       MeasureValidationDate = b.Validated_Date,
                                       MeasureValidatedBy = b.Validated_By,
                                       ProgramDescription = b.Program_Description
                                   }).ToCollection<MSTMetricDetailsDTO>();
                    }
                }


                if (results.Any())
                {
                    string detailHelpLink = ConfigurationUtilities.GetAppSetting("MSTSearchResultsDetailHelpLink", string.Empty);
                    results.ToList<MSTMetricDetailsDTO>().ForEach(l => l.MSTMetricDetailHelpLink = detailHelpLink);
                }
            }
            return results;
        }

        /// <summary>
        /// Gets a list of metric details saved to the genBOE database.
        /// </summary>
        /// <param name="ids">Metric Ids</param>
        /// <returns>Metric details for the given list of Ids.</returns>
        public ICollection<MSTMetricDetailsDTO> GetMetricDetailsByIds(ICollection<int> ids)
        {
            ICollection<MSTMetricDetailsDTO> results = new Collection<MSTMetricDetailsDTO>();

            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                results = (from m in gbe.MetricDetails
                           where ids.Contains(m.MetricDetailID)
                           select new MSTMetricDetailsDTO
                           {
                                BaseMeasure1Data = m.BaseMeasure1Data,
                                BaseMeasure1Name = m.BaseMeasure1Name,
                                BaseMeasure2Data = m.BaseMeasure2Data,
                                BaseMeasure2Name = m.BaseMeasure2Name,
                                BaseMeasure3Data = m.BaseMeasure3Data,
                                BaseMeasure3Name = m.BaseMeasure3Name,
                                BaseMeasure4Data = m.BaseMeasure4Data,
                                BaseMeasure4Name = m.BaseMeasure4Name,
                                BaseMeasure5Data = m.BaseMeasure5Data,
                                BaseMeasure5Name = m.BaseMeasure5Name,
                                Comment = m.Comment,
                                ContractNumber = m.ContractNumber,
                                EndDate = m.EndDate,
                                Equation = m.Equation,
                                Id = m.MetricDetailID,
                                PMMMeasureId = m.SourceSystemID,
                                MeasureQualifier = m.MeasureQualifier,
                                MeasureData = m.MeasureData,
                                MeasureFunction = m.MeasureFunction,
                                MeasureFunctionId = m.MeasureFunctionID,
                                MeasureName = m.MeasureName,
                                MeasureId = m.MeasureID,
                                DataSourceId = m.DataSourceID,
                                DataSource = m.DataSource,
                                ProgramName = m.ProgramName,
                                ProgramId = m.ProgramID,
                                ScopeName = m.ScopeName,
                                StartDate = m.StartDate,
                                WorkPackages = m.WorkPackages,
                                BusinessArea = m.BusinessArea,
                                LineOfBusiness = m.LineOfBusiness,
                                MeasureCategoryName = m.MeasureCategoryName,
                                MeasureDescription = m.MeasureDescription,
                                MeasureGroupName = m.MeasureGroupName,
                                MeasureLink = m.MeasureLink,
                                MeasureValidationDate = m.MeasureValidationDate,
                                MeasureValidatedBy = m.MeasureValidatedBy,
                                ProgramDescription = m.ProgramDescription
                           }).ToCollection<MSTMetricDetailsDTO>();
            }

            if (results.Any())
            {
                string detailHelpLink = ConfigurationUtilities.GetAppSetting("MSTSearchResultsDetailHelpLink", string.Empty);
                results.ToList<MSTMetricDetailsDTO>().ForEach(l => l.MSTMetricDetailHelpLink = detailHelpLink);
            }
            return results;
        }

        /// <summary>
        /// Gets MST PMM metrics related to task elements. NOTE: These metrics are stored in the genBoe database. They are a snapshot of the
        /// metric when it was imported by the user selecting the metric for the task element.
        /// </summary>
        /// <param name="inTaskElementIds">Task element Ids.</param>
        /// <returns>Metrics related to the given task element Ids.</returns>
        [DbQuery]
        virtual public ICollection<MSTMetricDetailsDTO> GetByTaskElementIds(ICollection<int> inTaskElementIds)
        {
            ICollection<MSTMetricDetailsDTO> toReturn = new Collection<MSTMetricDetailsDTO>();

            Collection<BOETaskElementMetricDetailXREF> metricXREFs = new Collection<BOETaskElementMetricDetailXREF>();

            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                metricXREFs = (from o in gbe.BOETaskElementMetricDetailXREFs
                               where inTaskElementIds.Contains(o.BOETaskElementID)
                               select o).ToCollection();
            }
            
            if (metricXREFs.Any())
            {
                ICollection<int> metricIds = metricXREFs.Select(m => m.MetricDetailID).ToCollection<int>();
                toReturn = this.GetMetricDetailsByIds(metricIds);
                foreach (MSTMetricDetailsDTO dto in toReturn)
                {
                    dto.DateAddedToTaskElement = metricXREFs.Where(m => m.MetricDetailID == dto.Id).Select(r => r.UpdateDT).FirstOrDefault();
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Gets a mapping of task element id to metric id mappings.
        /// </summary>
        /// <param name="inTaskElementIds">Task element Ids.</param>
        /// <returns>Collection of mappings.</returns>
        public ICollection<MetricIdTaskElementIdXrefDTO> GetTaskElementIdMeticIdMappings(ICollection<int> inTaskElementIds)
        {
            ICollection<MetricIdTaskElementIdXrefDTO> xrefs = new Collection<MetricIdTaskElementIdXrefDTO>();
            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                xrefs = (from o in gbe.BOETaskElementMetricDetailXREFs
                         where inTaskElementIds.Contains(o.BOETaskElementID)
                         select new MetricIdTaskElementIdXrefDTO
                         {
                             MetricId = o.MetricDetailID,
                             TaskElementId = o.BOETaskElementID
                         }).ToCollection<MetricIdTaskElementIdXrefDTO>();
            }

            return xrefs;
        }

        /// <summary>
        /// Saves measures to genBOE by reconciling the current measures with the given list.
        /// </summary>
        /// <param name="taskElementId">Task element id.</param>
        /// <param name="pmmMetricIds">PMM metric Ids.</param>
        public void Save(int taskElementId, ICollection<int> pmmMetricIds)
        {
            if (pmmMetricIds == null)
            {
                throw new ArgumentNullException(nameof(pmmMetricIds));
            }

            // All currently stored metrics for the task element.
            ICollection<MSTMetricDetailsDTO> currentTaskElementMetrics = this.GetByTaskElementIds(new Collection<int>() { taskElementId });
            ICollection<int> idsToAdd = new Collection<int>();
            ICollection<int> idsToDelete = new Collection<int>();

            foreach(MSTMetricDetailsDTO dto in currentTaskElementMetrics)
            {
                if (!pmmMetricIds.Contains(dto.Id))
                {
                    // This metric is no longer associated with the task element. Delete from genBOE.
                    idsToDelete.Add(dto.Id);
                }
            }

            foreach(int id in pmmMetricIds)
            {
                MSTMetricDetailsDTO currentMetric = currentTaskElementMetrics.FirstOrDefault(i => i.Id == id);
                if (currentMetric == null)
                {
                    // This is a new metric.
                    idsToAdd.Add(id);
                }
            }

            foreach(int deleteId in idsToDelete)
            {
                this.Delete(taskElementId, deleteId);
            }

            if (idsToAdd.Any())
            {
                // Get metric data from PMM
                ICollection<MSTMetricDetailsDTO> pmmMetrics = this.GetByIds(idsToAdd);

                foreach (int insertId in idsToAdd)
                {
                    MSTMetricDetailsDTO pmmMetric = pmmMetrics.FirstOrDefault(m => m.PMMMeasureId == insertId);
                    if (pmmMetric != null)
                    {
                        using (GenBoeEntities gbe = new GenBoeEntities())
                        {
                            int? newId = gbe.insertMetricDetail(-2, pmmMetric.PMMMeasureId, pmmMetric.ProgramName, pmmMetric.ScopeName, pmmMetric.MeasureData,
                                pmmMetric.MeasureName, pmmMetric.DataSource, pmmMetric.MeasureFunction, pmmMetric.Equation, pmmMetric.Comment, pmmMetric.StartDate,
                                pmmMetric.EndDate, pmmMetric.ContractNumber, pmmMetric.WorkPackages, pmmMetric.BaseMeasure1Name, pmmMetric.BaseMeasure1Data, pmmMetric.ProgramId,
                                pmmMetric.MeasureFunctionId, pmmMetric.DataSourceId, pmmMetric.MeasureId, pmmMetric.MeasureQualifier, pmmMetric.BaseMeasure2Name, pmmMetric.BaseMeasure2Data,
                                pmmMetric.BaseMeasure3Name, pmmMetric.BaseMeasure3Data, pmmMetric.BaseMeasure4Name, pmmMetric.BaseMeasure4Data, pmmMetric.BaseMeasure5Name, pmmMetric.BaseMeasure5Data,
                                pmmMetric.BusinessArea, pmmMetric.LineOfBusiness, pmmMetric.MeasureGroupName, pmmMetric.MeasureCategoryName, pmmMetric.MeasureDescription, pmmMetric.MeasureLink,
                                pmmMetric.MeasureValidationDate, pmmMetric.MeasureValidatedBy, pmmMetric.ProgramDescription).FirstOrDefault();

                            gbe.insertBOETaskElementMetricDetailXREF(-1, taskElementId, newId, DateTime.Now);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Deletes the specified task element identifier.
        /// </summary>
        /// <param name="taskElementId">The task element identifier.</param>
        /// <param name="metricId">The metric identifier.</param>
        private void Delete(int taskElementId, int metricId)
        {
            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                BOETaskElementMetricDetailXREF xref = (from o in gbe.BOETaskElementMetricDetailXREFs
                                                       where o.BOETaskElementID == taskElementId && o.MetricDetailID == metricId
                                                       select o).FirstOrDefault();
                if (xref != null)
                {
                    gbe.deleteBOETaskElementMetricDetail(xref.BTEMDID, xref.UpdateDT);
                    gbe.deleteMetricDetail(metricId);
                }
            }
        }

        /// <summary>
        /// Gets a list of metrics by boe id.
        /// </summary>
        /// <param name="inBoeIds">BOE Ids.</param>
        /// <returns>Metrics by BOE ids.</returns>
        [DbQuery]
        public ICollection<MSTMetricDetailsDTO> GetByBoeIds(ICollection<int> inBoeIds)
        {
            ICollection<MSTMetricDetailsDTO> toReturn = new Collection<MSTMetricDetailsDTO>();

            using (GenBoeEntities gbe = new GenBoeEntities())
            {
				List<int> taskElementIds = (from o in gbe.BOETaskElements
                                      where inBoeIds.Contains(o.BOEID)
                                      select o.BOETaskElementID).ToList();

                toReturn = this.GetByTaskElementIds(taskElementIds);
            }

            return toReturn;
        }
    }
}
