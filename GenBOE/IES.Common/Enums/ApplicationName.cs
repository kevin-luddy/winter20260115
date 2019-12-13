// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common
{
    using System.ComponentModel;

    /// <summary>
    /// Used to specify application name for Who's Online
    /// </summary>
    public enum ApplicationName
    {
        [Description("RDM")]
        RDM,
        [Description("RDSB")]
        RDSB
    }
}
