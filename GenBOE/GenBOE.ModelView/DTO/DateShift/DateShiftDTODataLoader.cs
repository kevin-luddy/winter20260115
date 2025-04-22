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
    using System.Text;
    using IES.Common;
    using GenBOE.Dtos;
    using GenBOE.Models;

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
	}
}