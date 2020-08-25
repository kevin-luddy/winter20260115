// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using GenBOE.ActionLogic.Common.Calculations;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common;

    public class BoeTaskOrdinaryVariableModelView : PersistedDataModelView
    {
        public BoeTaskOrdinaryVariableModelView()
        {
            this.OrdinaryVariableID = 0;
            this.OrdinaryVariableName = String.Empty;
            this.OrdinaryVariableValue = "0";
            this.IsPercentage = false;
            this.OrdinaryVariableValueType = VarValueType.Discrete;
            this.BOEToSum = new Collection<int>();
            this.WBSToSum = new Collection<int>();
            this.CLINToSum = new Collection<int>();
            this.ResourceTypes = new Collection<int>();
            this.SortBOEBy = VarSortBOEBy.WBS;
            this.Deleted = false;
            this.DefaultSize = string.Empty;
        }

        public BoeTaskOrdinaryVariableModelView(OrdinaryVariableDto inBoeTaskOrdinaryVariable, IVariableSelectBOEtoSumCalculation inVariableSelectBOEtoSumCalculation, FullWorkspace workspace)
            : this()
        {
            if (inBoeTaskOrdinaryVariable == null)
            {
                throw new ArgumentNullException(nameof(inBoeTaskOrdinaryVariable));
            }

            if (inVariableSelectBOEtoSumCalculation == null)
            {
                throw new ArgumentNullException(nameof(inVariableSelectBOEtoSumCalculation));
            }

            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            this.OrdinaryVariableID = inBoeTaskOrdinaryVariable.Id;
            this.OrdinaryVariableName = inBoeTaskOrdinaryVariable.OrdinaryVariableName;
            this.IsPercentage = inBoeTaskOrdinaryVariable.IsPercentage;
            this.DefaultSize = inBoeTaskOrdinaryVariable.DefaultSize;

            if (this.IsPercentage.HasValue && this.IsPercentage.Value)
            {
                this.OrdinaryVariableValue = inBoeTaskOrdinaryVariable.OrdinaryVariableValue.HasValue ? (inBoeTaskOrdinaryVariable.OrdinaryVariableValue.Value * 100).ToString("0.######") + "%" : "0";
            }
            else
            {
                DataClassForSumOfBOEsCalculation data = new DataClassForSumOfBOEsCalculation();
                data.FillData(new List<OrdinaryVariableDto>() { inBoeTaskOrdinaryVariable }, null, workspace);

                this.OrdinaryVariableValue = inVariableSelectBOEtoSumCalculation.GetTaskVarLabelTotal(inBoeTaskOrdinaryVariable, data).ToString("0.######");
            }

            this.UpdateDate = inBoeTaskOrdinaryVariable.UpdateDate;
            this.OrdinaryVariableValueType = inBoeTaskOrdinaryVariable.ValueType;

            if (inBoeTaskOrdinaryVariable.ValueType == VarValueType.SumOfBOEs)
            {
                foreach (SelectBOEsToSum toSum in inBoeTaskOrdinaryVariable.SelectedBOEsToSum)
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

                this.ResourceTypes = inBoeTaskOrdinaryVariable.SumVariableResourceTypeIDs.ToCollection();

                this.SortBOEBy = inBoeTaskOrdinaryVariable.SortBOEBy;
            }

            this.Deleted = inBoeTaskOrdinaryVariable.Updateable == UpdateType.Deleted;
        }
        
        /// <summary>
        /// Converts this ModelView into the cooresponding new DTO.
        /// </summary>
        /// <returns>A new DTO ready to Insert</returns>
        public OrdinaryVariableDto GetAssociatedDTO()
        {
            return this.GetAssociatedDTO(null);
        }

        /// <summary>
        /// Converts this ModelView into the cooresponding DTO. Takes in an existing DTO and sets the data
        /// necessary to perform an Upsert on it.
        /// </summary>
        /// <param name="existingBoeTaskOrdinaryVariable">A DTO to update with this ModelView's data</param>
        /// <returns>A DTO ready to Upsert</returns>
        public OrdinaryVariableDto GetAssociatedDTO(OrdinaryVariableDto existingBoeTaskOrdinaryVariable)
        {
            // Create the DTO to be returned
            OrdinaryVariableDto toReturn = new OrdinaryVariableDto();

            // If an existing DTO was passed in, set the return value to that DTO and reset the Summed BOEs collection
            if (existingBoeTaskOrdinaryVariable != null)
            {
                toReturn = existingBoeTaskOrdinaryVariable;
                toReturn.SelectedBOEsToSum = new Collection<SelectBOEsToSum>();
            }
            // If no existing DTO was passed in, we'll use the new Variable ID from the View
            else
            {
                toReturn.Id = this.OrdinaryVariableID;
            }

            // Set values needed for both Update and Insert
            toReturn.OrdinaryVariableName = this.OrdinaryVariableName;
            toReturn.IsPercentage = this.IsPercentage;
            toReturn.DefaultSize = this.DefaultSize;

            if (toReturn.IsPercentage.HasValue && toReturn.IsPercentage.Value)
            {
                toReturn.OrdinaryVariableValue = decimal.Parse(this.OrdinaryVariableValue.Replace("%", string.Empty)) / 100;
            }
            else
            {
                toReturn.OrdinaryVariableValue = decimal.Parse(this.OrdinaryVariableValue);
            }

            toReturn.ValueType = this.OrdinaryVariableValueType;
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

        public int OrdinaryVariableID { get; set; }
        public string OrdinaryVariableValue { get; set; }
        public string OrdinaryVariableName { get; set; }
        public bool? IsPercentage { get; set; }
        public VarValueType OrdinaryVariableValueType { get; set; }
        public Collection<int> BOEToSum { get; set; }
        public Collection<int> WBSToSum { get; set; }
        public Collection<int> CLINToSum { get; set; }
        public Collection<int> ResourceTypes { get; set; }
        public VarSortBOEBy SortBOEBy { get; set; }
        public bool Deleted { get; set; }

        /// <summary>
        /// The size of the variable when/if metric was imported.
        /// </summary>
        public string DefaultSize { get; set; }
    }
}