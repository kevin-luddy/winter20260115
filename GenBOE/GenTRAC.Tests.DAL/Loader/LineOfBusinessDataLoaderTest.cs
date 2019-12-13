// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
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
    /// Test class for the LineOfBusinessDataLoaderTest
    /// </summary>
    [TestClass]
    public class LineOfBusinessDataLoaderTest
    {
        /// <summary>
        /// ILineOfBusinessDataLoader
        /// </summary>
        private IPickListLoader sut = new LineOfBusinessDataLoader();

        /// <summary>
        /// A test for Get All
        /// </summary>
        [TestMethod]
        public void L_GetLineOfBusinessAllTest()
        {
            ICollection<PickListDto> actual = this.sut.GetPickListValues();

            int totalLinesOfBusiness;

            using (genTRACEntities gbe = new genTRACEntities())
            {
                totalLinesOfBusiness = (from p in gbe.LineOfBusinessLUs
                                     select p).Count();
            }

            Assert.AreEqual(totalLinesOfBusiness, actual.Count, "The number of Lines of Business returned did not match the number in the database.");
        }

        /// <summary>
        /// A test for GetById
        /// </summary>
        [TestMethod]
        public void L_GetLineOfBusinessByIdTest()
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
        public void L_GetLineOfBusinessByIdMultipleTest()
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
