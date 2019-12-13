/*
    Copyright 2016-2018 Lockheed Martin Corporation.

    This computer software has been provided in confidence, and contains trade secret and/or privileged or confidential 
    commercial or financial information. Public disclosure of any information marked as indicated above is prohibited 
    by the Trade Secrets Act (18 U.S.C. Sec. 1905) and the Economic Espionage Act of 1996 (18 U.S.C. Sec. 1831 et seq.) 
    and is not to be made available to third parties without the prior written permission of Lockheed Martin Corporation.
*/

using System.Collections.Generic;
using APTSPropricerApi.Connection;
using APTSPropricerApi.DTOs;
using EBS.ProPricer.Model;

namespace APTSPropricerApi.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public class FactorRateTablesController : ProPricerController
    {
        // GET api/factorratetables
        /// <summary>
        /// Returns the list of factor rate tables from the PROPRICER global library.
        /// </summary>
        /// <param name="instanceId">The instance identifier.</param>
        /// <returns>
        /// A collection of factor rate tables.
        /// </returns>
        public IEnumerable<FactorRateTableDto> Get(int instanceId)
        {
            using (IProPricerConnection ppc = (IProPricerConnection)PoolManager.GetInstance(instanceId).GetObjectsFromPool())
            {
                //ppc.workspace.GlobalLibrary.FactorRateTables.Open();
                //IEnumerable<FactorRateTableDto> factorsResult =
                //    from prop in ppc.workspace.GlobalLibrary.FactorRateTables.Cast<FactorRateTable>()
                //    select new FactorRateTableDto
                //    {
                //        id = prop.Id.ToString(),
                //        name = prop.Name,
                //        description = prop.Description,
                //        parentFolder = prop.ParentFolder.Name,
                //        isCurrent = prop.IsCurrent
                //    };
                //ppc.workspace.GlobalLibrary.FactorRateTables.Close();
                //return factorsResult;

                List<FactorRateTableDto> factors = new List<FactorRateTableDto>();
                ppc.Workspace.GlobalLibrary.FactorRateTables.Open();
                foreach (FactorRateTable ppFactor in ppc.Workspace.GlobalLibrary.FactorRateTables)
                {
                    FactorRateTableDto factorTbl = new FactorRateTableDto
                    {
                        Id = ppFactor.Id.ToString(),
                        Name = ppFactor.Name,
                        Description = ppFactor.Description
                    };
                    if (ppFactor.ParentFolder != null)
                    {
                        factorTbl.ParentFolder = ppFactor.ParentFolder.Name;
                    }

                    factorTbl.IsCurrent = ppFactor.IsCurrent;
                    factors.Add(factorTbl);
                }

                ppc.Workspace.GlobalLibrary.FactorRateTables.Close();
                return factors;
            }
        }
    }
}