using EBS.ProPricer.Data;
using EBS.ProPricer.Model;
using EBS.ProPricer.Model.General;
using APTSPropricerApi.Connection;
using APTSPropricerApi.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace APTSPropricerApi.Controllers
{
    public class ResourceSummaryDefinitionsController : ApiController
    {
         readonly IProPricerConnection ppc;

        PoolManager poolManager = PoolManager.Instance;

        public ResourceSummaryDefinitionsController()                //IProPricerConnection ppc)
        {
            this.ppc = (IProPricerConnection)(poolManager.GetObjectsFromPool());
            //  this.ppc = ppc;
        }

        protected override void Dispose(bool disposing)
        {
            poolManager.GiveObjectBackToPool(ppc);
            base.Dispose(disposing);
        }

        // GET api/ResourceSummaryDefinitions
        /// <summary>
        /// Returns the list of resources summary definitions in the instance of PROPRICER.
        /// </summary>
        /// <returns>Returns a collection of resources summary definitions from the instance of PROPRICER.</returns>
        public IEnumerable<ResourceSummaryDefinitionsDto> Get()
         {
             List<ResourceSummaryDefinitionsDto> resdfl = new List<ResourceSummaryDefinitionsDto>();
             if (ppc.workspace != null)
             {
                ppc.workspace.ResourceSummaryDefinitions.Open();

                foreach (var rsd in ppc.workspace.ResourceSummaryDefinitions)
                 {
                     ResourceSummaryDefinitionsDto rsddto = new ResourceSummaryDefinitionsDto();
                     rsddto.id = rsd.Id.ToString();
                     rsddto.name = rsd.Name;
                     rsddto.dataType = rsd.Type.ToString();
                     resdfl.Add(rsddto);
                 }
                ppc.workspace.ResourceSummaryDefinitions.Close();
             }

             return resdfl;
         }

    }
}
