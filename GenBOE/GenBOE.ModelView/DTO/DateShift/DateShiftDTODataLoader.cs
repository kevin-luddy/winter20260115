// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
	using GenBOE.Dtos;
	using IES.Common;
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Data;
	using System.Data.Entity;
	using System.Linq;

	/// <summary>
	/// Date shift DTO data loader.
	/// </summary>
	public class DateShiftDTODataLoader : DataLoader<DateShiftDTO>, IDateShiftDTODataLoader
	{
		/// <summary>
		/// Resource spread loader.
		/// </summary>
		private readonly IResourceSpreadLoader resourceSpreadLoader;

		/// <summary>
		/// Skill mix loader.
		/// </summary>
		private readonly ISkillMixDTOLoader skillMixLoader;

		/// <summary>
		/// Common discolsure loader.
		/// </summary>
		private readonly ICommonDisclosureSMDTODataLoader commonDisclosureLoader;

		/// <summary>
		/// Default constructor
		/// </summary>
		public DateShiftDTODataLoader(IResourceSpreadLoader resourceSpreadLoader, ISkillMixDTOLoader skillMixLoader, ICommonDisclosureSMDTODataLoader commonDisclosureLoader)
		{
			this.Log = new Logger(typeof(DateShiftDTODataLoader));
			this.resourceSpreadLoader = resourceSpreadLoader;
			this.skillMixLoader = skillMixLoader;
			this.commonDisclosureLoader = commonDisclosureLoader;
		}

		/// <summary>
		/// Updates the date shifted objects in the database.
		/// </summary>
		/// <param name="dateShiftDTOs">Date shifted DTOs.</param>
		public void Update(ICollection<DateShiftDTO> dateShiftDTOs)
		{
			if (dateShiftDTOs == null)
			{
				throw new ArgumentNullException(nameof(dateShiftDTOs));
			}

			List<DateShiftDTO> dateShiftDTOsToUpdate = new List<DateShiftDTO>();

			foreach (DateShiftDTO dateShiftDTO in dateShiftDTOs)
			{
				dateShiftDTOsToUpdate.AddRange(RecursivelyGetDateShiftDTOs(dateShiftDTO.OriginalObject as IDateShiftable));
			}

			foreach (DateShiftDTO dateShiftDTOtoUpdate in dateShiftDTOsToUpdate)
			{
				// Update any labor spreads.
				if (dateShiftDTOtoUpdate.LaborSpreads != null && dateShiftDTOtoUpdate.Level == (int)Level.Labor)
				{
					resourceSpreadLoader.BulkSave(dateShiftDTOtoUpdate.LaborSpreads);
				}

				// Update any skill mixes and common disclosure for BOE Task Elements.
				if (dateShiftDTOtoUpdate.BOETaskElementId != null && dateShiftDTOtoUpdate.Level == (int)Level.Task)
				{
					if (dateShiftDTOtoUpdate.SkillMixTable != null && dateShiftDTOtoUpdate.SkillMixTable.Any())
					{
						List<SkillMixDTO> dtos = new List<SkillMixDTO>();
						foreach (SkillMixModelView skillMixModelView in dateShiftDTOtoUpdate.SkillMixTable)
						{
							SkillMixDTO dto = skillMixModelView.ToDto();
							dto.BOEID = dateShiftDTOtoUpdate.BoeId;
							dto.BOETaskElementID = dateShiftDTOtoUpdate.BOETaskElementId.Value;
							dtos.Add(dto);
						}

						this.skillMixLoader.InsertSkillMix(dtos);
					}
					else
					{
						// If the dto has no skill mix tables, it's possible they were cleared out, so make sure old data is deleted
						this.skillMixLoader.DeleteSkillMixByBOETaskElementID(dateShiftDTOtoUpdate.BOETaskElementId.Value);
					}

					// Save the Common Disclosure DTOs
					if (dateShiftDTOtoUpdate.CommonDisclosureTable != null && dateShiftDTOtoUpdate.CommonDisclosureTable.Any())
					{
						List<CommonDisclosureSkillMixDTO> dtos = new List<CommonDisclosureSkillMixDTO>();
						foreach (CommonDisclosureModelView commonDisclosure in dateShiftDTOtoUpdate.CommonDisclosureTable)
						{
							CommonDisclosureSkillMixDTO dto = commonDisclosure.ToDto();
							dto.BOEID = dateShiftDTOtoUpdate.BoeId;
							dto.BOETaskElementID = dateShiftDTOtoUpdate.BOETaskElementId.Value;
							dtos.Add(dto);
						}

						this.commonDisclosureLoader.InsertCommonDisclosureSM(dtos);
					}
					else
					{
						// If the dto has no common disclosure tables, it's possible they were cleared out, so make sure old data is deleted
						this.commonDisclosureLoader.DeleteCommonDisclosureSkillMixByBOETaskElementID(dateShiftDTOtoUpdate.Id);
					}
				}
			}

			// Update the dates in the database for all shifted items.
			this.UpdateDateShifts(dateShiftDTOsToUpdate);
		}

		#region Inherited
		/// <summary>
		/// Not implemented and not needed.
		/// </summary>
		/// <exception cref="NotImplementedException"></exception>
		public override ICollection<DateShiftDTO> GetByIds(ICollection<int> ids)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Not implemented and not needed.
		/// </summary>
		/// <exception cref="NotImplementedException"></exception>
		protected override int? Delete(DateShiftDTO dtoToDelete)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Not implemented and not needed.
		/// </summary>
		/// <exception cref="NotImplementedException"></exception>
		protected override int? Upsert(DateShiftDTO dtoToUpsert)
		{
			throw new NotImplementedException();
		}
		#endregion

		/// <summary>
		/// Update date shifts in the database.
		/// </summary>
		/// <param name="dateShifts">Dateshift objects</param>
		public virtual void UpdateDateShifts(ICollection<DateShiftDTO> dateShifts)
		{
			if (dateShifts == null || dateShifts.Count == 0) { throw new ArgumentNullException(nameof(dateShifts)); }

			using (StopwatchTimer sw = new StopwatchTimer(this.Log))
			{
				Collection<string> dateShiftPropertiesToIncludeInTable = new Collection<string>()
				{
					"Level", "Id", "StartDate", "EndDate", "UpdateDate"
				};

				using (DbContext objectContext = new DbContext(Constants.BOE_DB_CONTEXT_NAME))
				{
					DataTable dateShiftsDataTable = StoredProcedureHelper.ToDataTable<DateShiftDTO>(dateShifts, dateShiftPropertiesToIncludeInTable);

					StoredProcedureHelper.ExecuteTableValueProcedure(
						objectContext,
						dateShiftsDataTable,
						"updateDateShiftviaTableParameter",
						"@DateShifts",
						"TT_DateShift",
						false
					);
				}
			}
		}

		/// <summary>
		/// Gets the date shift child DTOs recursively.
		/// </summary>
		/// <param name="dateShiftable">Date shiftable.</param>
		/// <returns>List of date shift dtos that were in the child property.</returns>
		private List<DateShiftDTO> RecursivelyGetDateShiftDTOs(IDateShiftable dateShiftable)
		{
			if (dateShiftable == null)
			{
				throw new ArgumentNullException(nameof(dateShiftable));
			}

			List<DateShiftDTO> dateShiftDTOs = new List<DateShiftDTO>();

			DateShiftDTO dateShiftDTO = DateShiftDTO.FromIDateShiftable(dateShiftable);

			if (dateShiftDTO != null)
			{
				dateShiftDTOs.Add(dateShiftDTO);
			}

			if (dateShiftable.Children != null)
			{
				foreach (IDateShiftable child in dateShiftable.Children)
				{
					dateShiftDTOs.AddRange(RecursivelyGetDateShiftDTOs(child));
				}
			}

			return dateShiftDTOs;
		}
	}
}