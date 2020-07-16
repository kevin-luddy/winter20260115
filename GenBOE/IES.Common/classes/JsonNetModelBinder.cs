// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common
{
    using System;
    using System.IO;
    using System.Text;
    using System.Web.Mvc;
    using Newtonsoft.Json;

    /// <summary>
    /// Model Binder telling MVC to use Json.NET
    /// </summary>
    /// <seealso cref="IModelBinder" />
    public class JsonNetModelBinder : IModelBinder
    {
        /// <summary>
        /// Binds the model to a value by using the specified controller context and binding context.
        /// </summary>
        /// <param name="controllerContext">The controller context.</param>
        /// <param name="bindingContext">The binding context.</param>
        /// <returns>
        /// The bound value.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// bindingContext
        /// or
        /// controllerContext
        /// </exception>
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            if(bindingContext == null)
		    {
			    throw new ArgumentNullException(nameof(bindingContext));
		    }

            if (controllerContext == null)
            {
                throw new ArgumentNullException(nameof(controllerContext));
            }

            controllerContext.HttpContext.Request.InputStream.Position = 0;
            var stream = controllerContext.RequestContext.HttpContext.Request.InputStream;
            var readStream = new StreamReader(stream, Encoding.UTF8);
            var json = readStream.ReadToEnd();
            return JsonConvert.DeserializeObject(json, bindingContext.ModelType);
        }
    }
}
