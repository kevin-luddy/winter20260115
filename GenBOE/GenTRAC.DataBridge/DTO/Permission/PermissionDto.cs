// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.DataBridge.DTO
{
    using System;
    using IES.Common;

    /// <summary>
    /// Permission Dto
    /// </summary>
    [Serializable]
    public class PermissionDto : IES.Common.UpdateableDTO, IES.Common.Interfaces.ICachableDTO
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
