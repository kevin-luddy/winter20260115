namespace GenBOE.Web.Common
{
    using System;
    using System.Text.RegularExpressions;
    using System.Web.Mvc;
    using IES.Common;

    public class StringNonPrintableCharRemovalModelBinder : DefaultModelBinder
    {
        /// <summary>
        /// Create static Regex object for SkipTabs.
        /// Need to skip tabs "\x09" and line feeds "\x10".
        /// </summary>
        private static Regex regexSkipTabs = new Regex(@"[\x00-\x08||\x0B-\x1F]", RegexOptions.None, Constants.REGEX_TIMEOUT);

        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            if (bindingContext == null) { throw new ArgumentNullException(nameof(bindingContext)); }
            if (controllerContext == null) { throw new ArgumentNullException(nameof(controllerContext)); }

            bool shouldPerformRequestValidation = controllerContext.Controller.ValidateRequest && bindingContext.ModelMetadata.RequestValidationEnabled;

            string result = null;

            ValueProviderResult valueProviderResult = bindingContext.GetValueFromValueProvider(shouldPerformRequestValidation);

            if (valueProviderResult != null)
            {
                result = valueProviderResult.AttemptedValue;
                result = regexSkipTabs.Replace(result, string.Empty);
            }

            return valueProviderResult == null ? base.BindModel(controllerContext, bindingContext) : result;
        }
    }
}