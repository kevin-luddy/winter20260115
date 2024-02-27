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
	using GenBOE.Models;
	using GenBOE.Dtos;
	using static IES.Common.Constants;

	/// <summary>
	/// Resource Type Loader
	/// </summary>
	public class ResourceTypeLoader : BulkDataLoader<ResourceTypeDto, BOELaborType>, IResourceTypeLoader
	{
		/// <summary>
		/// Constructor
		/// </summary>
		public ResourceTypeLoader()
		{
			this.Log = new Logger(typeof(ResourceTypeLoader));
		}

		[DbQuery]
		virtual public ICollection<ResourceTypeDto> GetByBoeId(int boeId)
		{
			List<ResourceTypeDto> toReturn = null;

			using (StopwatchTimer sw = new StopwatchTimer(this.Log))
			{
				using (GenBoeEntities gbe = new GenBoeEntities())
				{
					toReturn = (from tE in gbe.BOETaskElements
								where tE.BOEID == boeId && tE.TaskElementTypeID == (int)TaskElementType.Labor
								join laborType in gbe.BOELaborTypes on tE.BOETaskElementID equals laborType.BOETaskElementID
								select new ResourceTypeDto
								{
									BoeID = laborType.BOETaskElement.BOEID,
									Id = laborType.BOELaborTypeID,
									PerformingOrgID = laborType.PerformingOrganizationID,
									ResourceID = laborType.ResourceID,
									BusinessResourceCodeID = laborType.BRCResourceID,
									ValueSpread = laborType.ValueSpread,
									SpreadCurveIDValue = laborType.SpreadCurveID,
									PercentSpread = laborType.PercentSpread,
									StartDateValue = laborType.BOELaborTypeStartDate,
									EndDateValue = laborType.BOELaborTypeEndDate,
									TaskElementId = laborType.BOETaskElementID,
									UpdateDate = laborType.UpdateDT,
									PercentSpreadLocked = laborType.PercentSpreadLocked,
									HourSpreadLocked = laborType.HourSpreadLocked,
									SpreadType = laborType.SpreadTypeID.HasValue ? (SpreadType)laborType.SpreadTypeID.Value : SpreadType.NotSet,
									WBSID = laborType.WBSID,
									CLINID = laborType.CLINID,
									CanOffload = laborType.CanOffload ?? false,
									LaborTypeOrder = laborType.LaborSortId
								}).ToList();

					LoadSikorskyFields(gbe, toReturn);
				}
			}

			return toReturn;
		}

		/// <summary>
		/// Get By Ids
		/// </summary>
		/// <param name="ids">Ids to get</param>
		/// <returns>Collection of resource types</returns>
		[DbQuery]
		public override ICollection<ResourceTypeDto> GetByIds(ICollection<int> ids)
		{
			using (StopwatchTimer sw = new StopwatchTimer(this.Log))
			{
				List<ResourceTypeDto> toReturn;

				using (GenBoeEntities gbe = new GenBoeEntities())
				{
					toReturn = gbe.BOELaborTypes.Where(x => ids.Contains(x.BOELaborTypeID))
						.Select(laborType => new ResourceTypeDto
						{
							BoeID = laborType.BOETaskElement.BOEID,
							Id = laborType.BOELaborTypeID,
							PerformingOrgID = laborType.PerformingOrganizationID,
							ResourceID = laborType.ResourceID,
							ValueSpread = laborType.ValueSpread,
							SpreadCurveIDValue = laborType.SpreadCurveID,
							PercentSpread = laborType.PercentSpread,
							StartDateValue = laborType.BOELaborTypeStartDate,
							EndDateValue = laborType.BOELaborTypeEndDate,
							TaskElementId = laborType.BOETaskElementID,
							UpdateDate = laborType.UpdateDT,
							PercentSpreadLocked = laborType.PercentSpreadLocked,
							HourSpreadLocked = laborType.HourSpreadLocked,
							SpreadType = laborType.SpreadTypeID.HasValue ? (SpreadType)laborType.SpreadTypeID.Value : SpreadType.NotSet,
							WBSID = laborType.WBSID,
							CLINID = laborType.CLINID,
							CanOffload = laborType.CanOffload ?? false,
							LaborTypeOrder = laborType.LaborSortId,
							BusinessResourceCodeID = laborType.BRCResourceID
						}).ToList();

					LoadSikorskyFields(gbe, toReturn);
				}

				return toReturn;
			}
		}

		#region Sikorsky / Project Map Custom Fields

		/// <summary>
		/// Loads Sikorsky Custom Fields into Resource Type properties
		/// </summary>
		/// <param name="gbe">GenBOE Entities connected to the DB</param>
		/// <param name="resourcesToLoad">Resources to load</param>
		[DbQuery]
		public static void LoadSikorskyFields(GenBoeEntities gbe, List<ResourceTypeDto> resourcesToLoad)
		{
			if (gbe == null || resourcesToLoad == null) { return; }

			List<int> resourceTypeIds = resourcesToLoad.Select(z => z.Id).ToList();

			var sikorskyCfData = gbe.BOELaborTypeCustomFieldValueXREFs.Where(x => resourceTypeIds.Contains(x.BOELaborTypeID)).Select(x => new
			{
				CfValue = x.CustomFieldValue.CustomFieldValueDescription,
				FieldName = x.CustomFieldValue.CustomField.CustomFieldName,
				ResourceTypeId = x.BOELaborTypeID
			}).ToList();

			resourcesToLoad.ForEach(resource =>
			{
				resource.AddOrDelete = sikorskyCfData.FirstOrDefault(x => x.ResourceTypeId == resource.Id && x.FieldName == SikorskyConstants.SIKORSKY_CF_ADDDELETE)?.CfValue;
			});
		}

		#endregion

		/// <summary>
		/// Delete
		/// </summary>
		/// <param name="dtoToDelete">Type to delete</param>
		/// <returns>Id deleted</returns>
		protected override int? Delete(ResourceTypeDto dtoToDelete)
		{
			using (StopwatchTimer sw = new StopwatchTimer(this.Log))
			{
				int? toReturn = null;

				if (dtoToDelete != null)
				{
					this.Log.Debug(string.Format("ResourceTypeLoader.Delete => Ntid: {1}, BoeId: {0}, Item Id: {2}, Value: {3}",
						 dtoToDelete.BoeID, System.Threading.Thread.CurrentPrincipal.Identity.Name, dtoToDelete.Id, dtoToDelete.ValueSpread));

					using (GenBoeEntities gbe = new GenBoeEntities())
					{
						gbe.deleteBOELaborType(dtoToDelete.Id, dtoToDelete.UpdateDate);
					}

					toReturn = dtoToDelete.Id;
				}

				return toReturn;
			}
		}

		/// <summary>
		/// Upsert
		/// </summary>
		/// <param name="dtoToUpsert">type to upsert</param>
		/// <returns>Id of upserted dto</returns>
		protected override int? Upsert(ResourceTypeDto dtoToUpsert)
		{
			if (dtoToUpsert == null)
			{
				throw new ArgumentNullException(nameof(dtoToUpsert));
			}

			using (StopwatchTimer sw = new StopwatchTimer(this.Log))
			{
				int? result = null;

				if (dtoToUpsert != null)
				{
					this.Log.Debug(string.Format("ResourceTypeLoader.Upsert => Ntid: {1}, BoeId: {0}, Item Id: {2}, Value: {3}",
						 dtoToUpsert.BoeID, System.Threading.Thread.CurrentPrincipal.Identity.Name, dtoToUpsert.Id, dtoToUpsert.ValueSpread));

					// Set Resource ID to null if 0 since it will break upsert
					if (dtoToUpsert.ResourceID.HasValue && dtoToUpsert.ResourceID == 0)
					{
						dtoToUpsert.ResourceID = null;
					}

					// Set Business Resource Code ID to null if 0 since it will break upsert
					if (dtoToUpsert.BusinessResourceCodeID.HasValue && dtoToUpsert.BusinessResourceCodeID == 0)
					{
						dtoToUpsert.BusinessResourceCodeID = null;
					}

					using (GenBoeEntities gbe = new GenBoeEntities())
					{
						result = gbe.upsertBOELaborType(
							dtoToUpsert.Id,
							dtoToUpsert.ResourceID,
							dtoToUpsert.PerformingOrgID,
							dtoToUpsert.StartDate.Normalize(),
							dtoToUpsert.EndDate.Normalize(),
							(int?)dtoToUpsert.SpreadCurveID,
							dtoToUpsert.PercentSpread,
							dtoToUpsert.ValueSpread,
							dtoToUpsert.TaskElementId,
							(int?)dtoToUpsert.SpreadType,
							dtoToUpsert.UpdateDate,
							dtoToUpsert.PercentSpreadLocked,
							dtoToUpsert.HourSpreadLocked,
							dtoToUpsert.WBSID,
							dtoToUpsert.CLINID,
							dtoToUpsert.CanOffload,
							dtoToUpsert.LaborTypeOrder,
							dtoToUpsert.BusinessResourceCodeID).FirstOrDefault();
					}
				}

				return result;
			}
		}

		/// <summary>
		/// Provides the metadata to support bulk save processing for BoeTaskElementDTOs
		/// </summary>
		/// <exception cref=NotImplementedException>Thrown by default. Any loader that supports bulk save processing must implement this method.</exception>
		/// <returns>Meta data required for bulk save processing</returns>
		public override BulkSaveMetaData CreateBulkSaveMetaData()
		{
			BulkSaveMetaData metaData = new BulkSaveMetaData(Constants.BOE_DB_CONTEXT_NAME);

			metaData.BulkDeleteStoredProcedureName = "deleteBOELaborTypeviaTableParameter";
			metaData.BulkInsertStoredProcedureName = "insertBOELaborTypeviaTableParameter";
			metaData.BulkUpdateStoredProcedureName = "updateBOELaborTypeviaTableParameter";

			metaData.BulkInsertStoredProcedureReturnsUpdateDate = true;
			metaData.BulkUpdateStoredProcedureReturnsUpdateDate = true;

			metaData.DBTableTypeName = "TT_BOELaborType";
			metaData.StoredProcedureTableTypeParameterName = "@BOELaborType";

			metaData.EntityPropertiesToMapToDataTable = new Collection<string>()
			{
				"BOELaborTypeID", "UpdateDT", "ResourceID", "PerformingOrganizationID", "BOELaborTypeStartDate", "BOELaborTypeEndDate",
				"SpreadCurveID", "PercentSpread", "ValueSpread", "BOETaskElementID", "SpreadTypeID", "PercentSpreadLocked", "HourSpreadLocked",
				"WBSID", "CLINID", "CanOffload", "LaborSortId", "BRCResourceID"
			};

			return metaData;
		}

		/// <summary>
		/// Override to allow us to log.. feel free to remove this
		/// </summary>
		public override IDictionary<int, int> BulkSave(ICollection<ResourceTypeDto> dtosToSave)
		{
			if (dtosToSave == null) { throw new ArgumentNullException(nameof(dtosToSave)); }

			this.Log.Debug(string.Format("ResourceTypeLoader.BulkSave => Ntid: {2}, BoeId: {1}, Count: {0}",
				dtosToSave.Count, dtosToSave.First().BoeID, System.Threading.Thread.CurrentPrincipal.Identity.Name));

			foreach (ResourceTypeDto aDto in dtosToSave)
			{
				aDto.StartDateValue = aDto.StartDateValue.Normalize();
				aDto.EndDateValue = aDto.EndDateValue.Normalize();

				this.Log.Debug(string.Format("ResourceTypeLoader.BulkSave => Item Id: {0}, Value: {1}", aDto.Id, aDto.ValueSpread));
			}

			return base.BulkSave(dtosToSave);
		}

		/// <summary>
		/// Converts the DTO into an entity.
		/// </summary>
		/// <param name="dtoToConvert">dto to convert</param>
		/// <returns>entity representing the dto</returns>
		protected override BOELaborType ConvertDtoToEntity(ResourceTypeDto dtoToConvert)
		{
			BOELaborType entity = new BOELaborType();

			if (dtoToConvert == null)
			{
				throw new ArgumentNullException(nameof(dtoToConvert));
			}

			entity.BOELaborTypeID = dtoToConvert.Id;
			entity.PerformingOrganizationID = dtoToConvert.PerformingOrgID;
			entity.ResourceID = dtoToConvert.ResourceID;
			entity.ValueSpread = dtoToConvert.ValueSpread;
			entity.SpreadCurveID = dtoToConvert.SpreadCurveID.HasValue ? (int)dtoToConvert.SpreadCurveID.Value : 0;
			entity.BOELaborTypeStartDate = dtoToConvert.StartDate.Value;
			entity.BOELaborTypeEndDate = dtoToConvert.EndDate.Value;
			entity.BOETaskElementID = dtoToConvert.TaskElementId;
			entity.UpdateDT = dtoToConvert.UpdateDate;
			entity.PercentSpread = dtoToConvert.PercentSpread;
			entity.PercentSpreadLocked = dtoToConvert.PercentSpreadLocked;
			entity.HourSpreadLocked = dtoToConvert.HourSpreadLocked;
			entity.SpreadTypeID = (int)dtoToConvert.SpreadType;
			entity.WBSID = dtoToConvert.WBSID;
			entity.CLINID = dtoToConvert.CLINID;
			entity.CanOffload = dtoToConvert.CanOffload;
			entity.LaborSortId = dtoToConvert.LaborTypeOrder;
			entity.BRCResourceID = dtoToConvert.BusinessResourceCodeID;

			return entity;
		}
	}
}
