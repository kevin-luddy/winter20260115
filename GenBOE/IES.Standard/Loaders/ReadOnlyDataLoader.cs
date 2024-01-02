// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Standard
{
    using System.Collections.Generic;
    using System.Linq;
	using Microsoft.Extensions.Logging;

	/// <summary>
	/// Read Only Data Loader base class
	/// </summary>
	public abstract class ReadOnlyDataLoader<TDtoType> : IReadOnlyDataLoader<TDtoType>
        where TDtoType : IUpdateableDTO
    {
        /// <summary>
        /// Logger
        /// </summary>
        protected ILogger Log { get; private set; }

        /// <summary>
        /// Default Constructor
        /// </summary>
        protected ReadOnlyDataLoader(ILogger logger)
        {
            this.Log = logger;
        }

        /// <summary>
        /// Get item by Id
        /// </summary>
        /// <param name="id">Item Id</param>
        /// <returns>Item by the primary key</returns>
        public TDtoType GetById(int id)
        {
            ICollection<TDtoType> result = this.GetByIds(new List<int>() { id });

            return result == null ? default(TDtoType) : result.FirstOrDefault();
        }

        /// <summary>
        /// Gets all objects based on the IDs that were passed in
        /// </summary>
        /// <param name="ids">IDs</param>
        /// <returns>Corresponding Data</returns>
        public abstract ICollection<TDtoType> GetByIds(ICollection<int> ids);
    }
}
