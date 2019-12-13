// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2014 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenBOE.ActionLogic
{
    /// <summary>
    /// Controller logic interface for the base controller logic class
    /// </summary>
    public interface IGenBOEControllerLogic
    {
        /// <summary>
        /// Gets the company specific Piwik site id
        /// </summary>
        int PiwikSiteID { get; }

        /// <summary>
        /// Gets the company specific genBoe URL
        /// </summary>
        Uri SiteURL { get; }
    }
}
