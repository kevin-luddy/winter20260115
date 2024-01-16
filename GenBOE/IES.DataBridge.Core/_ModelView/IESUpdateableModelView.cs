// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.DataBridge.ModelViews
{
	using System;
	using IES.Common.Core.Enums;
	using IES.Common.Core.Interfaces;
	using IES.Common.Core.Models;
	using Newtonsoft.Json;

	/// <summary>
	/// RDM base Model View. 
	/// Currently trying to transition ModelView common behaviors to this base class.
	/// This can be used as the base class for anything coming back from the client
	/// that may be dirty.
	/// </summary>
	[Serializable]
    public abstract class IESUpdateableModelView : IUpdateableDTO
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IESUpdateableModelView"/> class.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        protected IESUpdateableModelView()
        {
            this.UpdateDate = DateTime.MinValue;
            this.Updateable = UpdateType.None;
        }

        /// <summary>
        /// Gets or sets the Dirty flag.
        /// </summary>
        [JsonProperty(PropertyName = "D")]
        public bool Dirty { get; set; }

        /// <summary>
        /// Unique Id for the DTO
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Last Update Date
        /// </summary>
        public virtual DateTime UpdateDate { get; set; }

        /// <summary>
        /// Update Type
        /// </summary>
        public virtual UpdateType Updateable { get; set; }

        /// <summary>
        /// Updates the id (if its a new id) and the update date (if provided). Updateable is reset to none. 
        /// </summary>
        /// <param name="newOrExistingId">New id value</param>
        /// <param name="newUpdateDate">new update date</param>
        /// <exception cref="ArgumentException">Thrown if the newOrExistingId is less than 0 if it's new or does not match the dto's existing id.</exception>
        /// <exception cref="InvalidOperationException">Thrown if update type is not 'Upsert'</exception>
        public void Update(int newOrExistingId, DateTime? newUpdateDate)
        {
            // check input args
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
                throw new ArgumentException("Cannot perform Update when new Update Date is older than original update date for DTO Id: " + this.Id.ToString(), nameof(newUpdateDate));
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
        protected virtual void PropagateNewParentIdToChildDTOs(int newParentId)
        {
            throw new NotImplementedException("This method needs to be implemented by your DTO.");
        }
    }
}