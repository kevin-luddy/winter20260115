namespace IES.Common.Core.Attributes
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel.DataAnnotations;

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1018:MarkAttributesWithAttributeUsage")]
	public class EnumRequiredAttribute : ValidationAttribute
	{
		public Type EnumType { get; set; }
		public ICollection<object> AllowedValues { get; set; }

		public EnumRequiredAttribute(Type enumType, ICollection<object> allowedValues)
		{
			if (enumType == null)
			{
				throw new ArgumentNullException(nameof(enumType));
			}

			if (allowedValues == null)
			{
				throw new ArgumentNullException(nameof(allowedValues));
			}

			if (!enumType.IsEnum)
			{
				throw new ArgumentException("enumType must be of type System.Enum");
			}
			else
			{
				foreach (object allowedValue in allowedValues)
				{
					if (allowedValue.GetType() != enumType)
					{
						throw new ArgumentException("All allowed values must be of type enumType.");
					}
				}

				EnumType = enumType;
				AllowedValues = allowedValues;
			}
		}

		public override bool IsValid(object value)
		{
			return AllowedValues.Contains(value);
		}
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1018:MarkAttributesWithAttributeUsage")]
	public class EnumRangeAttribute : RangeAttribute
	{
		public EnumRangeAttribute(double minimum, double maximum)
			: base(minimum, maximum)
		{
		}

		public EnumRangeAttribute(int minimum, int maximum)
			: base(minimum, maximum)
		{
		}

		public EnumRangeAttribute(Type type, string minimum, string maximum)
			: base(type, minimum, maximum)
		{
		}

		public override bool IsValid(object value)
		{
			if (value == null)
			{
				return false;
			}
			if (value.GetType().IsEnum)
			{
				return base.IsValid((int)value);
			}
			else
			{
				return false;
			}
		}
	}
}
