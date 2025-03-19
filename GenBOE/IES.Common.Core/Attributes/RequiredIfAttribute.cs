// If this is really needed, consider having the class implement interface IValidatableObject instead like https://stackoverflow.com/a/39102149

//// -----------------------------------------------------------------------
//// <copyright company="Lockheed Martin Corporation">
////     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
//// </copyright>
//// -----------------------------------------------------------------------

//// copied from:
//// http://stackoverflow.com/questions/3713281/attribute-dependent-on-another-field

//namespace IES.Common.Core.Attributes
//{
//	using System;
//	using System.Collections.Generic;
//	using System.ComponentModel.DataAnnotations;
//	using System.Linq;

//	/// <summary>
//	/// Required If Attribute
//	/// </summary>
//	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1018:MarkAttributesWithAttributeUsage")]
//	public class RequiredIfAttribute : ValidationAttribute
//	{
//		private readonly RequiredAttribute innerAttribute = new();
//		public string DependentUpon { get; set; }
//		public object Value { get; set; }

//		public RequiredIfAttribute(string dependentUpon, object value)
//		{
//			DependentUpon = dependentUpon;
//			Value = value;
//		}

//		public RequiredIfAttribute(string dependentUpon)
//		{
//			DependentUpon = dependentUpon;
//			Value = null;
//		}

//		public override bool IsValid(object value)
//		{
//			throw new NotSupportedException();
//			// return base.IsValid(value);
//		}

//		/// <summary>
//		/// Validate with the validation context
//		/// </summary>
//		/// <param name="value">The value</param>
//		/// <param name="validationContext">The context</param>
//		/// <returns>Validation Result</returns>
//		protected override ValidationResult IsValid(object value, ValidationContext validationContext)
//		{
//			// get a reference to the property this validation depends upon
//			System.Reflection.PropertyInfo field = validationContext.ObjectType.GetProperty(DependentUpon);

//			if (field != null)
//			{
//				// get the value of the dependent property
//				object dependentValue = field.GetValue(validationContext.ObjectInstance, null);

//				// compare the value against the target value
//				if (dependentValue != null)
//				{
//					bool matched = Value == null;
//					if (!matched)
//					{
//						if (Value.GetType() == typeof(object[]))
//						{
//							object[] attributeValues = Value as object[];
//							matched = attributeValues.Any(av => av.ToString() == dependentValue.ToString());
//						}
//						else
//						{
//							matched = dependentValue.Equals(Value);
//						}
//					}

//					if (matched)
//					{
//						// match => means we should try validating this field is not empty

//						if (!innerAttribute.IsValid(value))
//						// validation failed - return an error
//						{
//							return new ValidationResult(ErrorMessage, new List<string>() { validationContext.MemberName });
//						}
//					}
//				}
//			}

//			return ValidationResult.Success;
//		}

//		/// <summary>
//		/// Override to require validation context
//		/// </summary>
//		public override bool RequiresValidationContext => true;
//	}

//	// TODO TIW IDataAnnotationsValidator ?
//	//public class RequiredIfValidator : IDataAnnotationsValidator //DataAnnotationsModelValidator<RequiredIfAttribute>
//	//   {
//	//       public RequiredIfValidator(ModelMetadata metadata, ControllerContext context, RequiredIfAttribute attribute)
//	//           : base(metadata, context, attribute)
//	//       { }

//	//       public override IEnumerable<ModelClientValidationRule> GetClientValidationRules()
//	//       {
//	//           // no client validation - I might well blog about this soon!
//	//           return base.GetClientValidationRules();
//	//       }

//	//       public override IEnumerable<ModelValidationResult> Validate(object container)
//	//       {
//	//           // get a reference to the property this validation depends upon
//	//           var field = Metadata.ContainerType.GetProperty(Attribute.DependentUpon);

//	//           if (field != null)
//	//           {
//	//               // get the value of the dependent property
//	//               var value = field.GetValue(container, null);

//	//               // compare the value against the target value
//	//               if (value != null)
//	//               {
//	//                   bool matched = Attribute.Value == null;
//	//                   if (!matched)
//	//                   {
//	//                       if (Attribute.Value.GetType() == typeof(object[]))
//	//                       {
//	//                           object[] attributeValues = Attribute.Value as object[];
//	//                           matched = attributeValues.Any(av => av.ToString() == value.ToString());
//	//                       }
//	//                       else
//	//                       {
//	//                           matched = value.Equals(Attribute.Value);
//	//                       }
//	//                   }

//	//                   if (matched)
//	//                   {
//	//                       // match => means we should try validating this field
//	//                       if (!Attribute.IsValid(Metadata.Model))
//	//                           // validation failed - return an error
//	//                       {
//	//                           yield return new ModelValidationResult { Message = this.ErrorMessage };
//	//                       }
//	//                   }
//	//               }
//	//           }
//	//       }
//	//   }
//}
