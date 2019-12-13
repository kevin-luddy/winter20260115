using Microsoft.Practices.Unity;

namespace IES.Common
{
    public interface IUnityContainerAccessor
    {
        IUnityContainer Container { get; }
    }
}
