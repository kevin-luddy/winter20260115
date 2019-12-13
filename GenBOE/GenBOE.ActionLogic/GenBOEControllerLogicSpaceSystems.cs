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
    /// Controller logic class for the base controller
    /// </summary>
    public class GenBOEControllerLogicSpaceSystems : GenBOEControllerLogic
    {
        /// <summary>
        /// Gets the company specific Piwik site id
        /// </summary>
        public override int PiwikSiteID
        {
            get
            {
                return 266;
            }
        }

        /// <summary>
        /// Gets the company specific genBoe URL that we want to collect metrix for
        /// </summary>
        public override Uri SiteURL
        {
            get
            {
                // we only want to collect metrics from production so we don't use the ServerURL key in the web.config
                return new Uri("https://genboe.ssc.lmco.com");
            }
        }
    }
}
