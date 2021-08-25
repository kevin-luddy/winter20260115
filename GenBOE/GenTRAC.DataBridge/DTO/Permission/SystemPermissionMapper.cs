// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.DataBridge.DTO
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using IES.Common;

    /// <summary>
    /// System Permissions Dto Data Mapper
    /// </summary>
    public class SystemPermissionMapper : IInternalSystemPermissionMapper
    {
        /// <summary>
        /// Logger
        /// </summary>
        private Logger log = new Logger(typeof(SystemPermissionMapper));

        /// <summary>
        /// System Permission Loader
        /// </summary>
        protected ISystemPermissionLoader SystemPermissionLoader { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="systemPermissionLoader">System Permission Loader</param>
        public SystemPermissionMapper(ISystemPermissionLoader systemPermissionLoader)
        {
            this.SystemPermissionLoader = systemPermissionLoader;
        }

        /// <summary>
        /// Get items by Id
        /// </summary>
        /// <param name="itemId">Item Id.</param>
        /// <returns>Dto for the corresponding Id.</returns>
        public SystemPermissionDto GetById(int itemId)
        {
            if (itemId < 0)
            {
                // a null is returned here to prevent going to cache/db for a negative id which should never exist
                return null;
            }

            SystemPermissionDto result;

            using (StopwatchTimer sw = new StopwatchTimer("PermissionMapper.GetById", this.log))
            {
                result = this.SystemPermissionLoader.GetById(itemId);
            }

            return result;
        }

        /// <summary>
        /// Returns a list of permissions DTOs on the system level
        /// </summary>
        /// <returns>Permission DTO collection</returns>
        public ICollection<SystemPermissionDto> GetSystemPermissions()
        {
            ICollection<SystemPermissionDto> toReturn = new List<SystemPermissionDto>();

            using (StopwatchTimer sw = new StopwatchTimer("PermissionMapper.GetSystemPermissions", this.log))
            {
                ICollection<int> permissionIds = this.SystemPermissionLoader.GetAllIds();
                toReturn = this.GetDtos(permissionIds);
            }

            return toReturn;
        }

        /// <summary>
        /// Returns a list of permissions for a single user
        /// </summary>
        /// <param name="userId">User Id</param>
        /// <returns>List of permissions</returns>
        public ICollection<SystemPermissionDto> GetByUserId(int userId)
        {
            List<SystemPermissionDto> toReturn = new List<SystemPermissionDto>();

            using (StopwatchTimer sw = new StopwatchTimer("PermissionMapper.GetSystemPermissions", this.log))
            {
                ICollection<int> systemPermissionIds = this.SystemPermissionLoader.GetIdsByUserId(userId);
                toReturn.AddRange(this.GetDtos(systemPermissionIds));
            }

            return toReturn;
        }

        /// <summary>
        /// Saves a permissionsDto, returning the post save permissionsDto.
        /// </summary>
        /// <param name="permission">Permissions to save</param>
        /// <returns>Id of the saved Permissions</returns>
        int? IInternalSystemPermissionMapper.Save(SystemPermissionDto permission)
        {
            int? toReturn = null;

            using (StopwatchTimer sw = new StopwatchTimer("PermissionMapper.SavePermissionsDto", this.log))
            {
                if (permission == null)
                {
                    throw new ArgumentNullException(nameof(permission));
                }

                toReturn = this.SystemPermissionLoader.Save(permission);
            }

            return toReturn;
        }

        /// <summary>
        /// Returns a collection of DTOs from a collection of IDs
        /// </summary>
        /// <param name="ids">IDs to get</param>
        /// <returns>Collection of DTOs</returns>
        protected ICollection<SystemPermissionDto> GetDtos(ICollection<int> ids)
        {
            List<SystemPermissionDto> toReturn = new List<SystemPermissionDto>();

            if (ids != null && ids.Any())
            {
                using (StopwatchTimer sw = new StopwatchTimer("PermissionMapper.GetDtos", this.log))
                {
                    toReturn = this.SystemPermissionLoader.GetByIds(ids).ToList();
                }
            }

            return toReturn;
        }
    }
}
