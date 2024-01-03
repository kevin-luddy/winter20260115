// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2022 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Core
{
    using System.Collections.Generic;
    
    /// <summary>
    /// Data structure to return for AJAX calls.
    /// </summary>
    /// <typeparam name="T">The type of data being returned</typeparam>
    public class IESResponse<T>
    {
        /// <summary>
        /// True if the request succeeded, false otherwise.
        /// </summary>
        public bool IsSuccessful { get; set; }

        /// <summary>
        /// Error or other informational messages
        /// </summary>
        public ICollection<string> Messages { get; set; } = new List<string>();

        /// <summary>
        /// Collection of data meeting the request criteria
        /// </summary>
        public ICollection<T> Data { get; set; } = new List<T>();
    }
}
