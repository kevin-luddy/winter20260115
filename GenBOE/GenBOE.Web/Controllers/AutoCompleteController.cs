using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;
using GenBOE.ActionLogic.ControllerLogic;
using GenBOE.Business.Metrics;
using GenBOE.Common;
using GenBOE.DataBridge.Common;
using GenBOE.DataBridge.Common.Interfaces;
using GenBOE.DataBridge.DTO;
using GenBOE.Dtos;
using GenBOE.Objects;
using GenBOE.Web.Common;
using GenBOE.Web.ModelView;

namespace GenBOE.Web.Controllers
{
    public class AutoCompleteController : GenBOEController
    {
        private Logger _log = new Logger(typeof(AutoCompleteController));

        private ResourceDTODataLoader _ResourceDTODataLoader = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="inSecurityAccess">Access to the security APIs</param>
        public AutoCompleteController(ISecurityAccess inSecurityAccess,
            CommonDataMapper inCommonDataMapper,
            ResourceDTODataLoader inResourceDTODataLoader,
            SiteMasterUtilities inSiteMasterUtilities,
            SystemMetrics inSystemMetrics,
            IFullObjectFactory factory,
            IUserDTODataLoader userLoader,
            IPermissionsDTODataLoader permissionLoader,
            IGenBOEControllerLogic inControllerLogic
            )
            : base(inSecurityAccess, inCommonDataMapper, inSiteMasterUtilities, inSystemMetrics, factory, userLoader, permissionLoader, inControllerLogic)
        {
            _ResourceDTODataLoader = inResourceDTODataLoader;
        }

        #region Actions

        virtual public ActionResult AutoCompleteResources(string workspace, int boeID, ElementOfCostType? EOC, SegmentType? Segment, string searchTerm)
        {
            FullWorkspace thisWS = this.Factory.CreateFullWorkspace(workspace);
                        
            Stopwatch sw = InitializeAction(_log, "AutoCompleteResources", SecurityPage.BoeLaborTypes, SecurityAuthorization.Read, thisWS, boeID);

            ICollection<ResourceDTO> resourceDTOData = _ResourceDTODataLoader.GetByListIdAndElementOfCost(thisWS.ResourceListID, new Collection<ElementOfCostType>() { ElementOfCostType.LMLabor, ElementOfCostType.IWTA, ElementOfCostType.Sub, ElementOfCostType.ODC, ElementOfCostType.Materials, ElementOfCostType.Travel });

            resourceDTOData = (from resource in resourceDTOData
                              where (!EOC.HasValue || EOC.Value == ElementOfCostType.NotSet || resource.ElementOfCost == EOC) // filter element of cost
                               && (!Segment.HasValue || Segment.Value == SegmentType.None || resource.Segment == Segment) // filter segment
                               && resource.ResourceDesc.ContainsEquivalent(searchTerm)
                              select resource).ToList();
            
            Collection<ResourceDataForGridsModelView> resourceData = new Collection<ResourceDataForGridsModelView>();
            foreach (ResourceDTO resource in resourceDTOData)
            {
                ResourceDataForGridsModelView resourceMV = new ResourceDataForGridsModelView();
                resourceMV.ResourceID = resource.Id;
                resourceMV.ElementOfCost = resource.ElementOfCost;
                resourceMV.Segment = resource.Segment;
                resourceMV.SpreadText = resource.ResourceName;
                resourceMV.RateType = resource.RateType;

                // Labor/IWTA/Sub uses Labor Type only in the BOE Summary and the Resoruce Code should only show Description
                if (resourceMV.ElementOfCost == ElementOfCostType.LMLabor || resourceMV.ElementOfCost == ElementOfCostType.IWTA || resourceMV.ElementOfCost == ElementOfCostType.Sub
                    || resourceMV.ElementOfCost == ElementOfCostType.ODC || resourceMV.ElementOfCost == ElementOfCostType.Materials || resourceMV.ElementOfCost == ElementOfCostType.Travel)
                {
                    resourceMV.ResourceCode = resource.ResourceDesc;
                    resourceMV.BOESummaryText = resource.LaborType;
                }
                resourceData.Add(resourceMV);
            }

            IOrderedEnumerable<ResourceDataForGridsModelView> orderedResourceData = resourceData.OrderBy(x => x.ResourceCode);
            resourceData = orderedResourceData.ToCollection();
            FinalizeAction(_log, "AutoCompleteResources", sw);
            return Json(resourceData);
        }

        #endregion Actions
    }
}
