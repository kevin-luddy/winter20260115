// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

using GenBOE.ActionLogic.ModelView.BOE;
using IES.Common;
using IES.Common.classes;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace GenBOE.Web.Common
{
    public class TitleModelBinder : DefaultModelBinder
    {
        /// <summary>
        /// Binds to proper type based on company configuration
        /// </summary>
        /// <param name="controllerContext">The <see cref="ControllerContext"/></param>
        /// <param name="bindingContext">The <see cref="ModelBindingContext"/></param>
        /// <returns></returns>
        public override object BindModel(ControllerContext controllerContext,
            ModelBindingContext bindingContext)
        {
            if (controllerContext == null)
            {
                throw new ArgumentNullException(nameof(controllerContext), "controllerContext cannot be null.");
            }
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext), "bindingContext cannot be null.");
            }
            bindingContext.ModelName = "inBoeHeader";
            switch (SystemConfiguration.Instance().CompanyMode)
            {
                case CompanyConfiguration.MST:
                // Requirements are identical to SSC so we can re-use the Space implementation here
                case CompanyConfiguration.SpaceSystems:
                    bindingContext.ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(null, typeMap["BOEHeaderSpaceModelView"]);
                    break;
                case CompanyConfiguration.ISGS:
                default:
                    bindingContext.ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(null, typeMap["BOEHeaderISGSModelView"]);
                    break;
            }
            return base.BindModel(controllerContext, bindingContext);
        }

        static Dictionary<string, Type> typeMap = new Dictionary<string, Type>{
		        {"BOEHeaderISGSModelView", typeof(BOEHeaderISGSModelView)},
		        {"BOEHeaderSpaceModelView", typeof(BOEHeaderSpaceModelView)}
	        };
    }
}