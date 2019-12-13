namespace GenBOE.ActionLogic.ValidationAttributes
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using IES.Common;

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Module, AllowMultiple = true)]
    public sealed class LaborSpreadCostValidationAttribute : ValidationAttribute
    {
        // Create static Regex object for SpreadValueFormat.
        private static Regex regexSpreadValueFormat = new Regex(@"(^[+-]?\d{0,10}([.]\d{1,2})?$)", RegexOptions.None, Constants.REGEX_TIMEOUT);

        public string LaborSpreadValue { get; set; }
        public string IsSpreadTypeCost { get; set; }
        /// <summary>
        /// Boolean about whether the rate type changed or not.
        /// </summary>
        public string RateTypeChanged { get; set; }

        /// <summary>
        /// This method will be called to validate a LaborSpread "Cost" value.
        ///     If SpreadType is "Hours", skip validation (return true).
        ///     If SpreadType is "Cost", the spread value must be between -9,999,999,999.99 and 9,999,999,999.99.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public override bool IsValid(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            Type contextType = value.GetType();

            PropertyInfo laborSpreadValueProperty = contextType.GetProperty(this.LaborSpreadValue);
            if (laborSpreadValueProperty == null)
            {
                throw new ArgumentException(
                    string.Format(
                        "LaborSpreadValue must be a valid property. '{0}' is not a property on the type '{1}'.",
                        this.LaborSpreadValue,
                        contextType.Name));
            }

            PropertyInfo isSpreadTypeCostProperty = contextType.GetProperty(this.IsSpreadTypeCost);
            if (isSpreadTypeCostProperty == null)
            {
                throw new ArgumentException(
                    string.Format(
                        "IsSpreadTypeCost must be a valid property. '{0}' is not a property on the type '{1}'.",
                        this.IsSpreadTypeCost,
                        contextType.Name));
            }

            PropertyInfo rateTypeChangedProperty = contextType.GetProperty(this.RateTypeChanged);
            if (rateTypeChangedProperty == null)
            {
                throw new ArgumentException(
                    string.Format(
                        "RateTypeChanged must be a valid property. '{0}' is not a property on the type '{1}'.",
                        this.RateTypeChanged,
                        contextType.Name));
            }

            object laborSpreadValueObject = laborSpreadValueProperty.GetValue(value, null) ?? 0;
            object isSpreadTypeCostObject = isSpreadTypeCostProperty.GetValue(value, null) ?? false;
            object rateTypeChangedObject = rateTypeChangedProperty.GetValue(value, null) ?? false;

            decimal laborSpreadValue;
            bool isSpreadTypeCost, rateTypeChanged;

            try
            {
                laborSpreadValue = Convert.ToDecimal(laborSpreadValueObject);
            }
            catch (FormatException)
            {
                throw new ArgumentException(
                    string.Format(
                        "LaborSpreadValue must be a valid number. '{0}' is not a valid number on the type '{1}'.", this.LaborSpreadValue,
                        contextType.Name));
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException(
                    string.Format(
                        "LaborSpreadValue must be a valid number. '{0}' is not a valid number on the type '{1}'.", this.LaborSpreadValue,
                        contextType.Name));
            }

            try
            {
                isSpreadTypeCost = Convert.ToBoolean(isSpreadTypeCostObject);
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException(
                    string.Format(
                        "IsSpreadTypeCost must be a valid boolean. '{0}' is not a valid boolean on the type '{1}'.", this.IsSpreadTypeCost,
                        contextType.Name));
            }

            try
            {
                rateTypeChanged = Convert.ToBoolean(rateTypeChangedObject);
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException(
                    string.Format(
                        "RateTypeChanged must be a valid boolean. '{0}' is not a valid boolean on the type '{1}'.", this.RateTypeChanged,
                        contextType.Name));
            }

            // Only check if the rate type has not changed
            if (!rateTypeChanged)
            {
                //  If SpreadType is "Hours", skip validation (return true).
                if (!isSpreadTypeCost)
                {
                    return true;
                }

                // If SpreadType is "Cost"
                if (isSpreadTypeCost)
                {

                    // Spread value must be between -9,999,999,999.99 and 9,999,999,999.99.
                    if (laborSpreadValue < -9999999999.99M || laborSpreadValue > 9999999999.99M)
                    {
                        return false;
                    }

                    // Spread value may contain up to 10 digits to the left, and 2 digits to the right of the decimal point
                    Match m = regexSpreadValueFormat.Match(laborSpreadValue.ToString());

                    if (!m.Success)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}