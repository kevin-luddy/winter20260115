// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
	using GenBOE.Dtos;
	using IES.Common;
	using IES.Common.Exceptions;
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Data;
	using System.Data.Entity;
	using System.Data.SqlClient;
	using System.Linq;

	public class DateShiftDTODataLoader : DataLoader<DateShiftDTO>, IDateShiftDTODataLoader
	{
		/// <summary>
		/// Default constructor
		/// </summary>
		public DateShiftDTODataLoader()
		{
			this.Log = new Logger(typeof(DateShiftDTODataLoader));
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

			this.UpdateDateShifts(dateShiftDTOsToUpdate);
		}

		#region Inheritted
		/// <summary>
		/// Not implemented and not needed.
		/// </summary>
		/// <param name="ids"></param>
		/// <returns></returns>
		/// <exception cref="NotImplementedException"></exception>
		public override ICollection<DateShiftDTO> GetByIds(ICollection<int> ids)
		{
			throw new NotImplementedException();
		}

		protected override int? Delete(DateShiftDTO dtoToDelete)
		{
			throw new NotImplementedException();
		}

		protected override int? Upsert(DateShiftDTO dtoToUpsert)
		{
			throw new NotImplementedException();
		}

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
		#endregion

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