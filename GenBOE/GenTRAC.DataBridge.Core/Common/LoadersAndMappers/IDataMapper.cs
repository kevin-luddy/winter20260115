// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.DataBridge.Core.Common.LoadersAndMappers
{
	using IES.Common.Core.Models;

	/// <summary>
	/// IDataMapper interface.
	/// </summary>
	public interface IDataMapper<TDtoType> where TDtoType : UpdateableDTO
	{
		/// <summary>
		/// Clear cache keys
		/// </summary>
		/// <param name="dto">Dto for which the keys should be cleared</param>
		void ClearCacheKeys(TDtoType dto);

		/// <summary>
		/// Get items by Capture Id
		/// </summary>
		/// <param name="itemId">Item Id.</param>
		/// <returns>Dto for the corresponding Id.</returns>
		TDtoType GetById(int itemId);
	}

	/// <summary>
	/// The internal interface for saves
	/// </summary>
	/// <typeparam name="TDtoType">Type of dto to use</typeparam>
	internal interface IInternalDataMapper<TDtoType> : IDataMapper<TDtoType>
		where TDtoType : UpdateableDTO
	{
		/// <summary>
		/// Save the Dto, and return it back.
		/// </summary>
		/// <param name="dto">Dto to save.</param>
		/// <returns>Id of the item that was saved.</returns>
		int? Save(TDtoType dto);
	}
}
