// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel.DataAnnotations;
    using System.Web.Mvc;
    using GenBOE.ActionLogic.Common.Calculations;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common;

    public class WorkspaceVariableModelView : PersistedDataModelView
    {
        public WorkspaceVariableModelView()
        {
            this.WorkspaceVariableID = 0;
            this.WorkspaceVariableName = String.Empty;
            this.WorkspaceVariableValueType = -1;
            this.BOEToSum = new Collection<int>();
            this.WBSToSum = new Collection<int>();
            this.CLINToSum = new Collection<int>();
            this.ResourceTypes = new Collection<int>();
            this.SortBOEBy = VarSortBOEBy.WBS;
            this.WorkspaceVariableValue = "0";
            this.IsPercentage = false;
            this.UpdateDate = DateTime.Now;
            this.InUse = false;
            this.Deleted = false;
            this.Disabled = false;
        }

        public WorkspaceVariableModelView(WorkspaceVariableDTO variable, IVariableSelectBOEtoSumCalculation inVariableSelectBOEtoSumCalculation, FullWorkspace workspace)
            : this()
        {
            if (variable == null)
            {
                throw new ArgumentNullException(nameof(variable));
            }
            if (inVariableSelectBOEtoSumCalculation == null)
            {
                throw new ArgumentNullException(nameof(inVariableSelectBOEtoSumCalculation));
            }

            this.WorkspaceVariableID = variable.Id;
            this.WorkspaceVariableName = variable.WorkspaceVariableName;
            this.WorkspaceVariableValueType = (int)variable.ValueType;

            this.IsPercentage = variable.IsPercentage;

            if (this.IsPercentage.HasValue && this.IsPercentage.Value)
            {
                this.WorkspaceVariableValue = (variable.WorkspaceVariableValue * 100).ToString("#,##0.##########") + "%";
            }
            else
            {
                DataClassForSumOfBOEsCalculation data = new DataClassForSumOfBOEsCalculation();
                data.FillData(null, new List<WorkspaceVariableDTO>() { variable }, workspace);

                this.WorkspaceVariableValue = inVariableSelectBOEtoSumCalculation.GetWorkspaceVarLabelTotal(variable, data).ToString("#,##0.##########");
            }

            this.UpdateDate = variable.UpdateDate;
            this.InUse = variable.InUse;

            if (variable.ValueType == VarValueType.SumOfBOEs)
            {
                foreach (SelectBOEsToSum toSum in variable.SelectedBOEsToSum)
                {
                    if (toSum.BoeID.HasValue)
                    {
                        this.BOEToSum.Add(toSum.BoeID.Value);
                    }

                    else if (toSum.WBSID.HasValue)
                    {
                        this.WBSToSum.Add(toSum.WBSID.Value);
                    }

                    else if (toSum.CLINID.HasValue)
                    {
                        this.CLINToSum.Add(toSum.CLINID.Value);
                    }
                }

                this.ResourceTypes = variable.SumVariableResourceTypeIDs;

                this.SortBOEBy = variable.SortBOEBy;
            }
        }

        /// <summary>
        /// Converts this ModelView into the cooresponding new DTO.
        /// </summary>
        /// <returns>A new DTO ready to Insert</returns>
        public WorkspaceVariableDTO GetAssociatedDTO()
        {
            return this.GetAssociatedDTO(null);
        }

        /// <summary>
        /// Converts this ModelView into the cooresponding DTO. Takes in an existing DTO and sets the data
        /// necessary to perform an Upsert on it.
        /// </summary>
        /// <param name="existingWorkspaceVariableDTO">A DTO to update with this ModelView's data</param>
        /// <returns>A DTO ready to Upsert</returns>
        public WorkspaceVariableDTO GetAssociatedDTO(WorkspaceVariableDTO existingWorkspaceVariableDTO)
        {
            // Create the DTO to be returned
            WorkspaceVariableDTO toReturn = new WorkspaceVariableDTO();

            // If an existing DTO was passed in, set the return value to that DTO and reset the Summed BOEs collection
            if (existingWorkspaceVariableDTO != null)
            {
                toReturn = existingWorkspaceVariableDTO;
                toReturn.SelectedBOEsToSum = new Collection<SelectBOEsToSum>();
            }
            // If no existing DTO was passed in, we'll use the new Variable ID from the View
            else
            {
                toReturn.Id = this.WorkspaceVariableID;
            }

            // Set values needed for both Update and Insert
            toReturn.WorkspaceVariableName = this.WorkspaceVariableName;
            toReturn.IsPercentage = this.WorkspaceVariableValue.Contains("%");

            if (toReturn.IsPercentage.HasValue && toReturn.IsPercentage.Value)
            {
                toReturn.WorkspaceVariableValue = decimal.Parse(this.WorkspaceVariableValue.Replace("%", string.Empty)) / 100;
            }
            else
            {
                toReturn.WorkspaceVariableValue = decimal.Parse(this.WorkspaceVariableValue);
            }
            
            toReturn.ValueType = (VarValueType)this.WorkspaceVariableValueType;
            toReturn.SortBOEBy = this.SortBOEBy;
            toReturn.UpdateDate = new DateTime(this.UpdateDate.Ticks);
            toReturn.Updateable = (this.Deleted) ? UpdateType.Deleted : UpdateType.Upsert;

            // If this is a summed variable, set values in the Summed BOEs collection
            if (toReturn.ValueType == VarValueType.SumOfBOEs)
            {
                foreach (int summedBOEID in this.BOEToSum)
                {
                    toReturn.SelectedBOEsToSum.Add(new SelectBOEsToSum { BoeID = summedBOEID });
                }

                foreach (int summedWBSID in this.WBSToSum)
                {
                    toReturn.SelectedBOEsToSum.Add(new SelectBOEsToSum { WBSID = summedWBSID });
                }

                foreach (int summedCLINID in this.CLINToSum)
                {
                    toReturn.SelectedBOEsToSum.Add(new SelectBOEsToSum { CLINID = summedCLINID });
                }

                toReturn.SumVariableResourceTypeIDs = this.ResourceTypes;
            }

            // Return the DTO
            return toReturn;
        }

        public int WorkspaceVariableID { get; set; }

        [Required(ErrorMessage = "Workspace Variable name is required.")]
        [StringLength(20, ErrorMessage = "A maximum of 20 characters are allowed")]

        // A regular expression that is 20 chars long and can only be alphanumeric chars, and must start with alphabetic char
        // not case sensitive
        [RegularExpression(@"^[a-zA-Z][a-zA-Z0-9 ]{0,19}$", ErrorMessage = "A Workspace Variable must begin with an alphabetic character, and can only contain alphanumeric characters.")]
        public string WorkspaceVariableName { get; set; }

        [Range((int)VarValueType.Discrete, (int)VarValueType.SumOfBOEs, ErrorMessage = "Workspace Variable Value Type is required.")]
        public int WorkspaceVariableValueType { get; set; }

        public Collection<int> BOEToSum { get; set; }
        public Collection<int> WBSToSum { get; set; }
        public Collection<int> CLINToSum { get; set; }

        public Collection<int> ResourceTypes { get; set; }

        public VarSortBOEBy SortBOEBy { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "BOEs")]
        public ICollection<SelectListItem> WorkspaceVariableValueTypes
        {
            get
            {
                Collection<SelectListItem> toReturn = new Collection<SelectListItem>();

                toReturn.Add(new SelectListItem
                {
                    Text = "Select a Type",
                    Value = "-1"
                });

                toReturn.Add(new SelectListItem
                {
                    Text = VarValueType.Discrete.ToString(),
                    Value = ((int)VarValueType.Discrete).ToString(),
                    Selected = this.WorkspaceVariableValueType == (int)VarValueType.Discrete
                });

                toReturn.Add(new SelectListItem
                {
                    Text = "Sum of BOEs",
                    Value = ((int)VarValueType.SumOfBOEs).ToString(),
                    Selected = this.WorkspaceVariableValueType == (int)VarValueType.SumOfBOEs
                });

                return toReturn;
            }
        }

        [RegularExpression(@"^[-]?[\d,]*\.?\d+[\d,]*%?$", ErrorMessage = "An invalid value was entered.")]
        [Required(ErrorMessage = "Workspace Variable Value is required.")]
        public string WorkspaceVariableValue { get; set; }

        public bool? IsPercentage { get; set; }

        public bool InUse { get; set; }

        public bool Deleted { get; set; }

        public bool Disabled { get; set; }
    }
}