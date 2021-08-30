// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.ActionLogic
{
    using GenBOE.ActionLogic.ControllerLogic;
    using GenBOE.ActionLogic.ModelView;
    using IES.Common;
    using IES.Common.classes;
    using GenBOE.DataBridge.Common;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using Microsoft.Practices.Unity;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    [TestClass]
    public class BOETravelCostControllerLogicTest
    {
        /// <summary>
        /// Mock Trave DTO loader
        /// </summary>
        private Mock<ITravelDTODataLoader> _TravelDTOLoader;

        /// <summary>
        /// Travel Controller Logic
        /// </summary>
        private TravelControllerLogic _travelControllerLogic;

        /// <summary>
        /// Mock Retriever
        /// </summary>
        private Mock<IRetriever> _retriever;

        /// <summary>
        /// Mock Factory
        /// </summary>
        private Mock<IFullObjectFactory> _factory;

        /// <summary>
        /// Mock Permissions loader
        /// </summary>
        private Mock<IPermissionsDTODataLoader> _perissionsDtoDataLoader;

        /// <summary>
        /// Mock Common Data Mapper
        /// </summary>
        private Mock<ICommonDataMapper> _commonDataMapper;

        /// <summary>
        /// Mock Custom Field Value Loader
        /// </summary>
        private Mock<ICustomFieldValueDTODataLoader> customFieldValueLoader = new Mock<ICustomFieldValueDTODataLoader>();

        /// <summary>
        /// Initializes data before each test run for this class.
        /// </summary>
        [TestInitialize]
        public void Initialize()
        {
            _retriever = new Mock<IRetriever>();
            _factory = new Mock<IFullObjectFactory>();
            _perissionsDtoDataLoader = new Mock<IPermissionsDTODataLoader>();
            _commonDataMapper = new Mock<ICommonDataMapper>();
            _TravelDTOLoader = new Mock<ITravelDTODataLoader>();

            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), _retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), _factory.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), _perissionsDtoDataLoader.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), _commonDataMapper.Object);

            this._travelControllerLogic = new TravelControllerLogic(_TravelDTOLoader.Object, customFieldValueLoader.Object);
        }

        [TestMethod]
        public void ReorderTravelTaskElements_Test()
        {
              #region set up boetask elements
            TravelDTO task1 = new TravelDTO()
            {
                TaskTitle = "title1",
                BOETaskElementOrder = 0,
                Id = 1
            };
            TravelDTO task2 = new TravelDTO()
            {
                TaskTitle = "title2",
                BOETaskElementOrder = 0,
                Id = 2
            };
            TravelDTO task3 = new TravelDTO()
            {
                TaskTitle = "title3",
                BOETaskElementOrder = 0,
                Id = 3
            };
            TravelDTO task4 = new TravelDTO()
            {
                TaskTitle = "title4",
                BOETaskElementOrder = 0,
                Id = 4
            };
            TravelDTO task5 = new TravelDTO()
            {
                TaskTitle = "title5",
                BOETaskElementOrder = 0,
                Id = 5
            };

            //set up boe collection that exist in our DB

            ICollection<TravelDTO> BoeTaskElementCollection = new Collection<TravelDTO>();
            BoeTaskElementCollection.Add(task1);
            BoeTaskElementCollection.Add(task2);
            BoeTaskElementCollection.Add(task3);
            BoeTaskElementCollection.Add(task4);
            BoeTaskElementCollection.Add(task5);
            #endregion
            #region setup updated task
            TravelDTO utask1 = new TravelDTO()
            {
                TaskTitle = "title1",
                BOETaskElementOrder = 4,
                Updateable = UpdateType.Upsert,
                Id = 1
            };
            TravelDTO utask2 = new TravelDTO()
            {
                TaskTitle = "title2",
                BOETaskElementOrder = 3,
                Updateable = UpdateType.Upsert,
                Id = 2
            };
            TravelDTO utask3 = new TravelDTO()
            {
                TaskTitle = "title3",
                BOETaskElementOrder = 2,
                Updateable = UpdateType.Upsert,
                Id = 3
            };
            TravelDTO utask4 = new TravelDTO()
            {
                TaskTitle = "title4",
                BOETaskElementOrder = 1,
                Updateable = UpdateType.Upsert,
                Id = 4
            };
            TravelDTO utask5 = new TravelDTO()
            {
                TaskTitle = "title5",
                BOETaskElementOrder = 0,
                Id = 5,
                Updateable = UpdateType.Upsert
            };

            ICollection<TravelDTO> UpdatedTaskElementCollection = new Collection<TravelDTO>();

            UpdatedTaskElementCollection.Add(utask1);
            UpdatedTaskElementCollection.Add(utask2);
            UpdatedTaskElementCollection.Add(utask3);
            UpdatedTaskElementCollection.Add(utask4);
            UpdatedTaskElementCollection.Add(utask5);
            #endregion
            #region order from user
            TaskElementOrder order1 = new TaskElementOrder()
            {
                TaskID = task1.Id,
                ListOrder = 4
            };
            TaskElementOrder order2 = new TaskElementOrder()
            {
                TaskID = task2.Id,
                ListOrder = 3
            };
            TaskElementOrder order3 = new TaskElementOrder()
            {
                TaskID = task3.Id,
                ListOrder = 2
            };
            TaskElementOrder order4 = new TaskElementOrder()
            {
                TaskID = task4.Id,
                ListOrder = 1
            };
            TaskElementOrder order5 = new TaskElementOrder()
            {
                TaskID = task5.Id,
                ListOrder = 0
            };

            //set up the user input.
            TaskElementOrderCollection UserTaskElementCollection = new TaskElementOrderCollection();
            UserTaskElementCollection.BOETaskElements = new Collection<TaskElementOrder>();
            UserTaskElementCollection.BOETaskElements.Add(order1);
            UserTaskElementCollection.BOETaskElements.Add(order2);
            UserTaskElementCollection.BOETaskElements.Add(order3);
            UserTaskElementCollection.BOETaskElements.Add(order4);
            UserTaskElementCollection.BOETaskElements.Add(order5);
            #endregion


            FullBoe boeObject = new FullBoe()
            {
                Id = 1
            };

            this._TravelDTOLoader.Setup(x => x.SaveTravels(It.IsAny<ICollection<TravelDTO>>()));
            this._retriever.Setup(x => x.GetTravelCollectionByBoeID(boeObject.Id, It.IsAny<bool>())).Returns(BoeTaskElementCollection);
            this._TravelDTOLoader.Setup(x => x.GetByBoeIds(It.IsAny<ICollection<int>>(), true)).Returns(BoeTaskElementCollection);

            this._travelControllerLogic.ReOrderTaskElementOrder(boeObject, UserTaskElementCollection);
            this._TravelDTOLoader.Verify(x => x.SaveTravels(It.IsAny<ICollection<TravelDTO>>()), Times.Once());
            //make sure the boe taskelements are updated correctly. 
            for (int x = 0; x < UpdatedTaskElementCollection.Count; x++)
            {
                Assert.IsTrue(UpdatedTaskElementCollection.ElementAt(x).BOETaskElementOrder == boeObject.Travels.ElementAt(x).BOETaskElementOrder);
                Assert.IsTrue(UpdatedTaskElementCollection.ElementAt(x).Updateable == boeObject.Travels.ElementAt(x).Updateable);
            }

        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ReorderTravelTaskElements_TestNull()
        {
            #region set up boetask elements
            TravelDTO task1 = new TravelDTO()
            {
                TaskTitle = "title1",
                BOETaskElementOrder = 0,
                Id = 1
            };
            TravelDTO task2 = new TravelDTO()
            {
                TaskTitle = "title2",
                BOETaskElementOrder = 0,
                Id = 2
            };
            TravelDTO task3 = new TravelDTO()
            {
                TaskTitle = "title3",
                BOETaskElementOrder = 0,
                Id = 3
            };
            TravelDTO task4 = new TravelDTO()
            {
                TaskTitle = "title4",
                BOETaskElementOrder = 0,
                Id = 4
            };
            TravelDTO task5 = new TravelDTO()
            {
                TaskTitle = "title5",
                BOETaskElementOrder = 0,
                Id = 5
            };

            //set up boe collection that exist in our DB

            ICollection<TravelDTO> BoeTaskElementCollection = new Collection<TravelDTO>();
            BoeTaskElementCollection.Add(task1);
            BoeTaskElementCollection.Add(task2);
            BoeTaskElementCollection.Add(task3);
            BoeTaskElementCollection.Add(task4);
            BoeTaskElementCollection.Add(task5);
            #endregion
            #region setup updated task
            TravelDTO utask1 = new TravelDTO()
            {
                TaskTitle = "title1",
                BOETaskElementOrder = 4,
                Updateable = UpdateType.Upsert,
                Id = 1
            };
            TravelDTO utask2 = new TravelDTO()
            {
                TaskTitle = "title2",
                BOETaskElementOrder = 3,
                Updateable = UpdateType.Upsert,
                Id = 2
            };
            TravelDTO utask3 = new TravelDTO()
            {
                TaskTitle = "title3",
                BOETaskElementOrder = 2,
                Updateable = UpdateType.Upsert,
                Id = 3
            };
            TravelDTO utask4 = new TravelDTO()
            {
                TaskTitle = "title4",
                BOETaskElementOrder = 1,
                Updateable = UpdateType.Upsert,
                Id = 4
            };
            TravelDTO utask5 = new TravelDTO()
            {
                TaskTitle = "title5",
                BOETaskElementOrder = 0,
                Id = 5,
                Updateable = UpdateType.Upsert
            };

            ICollection<TravelDTO> UpdatedTaskElementCollection = new Collection<TravelDTO>();

            UpdatedTaskElementCollection.Add(utask1);
            UpdatedTaskElementCollection.Add(utask2);
            UpdatedTaskElementCollection.Add(utask3);
            UpdatedTaskElementCollection.Add(utask4);
            UpdatedTaskElementCollection.Add(utask5);
            #endregion
            #region order from user
            TaskElementOrder order1 = new TaskElementOrder()
            {
                TaskID = task1.Id,
                ListOrder = 4
            };
            TaskElementOrder order2 = new TaskElementOrder()
            {
                TaskID = task2.Id,
                ListOrder = 3
            };
            TaskElementOrder order3 = new TaskElementOrder()
            {
                TaskID = task3.Id,
                ListOrder = 2
            };
            TaskElementOrder order4 = new TaskElementOrder()
            {
                TaskID = task4.Id,
                ListOrder = 1
            };
            TaskElementOrder order5 = new TaskElementOrder()
            {
                TaskID = task5.Id,
                ListOrder = 0
            };

            //set up the user input.
            TaskElementOrderCollection UserTaskElementCollection = new TaskElementOrderCollection();
            UserTaskElementCollection.BOETaskElements = new Collection<TaskElementOrder>();
            UserTaskElementCollection.BOETaskElements.Add(order1);
            UserTaskElementCollection.BOETaskElements.Add(order2);
            UserTaskElementCollection.BOETaskElements.Add(order3);
            UserTaskElementCollection.BOETaskElements.Add(order4);
            UserTaskElementCollection.BOETaskElements.Add(order5);
            #endregion


            FullBoe boeObject = null;

            this._TravelDTOLoader.Setup(x => x.SaveTravels(It.IsAny<ICollection<TravelDTO>>()));

            this._travelControllerLogic.ReOrderTaskElementOrder(boeObject, UserTaskElementCollection);
         

        }

        /// <summary>
        /// Verifies DuplicateOdcTaskInBoe will create the appropriate number of new Travel tasks.
        /// </summary>
        [TestMethod]
        public void DuplicateTravelTaskInBoeTest()
        {
            BoeDTO boeDTO = new BoeDTO()
            {
                Id = 1,
                WorkspaceID = 1
            };

            FullBoe boe = new FullBoe(boeDTO);

            //Setup 3 Travel tasks to be duplicated
            TravelTripType travelTrip = new TravelTripType 
            { 
                TravelTripID = 1, 
                TripDate = Convert.ToDateTime("07/01/2011"), 
                BoeID = boe.Id, 
                GroupID = 1, 
                NumOfDays = 2, 
                NumOfIntervals = 0, 
                NumOfOccurences = 0, 
                NumOfPeople = 4, 
                NumOfTrips = 2, 
                PerfOrgID = 1, 
                Segment = SegmentType.DS, 
                SystemTripID = 1 
            };
            TravelTripType travelTrip2 = new TravelTripType 
            { 
                TravelTripID = 2, 
                TripDate = Convert.ToDateTime("09/01/2011"), 
                BoeID = boe.Id, 
                GroupID = 2, 
                NumOfDays = 2, 
                NumOfIntervals = 0, 
                NumOfOccurences = 0, 
                NumOfPeople = 4, 
                NumOfTrips = 2, 
                PerfOrgID = 1, 
                Segment = SegmentType.SSC, 
                SystemTripID = 1 
            };
            TravelDTO travel = new TravelDTO 
            { 
                Id = 1, 
                TaskTitle = "MOCKTRAVEL", 
                TravelTrips = new Collection<TravelTripType> 
                { 
                    travelTrip, 
                    travelTrip2
                }, 
                BoeID = boe.Id, 
                StartDate = Convert.ToDateTime("07/01/2011"), 
                EndDate = Convert.ToDateTime("12/01/2011") 
            };
            TravelDTO travel2 = new TravelDTO 
            { 
                Id = 2, 
                TaskTitle = "MOCKTRAVEL2", 
                TravelTrips = new Collection<TravelTripType> 
                { 
                    travelTrip, 
                    travelTrip2 
                }, 
                BoeID = boe.Id, 
                StartDate = Convert.ToDateTime("07/01/2011"), 
                EndDate = Convert.ToDateTime("12/01/2011") 
            };
            TravelDTO travel3 = new TravelDTO 
            { 
                Id = 3, 
                TaskTitle = "MOCKTRAVEL3", 
                TravelTrips = new Collection<TravelTripType> 
                { 
                    travelTrip, 
                    travelTrip2 
                }, 
                BoeID = boe.Id, 
                StartDate = Convert.ToDateTime("07/01/2011"), 
                EndDate = Convert.ToDateTime("12/01/2011") 
            };
            Collection<TravelDTO> travelCollection = new Collection<TravelDTO>();
            travelCollection.Add(travel);
            travelCollection.Add(travel2);
            travelCollection.Add(travel3);

            this._retriever.Setup(x => x.GetTravelCollectionByBoeID(boeDTO.Id, false)).Returns(travelCollection);

            //Request 5 dups of task 1, 0 of task 2, 2 of task 3 for total of 7 new tasks
            Dictionary<int, int> dupRequest = new Dictionary<int, int> { { 1, 5 }, { 2, 0 }, { 3, 2 } };

            //ACT
            this._travelControllerLogic.DuplicateTravelTaskElements(dupRequest, boe);

            //ASSERT
            this._TravelDTOLoader.Verify(x => x.SaveTravels(It.IsAny<ICollection<TravelDTO>>()), Times.Exactly(7));
        }

        /// <summary>
        /// Verifies DuplicateTravelTaskElements will throw null exception when null duplicateRequest is passed
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DuplicateTravelTaskElements_NullTest()
        {
            FullBoe boeObject = new FullBoe();

            this._travelControllerLogic.DuplicateTravelTaskElements(null, boeObject);
        }
    }
}
