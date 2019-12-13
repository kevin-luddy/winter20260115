using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace GenBOE.Web.Common
{
    /// <summary>
    /// Binding to use to troublshoot binding errors.  Will output the field name that is a duplicate.
    /// Uncomment this line in the Global.asax.cs file (ModelBinders.Binders.DefaultBinder = new DebugModelBinder();) to activate.
    /// </summary>
    public class DebugModelBinder :  DefaultModelBinder, IModelBinder
    {
        /// <summary>
        /// Binds the model.  Called by the ASP.NET MVC framework
        /// </summary>
        /// <param name="controllerContext">the <see cref="ControllerContext"/> object</param>
        /// <param name="bindingContext">the <see cref="ModelBindingContext"/> object</param>
        /// <returns></returns>
        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            if (controllerContext == null)
            {
                throw new ArgumentNullException(nameof(controllerContext), "controllerContext can't be null.");
            }
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext), "bindingContext can't be null.");
            }

            Dictionary<string, ModelMetadata> d = new Dictionary<string, ModelMetadata>(StringComparer.OrdinalIgnoreCase);
            foreach (var p in bindingContext.ModelMetadata.Properties)
            {
                var propertyName = p.PropertyName;
                try
                {
                    d.Add(propertyName, null);
                }
                catch (ArgumentException ex)
                {
                    throw new ArgumentException(
                        String.Format("The Item {0} has already been added", propertyName), ex);
                }
            }
            return base.BindModel(controllerContext, bindingContext);
        }
    }
}