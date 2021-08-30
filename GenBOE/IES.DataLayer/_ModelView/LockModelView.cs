// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.DataBridge.ModelViews
{
    using System;
    using System.Web.Configuration;
    using IES.Common;

    /// <summary>
    /// A Model View class for locking a document for edit.
    /// </summary>
    public class LockModelView : UpdateableDTO
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LockModelView"/> class.
        /// </summary>
        public LockModelView()
        {
            this.Id = -1;
            this.IsReadOnly = true;
            this.IsLockAllowed = false;
            this.InUse = null;
            this.Editing = string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LockModelView"/> class.
        /// </summary>
        /// <param name="isReadOnly">true if read-only access allowed; false if edit access allowed.</param>
        /// <param name="isLockAllowed">Used to determine when to display the "Enable Edit" button. True if user has edit privileges and document is not locked by another user; false otherwise.</param>
        /// <param name="inUse">DateTime stamp when user locked document for edit; null if not locked for edit.</param>
        /// <param name="editing">User who is currently editing the document; null if not locked for edit.</param>
        public LockModelView(bool isReadOnly, bool isLockAllowed, DateTime? inUse, string editing)
        {
            this.IsReadOnly = isReadOnly;
            this.IsLockAllowed = isLockAllowed;
            this.InUse = inUse;
            this.Editing = editing;
        }

        /// <summary>
        /// Gets the number of minutes a lock may be held before displaying a timeout warning message.
        /// </summary>
        public int EditLockTimeoutWarningMinutes
        {
            get
            {
                return Convert.ToInt32(WebConfigurationManager.AppSettings["EditLockTimeoutWarningMinutes"]);
            }
        }

        /// <summary>
        /// Gets the number of minutes a lock may be held before timout due to inactivity.
        /// </summary>
        public int EditLockTimeoutExpirationMinutes
        {
            get
            {
                return Convert.ToInt32(WebConfigurationManager.AppSettings["EditLockTimeoutExpirationMinutes"]);
            }
        }
        
        /// <summary>
        /// True if read-only access is allowed; False if edit access is allowed.
        /// </summary>
        public bool IsReadOnly { get; set; }

        /// <summary>
        /// Used to determine when to display the "Enable Edit" button. True if current user has edit privileges and document is not locked by another user.
        /// </summary>
        public bool IsLockAllowed { get; set; }

        /// <summary>
        /// Gets or sets the DateTime stamp when user locked document for edit.
        /// </summary>
        public DateTime? InUse { get; set; }

        /// <summary>
        /// Gets or sets the full name of the user who is currently editing the document; null if not locked for edit.
        /// </summary>
        public string Editing { get; set; }
    }
}
