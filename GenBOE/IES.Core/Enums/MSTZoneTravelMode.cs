using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

 
namespace GenBOE.Common
{
    using System.ComponentModel;

    /// <summary>
    /// 4 Modes for MST Travel (2 ZONE/2 NON-ZONE)
    /// </summary>
    public enum MSTZoneTravelMode
    {
        [Description("Domestic – No Airfare")]
        DOMESTIC_NO_AIRFARE,

        [Description("Domestic Round Trip Airfare")]
        DOMESTIC_AIRFARE,

        [Description("Domestic – Non Zone")]
        DOMESTIC_NON_ZONE,

        [Description("International")]
        INTERNATIONAL
    }
}
