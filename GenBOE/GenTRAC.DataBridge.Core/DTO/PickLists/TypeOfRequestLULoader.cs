// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.DataBridge.Core.DTO.PickLists
{
	using System.Collections.Generic;
	using System.Linq;
	using GenTRAC.Models;
	using IES.Common.Core.PickList;
	using IES.Common.Core.Utilities;
	using Microsoft.Extensions.Logging;

	/// <summary>
	/// Proposal LU Loader
	/// </summary>
	public class TypeOfRequestLULoader : PickListLoader
	{
		/// <summary>
		/// Default Ctor
		/// </summary>
		public TypeOfRequestLULoader(ILogger<TypeOfRequestLULoader> logger) : base(logger)
		{
		}

		/// <summary>
		/// Gets the corresponding LU table
		/// </summary>
		/// <returns>Request Types</returns>
		public override ICollection<PickListDto> GetPickListValues()
		{
			ICollection<PickListDto> result = new List<PickListDto>();

			using (StopwatchTimer sw = new StopwatchTimer("TypeOfRequestLULoader.GetProposalTypes", Log))
			{
				using (genTRACEntities dbModel = new genTRACEntities())
				{
					result = dbModel.RequestTypeLUs.Select(x => new PickListDto()
					{
						Id = x.RequestTypeID,
						Text = x.RequestType,
						IsActive = x.IsActive,
						InUse = x.Proposals.Any()
					}).ToList();
				}
			}

			return result;
		}

		/// <summary>
		/// Upsert
		/// </summary>
		/// <param name="dtoToUpsert">Dto that will be upserted</param>
		/// <returns>New Id</returns>
		protected override int? Upsert(PickListDto dtoToUpsert)
		{
			int? toReturn = null;

			using (StopwatchTimer sw = new StopwatchTimer("TypeOfRequestLULoader.Upsert", Log))
			{
				if (dtoToUpsert != null)
				{
					using (genTRACEntities dbModel = new genTRACEntities())
					{
						string text = dtoToUpsert.Text;

						// if we are updating & the item is in use, then we need to make sure we only modify the IsActive (in other words, we need to keep the same text..)
						if (dtoToUpsert.InUse && dtoToUpsert.Id > 0)
						{
							text = dbModel.RequestTypeLUs.First(x => x.RequestTypeID == dtoToUpsert.Id).RequestType;
						}

						toReturn = dbModel.upsertRequestTypes(dtoToUpsert.Id, text, dtoToUpsert.IsActive).First();
					}
				}
			}

			return toReturn;
		}

		/// <summary>
		/// Delete
		/// </summary>
		/// <param name="dtoToDelete">Dto To Delete</param>
		/// <returns>Id of object being deleted</returns>
		protected override int? Delete(PickListDto dtoToDelete)
		{
			int? toReturn = null;

			using (StopwatchTimer sw = new StopwatchTimer("TypeOfRequestLULoader.Delete", Log))
			{
				if (dtoToDelete != null && !dtoToDelete.InUse)
				{
					toReturn = dtoToDelete.Id;

					using (genTRACEntities dbModel = new genTRACEntities())
					{
						dbModel.deleteTypeOfRequest(dtoToDelete.Id);
					}
				}
			}

			return toReturn;
		}
	}
}
