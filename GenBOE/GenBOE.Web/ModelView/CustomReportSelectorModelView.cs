namespace GenBOE.Web.ModelView
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;
    using IES.Common;
    using IES.Common.classes;

    public class CustomReportSelectorModelView
    {
        /// <summary>
        /// Default drop-down text for "Sort By"
        /// </summary>
        public const string SELECT_SORT_BY_CRITERIA = "--- Select Sort By Criteria ---";
        
        /// <summary>
        /// Default drop-down text for "Sort By Boe Value"
        /// </summary>
        public const string SELECT_FILTER_CRITERIA = "--- Select Filter Criteria ---";

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="workspaceName">Current Workspace name</param>
		/// <param name="allBoeData">Data on all BOEs in the WS needed for custom export</param>
		/// <param name="sortBy">Primary sort by field</param>
		/// <param name="secondarySortBy">Secondary sort by field</param>
		/// <param name="usingTemplateBoe">Whether Workspace is using Template BOE</param>
		/// <param name="usingSkillMixTables">Whether Workspace is using Skill Mix Tables</param>
		/// <param name="enableAssignTaskAuthor">Whether Assign Task Author is enabled</param>
		public CustomReportSelectorModelView(string workspaceName, ICollection<BoeCustomReportBoeData> allBoeData, BoeCustomReportSortBy sortBy, BoeCustomReportSortBy secondarySortBy, bool usingTemplateBoe, bool usingSkillMixTables, bool enableAssignTaskAuthor)
        {
            this.WorkspaceName = workspaceName;

            this.AutoOpen = false;
            
            if (allBoeData == null)
            {
                allBoeData = new List<BoeCustomReportBoeData>(0);
            }

            List<SelectListItem> boeSortByList = new List<SelectListItem>();
            ICollection<BoeCustomReportSortBy> sortByValues = Enum.GetValues(typeof(BoeCustomReportSortBy)).Cast<BoeCustomReportSortBy>().ToList();
            foreach (BoeCustomReportSortBy sortByVal in sortByValues)
            {
                boeSortByList.Add(new SelectListItem
                {
                    Text = sortByVal.ToDescription(),
                    Value = sortByVal.ToString(),
                    Selected = (sortBy == sortByVal)
                });
            }

            List<SelectListItem> boeSecondarySortByList = new List<SelectListItem>();
            foreach (BoeCustomReportSortBy sortByVal in sortByValues)
            {
                boeSecondarySortByList.Add(new SelectListItem
                {
                    Text = sortByVal.ToDescription(),
                    Value = sortByVal.ToString(),
                    Selected = (secondarySortBy == sortByVal)
                });
            }

            //Remove primary selection from secondary list and secondary selection from primary list
            SelectListItem primarySelected = boeSortByList.FirstOrDefault(s => s.Selected);
            SelectListItem secondarySelected = boeSecondarySortByList.FirstOrDefault(s => s.Selected);
            if(primarySelected != null && primarySelected.Value != BoeCustomReportSortBy.SelectSortByCriteria.ToString())
            {
                boeSecondarySortByList.Remove(boeSecondarySortByList.Where(s => s.Value == primarySelected.Value).FirstOrDefault());
            }
            if(secondarySelected != null && secondarySelected.Value != BoeCustomReportSortBy.SelectSortByCriteria.ToString())
            {
                boeSortByList.Remove(boeSortByList.Where(s => s.Value == secondarySelected.Value).FirstOrDefault());
            }

            this.BoeSortByList = boeSortByList;
            this.BoeSecondarySortByList = boeSecondarySortByList;

            List<SelectListItem> boeSortValues = new List<SelectListItem>
            {
                new SelectListItem
                {
                     Value = string.Empty,
                     Text = SELECT_SORT_BY_CRITERIA,
                     Selected = true
                }
            };
            this.BoeSortValues = boeSortValues;
            this.BoeSecondarySortValues = boeSortValues;

            this.ComponentsUnselected = new List<SelectListItem>();
            ICollection<BoeCustomReportComponent> reportComponentValues = Enum.GetValues(typeof(BoeCustomReportComponent)).Cast<BoeCustomReportComponent>().ToList();
            CompanyConfiguration companyConfig = SystemConfiguration.Instance().CompanyMode;

            foreach (BoeCustomReportComponent reportComponentVal in reportComponentValues)
            {
                if (DisplayComponent(reportComponentVal, usingTemplateBoe, usingSkillMixTables, companyConfig, enableAssignTaskAuthor))
                {
                    this.ComponentsUnselected.Add(new SelectListItem
                    {
                        Text = reportComponentVal.ToDescription(),
                        Value = reportComponentVal.ToString()
                    });
                }
            }

            // initialize sort-values list
            this.PopulateSortValuesList(allBoeData, sortBy, secondarySortBy);

            // do not populate on initial load
            this.BoesUnselected = new List<SelectListItem>();
            this.BoesSelected = new List<SelectListItem>();
            this.ComponentsSelected = new List<SelectListItem>();
        }

		/// <summary>
		/// Determine if component should be displayed
		/// </summary>
		/// <param name="reportComponentVal">component</param>
		/// <param name="usingTemplateBOE">if WS is using Template BOE</param>
		/// <param name="skillMixEnabled">If skill mix is enabled</param>
		/// <param name="companyConfig">Company config (space vs RMS)</param>
		/// <param name="enableAssignTaskAuthor">Whether Assign Task Author is enabled</param>
		/// <returns>True if component should be displayed, false if not</returns>
		private bool DisplayComponent(BoeCustomReportComponent reportComponentVal, bool usingTemplateBOE, bool skillMixEnabled, CompanyConfiguration companyConfig, bool enableAssignTaskAuthor)
        {
            if ((reportComponentVal == BoeCustomReportComponent.TaskMOQRationale && usingTemplateBOE)
                || (reportComponentVal == BoeCustomReportComponent.TaskMOQAdditionalQueryFilters && (!usingTemplateBOE || companyConfig == CompanyConfiguration.SpaceSystems))
				|| (reportComponentVal == BoeCustomReportComponent.TaskMOQEmployeeIDFilters && (!usingTemplateBOE || companyConfig == CompanyConfiguration.MST))
				|| (reportComponentVal == BoeCustomReportComponent.SkillMixTables && !skillMixEnabled)
				|| (reportComponentVal == BoeCustomReportComponent.TaskAuthor && !enableAssignTaskAuthor))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Populates the lists of sort by values
        /// </summary>
        /// <param name="allBoeData">Data on all BOEs in the WS needed for custom export</param>
        /// <param name="sortBy">Primary sort by field</param>
        /// <param name="secondarySortBy">Secondary sort by field</param>
        /// <param name="sortByVal">Primary sort by value selected</param>
        /// <param name="secondarySortByVal">Secondary sort by value selected</param>
        private void PopulateSortValuesList(ICollection<BoeCustomReportBoeData> allBoeData, BoeCustomReportSortBy sortBy, BoeCustomReportSortBy secondarySortBy, string sortByVal = "", string secondarySortByVal = "")
        {
            List<SelectListItem> sortValues = new List<SelectListItem>
            {
                new SelectListItem
                {
                     Value = string.Empty,
                     Text = SELECT_FILTER_CRITERIA,
                     Selected = (String.IsNullOrEmpty(sortByVal))    // set the first option to Selected if there is no value selected
                }
            };

            List<SelectListItem> secondarySortValues = new List<SelectListItem>
            {
                new SelectListItem
                {
                     Value = string.Empty,
                     Text = SELECT_FILTER_CRITERIA,
                     Selected = (String.IsNullOrEmpty(secondarySortByVal))    // set the first option to Selected if there is no value selected
                }
            };

            //list of BOEs available to use for secondary sort-by-values
            ICollection<BoeCustomReportBoeData> availableBOEs = new List<BoeCustomReportBoeData>();
            //populate the primary sort values
            foreach (BoeCustomReportBoeData boe in allBoeData)
            {
                IList<SelectListItem> items = this.AssembleBoeSortValueDisplay(boe, sortBy);

                sortValues.AddRange(items.Where(x => !string.IsNullOrEmpty(x.Text) && !sortValues.Any(v => v.Value == x.Value)).ToCollection());

                //If the text for the BOEs value isn't empty, and this BOE isn't already in the list of available BOEs
                if (items.Any(x => !string.IsNullOrEmpty(x.Text)) && !availableBOEs.Any(x => x.BOEID == boe.BOEID))
                {
                    //And if there is no primary filter, add this BOE to the available BOEs for the secondary sort-by-values
                    if (string.IsNullOrEmpty(sortByVal))
                    {
                        availableBOEs.Add(boe);
                    }
                    //Else if there is a primary filter and this BOE matches it, add this BOE to the available BOEs for the secondary sort-by-values
                    else if (items.Any(x => x.Value == sortByVal))
                    {
                        availableBOEs.Add(boe);
                    }
                }
            }
            //set the selected value
            //this will only result in a null exception if the user manually edits the value in the page source before selecting the value
            sortValues.FirstOrDefault(v => v.Value == sortByVal).Selected = true;

            //If only a secondary filter is set, all boes are available for secondary sort-by-values
            if(!availableBOEs.Any() && sortBy == BoeCustomReportSortBy.SelectSortByCriteria && secondarySortBy != BoeCustomReportSortBy.SelectSortByCriteria)
            {
                availableBOEs = allBoeData;
            }

            //populate the secondary sort values
            foreach (BoeCustomReportBoeData boe in availableBOEs)
            {
                IList<SelectListItem> items = this.AssembleBoeSortValueDisplay(boe, secondarySortBy);

                secondarySortValues.AddRange(items.Where(x => !secondarySortValues.Any(v => v.Value == x.Value) && !string.IsNullOrEmpty(x.Text)).ToCollection());
            }
            //set the selected value
            //this will only result in a null exception if the user manually edits the value in the page source before selecting the value
            secondarySortValues.FirstOrDefault(v => v.Value == secondarySortByVal).Selected = true;

            this.BoeSortValues = sortValues.OrderBy(s => s.Text, SortOrder.Ascending).ToList();
            this.BoeSecondarySortValues = secondarySortValues.OrderBy(s => s.Text, SortOrder.Ascending).ToList();
        }

        /// <summary>
        /// Apply selection filters to generate either ALL or a SUBSET of the complete set of BOEs
        /// </summary>
        /// <param name="allBoeData">Data on all BOEs in the WS needed for custom export</param>
        /// <param name="sortBy">Primary sort by field</param>
        /// <param name="secondarySortBy">Secondary sort by field</param>
        /// <param name="selections">Selections made for the custom export</param>
        /// <param name="sortByVal">Primary sort by value selected</param>
        /// <param name="secondarySortByVal">Secondary sort by value selected</param>
        private void PopulateBOESelectionsList(ICollection<BoeCustomReportBoeData> allBoeData, BoeCustomReportSortBy sortBy, BoeCustomReportSortBy secondarySortBy, BoeCustomReportSelections selections, int? sortByValue = null, int? secondarySortByValue = null)
        {
            // Populate list based on the selected sort-by-value - Note: The drop-down value will be the primary key corresponding to the sortBy object
            ICollection<BoeCustomReportBoeData> filteredBoesList = populateBoesList(allBoeData, sortBy, secondarySortBy, sortByValue, secondarySortByValue).ToList();

            // assemble the list of BOEs for the user to choose from based on the filters
            List<SelectListItem> unselectedBoes = new List<SelectListItem>(filteredBoesList.Count());

            foreach (BoeCustomReportBoeData boe in filteredBoesList)
            {
                // skip any BOEs from the filtered list that have already been selected
                if (!selections.BoesSelected.ContainsKey(boe.BOEID))
                {
                    unselectedBoes.Add(this.AssembleBoeDisplayValue(boe, sortBy, secondarySortBy, sortByValue, secondarySortByValue));
                }
            }

            this.BoesUnselected = unselectedBoes.OrderBy(s => s.Text, SortOrder.Ascending).ToList();
        }

        /// <summary>
        /// Maintain the cumulative list of selected BOEs by repeatedly posting and re-rendering them from the page
        /// </summary>
        /// <param name="selections">Selections made for the custom export</param>
        private void PopulateBOESelections(BoeCustomReportSelections selections)
        {
            // resend the list of BOEs that were already selected by the user - preserve text display
            List<SelectListItem> selectedBoes = new List<SelectListItem>(selections.BoesSelected.Count);
            foreach (KeyValuePair<int, string> entry in selections.BoesSelected)
            {
                selectedBoes.Add(new SelectListItem
                {
                    Text = entry.Value,
                    Value = entry.Key.ToString()
                });
            }

            this.BoesSelected = selectedBoes;
        }

        private void SyncComponentSelections(BoeCustomReportSelections selections, Boolean autoOpen)
        {
            // start with complete list of all component options
            List<SelectListItem> unselectedComponents = new List<SelectListItem>(this.ComponentsUnselected);
            ICollection<string> selectedComponentIds = selections.ComponentsSelected.Select(b => b.ToString()).ToList();

            List<SelectListItem> selectedComponents = new List<SelectListItem>();

            foreach (string selectedComponentId in selectedComponentIds)
            {
                SelectListItem item;
                if ((item = unselectedComponents.FirstOrDefault(b => b.Value == selectedComponentId)) != null)
                {
                    unselectedComponents.Remove(item);
                    selectedComponents.Add(item);
                }
            }

            this.ComponentsSelected = selectedComponents;
            this.ComponentsUnselected = unselectedComponents;

            // if this is the first time opening the dialog, switch the unselected with the selected.  
            // this will make the report components be selected by default.
            if (!autoOpen) 
            {
                this.ComponentsSelected = unselectedComponents;
                this.ComponentsUnselected = selectedComponents;
            }
        }

		/// <summary>
		/// Constructor 
		/// </summary>
		/// <param name="workspaceName">Current Workspace name</param>
		/// <param name="allBoeData">Data on all BOEs in the WS needed for custom export</param>
		/// <param name="sortBy">Primary sort by field</param>
		/// <param name="secondarySortBy">Secondary sort by field</param>
		/// <param name="selections">Selections made for the custom export</param>
		/// <param name="usingTemplateBoe">Whether workspace is using Template BOE</param>
		/// <param name="enableAssignTaskAuthor">Whether Assign Task Author is enabled</param>
		public CustomReportSelectorModelView(string workspaceName, ICollection<BoeCustomReportBoeData> allBoeData, BoeCustomReportSortBy sortBy, BoeCustomReportSortBy secondarySortBy, BoeCustomReportSelections selections, bool usingTemplateBoe, bool usingSkillMixTables, bool enableAssignTaskAuthor)
            : this(workspaceName, allBoeData, sortBy, secondarySortBy, usingTemplateBoe, usingSkillMixTables, enableAssignTaskAuthor)
        {
            if (selections != null)
            {
                /*
                 *                         BoeSortBy       BoeSortByValue
                 *                         ---------       --------------
                 * Initial load:            <null>             <null>
                 * Change Sort-By:         "SORTBY"            <null>
                 * Change Sort-Value:      "SORTBY"          "SORTVALUE"
                 * 
                 */

                int sortByValue = -1;
                int secondarySortByValue = -1;

                if (string.IsNullOrEmpty(selections.BoeSortBy))
                {
                    // initial load

                    this.AutoOpen = false;

                    // Additional Query Filters excluded by default in SSC, only if Template BOE
                    if (SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.SpaceSystems && usingTemplateBoe)
                    {
                        selections.ComponentsSelected.Add(BoeCustomReportComponent.TaskMOQEmployeeIDFilters);
                    }
                }
                else
                {
                    this.AutoOpen = true;

                    //if no filters applied (A sort-by selection was changed)
                    if (string.IsNullOrEmpty(selections.BoeSortByValue) && string.IsNullOrEmpty(selections.BoeSecondarySortByValue))
                    {
                        this.PopulateSortValuesList(allBoeData, sortBy, secondarySortBy);
                        this.PopulateBOESelectionsList(allBoeData, sortBy, secondarySortBy, selections);
                    }
                    //if only primary filter applied (Primary sort-by-value selection was changed, Secondary not set)
                    else if (!string.IsNullOrEmpty(selections.BoeSortByValue) && string.IsNullOrEmpty(selections.BoeSecondarySortByValue))
                    {
                        if (int.TryParse(selections.BoeSortByValue, out sortByValue))
                        {
                            this.PopulateSortValuesList(allBoeData, sortBy, secondarySortBy, selections.BoeSortByValue);
                            this.PopulateBOESelectionsList(allBoeData, sortBy, secondarySortBy, selections, sortByValue);
                        }
                    }
                    //if only secondary filter applied (Secondary sort-by-value selection was changed, Primary not set)
                    else if (string.IsNullOrEmpty(selections.BoeSortByValue) && !string.IsNullOrEmpty(selections.BoeSecondarySortByValue))
                    {
                        if (int.TryParse(selections.BoeSecondarySortByValue, out secondarySortByValue))
                        {
                            this.PopulateSortValuesList(allBoeData, sortBy, secondarySortBy, string.Empty, selections.BoeSecondarySortByValue);
                            this.PopulateBOESelectionsList(allBoeData, sortBy, secondarySortBy, selections, null, secondarySortByValue);
                        }
                    }
                    //if both primary and secondary filters applied (A sort-by-value selection was changed while the other selection was set)
                    else
                    {
                        if (int.TryParse(selections.BoeSortByValue, out sortByValue) && int.TryParse(selections.BoeSecondarySortByValue, out secondarySortByValue))
                        {
                            this.PopulateSortValuesList(allBoeData, sortBy, secondarySortBy, selections.BoeSortByValue, selections.BoeSecondarySortByValue);
                            this.PopulateBOESelectionsList(allBoeData, sortBy, secondarySortBy, selections, sortByValue, secondarySortByValue);
                        }
                    }
                }

                this.PopulateBOESelections(selections);
                this.SyncComponentSelections(selections, this.AutoOpen);
            }
        }

        /// <summary>
        /// Build a select (option) item for the given BOE, sort-by and sort-by-value choices.
        /// </summary>
        /// <param name="boe">BOE</param>
        /// <param name="sortBy">Primary sort-by selection</param>
        /// <param name="secondarySortBy">Secondary sort-by selection</param>
        /// <param name="sortByValue">Primary sort-by-value selection - optional since it won't always be necessary when only a sortby is given</param>
        /// <param name="secondarySortByValue">Secondary sort-by-value selection - optional since it won't always be necessary when only a sortby is given</param>
        /// <returns>Select (option) item</returns>
        private SelectListItem AssembleBoeDisplayValue(BoeCustomReportBoeData boe, BoeCustomReportSortBy sortBy, BoeCustomReportSortBy secondarySortBy, int? sortByValue = null, int? secondarySortByValue = null)
        {
            bool wbsUsed = false;
            bool clinUsed = false;
            bool titleUsed = false;

            //The text will display primary sort field, then secondary sort field, then wbs, clin, and/or title if they were not used yet
            string displayTextPrimary = getPartialBoeDisplayValue(boe, sortBy, sortByValue, ref wbsUsed, ref clinUsed, ref titleUsed);
            string displayTextSecondary = getPartialBoeDisplayValue(boe, secondarySortBy, secondarySortByValue, ref wbsUsed, ref clinUsed, ref titleUsed);
            string displayValue = boe.BOEID.ToString();

            if (string.IsNullOrEmpty(displayTextPrimary) && string.IsNullOrEmpty(displayTextSecondary))
            {
                throw new NotImplementedException(string.Format("{0} and {1}", sortBy.ToString(), secondarySortBy.ToString()));
            }

            string completeDisplayText = string.Empty;

            //Add the remaining fields to the display text - default order: wbs clin title
            if (wbsUsed)
            {
                if (clinUsed)
                {
                    completeDisplayText = string.Format("{0} {1} {2}", displayTextPrimary, displayTextSecondary, boe.BoeTitle);
                }
                if (titleUsed)
                {
                    completeDisplayText = string.Format("{0} {1} {2} {3}", displayTextPrimary, displayTextSecondary, boe.ClinNumber, boe.ClinTitle);
                }
                else
                {
                    completeDisplayText = string.Format("{0} {1} {2} {3} {4}", displayTextPrimary, displayTextSecondary, boe.ClinNumber, boe.ClinTitle, boe.BoeTitle);
                }
            }
            else if (clinUsed)
            {
                if(titleUsed)
                {
                    completeDisplayText = string.Format("{0} {1} {2} {3}", displayTextPrimary, displayTextSecondary, boe.WbsNumber, boe.WbsTitle);
                }
                else
                {
                    completeDisplayText = string.Format("{0} {1} {2} {3} {4}", displayTextPrimary, displayTextSecondary, boe.WbsNumber, boe.WbsTitle, boe.BoeTitle);
                }
            }
            else if(titleUsed)
            {
                completeDisplayText = string.Format("{0} {1} {2} {3} {4} {5}", displayTextPrimary, displayTextSecondary, boe.WbsNumber, boe.WbsTitle, boe.ClinNumber, boe.ClinTitle);
            }
            else
            {
                completeDisplayText = string.Format("{0} {1} {2} {3} {4} {5} {6}", displayTextPrimary, displayTextSecondary, boe.WbsNumber, boe.WbsTitle, boe.ClinNumber, boe.ClinTitle, boe.BoeTitle);
            }

            return new SelectListItem
            {
                Text = completeDisplayText,
                Value = displayValue
            };
        }

        /// <summary>
        /// Gets the display value for one sort-by field (primary or secondary)
        /// </summary>
        /// <param name="boe">boe data</param>
        /// <param name="sortBy">Sort-by selection</param>
        /// <param name="sortByValue">Sort-by-value selection</param>
        /// <param name="wbsUsed">bool to note if the wbs has been added to the display value</param>
        /// <param name="clinUsed">bool to note if the clin has been added to the display value</param>
        /// <param name="titleUsed">bool to note if the title has been added to the display value</param>
        /// <returns>Part of the display value, formatted based on the sort-by selection</returns>
        private string getPartialBoeDisplayValue(BoeCustomReportBoeData boe, BoeCustomReportSortBy sortBy, int? sortByValue, ref bool wbsUsed, ref bool clinUsed, ref bool titleUsed)
        {
            string toReturn = string.Empty;

            switch (sortBy)
            {
                case BoeCustomReportSortBy.WBS:
                    toReturn = string.Format("{0} {1}", boe.WbsNumber, boe.WbsTitle);
                    wbsUsed = true;
                    break;
                case BoeCustomReportSortBy.CLIN:
                    toReturn = string.Format("{0} {1}", boe.ClinNumber, boe.ClinTitle);
                    clinUsed = true;
                    break;
                case BoeCustomReportSortBy.Author:
                    // can be multiple authors, but display ONLY the author that was used as the filter value
                    BoeCustomReportAuthorData author = boe.Authors.FirstOrDefault(a => a.AuthorUserID == sortByValue);
                    BoeCustomReportAuthorData authorFirstAndLastNames = boe.Authors.FirstOrDefault(a => a.AuthorFirstName != null && a.AuthorLastName != null);

                    // Show author name if (1) user picks a SortByValue or (2) user does not pick a SortByValue and the BOE Author has First and Last Names
                    if (author != null)
                    {
                        toReturn = author.AuthorDisplayValue;
                    }
                    else if (sortByValue == null && authorFirstAndLastNames != null)
                    {
                        toReturn = authorFirstAndLastNames.AuthorDisplayValue;
                    }
                    //else string stays empty
                    break;
                case BoeCustomReportSortBy.BOETitle:
                    toReturn = boe.BoeTitle;
                    titleUsed = true;
                    break;
                case BoeCustomReportSortBy.BOECustomField:
                    // can be multiple fields, but display ONLY the field that was used as the filter value
                    BoeCustomReportCustomFieldData customField = boe.CustomFields.FirstOrDefault(a => a.CustomFieldValueID == sortByValue);
                    BoeCustomReportCustomFieldData customFieldHasValueName = boe.CustomFields.FirstOrDefault(a => a.CustomFieldValueName != null);

                    // Show Custom Field if (1) user picks a SortByValue or (2) user does not pick a SortByValue and Custom Field has a value
                    if (customField != null)
                    {
                        toReturn = string.Format("{0} {1}", customField.CustomFieldValueName, customField.CustomFieldValueDescription);
                    }
                    else if (sortByValue == null && customFieldHasValueName != null)
                    {
                        toReturn = string.Format("{0} {1}", customFieldHasValueName.CustomFieldValueName, customFieldHasValueName.CustomFieldValueDescription);
                    }
                    //else no changes
                    break;
                default:
                    break;
            }

            return toReturn;
        }

        /// <summary>
        /// Populate list based on the selected sort-by-value
        /// </summary>
        /// <param name="allBoeData"></param>
        /// <param name="sortBy">Primary sortBy selected value (CLIN, WBS, etc.)</param>
        /// <param name="secondarySortBy">Secondary sortBy selected value</param>
        /// <param name="sortByValue">Additional filter, primary sortBy value</param>
        /// <param name="secondarySortByValue">Secondary sortBy value</param>
        /// <returns>Filtered list based on selected sort-by values</returns>
        private ICollection<BoeCustomReportBoeData> populateBoesList(ICollection<BoeCustomReportBoeData> allBoeData, BoeCustomReportSortBy sortBy, BoeCustomReportSortBy secondarySortBy, int? sortByValue = null, int? secondarySortByValue = null)
        {
            ICollection<BoeCustomReportBoeData> filteredBoesList = new List<BoeCustomReportBoeData>();

            //Populate based on the primary sortBy
            ICollection<BoeCustomReportBoeData> primaryFilteredBoesList = getPartialBoesList(allBoeData, sortBy, sortByValue);

            //Populate based on the secondary sortBy
            if(secondarySortBy == BoeCustomReportSortBy.SelectSortByCriteria)
            {
                //if there is no secondary sort, no need to filter/sort the list further
                filteredBoesList = primaryFilteredBoesList;
            }
            else if(primaryFilteredBoesList.Any())
            {
                //if there was a primary sort set, use the list based on that to apply the secondary sort
                filteredBoesList = getPartialBoesList(primaryFilteredBoesList, secondarySortBy, secondarySortByValue);
            }
            else
            {
                //if only a secondary sort was set (or neither was), perform the sort on the full BOE data
                filteredBoesList = getPartialBoesList(allBoeData, secondarySortBy, secondarySortByValue);
            }

            return filteredBoesList;
        }

        /// <summary>
        /// Get the partial list of Boes based on one sort/filter
        /// </summary>
        /// <param name="boeData">BOEs to be sorted/filtered</param>
        /// <param name="sortBy">Sort-by selection</param>
        /// <param name="sortByValue">Sort-by-value selection</param>
        /// <returns>Partially filtered list based on one of the selected sort-by values</returns>
        private ICollection<BoeCustomReportBoeData> getPartialBoesList(ICollection<BoeCustomReportBoeData> boeData, BoeCustomReportSortBy sortBy, int? sortByValue)
        {
            ICollection<BoeCustomReportBoeData> filteredBoesList = new List<BoeCustomReportBoeData>();

            switch (sortBy)
            {
                case BoeCustomReportSortBy.WBS:
                    if (sortByValue.HasValue)
                    {
                        filteredBoesList = boeData.Where(a => a.WBSID.HasValue && a.WBSID.Value == sortByValue).ToList();
                    }
                    else
                    {
                        filteredBoesList = boeData.Where(a => a.WBSID.HasValue).ToList();
                    }
                    break;

                case BoeCustomReportSortBy.CLIN:
                    if (sortByValue.HasValue)
                    {
                        filteredBoesList = boeData.Where(a => a.CLINID.HasValue && a.CLINID.Value == sortByValue).ToList();
                    }
                    else
                    {
                        filteredBoesList = boeData.Where(a => a.CLINID.HasValue).ToList();
                    }
                    break;

                case BoeCustomReportSortBy.BOETitle:
                    if (sortByValue.HasValue)
                    {
                        filteredBoesList = boeData.Where(a => a.BOEID == sortByValue).ToList();
                    }
                    else
                    {
                        filteredBoesList = boeData.Where(a => !string.IsNullOrEmpty(a.BoeTitle)).ToList();
                    }
                    break;

                case BoeCustomReportSortBy.Author:
                    if (sortByValue.HasValue)
                    {
                        filteredBoesList = boeData.Where(b => b.Authors.Select(a => a.AuthorUserID).Contains((int)sortByValue)).ToList();
                    }
                    else
                    {
                        filteredBoesList = boeData.Where(b => b.Authors.Count() > 0).ToList();
                    }
                    break;

                case BoeCustomReportSortBy.BOECustomField:
                    if (sortByValue.HasValue)
                    {
                        filteredBoesList = boeData.Where(b => b.CustomFields.Select(f => f.CustomFieldValueID).Contains((int)sortByValue)).ToList();
                    }
                    else
                    {
                        filteredBoesList = boeData.Where(b => b.CustomFields.Any()).ToList();
                    }
                    break;

                default:
                    filteredBoesList = new List<BoeCustomReportBoeData>(0);
                    break;
            }

            return filteredBoesList;
        }


        /// <summary>
        /// Build a list of select (option) items for the given BOE and sort-by choice.
        /// </summary>
        /// <param name="boe">BOE</param>
        /// <param name="sortBy">Sort-by selection</param>
        /// <returns>Select (option) items</returns>
        private IList<SelectListItem> AssembleBoeSortValueDisplay(BoeCustomReportBoeData boe, BoeCustomReportSortBy sortBy)
        {
            IList<SelectListItem> results = new List<SelectListItem>();

            switch (sortBy)
            {
                case BoeCustomReportSortBy.SelectSortByCriteria:
                   break;
                case BoeCustomReportSortBy.WBS:
                    results.Add(new SelectListItem
                    {
                        Text = string.Format("{0} {1}", boe.WbsNumber, boe.WbsTitle).TrimEnd(),
                        Value = boe.WBSID.HasValue ? boe.WBSID.Value.ToString() : string.Empty
                    });
                    break;
                case BoeCustomReportSortBy.CLIN:
                    results.Add(new SelectListItem
                    {
                        Text = string.Format("{0} {1}", boe.ClinNumber, boe.ClinTitle).TrimEnd(),
                        Value = boe.CLINID.HasValue ? boe.CLINID.Value.ToString() : string.Empty
                    });
                    break;
                case BoeCustomReportSortBy.Author:
                    foreach (BoeCustomReportAuthorData author in boe.Authors)
                    {
                        results.Add(new SelectListItem
                        {
                            Text = string.Format("{0} {1}", author.AuthorDisplayValue, author.IsSubcontractor ? "(Sub)" : string.Empty).TrimEnd(),
                            Value = author.AuthorUserID.ToString()
                        });
                    }
                    break;
                case BoeCustomReportSortBy.BOETitle:
                    results.Add(new SelectListItem
                    {
                        Text = boe.BoeTitle ?? string.Empty,
                        Value = boe.BOEID.ToString()
                    });
                    break;
                case BoeCustomReportSortBy.BOECustomField:
                    foreach (BoeCustomReportCustomFieldData field in boe.CustomFields)
                    {
                        results.Add(new SelectListItem
                        {
                            Text = string.Format("{0} {1}", field.CustomFieldValueDescription, field.CustomFieldValueName).TrimEnd(),
                            Value = field.CustomFieldValueID.ToString()
                        });
                    }
                    break;
                default:
                    throw new NotImplementedException(sortBy.ToString());
            }

            return results;
        }

        public bool AutoOpen { get; set; }
        public string WorkspaceName { get; set; }
        public ICollection<SelectListItem> BoeSortByList { get; set; }
        public ICollection<SelectListItem> BoeSortValues { get; set; }
        public ICollection<SelectListItem> BoeSecondarySortByList { get; set; }
        public ICollection<SelectListItem> BoeSecondarySortValues { get; set; }
        public ICollection<SelectListItem> BoesUnselected { get; set; }
        public ICollection<SelectListItem> BoesSelected { get; set; }
        public ICollection<SelectListItem> ComponentsUnselected { get; set; }
        public ICollection<SelectListItem> ComponentsSelected { get; set; }
    }
}