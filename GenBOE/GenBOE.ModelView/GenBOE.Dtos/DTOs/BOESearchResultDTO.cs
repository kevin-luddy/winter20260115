// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Dtos
{
    /// <summary>
    /// BOE search result DTO
    /// </summary>
    public class BOESearchResultDTO
    { 
        /// <summary>
        /// Gets or sets BOEID
        /// </summary>
        public int BOEID { get; set; }

        /// <summary>
        /// Gets or sets WorkspaceID
        /// </summary>
        public int WorkspaceID { get; set; }

        /// <summary>
        /// Gets or sets the project map identifier.
        /// </summary>
        public int ProjectMapId { get; set; }
    }
}