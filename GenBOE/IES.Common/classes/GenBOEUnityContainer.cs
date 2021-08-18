// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common.classes
{
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Practices.Unity;

    [ExcludeFromCodeCoverage]
    public sealed class GenBOEUnityContainer : IUnityContainerAccessor
    {
        #region IUnityContainerAccessor Members
        public static IUnityContainer Container { get; set; }

        static GenBOEUnityContainer()
        {
            Container = new UnityContainer();
        }

        IUnityContainer IUnityContainerAccessor.Container
        {
            get { return Container; }
        }

        public static T Resolve<T>()
        {
            return Container.Resolve<T>();
        }

        #endregion IUnityContainerAccessor Members
    }
}
