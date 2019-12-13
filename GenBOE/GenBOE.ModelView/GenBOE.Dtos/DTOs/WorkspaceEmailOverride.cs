// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Dtos
{
    using IES.Common;

    /// <summary>
    /// An Email Override for a Workspace.
    /// </summary>
    /// <seealso cref="IES.Common.UpdateableDTO" />
    public class WorkspaceEmailOverrideDTO : UpdateableDTO
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkspaceEmailOverrideDTO"/> class.
        /// </summary>
        public WorkspaceEmailOverrideDTO()
        {
            this.Id = -1;
        }

        /// <summary>
        /// Gets or sets the type of the email.
        /// </summary>
        public EmailTypes EmailType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to set the email to on or off as an override from System Default.
        /// </summary>
        public bool TurnOn { get; set; }
    }
}
