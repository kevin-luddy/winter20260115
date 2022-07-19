// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2022 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.DataBridge.DTO
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;
	using GenTRAC.DataBridge.DTO.Contracts;
	using GenTRAC.Models;
    using IES.Common;

	/// <summary>
	/// Cage Codes Loader
	/// </summary>
	public class CageCodesLoader : DataLoader<CageCodesDTO>, ICageCodesLoader
	{
		public override ICollection<CageCodesDTO> GetByIds(ICollection<int> ids)
		{
			throw new NotImplementedException();
		}

		protected override int? Delete(CageCodesDTO dtoToDelete)
		{
			throw new NotImplementedException();
		}

		protected override int? Upsert(CageCodesDTO dtoToUpsert)
		{
			throw new NotImplementedException();
		}
	}
}
