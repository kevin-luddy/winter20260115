using System;
using System.Web.Mvc;

namespace GenBOE.Web.Common
{
    public class LongModelBinder : DefaultModelBinder
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly")]
        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException("Binding Context was null");
            }

            var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

            long value;
            return valueProviderResult == null || !long.TryParse(valueProviderResult.AttemptedValue, out value) ? base.BindModel(controllerContext, bindingContext) : value;
        }
    }
}