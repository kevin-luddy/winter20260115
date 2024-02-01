// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

// copied from:
// http://stackoverflow.com/questions/3713281/attribute-dependent-on-another-field

namespace IES.Common
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Web.Mvc;

    /// <summary>
    /// Required If Attribute
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1018:MarkAttributesWithAttributeUsage")]
    public class RequiredIfAttribute : ValidationAttribute
    {
        private RequiredAttribute innerAttribute = new RequiredAttribute();
        public string DependentUpon { get; set; }
        public object Value { get; set; }

        public RequiredIfAttribute(string dependentUpon, object value)
        {
            this.DependentUpon = dependentUpon;
            this.Value = value;
        }

        public RequiredIfAttribute(string dependentUpon)
        {
            this.DependentUpon = dependentUpon;
            this.Value = null;
        }

        public override bool IsValid(object value)
        {
            return innerAttribute.IsValid(value);
        }
    }

    public class RequiredIfValidator : DataAnnotationsModelValidator<RequiredIfAttribute>
    {
        public RequiredIfValidator(ModelMetadata metadata, ControllerContext context, RequiredIfAttribute attribute)
            : base(metadata, context, attribute)
        { }

        public override IEnumerable<ModelClientValidationRule> GetClientValidationRules()
        {
            // no client validation - I might well blog about this soon!
            return base.GetClientValidationRules();
        }

        public override IEnumerable<ModelValidationResult> Validate(object container)
        {
			// get a reference to the property this validation depends upon
			System.Reflection.PropertyInfo field = Metadata.ContainerType.GetProperty(Attribute.DependentUpon);

            if (field != null)
            {
				// get the value of the dependent property
				object value = field.GetValue(container, null);

                // compare the value against the target value
                if (value != null)
                {
                    bool matched = Attribute.Value == null;
                    if (!matched)
                    {
                        if (Attribute.Value.GetType() == typeof(object[]))
                        {
                            object[] attributeValues = Attribute.Value as object[];
                            matched = attributeValues.Any(av => av.ToString() == value.ToString());
                        }
                        else
                        {
                            matched = value.Equals(Attribute.Value);
                        }
                    }

                    if (matched)
                    {
                        // match => means we should try validating this field
                        if (!Attribute.IsValid(Metadata.Model))
                            // validation failed - return an error
                        {
                            yield return new ModelValidationResult { Message = this.ErrorMessage };
                        }
                    }
                }
            }
        }
    }
}
