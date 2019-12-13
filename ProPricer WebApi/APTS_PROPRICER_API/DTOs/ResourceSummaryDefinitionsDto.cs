using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace APTSPropricerApi.DTOs
{
    public class ResourceSummaryDefinitionsDto
    {
        public String id { get; set; }
        public String name { get; set; }
        public String dataType { get; set; }
        public Byte maxLength { get; set; }
        public String decimals { get; set; }
        public String defaultValue { get; set; }
        public Boolean validate { get; set; }
        public Boolean required { get; set; }
    }
}