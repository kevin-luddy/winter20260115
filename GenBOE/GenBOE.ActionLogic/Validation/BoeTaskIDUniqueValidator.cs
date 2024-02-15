// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.Validation
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Objects;

    public class BoeTaskIDUniqueValidator : Validator 
    {
        private IFullObjectFactory _FullObectFactory;

        public BoeTaskIDUniqueValidator(IFullObjectFactory inFullObjectFactory)
        {
            this._FullObectFactory = inFullObjectFactory;
        }

        public override Collection<String> validation(object value, Collection<Dictionary<String, String>> inData)
        {
            Collection<String> response = new Collection<string>();

            // the Task ID is not required so a null value is acceptable
            if (value == null)
            {
                return response;
            }
            if (value.GetType() != typeof(string))
            {
                throw new InvalidCastException("value");
            }

            string valueToValidate = (string)value;
            int count = 0;

            Dictionary<String, String> data = inData!= null && inData.Count > 0 ? inData.First<Dictionary<String, String>>() : null;
            if (data != null && (data.Keys.Contains("BOEID") || data.Keys.Contains("BoeID")) && data.Keys.Contains("TaskID") && 
                  (data.Keys.Contains("TaskElementDetailID") || data.Keys.Contains("ODCID") || data.Keys.Contains("TravelID") || data.Keys.Contains("MaterialID")))
            {
                int boeID = data.Keys.Contains("BOEID") ? Convert.ToInt32(data["BOEID"]) : Convert.ToInt32(data["BoeID"]);
                FullBoe boe = this._FullObectFactory.CreateFullBoe(boeID);
                
                string taskIdFromUI = data["TaskID"];
                List<string> taskIds = new List<string> { taskIdFromUI };
                IReadOnlyCollection<BoeTaskElementDTO> boeTaskElements = boe.TaskElements;
                IReadOnlyCollection<OtherDirectCostDTO> odcTaskElements = boe.OtherDirectCosts;
                IReadOnlyCollection<TravelDTO> travelTaskElements = boe.Travels;
                IReadOnlyCollection<MaterialDTO> materialTaskElements = boe.Materials;

                // first check - if task element id already exists, and the TaskID is the same
                // as what was passed in ... it's valid (because it's already been saved, and we 
                // assume whatever has been saved already is valid).
                if (data.Keys.Contains("TaskElementDetailID"))
                {
                    int taskElementId = Convert.ToInt32(data["TaskElementDetailID"]);
					BoeTaskElementDTO boeTaskElement = boeTaskElements.Where(x => x.Id == taskElementId && x.BOETaskID == taskIdFromUI).Select(x => x).FirstOrDefault();
                    if (boeTaskElement != null)
                    {
                        return response; // valid
                    }
                }
                else if (data.Keys.Contains("ODCID"))
                {
                    int taskElementId = Convert.ToInt32(data["ODCID"]);
					OtherDirectCostDTO odcTaskElement = odcTaskElements.Where(x => x.Id == taskElementId && x.TaskID == taskIdFromUI).Select(x => x).FirstOrDefault();
                    if (odcTaskElement != null)
                    {
                        return response; // valid
                    }
                }
                else if (data.Keys.Contains("TravelID"))
                {
                    int taskElementId = Convert.ToInt32(data["TravelID"]);
					TravelDTO travelTaskElement = travelTaskElements.Where(x => x.Id == taskElementId && x.TaskID == taskIdFromUI).Select(x => x).FirstOrDefault();
                    if (travelTaskElement != null)
                    {
                        return response; // valid
                    }
                }
                else if (data.Keys.Contains("MaterialID"))
                {
                    int taskElementId = Convert.ToInt32(data["MaterialID"]);
					MaterialDTO materialTaskElement = materialTaskElements.Where(x => x.Id == taskElementId && x.TaskID == taskIdFromUI).Select(x => x).FirstOrDefault();
                    if (materialTaskElement != null)
                    {
                        return response; // valid
                    }
                }

                // second check (if first didn't prove validity) - look across all task elements for this Id, if it's already present
                // our validation should fail
                taskIds.AddRange(boeTaskElements.Select(x => x.BOETaskID).ToArray());
                taskIds.AddRange(odcTaskElements.Select(x => x.TaskID).ToArray());
                taskIds.AddRange(travelTaskElements.Select(x => x.TaskID).ToArray());
                taskIds.AddRange(materialTaskElements.Select(x => x.TaskID).ToArray());

				int linqresults = (from a in taskIds
                                   where !String.IsNullOrEmpty(a) && a.ToLower() == valueToValidate.ToLower()
                                   select a).Count();
                count = linqresults;
            }


            if (count > 1)
            {
                response.Add("The Task ID must be unique among Labor, Travel and ODC task elements.");
            }
            return response;
        }
    }
}
