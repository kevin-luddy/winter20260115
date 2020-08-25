// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.ActionLogic.WBS
{
    using System;
    using System.Collections.ObjectModel;
    using GenBOE.ActionLogic.WBS;
    using GenBOE.Dtos;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class NestedWBSUtilitiesTest
    {
        [TestMethod]
        public void AdjustLevelsTest()
        {
            NestedWBSUtilities sut = new NestedWBSUtilities();

            WbsDTO wbs0 = new WbsDTO() { Level = 5 };
            WbsDTO wbs1 = new WbsDTO() { Level = 6 };
            WbsDTO wbs2 = new WbsDTO() { Level = 4 };
            WbsDTO wbs3 = new WbsDTO() { Level = 6 };
            WbsDTO wbs4 = new WbsDTO() { Level = 8 };
            Collection<WbsDTO> wbsCollection = new Collection<WbsDTO>() {
                wbs0, wbs1, wbs2, wbs3, wbs4 };

            sut.AdjustLevels(wbsCollection);

            Assert.IsTrue(wbsCollection[0].Level == 1);
            Assert.IsTrue(wbsCollection[1].Level == 2);
            Assert.IsTrue(wbsCollection[2].Level == 0);
            Assert.IsTrue(wbsCollection[3].Level == 2);
            Assert.IsTrue(wbsCollection[4].Level == 4);
        }

        [TestMethod]
        public void SetParentsAndChildrenInUseTest()
        {
            NestedWBSUtilities sut = new NestedWBSUtilities();

            WbsDTO wbs0 = new WbsDTO() { WbsNumber = "1", inUse = false };
            WbsDTO wbs1 = new WbsDTO() { WbsNumber = "1.1", inUse = false };
            WbsDTO wbs2 = new WbsDTO() { WbsNumber = "1.1.1", inUse = true };
            WbsDTO wbs3 = new WbsDTO() { WbsNumber = "1.1.2", inUse = false };
            WbsDTO wbs4 = new WbsDTO() { WbsNumber = "1.1.2.1", inUse = true };
            WbsDTO wbs5 = new WbsDTO() { WbsNumber = "1.1.2.2", inUse = true };
            WbsDTO wbs6 = new WbsDTO() { WbsNumber = "1.1.2.3", inUse = true };
            WbsDTO wbs7 = new WbsDTO() { WbsNumber = "1.1.3", inUse = true };
            WbsDTO wbs8 = new WbsDTO() { WbsNumber = "1.1.3.1", inUse = false };
            WbsDTO wbs9 = new WbsDTO() { WbsNumber = "1.1.3.2", inUse = false };
            WbsDTO wbs10 = new WbsDTO() { WbsNumber = "1.1.4", inUse = true };
            WbsDTO wbs11 = new WbsDTO() { WbsNumber = "1.1.5", inUse = true };
            WbsDTO wbs12 = new WbsDTO() { WbsNumber = "1.2", inUse = false };
            WbsDTO wbs13 = new WbsDTO() { WbsNumber = "1.2.1", inUse = true };
            WbsDTO wbs14 = new WbsDTO() { WbsNumber = "1.2.2", inUse = true };
            WbsDTO wbs15 = new WbsDTO() { WbsNumber = "1.2.3", inUse = true };
            WbsDTO wbs16 = new WbsDTO() { WbsNumber = "1.2.4", inUse = true };
            WbsDTO wbs17 = new WbsDTO() { WbsNumber = "1.2.5", inUse = false };
            Collection<WbsDTO> wbsCollection = new Collection<WbsDTO>() {
                wbs0, wbs1, wbs2, wbs3, wbs4, wbs5, wbs6, wbs7, wbs8, wbs9, wbs10, wbs11, wbs12, wbs13, wbs14, wbs15, wbs16, wbs17 };

            sut.SetParentsAndChildrenInUse(wbsCollection);

            Assert.IsTrue(wbsCollection[0].inUse == true);
            Assert.IsTrue(wbsCollection[1].inUse == true);
            Assert.IsTrue(wbsCollection[2].inUse == true);
            Assert.IsTrue(wbsCollection[3].inUse == true);
            Assert.IsTrue(wbsCollection[4].inUse == true);
            Assert.IsTrue(wbsCollection[5].inUse == true);
            Assert.IsTrue(wbsCollection[6].inUse == true);
            Assert.IsTrue(wbsCollection[7].inUse == true);
            Assert.IsTrue(wbsCollection[8].inUse == true);
            Assert.IsTrue(wbsCollection[9].inUse == true);
            Assert.IsTrue(wbsCollection[10].inUse == true);
            Assert.IsTrue(wbsCollection[11].inUse == true);
            Assert.IsTrue(wbsCollection[12].inUse == true);
            Assert.IsTrue(wbsCollection[13].inUse == true);
            Assert.IsTrue(wbsCollection[14].inUse == true);
            Assert.IsTrue(wbsCollection[15].inUse == true);
            Assert.IsTrue(wbsCollection[16].inUse == true);
            Assert.IsTrue(wbsCollection[17].inUse == false);
        }

        [TestMethod]
        public void SetParentsInUseTest()
        {
            NestedWBSUtilities sut = new NestedWBSUtilities();

            WbsDTO wbs0 = new WbsDTO() { WbsNumber = "1", inUse = false };
            WbsDTO wbs1 = new WbsDTO() { WbsNumber = "1.1", inUse = false };
            WbsDTO wbs2 = new WbsDTO() { WbsNumber = "1.1.1", inUse = true };
            WbsDTO wbs3 = new WbsDTO() { WbsNumber = "1.1.2", inUse = false };
            WbsDTO wbs4 = new WbsDTO() { WbsNumber = "1.1.2.1", inUse = true };
            WbsDTO wbs5 = new WbsDTO() { WbsNumber = "1.1.2.2", inUse = true };
            WbsDTO wbs6 = new WbsDTO() { WbsNumber = "1.1.2.3", inUse = true };
            WbsDTO wbs7 = new WbsDTO() { WbsNumber = "1.1.3", inUse = true };
            WbsDTO wbs8 = new WbsDTO() { WbsNumber = "1.1.3.1", inUse = false };
            WbsDTO wbs9 = new WbsDTO() { WbsNumber = "1.1.3.2", inUse = false };
            WbsDTO wbs10 = new WbsDTO() { WbsNumber = "1.1.4", inUse = true };
            WbsDTO wbs11 = new WbsDTO() { WbsNumber = "1.1.5", inUse = true };
            WbsDTO wbs12 = new WbsDTO() { WbsNumber = "1.2", inUse = false };
            WbsDTO wbs13 = new WbsDTO() { WbsNumber = "1.2.1", inUse = true };
            WbsDTO wbs14 = new WbsDTO() { WbsNumber = "1.2.2", inUse = true };
            WbsDTO wbs15 = new WbsDTO() { WbsNumber = "1.2.3", inUse = true };
            WbsDTO wbs16 = new WbsDTO() { WbsNumber = "1.2.4", inUse = true };
            WbsDTO wbs17 = new WbsDTO() { WbsNumber = "1.2.5", inUse = false };
            Collection<WbsDTO> wbsCollection = new Collection<WbsDTO>() {
                wbs0, wbs1, wbs2, wbs3, wbs4, wbs5, wbs6, wbs7, wbs8, wbs9, wbs10, wbs11, wbs12, wbs13, wbs14, wbs15, wbs16, wbs17 };

            sut.SetParentsInUse(wbsCollection);

            Assert.IsTrue(wbsCollection[0].inUse == true);
            Assert.IsTrue(wbsCollection[1].inUse == true);
            Assert.IsTrue(wbsCollection[2].inUse == true);
            Assert.IsTrue(wbsCollection[3].inUse == true);
            Assert.IsTrue(wbsCollection[4].inUse == true);
            Assert.IsTrue(wbsCollection[5].inUse == true);
            Assert.IsTrue(wbsCollection[6].inUse == true);
            Assert.IsTrue(wbsCollection[7].inUse == true);
            Assert.IsTrue(wbsCollection[8].inUse == false);
            Assert.IsTrue(wbsCollection[9].inUse == false);
            Assert.IsTrue(wbsCollection[10].inUse == true);
            Assert.IsTrue(wbsCollection[11].inUse == true);
            Assert.IsTrue(wbsCollection[12].inUse == true);
            Assert.IsTrue(wbsCollection[13].inUse == true);
            Assert.IsTrue(wbsCollection[14].inUse == true);
            Assert.IsTrue(wbsCollection[15].inUse == true);
            Assert.IsTrue(wbsCollection[16].inUse == true);
            Assert.IsTrue(wbsCollection[17].inUse == false);
        }

        [TestMethod]
        public void SetChildrenInUseTest()
        {
            NestedWBSUtilities sut = new NestedWBSUtilities();

            WbsDTO wbs0 = new WbsDTO() { WbsNumber = "1", inUse = false };
            WbsDTO wbs1 = new WbsDTO() { WbsNumber = "1.1", inUse = false };
            WbsDTO wbs2 = new WbsDTO() { WbsNumber = "1.1.1", inUse = true };
            WbsDTO wbs3 = new WbsDTO() { WbsNumber = "1.1.2", inUse = false };
            WbsDTO wbs4 = new WbsDTO() { WbsNumber = "1.1.2.1", inUse = true };
            WbsDTO wbs5 = new WbsDTO() { WbsNumber = "1.1.2.2", inUse = true };
            WbsDTO wbs6 = new WbsDTO() { WbsNumber = "1.1.2.3", inUse = true };
            WbsDTO wbs7 = new WbsDTO() { WbsNumber = "1.1.3", inUse = true };
            WbsDTO wbs8 = new WbsDTO() { WbsNumber = "1.1.3.1", inUse = false };
            WbsDTO wbs9 = new WbsDTO() { WbsNumber = "1.1.3.2", inUse = false };
            WbsDTO wbs10 = new WbsDTO() { WbsNumber = "1.1.4", inUse = true };
            WbsDTO wbs11 = new WbsDTO() { WbsNumber = "1.1.5", inUse = true };
            WbsDTO wbs12 = new WbsDTO() { WbsNumber = "1.2", inUse = false };
            WbsDTO wbs13 = new WbsDTO() { WbsNumber = "1.2.1", inUse = true };
            WbsDTO wbs14 = new WbsDTO() { WbsNumber = "1.2.2", inUse = true };
            WbsDTO wbs15 = new WbsDTO() { WbsNumber = "1.2.3", inUse = true };
            WbsDTO wbs16 = new WbsDTO() { WbsNumber = "1.2.4", inUse = true };
            WbsDTO wbs17 = new WbsDTO() { WbsNumber = "1.2.5", inUse = false };
            Collection<WbsDTO> wbsCollection = new Collection<WbsDTO>() {
                wbs0, wbs1, wbs2, wbs3, wbs4, wbs5, wbs6, wbs7, wbs8, wbs9, wbs10, wbs11, wbs12, wbs13, wbs14, wbs15, wbs16, wbs17 };

            sut.SetChildrenInUse(wbsCollection);

            Assert.IsTrue(wbsCollection[0].inUse == false);
            Assert.IsTrue(wbsCollection[1].inUse == false);
            Assert.IsTrue(wbsCollection[2].inUse == true);
            Assert.IsTrue(wbsCollection[3].inUse == false);
            Assert.IsTrue(wbsCollection[4].inUse == true);
            Assert.IsTrue(wbsCollection[5].inUse == true);
            Assert.IsTrue(wbsCollection[6].inUse == true);
            Assert.IsTrue(wbsCollection[7].inUse == true);
            Assert.IsTrue(wbsCollection[8].inUse == true);
            Assert.IsTrue(wbsCollection[9].inUse == true);
            Assert.IsTrue(wbsCollection[10].inUse == true);
            Assert.IsTrue(wbsCollection[11].inUse == true);
            Assert.IsTrue(wbsCollection[12].inUse == false);
            Assert.IsTrue(wbsCollection[13].inUse == true);
            Assert.IsTrue(wbsCollection[14].inUse == true);
            Assert.IsTrue(wbsCollection[15].inUse == true);
            Assert.IsTrue(wbsCollection[16].inUse == true);
            Assert.IsTrue(wbsCollection[17].inUse == false);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AdjustLevels_NullTest()
        {
            NestedWBSUtilities sut = new NestedWBSUtilities();
            sut.AdjustLevels(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SetParentsAndChildrenInUse_NullTest()
        {
            NestedWBSUtilities sut = new NestedWBSUtilities();
            sut.SetParentsAndChildrenInUse(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SetParentsInUse_NullTest()
        {
            NestedWBSUtilities sut = new NestedWBSUtilities();
            sut.SetParentsInUse(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SetChildrenInUse_NullTest()
        {
            NestedWBSUtilities sut = new NestedWBSUtilities();
            sut.SetChildrenInUse(null);
        }
    }
}
