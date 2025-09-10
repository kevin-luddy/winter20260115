// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.Core.Loaders.SystemSetting
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Linq;
	using GenBOE.DataBridge.Core.Common;
	using GenBOE.DataBridge.Core.DTO;
	using GenBOE.Models;
	using IES.Common.Core.Interfaces;
	using IES.Common.Core.Utilities;
	using Microsoft.Extensions.Logging;

	public class SystemSettingDTODataLoader : ISystemSettingDTODataLoader
	{
		private ILogger _log;

		/// <summary>
		/// Cache Object
		/// </summary>
		private ICacheService cache;

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
		public SystemSettingDTODataLoader(ILogger<SystemSettingDTODataLoader> logger, ICacheService cacheService)
		{
			this.cache = cacheService;
			this._log = logger;
		}

		/// <summary>
		/// Get all system setting DTOs
		/// </summary>
		/// <returns>All system settings</returns>
		public virtual ICollection<SystemSettingDTO> GetSystemSettings()
		{
			ICollection<SystemSettingDTO> toReturn = new Collection<SystemSettingDTO>();
			using (StopwatchTimer sw = new StopwatchTimer(_log))
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

		/// <summary>
		/// Get Skill Mix Settings
		/// </summary>
		/// <returns>All Skill Mix Settings</returns>
		public virtual ICollection<SystemSettingDTO> GetSkillMixSettings()
		{
			ICollection<SystemSettingDTO> toReturn = new Collection<SystemSettingDTO>();
			using (StopwatchTimer sw = new StopwatchTimer(_log))
			{
				using (GenBoeEntities gbe = new GenBoeEntities())
				{
					toReturn =
					   (from ss in gbe.SystemSettings
						where ss.Key.Contains(SystemSettingConstants.SKILL_MIX_BLACKLIST)
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
		public virtual SystemSettingDTO GetSystemSetting(string key)
		{
			if (string.IsNullOrEmpty(key))
			{
				throw new ArgumentNullException(nameof(key));
			}

			SystemSettingDTO toReturn = null;
			string cacheKey = cacheKeySystem + key;

			// load the setting from cache. if the key does not exist then null is returned
			toReturn = (SystemSettingDTO)cache.GetData(cacheKey);

			// if the setting is not found in the cache, try to get it from the database
			if (toReturn == null)
			{
				using (StopwatchTimer sw = new StopwatchTimer(_log))
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
					cache.Add(cacheKey, toReturn, secondsToCache);
				}
			}

			return toReturn;
		}

		#region Commit


		/// <summary>
		/// Save the System Setting
		/// </summary>
		/// <param name="systemSetting">system setting to save</param>
		public virtual string SaveSystemSetting(SystemSettingDTO systemSetting)
		{
			if (systemSetting == null)
			{
				throw new ArgumentNullException(nameof(systemSetting));
			}

			string key = string.Empty;

			using (StopwatchTimer sw = new StopwatchTimer(_log))
			{
				using (GenBoeEntities gbe = new GenBoeEntities())
				{
					key = gbe.upsertSystemSetting(systemSetting.Key, systemSetting.Value).FirstOrDefault();
				}
			}

			if (key == systemSetting.Key)
			{
				// Update the Cache
				string cacheKey = cacheKeySystem + key;
				cache.Add(cacheKey, systemSetting, secondsToCache);
			}

			return key;
		}

		/// <summary>
		/// Clear (delete) the system setting
		/// </summary>
		/// <param name="key">system setting key to clear</param>
		public virtual void ClearSystemSetting(string key)
		{
			if (string.IsNullOrEmpty(key))
			{
				throw new ArgumentNullException(nameof(key));
			}

			using (StopwatchTimer sw = new StopwatchTimer(_log))
			{
				using (GenBoeEntities gbe = new GenBoeEntities())
				{
					gbe.deleteSystemSetting(key);
				}
			}

			// Update the Cache
			string cacheKey = cacheKeySystem + key;
			cache.Remove(cacheKey);
		}
		#endregion Commit
	}
}
