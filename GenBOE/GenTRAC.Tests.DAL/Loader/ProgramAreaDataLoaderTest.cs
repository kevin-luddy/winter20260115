// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.Tests.DAL.Loader
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using GenTRAC.DataBridge.DTO;
    using GenTRAC.Models;
    using IES.Common.PickList;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Test class for the ProgramAreaDataLoaderTest
    /// </summary>
    [TestClass]
    public class ProgramAreaDataLoaderTest
    {
        /// <summary>
        /// IProgramAreaDataLoader
        /// </summary>
        private ProgramAreaDataLoader sut = new ProgramAreaDataLoader();

        /// <summary>
        ///A test for Get ALL
        ///</summary>
        [TestMethod]
        public void L_GetProgramAreaAll_Test()
        {
            ICollection<PickListDto> actual = this.sut.GetPickListValues();

            int totalProgramAreas;

            using (genTRACEntities gbe = new genTRACEntities())
            {
                totalProgramAreas = (from p in gbe.ProgramAreaLUs
                                  select p).Count();
            }

            Assert.AreEqual(totalProgramAreas, actual.Count, "The number of Program Areas returned did not match the number in the database.");
        }

        /// <summary>
        /// A test for GetById
        ///</summary>
        [TestMethod]
        public void L_GetProgramAreaById_Test()
        {
            PickListDto expected = null;

            int id = this.sut.GetPickListValues().FirstOrDefault().Id; // should always have values in db

            PickListDto actual = this.sut.GetById(id);

            // Assert obj from db is not null
            Assert.IsNotNull(actual);

            // Assert that the object initialized as null is not same as the actual obj from db
            Assert.AreNotEqual(expected, actual);
        }

        /// <summary>
        /// A test for GetById (multiple)
        ///</summary>
        [TestMethod]
        public void L_GetProgramAreaByIdMultipleTest()
        {
            int id1 = this.sut.GetPickListValues().ElementAt(0).Id; // should always have values in db
            int id2 = this.sut.GetPickListValues().ElementAt(1).Id;
            ICollection<int> ids = new Collection<int>() { id1, id2 };

            ICollection<PickListDto> actual = this.sut.GetByIds(ids);

            Assert.IsNotNull(actual);
            Assert.AreEqual(2, actual.Count);
            Assert.IsTrue(actual.Where(x => x.Id == id1).Any());
            Assert.IsTrue(actual.Where(x => x.Id == id2).Any());
        }
    }
}
