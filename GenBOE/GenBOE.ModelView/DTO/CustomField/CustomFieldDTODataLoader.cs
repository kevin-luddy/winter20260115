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
    using GenBOE.Models;
    using GenBOE.Dtos;

    public class CustomFieldDTODataLoader : DataLoader<CustomFieldDTO>, ICustomFieldDTODataLoader
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public CustomFieldDTODataLoader() 
        {
            this.Log = new Logger(typeof(CustomFieldDTODataLoader));
        }

        #region Retrieve

        /// <summary>
        /// Gets a collection of Custom Fields for a list of Ids.
        /// </summary>
        /// <param name="ids">Custom field Ids.</param>
        /// <returns>Custom Fields Dtos for a list of Ids</returns>
        [DbQuery]
        public override ICollection<CustomFieldDTO> GetByIds(ICollection<int> ids)
        {
            using (StopwatchTimer sw = new StopwatchTimer(Log))
            {
                ICollection<CustomFieldDTO> toReturn = new List<CustomFieldDTO>();

                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    toReturn = (from c in gbe.CustomFields.Where(c => ids.Contains(c.CustomFieldID))
                                select new CustomFieldDTO
                                {
                                    Id = c.CustomFieldID,
                                    CustomFieldName = c.CustomFieldName,
                                    CustomFieldDisplayID = (CustomFieldType)c.CustomFieldDisplayID,
                                    CustomFieldRequired = c.CustomFieldRequired,
                                    WorkspaceID = c.WorkspaceID,
                                    UpdateDate = c.UpdateDT,
                                    IsOpenEnded = c.IsOpenEnded
                                }).ToCollection<CustomFieldDTO>();
                }

                return toReturn;
            }
        }

        /// <summary>
        /// Get Custom Fields by workspace Id.
        /// </summary>
        /// <param name="inWorkspaceID">workspace</param>
        /// <returns>Custom Field Dtos associated with the workspace.</returns>
        [DbQuery]
        virtual public ICollection<CustomFieldDTO> GetByWorkspaceId(int inWorkspaceID)
        {
            using (StopwatchTimer sw = new StopwatchTimer(Log))
            {
                ICollection<CustomFieldDTO> toReturn = new List<CustomFieldDTO>();

                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    toReturn = (from c in gbe.CustomFields
                                where c.WorkspaceID == inWorkspaceID
                                select new CustomFieldDTO
                                {
                                    Id = c.CustomFieldID,
                                    CustomFieldName = c.CustomFieldName,
                                    CustomFieldDisplayID = (CustomFieldType)c.CustomFieldDisplayID,
                                    CustomFieldRequired = c.CustomFieldRequired,
                                    WorkspaceID = c.WorkspaceID,
                                    UpdateDate = c.UpdateDT,
                                    IsOpenEnded = c.IsOpenEnded
                                }).ToCollection<CustomFieldDTO>();
                }

                return toReturn;
            }
        }

        /// <summary>
        /// Get all custom field values assigned to the given BOEs
        /// </summary>
        /// <param name="boeID">The BOE</param>
        /// <returns>Dictionary of custom field values for the BOEs</returns>
        [DbQuery]
        public IDictionary<int, IDictionary<CustomFieldValueDTO, CustomFieldDTO>> GetAllAssignedBOECustomFieldValuesForBoeIds(ICollection<int> boeIds)
        {
            IDictionary<int, IDictionary<CustomFieldValueDTO, CustomFieldDTO>> results = new Dictionary<int, IDictionary<CustomFieldValueDTO, CustomFieldDTO>>();

            using (GenBoeEntities db = new GenBoeEntities())
            {
                var values =
                    (from X in db.BOECustomFieldValueXREFs
                     join B in db.BOEs on X.BOEID equals B.BOEID
                     join V in db.CustomFieldValues on X.CustomFieldValueID equals V.CustomFieldValueID
                     join F in db.CustomFields on V.CustomFieldID equals F.CustomFieldID
                     where boeIds.Contains(B.BOEID)
                     select new
                     {
                         BOEID = B.BOEID,
                         WorkspaceID = B.WorkspaceID,
                         CustomFieldID = F.CustomFieldID,
                         CustomFieldDisplayID = F.CustomFieldDisplayID,
                         CustomFieldRequired = F.CustomFieldRequired,
                         CustomFieldName = F.CustomFieldName,
                         CustomFieldValueID = V.CustomFieldValueID,
                         CustomFieldValueName = V.CustomFieldValueName,
                         CustomFieldValueDescription = V.CustomFieldValueDescription,
                         CustomFieldValueInUseFlag = V.CustomFieldValueInUseFlag,
                         CustomFIeldIsOpenEnded = F.IsOpenEnded
                     }).ToList();

                foreach (int boeId in boeIds)
                {
                    IDictionary<CustomFieldValueDTO, CustomFieldDTO> partial = new Dictionary<CustomFieldValueDTO, CustomFieldDTO>();

                    foreach (var field in values.Where(x => x.BOEID == boeId).OrderBy(c => c.CustomFieldID))
                    {
                        CustomFieldDTO customField = new CustomFieldDTO
                        {
                            Id = field.CustomFieldID,
                            CustomFieldName = field.CustomFieldName,
                            CustomFieldDisplayID = field.CustomFieldDisplayID.GetEnumeratedValue<CustomFieldType>(CustomFieldType.BoeDisplay),
                            CustomFieldRequired = field.CustomFieldRequired,
                            WorkspaceID = field.WorkspaceID,
                            IsOpenEnded = field.CustomFIeldIsOpenEnded
                        };

                        CustomFieldValueDTO customFieldValue = new CustomFieldValueDTO
                        {
                            CustomFieldValueID = field.CustomFieldValueID,
                            CustomFieldValueName = field.CustomFieldValueName,
                            CustomFieldValueDescription = field.CustomFieldValueDescription,
                            CustomFieldID = field.CustomFieldID,
                            CustomFieldValueInUseFlag = field.CustomFieldValueInUseFlag
                        };

                        partial.Add(customFieldValue, customField);
                    }

                    results.Add(boeId, partial);
                }

            }

            return results;
        }

        #endregion Retrieve

        #region Commit

        protected override int? Upsert(CustomFieldDTO dtoToUpsert)
        {
            using (StopwatchTimer sw = new StopwatchTimer(Log))
            {
                int? result = null;
            
                if (dtoToUpsert != null)
                {
                    using (GenBoeEntities gbe = new GenBoeEntities())
                    {
                        result = gbe.upsertCustomField(dtoToUpsert.Id,
                            dtoToUpsert.CustomFieldName,
                            dtoToUpsert.CustomFieldRequired,
                            dtoToUpsert.IsOpenEnded,
                            (int)dtoToUpsert.CustomFieldDisplayID,
                            dtoToUpsert.WorkspaceID,
                            dtoToUpsert.UpdateDate).FirstOrDefault();
                    }
                }
                return result;
            }
        }

        protected override int? Delete(CustomFieldDTO dtoToDelete)
        {
            using (StopwatchTimer sw = new StopwatchTimer(Log))
            {
                int? toReturn = null;

                if (dtoToDelete != null)
                {
                    using (GenBoeEntities gbe = new GenBoeEntities())
                    {
                        gbe.deleteCustomField(dtoToDelete.Id, dtoToDelete.UpdateDate);
                    }

                    toReturn = dtoToDelete.Id;
                }
                return toReturn;
            }
        }

        #endregion Commit
    }
}
