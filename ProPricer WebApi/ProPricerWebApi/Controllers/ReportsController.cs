/*
    Copyright 2016-2020 Lockheed Martin Corporation.

    This computer software has been provided in confidence, and contains trade secret and/or privileged or confidential 
    commercial or financial information. Public disclosure of any information marked as indicated above is prohibited 
    by the Trade Secrets Act (18 U.S.C. Sec. 1905) and the Economic Espionage Act of 1996 (18 U.S.C. Sec. 1831 et seq.) 
    and is not to be made available to third parties without the prior written permission of Lockheed Martin Corporation.
*/

using APTSPropricerApi.Connection;
using EBS.ProPricer.Model.General;
using Microsoft.AspNetCore.Mvc;

namespace APTSPropricerApi.Controllers
{
    /// <summary>
    /// Reports.
    /// </summary>
    public class ReportsController : ProPricerController
    {
        private readonly PoolManagerList poolManagerList;

        /// <summary>
        /// #ctor
        /// </summary>
        public ReportsController(ILogger<ReportsController> logger, PoolManagerList poolManagerList) : base(logger)
        {
            this.poolManagerList = poolManagerList;
        }

        // GET api/reports
        /// <summary>
        /// Return the list of reports from the global library.
        /// </summary>
        /// <param name="instanceId">The instance identifier.</param>
        [HttpGet]
        [Route("{instanceId}")]
        public void Get(int instanceId)
        {
            using (IProPricerConnection ppc = (IProPricerConnection)poolManagerList.GetInstance(instanceId).GetObjectsFromPool())
            {
                ppc.Workspace.Reports.Open();
                foreach (IReport rep in ppc.Workspace.Reports.Items())
                {
                    ////System.Diagnostics.Debug.WriteLine("rep.Id");
                    ////System.Diagnostics.Debug.WriteLine(rep.Id);
                    ////System.Diagnostics.Debug.WriteLine("rep.Name");
                    ////System.Diagnostics.Debug.WriteLine(rep.Name);
                    ////System.Diagnostics.Debug.WriteLine("rep.Category");
                    ////System.Diagnostics.Debug.WriteLine(rep.Category);
                    ////System.Diagnostics.Debug.WriteLine("rep.Description");
                    ////System.Diagnostics.Debug.WriteLine(rep.Description);
                    ////System.Diagnostics.Debug.WriteLine("rep.Parent");
                    ////System.Diagnostics.Debug.WriteLine(rep.Parent);
                    ////System.Diagnostics.Debug.WriteLine("rep.ParentCollection");
                    ////System.Diagnostics.Debug.WriteLine(rep.ParentCollection);
                    ////System.Diagnostics.Debug.WriteLine("rep.ParentLibrary");
                    ////System.Diagnostics.Debug.WriteLine(rep.ParentLibrary);
                    ////System.Diagnostics.Debug.WriteLine("rep.ParentProposal");
                    ////System.Diagnostics.Debug.WriteLine(rep.ParentProposal);
                    ////System.Diagnostics.Debug.WriteLine("rep.ParentWorkspace");
                    ////System.Diagnostics.Debug.WriteLine(rep.ParentWorkspace);
                    ////System.Diagnostics.Debug.WriteLine("rep.BrokenRules");
                    ////System.Diagnostics.Debug.WriteLine(rep.BrokenRules);
                    ////System.Diagnostics.Debug.WriteLine("rep.ComponentCategory");
                    ////System.Diagnostics.Debug.WriteLine(rep.ComponentCategory);
                    ////System.Diagnostics.Debug.WriteLine("rep.ComponentContext");
                    ////System.Diagnostics.Debug.WriteLine(rep.ComponentContext);
                    ////System.Diagnostics.Debug.WriteLine("rep.ComponentDescription");
                    ////System.Diagnostics.Debug.WriteLine(rep.ComponentDescription);
                    ////System.Diagnostics.Debug.WriteLine("rep.ComponentDescriptor");
                    ////System.Diagnostics.Debug.WriteLine(rep.ComponentDescriptor);
                    ////System.Diagnostics.Debug.WriteLine("rep.ComponentState");
                    ////System.Diagnostics.Debug.WriteLine(rep.ComponentState);
                }

                ppc.Workspace.Reports.Close();
            }
        }
    }
}