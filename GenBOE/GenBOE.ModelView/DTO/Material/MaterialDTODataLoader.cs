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
    using IES.Common;
    using GenBOE.Dtos;
    using GenBOE.Models;

    public class MaterialDTODataLoader : IMaterialDTODataLoader
    {
        private Logger _log = new Logger(typeof(MaterialDTODataLoader));

        /// <summary>
        /// Constructor
        /// </summary>
        public MaterialDTODataLoader() { }

        #region Retrieves

        /// <summary>
        /// Gets a Material by Id
        /// </summary>
        /// <param name="inMaterialID">Id</param>
        /// <param name="includeRTEFields">Indicates whether RTE fields should be retrieved as a part of the data pull</param>
        /// <returns>Material for the id</returns>
        [DbQuery]
        virtual public MaterialDTO GetById(int inMaterialID, bool includeRTEFields = false)
        {
            MaterialDTO result;

            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    result = (from m in gbe.MaterialTaskElements
                              where m.MaterialTaskElementID == inMaterialID
                              select new MaterialDTO
                              {
                                  // 2 RTE fields:
                                  TaskDescription = includeRTEFields ? m.MaterialTaskDescription : null,
                                  MoqText = includeRTEFields ? m.MaterialMOQText : null,
                                  WasMoqTextSet = includeRTEFields,
                                  WasDescriptionSet = includeRTEFields,

                                  Id = m.MaterialTaskElementID,
                                  TaskID = m.MaterialTaskID,
                                  TaskTitle = m.MaterialTaskTitle,
                                  UpdateDate = m.UpdateDT,
                                  BoeID = m.BOEID
                              }).FirstOrDefault();
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the number of materials for the specified Boe
        /// </summary>
        /// <param name="boeId">BOE Id</param>
        /// <returns>Number of materials</returns>
        [DbQuery]
        public int GetNumberOfMaterialsForBoeId(int inBoeID)
        {
            int result = 0;
            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    result = gbe.MaterialTaskElements.Count(m => m.BOEID == inBoeID);
                }
            }

            return result;
        }

        /// <summary>
        /// Gets Materials by Boe Id
        /// </summary>
        /// <param name="boeIds">Boe Ids</param>
        /// <param name="includeRTEFields">Indicates whether RTE fields should be retrieved as a part of the data pull</param>
        /// <returns>Material Dtos</returns>
        [DbQuery]
        virtual public Collection<MaterialDTO> GetByBoeIds(Collection<int> boeIds, bool includeRTEFields = false)
        {
            List<MaterialDTO> result;

            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    result = (from m in gbe.MaterialTaskElements
                              where boeIds.Contains(m.BOEID)
                              select new MaterialDTO
                              {
                                  // 2 RTE fields:
                                  TaskDescription = includeRTEFields ? m.MaterialTaskDescription : null,
                                  MoqText = includeRTEFields ? m.MaterialMOQText : null,
                                  WasMoqTextSet = includeRTEFields,
                                  WasDescriptionSet = includeRTEFields,

                                  Id = m.MaterialTaskElementID,
                                  TaskID = m.MaterialTaskID,
                                  TaskTitle = m.MaterialTaskTitle,
                                  UpdateDate = m.UpdateDT,
                                  BoeID = m.BOEID
                              }).ToList();
                }
            }

            return result.ToCollection();
        }

        #region RTE Load Methods

        /// <summary>
        /// Pulls RTE fields for the DTOs, and updates them as needed
        /// </summary>
        /// <param name="dtos">DTOs that whose RTE fields will be loaded, if they are null</param>
        [DbQuery]
        public void LoadRTEFields(ICollection<MaterialDTO> dtos)
        {
            if (dtos == null || !dtos.Any()) { return; }

            List<RteFieldsHelper> dataFromDb = null;

            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                // Only want to pull RTE for an object when:
                //   - the object exists (Id > 0)
                //   - at least one of the RTE fields has not been inserted into, or retrieved already
                List<int> dtoIds = dtos.Where(x => x.Id > 0 && (!x.WasMoqTextSet || !x.WasDescriptionSet) && x.Updateable != UpdateType.Deleted).Select(x => x.Id).ToList();

                if (dtoIds.Any())
                {
                    using (GenBoeEntities gbe = new GenBoeEntities())
                    {
                        gbe.Database.CommandTimeout = 360;  // give queries enough time to execute

                        // get the basic BOE data from the sprocResults
                        dataFromDb = (from o in gbe.MaterialTaskElements
                                      where dtoIds.Contains(o.MaterialTaskElementID)
                                      select new RteFieldsHelper
                                      {
                                          Id = o.MaterialTaskElementID,
                                          Description = o.MaterialTaskDescription,
                                          MoqText = o.MaterialMOQText
                                      }).ToList();
                    }

                    dataFromDb.AsParallel().ForAll(dbData =>
                    {
                        MaterialDTO dto = dtos.First(b => b.Id == dbData.Id);

                        // We do not want to overwrite existing data during, so we only fill missing data, if available
                        dto.TaskDescription = dto.WasDescriptionSet ? dto.TaskDescription : dbData.Description;
                        dto.MoqText = dto.WasMoqTextSet ? dto.MoqText : dbData.MoqText;
                    });
                }
            }
        }

        #endregion

        #endregion Retrieves

        #region Commits

        #region Material Element Commits

        /// <summary>
        /// Save materials data
        /// </summary>
        /// <param name="inMaterialDTOs">Materials to save</param>
        virtual public void SaveMaterials(ICollection<MaterialDTO> inMaterialDTOs)
        {
            if (inMaterialDTOs == null) { throw new ArgumentNullException(nameof(inMaterialDTOs)); }

            // Load just in case.. to make sure we do not wipe out the RTE values
            this.LoadRTEFields(inMaterialDTOs);

            // The Material Elements should be the first items saved
            foreach (MaterialDTO material in inMaterialDTOs)
            {
                if (material.Updateable == UpdateType.Deleted)
                {
                    this.DeleteMaterial(material);
                }
                else if (material.Updateable == UpdateType.Upsert)
                {
                    throw new NotSupportedException();
                }
            }
        }

        /// <summary>
        /// Delete the Material element
        /// </summary>
        /// <param name="inMaterial">Material to delete</param>
        virtual internal void DeleteMaterial(MaterialDTO inMaterial)
        {
            if (inMaterial == null) { throw new ArgumentNullException(nameof(inMaterial)); }

            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                gbe.deleteMaterialTaskElement(inMaterial.Id, inMaterial.UpdateDate);
            }
        }

        #endregion

        #region Material Type Commits

        /// <summary>
        /// Deletes All Material elements for a BOE 
        /// </summary>
        /// <param name="boeId">BOE Id</param>
        public void DeleteAllMaterialTaskElements(int boeId)
        {
            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                using (GenBoeEntities gbm = new GenBoeEntities())
                {
                    gbm.deleteAllMaterialTaskElements(boeId);
                }
            }
        }

        #endregion

        #endregion
    }
}