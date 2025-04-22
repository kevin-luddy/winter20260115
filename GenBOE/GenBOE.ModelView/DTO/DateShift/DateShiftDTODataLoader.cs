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

			this.Save(dateShiftDTOs);
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

		public override Dictionary<int, int> Save(ICollection<DateShiftDTO> dtosToSave)
		{
			if (dtosToSave == null)
			{
				throw new ArgumentNullException(nameof(dtosToSave));
			}

			Dictionary<int, int> toReturn = new Dictionary<int, int>();

			try
			{
				// Create a data table to hold the date shift data
				DataTable dataTable = new DataTable();
				dataTable.Columns.Add("Level", typeof(string));
				dataTable.Columns.Add("Id", typeof(int));
				dataTable.Columns.Add("StartDate", typeof(DateTime));
				dataTable.Columns.Add("EndDate", typeof(DateTime));
				dataTable.Columns.Add("BOEStateID", typeof(int));
				dataTable.Columns.Add("UpdateDT", typeof(DateTime));

				// Add the date shift data to the data table
				foreach (DateShiftDTO dto in dtosToSave)
				{
					dataTable.Rows.Add(dto.DateShiftLevelValue.ToString(), dto.Id, dto.StartDate, dto.EndDate, null, DateTime.Now);
				}

				// Execute the stored procedure to update the date shifts
				using (DbContext objectContext = new DbContext(Constants.BOE_DB_CONTEXT_NAME))
				{
					ICollection<KeyValuePair<int, DateTime?>> updateResult = StoredProcedureHelper.ExecuteTableValueProcedure(
						objectContext,
						dataTable,
						"updateDateShiftviaTableParameter",
						"@DateShifts",
						"TT_DateShift",
						false);

					// Get the updated IDs and update dates
					for (int i = 0; i < dtosToSave.Count; i++)
					{
						DateShiftDTO dto = dtosToSave.ElementAt(i);
						if (updateResult.Any())
						{
							KeyValuePair<int, DateTime?> kvp = updateResult.ElementAt(i);
							toReturn.Add(dto.BoeId, kvp.Key);
						}
					}
				}
			}
			catch (SqlException ex)
			{
				Log.Error(ex);
				throw new GeneralAppException("There was an error updating from a Date shift.  Contact a system administrator for assistance.");
			}

			return toReturn;
		}
		#endregion
	}
}