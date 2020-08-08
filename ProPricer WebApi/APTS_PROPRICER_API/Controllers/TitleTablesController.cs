/*
    Copyright 2016-2020 Lockheed Martin Corporation.

    This computer software has been provided in confidence, and contains trade secret and/or privileged or confidential 
    commercial or financial information. Public disclosure of any information marked as indicated above is prohibited 
    by the Trade Secrets Act (18 U.S.C. Sec. 1905) and the Economic Espionage Act of 1996 (18 U.S.C. Sec. 1831 et seq.) 
    and is not to be made available to third parties without the prior written permission of Lockheed Martin Corporation.
*/

using System;
using System.Collections.Generic;
using APTSPropricerApi.Connection;
using APTSPropricerApi.DTOs;
using EBS.Core;
using EBS.ProPricer.Model;

namespace APTSPropricerApi.Controllers
{
    public class TitleTablesController : ProPricerController
    {
        // GET api/titletablenames
        /// <summary>
        /// Returns the list of Title Table names from the Global Library in PROPRICER.
        /// </summary>
        /// <param name="instanceId">The instance identifier.</param>
        /// <returns>
        /// A collection of title table names.
        /// </returns>
        public IEnumerable<string> Get(int instanceId)
        {
            List<string> titles = new List<string>();
            using (IProPricerConnection ppc = (IProPricerConnection)PoolManager.GetInstance(instanceId).GetObjectsFromPool())
            {
                if (ppc.Workspace != null)
                {
                    ppc.Workspace.GlobalLibrary.TitleTables.Open();
                    foreach (TitleTable tbl in ppc.Workspace.GlobalLibrary.TitleTables.Items())
                    {
                        string title = tbl.Name;
                        titles.Add(title);
                    }

                    ppc.Workspace.GlobalLibrary.TitleTables.Close();
                }
            }

            return titles;
        }

        // GET api/titletablenames/id
        /// <summary>
        /// Returns the title data for a given title table
        /// </summary>
        /// <param name="instanceId">The instance identifier.</param>
        /// <param name="id">The name of the title table</param>
        /// <returns>
        /// Returns the title rows for the given title table
        /// </returns>
        public IEnumerable<TitleTablesDto> Get(int instanceId, string id)
        {
            TitleTable tt = null;
            List<TitleTablesDto> titles = new List<TitleTablesDto>();
            using (IProPricerConnection ppc = (IProPricerConnection)PoolManager.GetInstance(instanceId).GetObjectsFromPool())
            {
                // example id - F35 System Code
                tt = ppc.Workspace.GlobalLibrary.TitleTables.Find(id).Value();
                try
                {
                    tt.Open();
                    foreach (Title t in tt.Elements.Items())
                    {
                        TitleTablesDto ttd = new TitleTablesDto
                        {
                            Name = t.Name,
                            Description = t.Description
                        };
                        titles.Add(ttd);
                    }

                    tt.Close();
                }
                catch (Exception ex)
                {
                    this.Logger.Error(ex);
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }
            }

            return titles;
        }
    }
}