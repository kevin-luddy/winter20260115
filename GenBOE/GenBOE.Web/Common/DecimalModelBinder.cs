using System;
using System.Web.Mvc;

// http://stackoverflow.com/questions/5500150/mvc3-model-binding-causes-the-parameter-conversion-from-type-system-int32-to

namespace GenBOE.Web.Common
{
    public class DecimalModelBinder : DefaultModelBinder
    {
        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext),"Binding Context was null");
            }

			ValueProviderResult valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

            decimal value;
            return valueProviderResult == null || !Decimal.TryParse(valueProviderResult.AttemptedValue, out value) ? base.BindModel(controllerContext, bindingContext) : value;
        }
    }
}