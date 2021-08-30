// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.ActionLogic.Mediator
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using GenTRAC.DataBridge.DTO;

    /// <summary>
    /// Permissions mediator class
    /// </summary>
    public class SystemPermissionMediator : ISystemPermissionMediator 
    {
        /// <summary>
        /// Permissions Mapper
        /// </summary>
        private IInternalSystemPermissionMapper permissionMapper = null;

        /// <summary>
        /// Constructor, requires a reference to the internal mapper
        /// </summary>
        /// <param name="inPermissionsMapper">Permissions mapper</param>
        public SystemPermissionMediator(ISystemPermissionMapper inPermissionsMapper)
        {
            this.permissionMapper = inPermissionsMapper as IInternalSystemPermissionMapper;
        }

        /// <summary>
        /// Saves a collection of permissionsDto
        /// </summary>
        /// <param name="inPermissionDtos">Permissions to save</param>
        /// <returns>Collection of the DTOs of the saved Permissions</returns>
        public ICollection<SystemPermissionDto> SavePermissionDtos(ICollection<SystemPermissionDto> inPermissionDtos)
        {
            if (inPermissionDtos == null)
            {
                throw new ArgumentNullException(nameof(inPermissionDtos));
            }

            Collection<SystemPermissionDto> toReturn = new Collection<SystemPermissionDto>();

            foreach (SystemPermissionDto permission in inPermissionDtos)
            {
                SystemPermissionDto afterSave = this.SavePermissionDto(permission);

                if (afterSave != null)
                {
                    toReturn.Add(afterSave);
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Saves a permission dto
        /// </summary>
        /// <param name="inPermissionDto">dto to save</param>
        /// <returns>saved dto</returns>
        public SystemPermissionDto SavePermissionDto(SystemPermissionDto inPermissionDto)
        {
            // Pre save business logic
            if (inPermissionDto == null)
            {
                throw new ArgumentNullException(nameof(inPermissionDto));
            }

            // Save
            int? toReturn = this.permissionMapper.Save(inPermissionDto);

            // Return
            // TODO: Evalutate this, possibly returning int? instead.
            return this.permissionMapper.GetById(toReturn.Value);
        }
    }
}