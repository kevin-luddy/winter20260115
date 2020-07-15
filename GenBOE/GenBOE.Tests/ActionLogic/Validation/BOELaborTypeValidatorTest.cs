// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.ActionLogic.Validation
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using GenBOE.ActionLogic.Validation;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class BOELaborTypeValidatorTest : MOQObject
    {
        [TestInitialize]
        override public void Setup()
        {
            base.Setup();

            DateTime workspaceStart = Convert.ToDateTime("01/01/2011");
            DateTime workspaceEnd = Convert.ToDateTime("01/01/2014");
            DateTime clinStart = Convert.ToDateTime("02/01/2012");
            DateTime clinEnd = Convert.ToDateTime("01/01/2013");
            DateTime boeStart = Convert.ToDateTime("04/01/2012");
            DateTime boeEnd = Convert.ToDateTime("12/01/2012");

            this.Workspace.ContractStartDate = workspaceStart;
            this.Workspace.ContractEndDate = workspaceEnd;
            this.Clin1.StartDate = clinStart;
            this.Clin1.EndDate = clinEnd;
            this.Boe1.StartDate = boeStart;
            this.Boe1.EndDate = boeEnd;
        }

        [TestMethod]
        public void BOELaborTypeValidator_IsValid()
        {
            ////var boeDTOMapper = new Mock<IBoeDTODataMapper>();

            ////boeDTOMapper.Setup(x => x.GetBoeDataByBoeID(this.Boe1.Id)).Returns(this.Boe1);

            DateTime boeLTStart = Convert.ToDateTime("05/01/2012");
            DateTime boeLTEnd = Convert.ToDateTime("12/01/2012");
            IEnumerable<BoeTaskElementDTO> boeTaskElements = new Collection<BoeTaskElementDTO> {{new BoeTaskElementDTO{BoeID=this.Boe1.Id, StartDate=this.Boe1.StartDate, EndDate=this.Boe1.EndDate, Id=1, 
                    taskElementLabors= new Collection<ResourceTypeDto>{new ResourceTypeDto{BoeID=this.Boe1.Id,  SpreadType = IES.Common.SpreadType.Hours, EndDateValue=boeLTEnd, StartDateValue=boeLTStart, Id=1}}}} };

            StartEndDateTypeValidator sut = new StartEndDateTypeValidator(this.Boe1.StartDate, this.Boe1.EndDate, "BOE");
            bool returnValue = sut.isValid(boeTaskElements, null);
            Assert.IsTrue(returnValue);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void BOELTTypeValidator_NullIsValidTest()
        {
            StartEndDateTypeValidator sut = new StartEndDateTypeValidator(null, null, "BOE");
            sut.isValid(null, null);
        }
    }
}
