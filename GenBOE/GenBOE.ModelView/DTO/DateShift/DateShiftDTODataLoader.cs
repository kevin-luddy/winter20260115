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
		/// Update the respective levels in the database with the Date Shift data.
		/// </summary>
		/// <param name="dateShiftDTOs">Collection of Date Shift dates for updating.</param>
		public void Update(ICollection<DateShiftDTO> dateShiftDTOs)
		{
			if (dateShiftDTOs == null)
			{
				throw new ArgumentNullException(nameof(dateShiftDTOs));
			}

			foreach (DateShiftDTO dateShiftDTO in dateShiftDTOs)
			{
				dateShiftDTO.Updateable = UpdateType.Upsert;
			}

			this.UpdateDateShifts(dateShiftDTOs);
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

		public virtual int? UpdateDateShifts(ICollection<DateShiftDTO> dateShifts)
		{
			int? toReturn = null;
			if (dateShifts == null || dateShifts.Count == 0) { throw new ArgumentNullException(nameof(dateShifts)); }

			using (StopwatchTimer sw = new StopwatchTimer(this.Log))
			{
				Collection<string> dateShiftPropertiesToIncludeInTable = new Collection<string>()
				{
					"Level", "Id", "StartDate", "EndDate", "BOEStateID", "UpdateDate"
				};

				using (DbContext objectContext = new DbContext(Constants.BOE_DB_CONTEXT_NAME))
				{
					DataTable dateShiftsDataTable = StoredProcedureHelper.ToDataTable<DateShiftDTO>(dateShifts, dateShiftPropertiesToIncludeInTable);

					// Convert the Level and BOEStateID columns to integers
					foreach (DataRow row in dateShiftsDataTable.Rows)
					{
						row["Level"] = (int)row["Level"];
						row["BOEStateID"] = (int)row["BOEStateID"];
					}

					toReturn = StoredProcedureHelper.ExecuteTableValueProcedure(
						objectContext,
						dateShiftsDataTable,
						"updateDateShiftviaTableParameter",
						"@DateShifts",
						"TT_DateShift",
						false
					).FirstOrDefault().Key;
				}
				return toReturn;
			}
		}
		#endregion
	}
}