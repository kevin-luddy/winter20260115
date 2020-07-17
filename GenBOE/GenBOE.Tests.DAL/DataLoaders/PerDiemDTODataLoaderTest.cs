// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.DAL.DataLoaders
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using GenBOE.DataBridge.DTO;
    using System.Collections.ObjectModel;
    using GenBOE.Dtos;
    using IES.Common;

    [TestClass]
    public class PerDiemDTODataLoaderTest
    {
        [TestMethod]
        public void L_GetPerDiemDTOByIds()
        {

            var sut = new PerDiemDTODataLoader();
            string qualification = "mock" + Guid.NewGuid().ToString().Substring(0, 30);
            PerDiemDTO perDiem = new PerDiemDTO { Id = -1, PerDiemNotes = "mock notes", PerDiemDestination = "MockPerDiemDest", HotelRate = 50m, MIERate = 50m, LastUpdatedBy = GlobalTestCaseSetup.GlobalBOEAuthorID, UpdateDate = DateTime.Now, Qualification = qualification, Updateable=UpdateType.Upsert };

            int id = sut.SavePerDiem(perDiem);

            Assert.IsTrue(id > 0, "per diem didn't save");

            PerDiemDTO returnedDiem = sut.GetByIds(new Collection<int> { id }).FirstOrDefault();
            Assert.AreEqual(returnedDiem.HotelRate, perDiem.HotelRate, "hotel rate didn't match");
            Assert.AreEqual(returnedDiem.MIERate, perDiem.MIERate, "mie rate didn't match");
            Assert.AreEqual(returnedDiem.PerDiemDestination, perDiem.PerDiemDestination, "per diem dest didn't match");


        }

        [TestMethod]
        public void L_GetAllPerDiem()
        {
            var sut = new PerDiemDTODataLoader();
            string qualification = "mock" + Guid.NewGuid().ToString().Substring(0, 30);
            PerDiemDTO perDiem = new PerDiemDTO { Id = -1, PerDiemNotes = "mock notes", PerDiemDestination = "MockPerDiemDest", HotelRate = 50m, MIERate = 50m, LastUpdatedBy = GlobalTestCaseSetup.GlobalBOEAuthorID, UpdateDate = DateTime.Now,Qualification = qualification, Updateable=UpdateType.Upsert };

            int id = sut.SavePerDiem(perDiem);

            Assert.IsTrue(id > 0, "per diem didn't save");

            ICollection<PerDiemDTO> perDiems = sut.GetAllPerDiem();

            Assert.IsTrue(perDiems.Count > 0, "no per diems exist");

        }

        [TestMethod]
        public void L_SavePerDiem()
        {
            var sut = new PerDiemDTODataLoader();
            string qualification = "mock" + Guid.NewGuid().ToString().Substring(0, 30);
            PerDiemDTO perDiem = new PerDiemDTO { Id = -1, PerDiemNotes = "mock notes", PerDiemDestination = "MockPerDiemDest", HotelRate = 50m, MIERate = 50m, LastUpdatedBy = GlobalTestCaseSetup.GlobalBOEAuthorID, UpdateDate = DateTime.Now, Qualification = qualification, Updateable=UpdateType.Upsert };

            int id = sut.SavePerDiem(perDiem);

            Assert.IsTrue(id > 0, "per diem didn't save");

            //now let's edit the hotel rate of this per diem
            perDiem = sut.GetByIds(new Collection<int> { id }).FirstOrDefault();
            perDiem.HotelRate = 75m;
            perDiem.Updateable = UpdateType.Upsert;

            id = sut.SavePerDiem(perDiem);

            perDiem = sut.GetByIds(new Collection<int> { id }).FirstOrDefault();

            Assert.AreEqual(perDiem.Id, id, "ids didn't match");
            Assert.AreEqual(perDiem.HotelRate, 75m, "hotel rate doesn't match");

        }
        
    }
}
