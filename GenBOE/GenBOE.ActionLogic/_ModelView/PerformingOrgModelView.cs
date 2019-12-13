using GenBOE.Dtos;
using System;

namespace GenBOE.ActionLogic.ModelView
{

    public class PerformingOrgModelView
    {
        public PerformingOrgModelView()
        {
            this.PerformingOrgID = 0;
            this.PerformingOrgName = string.Empty;
            this.PerformingOrgDesc = string.Empty;
        }
        /// <summary>
        /// Passes in a PerformingOrgDto and get a PerformingOrgModelView
        /// </summary>
        /// <param name="org">PerformingOrgDto to convert</param>
        public PerformingOrgModelView(PerformingOrgDTO org)
        {
            if (org == null)
            {
                throw new ArgumentNullException(nameof(org));
            }
            this.PerformingOrgID = org.Id;
            this.PerformingOrgName = org.PerformingOrgName;
            this.PerformingOrgDesc = org.PerformingOrgDesc;
        }

        public int PerformingOrgID { get; set; }

        public string PerformingOrgName { get; set; }

        public string PerformingOrgDesc{ get; set; }
    }
}
