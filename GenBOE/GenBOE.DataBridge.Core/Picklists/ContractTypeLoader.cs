// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.Core.Picklists
{
	using System.Collections.Generic;
	using System.Linq;
	using GenBOE.Models;
	using IES.Common.Core;
	using IES.Common.Core.PickList;
	using IES.Common.Core.Utilities;
	using Microsoft.Extensions.Logging;

	/// <summary>
	/// ContractTypeLULoader
	/// </summary>
	public class ContractTypeLoader : PickListLoader
	{
		/// <summary>
		/// Default Ctor
		/// </summary>
		public ContractTypeLoader(ILogger<ContractTypeLoader> logger) : base(logger)
		{
		}

		/// <summary>
		/// Gets the corresponding LU table
		/// </summary>
		/// <returns>Levels Of Commitment</returns>
		public override ICollection<PickListDto> GetPickListValues()
		{
			ICollection<PickListDto> result = new List<PickListDto>();

			using (StopwatchTimer sw = new(Log))
			{
				using (GenBoeEntities dbModel = new())
				{
					result = dbModel.ContractTypeLUs.Select(x => new PickListDto()
					{
						Id = x.ContractTypeID,
						Text = x.ContractType,
						IsActive = x.IsActive,
						InUse = x.CLINs.Any() || x.WorkspaceContractTypeXREFs.Any()
					}).OrderBy(p => p.Text).ToList();
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

			using (StopwatchTimer sw = new("ContractTypeLULoader.Upsert", Log))
			{
				if (dtoToUpsert != null)
				{
					using (GenBoeEntities dbModel = new())
					{
						string text = dtoToUpsert.Text;

						// if we are updating & the item is in use, then we need to make sure we only modify the IsActive (in other words, we need to keep the same text..)
						if (dtoToUpsert.InUse && dtoToUpsert.Id >= 0)
						{
							text = dbModel.ContractTypeLUs.First(x => x.ContractTypeID == dtoToUpsert.Id).ContractType;
						}

						toReturn = dbModel.upsertContractType(dtoToUpsert.Id, text, dtoToUpsert.IsActive).First();
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

			using (StopwatchTimer sw = new("ContractTypeLULoader.Delete", Log))
			{
				if (dtoToDelete != null && !dtoToDelete.InUse)
				{
					toReturn = dtoToDelete.Id;

					using (GenBoeEntities dbModel = new())
					{
						dbModel.deleteContractType(dtoToDelete.Id);
					}
				}
			}

			return toReturn;
		}
	}
}
