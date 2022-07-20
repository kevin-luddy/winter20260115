// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2022 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.DataBridge.DTO
{
	using System.Collections.Generic;
	using System.Linq;
	using GenTRAC.DataBridge.DTO.Contracts;
	using GenTRAC.Models;
	using IES.Common;

	/// <summary>
	/// Cage Codes Loader
	/// </summary>
	public class CageCodesLoader : ICageCodesLoader
	{
		/// <summary>
		/// Logger
		/// </summary>
		protected Logger Log { get; set; }

		/// <summary>
		/// Constructor
		/// </summary>
		public CageCodesLoader()
		{
			this.Log = new Logger(typeof(CageCodesLoader));
		}

		/// <summary>
		/// Get All Cage Codes
		/// </summary>
		/// <returns>All Cage Codes</returns>
		public ICollection<CageCodesDTO> GetAllCageCodesData()
		{
			ICollection<CageCodesDTO> toReturn = new List<CageCodesDTO>();

            using (StopwatchTimer sw = new StopwatchTimer("CageCodes.GetAllCageCodesData", Log))
            {
                using (genTRACEntities dbModel = new genTRACEntities())
                {
                    toReturn = dbModel.CageCodes
                        .Select(x => new CageCodesDTO()
                        {
                            CageCode = x.CageCode1,
                            Address1 = x.Address1,
							Address2 = x.Address2,
							City = x.City,
							State = x.State,
							Zip = x.Zip
                        }).ToList();
                }
            }

            return toReturn;
        }

		/// <summary>
		/// Gets the data associated to a cage code by a given cage code.
		/// </summary>
		/// <param name="cageCode">Cage Code</param>
		/// <returns>Specified cage code data</returns>
		public CageCodesDTO GetDataByCageCode(string cageCode)
		{
			CageCodesDTO toReturn = new CageCodesDTO();

			using (StopwatchTimer sw = new StopwatchTimer("CageCodes.GetDataByCageCode", Log))
			{
				using (genTRACEntities dbModel = new genTRACEntities())
				{
					toReturn = dbModel.CageCodes
						.Select(x => new CageCodesDTO()
						{
							CageCode = x.CageCode1,
							Address1 = x.Address1,
							Address2 = x.Address2,
							City = x.City,
							State = x.State,
							Zip = x.Zip
						}).FirstOrDefault(x => x.CageCode == cageCode);
				}
			}

			return toReturn;
		}
	}
}