// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.ActionLogic.ControllerLogic
{
	using System.IO;
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using Moq;
	using GenBOE.ActionLogic._ControllerLogic.Backend;
	using GenBOE.Objects;
	using System.Collections.ObjectModel;
	using GenBOE.Dtos;
	using GenBOE.ActionLogic.BLL;
	using GenBOE.ActionLogic.BOETransitions;
	using GenBOE.ActionLogic.IO.Export;
	using GenBOE.ActionLogic.Common.Email;
	using GenBOE.DataBridge.DTO;
	using GenBOE.ActionLogic.Validation;
	using GenBOE.ActionLogic.Common.Calculations;
	using System.Collections.Generic;
	using System;
	using IES.Common.classes;
	using Microsoft.Practices.Unity;
	using GenBOE.DataBridge.Common;
	using GenBOE.ActionLogic.IO.Import;

	/// <summary>
	/// genBOE Angular CLIN tests
	/// </summary>
	[TestClass]
	public class CLINControllerLogicTests
	{
		Mock<GenBOE.Objects.IFullObjectFactory> fullObjectFactory;
		Mock<IValidationHelper> validationHelper;
		Mock<IVariableSelectBOEtoSumCalculation> variableSelectBOEtoSumCalculation;
		Mock<IBoeTaskElementRecalculation> boeTaskElementRecalculation;
		Mock<IBoeTaskElementMediator> boeTaskElementMediator;
		Mock<IBOEStateMachine> boeStateMachine;
		Mock<IBoeMediator> boeMediator;
		Mock<IClinDTODataLoader> clinDTODataLoader;
		Mock<IWorkspaceVariableDTODataLoader> workspaceVariableDTODataLoader;
		Mock<ContractTypeLoader> contractTypeLoader;
		Mock<IBoeEmailer> boeEmailer;
		Mock<ICLINExporter> clinExporter;
		Mock<ICLINImporter> clinImporter;
		Mock<GenBOE.Objects.IRetriever> retriever = new Mock<GenBOE.Objects.IRetriever>();
		Mock<ICommonDataMapper> commonDataMapper = new Mock<ICommonDataMapper>();
		private Mock<IPermissionsDTODataLoader> _permissions = new Mock<IPermissionsDTODataLoader>();
		Mock<IBoeDTODataLoader> boeLoader = new Mock<IBoeDTODataLoader>();

		/// <summary>
		/// Setup ControllerLogic
		/// </summary>
		/// <returns></returns>
		public CLINControllerLogic CreateSut()
		{
			this.fullObjectFactory = new Mock<GenBOE.Objects.IFullObjectFactory>();
			this.validationHelper = new Mock<IValidationHelper>();
			this.variableSelectBOEtoSumCalculation = new Mock<IVariableSelectBOEtoSumCalculation>();
			this.boeTaskElementRecalculation = new Mock<IBoeTaskElementRecalculation>();
			this.boeTaskElementMediator = new Mock<IBoeTaskElementMediator>();
			this.boeStateMachine = new Mock<IBOEStateMachine>();
			this.boeMediator = new Mock<IBoeMediator>();
			this.clinDTODataLoader = new Mock<IClinDTODataLoader>();
			this.workspaceVariableDTODataLoader = new Mock<IWorkspaceVariableDTODataLoader>();
			this.contractTypeLoader = new Mock<ContractTypeLoader>();
			this.boeEmailer = new Mock<IBoeEmailer>();
			this.clinExporter = new Mock<ICLINExporter>();
			this.clinImporter = new Mock<ICLINImporter>();
			this.boeLoader = new Mock<IBoeDTODataLoader>();


			return new CLINControllerLogic(fullObjectFactory.Object, validationHelper.Object, variableSelectBOEtoSumCalculation.Object, boeTaskElementRecalculation.Object,
				boeTaskElementMediator.Object, boeStateMachine.Object, boeMediator.Object, boeLoader.Object, clinDTODataLoader.Object, workspaceVariableDTODataLoader.Object,
				contractTypeLoader.Object, boeEmailer.Object, clinExporter.Object, clinImporter.Object);
		}

		/// <summary>
		/// Test the Invalid Exception throw
		/// </summary>
		[TestMethod]
		public void ExportCLINs_InvalidException()
		{
			GenBOEUnityContainer.Container.RegisterInstance(typeof(GenBOE.Objects.IRetriever), retriever.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), _permissions.Object);

			GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), commonDataMapper.Object);

			CLINControllerLogic sut = CreateSut();
			Mock<FullWorkspace> workspaceMock = new Mock<FullWorkspace>();
			ClinDTO clinDto = new ClinDTO
			{
				Id = 1,
				ClinNumber = "123",
				ClinTitle = "Test CLIN",
				ClinPaddedNumber = "00123",
				StartDate = DateTime.Now,
				EndDate = DateTime.Now,
				WorkspaceID = 1,
				InUse = false,
				ContractType = 1
			};
			FullClin expectedCLIN = new FullClin(clinDto);
			IReadOnlyCollection<FullClin> expectedCLINCollection = new ReadOnlyCollection<FullClin>(new List<FullClin> { expectedCLIN });

			workspaceMock.Setup(w => w.ClinsNoMultiClin).Returns(expectedCLINCollection);

			// Act and Assert
			try
			{
				using (FileStream result = sut.ExportCLINs(workspaceMock.Object))
				{
					Assert.Fail("Expected InvalidOperationException to be thrown");
				}
			}
			catch (InvalidOperationException ex)
			{
				Assert.IsNotNull(ex);
			}
		}
	}
}