// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.DataBridge.DTO
{
    using System;
    using IES.Core;

    /// <summary>
    /// Permission Dto
    /// </summary>
    [Serializable]
    public class PermissionDto : IES.Core.UpdateableDTO, IES.Core.Interfaces.ICachableDTO
    {
        /// <summary>
        /// default constructor
        /// </summary>
        public PermissionDto()
        {
            this.Id = -1;
            this.UserId = int.MinValue;
            this.Role = PtmRole.NotSet;
            this.UpdateDate = DateTime.MinValue;
        }

        /// <summary>
        /// for ICachableDTO..
        /// </summary>
        /// <returns>Primary Key</returns>
        public int GetPrimaryKeyID()
        {
            return this.Id;
        }

        /// <summary>
        /// User Id
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Role
        /// </summary>
        public PtmRole Role { get; set; }
    }
}
