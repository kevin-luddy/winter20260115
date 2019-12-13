// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace CopyWorkspace
{
    using System;
    using System.Web.Mvc;
    using GenBOE;
    using GenBOE.ActionLogic.Validation;
    using IES.Common;
    using IES.Common.classes;
    using Microsoft.Practices.Unity;

    /// <summary>
    /// Loads the Unity Containers
    /// </summary>
    /// <seealso cref="GenBOE.MvcApplication" />
    public class LoadContainers : MvcApplication
    {
        /// <summary>
        /// The logger
        /// </summary>
        private Logger logger = new Logger(typeof(LoadContainers));

        /// <summary>
        /// Initializes the factory.
        /// </summary>
        public void InitializeFactory()
        {
            try
            {
                this.InitializeContainer();
                GenBOEUnityContainer.Container.RegisterType(typeof(ISecurityInformation), typeof(SecurityInformationOverride), this.GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(IActiveDirectoryUtilities)), new ResolvedParameter(typeof(ICache))));
            }
            catch (Exception ex)
            {
                this.logger.Error(ex, "FATAL - InitializeContainer failed.");
                throw;
            }

            try
            {
                IControllerFactory factory = new UnityFactory(GenBOEUnityContainer.Container);
                ControllerBuilder.Current.SetControllerFactory(factory);
            }
            catch (Exception ex)
            {
                this.logger.Error(ex, "FATAL - Unity Factory setup failed.");
                throw;
            }

            try
            {
                // jim 5/14/2011 - this is kind of a workaround to get the Instance property in
                // the validation factory valued since this is a special class that doesn't fit into
                // the 'normal' Dependency Injection model [ask adam about this]
                GenBOEUnityContainer.Container.Resolve(typeof(ValidationFactory));
            }
            catch (Exception ex)
            {
                this.logger.Error(ex, "FATAL - ValidationFactory setup failed.");
                throw;
            }
        }
    }
}
