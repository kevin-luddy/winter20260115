// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.ActionLogic.BLL
{
    using GenBOE.ActionLogic.BLL;
    using GenBOE.ActionLogic.BOETransitions;
    using GenBOE.DataBridge.Common;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Objects;
    using IES.Common.classes;
    using Microsoft.Practices.Unity;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class BOEMediatorTest : MOQObject
    {
        private Mock<IBOEStateMachine> _boeStateMachine = null;
        private Mock<IFullObjectFactory> factory;
        private Mock<IRetriever> retriever;
        private Mock<IPermissionsDTODataLoader> _PermissionDataLoader = null;
        private Mock<IBoeDTODataLoader> _BoeLoader = null;
        private Mock<ICommonDataMapper> _CommonDataMapper = null;

        private BoeMediator CreateSystem()
        {
            _BoeLoader = new Mock<IBoeDTODataLoader>();
            _CommonDataMapper = new Mock<ICommonDataMapper>();
            this.factory = new Mock<IFullObjectFactory>();
            this.retriever = new Mock<IRetriever>();
            _boeStateMachine = new Mock<IBOEStateMachine>();
            _PermissionDataLoader = new Mock<IPermissionsDTODataLoader>();
            BoeMediator _boeMediator = new BoeMediator(null, _BoeLoader.Object, _PermissionDataLoader.Object);

            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), _CommonDataMapper.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), _PermissionDataLoader.Object);
            return _boeMediator;
        }
    }
}
