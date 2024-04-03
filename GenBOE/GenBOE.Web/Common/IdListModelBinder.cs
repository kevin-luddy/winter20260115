using System;
using System.Linq;
using System.Web.Mvc;

// http://stackoverflow.com/questions/9508265/how-do-i-accept-an-array-as-an-asp-net-mvc-controller-action-parameter

namespace GenBOE.Web.Common
{
    public class IdListModelBinder : DefaultModelBinder
    {
        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            if (controllerContext == null)
            {
                throw new ArgumentNullException(nameof(controllerContext), "controllerContext cannot be null.");
            }
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext), "bindingContext cannot be null.");
            }

			ValueProviderResult value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            if (value == null || string.IsNullOrEmpty(value.AttemptedValue))
            {
                return null;
            }

            return value
                .AttemptedValue
                .Split(',')
                .Select(int.Parse)
                .ToList();
        }
    }
}