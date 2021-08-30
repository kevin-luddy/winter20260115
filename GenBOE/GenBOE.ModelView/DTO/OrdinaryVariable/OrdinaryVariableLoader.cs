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
    using System.Data;
    using System.Data.Entity;
    using System.Linq;
    using IES.Common;
    using GenBOE.Dtos;
    using GenBOE.Models;

    public class OrdinaryVariableLoader : BulkDataLoader<OrdinaryVariableDto, OrdinaryVariable>, IOrdinaryVariableLoader
    {
        public OrdinaryVariableLoader()
        {
            this.Log = new Logger(typeof(OrdinaryVariableLoader));
        }

        /// <summary>
        /// Gets Ordinary Variables By Task Element Ids
        /// </summary>
        /// <param name="taskElementIds">task element Ids</param>
        /// <returns>Corresponding data</returns>
        [DbQuery]
        public ICollection<OrdinaryVariableDto> GetByTaskElementIds(HashSet<int?> taskElementIds)
        {
            using (StopwatchTimer sw = new StopwatchTimer(Log))
            {
                ICollection<OrdinaryVariableDto> toReturn;

                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    toReturn = this.ConvertToDto(
                        (from x in gbe.OrdinaryVariables
                        join y in gbe.BOETaskElements on x.BOETaskElementID equals y.BOETaskElementID
                        where taskElementIds.Contains(x.BOETaskElementID)
                        select new OrdinaryVariableData
                        {
                            Variable = x,
                            BoeId = y.BOEID,
                            BoesToSum = from z in gbe.SumOfBOE_OrdinaryVariableXREF
                                        where z.OrdinaryVariableID == x.OrdinaryVariableID
                                        select new SelectBOEsToSum
                                        {
                                            // need the primary key for this table to support bulk deletes
                                            OVSumID = z.OVSumID,
                                            OrdinaryVariableID = z.OrdinaryVariableID,
                                            BoeID = z.BOEID,
                                            CLINID = z.CLINID,
                                            WBSID = z.WBSID
                                        },
                            TypesOfResourcesToSum = from a in gbe.OrdinaryVariableSumVariableResourceTypeXREFs
                                                    where a.OrdinaryVariableID == x.OrdinaryVariableID
                                                     select a.SumVariableResourceTypeID
                        }).ToList());

                }
                return toReturn;
            }
        }

        [DbQuery]
        public override ICollection<OrdinaryVariableDto> GetByIds(ICollection<int> ids)
        {
            using (StopwatchTimer sw = new StopwatchTimer(Log))
            {
                ICollection<OrdinaryVariableDto> toReturn;

                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    toReturn = this.ConvertToDto(
                        (from x in gbe.OrdinaryVariables
                        join y in gbe.BOETaskElements on x.BOETaskElementID equals y.BOETaskElementID
                        where ids.Contains(x.OrdinaryVariableID)
                        select new OrdinaryVariableData
                        {
                            Variable = x,
                            BoeId = y.BOEID,
                            BoesToSum = from z in gbe.SumOfBOE_OrdinaryVariableXREF
                                        where z.OrdinaryVariableID == x.OrdinaryVariableID
                                        select new SelectBOEsToSum {
                                            OVSumID = z.OVSumID,
                                            OrdinaryVariableID = z.OrdinaryVariableID,
                                            BoeID = z.BOEID,
                                            CLINID = z.CLINID,
                                            WBSID = z.WBSID
                                        },
                            TypesOfResourcesToSum = from a in gbe.OrdinaryVariableSumVariableResourceTypeXREFs
                                                    where a.OrdinaryVariableID == x.OrdinaryVariableID
                                                     select a.SumVariableResourceTypeID
                        }).ToList());

                }
                return toReturn;
            }
        }

        [DbQuery]
        public ICollection<int> GetIdsByTaskElementId(int taskElementId)
        {
            using (StopwatchTimer sw = new StopwatchTimer(Log))
            {
                ICollection<int> ids = new List<int>();

                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    ids = gbe.OrdinaryVariables.Where(x => x.BOETaskElementID == taskElementId).Select(x => x.OrdinaryVariableID).ToList();
                }

                return ids;
            }
        }

        /// <summary>
        /// Bulk Saves the Ordinary Variables and associated SumOfBOEs if the XXXType is SumOfBOEs
        /// </summary>
        /// <param name="dtosToSave"></param>
        /// <returns></returns>
        public override IDictionary<int, int> BulkSave(ICollection<OrdinaryVariableDto> dtosToSave)
        {
            if (dtosToSave == null)
            {
                throw new ArgumentNullException(nameof(dtosToSave));
            }

            Collection<OrdinaryVariableDto> saveableDtos = (from dto in dtosToSave 
                                                              where dto.Updateable != UpdateType.None
                                                              select dto).ToCollection();

            IDictionary<int, int> toReturn = new Dictionary<int, int>();

            if (dtosToSave.Any(d => d.Updateable != UpdateType.None))
            {
                // Any data associated with deleted ordinary variables will be handled by the bulk delelte, but 
                // we need to focus on the upsertable ones to update referential tables for boes to sum and resource types of those boes. 
                Collection<OrdinaryVariableDto> upsertableDtosOnly = (from dto in saveableDtos
                                                                      where dto.Updateable == UpdateType.Upsert
                                                                      select dto).ToCollection();

                // bulk save the ordinary variables
                toReturn = base.BulkSave(saveableDtos);

                using (GenBoeEntities gbe = new GenBoeEntities())
                {

                    // NOTE: generally, the following code to delete/insert SumOfBOEs would normally
                    //       go into it's own data loader. However, the primary key is a long which doesn't match the 
                    //       DataLoader pattern of an int for the id. Thus we sadly have to leave this code here.
                    Collection<string> sumOfBOEPropertiesToIncludeInTable = new Collection<string>()
                    {
                        "OVSumID", "OrdinaryVariableID", "CLINID", "WBSID", "BOEID"
                    };

                    #region Bulk Delete SumOfBoes
                    // delete all SumOfBOEs associated with the newly upserted dtos whose value type is SumOfBOEs
                    Collection<long> deleteableBoesToSumDtosIds = upsertableDtosOnly.Where(d => d.ValueType == VarValueType.SumOfBOEs).SelectMany(d => d.SelectedBOEsToSum).Where(s => s.Updateable == UpdateType.Deleted).Select(s => s.OVSumID).Distinct().ToCollection();

                    if (deleteableBoesToSumDtosIds.Any())
                    {
                        // create entities to delete - the only value that is used by the sp is the OVSumID
                        Collection<SumOfBOE_OrdinaryVariableXREF> sumOfBoeEntities = new Collection<SumOfBOE_OrdinaryVariableXREF>();
                        foreach (long anOrdinaryVariableID in deleteableBoesToSumDtosIds)
                        {
                            SumOfBOE_OrdinaryVariableXREF anEntity = new SumOfBOE_OrdinaryVariableXREF()
                            {
                                CLINID = null,
                                WBSID = null,
                                BOEID = null,
                                OrdinaryVariableID = 0, // doesn't matter what value since the sp only uses OVSumID
                                OVSumID = anOrdinaryVariableID
                            };
                            sumOfBoeEntities.Add(anEntity);
                        }

                        // convert the entities to DataTable
                        DataTable sumOfBoeDataTable = StoredProcedureHelper.ToDataTable<SumOfBOE_OrdinaryVariableXREF>(sumOfBoeEntities, sumOfBOEPropertiesToIncludeInTable);

                        // Call Execute to bulk delete the entities
                        StoredProcedureHelper.ExecuteTableValueProcedure(
                            gbe,
                            sumOfBoeDataTable,
                            "deleteSumOfBOEByOrdinaryVariableIDviaTableParameter",
                            "@SumOfBOE_OrdinaryVariableXREF",
                            "TT_SumOfBOE_OrdinaryVariableXREF",
                            false);
                    }
                    #endregion  Bulk Delete SumOfBoes

                    #region Bulk Insert SumOfBoe

                    // now insert the new SumOfBoes
                    Collection<SelectBOEsToSum> insertableBoesToSumDtos = upsertableDtosOnly.Where(d => d.ValueType == VarValueType.SumOfBOEs).SelectMany(d => d.SelectedBOEsToSum).Where(s => s.Updateable != UpdateType.Deleted).ToCollection();

                    if (insertableBoesToSumDtos.Any())
                    {
                        Collection<SumOfBOE_OrdinaryVariableXREF> sumOfBoeEntitiesToInsert = new Collection<SumOfBOE_OrdinaryVariableXREF>();
                        foreach (SelectBOEsToSum aSumOfBoe in insertableBoesToSumDtos)
                        {
                            SumOfBOE_OrdinaryVariableXREF anEntity = new SumOfBOE_OrdinaryVariableXREF()
                            {
                                CLINID = aSumOfBoe.CLINID,
                                WBSID = aSumOfBoe.WBSID,
                                BOEID = aSumOfBoe.BoeID,
                                OrdinaryVariableID = aSumOfBoe.OrdinaryVariableID.Value, // TODO - fix logic to account for when ordinary variable id isn't set
                                OVSumID = aSumOfBoe.OVSumID
                            };
                            sumOfBoeEntitiesToInsert.Add(anEntity);
                        }

                        DataTable sumOfBoesToInsertDataTable = StoredProcedureHelper.ToDataTable<SumOfBOE_OrdinaryVariableXREF>(sumOfBoeEntitiesToInsert, sumOfBOEPropertiesToIncludeInTable);

                        // Call Execute to bulk insert the entities
                        StoredProcedureHelper.ExecuteTableValueProcedure(
                            gbe,
                            sumOfBoesToInsertDataTable,
                            "insertSumOfBOE_OrdinaryVariableviaTableParameter",
                            "@SumOfBOE_OrdinaryVariableXREF",
                            "TT_SumOfBOE_OrdinaryVariableXREF",
                            false);
                    }
                    #endregion  Bulk Insert SumOfBoes

                    #region Bulk Delete SumVariableResourceType

                    Collection<string> sumPropertiesToIncludeInTable = new Collection<string>()
                    {
                        "OVSVRTID", "OrdinaryVariableID", "SumVariableResourceTypeID"
                    };

                    // delete all SumVariableResourceTypes associated with the newly upserted dtos
                    Collection<OrdinaryVariableSumVariableResourceTypeXREF> deleteableSumVariableEntities =
                        (from upsertDto in upsertableDtosOnly
                         from sumVariableResourceID in upsertDto.SumVariableResourceTypeIDs
                         select new OrdinaryVariableSumVariableResourceTypeXREF()
                         {
                             OVSVRTID = 0, // stored procedure deletes by OrdinaryVariableID so this value doesn't matter
                             SumVariableResourceTypeID = sumVariableResourceID,
                             OrdinaryVariableID = upsertDto.Id
                         }).ToCollection();

                    if (deleteableSumVariableEntities.Any())
                    {
                        // convert the entities to DataTable
                        DataTable sumDataTableToDelete = StoredProcedureHelper.ToDataTable<OrdinaryVariableSumVariableResourceTypeXREF>(deleteableSumVariableEntities, sumPropertiesToIncludeInTable);

                        // Call Execute to bulk delete the entities
                        StoredProcedureHelper.ExecuteTableValueProcedure(
                            gbe as DbContext,
                            sumDataTableToDelete,
                            "deleteOrdinaryVariableResourceTypeviaTableParameter",
                            "@SumOfBOE_OrdinaryVariableXREF",
                            "TT_OrdinaryVariableSumVariableResourceTypeXREF",
                            false);
                    }
                    #endregion  Bulk Delete SumVariableResourceType

                    #region Bulk Insert SumVariableResourceType
                    // now insert the new SumOfBoes
                    Collection<OrdinaryVariableSumVariableResourceTypeXREF> insertableSumVariableEntities =
                        (from upsertDto in upsertableDtosOnly
                         from sumVariableResourceID in upsertDto.SumVariableResourceTypeIDs
                         select new OrdinaryVariableSumVariableResourceTypeXREF()
                         {
                             OVSVRTID = -1,
                             SumVariableResourceTypeID = sumVariableResourceID,
                             OrdinaryVariableID = upsertDto.Id
                         }).ToCollection();

                    if (insertableSumVariableEntities.Any())
                    {
                        // convert the entities to DataTable
                        DataTable sumDataTableToInsert = StoredProcedureHelper.ToDataTable<OrdinaryVariableSumVariableResourceTypeXREF>(insertableSumVariableEntities, sumPropertiesToIncludeInTable);

                        // Call Execute to bulk insert the entities
                        StoredProcedureHelper.ExecuteTableValueProcedure(
                            gbe as DbContext,
                            sumDataTableToInsert,
                            "insertOrdinaryVariableResourceTypeviaTableParameter",
                            "@SumOfBOE_OrdinaryVariableXREF",
                            "TT_OrdinaryVariableSumVariableResourceTypeXREF",
                            false);
                    }
                    #endregion Bulk Insert SumVariableResourceType
                }
            }// end if Any

            return toReturn;
        }

        protected override int? Delete(OrdinaryVariableDto dtoToDelete)
        {
            using (StopwatchTimer sw = new StopwatchTimer(Log))
            {
                int? toReturn = null;
            
                if (dtoToDelete != null)
                {
                    using (GenBoeEntities gbe = new GenBoeEntities())
                    {
                        
                        if (dtoToDelete.ValueType.Equals(VarValueType.SumOfBOEs))
                        {
                            gbe.deleteSumOfBOEByOrdinaryVariableID(dtoToDelete.Id);
                        }
                    
                        gbe.deleteBOETaskElementOrdinaryVariable(
                            dtoToDelete.Id,
                            dtoToDelete.UpdateDate);
                    }

                    toReturn = dtoToDelete.Id;
                }
                return toReturn;
            }
        }

        protected override int? Upsert(OrdinaryVariableDto dtoToUpsert)
        {
            using (StopwatchTimer sw = new StopwatchTimer(Log))
            {
                int ordinaryVariableID = 0;

                if (dtoToUpsert != null)
                {
                    using (GenBoeEntities gbe = new GenBoeEntities())
                    {
                        ordinaryVariableID = gbe.upsertBOETaskElementOrdinaryVariable(
                            dtoToUpsert.Id,
                            dtoToUpsert.OrdinaryVariableName,
                            dtoToUpsert.OrdinaryVariableValue,
                            (int)dtoToUpsert.SortBOEBy,
                            (int)dtoToUpsert.ValueType,
                            dtoToUpsert.TaskElementId,
                            dtoToUpsert.IsPercentage,
                            dtoToUpsert.UpdateDate,
                            dtoToUpsert.DefaultSize).FirstOrDefault().Value;

                        // delete all the sum of boe task variables before saving the new list
                        // this way the front end doesn't have to keep track of all the checks/unchecks
                        // Also this cleans up the variable if it changed from SumOfBOEs to Discrete
                        gbe.deleteSumOfBOEByOrdinaryVariableID(ordinaryVariableID);
                        List<SelectBOEsToSum> toBeInserted = dtoToUpsert.SelectedBOEsToSum.ToList();

                        foreach (SelectBOEsToSum boe in toBeInserted)
                        {
                            int? sprocResults = gbe.insertSumOfBOE_OrdinaryVariable(ordinaryVariableID, boe.CLINID, boe.WBSID, boe.BoeID).FirstOrDefault();

                            if (sprocResults.HasValue)
                            {
                                // dto needs to have the new id in order to support bulk delete
                                boe.OVSumID = sprocResults.Value;
                                boe.OrdinaryVariableID = ordinaryVariableID;
                            }
                        }

                        // delete all the task variables resource types before saving the new resources
                        // this way the front end doesn't have to keep track of all the checks/unchecks
                        gbe.deleteOrdinaryVariableResourceType(ordinaryVariableID);

                        if (dtoToUpsert.SumVariableResourceTypeIDs.Any())
                        {
                            foreach (int resourceTypeID in dtoToUpsert.SumVariableResourceTypeIDs)
                            {
                                gbe.insertOrdinaryVariableResourceType(ordinaryVariableID, resourceTypeID);
                            }
                        }
                    }
                }

                return ordinaryVariableID;
            }
        }

        private ICollection<OrdinaryVariableDto> ConvertToDto(ICollection<OrdinaryVariableData> entities)
        {
            List<OrdinaryVariableDto> result = new List<OrdinaryVariableDto>();

            foreach (OrdinaryVariableData entity in entities)
            {
                OrdinaryVariableDto response = new OrdinaryVariableDto
                {
                    Id = entity.Variable.OrdinaryVariableID,
                    OrdinaryVariableName = entity.Variable.OrdinaryVariableName,
                    OrdinaryVariableValue = entity.Variable.OrdinaryVariableValue,
                    IsPercentage = entity.Variable.IsPercentage,
                    ValueType = (VarValueType)entity.Variable.ValueTypeID,
                    SortBOEBy = (VarSortBOEBy)entity.Variable.SortByID,
                    UpdateDate = entity.Variable.UpdateDT,
                    BoeID = entity.BoeId,
                    TaskElementId = entity.Variable.BOETaskElementID,
                    SelectedBOEsToSum = entity.BoesToSum.ToList(),
                    SumVariableResourceTypeIDs = entity.TypesOfResourcesToSum.ToList(),
                    DefaultSize = entity.Variable.DefaultSize != null ?  entity.Variable.DefaultSize : string.Empty
                };

                result.Add(response);
            }

            return result;
        }

        /// <summary>
        /// Provides the metadata to support bulk save processing for DTOs
        /// </summary>
        /// <exception cref=NotImplementedException>Thrown by default. Any loader that supports bulk save processing must implement this method.</exception>
        /// <returns>Meta data required for bulk save processing</returns>
        override public BulkSaveMetaData CreateBulkSaveMetaData()
        {
            BulkSaveMetaData metaData = new BulkSaveMetaData(Constants.BOE_DB_CONTEXT_NAME);

            metaData.BulkDeleteStoredProcedureName = "deleteBOETaskElementOrdinaryVariableviaTableParameter";
            metaData.BulkInsertStoredProcedureName = "insertBOETaskElementOrdinaryVariableviaTableParameter";
            metaData.BulkUpdateStoredProcedureName = "updateBOETaskElementOrdinaryVariableviaTableParameter";

            metaData.BulkInsertStoredProcedureReturnsUpdateDate = true;
            metaData.BulkUpdateStoredProcedureReturnsUpdateDate = true;

            metaData.DBTableTypeName = "TT_OrdinaryVariable";
            metaData.StoredProcedureTableTypeParameterName = "@BOETaskElementOrdinaryVariableTableParameter";

            metaData.EntityPropertiesToMapToDataTable = new Collection<string>()
            {
                "OrdinaryVariableID", "UpdateDT", "OrdinaryVariableName", "OrdinaryVariableValue", "BOETaskElementID", 
                "SortByID", "ValueTypeID", "IsPercentage", "DefaultSize"
            };

            return metaData;
        }

        /// <summary>
        /// Converts the Task Element DTO into a BOETaskElement entity.
        /// </summary>
        /// <param name="dtoToConvert">dto to convert</param>
        /// <returns>entity representing the dto</returns>
        protected override OrdinaryVariable ConvertDtoToEntity(OrdinaryVariableDto dtoToConvert)
        {
            if (dtoToConvert == null)
            {
                throw new ArgumentNullException(nameof(dtoToConvert));
            }

            OrdinaryVariable entity = null;

            entity = new OrdinaryVariable()
            {
                OrdinaryVariableID = dtoToConvert.Id,
                OrdinaryVariableName = dtoToConvert.OrdinaryVariableName,
                OrdinaryVariableValue = dtoToConvert.OrdinaryVariableValue,
                SortByID = (int)dtoToConvert.SortBOEBy,
                ValueTypeID = (int)dtoToConvert.ValueType,
                IsPercentage = dtoToConvert.IsPercentage,
                DefaultSize = dtoToConvert.DefaultSize,
                BOETaskElementID = dtoToConvert.TaskElementId,
                UpdateDT = dtoToConvert.UpdateDate
            };

            return entity;
        }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification="Code Analysis is wrong, this is clearly instantiated through linq")]
        private class OrdinaryVariableData
        { 
            public OrdinaryVariable Variable { get; set; }
            public int BoeId { get; set; }
            public IEnumerable<SelectBOEsToSum> BoesToSum { get; set; }
            public IEnumerable<int> TypesOfResourcesToSum { get; set; }
        }
    }
}
