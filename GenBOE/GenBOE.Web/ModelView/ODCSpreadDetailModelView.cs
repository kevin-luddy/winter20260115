using System;
using System.Collections.ObjectModel;
using IES.Common;
using GenBOE.DataBridge.DTO;
using GenBOE.Dtos;

namespace GenBOE.Web.ModelView
{

    public class ODCSpreadDetailModelView
    {
        public ODCSpreadDetailModelView()
        {
            ODCTypeID = -1;
            ResourceID = null;
            Cost = 0;
            ODCSpreadCurveID = SpreadCurves.DiscreteHours;
            StartDate = DateTime.MinValue;
            EndDate = DateTime.MinValue;
            ODCSpreads = new Collection<ODCSpreadModelView>();
            ResourceID = string.Empty;
            PerformingOrgID = string.Empty;
        }

        public ODCSpreadDetailModelView(
            OtherDirectCostType inODCType,
            IResourceDTODataLoader inIResourceDTODataLoader,
            IPerformingOrgDTODataLoader perfOrgLoader)
            : this()
        {
            if (inIResourceDTODataLoader == null)
            {
                throw new ArgumentNullException(nameof(inIResourceDTODataLoader));
            }
            if (perfOrgLoader == null)
            {
                throw new ArgumentNullException(nameof(perfOrgLoader));
            }

            if (inODCType != null)
            {
                ODCTypeID = inODCType.ODCTypeID;
                Cost = inODCType.Cost;
                StartDate = inODCType.StartDate.HasValue ? GenBOEUtilities.AdjustDateTimePrecision(inODCType.StartDate.Value, DateTimePrecision.Month) : inODCType.StartDate;
                EndDate = inODCType.EndDate.HasValue ? GenBOEUtilities.AdjustDateTimePrecision(inODCType.EndDate.Value, DateTimePrecision.Month)  : inODCType.EndDate;
                ODCSpreadCurveID = inODCType.SpreadCurve;

                // get Resource Name
                if (inODCType.ResourceID.HasValue)
                {
                    ResourceDTO resource = inIResourceDTODataLoader.GetById(inODCType.ResourceID.Value);
                    ResourceID = resource.ResourceName;
                }

                // get performingorgid
                if (inODCType.PerformingOrgID.HasValue)
                {
                    PerformingOrgDTO performOrg = perfOrgLoader.GetById(inODCType.PerformingOrgID.Value);
                    PerformingOrgID = performOrg.PerformingOrgName;
                }

                // add labor spreads
                foreach (OtherDirectCostSpread LS in inODCType.ODCSpreads)
                {
                    ODCSpreads.Add(new ODCSpreadModelView(LS));
                }
               
            }
        }

        public int? ODCTypeID { get; set; }

        public SpreadCurves ODCSpreadCurveID { get; set; }

        public decimal? Cost { get; set; }

        public string ResourceID { get; set; }

        public string PerformingOrgID { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public Collection<ODCSpreadModelView> ODCSpreads { get; set; }

    }// LaborSpreadDetailModelView
}
