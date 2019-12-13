// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ValidationAttributes
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics.CodeAnalysis;
    using System.Web.Mvc;

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public sealed class ContractTypesValidationAttribute : ValidationAttribute
    {
        public string SelectedContractTypes { get; set; }

        public ContractTypesValidationAttribute()
        {
            this.SelectedContractTypes = null;
        }

        public override bool IsValid(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            ICollection<int> selectedContractTypes = null;
            if (value is ICollection<int>)
            {
                selectedContractTypes = (ICollection<int>)value;
            }
            else
            {
                return false;
            }

            if (selectedContractTypes == null || selectedContractTypes.Count == 0)
            {
                return false;
            }

            foreach (int contractType in selectedContractTypes)
            {
                if (contractType < 1)
                {
                    return false;
                }
            }

            return true;
        }
    }

    [ExcludeFromCodeCoverage]
    public class ContractTypesValidationAttributeAdapter : DataAnnotationsModelValidator<ContractTypesValidationAttribute>
    {
        public ContractTypesValidationAttributeAdapter(ModelMetadata metadata, ControllerContext context, ContractTypesValidationAttribute attribute)
            : base(metadata, context, attribute)
        {
        }

        public override IEnumerable<ModelValidationResult> Validate(object container)
        {
            if (!this.Attribute.IsValid(this.Metadata.Model))
            {
                yield return new ModelValidationResult
                {
                    Message = this.ErrorMessage,
                    MemberName = this.Attribute.SelectedContractTypes
                };
            }
        }
    }

}