// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Dtos
{
    public class RteCustomTemplateSourceModelView
    {
        /// <summary>
        /// Gets or sets the Source Id.
        /// </summary>
        public int SourceId { get; set; }

        /// <summary>
        /// Gets or sets the Description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets whether this is for Tasks only or for BOEs.
        /// </summary>
        public bool TaskOnly { get; set; }
    }
}
