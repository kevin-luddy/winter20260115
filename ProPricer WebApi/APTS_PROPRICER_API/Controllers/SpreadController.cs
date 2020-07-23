/*
    Copyright 2016-2020 Lockheed Martin Corporation.

    This computer software has been provided in confidence, and contains trade secret and/or privileged or confidential 
    commercial or financial information. Public disclosure of any information marked as indicated above is prohibited 
    by the Trade Secrets Act (18 U.S.C. Sec. 1905) and the Economic Espionage Act of 1996 (18 U.S.C. Sec. 1831 et seq.) 
    and is not to be made available to third parties without the prior written permission of Lockheed Martin Corporation.
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using APTSPropricerApi.Connection;
using APTSPropricerApi.DTOs;
using EBS.Core;
using EBS.ProPricer.Data;
using EBS.ProPricer.Model;
using EBS.ProPricer.Model.General;

namespace APTSPropricerApi.Controllers
{
    /// <summary>
    /// Utility to calculate a resource spread for a given curve
    /// </summary>
    public class SpreadController : ProPricerController
    {
        // GET api/spread
        /// <summary>
        /// Returns an array of spread values for the given input parms.
        /// </summary>
        /// <param name="instanceId">The instance identifier.</param>
        /// <param name="amount">The total amount to be spread.</param>
        /// <param name="curve">The entity id of a selected curve from the global library. Ex: 58b0d1c8-b06d-11e3-83f5-b499bae158c0</param>
        /// <param name="startDate">yyyy-mm format.</param>
        /// <param name="endDate">yyyy-mm format.</param>
        /// <returns>
        /// Returns a collection of the monthly spread amounts.
        /// </returns>
        public IEnumerable<SpreadDto> Get(int instanceId, double amount, string curve, string startDate, string endDate)
        {
            List<SpreadDto> spreadList = new List<SpreadDto>();
            using (IProPricerConnection ppc = (IProPricerConnection)PoolManager.GetInstance(instanceId).GetObjectsFromPool())
            {
                try
                {
                    EntityId pEntityId = new EntityId(new Guid(curve));
                    Curve c = ppc.Workspace.GlobalLibrary.Curves.Find(pEntityId).Value();

                    TimeFrame start = new TimeFrame(DateTime.Parse(startDate + "-01"));
                    TimeFrame end = new TimeFrame(DateTime.Parse(endDate + "-01"));
                    TimePeriod period = new TimePeriod(TimeUnit.Month, start, end);

                    IEnumerable<KeyValuePair<int, double>> rawSpreadList = SpreadUtils.GenerateSpread(amount, period, TimeUnit.Month, 2, SpreadMethod.WeightedAvg, c);

                    DateTime dtStart = DateTime.ParseExact(startDate, "yyyy-MM", CultureInfo.InvariantCulture);

                    // Translate the rawSpreadList to an array of SpreadDto objects
                    foreach (KeyValuePair<int, double> item in rawSpreadList)
                    {
                        Console.WriteLine(item.ToString());
                        SpreadDto s = new SpreadDto();
                        DateTime thisDate = dtStart.AddMonths(item.Key);
                        s.Year = thisDate.Year;
                        s.Month = thisDate.Month;
                        s.Value = item.Value.ToString();
                        spreadList.Add(s);
                    }
                }
                catch (Exception ex)
                {
                    this.Logger.Error(ex);
                }
            }
            return spreadList;
        }
    }
}