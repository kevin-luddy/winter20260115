// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Core
{
    using System;
    using System.Globalization;

    /// <summary>
    /// Base class for the Updateable DTOs
    /// </summary>
    [Serializable]
    public abstract class UpdateableDTO : IUpdateableDTO
    {
        /// <summary>
        /// Unique Id for the DTO
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Default Ctor
        /// </summary>
        protected UpdateableDTO()
        {
            this.UpdateDate = DateTime.MinValue;
            this.Updateable = UpdateType.None;
        }

        /// <summary>
        /// Last Update Date
        /// </summary>
        public DateTime UpdateDate { get; set; }

        /// <summary>
        /// Gets or sets the update date as a long.
        /// </summary>
        public string UpdateDateLong
        {
            get
            {
                return this.UpdateDate.Ticks.ToString();
            }
            set
            {
                long val;
                if (long.TryParse(value, out val))
                {
                    this.UpdateDate = new DateTime(val);
                }
            }
        }

        /// <summary>
        /// Update Type
        /// </summary>
        public UpdateType Updateable { get; set; }

        /// <summary>
        /// Updates the id (if its a new id) and the update date (if provided). Updateable is reset to none. 
        /// </summary>
        /// <param name="Id">New id value</param>
        /// <param name="newUpdateDate">new update date</param>
        /// <exception cref="ArgumentException">Thrown if the newOrExistingId is less than 0 if it's new or does not match the dto's existing id.</exception>
        /// <exception cref="InvalidOperationException">Thrown if update type is not 'Upsert'</exception>
        public void Update(int newOrExistingId, DateTime? newUpdateDate)
        {
            //check input args
            if (newOrExistingId < 0)
            {
                throw new ArgumentException("Cannot perform Update because the new Id is not a valid value. Id = " + newOrExistingId.ToString(), nameof(newOrExistingId));
            }
            if (this.Updateable != UpdateType.Upsert)
            {
                throw new InvalidOperationException("Cannot perform Update on DTO with update type of :" + this.Updateable.ToString());
            }
            if (!System.Diagnostics.Debugger.IsAttached && newUpdateDate.HasValue && this.UpdateDate > newUpdateDate.Value)
            {
                throw new ArgumentException($"Cannot perform Update when new Update Date {newUpdateDate?.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)} is older than Original Update Date {this.UpdateDate.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)} for DTO Id: {this.Id.ToString()}", nameof(newUpdateDate));
            }
            if (this.Updateable == UpdateType.Upsert && this.Id > 0 && this.Id != newOrExistingId)
            {
                throw new ArgumentException("Cannot perform Update when the existing Id does not match the Id argument:" + this.Id.ToString(), nameof(newOrExistingId));
            }

            if (this.Id < 0)
            {
                this.Id = newOrExistingId;
            }
            
            // Now propagate the new parent Id to child DTOs so they identify the correct 'parent'
            // For example, task element Id must be identified on ordinary variables, task variables, etc.
            this.PropagateNewParentIdToChildDTOs(newOrExistingId);

            if (newUpdateDate.HasValue)
            {
                this.UpdateDate = newUpdateDate.Value;
            }

            // reset the Update status back to none 
            this.Updateable = UpdateType.None;
        }

        /// <summary>
        /// Propagates the new 'parent' DTO Id to all 'child' DTOs in collections. This is a placeholder and must be implemented by child DTOs.
        /// </summary>
        /// <param name="newParentId">new id of the parent DTO</param>
        /// <exception cref="NotImplementedException">Thrown because the child DTO needs to implement this method.</exception>
        virtual protected void PropagateNewParentIdToChildDTOs(int newParentId)
        {
            throw new NotImplementedException("This method needs to be implemented by your DTO.");
        }
    }

    /// <summary>
    /// Update Type Enum
    /// </summary>
    public enum UpdateType
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0,

        /// <summary>
        /// Upsert
        /// </summary>
        Upsert = 1,

        /// <summary>
        /// Delete
        /// </summary>
        Deleted = 2
    }
}
