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
using EBS.ProPricer.Model.General;
using EBS.ProPricer.Model.Pricing;

namespace APTSPropricerApi.Controllers
{
    /// <summary>
    /// Methods to read and update tasks and their associated resources
    /// </summary>
    public class TraceReportController : ProPricerController
    {
        // GET api/TraceReport/proposalid
        /// <summary>
        /// Returns the traceability report numbers for a given proposal
        /// </summary>
        /// <param name="instanceId">The instance identifier.</param>
        /// <param name="id">The EntityId of the proposal in the form of a GUID. Ex: 58b0d1c8-b06d-11e3-83f5-b499bae158c0</param>
        /// <returns>
        /// Returns the traceability report numbers for a given proposal
        /// </returns>
        public IEnumerable<TraceReportDto> Get(int instanceId, string id)
        {
            bool getall = false;

            if (id.EndsWith("Direct"))
            {
                id = id.Substring(0, id.Length - 6);
            }
            else
            {
                getall = true;
            }

            ProposalDto pDto = new ProposalDto();
            Proposal pr = null;
            string whichvar = "proposal id";
            List<TraceReportDto> tracerep = new List<TraceReportDto>();
            List<TraceReportDto> traceclass = new List<TraceReportDto>();
            BurdenCostDto burdens = new BurdenCostDto();
            using (IProPricerConnection ppc = (IProPricerConnection)PoolManager.GetInstance(instanceId).GetObjectsFromPool())
            {
                try
                {
                    // GUID or Name|Version?
                    if (id.Contains("|"))
                    {
                        // Name
                        string[] parts = id.Split('|');
                        pr = ppc.Workspace.Proposals.Find(parts[0], parts[1]).Value();
                    }
                    else
                    {
                        // GUID
                        EBS.ProPricer.Data.EntityId pEntityId = new EBS.ProPricer.Data.EntityId(new Guid(id));
                        pr = ppc.Workspace.Proposals.Find(pEntityId).Value();
                    }

                    pr.Open();
                    ///////////////////////////////////
                    foreach (Task t in pr.Tasks.Items())
                    {
                        whichvar = "tasks";
                        t.Open();

                        System.Diagnostics.Debug.WriteLine("id " + t.Id.Value);
                        System.Diagnostics.Debug.WriteLine("BOE id " + t.BOE.Id.Value);

                        whichvar = "resource assignment";

                        if (t.ResourceAssignments != null && t.ResourceAssignments.Count > 0)
                        {
                            foreach (IResourceAssignment r in t.ResourceAssignments.Items())
                            {
                                r.Open();
                                if (r.Source.Type.ToString() == "Direct" || r.Source.Type.ToString() == "Travel" || getall)
                                {
                                    bool havecost = false;
                                    bool havediscreet = false;
                                    bool havediscreetclass = false;
                                    if (r.Source.Type.ToString() == "Direct" ||
                                        r.Source.Type.ToString() == "Travel")
                                    {
                                        havediscreet = true;
                                        havediscreetclass = true;
                                    }

                                    string rtype = r.Info.RateType.ToString();
                                    //   if (r.Source.Type.ToString() != "Group")
                                    //       if (r.Source.Type.ToString() != "Direct")
                                    //           burdens.name = string.Empty;
                                    // System.Diagnostics.Debug.WriteLine("Res id " + r.Task.Id.Value.ToString());
                                    // System.Diagnostics.Debug.WriteLine("rtype " + rtype);
                                    // System.Diagnostics.Debug.WriteLine("TotalAmount " + r.Spread.TotalAmount.ToString());
                                    // System.Diagnostics.Debug.WriteLine("Resource " + r.Info.Resource.Name.ToString());
                                    // System.Diagnostics.Debug.WriteLine("ResourceClass " + r.Info.ResourceClass.Name.ToString());
                                    // System.Diagnostics.Debug.WriteLine("DirectCost " + Extensions.GetCost(r).DirectCost.ToString());
                                    // System.Diagnostics.Debug.WriteLine("Cost " + Extensions.GetCost(r).BaseCost.ToString());
                                    //if labor spread --- add hours and cost
                                    if (rtype == "Hours")
                                    {
                                        burdens.Name = "Hours";
                                        burdens.Value = r.Spread.TotalAmount.ToString();
                                        havecost = false;

                                        foreach (TraceReportDto cost in tracerep)
                                        {
                                            if (cost.ResName == r.Info.Resource.Name && burdens.Name == cost.PpCol)
                                            {
                                                if (havediscreet)
                                                {
                                                    cost.DiscreteAmt = cost.DiscreteAmt + double.Parse(burdens.Value);
                                                }
                                                else
                                                {
                                                    cost.FactoredAmt = cost.FactoredAmt + double.Parse(burdens.Value);
                                                }

                                                havecost = true;
                                                havediscreet = false; //associated dollars will be factored
                                                break;
                                            }
                                        }

                                        if (!havecost)
                                        {
                                            TraceReportDto tracecost = new TraceReportDto
                                            {
                                                ResName = r.Info.Resource.Name,
                                                ResDescription = r.Info.Resource.Description,
                                                ResClass = r.Info.ResourceClass.Name,
                                                PpCol = burdens.Name
                                            };
                                            if (havediscreet)
                                            {
                                                tracecost.DiscreteAmt = double.Parse(burdens.Value);
                                                tracecost.FactoredAmt = 0;
                                                havediscreet = false; //associated dollars will be factored
                                            }
                                            else
                                            {
                                                tracecost.DiscreteAmt = 0;
                                                tracecost.FactoredAmt = double.Parse(burdens.Value);
                                            }

                                            tracerep.Add(tracecost);
                                        }

                                        havecost = false;
                                        foreach (TraceReportDto cost in traceclass)
                                        {
                                            if (cost.ResClass == r.Info.ResourceClass.Name && cost.PpCol == burdens.Name)
                                            {
                                                if (havediscreetclass)
                                                {
                                                    cost.DiscreteAmt = cost.DiscreteAmt + double.Parse(burdens.Value);
                                                }
                                                else
                                                {
                                                    cost.FactoredAmt = cost.FactoredAmt + double.Parse(burdens.Value);
                                                }

                                                havecost = true;
                                                havediscreetclass = false; //associated dollars will be factored
                                                break;
                                            }
                                        }

                                        if (!havecost)
                                        {
                                            TraceReportDto tracecost = new TraceReportDto
                                            {
                                                ResName = null,
                                                ResDescription = null,
                                                ResClass = r.Info.ResourceClass.Name,
                                                PpCol = burdens.Name
                                            };
                                            if (havediscreetclass)
                                            {
                                                tracecost.DiscreteAmt = double.Parse(burdens.Value);
                                                tracecost.FactoredAmt = 0;
                                                havediscreetclass = false; //associated dollars will be factored
                                            }
                                            else
                                            {
                                                tracecost.DiscreteAmt = 0;
                                                tracecost.FactoredAmt = double.Parse(burdens.Value);
                                            }

                                            traceclass.Add(tracecost);
                                        }
                                    } //(rtype == "Hours")

                                    // if (rtype == "Group")
                                    //     burdens.name = "Direct Cost";
                                    //rtype Units
                                    //TotalAmount 100
                                    //Resource KITS
                                    //ResourceClass UNITS

                                    //TotalAmount 18017
                                    //ResourceClass OTHER COM - IWTA & TEAM

                                    //TotalAmount 25000
                                    //ResourceClass OTHER CHARGES - BELOW THE LINE

                                    burdens.Name = "Direct Cost";
                                    try
                                    {
                                        if (r.Info.ResourceClass.Name == "UNITS" && (r.Info.Resource.Name == "KITS" || r.Info.Resource.Name == "REPORTS")
                                            || r.Info.ResourceClass.Name == "OTHER COM - IWTA & TEAM" || r.Info.ResourceClass.Name == "OTHER CHARGES - BELOW THE LINE")
                                        {
                                            burdens.Value = r.Spread.TotalAmount.ToString();
                                        }
                                        else
                                        {
                                            burdens.Value = r.GetCost().DirectCost.ToString();
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        this.Logger.Error(ex);
                                        burdens.Value = "0";
                                    }

                                    havecost = false;
                                    foreach (TraceReportDto cost in tracerep)
                                    {
                                        if (cost.ResName == r.Info.Resource.Name && burdens.Name == cost.PpCol)
                                        {
                                            if (havediscreet)
                                            {
                                                cost.DiscreteAmt = cost.DiscreteAmt + double.Parse(burdens.Value);
                                            }
                                            else
                                            {
                                                cost.FactoredAmt = cost.FactoredAmt + double.Parse(burdens.Value);
                                            }

                                            havecost = true;
                                            break;
                                        }
                                    }

                                    if (!havecost)
                                    {
                                        TraceReportDto tracecost = new TraceReportDto
                                        {
                                            ResName = r.Info.Resource.Name,
                                            ResDescription = r.Info.Resource.Description,
                                            PpCol = burdens.Name,
                                            ResClass = r.Info.ResourceClass.Name
                                        };
                                        if (havediscreet)
                                        {
                                            tracecost.DiscreteAmt = double.Parse(burdens.Value);
                                            tracecost.FactoredAmt = 0;
                                        }
                                        else
                                        {
                                            tracecost.DiscreteAmt = 0;
                                            tracecost.FactoredAmt = double.Parse(burdens.Value);
                                        }

                                        tracerep.Add(tracecost);
                                    }

                                    havecost = false;
                                    //    burdens.name = "Dollars";
                                    foreach (TraceReportDto cost in traceclass)
                                    {
                                        if (cost.ResClass == r.Info.ResourceClass.Name && cost.PpCol == burdens.Name)
                                        {
                                            //       if (r.Info.ResourceClass.Name.ToString() == "OTHER - IWTA")
                                            //           havecost = false;   for debugging
                                            if (havediscreetclass)
                                            {
                                                cost.DiscreteAmt = cost.DiscreteAmt + double.Parse(burdens.Value);
                                            }
                                            else
                                            {
                                                cost.FactoredAmt = cost.FactoredAmt + double.Parse(burdens.Value);
                                            }

                                            havecost = true;
                                            havediscreetclass = false; //associated dollars will be factored
                                            break;
                                        }
                                    }

                                    if (!havecost)
                                    {
                                        TraceReportDto tracecost = new TraceReportDto
                                        {
                                            ResName = null,
                                            ResDescription = null,
                                            ResClass = r.Info.ResourceClass.Name,
                                            PpCol = burdens.Name
                                        };
                                        //  if (r.Info.ResourceClass.Name.ToString() == "OTHER - IWTA")
                                        //      havecost = false;   for debugging

                                        if (havediscreetclass)
                                        {
                                            tracecost.DiscreteAmt = double.Parse(burdens.Value);
                                            tracecost.FactoredAmt = 0;
                                            havediscreetclass = false; //associated dollars will be factored
                                        }
                                        else
                                        {
                                            tracecost.DiscreteAmt = 0;
                                            tracecost.FactoredAmt = double.Parse(burdens.Value);
                                        }

                                        traceclass.Add(tracecost);
                                    }

                                    // COST for v9.2

                                    if (r.GetCost() != null)
                                    {
                                        CostInfo c = r.GetCost();

                                        foreach (IBurdenCostElement el in c.BurdenElements)
                                        {
                                            burdens.Name = el.Name;
                                            burdens.Value = c.BurdenCost(el.Position).ToString();
                                            if (double.Parse(burdens.Value) != 0.0 &&
                                                (burdens.Name == "Escal" ||
                                                 burdens.Name == "Fringe" ||
                                                 burdens.Name == "OT PREM ENGR" ||
                                                 burdens.Name == "OT PREM MFG" ||
                                                 burdens.Name == "OT PREM MM" ||
                                                 burdens.Name == "OT PREM OD" ||
                                                 burdens.Name == "Tot OT PREM" ||
                                                 burdens.Name == "ENGR OH" ||
                                                 burdens.Name == "MFG OH FWT" ||
                                                 burdens.Name == "MFG OH MAR" ||
                                                 burdens.Name == "MFG OH PLM" ||
                                                 burdens.Name == "MM OH" ||
                                                 burdens.Name == "Tot OH" ||
                                                 burdens.Name == "Marketing" ||
                                                 burdens.Name == "PROG OFFICE" ||
                                                 burdens.Name == "G&A" ||
                                                 burdens.Name == "Tot 414COM" ||
                                                 burdens.Name == "Tot 417COM" ||
                                                 burdens.Name == "Profit/Fee"))
                                            {
                                                if (burdens.Name == "Tot 414COM" ||
                                                    burdens.Name == "Tot 417COM")
                                                {
                                                    burdens.Name = "Total COM";
                                                }

                                                havecost = false;
                                                if (burdens.Name == "Profit/Fee")
                                                {
                                                    havecost = false;
                                                }

                                                foreach (TraceReportDto cost in tracerep)
                                                {
                                                    if (cost.ResName == r.Info.Resource.Name && burdens.Name == cost.PpCol)
                                                    {
                                                        cost.FactoredAmt = cost.FactoredAmt + double.Parse(burdens.Value);
                                                        havecost = true;
                                                        break;
                                                    }
                                                }

                                                if (!havecost)
                                                {
                                                    TraceReportDto tracecost = new TraceReportDto
                                                    {
                                                        ResName = r.Info.Resource.Name,
                                                        ResDescription = r.Info.Resource.Description,
                                                        PpCol = burdens.Name,
                                                        ResClass = r.Info.ResourceClass.Name,
                                                        DiscreteAmt = 0,
                                                        FactoredAmt = double.Parse(burdens.Value)
                                                    };
                                                    tracerep.Add(tracecost);
                                                }

                                                havecost = false;
                                                if (burdens.Name == "Fringe" ||
                                                    burdens.Name == "Tot OT PREM" ||
                                                    burdens.Name == "Tot OH" ||
                                                    burdens.Name == "Marketing" ||
                                                    burdens.Name == "PROG OFFICE" ||
                                                    burdens.Name == "G&A" ||
                                                    burdens.Name == "Total COM" ||
                                                    burdens.Name == "Profit/Fee")
                                                {
                                                    foreach (TraceReportDto cost in traceclass)
                                                    {
                                                        if (cost.ResName == burdens.Name && burdens.Name == cost.PpCol)
                                                        {
                                                            if (burdens.Name == "Profit/Fee")
                                                            {
                                                            }

                                                            cost.FactoredAmt = cost.FactoredAmt + double.Parse(burdens.Value);
                                                            havecost = true;
                                                            break;
                                                        }
                                                    }

                                                    if (!havecost)
                                                    {
                                                        TraceReportDto tracecost = new TraceReportDto
                                                        {
                                                            ResName = burdens.Name,
                                                            ResDescription = null,
                                                            PpCol = burdens.Name,
                                                            ResClass = null, //r.Info.ResourceClass.Name.ToString();
                                                            DiscreteAmt = 0,
                                                            FactoredAmt = double.Parse(burdens.Value)
                                                        };
                                                        traceclass.Add(tracecost);
                                                    }
                                                }
                                            }
                                        }
                                    } //if (Extensions.GetCost(r) != null)
                                }

                                r.Close();
                            }
                        }

                        t.Close();
                    }
                }
                catch (Exception ex)
                {
                    this.Logger.Error(ex, "Error with " + whichvar);
                    traceclass = new List<TraceReportDto>();
                    TraceReportDto tracecost = new TraceReportDto
                    {
                        ResName = "Error",
                        ResDescription = ex.Message,
                        PpCol = "Error",
                        ResClass = null, //r.Info.ResourceClass.Name.ToString();
                        DiscreteAmt = 0,
                        FactoredAmt = 0
                    };
                    traceclass.Add(tracecost);
                    //System.Diagnostics.Debug.WriteLine("Error with " + whichvar + " - " + ex.Message);
                    //tasks[tasks.Count].id = "Error with " + whichvar + " - " + ex.Message;
                }
                //////////////////////////////////

                pr.Close();

                foreach (TraceReportDto cls in traceclass)
                {
                    if (cls.ResName == "Fringe")
                    {
                        cls.ResName = "TOTAL FRINGE";
                    }

                    if (cls.ResName == "Tot OT PREM")
                    {
                        cls.ResName = "TOTAL OVERTIME PREMIUM";
                    }

                    if (cls.ResName == "Tot OH")
                    {
                        cls.ResName = "TOTAL OVERHEAD";
                    }

                    if (cls.ResName == "Marketing")
                    {
                        cls.ResName = "TOTAL Marketing";
                    }

                    if (cls.ResName == "PROG OFFICE")
                    {
                        cls.ResName = "TOTAL PROGRAM OFFICE";
                    }

                    if (cls.ResName == "G&A")
                    {
                        cls.ResName = "TOTAL G&A";
                    }

                    if (cls.ResName == "Total COM")
                    {
                        cls.ResName = "TOTAL Cost of Money";
                    }

                    if (cls.ResName == "Profit/Fee")
                    {
                        cls.ResName = "TOTAL Profit/ Fee";
                    }
                }
            }
            return traceclass; // tracerep
        }
    }
}