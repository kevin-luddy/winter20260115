// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView.Workspace
{
    using IES.Common;

    /// <summary>
    /// Model View for Workspace Email Overrides
    /// </summary>
    public class WorkspaceEmailOverrideModelView
    {
        public WorkspaceEmailOverrideModelView()
        {
            this.Id = -1;
            this.Subject = string.Empty;
            this.Body = string.Empty;
        }

        /// <summary>
        /// The subject line in the email, may contain replacement tokens such as {0}
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// The body of the email, may contain replacement tokens such as {0}
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// Gets or sets the trigger for this email (text shown to user)
        /// </summary>
        public string Trigger { get; set; }

        /// <summary>
        /// Gets or sets the recipient of the email.
        /// </summary>
        public string Recipient { get; set; }

        /// <summary>
        /// Gets or sets the category.
        /// </summary>
        public string Category { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether system value is forced.
        /// </summary>
        public bool SystemForced { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether system default is On or Off.
        /// </summary>
        public bool SystemDefaultOn { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether workspace is override to On or Off.
        /// </summary>
        public bool? WorkspaceOverrideOn { get; set; }

        /// <summary>
        /// Gets or sets the type of the email.
        /// </summary>
        public EmailTypes EmailType { get; set; }

        /// <summary>
        /// Gets or sets the identifier used for the Workspace Override.
        /// </summary>
        public int Id { get; set; }
    }
}
