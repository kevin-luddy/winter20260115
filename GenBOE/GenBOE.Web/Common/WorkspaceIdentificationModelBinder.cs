// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Web.Common
{
    using System;
    using System.Collections.Generic;
    using System.Web.Mvc;
    using GenBOE.ActionLogic.ModelView.Workspace;
    using IES.Common;
    using IES.Common.classes;

    public class WorkspaceIdentificationModelBinder : DefaultModelBinder
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
            bindingContext.ModelName = "workspaceDetails";
            switch (SystemConfiguration.Instance().CompanyMode)
            {
                case CompanyConfiguration.MST:
                    bindingContext.ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(null, typeMap["WorkspaceIdentificationMSTModelView"]);
                    break;

                case CompanyConfiguration.SpaceSystems:
                    bindingContext.ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(null, typeMap["WorkspaceIdentificationSpaceModelView"]);
                    break;
                case CompanyConfiguration.ISGS:
                default:
                    bindingContext.ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(null, typeMap["WorkspaceIdentificationISGSModelView"]);
                    break;
            }
            return base.BindModel(controllerContext, bindingContext);
        }

        static Dictionary<string, Type> typeMap = new Dictionary<string, Type>{
		        {"WorkspaceIdentificationSpaceModelView", typeof(WorkspaceIdentificationSpaceModelView)},
		        {"WorkspaceIdentificationMSTModelView", typeof(WorkspaceIdentificationMSTModelView)}
	        };
    }
}