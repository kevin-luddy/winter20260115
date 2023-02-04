/*
    Copyright 2016-2020 Lockheed Martin Corporation.

    This computer software has been provided in confidence, and contains trade secret and/or privileged or confidential 
    commercial or financial information. Public disclosure of any information marked as indicated above is prohibited 
    by the Trade Secrets Act (18 U.S.C. Sec. 1905) and the Economic Espionage Act of 1996 (18 U.S.C. Sec. 1831 et seq.) 
    and is not to be made available to third parties without the prior written permission of Lockheed Martin Corporation.
*/

using APTSPropricerApi.Common;
using APTSPropricerApi.Connection;
using APTSPropricerApi.DTOs;
using EBS.Core;
using EBS.ProPricer.Data;
using EBS.ProPricer.Model;
using EBS.ProPricer.Model.General;
using EBS.ProPricer.Model.Pricing;
using Microsoft.AspNetCore.Mvc;
using Task = EBS.ProPricer.Model.Task;

namespace APTSPropricerApi.Controllers
{
    /// <summary>
    /// Methods to read and update tasks and their associated resources
    /// </summary>
    public class TasksController : ProPricerController
    {
        /// <summary>
        /// Pool Manager
        /// </summary>
        private readonly PoolManagerList poolManagerList;

        /// <summary>
        /// #ctor
        /// </summary>
        public TasksController(ILogger<TasksController> logger, PoolManagerList poolManagerList) : base(logger)
        {
            this.poolManagerList = poolManagerList;
        }

        // GET api/tasks/proposalid
        /// <summary>
        /// Returns the tasks for a given proposal
        /// </summary>
        /// <param name="instanceId">The instance identifier.</param>
        /// <param name="id">The EntityId of the proposal in the form of a GUID. Ex: 58b0d1c8-b06d-11e3-83f5-b499bae158c0</param>
        /// <returns>
        /// Returns the tasks for a given proposal
        /// </returns>
        [HttpGet]
        [Route("{instanceId}/{id}")]
        public IEnumerable<TaskDto> Get(int instanceId, string id)
        {
            return Utility.GetTasksForProposal(poolManagerList, Logger, instanceId, id);
        }

        // POST api/tasks
        /// <summary>
        /// Methods to read and update tasks and their associated resources
        /// </summary>
        /// <param name="instanceId">The instance identifier.</param>
        /// <param name="proposalAndTasks">The proposal and tasks.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("{instanceId}")]
        public IEnumerable<TaskDto> Post(int instanceId, [FromBody] ProposalDto proposalAndTasks)
        {
            List<TaskDto> retasks = new();
            TaskDto retask;
            if (proposalAndTasks == null)
            {
                retask = new TaskDto
                {
                    Id = "0",
                    Name = "No data was entered to change"
                };
                retasks.Add(retask);
                return retasks;
            }

            string whichvar = "editing proposal";
            Proposal ppProposal = null;
            using (IProPricerConnection ppc = (IProPricerConnection)poolManagerList.GetInstance(instanceId).GetObjectsFromPool())
            {
                try
                {
                    EntityId pEntityId = new(new Guid(proposalAndTasks.Id));
                    ppProposal = ppc.Workspace.Proposals.Find(pEntityId).Value();

                    //  Proposal ppProposal = ppc.workspace.Proposals.Find(proposalAndTasks.Name, proposalAndTasks.Version).Value;

                    //get burden pool library
                    BurdenPoolLibrary burdenPoolLibrary = ppc.Workspace.GlobalLibrary.BurdenPools;

                    //lock the proposal for modification.
                    ppProposal.Open();
                    ppProposal.BeginEdit();

                    List<TaskDto> addtasks = this.AddTasks(ppc, ppProposal, proposalAndTasks, burdenPoolLibrary);

                    foreach (TaskDto item in addtasks)
                    {
                        retasks.Add(item);
                    }

                    //End edits and release the record
                    ppProposal.EndEdit();
                }
                catch (BOBrokenRulesException ex)
                {
                    this.Logger.LogError(ex, "Error with " + whichvar + " - " + ex.BrokenRules[0]);
                    System.Diagnostics.Debug.WriteLine("Error with " + whichvar + " - " + ex.BrokenRules[0]);
                    retask = new TaskDto
                    {
                        Id = "0",
                        Name = "Broken rules with " + whichvar + " - " + ex.BrokenRules[0]
                    };
                    retasks.Add(retask);
                    ppProposal?.CancelEdit();
                }
                //exception block to catch any other issues with the data
                catch (Exception ex)
                {
                    this.Logger.LogError(ex, "Error with " + whichvar);
                    System.Diagnostics.Debug.WriteLine("Error with " + whichvar + " - " + ex.Message);
                    retask = new TaskDto
                    {
                        Id = "0",
                        Name = "Error with " + whichvar + " - " + ex.Message
                    };
                    retasks.Add(retask);
                    ppProposal?.CancelEdit();
                }

                //Close the proposal
                ppProposal?.Close();
            }

            return retasks;
        }

        // DELETE api/tasks/35915285-1000-e511-8b16-005056c00008
        /// <summary>
        /// Delete all tasks from a proposal identified by entity id (ex. 35915285-1000-e511-8b16-005056c00008)
        /// </summary>
        /// <param name="instanceId">The instance identifier.</param>
        /// <param name="id">The EntityId of the proposal in the form of a GUID. Ex: 58b0d1c8-b06d-11e3-83f5-b499bae158c0</param>
        [HttpDelete]
        [Route("{instanceId}/{id}")]
        public void Delete(int instanceId, string id)
        {
            using (IProPricerConnection ppc = (IProPricerConnection)poolManagerList.GetInstance(instanceId).GetObjectsFromPool())
            {
                try
                {
                    EntityId pEntityId = new(new Guid(id));
                    Proposal pr = ppc.Workspace.Proposals.Find(pEntityId).Value();

                    //lock the proposal for modification.
                    pr.Open();
                    pr.BeginEdit();

                    pr.Tasks.DeleteAll();

                    //End edits and commit deletes 
                    pr.EndEdit();

                    //Close the proposal
                    pr.Close();
                }
                //exception block to catch any issues with deleting
                catch (Exception ex)
                {
                    this.Logger.LogError(ex, "Error Deleting Task");
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }
            }
        }

        // loop thru each supplied task and add to the current proposal
        private List<TaskDto> AddTasks(IProPricerConnection ppc, Proposal proposal, ProposalDto proposalAndTasks, BurdenPoolLibrary burdenPoolLibrary)
        {
            List<TaskDto> retasks = new();
            foreach (TaskDto task in proposalAndTasks.Tasks)
            {
                //if (task.name == "175")
                //{
                //    task.id = task.id;
                //}
                string rc = this.AddTask(ppc, proposal, task, burdenPoolLibrary);
                TaskDto retask = new();
                if (rc == string.Empty)
                {
                    Optional<Task> tc = proposal.Tasks.Find(task.Name);
                    if (tc.HasValue)
                    {
                        retask.Id = tc.Value.Id.ToString();
                        retask.Name = tc.Value.Name;
                    }
                    else
                    {
                        retask.Id = "0";
                        retask.Name = "Error with " + task.Name + " - cannot find";
                    }
                }
                else
                {
                    Optional<Task> tc = proposal.Tasks.Find(task.Name);
                    if (tc.HasValue)
                    {
                        retask.Id = tc.Value.Id.ToString();
                        retask.Name = "Error with " + task.Name + " - " + rc;
                    }
                    else
                    {
                        retask.Id = "0";
                        retask.Name = "Error with " + task.Name + " - " + rc;
                    }
                }

                retasks.Add(retask);
            }

            return retasks;
        }

        private string AddTask(IProPricerConnection ppc, Proposal proposal, TaskDto task, BurdenPoolLibrary burdenPoolLibrary)
        {
            if (proposal == null)
            {
                throw new ArgumentNullException(nameof(proposal));
            }

            if (task == null)
            {
                throw new ArgumentNullException(nameof(task));
            }

            if (burdenPoolLibrary == null)
            {
                throw new ArgumentNullException(nameof(burdenPoolLibrary));
            }

            string taskrc; //good 
            string whichvar = "task";
            Task myTask = null;
            //Insert the task name(required) and the description(optional).
            try
            {
                //BEGIN: task data insertion and locking the record
                myTask = proposal.Tasks.AddNew();
                myTask.BeginEdit();
                whichvar = "name";
                myTask.Name = task.Name;
                whichvar = "description";
                myTask.Description = task.Description;
                whichvar = "start date";
                myTask.StartDate = TimeFrame.FromMonth(Convert.ToDateTime(task.StartDate).Year, Convert.ToDateTime(task.StartDate).Month);
                whichvar = "end date";
                myTask.EndDate = TimeFrame.FromMonth(Convert.ToDateTime(task.EndDate).Year, Convert.ToDateTime(task.EndDate).Month);
                whichvar = "actual fee";
                if (task.ActualFee != null)
                {
                    myTask.Fee = double.Parse(task.ActualFee);
                }

                whichvar = "quantity";
                myTask.Quantity = task.Quantity;

                /****************************************************************
                 * Task dates are required fields. These dates must fall between
                 * the start and end dates of the proposal!
                 * *************************************************************/
                //try
                //{
                //    myTask.StartDate = Convert.ToDateTime(taskStartDate);
                //    myTask.EndDate = Convert.ToDateTime(taskEndDate);
                //}
                //catch (Exception ex)
                //{
                //    this.Logger.LogError(ex);
                //    System.Diagnostics.Debug.WriteLine("Task Start / End Date: " + ex.Message);
                //}

                // DAM - experimental
                //EBS.ProPricer.Model.ResourceAssignment myRes = myTask.Resources.AddNew();
                //EBS.ProPricer.Model.ResourceLibrary;

                ////task fee insertion
                //try
                //{
                //    myTask.Fee = Convert.ToDouble(profitFee);
                //}
                //catch (Exception ex)
                //{
                //    this.Logger.LogError(ex);
                //    System.Diagnostics.Debug.WriteLine("Profit/Fee: " + ex.Message);
                //}

                //try
                //{
                //    //Add the burden pool if it has not already been added.
                //    if (burdenPoolLibrary.Find(burdenPool).HasValue == true)
                //        myTask.BurdenPool = burdenPoolLibrary.Find(burdenPool).Value;
                //    else
                //        throw new Exception("BurdenPool addition provided was not found or already exists; Skipped burden pool insertion.");
                //}
                //catch (Exception ex)
                //{
                //    this.Logger.LogError(ex);
                //    System.Diagnostics.Debug.WriteLine("BurdenPool: " + ex.Message);
                //}

                whichvar = "add resource";
                string rerc = this.AddTaskResource(ppc, proposal, task, myTask);
                taskrc = rerc;

                whichvar = "add summary fields";
                string sfrc = this.AddSummaryFields(proposal, task, myTask);
                if (taskrc == string.Empty)
                {
                    taskrc = sfrc;
                }

                whichvar = "add material assignments";
                string marc = this.AddMaterialAssignments(ppc, proposal, task, myTask);
                if (taskrc == string.Empty)
                {
                    taskrc = marc;
                }

                whichvar = "add travel assignments";
                string trarc = this.AddTravelAssignments(ppc, proposal, task, myTask);
                if (taskrc == string.Empty)
                {
                    taskrc = trarc;
                }

                //End edits and release the record
                myTask.EndEdit();
            }
            //exception block to catch any broken rules relating to the API business logic
            catch (BOBrokenRulesException ex)
            {
                this.Logger.LogError(ex, "Error with " + whichvar + " - " + ex.BrokenRules[0]);
                System.Diagnostics.Debug.WriteLine("Error with " + whichvar + " - " + ex.BrokenRules[0]);
                taskrc = "Broken rules with " + whichvar + " - " + ex.BrokenRules[0];
                myTask?.CancelEdit();
            }
            //exception block to catch any other issues with the data
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "Error with " + whichvar);
                System.Diagnostics.Debug.WriteLine("Error with " + whichvar + " - " + ex.Message);
                taskrc = "Error with " + whichvar + " - " + ex.Message;
                myTask?.CancelEdit();
            }

            // HACK: chose a resource at random.  Need to pass resource from calling process.
            // String[] resourceNames = { "CUSTOMER SUPPORT", "FACTORY ASSEMBLY - FWT", "FIELD OPERATIONS - FWT", "MANUFACTURING SUPPORT - FWT", "PRODUCT ENGINEERING", "QUALITY ASSURANCE - FWT", "SUPPLIER QUALITY MANAGEMENT" };
            //  int index = new Random().Next(resourceNames.Length);

            //AddTaskResource(ppc, proposal, myTask, resourceNames[index]);
            return taskrc;
        }

        private string AddSummaryFields(Proposal prop, TaskDto task, EBS.ProPricer.Model.Task myTask)
        {
            string rc = string.Empty; //good
            string whichvar = string.Empty;
            SummaryFieldDefinition oldsf = null;
            if (task.SummaryFields != null)
            {
                try
                {
                    for (int i = 0; i < myTask.SummaryFields.Count; i++)
                    {
                        oldsf = myTask.SummaryFields.Definitions[i];
                        foreach (SummaryFieldsDto newsf in task.SummaryFields)
                        {
                            if (oldsf.Name == newsf.Key)
                            {
                                if (oldsf.DataType.ToString() != "List")
                                {
                                    oldsf.BeginEdit();
                                    myTask.SummaryFields[i] = newsf.Value;
                                    oldsf.EndEdit();
                                    break;
                                }

                                if (newsf.Value != string.Empty)
                                {
                                    Optional<SummaryFieldStandardValue> sfdl = oldsf.ValueList.Find(newsf.Value);
                                    if (sfdl.HasValue)
                                    {
                                        whichvar = "setting summary field " + newsf.Value + " to " + sfdl.Value;
                                        oldsf.BeginEdit();
                                        myTask.SummaryFields[i] = sfdl.Value;
                                        oldsf.EndEdit();
                                    }
                                }

                                break;
                            }
                        }
                    }
                }
                catch (BOBrokenRulesException ex)
                {
                    this.Logger.LogError(ex, "Error " + " - " + ex.BrokenRules[0]);
                    System.Diagnostics.Debug.WriteLine("Error " + " - " + ex.BrokenRules[0]);
                    rc = rc + "Error with " + whichvar + " - " + ex.BrokenRules[0];
                    oldsf.CancelEdit();
                }
                catch (Exception ex)
                {
                    this.Logger.LogError(ex, "Error adding Summary Fields in Task");
                    System.Diagnostics.Debug.WriteLine("Error " + " - " + ex.Message);
                    rc = rc + "Error with " + whichvar + " - " + ex.Message;
                    if (oldsf is not null)
                    {
                        oldsf.CancelEdit();
                    }
                }
            }

            //*  code where I was trying to figure something out may have to add value to list one day  
            // ((EBS.ProPricer.Model.Title)(myTask.SummaryFields.ElementAt(1)).Value.Value).Name = newsf.value;
            //   ((oldsf.Value).Name.ElementAt(1)) = newsf.value;
            //     oldsf.Value = newsf.value; 
            //   IEnumerable<SummaryFieldStandardValue> svl = oldsf.Value.ValueList.Items();
            //      ((System.Collections.Generic.KeyValuePair<EBS.ProPricer.Model.SummaryFieldDefinition, EBS.ProPricer.Model.General.SummaryFieldValue?>[])new EBS.Core.Collections.DebugViews.CollectionDebugView(t.SummaryFields).Items)[0] = "11";

            //    SummaryFieldStandardValue sfsv = oldsf.Value.ValueList.AddNew();
            //    bool worked = oldsf.Value.ValueList.AddNew < sfsv >
            //   sfsv.Value = newsf.value;

            // var oldsf = myTask.SummaryFields.Definitions.Find(newsf.key.ToString());
            //               myTask.SummaryFields.Definitions.Open();
            //    if (oldsf.HasValue)

            //foreach (var oldsf in myTask.SummaryFields)
            //  {
            //      oldsf.Key.BeginEdit();
            //      if (oldsf.Key.Name.ToString() == newsf.key.ToString())
            //      {
            //          //  EBS.ProPricer.Model.General.SummaryFieldValue val = oldsf.Value.ToString();
            //          //  val.StandardValue.Value = "111";
            //          //  oldsf.Key.ValueList.Open();
            //          if (oldsf.Key.ValueList) 
            //          {
            //              //oldsf.Key.ValueList.AddNew();
            //              //SummaryFieldValue sfv = oldsf.Key.ValueList.AddNew();
            //              SummaryFieldStandardValue sfsv = oldsf.Key.ValueList.AddNew(); 
            //              sfsv.Value = newsf.value;

            //            //  EBS.ProPricer.Data.SummaryFieldDataType sdt;
            //            //  Enum.TryParse(oldsf.Key.DataType.ToString(), out sdt);
            //             // bool worked = oldsf.Key.ValueList.AddNew<sfv>
            //          }
            //          else
            //          {
            //              foreach (var item in oldsf.Key.ValueList)
            //              {
            //                  item.Value = newsf.value;
            //              }
            //          }
            //      }
            //      oldsf.Key.EndEdit();
            //  }

            return rc;
        }

        private string AddMaterialAssignments(IProPricerConnection ppc, Proposal prop, TaskDto task, EBS.ProPricer.Model.Task myTask)
        {
            string rc = string.Empty; //good
            if (task.MaterialAssignments != null)
            {
                foreach (MaterialAssignmentDto newma in task.MaterialAssignments)
                {
                    Resource res = null;
                    try
                    {
                        res = ppc.Workspace.GlobalLibrary.Resources.Find(newma.ResourceAssignment.Name).Value();
                    }
                    catch (Exception ex)
                    {
                        rc = "Error with " + newma.ResourceAssignment.Name + " - " + ex.Message;
                        this.Logger.LogError(ex, rc);
                        System.Diagnostics.Debug.WriteLine(rc);
                    }

                    if (res != null)
                    {
                        //Check to see if resource exists in the direct rate table
                        if (prop.DirectRateTable.Elements.Find(res).HasValue())
                        {
                            MaterialAssignment ma = myTask.MaterialAssignments.AddNew();
                            string whichvar = "material assignment";
                            try
                            {
                                ma.BeginEdit();
                                ma.MaterialName = newma.MaterialName;
                                ma.Description = newma.Description;
                                ma.Name = newma.PartName;
                                ma.MaterialDescription = newma.PartDescription;
                                if (newma.UnitCost != null)
                                {
                                    ma.BaseUnitCost = double.Parse(newma.UnitCost);
                                }

                                //   ma.UnitQty = newma.unitQty;
                                whichvar = "material assignment resource";
                                ma.ResourceAssignmentInfo.Resource = res;
                                //  7ma.MaterialDescription = newma.description;

                                whichvar = "material assignment spread";
                                IAmountCollection rsd = ma.Spread.Distribution;

                                foreach (SpreadDto s in newma.ResourceAssignment.Spread)
                                {
                                    TimeFrame mnyr = TimeFrame.FromMonth(s.Year, s.Month);
                                    rsd[mnyr] = double.Parse(s.Value);
                                }

                                //AddAmount can also be used to add a new resource to an existing one.

                                ma.Spread.Method = SpreadMethod.WeightedAvg;

                                whichvar = "material assignment spread amount";
                                ma.Spread.Amount = newma.ResourceAssignment.Amount != null ? double.Parse(newma.ResourceAssignment.Amount) : 0;
                                whichvar = "material assignment curve";
                                if (newma.ResourceAssignment.SpreadCurve != null && newma.ResourceAssignment.SpreadCurve != string.Empty)
                                {
                                    Curve c = ppc.Workspace.GlobalLibrary.Curves.Find(newma.ResourceAssignment.SpreadCurve, CurveType.System).Value();
                                    ma.Spread.Curve = c;
                                }

                                whichvar = "material assignment start date";
                                //                            ma.Spread = (newma.Spread.Curve != null) ? newma.Spread.Curve.Name : string.Empty;
                                if (newma.ResourceAssignment.StartDate != null && newma.ResourceAssignment.StartDate != string.Empty)
                                {
                                    ma.Spread.StartDate = TimeFrame.FromMonth(int.Parse(newma.ResourceAssignment.StartDate.Substring(3, 4)), int.Parse(newma.ResourceAssignment.StartDate.Substring(0, 2))).Value;
                                }

                                whichvar = "material assignment end date";
                                if (newma.ResourceAssignment.EndDate != null && newma.ResourceAssignment.EndDate != string.Empty)
                                {
                                    ma.Spread.EndDate = TimeFrame.FromMonth(int.Parse(newma.ResourceAssignment.EndDate.Substring(3, 4)), int.Parse(newma.ResourceAssignment.EndDate.Substring(0, 2))).Value;
                                }

                                if (newma.ResourceAssignment.ResourceFields != null)
                                {
                                    IEnumerable<ResourceFieldsDto> resourceFields = newma.ResourceAssignment.ResourceFields;
                                    foreach (ResourceFieldsDto ritem in resourceFields)
                                    {
                                        if (ritem.Value != string.Empty)
                                        {
                                            whichvar = "material assignment resource fields";
                                            ResourceFieldDefinition rfd = ppc.Workspace.GlobalLibrary.ResourceFieldDefinitions.Find(ritem.Key).Value();
                                            try
                                            {
                                                ResourceFieldStandardValue newval = rfd.ValueList.Find(ritem.Value).Value();
                                                //  item.Value.Open();
                                                //  item.Value.BeginEdit();
                                                ResourceAssignmentInfo.ResourceFieldProperty.GetDescriptor(ppc.Workspace.GlobalLibrary.ResourceFieldDefinitions.Items().ToList().IndexOf(rfd)).SetValue(ma.ResourceAssignmentInfo, newval);
                                                // item.Value.EndEdit();
                                                //  item.Value.Close();
                                            }
                                            catch (Exception mx)
                                            {
                                                string msg = "Error with field " + ritem.Key + "=" + ritem.Value + " - " + mx.Message;
                                                this.Logger.LogError(mx, msg);
                                                System.Diagnostics.Debug.WriteLine(msg);
                                                rc = rc + msg;
                                            }
                                        }
                                    }
                                }

                                //// associated costs
                                if (newma.AssociatedCosts != null)
                                {
                                    IEnumerable<AssociatedCostsDto> asclistdto = newma.AssociatedCosts;

                                    foreach (AssociatedCostsDto ascitem in asclistdto)
                                    {
                                        whichvar = "material associated costs";
                                        MaterialAssignmentAssociatedCost maasc = ma.AssociatedCosts.AddNew();
                                        maasc.BeginEdit();

                                        maasc.Name = ascitem.Name;
                                        //  maasc.ResourceAssignmentInfo.Resource.Name = ascitem.resource.name;
                                        //  maasc.ResourceAssignmentInfo.Resource.Description = ascitem.resource.name;
                                        //  maasc.ResourceAssignmentInfo.Resource.ResourceClass.Name = ascitem.resource.name;

                                        //  ResourceType rst;
                                        //  Enum.TryParse(ascitem.resource.type, out rst);
                                        //  maasc.ResourceAssignmentInfo.Resource.Type = rst;

                                        whichvar = "material associated costs resource";
                                        Resource ascres = ppc.Workspace.GlobalLibrary.Resources.Find(ascitem.Resource.Name).Value();
                                        if (ascres != null)
                                        {
                                            if (prop.DirectRateTable.Elements.Find(ascres).HasValue())
                                            {
                                                maasc.ResourceAssignmentInfo.Resource = ascres;
                                            }
                                        }

                                        whichvar = "material associated costs resource fields";
                                        if (ascitem.ResourceFields != null)
                                        {
                                            IEnumerable<ResourceFieldsDto> resourceFields = ascitem.ResourceFields;
                                            foreach (ResourceFieldsDto ritem in resourceFields)
                                            {
                                                if (ritem.Value != string.Empty)
                                                {
                                                    ResourceFieldDefinition rfd = ppc.Workspace.GlobalLibrary.ResourceFieldDefinitions.Find(ritem.Key).Value();
                                                    try
                                                    {
                                                        ResourceFieldStandardValue newval = rfd.ValueList.Find(ritem.Value).Value();
                                                        //  item.Value.Open();
                                                        //  item.Value.BeginEdit();
                                                        ResourceAssignmentInfo.ResourceFieldProperty.GetDescriptor(ppc.Workspace.GlobalLibrary.ResourceFieldDefinitions.Items().ToList().IndexOf(rfd)).SetValue(maasc.ResourceAssignmentInfo, newval);
                                                        // item.Value.EndEdit();
                                                        //  item.Value.Close();
                                                    }
                                                    catch (Exception mx)
                                                    {
                                                        string msg = "Error with field " + ritem.Key + "=" + ritem.Value + " - " + mx.Message;
                                                        this.Logger.LogError(mx, msg);
                                                        System.Diagnostics.Debug.WriteLine(msg);
                                                        rc = rc + msg;
                                                    }
                                                }
                                            }
                                        }

                                        whichvar = "material associated costs spread";
                                        IAmountCollection asd = maasc.Spread.Distribution;

                                        foreach (SpreadDto s in ascitem.Spread)
                                        {
                                            TimeFrame mnyr = TimeFrame.FromMonth(s.Year, s.Month);
                                            asd[mnyr] = double.Parse(s.Value);
                                        }

                                        //AddAmount can also be used to add a new resource to an existing one.

                                        maasc.Spread.Method = SpreadMethod.WeightedAvg;

                                        //  maasc.Spread.Amount = (ascitem.amount != null) ? ascitem.amount : 0;
                                        whichvar = "material associated costs curve";
                                        if (ascitem.SpreadCurve != null && ascitem.SpreadCurve != string.Empty)
                                        {
                                            Curve c = ppc.Workspace.GlobalLibrary.Curves.Find(ascitem.SpreadCurve, CurveType.System).Value();
                                            maasc.Spread.Curve = c;
                                        }

                                        whichvar = "material associated costs start date";
                                        if (ascitem.StartDate != null && ascitem.StartDate != string.Empty)
                                        {
                                            maasc.Spread.StartDate = TimeFrame.FromMonth(int.Parse(ascitem.StartDate.Substring(3, 4)), int.Parse(ascitem.StartDate.Substring(0, 2))).Value;
                                        }

                                        whichvar = "material associated costs end date";
                                        if (ascitem.EndDate != null && ascitem.EndDate != string.Empty)
                                        {
                                            maasc.Spread.EndDate = TimeFrame.FromMonth(int.Parse(ascitem.EndDate.Substring(3, 4)), int.Parse(ascitem.EndDate.Substring(0, 2))).Value;
                                        }

                                        maasc.Amount = ascitem.Amount;
                                        maasc.LinkQty = ascitem.LinkQty;
                                        maasc.LinkSpread = ascitem.LinkSpread;

                                        maasc.EndEdit();
                                    }
                                }

                                ma.EndEdit();
                            }
                            catch (BOBrokenRulesException ex)
                            {
                                this.Logger.LogError(ex, "Error " + " - " + ex.BrokenRules[0]);
                                System.Diagnostics.Debug.WriteLine("Error " + " - " + ex.BrokenRules[0]);
                                rc = rc + "Error with " + whichvar + " - " + ex.BrokenRules[0] + " - " + newma.ResourceAssignment.Name;
                                ma.CancelEdit();
                            }
                            catch (Exception ex)
                            {
                                this.Logger.LogError(ex, "Error Adding Material Assignments");
                                System.Diagnostics.Debug.WriteLine("Error " + " - " + ex.Message);
                                rc = rc + "Error with " + whichvar + " - " + ex.Message + " - " + newma.ResourceAssignment.Name;
                                ma.CancelEdit();
                            }
                        }
                        else
                        {
                            this.Logger.LogError("Resource does not exist in the GDirect Rate table.");
                            System.Diagnostics.Debug.WriteLine("Resource does not exist in the GDirect Rate table.");
                            rc = rc + "Resource does not exist in the GDirect Rate table.";
                        }
                    }
                    else
                    {
                        this.Logger.LogError("Resource does not exist in the Global Library.");
                        System.Diagnostics.Debug.WriteLine("Resource does not exist in the Global Library.");
                        rc = rc + "Resource does not exist in the Global Library.";
                    }
                }
            }

            return rc;
        }

        private string AddTravelAssignments(IProPricerConnection ppc, Proposal prop, TaskDto task, Task myTask)
        {
            string rc = string.Empty; //good
            if (task.Travels != null)
            {
                foreach (TravelsDto newtr in task.Travels)
                {
                    TravelAssignment tra = myTask.Travels.AddNew();
                    string whichvar = "travel assignment";
                    try
                    {
                        tra.BeginEdit();
                        tra.Name = newtr.Name;
                        tra.Description = newtr.Description;
                        tra.DestinationName = newtr.Destination;

                        //  tra.DestinationDescription = newtr.destinationDescription;
                        tra.Comments = newtr.Comments;
                        tra.People = (short) newtr.People;
                        tra.Days = double.Parse(newtr.Days);

                        // check for a resource
                        if (newtr.ResourceAssignment.Name != null)
                        {
                            Resource res = ppc.Workspace.GlobalLibrary.Resources.Find(newtr.ResourceAssignment.Name).Value();
                            if (res != null)
                            {
                                if (prop.DirectRateTable.Elements.Find(res).HasValue())
                                {
                                    tra.ResourceAssignmentInfo.Resource = res;
                                }
                            }
                        }
                        //  tra.Trips = newtr.trips;
                        //  tra.tripCost = newtr.TripCost;
                        //  tra.totalCost = newtr.TotalCost;

                        whichvar = "travel assignment spread";
                        IAmountCollection rsd = tra.Spread.Distribution;

                        foreach (SpreadDto s in newtr.ResourceAssignment.Spread)
                        {
                            TimeFrame mnyr = TimeFrame.FromMonth(s.Year, s.Month);
                            rsd[mnyr] = double.Parse(s.Value);
                        }

                        whichvar = "travel resource fields";
                        if (tra.ResourceAssignmentInfo.Resource != null
                            && newtr.ResourceAssignment.ResourceFields != null)
                        {
                            IEnumerable<ResourceFieldsDto> resourceFields = newtr.ResourceAssignment.ResourceFields;
                            foreach (ResourceFieldsDto ritem in resourceFields)
                            {
                                ResourceFieldDefinition rfd = ppc.Workspace.GlobalLibrary.ResourceFieldDefinitions.Find(ritem.Key).Value();
                                if (ritem.Value == string.Empty)
                                {
                                    try
                                    {
                                        ResourceFieldStandardValue nullval = null;
                                        ResourceAssignmentInfo.ResourceFieldProperty.GetDescriptor(ppc.Workspace.GlobalLibrary.ResourceFieldDefinitions.Items().ToList().IndexOf(rfd)).SetValue(tra.ResourceAssignmentInfo, nullval);
                                    }
                                    catch (Exception mx)
                                    {
                                        string msg = "Error with field " + ritem.Key + "=" + ritem.Value + " - " + mx.Message;
                                        this.Logger.LogError(mx, msg);
                                        System.Diagnostics.Debug.WriteLine(msg);
                                        rc = rc + msg;
                                    }
                                }
                                else
                                {
                                    try
                                    {
                                        ResourceFieldStandardValue newval = rfd.ValueList.Find(ritem.Value).Value();
                                        ResourceAssignmentInfo.ResourceFieldProperty.GetDescriptor(ppc.Workspace.GlobalLibrary.ResourceFieldDefinitions.Items().ToList().IndexOf(rfd)).SetValue(tra.ResourceAssignmentInfo, newval);
                                    }
                                    catch (Exception mx)
                                    {
                                        string msg = "Error with field " + ritem.Key + "=" + ritem.Value + " - " + mx.Message;
                                        this.Logger.LogError(mx, msg);
                                        System.Diagnostics.Debug.WriteLine(msg);
                                        rc = rc + msg;
                                    }
                                }
                            }
                        }

                        whichvar = "travel expense fields";
                        // TravelExpenseDefinition oldtf = null;
                        if (newtr.Expenses != null)
                        {
                            try
                            {
                                foreach (TravelAssignment.Expense oldtf in tra.Expenses.Items())
                                {
                                    foreach (TravelExpenseDto newtf in newtr.Expenses)
                                    {
                                        if (oldtf.Definition.Name == newtf.Name)
                                        {
                                            oldtf.Quantity = double.Parse(newtf.Qty);
                                            if (newtf.Rate != null && newtf.Rate != "0")
                                            {
                                                oldtf.Rate = double.Parse(newtf.Rate);
                                            }

                                            break;
                                        }
                                    }
                                }
                            }
                            catch (BOBrokenRulesException ex)
                            {
                                this.Logger.LogError(ex, "Error " + " - " + ex.BrokenRules[0]);
                                System.Diagnostics.Debug.WriteLine("Error " + " - " + ex.BrokenRules[0]);
                                rc = rc + "Error with " + " - " + ex.BrokenRules[0];
                            }
                            catch (Exception ex)
                            {
                                this.Logger.LogError(ex, "Error Adding Travel Assignments");
                                System.Diagnostics.Debug.WriteLine("Error " + " - " + ex.Message);
                                rc = rc + "Error with " + " - " + ex.Message;
                            }
                        }

                        tra.Spread.Method = SpreadMethod.WeightedAvg;

                        whichvar = "travel assignment spread amount";
                        tra.Spread.Amount = newtr.ResourceAssignment.Amount != null ? double.Parse(newtr.ResourceAssignment.Amount) : 0;
                        whichvar = "travel assignment curve";
                        if (newtr.ResourceAssignment.SpreadCurve != null && newtr.ResourceAssignment.SpreadCurve != string.Empty)
                        {
                            Curve c = ppc.Workspace.GlobalLibrary.Curves.Find(newtr.ResourceAssignment.SpreadCurve, CurveType.System).Value();
                            tra.Spread.Curve = c;
                        }

                        whichvar = "travel assignment start date";
                        //                            ma.Spread = (newma.Spread.Curve != null) ? newma.Spread.Curve.Name : string.Empty;
                        if (newtr.ResourceAssignment.StartDate != null && newtr.ResourceAssignment.StartDate != string.Empty)
                        {
                            tra.Spread.StartDate = TimeFrame.FromMonth(int.Parse(newtr.ResourceAssignment.StartDate.Substring(3, 4)), int.Parse(newtr.ResourceAssignment.StartDate.Substring(0, 2))).Value;
                        }

                        whichvar = "travel assignment end date";
                        if (newtr.ResourceAssignment.EndDate != null && newtr.ResourceAssignment.EndDate != string.Empty)
                        {
                            tra.Spread.EndDate = TimeFrame.FromMonth(int.Parse(newtr.ResourceAssignment.EndDate.Substring(3, 4)), int.Parse(newtr.ResourceAssignment.EndDate.Substring(0, 2))).Value;
                        }

                        tra.EndEdit();
                    }
                    catch (BOBrokenRulesException ex)
                    {
                        this.Logger.LogError(ex, "Error " + " - " + ex.BrokenRules[0]);
                        System.Diagnostics.Debug.WriteLine("Error " + " - " + ex.BrokenRules[0]);
                        rc = rc + "Error with " + whichvar + " - " + ex.BrokenRules[0];
                        tra.CancelEdit();
                    }
                    catch (Exception ex)
                    {
                        this.Logger.LogError(ex, "Error Adding Travel Assignments");
                        System.Diagnostics.Debug.WriteLine("Error " + " - " + ex.Message);
                        rc = rc + "Error with " + whichvar + " - " + ex.Message;
                        tra.CancelEdit();
                    }
                }
            }

            return rc;
        }

        private string AddTaskResource(IProPricerConnection ppc, Proposal prop, TaskDto task, EBS.ProPricer.Model.Task myTask)
        {
            string rc = string.Empty; //good 
            if (task.ResourceAssignments != null)
            {
                foreach (ResourceAssignmentDto item in task.ResourceAssignments)
                {
                    Resource res = ppc.Workspace.GlobalLibrary.Resources.Find(item.Name).Value();
                    if (res != null)
                    {
                        //Check to see if resource exists in the direct rate table
                        if (prop.DirectRateTable.Elements.Find(res).HasValue())
                        {
                            //invoke the add new

                            ResourceAssignment resource = myTask.Resources.AddNew();
                            string whichvar = "add resource";
                            try
                            {
                                //Resource should exist in the underlying rate table.  If not, the addition will fail.
                                //Define the resource parameters.  Resource name and spread amount.  If no date is defined,
                                //the task start/end dates are used.
                                resource.BeginEdit();
                                resource.Info.Resource = res;

                                whichvar = "resource spread amount";
                                //AddAmount can also be used to add a new resource to an existing one.
                                resource.Spread.Amount = item.Amount == null ? 0 : double.Parse(item.Amount);
                                resource.Spread.Method = SpreadMethod.WeightedAvg;
                                whichvar = "resource spread curve";
                                if (item.SpreadCurve != null && item.SpreadCurve != string.Empty)
                                {
                                    Curve c = ppc.Workspace.GlobalLibrary.Curves.Find(item.SpreadCurve, CurveType.System).Value();
                                    resource.Spread.Curve = c;
                                }

                                IAmountCollection rsd = resource.Spread.Distribution;

                                whichvar = "resource spread";
                                foreach (SpreadDto s in item.Spread)
                                {
                                    TimeFrame mnyr = TimeFrame.FromMonth(s.Year, s.Month);
                                    rsd[mnyr] = double.Parse(s.Value);
                                }

                                whichvar = "resource fields";
                                if (item.ResourceFields != null)
                                {
                                    IEnumerable<ResourceFieldsDto> resourceFields = item.ResourceFields;
                                    foreach (ResourceFieldsDto ritem in resourceFields)
                                    {
                                        if (ritem.Value != string.Empty)
                                        {
                                            try
                                            {
                                                //if (ritem.key != "SOURCE")
                                                //   ritem.value = "mike";
                                                ResourceFieldDefinition rfd;

                                                rfd = prop.Locked ? prop.Library.ResourceFieldDefinitions.Find(ritem.Key).Value()
                                                    : ppc.Workspace.GlobalLibrary.ResourceFieldDefinitions.Find(ritem.Key).Value();
                                                
                                                if (!rfd.ValueList.Find(ritem.Value()).HasValue())
                                                {
                                                    if (rfd.AddToList && ritem.Key == "WBS1")
                                                    {
                                                        ResourceFieldStandardValue newRf = rfd.ValueList.AddNew();
                                                        newRf.Value = ritem.Value;
                                                        newRf.EndEdit();
                                                    }
                                                }

                                                ResourceFieldStandardValue newval = rfd.ValueList.Find(ritem.Value).Value();
                                                ResourceAssignmentInfo.ResourceFieldProperty.GetDescriptor(ppc.Workspace.GlobalLibrary.ResourceFieldDefinitions.Items().ToList().IndexOf(rfd)).SetValue(resource.Info, newval);
                                            }
                                            catch (Exception mx)
                                            {
                                                string msg = "Error with field " + ritem.Key + "=" + ritem.Value + " - " + mx.Message;
                                                this.Logger.LogError(mx, msg);
                                                System.Diagnostics.Debug.WriteLine(msg);
                                                rc = rc + msg;
                                            }
                                        }
                                    }
                                }

                                resource.EndEdit();
                            }
                            catch (BOBrokenRulesException ex)
                            {
                                this.Logger.LogError(ex, "Error with " + " - " + ex.BrokenRules[0]);
                                System.Diagnostics.Debug.WriteLine("Error with " + " - " + ex.BrokenRules[0]);
                                rc = rc + "Error with " + whichvar + " - " + ex.BrokenRules[0];
                                resource.CancelEdit();
                            }
                            catch (Exception ex)
                            {
                                this.Logger.LogError(ex, "Error Adding Task Resource");
                                System.Diagnostics.Debug.WriteLine("Error with " + " - " + ex.Message);
                                rc = rc + "Error with " + whichvar + " - " + ex.Message;
                                resource.CancelEdit();
                            }
                        }
                        else
                        {
                            this.Logger.LogError("Resource does not exist in the GDirect Rate table.");
                            System.Diagnostics.Debug.WriteLine("Resource does not exist in the GDirect Rate table.");
                            rc = rc + "Resource does not exist in the GDirect Rate table.";
                        }
                    }
                    else
                    {
                        this.Logger.LogError("Resource does not exist in the Global Library.");
                        System.Diagnostics.Debug.WriteLine("Resource does not exist in the Global Library.");
                        rc = rc + "Resource does not exist in the Global Library.";
                    }
                }
            }

            return rc;
        }

        // PuT api/tasks
        /// <summary>
        /// Methods to read and update tasks and their associated resources
        /// </summary>
        /// <param name="instanceId">The instance identifier.</param>
        /// <param name="proposalAndTasks">The proposal and tasks.</param>
        /// <returns></returns>
        [HttpPut]
        [Route("{instanceId}")]
        public ReturnDto Put(int instanceId, [FromBody] ProposalDto proposalAndTasks)
        {
            if (proposalAndTasks == null)
            {
                ReturnDto retdto = new()
                {
                    Retcode = "500",
                    Retmsg = "No data was entered to change"
                };
                return retdto;
            }

            string rc = "200";
            string whichvar = "editing proposal";
            using (IProPricerConnection ppc = (IProPricerConnection)poolManagerList.GetInstance(instanceId).GetObjectsFromPool())
            {
                try
                {
                    EntityId pEntityId = new(new Guid(proposalAndTasks.Id));
                    Proposal ppProposal = ppc.Workspace.Proposals.Find(pEntityId).Value();

                    //  Proposal ppProposal = ppc.workspace.Proposals.Find(proposalAndTasks.Name, proposalAndTasks.Version).Value;

                    //get burden pool library
                    BurdenPoolLibrary burdenPoolLibrary = ppc.Workspace.GlobalLibrary.BurdenPools;

                    //lock the proposal for modification.
                    ppProposal.Open();
                    ppProposal.BeginEdit();

                    ///////////////////////////////////
                    foreach (TaskDto newt in proposalAndTasks.Tasks)
                    {
                        Optional<Task> tc = ppProposal.Tasks.Find(newt.Name);
                        if (tc.HasValue)
                        {
                            Task t = tc.Value;
                            t.Open();
                            t.BeginEdit();
                            if (t.Resources != null)
                            {
                                foreach (ResourceAssignment resource in t.Resources.Items())
                                {
                                    resource.Open();
                                    resource.BeginEdit();

                                    IEnumerable<ResourceAssignmentDto> resourceAssignments = newt.ResourceAssignments;

                                    foreach (ResourceAssignmentDto ra in resourceAssignments)
                                    {
                                        IEnumerable<ResourceFieldsDto> resourceFields = ra.ResourceFields;
                                        foreach (ResourceFieldsDto item in resourceFields)
                                        {
                                            ResourceFieldDefinition rfd = ppc.Workspace.GlobalLibrary.ResourceFieldDefinitions.Find(item.Key).Value();
                                            try
                                            {
                                                ResourceFieldStandardValue newval = rfd.ValueList.Find(item.Value).Value();
                                                //  item.Value.Open();
                                                //  item.Value.BeginEdit();
                                                ResourceAssignmentInfo.ResourceFieldProperty.GetDescriptor(ppc.Workspace.GlobalLibrary.ResourceFieldDefinitions.Items().ToList().IndexOf(rfd)).SetValue(resource.Info, newval);
                                                // item.Value.EndEdit();
                                                //  item.Value.Close();
                                            }
                                            catch (Exception mx)
                                            {
                                                this.Logger.LogError(mx, "Error Putting Proposal and Tasks into ProPricer");
                                                System.Diagnostics.Debug.WriteLine("Error with field" + " - " + mx.Message);
                                            }
                                        }
                                    }

                                    resource.EndEdit();
                                    resource.Close();
                                }
                            }

                            t.EndEdit();
                            t.Close();
                        }
                    }

                    //End edits and release the record
                    ppProposal.EndEdit();

                    //Close the proposal
                    ppProposal.Close();
                }
                catch (BOBrokenRulesException ex)
                {
                    this.Logger.LogError(ex, "Error with " + whichvar + " - " + ex.BrokenRules[0]);
                    System.Diagnostics.Debug.WriteLine("Error with " + whichvar + " - " + ex.BrokenRules[0]);
                    ReturnDto retdto = new()
                    {
                        Retcode = "500",
                        Retmsg = "Broken rules with " + whichvar + " - " + ex.BrokenRules[0]
                    };
                    return retdto;
                }
                //exception block to catch any other issues with the data
                catch (Exception ex)
                {
                    this.Logger.LogError(ex, "Error with " + whichvar);
                    System.Diagnostics.Debug.WriteLine("Error with " + whichvar + " - " + ex.Message);
                    ReturnDto retdto = new()
                    {
                        Retcode = "500",
                        Retmsg = "Error with " + whichvar + " - " + ex.Message
                    };
                    return retdto;
                }
            }
            ReturnDto goodretdto = new()
            {
                Retcode = rc,
                Retmsg = "Successful"
            };
            return goodretdto;
        }
    }
}