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
	using IES.Common.Core;
	using IES.Common.Core.Enums;
	using IES.Common.Core.PickList;
	using IES.Common.Core.Utilities;
	using Microsoft.Extensions.Logging;

	/// <summary>
	/// ContractTypeLULoader
	/// </summary>
	public class ContractTypeLULoader : PickListLoader
	{
		/// <summary>
		/// Default Ctor
		/// </summary>
		public ContractTypeLULoader(ILogger<ContractTypeLULoader> logger) : base(logger)
		{
		}

		/// <summary>
		/// Gets the corresponding LU table
		/// </summary>
		/// <returns>Levels Of Commitment</returns>
		public override ICollection<PickListDto> GetPickListValues()
		{
			ICollection<PickListDto> result = new List<PickListDto>();

			using (StopwatchTimer sw = new StopwatchTimer(Log))
			{
				using (genTRACEntities dbModel = new genTRACEntities())
				{
					result = dbModel.ContractTypeLUs.Select(x => new PickListDto()
					{
						Id = x.ContractTypeID,
						Text = x.ContractType,
						IsActive = x.IsActive,
						InUse = x.Proposals.Any(),
						IenumParentIds = x.ContractTypeGroupLUs.Select(g => g.ContractTypeGroupID)
					}).OrderBy(p => p.Text).ToList();
				}
			}

			result.AsParallel().ForAll(
				ct =>
				{
					ct.ParentIds = ct.IenumParentIds.ToCollection(); ct.IenumParentIds = null;
				});

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

			using (StopwatchTimer sw = new StopwatchTimer("ContractTypeLULoader.Upsert", Log))
			{
				if (dtoToUpsert != null)
				{
					using (genTRACEntities dbModel = new genTRACEntities())
					{
						string text = dtoToUpsert.Text;

						// if we are updating & the item is in use, then we need to make sure we only modify the IsActive (in other words, we need to keep the same text..)
						if (dtoToUpsert.InUse && dtoToUpsert.Id >= 0)
						{
							text = dbModel.ContractTypeLUs.First(x => x.ContractTypeID == dtoToUpsert.Id).ContractType;
						}

						string parentIds = string.Join(",", dtoToUpsert.ParentIds.Select(i => i.ToString()));

						toReturn = dbModel.upsertContractType(dtoToUpsert.Id, text, dtoToUpsert.IsActive, parentIds).First();
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

			using (StopwatchTimer sw = new StopwatchTimer("ContractTypeLULoader.Delete", Log))
			{
				if (dtoToDelete != null && !dtoToDelete.InUse)
				{
					toReturn = dtoToDelete.Id;

					using (genTRACEntities dbModel = new genTRACEntities())
					{
						dbModel.deleteContractType(dtoToDelete.Id);
					}
				}
			}

			return toReturn;
		}

		/// <summary>
		/// Gets the parent pick list type, if applicable.
		/// </summary>
		public override PickListEnum? ParentPickList
		{
			get
			{
				return PickListEnum.ContractTypeGroup;
			}
		}

		/// <summary>
		/// Gets a value indicating whether this Pick List allows multiple parents.
		/// </summary>
		public override bool AllowsMultipleParents
		{
			get
			{
				return true;
			}
		}
	}
}
