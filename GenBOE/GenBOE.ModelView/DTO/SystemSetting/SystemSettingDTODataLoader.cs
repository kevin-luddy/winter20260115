// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
    using System;
    using System.Linq;
    using IES.Common;
    using GenBOE.Models;
    using GenBOE.Dtos;
    using System.Collections.ObjectModel;
    using System.Collections.Generic;

    public class SystemSettingDTODataLoader : GenBOE.DataBridge.DTO.ISystemSettingDTODataLoader
    {
        private Logger _log = new Logger(typeof(SystemSettingDTODataLoader));
        
		/// <summary>
        /// Cache Object
        /// </summary>
        private MemoryCache cache;

        /// <summary>
        /// The number of seconds to store in cache
        /// </summary>
        private int secondsToCache = 300;

        /// <summary>
        /// Cache key for System data
        /// </summary>
        private string cacheKeySystem = "SystemSettingDataLoader_";

		/// <summary>
		/// Default ctor for SystemSettingDTODataLoader
		/// </summary>
        public SystemSettingDTODataLoader() 
		{
			this.cache = new MemoryCache();
		}

        /// <summary>
        /// Get all system setting DTOs
        /// </summary>
        /// <returns>All system settings</returns>
        virtual public ICollection<SystemSettingDTO> GetSystemSettings()
        {
            ICollection<SystemSettingDTO> toReturn = new Collection<SystemSettingDTO>();
            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    toReturn =
                       (from ss in gbe.SystemSettings
                        select new SystemSettingDTO
                        {
                            Key = ss.Key,
                            Value = ss.Value
                        }
                        ).OrderBy(ss => ss.Key).ToList();
                }
            }

            return toReturn;
        }


		virtual public ICollection<SystemSettingDTO> GetSkillMixSettings()
		{
			ICollection<SystemSettingDTO> toReturn = new Collection<SystemSettingDTO>();
			using (StopwatchTimer sw = new StopwatchTimer(this._log))
			{
				using (GenBoeEntities gbe = new GenBoeEntities())
				{
					toReturn =
					   (from ss in gbe.SystemSettings
						where ss.Key.Contains(Constants.SKILL_MIX_WHITELIST) || ss.Key.Contains(Constants.ENABLE_SKILL_MIX_WHITELIST)
						select new SystemSettingDTO
						{
							Key = ss.Key,
							Value = ss.Value
						}
						).OrderBy(ss => ss.Key).ToList();
				}
			}

			return toReturn;
		}

		/// <summary>
		/// Get the system setting DTO
		/// </summary>
		/// <param name="key">system setting key to retrieve</param>
		/// <returns>system setting value</returns>
		[DbQuery]
        virtual public SystemSettingDTO GetSystemSetting(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            SystemSettingDTO toReturn = null;
            string cacheKey = this.cacheKeySystem + key;

            // load the setting from cache. if the key does not exist then null is returned
            toReturn = (SystemSettingDTO)this.cache.GetData(cacheKey);

            // if the setting is not found in the cache, try to get it from the database
            if (toReturn == null)
            {
                using (StopwatchTimer sw = new StopwatchTimer(this._log))
                {
                    using (GenBoeEntities gbe = new GenBoeEntities())
                    {
                        SystemSettingDTO SystemSetting =
                           (from r in gbe.SystemSettings
                            where r.Key == key
                            select new SystemSettingDTO
                            {
                                Key = r.Key,
                                Value = r.Value
                            }).FirstOrDefault();

                        toReturn = SystemSetting;
                    }
                }

                if (toReturn != null)
                {
                    this.cache.Add(cacheKey, toReturn, secondsToCache);
                }
            }

           return toReturn;
        }

        #region Commit


        /// <summary>
        /// Save the System Setting
        /// </summary>
        /// <param name="systemSetting">system setting to save</param>
        virtual public string SaveSystemSetting(SystemSettingDTO systemSetting)
        {
            if (systemSetting == null)
            {
                throw new ArgumentNullException(nameof(systemSetting));
            }

            string key = string.Empty;

            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    key = gbe.upsertSystemSetting(systemSetting.Key, systemSetting.Value).FirstOrDefault();
                }
            }

            if (key == systemSetting.Key)
            {
                // Update the Cache
                string cacheKey = this.cacheKeySystem + key;
                this.cache.Add(cacheKey, systemSetting, secondsToCache);
            }

            return key;
        }

        /// <summary>
        /// Clear (delete) the system setting
        /// </summary>
        /// <param name="key">system setting key to clear</param>
        virtual public void ClearSystemSetting(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    gbe.deleteSystemSetting(key);
                }
            }

            // Update the Cache
            string cacheKey = this.cacheKeySystem + key;
            this.cache.Remove(cacheKey);
        }
        #endregion Commit
    }
}
