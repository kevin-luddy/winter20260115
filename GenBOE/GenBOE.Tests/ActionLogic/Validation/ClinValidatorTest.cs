// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.ActionLogic.Validation
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using GenBOE.ActionLogic.Validation;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common.classes;
    using Microsoft.Practices.Unity;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class ClinValidatorTest
    {
        int wsid = 99;
        Mock<IRetriever> retriever = new Mock<IRetriever>();
        Mock<IFullObjectFactory> factory = new Mock<IFullObjectFactory>();

        [TestMethod]
        public void CLINValidator_IsValid()
        {
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);            

            // Set Up CLINs
            var clins = new Collection<FullClin>
            {
                new FullClin { Id = 1, WorkspaceID = wsid, StartDate = Convert.ToDateTime("03/2010"), EndDate = Convert.ToDateTime("10/2010"), ClinNumber = "34", ClinTitle = "Clin34", ClinPaddedNumber = "00000000000000000034??" },
                new FullClin { Id = 2, WorkspaceID = wsid, StartDate = Convert.ToDateTime("03/2009"), EndDate = Convert.ToDateTime("10/2021"), ClinNumber = "35", ClinTitle = "Clin35", ClinPaddedNumber = "00000000000000000035??" },
                new FullClin { Id = 3, WorkspaceID = wsid, StartDate = Convert.ToDateTime("03/2011"), EndDate = Convert.ToDateTime("10/2011"), ClinNumber = "36", ClinTitle = "Clin36", ClinPaddedNumber = "00000000000000000036??" },
                new FullClin { Id = 4, WorkspaceID = wsid, StartDate = Convert.ToDateTime("03/2012"), EndDate = Convert.ToDateTime("10/2012"), ClinNumber = "36", ClinTitle = "Clin37", ClinPaddedNumber = "00000000000000000036??" },
                new FullClin { Id = 5, WorkspaceID = wsid, StartDate =Convert.ToDateTime("03/2010"), EndDate = null, ClinNumber = "38", ClinTitle = "Clin38", ClinPaddedNumber = "00000000000000000038??" },
                new FullClin { Id = 6, WorkspaceID = wsid, StartDate = null, EndDate = Convert.ToDateTime("10/2010"), ClinNumber = "39", ClinTitle = "Clin39", ClinPaddedNumber = "00000000000000000039??" }
            };

            // Set Up Mappers
            WorkspaceDTO workspaceDto = new WorkspaceDTO { Id = wsid, ContractStartDate = Convert.ToDateTime("01/2010"), ContractEndDate = Convert.ToDateTime("12/2020") };

            this.factory.Setup(x => x.CreateFullWorkspace(wsid)).Returns(new FullWorkspace(workspaceDto));
            this.retriever.Setup(x => x.GetClinsByWorkspaceId(wsid)).Returns(clins);

            // Act and Assert
            var sut = new CLINValidator(factory.Object);

            // Unique number and valid dates
            Assert.IsTrue(sut.isValid(new FullClin(clins[0]), null));

            // Unique number and invalid dates
            Assert.IsFalse(sut.isValid(new FullClin(clins[1]), null));

            // Duplicate number and valid dates
            Assert.IsFalse(sut.isValid(new FullClin(clins[3]), null));

            // Unique number but invalid date because start date exists and not end date
            Assert.IsFalse(sut.isValid(new FullClin(clins[4]), null));

            // Unique number but invalid date because end date exists and not start date
            Assert.IsFalse(sut.isValid(new FullClin(clins[5]), null));
        }

        [TestMethod]
        public void CLINValidator_Validation()
        {
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);

            // Set Up CLINs
            var clins = new Collection<FullClin>
            {
                new FullClin { Id = 1, WorkspaceID = wsid, StartDate = Convert.ToDateTime("03/2010"), EndDate = Convert.ToDateTime("10/2010"), ClinNumber = "34", ClinTitle = "Clin34", ClinPaddedNumber = "00000000000000000034??" },
                new FullClin { Id = 2, WorkspaceID = wsid, StartDate = Convert.ToDateTime("03/2011"), EndDate = Convert.ToDateTime("10/2011"), ClinNumber = "35", ClinTitle = "Clin35", ClinPaddedNumber = "00000000000000000035??" },
                new FullClin { Id = 3, WorkspaceID = wsid, StartDate = Convert.ToDateTime("03/2009"), EndDate = Convert.ToDateTime("10/2021"), ClinNumber = "36", ClinTitle = "Clin36", ClinPaddedNumber = "00000000000000000036??" },
                new FullClin { Id = 4, WorkspaceID = wsid, StartDate = Convert.ToDateTime("03/2012"), EndDate = Convert.ToDateTime("10/2012"), ClinNumber = "37", ClinTitle = "Clin37", ClinPaddedNumber = "00000000000000000037??" },
                new FullClin { Id = 5, WorkspaceID = wsid, StartDate = Convert.ToDateTime("12/2009"), EndDate = Convert.ToDateTime("01/2021"), ClinNumber = "37", ClinTitle = "Clin38", ClinPaddedNumber = "00000000000000000037??" },
                new FullClin { Id = 6, WorkspaceID = wsid, StartDate = Convert.ToDateTime("12/2009"), EndDate = Convert.ToDateTime("12/2020"), ClinNumber = "39", ClinTitle = "Clin39", ClinPaddedNumber = "00000000000000000039??" },
                new FullClin { Id = 7, WorkspaceID = wsid, StartDate =Convert.ToDateTime("03/2010"), EndDate = null, ClinNumber = "40", ClinTitle = "Clin40", ClinPaddedNumber = "00000000000000000040??" },
                new FullClin { Id = 8, WorkspaceID = wsid, StartDate = null, EndDate = Convert.ToDateTime("10/2010"), ClinNumber = "41", ClinTitle = "Clin41", ClinPaddedNumber = "00000000000000000041??" }
           
            };

            // Set Up Mappers
            WorkspaceDTO workspaceDto = new WorkspaceDTO { Id = wsid, ContractStartDate = Convert.ToDateTime("01/2010"), ContractEndDate = Convert.ToDateTime("12/2020") };
            //_workspaceMapper.Setup(x => x.GetWorkspaceById(wsid)).Returns(workspaceDto);

            this.factory.Setup(x => x.CreateFullWorkspace(wsid)).Returns(new FullWorkspace(workspaceDto));
            this.retriever.Setup(x => x.GetClinsByWorkspaceId(wsid)).Returns(clins);

            // Act and Assert
            var sut = new CLINValidator(factory.Object);

            // Unique number and valid dates
            Assert.IsTrue(sut.validation(new FullClin(clins[0]), (Collection<Dictionary<string, string>>)null).Count == 0);

            // Unique number and valid dates
            Assert.IsTrue(sut.validation(new FullClin(clins[1]), (Collection<Dictionary<string, string>>)null).Count == 0);

            // Unique number and invalid dates
            Assert.IsTrue(sut.validation(new FullClin(clins[2]), (Collection<Dictionary<string, string>>)null).Count == 2);

            // Duplicate number and valid dates
            Assert.IsTrue(sut.validation(new FullClin(clins[3]), (Collection<Dictionary<string, string>>)null).Count == 1);

            // Duplicate number and invalid dates
            Assert.IsTrue(sut.validation(new FullClin(clins[4]), (Collection<Dictionary<string, string>>)null).Count == 3);

            // Unique number and one invalid date
            Assert.IsTrue(sut.validation(new FullClin(clins[5]), (Collection<Dictionary<string, string>>)null).Count == 1);

            // Unique number but invalid date because start date exists and not end date
            Assert.IsFalse(sut.isValid(new FullClin(clins[6]), null));

            // Unique number but invalid date because end date exists and not start date
            Assert.IsFalse(sut.isValid(new FullClin(clins[7]), null));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CLINValidator_NullIsValidTest()
        {
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);

            var sut = new CLINValidator(factory.Object);
            sut.isValid(null, null);
        }

    }
}
