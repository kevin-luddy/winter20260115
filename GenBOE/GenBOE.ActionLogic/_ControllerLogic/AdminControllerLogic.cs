// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ControllerLogic
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Web.Mvc;
    using GenBOE.ActionLogic;
    using GenBOE.ActionLogic.Common;
    using GenBOE.ActionLogic.ModelView.Admin;
    using GenBOE.DataBridge.Common;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using IES.Common;
    using IES.Common.classes;
    using IES.Common.Exceptions;
    using ModelView;

    public class AdminControllerLogic : IAdminControllerLogic
    {
        #region Private Properties

        // Create static Regex objects.
        private static readonly Regex regexHourlyRate = new Regex(@"(^\d{0,3}([.]\d{1,2})?$)");
        private static readonly Regex regexPercentToOffload = new Regex(@"(^[0]?(?:\.[0-9]{1,3})$)");

        #endregion

        #region Protected Properties and Constructor

        public virtual string LABOR_RATES_EXAMPLE_LOCATION
        {
            get { return WebConstants.ISGS_LABOR_RATES_EXAMPLE_LOCATION; }
        }

        public virtual string LABOR_RATES_EXAMPLE_NAME
        {
            get { return WebConstants.ISGS_LABOR_RATES_EXAMPLE_NAME; }
        }

        private readonly ICommonDataMapper commonDataMapper;
        private readonly IResourceListDTODataLoader resourceListDTODataLoader;
        private readonly IPermissionsDTODataLoader permissionsDTODataLoader;

        /// <summary>
        /// Constructor
        /// </summary>
        public AdminControllerLogic(ICommonDataMapper inCommonDataMapper,
                                    IResourceListDTODataLoader inIResourceListDTODataLoader,
                                    IPermissionsDTODataLoader permissionsDTODataLoader)
        {
            this.commonDataMapper = inCommonDataMapper;
            this.resourceListDTODataLoader = inIResourceListDTODataLoader;
            this.permissionsDTODataLoader = permissionsDTODataLoader;
        }


        #endregion

        /// <summary>
        /// Logic for Displaying default resources
        /// </summary>
        /// <returns></returns>
        public DefaultResourcesModelView DisplayDefaultResourcesLogic()
        {

            // Initialize modelView to return
            DefaultResourcesModelView theModelView = new DefaultResourcesModelView();

            // Get the global resource list
            ResourceListDTO globalResourceList = this.resourceListDTODataLoader.GetResourceList(CommonConstants.GLOBAL_LIST_ID);

            if (globalResourceList != null)
            {
                // Create the modelview with the list and resources collection
                theModelView.ListID = globalResourceList.ResourceListID;
                theModelView.ListName = globalResourceList.ResourceListName;
                theModelView.UpdateDate = globalResourceList.UpdateDate;
            }

            // gather up element of cost types
            theModelView.elementOfCostTypes = this.commonDataMapper.GetElementOfCostTypes().Select(x => new SelectListItem
            {
                Text = x.ElementOfCostName,
                Value = x.ElementOfCostId.ToString()
            }).ToList();

            // gather up spread/rate types
            ICollection<SelectListItem> rateTypes = this.commonDataMapper.GetRateTypes().Select(x => new SelectListItem
            {
                Text = x.RateTypeName,
                Value = x.RateTypeID.ToString()
            }).ToList();
            theModelView.rateTypes = new Collection<SelectListItem>() { new SelectListItem { Text = "Select Rate Type", Value = "0" } }.Union(rateTypes).ToList();

            return theModelView;
        }       

        /// <summary>
        /// Gets the grid data for the Manage Zone Travel Origins Page
        /// </summary>
        /// <returns>Grid data</returns>
        public virtual ManageMSTZoneTravelOriginsGridModelView GetZoneTravelOriginGridData()
        {
            // do nothing
            return null;
        }

        /// <summary>
        /// Gets the zone travel origins.
        /// </summary>
        /// <returns>A collection of Zone Travel Origin data.</returns>
        public virtual ICollection<MSTZoneTravelOriginModelView> GetZoneTravelOrigins()
        {
            // do nothing
            return null;
        }

        /// <summary>
        /// Gets the grid data for the Manage Zone Travel Destinations Page
        /// </summary>
        /// <returns>grid data</returns>
        public virtual ManageMSTZoneTravelDestinationsGridModelView GetZoneTravelDestinationsGridData()
        {
            // do nothing
            return null;
        }

        /// <summary>
        /// Gets the grid data for the Manage Fees and Costs for Nonzone Travel page
        /// </summary>
        /// <returns>grid data</returns>
        public virtual NonzoneFeesAndCostsGridModelView GetNonzoneFeesAndCostsGridData()
        {
            // do nothing
            return null;
        }

        /// <summary>
        /// Saves Origin and Resource Data for MST Zone Travel - Upsert or Delete
        /// </summary>
        /// <param name="data">Origin MVs with data to be saved</param>
        public virtual void SaveOriginData(ICollection<MSTZoneTravelOriginModelView> data)
        {
            // do nothing
        }

        /// <summary>
        /// Saves Destination Data for MST Zone Travel - updates to assigned zone
        /// </summary>
        /// <param name="data">Destination MV with data to be saved</param>
        public virtual void SaveDestinationData(MSTZoneTravelDestinationModelView data)
        {
            // do nothing
        }

        /// <summary>
        /// Saves Fees and Costs data for Nonzone Travel
        /// </summary>
        /// <param name="data">NonzoneFeesAndCosts MV with data to be saved</param>
        public virtual void SaveFeesAndCostsData(NonzoneFeesAndCostsModelView data)
        {
            // do nothing
        }

        /// <summary>
        /// Validates the escalation rate.
        /// </summary>
        /// <param name="rate">The escalation rate.</param>
        /// <param name="allRates">All current system rates.</param>
        /// <param name="validationErrors">A list of validation errors</param>
        public virtual void ValidateEscalationRate(EscalationRateModelView rate, ICollection<EscalationRatesDTO> allRates, ICollection<ValidationMessage> validationErrors)
        {
            if (validationErrors is null)
            {
                throw new ArgumentNullException(nameof(validationErrors));
            }

            if (rate is null)
            {
                throw new ArgumentNullException(nameof(rate));
            }

            if (allRates is null)
            {
                throw new ArgumentNullException(nameof(allRates));
            }

            if (rate.Year < 1990 || rate.Year > 2400)
            {
                validationErrors.Add(new ValidationMessage("ValidYear", "Year must be between 1990 and 2400."));
            }

            // See if we are trying to add a rate for a year that already exists.
            // OR change an existing rate year to one that already exists
            if ((allRates.FirstOrDefault(x => x.Year == rate.Year && rate.EscalationRateID < 0) != null) ||
                (allRates.FirstOrDefault(x => x.Year == rate.Year && rate.EscalationRateID > 0 && x.EscalationRateID != rate.EscalationRateID) != null))
            {
                validationErrors.Add(new ValidationMessage("UniqueYear", "Escalation Rate Year must be unique."));
            }
        }

        /// <summary>
        /// Validates the offload rate.
        /// </summary>
        /// <param name="rate">The rate.</param>
        /// <param name="allRates">All current offload rates.</param>
        /// <param name="validationErrors">A list of validation errors.</param>
        public virtual void ValidateOffloadRate(OffloadRateModelView rate, ICollection<OffloadRatesDTO> allRates, Collection<ValidationMessage> validationErrors)
        {
            if (validationErrors is null) { throw new ArgumentNullException(nameof(validationErrors)); }
            if (rate is null) { throw new ArgumentNullException(nameof(rate)); }
            if (allRates is null) { throw new ArgumentNullException(nameof(allRates)); }

            ICollection<OffloadRatesDTO> allRatesExcludingCurrent = allRates.Where(r => r.Id != rate.OffloadRateID).ToCollection() ;

            if (rate.Year < 1990 || rate.Year > 2400)
            {
                validationErrors.Add(new ValidationMessage("ValidYear", "Year must be between 1990 and 2400."));
            }

            if (string.IsNullOrEmpty(rate.Resource))
            {
                validationErrors.Add(new ValidationMessage("Resource", "Resource is a required field."));
            }

            if (string.IsNullOrEmpty(rate.PerformingOrg))
            {
                validationErrors.Add(new ValidationMessage("PerformingOrg", "Performing Org is a required field."));
            }

            if (string.IsNullOrEmpty(rate.SubcontractorResource))
            {
                validationErrors.Add(new ValidationMessage("SubcontractorResource", "Subcontractor Resource is a required field."));
            }

            if (!regexHourlyRate.IsMatch(rate.HourlyRate.ToString()) || rate.HourlyRate <= 0m)
            {
                validationErrors.Add(new ValidationMessage("HourlyRate", "Hourly Rate must be a decimal between 0.01 and 999.99 with up to 2 decimal places."));
            }

            if (!regexPercentToOffload.IsMatch(rate.PercentToOffload.ToString()))
            {
                validationErrors.Add(new ValidationMessage("PercentToOffload", "Percent To Offload must be a decimal between 0.001 and 1.000 with up to 3 decimal places."));
            }

            // The same resource/performing org has to offload to the same sub resource (across all years/records)
            if(allRatesExcludingCurrent.Any(x => x.Resource == rate.Resource && x.PerformingOrg == rate.PerformingOrg && x.SubResource != rate.SubcontractorResource))
            {
                validationErrors.Add(new ValidationMessage("ResourceToSub", "Combination of Resource and Performing Org must be offloaded to the same Subcontractor Resource for all dates."));
            }

            // The same resource/performing org has to have the same offload % (across all years/records)
            if (allRatesExcludingCurrent.Any(x => x.Resource == rate.Resource && x.PerformingOrg == rate.PerformingOrg && x.Percent != rate.PercentToOffload))
            {
                validationErrors.Add(new ValidationMessage("ResourcePercentage", "Combination of Resource and Performing Org must have the same Percent To Offload for all dates."));
            }

            // See if we are trying to add a rate for a year-resource-perforg combo that already exists.
            // OR change an existing rate year-resource-perforg combo to one that already exists
            if ((allRates.FirstOrDefault(x => x.Year == rate.Year && x.Resource == rate.Resource && x.PerformingOrg == rate.PerformingOrg && rate.OffloadRateID < 0) != null) ||
                (allRates.FirstOrDefault(x => x.Year == rate.Year && x.Resource == rate.Resource && x.PerformingOrg == rate.PerformingOrg && rate.OffloadRateID > 0 && x.Id != rate.OffloadRateID) != null))
            {
                validationErrors.Add(new ValidationMessage("UniqueCombination", "Combination of Offload Rate Year, Resource, and Performing Org must be unique."));
            }
        }

        /// <summary>
        /// Kicks off DataAnnotation validation for the system settings
        /// </summary>
        /// <param name="systemSettings">System settings to validate</param>
        /// <param name="validationErrors">A list of validation errors.</param>
        public virtual void ValidateSystemSettings(ICollection<SystemSettingDTO> systemSettings, Collection<ValidationMessage> validationErrors)
        {
            if (systemSettings is null) { throw new ArgumentNullException(nameof(systemSettings)); }
            if (validationErrors is null) { throw new ArgumentNullException(nameof(validationErrors)); }

            foreach (var item in systemSettings.Select((value, i) => new { i, value }))
            {
                this.ValidateSystemSetting(item.value, item.i, validationErrors);
            }
        }

        /// <summary>
        /// Runs data validation using model view annotations
        /// </summary>
        /// <param name="systemSetting">System setting to validate</param>
        /// <param name="rowNum">Row index number</param>
        /// <param name="validationErrors">A list of validation errors.</param>
        private void ValidateSystemSetting(SystemSettingDTO systemSetting, int rowNum, Collection<ValidationMessage> validationErrors)
        {
            ValidationContext ctx = new ValidationContext(systemSetting, null, null);
            List<ValidationResult> errors = new List<ValidationResult>();
            Validator.TryValidateObject(systemSetting, ctx, errors, true);

            foreach (ValidationResult error in errors)
            {
                validationErrors.Add(new ValidationMessage
                {
                    ValidationIssue = error.ErrorMessage.Replace(@"&nbsp;", ""),
                    TreatAsWarning = false,
                    RowIndex = rowNum
                });
            }
        }

        /// <summary>
        /// Peform validation that is shared between saving or editing an Output Format Template
        /// </summary>
        /// <param name="templateName">Template Name</param>
        /// <param name="templateDescription">Template Description</param>
        /// <returns>Collection of any validation messages</returns>
        public Collection<ValidationMessage> ValidateTemplateOnSaveOrEdit(string templateName, string templateDescription)
        {
            Collection<ValidationMessage> toReturn = new Collection<ValidationMessage>();

            if (string.IsNullOrEmpty(templateName))
            {
                toReturn.Add(new ValidationMessage("Template Name is required."));
            }

            if (string.IsNullOrEmpty(templateDescription))
            {
                toReturn.Add(new ValidationMessage("Template Description is required."));
            }

            List<string> reservedCharacters = new List<string>() { "<", ">", ":", "\"", "'", "\\", "/", "|", "?", "*" };
            if (templateName != null && reservedCharacters.Any(x => templateName.Contains(x)))
            {
                toReturn.Add(new ValidationMessage("Template Name cannot contain any of the following reserved characters: < > : \" ' \\ / | ? *"));
            }

            return toReturn;
        }

        /// <summary>
        /// Gets the overdue training for users in the system.
        /// </summary>
        /// <param name="activeDirectoryUtilities">The AD Utilities class.</param>
        /// <param name="models">The models.</param>
        /// <param name="courseName">The name of the course to get overdue training for.</param>
        /// <returns>A list of users that have overdue training along with last completed date.</returns>
        public ICollection<TrainingModelView> GetOverdueTraining(IActiveDirectoryUtilities activeDirectoryUtilities, ICollection<TrainingModelView> models, string courseName)
        {
            if (activeDirectoryUtilities == null)
            {
                throw new ArgumentNullException(nameof(activeDirectoryUtilities));
            }

            if (models == null)
            {
                throw new ArgumentNullException(nameof(models));
            }

            if (string.IsNullOrWhiteSpace(courseName))
            {
                throw new ArgumentNullException(nameof(courseName));
            }

            TrainingCourseGroupModelView group = this.GetTrainingCourseGroups().FirstOrDefault(g => g.CourseGroupName == courseName);

            if (group == null)
            {
                throw new ArgumentException("The course group with name " + courseName + " was not found.", "courseName");
            }

            ICollection<TrainingModelView> overdueList = new List<TrainingModelView>();
            ICollection<GroupData> groups = activeDirectoryUtilities.GetAuthorizationGroupsFromWebConfig();

            // Filter out dev groups
            groups = groups.Where(g => g.Ntid.ToLower() != "ebs.estimationinitiative.devteam" && g.Ntid.ToLower() != "ebs.estimationinitiative.aacctdevteam").ToList();

            Dictionary <string, UserData> userDictionary = new Dictionary<string, UserData>();

            ICollection<string> roleNtIds = null;
            // get roles if needed
            if (group.PermissionRequired.HasValue)
            {
                roleNtIds = this.permissionsDTODataLoader.GetAllNtIdsForWorkspacePermission(group.PermissionRequired.Value).Select(s => s.ToLower()).ToList();
            }


            // For each group, check to see if the user has valid training 
            foreach (GroupData individualGroup in groups)
            {
                try
                {
                    ICollection<UserData> userNames = activeDirectoryUtilities.GetAdGroupUsers(individualGroup.Ntid);

                    foreach (UserData user in userNames)
                    {
                        userDictionary[user.Ntid] = user;
                    }
                }
                catch (GeneralAppException) { } // group does not exist, no harm done, just go to the next one
            }

            foreach (UserData user in userDictionary.Values)
            {
                // if using a role and not in role, then skip this user.  No need to validate against user.
                if (group.PermissionRequired.HasValue && !roleNtIds.Contains(user.Ntid.ToLower()))
                {
                    continue;
                }

                bool isValid = false;
                DateTime? lastCompleted = null;
                TrainingModelView invalidTraining = null;
                // loop through all courses in the group to see if any valid
                foreach (TrainingCourseModelView course in group.Courses)
                {
                    ICollection<TrainingModelView> trainingList = models.Where(m => course.CourseID == m.CourseId && m.UserId == user.EmployeeId).ToList();
                    if (trainingList.Any())
                    {
                        foreach (TrainingModelView training in trainingList)
                        {
                            // Was the course still applicable when it was last completed?
                            if (course.ValidUntil.HasValue && training.LastCompleted.HasValue && course.ValidUntil < training.LastCompleted)
                            {
                                // course was last taken after it was last valid, skip it
                                invalidTraining = training;
                                continue;
                            }

                            // keep track of last course valid (not training valid) completed through the multiple courses.
                            if (!lastCompleted.HasValue || lastCompleted < training.LastCompleted)
                            {
                                lastCompleted = training.LastCompleted;
                            }

                            // Check to see if the course is valid if taken once, or if it was last completed after the deadline for valid completions
                            if (course.IsValidOnce || training.LastCompleted > course.LastCompletedByDate)
                            {
                                isValid = true;
                                break;
                            }
                        }
                    }
                }

                if (!isValid)
                {
                    if (!lastCompleted.HasValue && invalidTraining != null)
                    {
                        // they haven't completed any of the current training, but have an invalid date for older training
                        overdueList.Add(invalidTraining);
                    }
                    else
                    {
                        // training is missing or out of date for this training group
                        TrainingModelView training = new TrainingModelView
                        {
                            CourseId = group.Courses.First().CourseID,
                            UserDisplayName = user.DisplayName,
                            UserId = user.EmployeeId,
                            LastCompleted = lastCompleted
                        };

                        overdueList.Add(training);
                    }
                }
            }

            return overdueList.OrderBy(o => o.UserDisplayName).ToList();
        }

        /// <summary>
        /// Gets the training course groups.
        /// </summary>
        /// <returns>Training course groups</returns>
        public virtual ICollection<TrainingCourseGroupModelView> GetTrainingCourseGroups()
        {
            List<TrainingCourseGroupModelView> groups = new List<TrainingCourseGroupModelView>();
            DateTime twoYearsAgo = DateTime.Now.AddYears(-2);

            // TINA
            groups.Add(new TrainingCourseGroupModelView
            {
                CourseGroupName = "TINA Training",
                Courses = new List<TrainingCourseModelView>
                {
                    new TrainingCourseModelView
                    {
                        CourseID = Constants.TINA_TRAINING_COURSEID,
                        LastCompletedByDate = twoYearsAgo
                    }
                }
            });

            return groups;
        }
    }
}
