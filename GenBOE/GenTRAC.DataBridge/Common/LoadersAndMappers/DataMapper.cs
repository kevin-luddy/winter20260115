// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.DataBridge.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using IES.Common;

    /// <summary>
    /// Data Mapper abstract class
    /// </summary>
    public abstract class DataMapper<TDtoType, TLoaderType> : IInternalDataMapper<TDtoType> where TDtoType : UpdateableDTO where TLoaderType : IDataLoader<TDtoType>
    {
        #region Properties

        /// <summary>
        /// Logger
        /// </summary>
        protected Logger Log { get; set; }

        /// <summary>
        /// Data loader for the mapper
        /// </summary>
        protected TLoaderType DataLoader { get; set; }

        /// <summary>
        /// Cache loader
        /// </summary>
        protected ICacheDataLoader CacheLoader { get; set; }

        /// <summary>
        /// Cache key used for Get By Id
        /// </summary>
        protected string CacheKeyForGetById { get; set; }

        #endregion

        /// <summary>
        /// Default Constructor
        /// </summary>
        protected DataMapper()
        {
        }

        /// <summary>
        /// Save the Dto, and return its id back.
        /// </summary>
        /// <param name="dto">Dto to save.</param>
        /// <returns>Id of the item that was saved.</returns>
        int? IInternalDataMapper<TDtoType>.Save(TDtoType dto)
        {
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto));
            }

            int? toReturn = null;

            using (StopwatchTimer sw = new StopwatchTimer("Save", this.Log))
            {
                int? result = this.DataLoader.Save(dto);

                this.ClearCacheKeys(dto);

                toReturn = result;
            }

            return toReturn;
        }

        /// <summary>
        /// Get items by Capture Id
        /// </summary>
        /// <param name="itemId">Item Id.</param>
        /// <returns>Dto for the corresponding Id.</returns>
        public TDtoType GetById(int itemId)
        {
            if (itemId < 0)
            {
                return null;
            }

            TDtoType result;

            using (StopwatchTimer sw = new StopwatchTimer("DataMapper.GetById", this.Log))
            {
                GetDtoByIdDelegate cacheDelegate = new GetDtoByIdDelegate(this.DataLoader.GetById);
                result = this.CacheLoader.GetData(cacheDelegate, new object[] { itemId }, this.CacheKeyForGetById + itemId) as TDtoType;
            }

            return result;
        }

        /// <summary>
        /// Returns a collection of DTOs from a collection of IDs
        /// </summary>
        /// <param name="ids">IDs to get</param>
        /// <returns>Collection of DTOs</returns>
        protected ICollection<TDtoType> GetDtos(ICollection<int> ids)
        {
            if (ids == null || !ids.Any())
            {
                return new List<TDtoType>();
            }

            ICollection<TDtoType> result = new List<TDtoType>();

            using (StopwatchTimer sw = new StopwatchTimer("DataMapper.GetDtos", this.Log))
            {
                Dictionary<string, int> cacheKeyToIDDictionary = new Dictionary<string, int>();
                foreach (int id in ids)
                {
                    cacheKeyToIDDictionary.Add(this.CacheKeyForGetById + id, id);
                }

                GetDtosByIdsDelegate<TDtoType> cacheDelegate = new GetDtosByIdsDelegate<TDtoType>(this.DataLoader.GetByIds);
                var tempResult = this.CacheLoader.GetData(cacheDelegate, this.CacheKeyForGetById, cacheKeyToIDDictionary);

                if (tempResult != null)
                {
                    foreach (TDtoType dto in tempResult)
                    {
                        result.Add(dto);
                    }
                }
            }

            return result;
        }

        #region Abstract Methods

        /// <summary>
        /// Cleare cache keys.. You need to execute this one, when you add onto it
        /// </summary>
        /// <param name="dto">Dto for which the keys should be cleared</param>
        public abstract void ClearCacheKeys(TDtoType dto);

        #endregion
    }
}
