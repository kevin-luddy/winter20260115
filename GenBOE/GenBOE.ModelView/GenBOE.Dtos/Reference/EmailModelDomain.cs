// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Dtos
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics.CodeAnalysis;
    using IES.Common;

    /// <summary>
    /// Class representing emails for the system
    /// </summary>
    [ExcludeFromCodeCoverage]
    [Serializable()]
    public class EmailModelDomain
    {
        /// <summary>
        /// default constructor
        /// </summary>
        public EmailModelDomain()
        {
            Subject = string.Empty;
            Body = string.Empty;
        }

        /// <summary>
        /// The ID of the email
        /// </summary>
        [Required]
        public EmailTypes EmailType { get; set; }

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
        /// Gets or sets a value indicating whether the default for workspaces is for this email to be on.
        /// </summary>
        [Required]
        public bool DefaultOn { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this email is forced on for all workspaces.
        /// </summary>
        [Required]
        public bool Forced { get; set; }

        /// <summary>
        /// Gets or sets the recipient of the email.
        /// </summary>
        public string Recipient { get; set; }

        /// <summary>
        /// Gets or sets the category.
        /// </summary>
        public string Category { get; set; }
    }
}
