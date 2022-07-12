// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Web.ModelView
{
    /// <summary>
    /// Model View for My Info page
    /// </summary>
    public class MyInfoModelView
    {
        /// <summary>
        /// Gets or sets the Display Name
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the Nt Id
        /// </summary>
        public string NtId { get; set; }

        /// <summary>
        /// Gets or sets the IES Bearer Token
        /// </summary>
        public string BearerToken { get; set; }
    }
}