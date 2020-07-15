// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

using GenBOE.ActionLogic.ModelView.Workspace;
using IES.Common;
using IES.Common.classes;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace GenBOE.Web.Common
{
    public class CreateWorkspaceStepOneModelBinder : DefaultModelBinder
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
            bindingContext.ModelName = "data";
            switch (SystemConfiguration.Instance().CompanyMode)
            {
                case CompanyConfiguration.MST:
                    bindingContext.ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(null, typeMap["CreateWorkspaceStepOneMSTModelView"]);
                    break;
                default:
                case CompanyConfiguration.SpaceSystems:
                    bindingContext.ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(null, typeMap["CreateWorkspaceStepOneSpaceModelView"]);
                    break;
            }
            return base.BindModel(controllerContext, bindingContext);
        }

        static Dictionary<string, Type> typeMap = new Dictionary<string, Type>{
		        {"CreateWorkspaceStepOneSpaceModelView", typeof(CreateWorkspaceStepOneSpaceModelView)},
		        {"CreateWorkspaceStepOneMSTModelView", typeof(CreateWorkspaceStepOneMSTModelView)}
	        };
    }
}