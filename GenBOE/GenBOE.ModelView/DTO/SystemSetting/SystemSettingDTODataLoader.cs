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
        public SystemSettingDTODataLoader() { }

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

            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    var SystemSetting =
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
        }
        #endregion Commit
    }
}
