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
	using GenBOE.ActionLogic.ModelView;
	using GenBOE.DataBridge.DTO;
	using GenBOE.Dtos;
	using GenBOE.Objects;
	using IES.Common;
	using IES.Common.classes;
	using IES.Common.Exceptions;

	public class ExportToProPricerFormatValidator : Validator
    {
        private IFullObjectFactory _factory;

        /// <summary>
        /// Constructor
        /// </summary>
        public ExportToProPricerFormatValidator(IFullObjectFactory factory)
        {
            this._factory = factory;
        }

        public override Collection<String> validation(object value, Collection<Dictionary<String, String>> inData)
        {
            Collection<String> response = new Collection<string>();

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }
            if (value.GetType() != typeof(ProPricerDTO))
            {
                throw new InvalidCastException("value");
            }

			ProPricerDTO dataToValidate = (ProPricerDTO)value;

            if (!dataToValidate.WorkspaceID.HasValue || dataToValidate.WorkspaceID < 0)
            {
                throw new ArgumentException("Format DTO must have WorkspaceID defined");
            }

            FullWorkspace workspace = this._factory.CreateFullWorkspace(dataToValidate.WorkspaceID.Value);

			// Validate unique format name
			bool duplicateFormatNames = (from f in workspace.ProPricerExports
                                        where f.ExportID != dataToValidate.ExportID &&
                                              f.Scope == dataToValidate.Scope &&
                                              f.FormatName.Trim().Equals(dataToValidate.FormatName.Trim(), StringComparison.CurrentCultureIgnoreCase)
                                        select f).Any();

            if (duplicateFormatNames)
            {
                response.Add("The format name must be unique within a Workspace scope.");
            }

			// UCOT - Space Only
			// If there are multiple MOQ types assigned to a task and one of those MOQ Types is Historical / Comparative / Analogous, add a warning
			if (Utilities.IsUCOTEnabledForSystem && SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.SpaceSystems)
			{
				foreach (FullBoe boe in workspace.Boes)
				{
					FullBoe fullBoe = this._factory.CreateFullBoe(boe.Id);

					string commaSeparatedTasks = string.Empty;

					foreach (BoeTaskElementDTO task in fullBoe.TaskElements)
					{
						ICollection<MoqTypeSelection> moqTypeSelectionsForTask = fullBoe.MoqTypeSelections.Where(x => x.TaskId == task.Id).ToList();
						bool doesSpecifiedMoqTypeExist = moqTypeSelectionsForTask.Any(x => x.SelectedMOQType == MOQType.Comparative || x.SelectedMOQType == MOQType.Historical
							|| x.SelectedMOQType == MOQType.AnalogousRelationships);
						if (moqTypeSelectionsForTask.Count > 1 && doesSpecifiedMoqTypeExist)
						{
							commaSeparatedTasks = string.Join(", ", task.TaskTitle);
						}
					}

					if (!string.IsNullOrEmpty(commaSeparatedTasks))
					{
						response.Add(string.Format(ValidationConstants.MULTI_TASK_WITH_MULTI_MOQ_TYPES_UCOT, commaSeparatedTasks));
					}
				}
			}

			return response;
        }
    }

}
