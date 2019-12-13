using Microsoft.Practices.Unity;
using System.Diagnostics.CodeAnalysis;

namespace IES.Common.classes
{ 
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
