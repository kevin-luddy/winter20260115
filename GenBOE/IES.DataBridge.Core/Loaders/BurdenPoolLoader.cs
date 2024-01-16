// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------
namespace IES.DataBridge.Loaders
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Linq;
	using IES.DataBridge.ModelViews;
	using IES.Models;
	using IES.Common.Core;
	using IES.Common.Core.Exceptions;
	using Microsoft.Extensions.Logging;
	using IES.Common.Core.Utilities;
	using IES.Common.Core.Models;
	using IES.Common.Core.Loaders;
	using IES.Common.Core.Enums;

	/// <summary>
	/// BurdenPoolLoader
	/// </summary>
	public class BurdenPoolLoader : DataLoader<BurdenPoolDetailModelView>, IBurdenPoolLoader
    {
        #region constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="BurdenPoolLoader"/> class.
        /// </summary>
        public BurdenPoolLoader(ILogger<BurdenPoolLoader> logger) : base(logger)
        {
        }

        #endregion

        #region Retrieves

        /// <summary>
        /// BurdenPoolGridModelView GetByRevision
        /// </summary>
        /// <param name="revisionId">int</param>
        /// <returns>BurdenPoolGridModelView</returns>
        public BurdenPoolGridModelView GetByRevision(int revisionId)
        {
            BurdenPoolGridModelView result = new BurdenPoolGridModelView();

            using (StopwatchTimer sw = new StopwatchTimer(this.Log))
            {
                using (IESEntities context = new IESEntities())
                {
                    // Get Rate Codes for this revision. Start with "Not Selected" option.
                    result.RateCodes = new Collection<OptionModelView>() { new OptionModelView() { Id = 0, Label = string.Empty } }; 
                    result.RateCodes.AddRange(context.RateCodes.Where(r => r.RevisionID == revisionId)
                        .Select(r => new OptionModelView()
                        {
                            Id = r.ID,
                            Label = r.RateCode1
                        })
                        .OrderBy(r => r.Label)
                        .ToCollection());

                    // Get Burden Elements in display order.
                    result.BurdenElements = context.BurdenElementLUs.Select(e =>
                            new BurdenElementModelView()
                            {
                                Id = e.ID,
                                DisplayOrder = e.DisplayOrder,
                                Name = e.BurdenElement,
                                Description = e.Description
                            })
                        .OrderBy(e => e.DisplayOrder)
                        .ToList();

                    // Get Burden Pools and Mappings.
                    result.BurdenPools = context.BurdenPoolLUs.Where(b => b.RevisionID == revisionId)
                        .Select(e => new BurdenPoolDetailModelView()
                        {
                            IsGaT2ApplicableForMissionSolutions = e.IsGaT2ApplicableForMissionSolutions,
                            IncludeGaT2InBurdAndCommBurdTables = e.IncludeGaT2InBurdAndCommBurdTables,
                            IsCommercial = e.IsCommercial,
                            ExcludeFCCOMFromCommercial = e.ExcludeFCCOM,
                            Description = e.Description,
                            BurdenPool = e.BurdenPool,
                            UpdateDate = e.UpdateDate,
                            Id = e.ID,
                            RevisionID = e.RevisionID,
                            BurdenElementRateCodeMappings =
                                e.ProPricerBurdenRateMaps.Select(x =>
                                new BurdenElementIdToRateCodeModelView()
                                {
                                    Id = x.ID,
                                    UpdateDate = x.UpdateDate,
                                    RateCode = x.RateCode.RateCode1,
                                    RateCodeId = x.RateCodeID,
                                    BurdenElementId = x.BurdenElementID
                                }).ToList()
                        }).ToList();

                    result = this.AddProPricerMapping(result);

                    return result;
                }
            }
        }

        /// <summary>
        /// Gets all Commercial and Government Burden Pools as options lists.
        /// </summary>
        /// <param name="revisionId">Revision Id</param>
        /// <param name="commercialBurdenPoolOptions">Out parameter for returning Commercial Burden Pool options.</param>
        /// <param name="governmentBurdenPoolsOptions">Out parameter for returning Government Burden Pool options.</param>
        public void GetBurdenPoolOptions(int revisionId, out ICollection<OptionModelView> commercialBurdenPoolOptions, out ICollection<OptionModelView> governmentBurdenPoolsOptions)
        {
            ICollection<BurdenPoolLU> burdenPools;
            using (IESEntities context = new IESEntities())
            {
                // get Burden Pools for this revision
                burdenPools = context.BurdenPoolLUs
                    .Where(bp => bp.RevisionID == revisionId)
                    .ToCollection();
            }

            ICollection<OptionModelView> commOptions = new List<OptionModelView> { new OptionModelView() { Id = 0, Label = string.Empty } };
            ICollection<OptionModelView> govtOptions = new List<OptionModelView> { new OptionModelView() { Id = 0, Label = string.Empty } };
            foreach (BurdenPoolLU burdenPool in burdenPools)
            {
                string label = string.IsNullOrEmpty(burdenPool.Description) ? burdenPool.BurdenPool : burdenPool.BurdenPool + " : " + burdenPool.Description;
                
                if (burdenPool.IsCommercial)
                {
                    commOptions.Add(new OptionModelView() { Id = burdenPool.ID, Label = label });
                }
                else
                {
                    govtOptions.Add(new OptionModelView() { Id = burdenPool.ID, Label = label });
                }
            }

            commercialBurdenPoolOptions = commOptions.OrderBy(x => x.Label).ToList();
            governmentBurdenPoolsOptions = govtOptions.OrderBy(x => x.Label).ToList();
        }

        /// <summary>
        /// Gets all objects based on the IDs that were passed in
        /// </summary>
        /// <param name="ids">IDs</param>
        /// <returns>
        /// Corresponding Data
        /// </returns>
        public override ICollection<BurdenPoolDetailModelView> GetByIds(ICollection<int> ids)
        {
            throw new NotImplementedException();
        }

        #endregion

        /// <summary>
        /// adds propricer mapping to each burdenpool
        /// </summary>
        /// <param name="model"> burdenPoolGridModelView</param>
        /// <returns>BurdenPoolGridModelView</returns>
        private BurdenPoolGridModelView AddProPricerMapping(BurdenPoolGridModelView model)
        {
            // Create a lookup list of burden element Ids that correspond to items in the BurdenElements array.
            List<int> burdenElementIds = model.BurdenElements.Select(x => x.Id).ToList();

            foreach (BurdenPoolDetailModelView bpdmv in model.BurdenPools)
            {
                bpdmv.BurdenElementRateCodeArray = new string[model.BurdenElements.Count];

                // Initialize the array with empty strings. 
                // (Note: leaving them null would cause rate code drop-down select lists in grid to display an extra blank option).
                for (int i = 0; i < bpdmv.BurdenElementRateCodeArray.Length; i++)
                {
                    bpdmv.BurdenElementRateCodeArray[i] = string.Empty;
                }

                // Populate specific array entries with Burden Element/Rate Code mappings
                foreach (BurdenElementIdToRateCodeModelView be2rc in bpdmv.BurdenElementRateCodeMappings)
                {
                    // Add rate code string to the proper burden element column
                    bpdmv.BurdenElementRateCodeArray[burdenElementIds.IndexOf(be2rc.BurdenElementId)] = be2rc.RateCode;
                }
            }

            return model;
        }

        #region Commits

        /// <summary>
        /// Upsert
        /// </summary>
        /// <param name="dtoToUpsert">Dto that is upserted</param>
        /// <returns>Id of the dto after the modification</returns>
        protected override int? Upsert(BurdenPoolDetailModelView dtoToUpsert)
        {
            if (dtoToUpsert == null)
            {
                throw new ArgumentNullException(nameof(dtoToUpsert));
            }

            int? burdenPoolId;

            using (IESEntities iesEntities = new IESEntities())
            {
                burdenPoolId = iesEntities.upsertBurdenPoolLU(
                    dtoToUpsert.Id,
                    dtoToUpsert.UpdateDate,
                    dtoToUpsert.RevisionID,
                    dtoToUpsert.BurdenPool,
                    dtoToUpsert.Description,
                    dtoToUpsert.IsGaT2ApplicableForMissionSolutions,
                    dtoToUpsert.IncludeGaT2InBurdAndCommBurdTables,
                    dtoToUpsert.IsCommercial,
                    dtoToUpsert.ExcludeFCCOMFromCommercial).First();
            }

            if (burdenPoolId > 0)
            {
                dtoToUpsert.Id = (int)burdenPoolId;
                this.UpsertCodeMappings(dtoToUpsert);
            }

            return burdenPoolId;
        }

        /// <summary>
        /// Override the base class save for performance. 
        /// </summary>
        /// <param name="dtoToSave">Dto to save.</param>
        /// <returns>NotImplementedException - Individual Save operation is not supported.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "SaveBurdenPools")]
        public override int? Save(BurdenPoolDetailModelView dtoToSave)
        {
            throw new NotImplementedException("Individual Save is not supported, use SaveBurdenPools.");
        }

        /// <summary>
        /// Save collection of BurdenPools. All pools are passed in, then all reference data
        /// can be retrieved in one DB call to get RateCodes and BurdenElements.
        /// </summary>
        /// <param name="dtosIn">BurdenPoolDetailModelView</param>
        /// <param name="revisionId">Should be the WIP revision.</param>
        public void SaveBurdenPools(ICollection<BurdenPoolDetailModelView> dtosIn, int revisionId)
        {
            BurdenPoolGridModelView burdenPoolGridModel = this.GetByRevision(revisionId);
            ICollection<BurdenPoolDetailModelView> dtosFromDb = burdenPoolGridModel.BurdenPools;

            if (dtosIn == null || dtosIn.Count == 0)
            {
                throw new ArgumentNullException(nameof(dtosIn));
            }

            // Verify burden pools all Adds are unique.
            IEnumerable<BurdenPoolDetailModelView> dupList = dtosIn.Where(x => x.Id < 0).Concat(dtosFromDb);
            List<string> duplicateBurdenPools = dupList.Where(x => !x.IsDeleted).GroupBy(x => x.BurdenPool)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicateBurdenPools.Any())
            {
                throw new GenValidationException(string.Format("Duplicate Burden Pools are not allowed: {0}.", string.Join(", ", duplicateBurdenPools)));
            }

            // Create a lookup list of burden element Ids that correspond to items in the BurdenElements array.
            List<int> burdenElementIds = burdenPoolGridModel.BurdenElements.Select(x => x.Id).ToList();

            // get each new burden pool
            foreach (BurdenPoolDetailModelView inBpdmv in dtosIn)
            {
                if (inBpdmv.Dirty)
                {
                    if (inBpdmv.Id < 1)
                    {
                        inBpdmv.RevisionID = revisionId;
                    }

                    if (inBpdmv.IsDeleted)
                    {
                        inBpdmv.Updateable = UpdateType.Deleted;
                        base.Save(inBpdmv);
                    }
                    else
                    {
                        // Build BurdenElementRateCodeMappings collection from arrays.
                        if (inBpdmv.BurdenElementRateCodeArray != null)
                        {
                            BurdenPoolDetailModelView checkBpdmv = dtosFromDb.FirstOrDefault(x => x.Id == inBpdmv.Id);
                            for (int i = 0; i < inBpdmv.BurdenElementRateCodeArray.Length; i++)
                            {
                                // For Adds (checkBpdmv == null) add all the mappings.
                                if (checkBpdmv == null || checkBpdmv.BurdenElementRateCodeArray[i] != inBpdmv.BurdenElementRateCodeArray[i])
                                {
                                    BurdenElementIdToRateCodeModelView oldBpm = null;

                                    // Find burden element Id.
                                    int burdenElementIdForDisplayPosition = burdenElementIds[i];

                                    if (inBpdmv.BurdenElementRateCodeMappings != null)
                                    {
                                        if (burdenElementIdForDisplayPosition >= 0)
                                        {
                                            oldBpm = inBpdmv.BurdenElementRateCodeMappings.FirstOrDefault(b => b.BurdenElementId == burdenElementIdForDisplayPosition);
                                        }
                                    }

                                    if (string.IsNullOrEmpty(inBpdmv.BurdenElementRateCodeArray[i]))
                                    {
                                        // Rate has been deleted.
                                        if (oldBpm != null)
                                        {
                                            oldBpm.Updateable = UpdateType.Deleted;
                                        }
                                    }
                                    else
                                    {
                                        // Rate has been added or modified.
                                        if (oldBpm == null)
                                        {
                                            // Add new mappings.
                                            if (inBpdmv.BurdenElementRateCodeMappings == null)
                                            {
                                                inBpdmv.BurdenElementRateCodeMappings = new Collection<BurdenElementIdToRateCodeModelView>();
                                            }

                                            inBpdmv.BurdenElementRateCodeMappings.Add(new BurdenElementIdToRateCodeModelView
                                            {
                                                Updateable = UpdateType.Upsert,
                                                Id = -1,
                                                BurdenElementId = burdenElementIdForDisplayPosition,
                                                RateCode = inBpdmv.BurdenElementRateCodeArray[i],
                                                RateCodeId = burdenPoolGridModel.RateCodes.First(r => r.Label == inBpdmv.BurdenElementRateCodeArray[i]).Id
                                        });
                                        }
                                        else
                                        {
                                            // Update existing mapping with new rate.
                                            oldBpm.Updateable = UpdateType.Upsert;
                                            oldBpm.RateCode = inBpdmv.BurdenElementRateCodeArray[i];
                                            oldBpm.RateCodeId = burdenPoolGridModel.RateCodes.First(r => r.Label == inBpdmv.BurdenElementRateCodeArray[i]).Id;
                                        }
                                    }
                                }
                            }
                        }

                        inBpdmv.Updateable = UpdateType.Upsert;
                        base.Save(inBpdmv);
                    }
                }
            }
        }

        /// <summary>
        /// upsertCodeMappings
        /// </summary>
        /// <param name="bpdmv">BurdenPoolDetailModelView</param>
        private void UpsertCodeMappings(BurdenPoolDetailModelView bpdmv)
        {
            if (bpdmv.BurdenElementRateCodeMappings != null)
            {
                using (IESEntities iesEntities = new IESEntities())
                {
                    foreach (BurdenElementIdToRateCodeModelView bercm in bpdmv.BurdenElementRateCodeMappings)
                    {
                        // No change needed for UpdateType.None.
                        if (bercm.Updateable == UpdateType.Deleted)
                        {
                            iesEntities.deleteProPricerBurdenRateMap(bercm.Id, bercm.UpdateDate);
                        }
                        else if (bercm.Updateable == UpdateType.Upsert)
                        {
                            iesEntities.upsertProPricerBurdenRateMap(bercm.Id, bercm.UpdateDate,
                                bpdmv.Id, bercm.BurdenElementId, bercm.RateCodeId);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Delete
        /// </summary>
        /// <param name="dtoToDelete">Dto that is deleted</param>
        /// <returns>Id of the deleted dto</returns>
        protected override int? Delete(BurdenPoolDetailModelView dtoToDelete)
        {
            if (dtoToDelete == null)
            {
                throw new ArgumentNullException(nameof(dtoToDelete));
            }

            int? toReturn = null;

            // Burden pool added, then deleted before save - don't delete.
            if (dtoToDelete.Id > 0)
            {
                using (IESEntities iesEntities = new IESEntities())
                {
                    toReturn = iesEntities.deleteBurdenPoolLU(dtoToDelete.Id, dtoToDelete.UpdateDate);  // Stored Procedure also deletes associated ProPricerBurdenRateMap rows 
                }
            }

            return toReturn;
        }
        #endregion
    }
}