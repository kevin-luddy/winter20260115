// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace RPM.DataBridge.DataLoaders
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using IES.Common;
    using Newtonsoft.Json;
    using RPM.DataBridge.Models;

    /// <summary>
    /// The data loader for OTIS Opportunity Data
    /// </summary>
    public class OTISOpportunityDataMVDataLoader
    {
        /// <summary>
        /// The logger.
        /// </summary>
        private Logger log;

        /// <summary>
        /// Constructor
        /// </summary>
        public OTISOpportunityDataMVDataLoader()
        {
            this.log = new Logger(typeof(OTISOpportunityDataMVDataLoader));
        }

        /// <summary>
        /// Populates OTISOpportunity information for the OTISOpportunity Data Model View
        /// TEST METHOD FOR USING OTIS FILE INSTEAD OF WEB METHOD CALL 
        /// </summary>
        /// <param name="fileName">The file used to retrieve the data.</param>
        /// <returns>The model view retrieved from the URI.</returns>
        public ICollection<OTISOpportunityModelView> GetOTISOpportunityModelViewFromTestFile(string fileName)
        {
            OTISOpportunityDataModelView opportunities = this.GetTestOTISOppCollectionFromTestFile(fileName);
            return opportunities.Opportunities;
        }

        /// <summary>
        /// serialize JSON Otis Opp Data into  OTISOpportunityDataModelView
        /// TEST METHOD FOR USING OTIS FILE INSTEAD OF WEB METHOD CALL 
        /// </summary>
        /// <param name="fileName">The file used to retrieve the data.</param>
        /// <returns>The model view retrieved from the URI.</returns>
        public OTISOpportunityDataModelView GetTestOTISOppCollectionFromTestFile(string fileName)
        {
            OTISOpportunityDataModelView dataModelView = new OTISOpportunityDataModelView();
            dataModelView.Opportunities = new List<OTISOpportunityModelView>();
            string json = string.Empty;
            using (StreamReader r = new StreamReader(fileName))
            {
                json = r.ReadToEnd();
                var datalist = JsonConvert.DeserializeObject<RootObject>(json);
                foreach (OppCollection otis_JSON_Opp in datalist.OppCollection)
                {
                    var otis = this.GetOtisOpp(otis_JSON_Opp);
                    dataModelView.Opportunities.Add(otis);
                }
            }

            return dataModelView;
        }

        /// <summary>
        /// Populates OTISOpportunity information for the OTISOpportunity Data Model View
        /// </summary>
        /// <param name="uri">The URI to call to retrieve the data.</param>
        /// <returns>The model view retrieved from the URI.</returns>
        public ICollection<OTISOpportunityModelView> GetOTISOpportunityModelView(Uri uri)
        {
            OTISOpportunityDataModelView opportunities = this.GetTestOTISOppCollection(uri);
            return opportunities.Opportunities;
        }

        /// <summary>
        /// Makes Service Call and serialize JSON Otis Opp Data into  OTISOpportunityDataModelView
        /// </summary>
        /// <param name="uri">The URI to call to retrieve the data.</param>
        /// <returns>The model view retrieved from the URI.</returns>
        public OTISOpportunityDataModelView GetTestOTISOppCollection(Uri uri)
        {
            OTISOpportunityDataModelView dataModelView = new OTISOpportunityDataModelView();
            dataModelView.Opportunities = new List<OTISOpportunityModelView>();
            string json = string.Empty;
            using (StopwatchTimer sw = new StopwatchTimer(this.log))
            {
                using (WebClient wc = new WebClient())
                {
                    wc.Credentials = CredentialCache.DefaultCredentials;
                    json = wc.DownloadString(uri);
                }

                var datalist = JsonConvert.DeserializeObject<RootObject>(json);

                foreach (OppCollection otis_JSON_Opp in datalist.OppCollection)
                {
                    var otis = this.GetOtisOpp(otis_JSON_Opp);
                    dataModelView.Opportunities.Add(otis);
                }

                return dataModelView;
            }
        }

        /// <summary>
        /// GetOtisOpp() creates OTISOpportunityModelView from OTIS_JSON_Opp collection
        /// </summary>
        /// <param name="otis_JSON_Opp">The json opportunities.</param>
        /// <returns>OTISOpportunityModelView</returns>
        private OTISOpportunityModelView GetOtisOpp(OppCollection otis_JSON_Opp)
        {
            OTISOpportunityModelView otis = new OTISOpportunityModelView();
            otis.OTISNumber = otis_JSON_Opp.OpportunityId;
            otis.LOB = otis_JSON_Opp.LOB;
            // if there's a dash '-' in LOB, to the right of dash is program area
            int programMarker = !string.IsNullOrEmpty(otis.LOB) ? otis.LOB.IndexOf("-") : 0;
            if (programMarker > 0)
            {
                otis.PA = otis.LOB.Substring(programMarker, otis.LOB.Length - programMarker).Replace("-", string.Empty).Trim();
                otis.LOB = otis.LOB.Substring(0, programMarker).Replace("-", string.Empty).Trim();
            }

            otis.Location = otis_JSON_Opp.PropLocationCity + " " + otis_JSON_Opp.PropLocationState + " " + otis_JSON_Opp.PropLocationCountry;
            otis.Category = otis_JSON_Opp.Category;
            otis.ContractType = otis_JSON_Opp.ContractType;
            otis.Competitive = otis_JSON_Opp.Competitive;
            otis.Classified = otis_JSON_Opp.Classified;
            otis.International = !string.IsNullOrEmpty(otis_JSON_Opp.DomesticOverseas) ? otis_JSON_Opp.DomesticOverseas.ToLower().Contains("overseas") ? true : false : false;
            otis.OppType = otis_JSON_Opp.OppType;
            otis.Role = otis_JSON_Opp.BidRole;
            otis.Title = otis_JSON_Opp.OpportunityName;
            otis.SecCodeId = otis_JSON_Opp.SecCodeId;
            otis.TotalProgramValue = otis_JSON_Opp.TotalProgramValue;
            otis.InitialContractValue = otis_JSON_Opp.InitialContractValue;
            if (string.IsNullOrEmpty(otis_JSON_Opp.InitialContractValue))
            {
                otis.EVal = 0;
            }
            else
            {
                otis.EVal = Convert.ToDecimal(otis_JSON_Opp.InitialContractValue);
            }

            otis.Status = otis_JSON_Opp.Status;
            otis.Customer = otis_JSON_Opp.Customer;
            otis.Description = otis_JSON_Opp.Description;
            otis.ThreeYrOrders = otis_JSON_Opp.ThreeYrOrders;

            if (otis_JSON_Opp.FinancialSummary != null)
            {
                foreach (FinancialRow fr in otis_JSON_Opp.FinancialSummary.FinancialRows)
                {
                    if (!string.IsNullOrEmpty(fr.FRType) && fr.FRType.Length == 1)
                    {
                        otis.PlanOrders = fr.FRType;
                    }
                }
            }

            if (otis_JSON_Opp.Milestones != null)
            {
                foreach (Milestone ms in otis_JSON_Opp.Milestones.Milestone)
                {
                    if (!string.IsNullOrEmpty(ms.Date))
                    {
                        if (ms.Name.Contains("Final RFP"))
                        {
                            otis.RFPDate = Convert.ToDateTime(ms.Date);
                        }
                        else if (ms.Name.Contains("Award"))
                        {
                            otis.AwardDate = Convert.ToDateTime(ms.Date);
                        }
                        else if (ms.Name.Contains("Prop Delivery (Planned)"))
                        {
                            otis.PlannedDate = Convert.ToDateTime(ms.Date);
                        }
                        else if (ms.Name.Contains("Prop Delivery (Actual)"))
                        {
                            otis.ActualDate = Convert.ToDateTime(ms.Date);
                        }
                    }
                }
            }

            if (otis_JSON_Opp.Staff != null)
            {
                foreach (Poc poc in otis_JSON_Opp.Staff.Pocs)
                {
                    if (!string.IsNullOrEmpty(poc.Role))
                    {
                        if (poc.Role == "BDMgr")
                        {
                            otis.BDMgr = poc.Name;
                        }
                        else if (poc.Role == "Capture Manager" || poc.Role == "CaptureManager")
                        {
                            otis.CaptMgr = poc.Name;
                        }
                        else if (poc.Role == "BD Lead")
                        {
                            otis.BDLead = poc.Name;
                        }
                        else if (poc.Role == "Proposal Manager" || poc.Role == "ProposalManager")
                        {
                            otis.PM = poc.Name;
                        }
                        else if (poc.Role.Contains("CELead") || poc.Role.Contains("CE Lead"))
                        {
                            otis.CELead = poc.Name;
                        }
                    }
                }
            }

            if (otis.RFPDate.HasValue)
            {
                otis.RFPDateString = otis.RFPDate.Value.ToString("yyyy-MM-dd");
            }

            if (otis.AwardDate.HasValue)
            {
                otis.AwardDateString = otis.AwardDate.Value.ToString("yyyy-MM-dd");
            }

            if (otis.ActualDate.HasValue)
            {
                otis.ActualDateString = otis.ActualDate.Value.ToString("yyyy-MM-dd");
            }

            if (otis.PlannedDate.HasValue)
            {
                otis.PlannedDateString = otis.PlannedDate.Value.ToString("yyyy-MM-dd");
            }
            
            // use ActualDate as end if it exists, otherwise set end date to rfpDate + 1 day
            if (otis.RFPDate.HasValue)
            {
                if (otis.ActualDate.HasValue && otis.ActualDate > otis.RFPDate)
                {
                    otis.RFPEndDateString = otis.ActualDateString;
                }
                else
                {
                    otis.RFPEndDateString = otis.RFPDate.Value.AddDays(1).ToString("yyyy-MM-dd");
                }
            }

            return otis;
        }
    }
}