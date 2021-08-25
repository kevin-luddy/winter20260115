// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.NewValidation
{

    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common;
    using IES.Common.Exceptions;

    /// <summary>
    /// Validates Project Map data
    /// </summary>
    public class ProjectMapValidator
    {
        /// <summary>
        /// Model views to validate passed into constructor
        /// </summary>
        private readonly ProjectMapModelView[] projectMapModelViews;

        /// <summary>
        /// Workspace resources and performing orgs are used to validate resource and cost center
        /// </summary>
        private readonly FullWorkspace ws;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="projectMapModelViews">Array of ProjectMapModelView objects</param>
        /// <param name="ws">FullWorkspace</param>
        public ProjectMapValidator(ProjectMapModelView[] projectMapModelViews, FullWorkspace ws)
        {
            this.projectMapModelViews = projectMapModelViews;
            this.ValidationMessages = new Collection<ValidationMessage>();
            this.ws = ws;
        }

        /// <summary>
        /// Contains list of validation issues discovered during validation
        /// </summary>
        public Collection<ValidationMessage> ValidationMessages { get; }

        /// <summary>
        /// Executes all validation methods and builds validation summary
        /// </summary>
        public void Validate()
        {
            this.RunModelValidation();
            this.ValidateActIdResourceWbsIsUnique();
            this.ValidateActIdDatesIsUnique();
            this.ValidateActIdActNameWbsIsUnique();
            this.ValidateActNameInitialResourceIsUnique();
            this.ValidateActIdWbsCategoryIsUnique();
            this.ValidateProjectLengthLessThanMaxYears();
            this.ValidateInitialResourceExistsInResourceTable();
            this.ValidateCostCenterExistsInPerformingOrgTable();
            this.ValidateHoursCostsAreValidResourceType();
            this.ValidateProjectMapTypeHasValidBucketizedData();
            this.ValidateBucketizedSpreadsStartEndDates();
            this.ValidateBucketizedSpreadsHoursCosts();
            this.ValidateWBSIsConsistent();
            this.ValidatePrecision();

            Dictionary<string, ICollection<ProjectMapModelView>> tieredData = ProjectMapConverter.GroupByTiers(this.projectMapModelViews);
            this.ValidateClassOfCostIsConsistent(tieredData);
            this.ValidateCamNameIsConsistent(tieredData);
            this.ValidateCategoryIsConsistent(tieredData);
            this.ValidateRationaleIsConsistent(tieredData);
        }

        /// <summary>
        /// Validates the precision of dollars and hours.
        /// </summary>
        public void ValidatePrecision()
        {
            foreach (var item in this.projectMapModelViews.Select((value, i) => new { i, value }))
            {
                if (item.value.Hours.HasValue)
                {
                    decimal hours = Utilities.AdjustPrecision(item.value.Hours.Value, this.ws.DecimalPrecision);
                    if (hours != item.value.Hours.Value)
                    {
                        this.ValidationMessages.Add(new ValidationMessage
                        {
                            ValidationIssue = $"Hours value is not within decimal precision range of {this.ws.DecimalPrecision}",
                            RowIndex = item.i
                        });
                    }
                }
                else if (item.value.Dollars.HasValue)
                {
                    decimal dollars = Utilities.AdjustPrecision(item.value.Dollars.Value, this.ws.CostDecimalPrecision);
                    if (dollars != item.value.Dollars.Value)
                    {
                        this.ValidationMessages.Add(new ValidationMessage
                        {
                            ValidationIssue = $"Dollars value is not within decimal precision range of {this.ws.CostDecimalPrecision}",
                            RowIndex = item.i
                        });
                    }
                }

                if (item.value.TieredPercentage.HasValue)
                {
                    // Tiered Percentage must be between 0 and 999.9 with up to 1 decimal place of precision.
                    decimal tieredPercentage = Utilities.AdjustPrecision(item.value.TieredPercentage.Value, 1);
                    if (tieredPercentage != item.value.TieredPercentage.Value)
                    {
                        this.ValidationMessages.Add(new ValidationMessage
                        {
                            ValidationIssue = "Tiered Percentage value is not within decimal precision range of 1",
                            RowIndex = item.i
                        });
                    } else if (tieredPercentage < 0 || tieredPercentage > 999.9m)
                    {
                        this.ValidationMessages.Add(new ValidationMessage
                        {
                            ValidationIssue = "Tiered Percentage value must be between 0 and 999.9",
                            RowIndex = item.i
                        });
                    }
                }

                if (this.ws.ProjectMapType == ProjectMapType.TimePhasedProjectMap && item.value.DiscreteMonths != null)
                {
                    // validate spreads
                    int precisionForSpreads = item.value.Hours.HasValue ? this.ws.CostDecimalPrecision : this.ws.DecimalPrecision;
                    for (int j = 0; j < item.value.DiscreteMonths.Length; j++)
                    {
                        decimal? monthValue = item.value.DiscreteMonths[j];
                        if (monthValue.HasValue)
                        {
                            decimal monthAdjusted = Utilities.AdjustPrecision(monthValue.Value, precisionForSpreads);
                            if (monthValue.Value != monthAdjusted)
                            {
                                this.ValidationMessages.Add(new ValidationMessage
                                {
                                    ValidationIssue = $"Discrete Month M{j + 1} value is not within decimal precision range of {this.ws.CostDecimalPrecision}",
                                    TreatAsWarning = true,
                                    RowIndex = item.i
                                });
                            }
                        }
                    }
                }
                this.ValidateObject(item.value, item.i);
            }          
        }

        /// <summary>
        /// Kicks off DataAnnotation validation for the class
        /// </summary>
        public void RunModelValidation()
        {
            foreach (var item in this.projectMapModelViews.Select((value, i) => new { i, value }))
            {
                this.ValidateObject(item.value, item.i);
            }
        }

        /// <summary>
        /// Runs data validation using model view annotations
        /// </summary>
        /// <param name="projectMapModelView">Object to validate</param>
        /// <param name="rowNum">Index number from the import file</param>
        public void ValidateObject(ProjectMapModelView projectMapModelView, int rowNum)
        {
            ValidationContext ctx = new ValidationContext(projectMapModelView, null, null);
            List<ValidationResult> errors = new List<ValidationResult>();
            Validator.TryValidateObject(projectMapModelView, ctx, errors, true);

            foreach (ValidationResult error in errors)
            {
                this.ValidationMessages.Add(new ValidationMessage
                {
                    ValidationIssue = error.ErrorMessage.Replace(@"&nbsp;", ""),
                    TreatAsWarning = false,
                    RowIndex = rowNum
                });
            }
        }

        /// <summary>
        /// Ensure no duplicates exist for composite Key ActivityId, InitialResource, Cost Center, WbsNumber, WbsElementTitle
        /// </summary>
        public void ValidateActIdResourceWbsIsUnique()
        {
            var duplicates = this.projectMapModelViews.Select((x, i) => new { index = i, value = x })
                .GroupBy(x => new
                {
                    x.value.ActivityID,
                    x.value.InitialResource,
                    x.value.CostCenter,
                    x.value.WbsNumber,
                    x.value.WbsElementTitle
                })
                .Where(x => x.Skip(1).Any())
                .ToArray();

            if (duplicates.Any())
            {
                //generate validation messages for each invalid record
                foreach (var duplicate in duplicates)
                {
                    foreach (var row in duplicate)
                    {
                        this.ValidationMessages.Add(new ValidationMessage
                        {
                            ValidationIssue =
                                "Activity ID, Activity Type Code, Cost Center, WBS Number, and WBS Element Title are not unique",
                            RowIndex = row.index,
                            FieldName =
                                $"Activity ID={row.value.ActivityID}, Activity Type Code={row.value.InitialResource}, Cost Center={row.value.CostCenter}, "
                                + $"WBS Number={row.value.WbsNumber}, WBS Element Title={row.value.WbsElementTitle}"
                        });
                    }
                }
            }
        }

        /// <summary>
        /// Ensure no duplicates exist for composite Key ActivityId, Resource, Cost Center, StartDate, and EndDate
        /// </summary>
        public void ValidateActIdDatesIsUnique()
        {
            var duplicates = this.projectMapModelViews.Select((x, i) => new { index = i, value = x })
                .GroupBy(x => new { x.value.ActivityID, x.value.StartDate, x.value.EndDate, x.value.InitialResource, x.value.CostCenter })
                .Where(x => x.Skip(1).Any())
                .ToArray();

            if (duplicates.Any())
            {
                //generate validation messages for each invalid record
                foreach (var duplicate in duplicates)
                {
                    foreach (var row in duplicate)
                    {
                        this.ValidationMessages.Add(new ValidationMessage
                        {
                            ValidationIssue = "Activity ID, Activity Type Code, Cost Center, Start Date, and End Date are not unique",
                            RowIndex = row.index,
                            FieldName =
                                $"Activity ID={row.value.ActivityID}, Activity Type Code={row.value.InitialResource}, Cost Center={row.value.CostCenter}, "
                                + $"Start Date={row.value.StartDate}, End Date={row.value.EndDate}"
                        });
                    }
                }
            }
        }

        /// <summary>
        /// Ensure no duplicates exist for composite Key ActivityId, ActivityName, Resource, Cost Center, WbsNumber, and WbsElementTitle
        /// </summary>
        public void ValidateActIdActNameWbsIsUnique()
        {
            var duplicates = this.projectMapModelViews.Select((x, i) => new { index = i, value = x })
                .GroupBy(
                    x => new { x.value.ActivityID, x.value.ActivityName, x.value.WbsNumber, x.value.WbsElementTitle, x.value.InitialResource, x.value.CostCenter })
                .Where(x => x.Skip(1).Any())
                .ToArray();

            if (duplicates.Any())
            {
                //generate validation messages for each invalid record
                foreach (var duplicate in duplicates)
                {
                    foreach (var row in duplicate)
                    {
                        this.ValidationMessages.Add(new ValidationMessage
                        {
                            ValidationIssue =
                                "Activity ID, Activity Name, Activity Type Code, Cost Center, WBS Number, and WBS Element Title and are not unique",
                            RowIndex = row.index,
                            FieldName =
                                $"Activity ID={row.value.ActivityID}, Activity Name={row.value.ActivityName}, Activity Type Code={row.value.InitialResource}, Cost Center={row.value.CostCenter}, "
                                + $"WBS Number={row.value.WbsNumber}, WBS Element Title={row.value.WbsElementTitle}"
                        });
                    }
                }
            }
        }

        /// <summary>
        /// Ensure no duplicates exist for composite Key ActivityName, InitialResource and Cost Center
        /// </summary>
        public void ValidateActNameInitialResourceIsUnique()
        {
            var duplicates = this.projectMapModelViews.Select((x, i) => new { index = i, value = x })
                .GroupBy(x => new { x.value.ActivityName, x.value.InitialResource, x.value.CostCenter })
                .Where(x => x.Skip(1).Any())
                .ToArray();

            if (duplicates.Any())
            {
                //generate validation messages for each invalid record
                foreach (var duplicate in duplicates)
                {
                    foreach (var row in duplicate)
                    {
                        this.ValidationMessages.Add(new ValidationMessage
                        {
                            ValidationIssue = "Activity Name, Activity Type Code and Cost Center are not unique",
                            TreatAsWarning = true,
                            RowIndex = row.index,
                            FieldName =
                                $"Activity Name={row.value.ActivityName}, Activity Type Code={row.value.InitialResource}, Cost Center={row.value.CostCenter}"
                        });
                    }
                }
            }
        }

        /// <summary>
        /// Validates the WBS Number and Element Title are consistent.
        /// </summary>
        public void ValidateWBSIsConsistent()
        {
            Dictionary<string, string> wbs = new Dictionary<string, string>();
            var rows = this.projectMapModelViews.Select((x, i) => new { index = i, value = x });
            foreach (var row in rows)
            {
                if (!string.IsNullOrWhiteSpace(row.value.WbsNumber))
                {
                    if (wbs.ContainsKey(row.value.WbsNumber))
                    {
                        if (row.value.WbsElementTitle != wbs[row.value.WbsNumber])
                        {
                            // the Element titles do not match for the same WBS Number
                            this.ValidationMessages.Add(new ValidationMessage
                            {
                                ValidationIssue = "WBS Number and WBS Element Title are not consistent",
                                RowIndex = row.index,
                                FieldName =
                                    $"WBS Number={row.value.WbsNumber}, WBS Element Title={row.value.WbsElementTitle}"
                            });
                        }
                    }
                    else
                    {
                        wbs.Add(row.value.WbsNumber, row.value.WbsElementTitle);
                    }
                }
            }
        }

        /// <summary>
        /// Validates the class of cost is unique inside a grouped tier.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public void ValidateClassOfCostIsConsistent(Dictionary<string, ICollection<ProjectMapModelView>> tieredData)
        {
            if (tieredData == null)
            {
                throw new ArgumentNullException(nameof(tieredData));
            }
            
            foreach (KeyValuePair<string, ICollection<ProjectMapModelView>> group in tieredData)
            {
                var duplicates = group.Value.GroupBy(x => new { x.ClassOfCost })
                    .ToArray();

                if (duplicates.Length > 1)
                {
                    //generate validation messages for each invalid record
                    foreach (var duplicate in duplicates)
                    {
                        foreach (var row in duplicate)
                        {
                            this.ValidationMessages.Add(new ValidationMessage
                            {
                                ValidationIssue = "Class of Cost is not consistent inside of a grouped tier (BOE)",
                                FieldName =
                                    $"Activity Name={row.ActivityName}, Activity Type Code={row.InitialResource}, Cost Center={row.CostCenter}, Class of Cost={row.ClassOfCost}"
                            });
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Validates the rationale is consistent.
        /// </summary>
        /// <param name="tieredData">The tiered data.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public void ValidateRationaleIsConsistent(Dictionary<string, ICollection<ProjectMapModelView>> tieredData)
        {
            if (tieredData == null)
            {
                throw new ArgumentNullException(nameof(tieredData));
            }

            foreach (KeyValuePair<string, ICollection<ProjectMapModelView>> group in tieredData)
            {
                var duplicates = group.Value.GroupBy(x => new { x.Rationale })
                    .ToArray();

                if (duplicates.Length > 1)
                {
                    //generate validation messages for each invalid record
                    foreach (var duplicate in duplicates)
                    {
                        foreach (var row in duplicate)
                        {
                            this.ValidationMessages.Add(new ValidationMessage
                            {
                                ValidationIssue = "Rationale is not consistent inside of a grouped tier (BOE)",
                                FieldName =
                                    $"Activity Name={row.ActivityName}, Activity Type Code={row.InitialResource}, Cost Center={row.CostCenter}, Rationale={row.Rationale}"
                            });
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Validates the category is consistent.
        /// </summary>
        /// <param name="tieredData">The tiered data.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public void ValidateCategoryIsConsistent(Dictionary<string, ICollection<ProjectMapModelView>> tieredData)
        {
            if (tieredData == null)
            {
                throw new ArgumentNullException(nameof(tieredData));
            }

            foreach (KeyValuePair<string, ICollection<ProjectMapModelView>> group in tieredData)
            {
                var duplicates = group.Value.GroupBy(x => new { x.Category })
                    .ToArray();

                if (duplicates.Length > 1)
                {
                    //generate validation messages for each invalid record
                    foreach (var duplicate in duplicates)
                    {
                        foreach (var row in duplicate)
                        {
                            this.ValidationMessages.Add(new ValidationMessage
                            {
                                ValidationIssue = "Category is not consistent inside of a grouped tier (BOE)",
                                FieldName =
                                    $"Activity Name={row.ActivityName}, Activity Type Code={row.InitialResource}, Cost Center={row.CostCenter}, Category={row.Category}"
                            });
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Validates the cam name is consistent.
        /// </summary>
        /// <param name="tieredData">The tiered data.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public void ValidateCamNameIsConsistent(Dictionary<string, ICollection<ProjectMapModelView>> tieredData)
        {
            if (tieredData == null)
            {
                throw new ArgumentNullException(nameof(tieredData));
            }

            foreach (KeyValuePair<string, ICollection<ProjectMapModelView>> group in tieredData)
            {
                var duplicates = group.Value.GroupBy(x => new { x.CamName })
                    .ToArray();

                if (duplicates.Length > 1)
                {
                    //generate validation messages for each invalid record
                    foreach (var duplicate in duplicates)
                    {
                        foreach (var row in duplicate)
                        {
                            this.ValidationMessages.Add(new ValidationMessage
                            {
                                ValidationIssue = "Cam Name is not consistent inside of a grouped tier (BOE)",
                                FieldName =
                                    $"Activity Name={row.ActivityName}, Activity Type Code={row.InitialResource}, Cost Center={row.CostCenter}, Cam Name={row.CamName}"
                            });
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Ensure no duplicates exist for composite Key ActivityId, Resource, Cost Center, WbsNumber, WbsElementTitle, and Category
        /// </summary>
        public void ValidateActIdWbsCategoryIsUnique()
        {
            var duplicates = this.projectMapModelViews.Select((x, i) => new { index = i, value = x })
                .GroupBy(x => new { x.value.ActivityID, x.value.WbsNumber, x.value.WbsElementTitle, x.value.Category, x.value.InitialResource, x.value.CostCenter })
                .Where(x => x.Skip(1).Any())
                .ToArray();

            if (duplicates.Any())
            {
                //generate validation messages for each invalid record
                foreach (var duplicate in duplicates)
                {
                    foreach (var row in duplicate)
                    {
                        this.ValidationMessages.Add(new ValidationMessage
                        {
                            ValidationIssue = "Activity ID, Activity Type Code, Cost Center, WBS Number, WBS ElementTitle, and Category are not unique",
                            RowIndex = row.index,
                            FieldName =
                                $"Activity ID={row.value.ActivityID}, Activity Type Code={row.value.InitialResource}, Cost Center={row.value.CostCenter}, "
                                + $"WBS Number={row.value.WbsNumber}, WBS ElementTitle={row.value.WbsElementTitle}, Category={row.value.Category}"
                        });
                    }
                }
            }
        }

        /// <summary>
        /// Validates that the difference between the earliest StartDate and the Latest EndDate is not more than 17 years
        /// </summary>
        public void ValidateProjectLengthLessThanMaxYears()
        {
            //Retrieve Minimum Date
            DateTime? minDate = (from d in this.projectMapModelViews select d.StartDate).Min();

            //Retrieve Maximum Date
            DateTime? maxDate = (from d in this.projectMapModelViews select d.EndDate).Max();

            if (minDate.HasValue && maxDate.HasValue)
            {
                int years = maxDate.Value.Year - minDate.Value.Year + 1;

                // max time-phased (showing buckets for each month) is 17, max non-timephased is 45
                int maxYears = this.ws.ProjectMapType == ProjectMapType.TimePhasedProjectMap ? 17 : 45;

                if (years > maxYears)
                {
                    //generate validation messages for each invalid record
                    this.ValidationMessages.Add(new ValidationMessage
                    {
                        ValidationIssue = $"Earliest start date and latest end date spans more than {maxYears} years",
                        FieldName = $"Earliest Start Date={minDate}, Latest End Date={maxDate}"
                    });
                }
                else if (years > 17 && this.ws.ProjectMapType == ProjectMapType.NonTimePhasedProjectMap)
                {
                    // generate warning about reports/exports only showing 17 years
                    this.ValidationMessages.Add(new ValidationMessage
                    {
                        ValidationIssue = "You loaded more than 17 years of data. Reports which list year by year data will not work correctly, as those support maximum of 17 years.",
                        FieldName = $"Earliest Start Date={minDate}, Latest End Date={maxDate}",
                        TreatAsWarning = true
                    }); 
                }
            }
        }

        /// <summary>
        /// Validates each resource by verifying that it exists in the workspace resource table
        /// </summary>
        public void ValidateInitialResourceExistsInResourceTable()
        {
            HashSet<ResourceDTO> resourceTable = new HashSet<ResourceDTO>(this.ws.ResourcesForWsResourceListId);
            var notInResourceTable = this.projectMapModelViews.Select((x, i) => new { index = i, value = x })
                .Where(pm => resourceTable.All(r => r.ResourceName.ToUpper() != pm.value.InitialResource?.ToUpper()))
                .ToList();

            if (notInResourceTable.Any())
            {
                //generate validation message for each invalid record
                foreach (var projectMapModelView in notInResourceTable)
                {
                    this.ValidationMessages.Add(new ValidationMessage
                    {
                        ValidationIssue = "Activity Type Code does not exist in the Resource table",
                        RowIndex = projectMapModelView.index,
                        FieldName = $"Activity Type Code={projectMapModelView.value.InitialResource}"
                    });
                }
            }

            HashSet<ResourceDTO> invalidResourceTable = new HashSet<ResourceDTO>(this.ws.ResourcesForWsResourceListId.Where(r => r.ElementOfCost != ElementOfCostType.LMLabor && r.ElementOfCost != ElementOfCostType.IWTA && r.ElementOfCost != ElementOfCostType.Travel && r.ElementOfCost != ElementOfCostType.ODC));
            var invalidResources = this.projectMapModelViews.Select((x, i) => new { index = i, value = x })
                .Where(pm => invalidResourceTable.Any(r => r.ResourceName.ToUpper() == pm.value.InitialResource?.ToUpper()))
                .ToList();

            if (invalidResources.Any())
            {
                //generate validation message for each invalid record
                foreach (var projectMapModelView in invalidResources)
                {
                    this.ValidationMessages.Add(new ValidationMessage
                    {
                        ValidationIssue = "Activity Type Code is set to a non-labor Resource",
                        RowIndex = projectMapModelView.index,
                        FieldName = $"Activity Type Code={projectMapModelView.value.InitialResource}"
                    });
                }
            }
        }

        /// <summary>
        /// Validates each CostCenter by verifying that it exists in the workspace PerformingOrg table
        /// </summary>
        public void ValidateCostCenterExistsInPerformingOrgTable()
        {
            HashSet<PerformingOrgDTO> resourceTable = new HashSet<PerformingOrgDTO>(this.ws.PerformingOrgsForWsList);
            var notInPerformingOrgTable = this.projectMapModelViews.Select((x, i) => new { index = i, value = x })
                .Where(pm => resourceTable.All(r => r.PerformingOrgName.ToUpper() != pm.value.CostCenter?.ToUpper()))
                .ToList();

            if (notInPerformingOrgTable.Any())
            {
                //generate validation message for each invalid record
                foreach (var projectMapModelView in notInPerformingOrgTable)
                {
                    this.ValidationMessages.Add(new ValidationMessage
                    {
                        ValidationIssue = "Cost Center does not exist in the Performing Org table",
                        RowIndex = projectMapModelView.index,
                        FieldName = $"Cost Center={projectMapModelView.value.CostCenter}"
                    });
                }
            }
        }

        /// <summary>
        /// Validates each resource type by verifying that only Hours or Costs are populated a given rate type
        /// </summary>
        public void ValidateHoursCostsAreValidResourceType()
        {
            var rows = this.projectMapModelViews.Select((x, i) => new { index = i, value = x })
                .ToList();

            if (rows.Any())
            {
                foreach (var projectMapModelView in rows)
                {
                    ResourceDTO rateType = (from r in this.ws.ResourcesForWsResourceListId
                        where r.ResourceName == projectMapModelView.value.InitialResource
                        select r).FirstOrDefault();

                    if (rateType != null)
                    {
                        switch (rateType.RateType)
                        {
                            case RateType.Hours:
                                if (projectMapModelView.value.Dollars.HasValue &&
                                    projectMapModelView.value.Dollars != 0m)
                                {
                                    this.ValidationMessages.Add(new ValidationMessage
                                    {
                                        ValidationIssue =
                                            "The selected Activity Type Code should have Hours, not Dollars, specified for resource",
                                        RowIndex = projectMapModelView.index,
                                        FieldName = $"Activity Type Code={projectMapModelView.value.InitialResource}"
                                    });
                                }
                                else if (!projectMapModelView.value.Hours.HasValue)
                                {
                                    this.ValidationMessages.Add(new ValidationMessage
                                    {
                                        ValidationIssue =
                                            "The selected Activity Type Code should have Hours, but none were specified for resource",
                                        RowIndex = projectMapModelView.index,
                                        FieldName = $"Activity Type Code={projectMapModelView.value.InitialResource}"
                                    });
                                }
                                break;
                            case RateType.Cost:
                                if (projectMapModelView.value.Hours.HasValue && projectMapModelView.value.Hours != 0m)
                                {
                                    this.ValidationMessages.Add(new ValidationMessage
                                    {
                                        ValidationIssue =
                                            "The selected Activity Type Code should have Dollars, not Hours, specified for resource",
                                        RowIndex = projectMapModelView.index,
                                        FieldName = $"Activity Type Code={projectMapModelView.value.InitialResource}"
                                    });
                                }
                                else if (!projectMapModelView.value.Dollars.HasValue)
                                {
                                    this.ValidationMessages.Add(new ValidationMessage
                                    {
                                        ValidationIssue =
                                            "The selected Activity Type Code should have Dollars, but none were specified for resource",
                                        RowIndex = projectMapModelView.index,
                                        FieldName = $"Activity Type Code={projectMapModelView.value.InitialResource}"
                                    });
                                }
                                break;

                            default:
                                break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Verify that Bucketized data is only being loaded into a Bucketized WS (and the opposite - non-bucketized into
        /// non-bucketized)
        /// </summary>
        public void ValidateProjectMapTypeHasValidBucketizedData()
        {
            dynamic rows;
            string errorMessage;

            if (this.ws.ProjectMapType == ProjectMapType.TimePhasedProjectMap)
            {
                rows = this.projectMapModelViews.Select((x, i) => new { index = i, value = x })
                    .Where(pm => pm.value.DiscreteMonths == null)
                    .ToList();
                errorMessage = "There was no bucketized data found, but the work space has a bucketized project map type.";
            }
            else
            {
                rows = this.projectMapModelViews.Select((x, i) => new { index = i, value = x })
                    .Where(pm => pm.value.DiscreteMonths != null)
                    .ToList();
                errorMessage =
                    "There was bucketized data found, but the work space has a non-bucketized project map type.";
            }

            foreach (dynamic projectMapModelView in rows)
            {
                this.ValidationMessages.Add(new ValidationMessage
                {
                    ValidationIssue = errorMessage,
                    RowIndex = projectMapModelView.index,
                    FieldName = $"Activity Type Code={projectMapModelView.value.InitialResource}"
                });
            }
        }

        /// <summary>
        /// Verify that for bucketized project map, start/end dates(spreads fit into the start/end dates of the resource
        /// type/line item)
        /// </summary>
        public void ValidateBucketizedSpreadsStartEndDates()
        {
            if (this.ws.ProjectMapType == ProjectMapType.TimePhasedProjectMap)
            {
                var rows = this.projectMapModelViews.Select((x, i) => new { index = i, value = x })
                    .ToList();

                if (rows.Any())
                {
                    DateTime? minDate = (from d in this.projectMapModelViews select d.StartDate).Min();
                    DateTime? maxDate = (from d in this.projectMapModelViews select d.EndDate).Max();

                    if (minDate.HasValue && maxDate.HasValue)
                    {
                        int years = maxDate.Value.Year - minDate.Value.Year + 1;

                        // This assumes that there will always be 17 years of months to validate. TODO: May need to be changed at a later date.
                        if (years <= 17)
                        {
                            foreach (var projectMapModelView in rows)
                            {
                                if (projectMapModelView.value.StartDate.HasValue &&
                                    projectMapModelView.value.EndDate.HasValue)
                                {
                                    int startMonth = 12 * (projectMapModelView.value.StartDate.Value.Year - minDate.Value.Year) +
                                                     projectMapModelView.value.StartDate.Value.Month - 1; // first month is always 1

                                    int endMonth = 12 * (projectMapModelView.value.EndDate.Value.Year - minDate.Value.Year) +
                                        projectMapModelView.value.EndDate.Value.Month - 1;

                                    // check if any data before the start date
                                    for (int i = 0; i < startMonth; i++)
                                    {
                                        if (projectMapModelView.value.DiscreteMonths != null &&
                                            projectMapModelView.value.DiscreteMonths[i].HasValue &&
                                            projectMapModelView.value.DiscreteMonths[i] != 0)
                                        {
                                            this.ValidationMessages.Add(new ValidationMessage
                                            {
                                                ValidationIssue = "M" + (i + 1).ToString() + " is before start date",
                                                RowIndex = projectMapModelView.index,
                                                FieldName =
                                                    $"Activity Type Code={projectMapModelView.value.InitialResource}"
                                            });
                                        }
                                    }
                                    // check if any data after the end date
                                    for (int i = endMonth+1; i < 204; i++)
                                    {
                                        if (projectMapModelView.value.DiscreteMonths != null &&
                                            projectMapModelView.value.DiscreteMonths[i].HasValue &&
                                            projectMapModelView.value.DiscreteMonths[i] != 0)
                                        {
                                            this.ValidationMessages.Add(new ValidationMessage
                                            {
                                                ValidationIssue = "M" + (i + 1).ToString() + " is past end date",
                                                RowIndex = projectMapModelView.index,
                                                FieldName =
                                                    $"Activity Type Code={projectMapModelView.value.InitialResource}"
                                            });
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            this.ValidationMessages.Add(new ValidationMessage
                            {
                                ValidationIssue = "There are too many months of data. Greater than 17 years.",
                                RowIndex = 0,
                                FieldName = ""
                            });
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Verify that for bucketized project map, summing up the spreads equals the total for the line item (hours/costs)
        /// </summary>
        public void ValidateBucketizedSpreadsHoursCosts()
        {
            if (this.ws.ProjectMapType == ProjectMapType.TimePhasedProjectMap)
            {
                var rows = this.projectMapModelViews.Select((x, i) => new { index = i, value = x })
                    .ToList();

                if (rows.Any())
                {
                    foreach (var projectMapModelView in rows)
                    {
                        ResourceDTO rateType = (from r in this.ws.ResourcesForWsResourceListId
                            where r.ResourceName == projectMapModelView.value.InitialResource
                            select r).FirstOrDefault();
                        if (rateType != null)
                        {
                            switch (rateType.RateType)
                            {
                                case RateType.Hours:
                                    if (projectMapModelView.value.DiscreteMonths != null && projectMapModelView.value.Hours.HasValue)
                                    {
                                        if (projectMapModelView.value.Hours != projectMapModelView.value.DiscreteMonths.Sum(d => d ?? 0))
                                        {
                                            this.ValidationMessages.Add(new ValidationMessage
                                            {
                                                ValidationIssue = "Line item hours does not match discrete months total",
                                                RowIndex = projectMapModelView.index,
                                                FieldName =
                                                    $"Activity Type Code={projectMapModelView.value.InitialResource}"
                                            });
                                        }
                                    }
                                    break;
                                case RateType.Cost:
                                    if (projectMapModelView.value.DiscreteMonths != null && projectMapModelView.value.Dollars.HasValue)
                                    {
                                        if (projectMapModelView.value.Dollars != projectMapModelView.value.DiscreteMonths.Sum(d => d ?? 0))
                                        {
                                            this.ValidationMessages.Add(new ValidationMessage
                                            {
                                                ValidationIssue = "Line item dollars does not match discrete months total",
                                                RowIndex = projectMapModelView.index,
                                                FieldName =
                                                    $"Activity Type Code={projectMapModelView.value.InitialResource}"
                                            });
                                        }
                                    }
                                    break;

                            }
                        }
                    }
                }
            }
        }
    }
}