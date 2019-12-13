// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2017 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.Business.DateChange
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text;
    using Dtos;
    using GenBOE.ActionLogic.ControllerLogic;
    using GenBOE.ActionLogic.ModelView.Workspace;
    using GenBOE.Business.Common.AdjustDate;
    using GenBOE.Business.Common.Email;
    using GenBOE.DataBridge.Common;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Objects;
    using IES.Common;
    using IES.Common.classes;
    using Microsoft.Practices.Unity;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    /// <summary>
    /// Date Adjuster test class.
    /// </summary>
    [TestClass]
    public class DateAdjusterTest
    {
        private DateAdjuster sut;
        private Mock<IWorkspaceDTODataLoader> workspaceLoader;
        private Mock<IClinDTODataLoader> clinLoader;
        private Mock<IBoeDTODataLoader> boeLoader;
        private Mock<IBoeTaskElementDTODataLoader> taskLoader;
        
        private Mock<IRetriever> retriever;
        private Mock<IFullObjectFactory> fullObjectFactory;
        private Mock<ICommonDataMapper> commonDataMapper;
        private Mock<IPermissionsDTODataLoader> permissionsLoader;
        private Mock<IUserDTODataLoader> userLoader;
        private Mock<ITravelDTODataLoader> travelLoader;
        private Mock<IBoeEmailer> emailer;
        private DateAdjusterTestCases testData;
        
        /// <summary>
        /// Setups this instance.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            workspaceLoader = new Mock<IWorkspaceDTODataLoader>();
            clinLoader = new Mock<IClinDTODataLoader>();
            boeLoader = new Mock<IBoeDTODataLoader>();
            taskLoader = new Mock<IBoeTaskElementDTODataLoader>();
            userLoader = new Mock<IUserDTODataLoader>();
            travelLoader = new Mock<ITravelDTODataLoader>();
            emailer = new Mock<IBoeEmailer>();

            UserDTO author = new UserDTO() { DisplayName = "Tester", EmailAddress = "tester@lmco.com", UserID = 1 };
            userLoader.Setup(u => u.GetUserForActiveUser()).Returns(author);

            retriever = new Mock<IRetriever>();
            fullObjectFactory = new Mock<IFullObjectFactory>();
            commonDataMapper = new Mock<ICommonDataMapper>();
            permissionsLoader = new Mock<IPermissionsDTODataLoader>();

            GenBOEUnityContainer.Container.RegisterInstance(typeof(IWorkspaceDTODataLoader), workspaceLoader.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IClinDTODataLoader), clinLoader.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IBoeDTODataLoader), boeLoader.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IBoeTaskElementDTODataLoader), taskLoader.Object);

            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), fullObjectFactory.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), commonDataMapper.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), permissionsLoader.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IUserDTODataLoader), userLoader.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(ITravelDTODataLoader), travelLoader.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IBoeEmailer), emailer.Object);
            testData = new DateAdjusterTestCases(retriever);
        }

        /// <summary>
        /// Run all of the date adjustment test cases
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals")]
        [TestMethod]
        public void RunDateChangeTestCases()
        {
            // Use callbacks to store all of the saved objects
            WorkspaceDTO savedWorkspace;
            ICollection<ClinDTO> savedClinCollection;
            ClinDTO savedClin;
            ICollection<BoeDTO> savedBoeCollection;
            BoeDTO savedBoe;
            ICollection<BoeTaskElementDTO> savedTaskCollection;
            ICollection<TravelDTO> savedTravelCollection;
            BoeTaskElementDTO savedTask;
            ICollection<ResourceTypeDto> savedResourceCollection;
            
            this.workspaceLoader.Setup(x => x.SaveWorkspaceSettings(It.IsAny<int>(), It.IsAny<WorkspaceDTO>())).Callback<int, WorkspaceDTO>((i, w) => { savedWorkspace = w; });
            this.clinLoader.Setup(x => x.Save(It.IsAny<ICollection<ClinDTO>>())).Callback<ICollection<ClinDTO>>(c => { savedClinCollection = c; });
            this.clinLoader.Setup(x => x.Save(It.IsAny<ClinDTO>())).Callback<ClinDTO>(c => { savedClin = c; });
            this.boeLoader.Setup(x => x.Save(It.IsAny<ICollection<BoeDTO>>())).Callback<ICollection<BoeDTO>>(b => { savedBoeCollection = b; });
            this.boeLoader.Setup(x => x.Save(It.IsAny<BoeDTO>())).Callback<BoeDTO>(b => { savedBoe = b; });
            this.taskLoader.Setup(x => x.Save(It.IsAny<ICollection<BoeTaskElementDTO>>())).Callback<ICollection<BoeTaskElementDTO>>(t => { savedTaskCollection = t; });
            this.taskLoader.Setup(x => x.BulkSave(It.IsAny<ICollection<BoeTaskElementDTO>>())).Callback<ICollection<BoeTaskElementDTO>>(t => { savedTaskCollection = t; savedResourceCollection = t.SelectMany(te => te.taskElementLabors).ToList(); });
            this.taskLoader.Setup(x => x.Save(It.IsAny<BoeTaskElementDTO>())).Callback<BoeTaskElementDTO>(t => { savedTask = t; });
            this.travelLoader.Setup(x => x.SaveTravels(It.IsAny<ICollection<TravelDTO>>())).Callback<ICollection<TravelDTO>>(t => { savedTravelCollection = t; });
            ICollection<FullWorkspace> workspaces = testData.getStartingWorkspaces();
            this.fullObjectFactory.Setup(x => x.CreateFullWorkspace(It.IsAny<int>())).Returns(workspaces.First());

            sut = new DateAdjuster(true);

            // get the test cases
            Collection<AdjustDateTestCase> tests = testData.getTestCases();
            
            // the error collection
            List<string> errors = new List<string>();

            foreach (AdjustDateTestCase test in tests)
            {
                // reset the saved data
                savedWorkspace = new WorkspaceDTO();
                savedClinCollection = new List<ClinDTO>();
                savedClin = new ClinDTO();
                savedBoeCollection = new List<BoeDTO>();
                savedBoe = new BoeDTO();
                savedTaskCollection = new List<BoeTaskElementDTO>();
                savedTravelCollection = new List<TravelDTO>();
                savedTask = new BoeTaskElementDTO();
                savedResourceCollection = new List<ResourceTypeDto>();
                
                // get the workspace from scratch
                workspaces = testData.getStartingWorkspaces();

                // assign the workspace based on the current test
                FullWorkspace workspace = workspaces.First();
                if (test.useWorkspace2)
                    workspace = workspaces.ElementAt<FullWorkspace>(1);

                // if this is a workspace shift
                if (test.DateShiftLevel == Level.Workspace)
                {
                    // create the collection of clin dates
                    Collection<ClinDateAdjustment> clinDates = CreateClinDateAdjustment(workspace, test);

                    // test
                    if (test.TestFromMSTControllerLogic)
                    {
                        WorkspaceControllerLogicMST mstLogic = new WorkspaceControllerLogicMST(workspaceLoader.Object, userLoader.Object, null, null, null, null, retriever.Object, fullObjectFactory.Object,
                            commonDataMapper.Object, permissionsLoader.Object, null, null, null, null, null, null, null, null, null, null);
                        WorkspaceUpdateClinDatesModelView mv = new WorkspaceUpdateClinDatesModelView()
                        {
                            NewEndDate = workspace.ContractEndDate.AddMonths(test.EndDateShift).ToString(),
                            NewStartDate = workspace.ContractStartDate.AddMonths(test.StartDateShift).ToString(),
                            SendEmailForBoeUpdates = true
                        };
                        mstLogic.PerformDateShift(workspace, mv);
                    }
                    else
                    {
                        sut.AdjustWorkspaceDate(workspace, workspace.ContractStartDate.AddMonths(test.StartDateShift), workspace.ContractEndDate.AddMonths(test.EndDateShift), clinDates, test.FlowdownType, test.DiscreteType);
                    }

                    // verify each object has the correct date and the number of objects saved is correct
                    errors.Add(TestAreEqual(test.ExpectedDateRange[Element.Workspace].Item1.Value.ToString("M/yy"), savedWorkspace.ContractStartDate.ToString("M/yy"), test.TestName + ", workspace start date."));
                    errors.Add(TestAreEqual(test.ExpectedDateRange[Element.Workspace].Item2.Value.ToString("M/yy"), savedWorkspace.ContractEndDate.ToString("M/yy"), test.TestName + ", workspace end date."));

                    // automatic and manual continue changing dates
                    if (test.FlowdownType == DateAdjustFlowdownType.Automatic || test.FlowdownType == DateAdjustFlowdownType.Manual || test.TestFromMSTControllerLogic) 
                    {
                        errors.Add(TestAreEqual(3, savedClinCollection.Count, test.TestName + ", clins saved."));
                        errors.Add(TestAreEqual(test.ExpectedDateRange[Element.Clin1].Item1.HasValue ? test.ExpectedDateRange[Element.Clin1].Item1.Value.ToString("M/yy") : null, savedClinCollection.ElementAt<ClinDTO>(0).StartDate.HasValue ? savedClinCollection.ElementAt<ClinDTO>(0).StartDate.Value.ToString("M/yy") : null, test.TestName + " clin1 start date"));
                        errors.Add(TestAreEqual(test.ExpectedDateRange[Element.Clin1].Item2.HasValue ? test.ExpectedDateRange[Element.Clin1].Item2.Value.ToString("M/yy") : null, savedClinCollection.ElementAt<ClinDTO>(0).EndDate.HasValue ? savedClinCollection.ElementAt<ClinDTO>(0).EndDate.Value.ToString("M/yy") : null, test.TestName + " clin1 end date"));
                        errors.Add(TestAreEqual(test.ExpectedDateRange[Element.Clin2].Item1.HasValue ? test.ExpectedDateRange[Element.Clin2].Item1.Value.ToString("M/yy") : null, savedClinCollection.ElementAt<ClinDTO>(1).StartDate.HasValue ? savedClinCollection.ElementAt<ClinDTO>(1).StartDate.Value.ToString("M/yy") : null, test.TestName + " Clin2 start date"));
                        errors.Add(TestAreEqual(test.ExpectedDateRange[Element.Clin2].Item2.HasValue ? test.ExpectedDateRange[Element.Clin2].Item2.Value.ToString("M/yy") : null, savedClinCollection.ElementAt<ClinDTO>(1).EndDate.HasValue ? savedClinCollection.ElementAt<ClinDTO>(1).EndDate.Value.ToString("M/yy") : null, test.TestName + " Clin2 end date"));
                        errors.Add(TestAreEqual(test.ExpectedDateRange[Element.Clin3].Item1.HasValue ? test.ExpectedDateRange[Element.Clin3].Item1.Value.ToString("M/yy") : null, savedClinCollection.ElementAt<ClinDTO>(2).StartDate.HasValue ? savedClinCollection.ElementAt<ClinDTO>(2).StartDate.Value.ToString("M/yy") : null, test.TestName + " Clin3 start date"));
                        errors.Add(TestAreEqual(test.ExpectedDateRange[Element.Clin3].Item2.HasValue ? test.ExpectedDateRange[Element.Clin3].Item2.Value.ToString("M/yy") : null, savedClinCollection.ElementAt<ClinDTO>(2).EndDate.HasValue ? savedClinCollection.ElementAt<ClinDTO>(2).EndDate.Value.ToString("M/yy") : null, test.TestName + " Clin3 end date"));
                        errors.Add(TestAreEqual(4, savedBoeCollection.Count, test.TestName + ", boes saved."));
                        errors.Add(TestAreEqual(test.ExpectedDateRange[Element.Boe1].Item1.Value.ToString("M/yy"), savedBoeCollection.ElementAt<BoeDTO>(0).StartDate.ToString("M/yy"), test.TestName + " Boe1 start date"));
                        errors.Add(TestAreEqual(test.ExpectedDateRange[Element.Boe1].Item2.Value.ToString("M/yy"), savedBoeCollection.ElementAt<BoeDTO>(0).EndDate.ToString("M/yy"), test.TestName + " Boe1 end date"));
                        errors.Add(TestAreEqual(test.ExpectedDateRange[Element.Boe2].Item1.Value.ToString("M/yy"), savedBoeCollection.ElementAt<BoeDTO>(1).StartDate.ToString("M/yy"), test.TestName + " Boe2 start date"));
                        errors.Add(TestAreEqual(test.ExpectedDateRange[Element.Boe2].Item2.Value.ToString("M/yy"), savedBoeCollection.ElementAt<BoeDTO>(1).EndDate.ToString("M/yy"), test.TestName + " Boe2 end date"));
                        errors.Add(TestAreEqual(test.ExpectedDateRange[Element.Boe3].Item1.Value.ToString("M/yy"), savedBoeCollection.ElementAt<BoeDTO>(2).StartDate.ToString("M/yy"), test.TestName + " Boe3 start date"));
                        errors.Add(TestAreEqual(test.ExpectedDateRange[Element.Boe3].Item2.Value.ToString("M/yy"), savedBoeCollection.ElementAt<BoeDTO>(2).EndDate.ToString("M/yy"), test.TestName + " Boe3 end date"));
                        errors.Add(TestAreEqual(test.ExpectedDateRange[Element.BoeNC].Item1.Value.ToString("M/yy"), savedBoeCollection.ElementAt<BoeDTO>(3).StartDate.ToString("M/yy"), test.TestName + " Boe No Clin start date"));
                        errors.Add(TestAreEqual(test.ExpectedDateRange[Element.BoeNC].Item2.Value.ToString("M/yy"), savedBoeCollection.ElementAt<BoeDTO>(3).EndDate.ToString("M/yy"), test.TestName + " Boe No Clin end date"));
                        errors.Add(TestAreEqual(4, savedTaskCollection.Count, test.TestName + " tasks saved."));
                        errors.Add(TestAreEqual(test.ExpectedDateRange[Element.Task1].Item1.Value.ToString("M/yy"), savedTaskCollection.ElementAt<BoeTaskElementDTO>(0).StartDate.Value.ToString("M/yy"), test.TestName + " Task1 start date"));
                        errors.Add(TestAreEqual(test.ExpectedDateRange[Element.Task1].Item2.Value.ToString("M/yy"), savedTaskCollection.ElementAt<BoeTaskElementDTO>(0).EndDate.Value.ToString("M/yy"), test.TestName + " Task1 end date"));
                        errors.Add(TestAreEqual(test.ExpectedDateRange[Element.Task2].Item1.Value.ToString("M/yy"), savedTaskCollection.ElementAt<BoeTaskElementDTO>(1).StartDate.Value.ToString("M/yy"), test.TestName + " Task2 start date"));
                        errors.Add(TestAreEqual(test.ExpectedDateRange[Element.Task2].Item2.Value.ToString("M/yy"), savedTaskCollection.ElementAt<BoeTaskElementDTO>(1).EndDate.Value.ToString("M/yy"), test.TestName + " Task2 end date"));
                        errors.Add(TestAreEqual(test.ExpectedDateRange[Element.Task3].Item1.Value.ToString("M/yy"), savedTaskCollection.ElementAt<BoeTaskElementDTO>(2).StartDate.Value.ToString("M/yy"), test.TestName + " Task3 start date"));
                        errors.Add(TestAreEqual(test.ExpectedDateRange[Element.Task3].Item2.Value.ToString("M/yy"), savedTaskCollection.ElementAt<BoeTaskElementDTO>(2).EndDate.Value.ToString("M/yy"), test.TestName + " Task3 end date"));
                        errors.Add(TestAreEqual(test.ExpectedDateRange[Element.TaskNC].Item1.Value.ToString("M/yy"), savedTaskCollection.ElementAt<BoeTaskElementDTO>(3).StartDate.Value.ToString("M/yy"), test.TestName + " Task No Clin start date"));
                        errors.Add(TestAreEqual(test.ExpectedDateRange[Element.TaskNC].Item2.Value.ToString("M/yy"), savedTaskCollection.ElementAt<BoeTaskElementDTO>(3).EndDate.Value.ToString("M/yy"), test.TestName + " Task No Clin end date"));

                        errors.Add(TestAreEqual(test.ExpectedDateRange[Element.Travel1].Item1.Value.ToString("M/yy"), savedTravelCollection.ElementAt<TravelDTO>(0).StartDate.Value.ToString("M/yy"), test.TestName + " Travel1 start date"));
                        errors.Add(TestAreEqual(test.ExpectedDateRange[Element.Travel1].Item2.Value.ToString("M/yy"), savedTravelCollection.ElementAt<TravelDTO>(0).EndDate.Value.ToString("M/yy"), test.TestName + " Travel1 end date"));
                        errors.Add(TestAreEqual(test.ExpectedDateRange[Element.TravelTrip1].Item1.Value.ToString("M/yy"), savedTravelCollection.ElementAt<TravelDTO>(0).MSTTravelTrips.First().TripDate.ToString("M/yy"), test.TestName + " Travel1's First Trip date"));
                        errors.Add(TestAreEqual(test.ExpectedDateRange[Element.Travel2].Item1.Value.ToString("M/yy"), savedTravelCollection.ElementAt<TravelDTO>(1).StartDate.Value.ToString("M/yy"), test.TestName + " Travel2 start date"));
                        errors.Add(TestAreEqual(test.ExpectedDateRange[Element.Travel2].Item2.Value.ToString("M/yy"), savedTravelCollection.ElementAt<TravelDTO>(1).EndDate.Value.ToString("M/yy"), test.TestName + " Travel2 end date"));
                        errors.Add(TestAreEqual(test.ExpectedDateRange[Element.Travel3].Item1.Value.ToString("M/yy"), savedTravelCollection.ElementAt<TravelDTO>(2).StartDate.Value.ToString("M/yy"), test.TestName + " Travel3 start date"));
                        errors.Add(TestAreEqual(test.ExpectedDateRange[Element.Travel3].Item2.Value.ToString("M/yy"), savedTravelCollection.ElementAt<TravelDTO>(2).EndDate.Value.ToString("M/yy"), test.TestName + " Travel3 end date"));
                        errors.Add(TestAreEqual(test.ExpectedDateRange[Element.TravelNC].Item1.Value.ToString("M/yy"), savedTravelCollection.ElementAt<TravelDTO>(3).StartDate.Value.ToString("M/yy"), test.TestName + " Travel No Clin start date"));
                        errors.Add(TestAreEqual(test.ExpectedDateRange[Element.TravelNC].Item2.Value.ToString("M/yy"), savedTravelCollection.ElementAt<TravelDTO>(3).EndDate.Value.ToString("M/yy"), test.TestName + " Travel No Clin end date"));

                        // only automatic changes resource and spread dates
                        if (test.FlowdownType == DateAdjustFlowdownType.Automatic || test.TestFromMSTControllerLogic) 
                        {
                            errors.Add(TestAreEqual(4, savedResourceCollection.Count, test.TestName + " resources saved."));
                            errors.Add(TestAreEqual(test.ExpectedDateRange[Element.Resource1].Item1.Value.ToString("M/yy"), savedResourceCollection.ElementAt<ResourceTypeDto>(0).StartDate.Value.ToString("M/yy"), test.TestName + " Resource1 start date"));
                            errors.Add(TestAreEqual(test.ExpectedDateRange[Element.Resource1].Item2.Value.ToString("M/yy"), savedResourceCollection.ElementAt<ResourceTypeDto>(0).EndDate.Value.ToString("M/yy"), test.TestName + " Resource1 end date"));
                            errors.Add(TestAreEqual(test.ExpectedDateRange[Element.Resource2].Item1.Value.ToString("M/yy"), savedResourceCollection.ElementAt<ResourceTypeDto>(1).StartDate.Value.ToString("M/yy"), test.TestName + " Resource2 start date"));
                            errors.Add(TestAreEqual(test.ExpectedDateRange[Element.Resource2].Item2.Value.ToString("M/yy"), savedResourceCollection.ElementAt<ResourceTypeDto>(1).EndDate.Value.ToString("M/yy"), test.TestName + " Resource2 end date"));
                            errors.Add(TestAreEqual(test.ExpectedDateRange[Element.Resource3].Item1.Value.ToString("M/yy"), savedResourceCollection.ElementAt<ResourceTypeDto>(2).StartDate.Value.ToString("M/yy"), test.TestName + " Resource3 start date"));
                            errors.Add(TestAreEqual(test.ExpectedDateRange[Element.Resource3].Item2.Value.ToString("M/yy"), savedResourceCollection.ElementAt<ResourceTypeDto>(2).EndDate.Value.ToString("M/yy"), test.TestName + " Resource3 end date"));
                            errors.Add(TestAreEqual(test.ExpectedDateRange[Element.ResourceNC].Item1.Value.ToString("M/yy"), savedResourceCollection.ElementAt<ResourceTypeDto>(3).StartDate.Value.ToString("M/yy"), test.TestName + " Resource No Clin start date"));
                            errors.Add(TestAreEqual(test.ExpectedDateRange[Element.ResourceNC].Item2.Value.ToString("M/yy"), savedResourceCollection.ElementAt<ResourceTypeDto>(3).EndDate.Value.ToString("M/yy"), test.TestName + " Resource No Clin end date"));
                        }
                        else
                        {
                            errors.Add(TestAreEqual(0, savedResourceCollection.Count, test.TestName + " no resources should have been saved."));
                        }
                    }
                    else
                    {
                        errors.Add(TestAreEqual(0, savedClinCollection.Count, test.TestName + ", no clins should have been saved."));
                        errors.Add(TestAreEqual(0, savedBoeCollection.Count, test.TestName + ", no boes should have been saved."));
                        errors.Add(TestAreEqual(0, savedTaskCollection.Count, test.TestName + ", no tasks should have been saved."));
                        errors.Add(TestAreEqual(0, savedResourceCollection.Count, test.TestName + " no resources should have been saved."));
                    }
                }

                // if this is a clin shift
                if (test.DateShiftLevel == Level.CLIN)
                {
                    // if this is a normal test
                    if (!test.useWorkspace2)
                    {
                        // get the current objects based on the test's ObjectId field.
                        FullClin clin = workspace.Clins.Where(c => c.Id == test.ObjectId).First();

                        Element clinUnderTest = (Element)Enum.Parse(typeof(Element), "Clin" + test.ObjectId);
                        Element boeUnderTest = (Element)Enum.Parse(typeof(Element), "Boe" + test.ObjectId);
                        Element taskUnderTest = (Element)Enum.Parse(typeof(Element), "Task" + test.ObjectId);
                        Element resourceUnderTest = (Element)Enum.Parse(typeof(Element), "Resource" + test.ObjectId);

                        // date shifts of int.MaxValue represent a null
                        DateTime? startDate = test.StartDateShift == int.MaxValue ? (DateTime?)null
                            : clin.StartDate.HasValue ? clin.StartDate.Value.AddMonths(test.StartDateShift) : workspace.ContractStartDate.AddMonths(test.StartDateShift);
                        DateTime? endDate = test.EndDateShift == int.MaxValue ? (DateTime?)null
                            : clin.EndDate.HasValue ? clin.EndDate.Value.AddMonths(test.EndDateShift) : workspace.ContractEndDate.AddMonths(test.EndDateShift);

                        // test
                        sut.AdjustClinDate(workspace, clin, startDate, endDate, test.FlowdownType, test.DiscreteType);

                        // verify each object has the correct date and the number of objects saved is correct
                        errors.Add(TestAreEqual(test.ExpectedDateRange[clinUnderTest].Item1.HasValue ? test.ExpectedDateRange[clinUnderTest].Item1.Value.ToString("M/yy") : null, savedClin.StartDate.HasValue ? savedClin.StartDate.Value.ToString("M/yy") : null, test.TestName + " clin1 start date"));
                        errors.Add(TestAreEqual(test.ExpectedDateRange[clinUnderTest].Item2.HasValue ? test.ExpectedDateRange[clinUnderTest].Item2.Value.ToString("M/yy") : null, savedClin.EndDate.HasValue ? savedClin.EndDate.Value.ToString("M/yy") : null, test.TestName + " clin1 end date"));

                        // automatic and manual continue changing dates
                        if (test.FlowdownType == DateAdjustFlowdownType.Automatic || test.FlowdownType == DateAdjustFlowdownType.Manual)
                        {
                            errors.Add(TestAreEqual(1, savedBoeCollection.Count, test.TestName + ", boes saved."));
                            errors.Add(TestAreEqual(test.ExpectedDateRange[boeUnderTest].Item1.Value.ToString("M/yy"), savedBoeCollection.ElementAt<BoeDTO>(0).StartDate.ToString("M/yy"), test.TestName + ", boe start date."));
                            errors.Add(TestAreEqual(test.ExpectedDateRange[boeUnderTest].Item2.Value.ToString("M/yy"), savedBoeCollection.ElementAt<BoeDTO>(0).EndDate.ToString("M/yy"), test.TestName + ", boe end date."));
                            errors.Add(TestAreEqual(1, savedTaskCollection.Count, test.TestName + " tasks saved."));
                            errors.Add(TestAreEqual(test.ExpectedDateRange[taskUnderTest].Item1.Value.ToString("M/yy"), savedTaskCollection.ElementAt<BoeTaskElementDTO>(0).StartDate.Value.ToString("M/yy"), test.TestName + " task start date"));
                            errors.Add(TestAreEqual(test.ExpectedDateRange[taskUnderTest].Item2.Value.ToString("M/yy"), savedTaskCollection.ElementAt<BoeTaskElementDTO>(0).EndDate.Value.ToString("M/yy"), test.TestName + " task end date"));

                            // only automatic changes resource and spread dates
                            if (test.FlowdownType == DateAdjustFlowdownType.Automatic)
                            {
                                errors.Add(TestAreEqual(1, savedResourceCollection.Count, test.TestName + " resources saved."));
                                errors.Add(TestAreEqual(test.ExpectedDateRange[resourceUnderTest].Item1.Value.ToString("M/yy"), savedResourceCollection.ElementAt<ResourceTypeDto>(0).StartDate.Value.ToString("M/yy"), test.TestName + " resource start date"));
                                errors.Add(TestAreEqual(test.ExpectedDateRange[resourceUnderTest].Item2.Value.ToString("M/yy"), savedResourceCollection.ElementAt<ResourceTypeDto>(0).EndDate.Value.ToString("M/yy"), test.TestName + " resource end date"));
                            }
                            else
                            {
                                errors.Add(TestAreEqual(0, savedResourceCollection.Count, test.TestName + " no resources should have been saved."));
                            }
                        }
                        else
                        {
                            errors.Add(TestAreEqual(0, savedBoeCollection.Count, test.TestName + ", no boes should have been saved."));
                            errors.Add(TestAreEqual(0, savedTaskCollection.Count, test.TestName + " no tasks should have been saved."));
                            errors.Add(TestAreEqual(0, savedResourceCollection.Count, test.TestName + " no resources should have been saved."));
                        }
                    }
                    else
                    {
                        // if we hit one of the extra cases, we know to use Clin 4
                        FullClin clin = workspace.Clins.Where(c => c.Id == 4).First();
                        DateTime? startDate = clin.StartDate.Value.AddMonths(test.StartDateShift);
                        DateTime? endDate = clin.EndDate.Value.AddMonths(test.EndDateShift);

                        // Assign each of the boe dates based on the override dates in the test case
                        foreach (KeyValuePair<int, Tuple<DateTime, DateTime>> kvp in test.BoeDateOverride)
                        {
                            FullBoe boe = clin.Boes.Where(x => x.Id == kvp.Key).First();
                            boe.StartDate = kvp.Value.Item1;
                            boe.EndDate = kvp.Value.Item2;
                        }

                        // test
                        sut.AdjustClinDate(workspace, clin, startDate, endDate, test.FlowdownType, test.DiscreteType);

                        // verify each object has the correct date and the number of objects saved is correct
                        errors.Add(TestAreEqual(test.ExpectedDateRange[Element.Clin4].Item1.HasValue ? test.ExpectedDateRange[Element.Clin4].Item1.Value.ToString("M/yy") : null, savedClin.StartDate.HasValue ? savedClin.StartDate.Value.ToString("M/yy") : null, test.TestName + " clin4 start date"));
                        errors.Add(TestAreEqual(test.ExpectedDateRange[Element.Clin4].Item2.HasValue ? test.ExpectedDateRange[Element.Clin4].Item2.Value.ToString("M/yy") : null, savedClin.EndDate.HasValue ? savedClin.EndDate.Value.ToString("M/yy") : null, test.TestName + " clin4 end date"));

                        // verify each boe
                        errors.Add(TestAreEqual(5, savedBoeCollection.Count, test.TestName + ", boes saved."));

                        if (test.ExpectedDateRange.ContainsKey(Element.Boe41))
                        {
                            errors.Add(TestAreEqual(test.ExpectedDateRange[Element.Boe41].Item1.Value.ToString("M/yy"), savedBoeCollection.ElementAt<BoeDTO>(0).StartDate.ToString("M/yy"), test.TestName + " Boe41 start date"));
                            errors.Add(TestAreEqual(test.ExpectedDateRange[Element.Boe41].Item2.Value.ToString("M/yy"), savedBoeCollection.ElementAt<BoeDTO>(0).EndDate.ToString("M/yy"), test.TestName + " Boe41 end date"));
                        }

                        if (test.ExpectedDateRange.ContainsKey(Element.Boe42))
                        {
                            errors.Add(TestAreEqual(test.ExpectedDateRange[Element.Boe42].Item1.Value.ToString("M/yy"), savedBoeCollection.ElementAt<BoeDTO>(1).StartDate.ToString("M/yy"), test.TestName + " Boe42 start date"));
                            errors.Add(TestAreEqual(test.ExpectedDateRange[Element.Boe42].Item2.Value.ToString("M/yy"), savedBoeCollection.ElementAt<BoeDTO>(1).EndDate.ToString("M/yy"), test.TestName + " Boe42 end date"));
                        }

                        if (test.ExpectedDateRange.ContainsKey(Element.Boe43))
                        {
                            errors.Add(TestAreEqual(test.ExpectedDateRange[Element.Boe43].Item1.Value.ToString("M/yy"), savedBoeCollection.ElementAt<BoeDTO>(2).StartDate.ToString("M/yy"), test.TestName + " Boe43 start date"));
                            errors.Add(TestAreEqual(test.ExpectedDateRange[Element.Boe43].Item2.Value.ToString("M/yy"), savedBoeCollection.ElementAt<BoeDTO>(2).EndDate.ToString("M/yy"), test.TestName + " Boe43 end date"));
                        }

                        if (test.ExpectedDateRange.ContainsKey(Element.Boe44))
                        {
                            errors.Add(TestAreEqual(test.ExpectedDateRange[Element.Boe44].Item1.Value.ToString("M/yy"), savedBoeCollection.ElementAt<BoeDTO>(3).StartDate.ToString("M/yy"), test.TestName + " Boe44 start date"));
                            errors.Add(TestAreEqual(test.ExpectedDateRange[Element.Boe44].Item2.Value.ToString("M/yy"), savedBoeCollection.ElementAt<BoeDTO>(3).EndDate.ToString("M/yy"), test.TestName + " Boe44 end date"));
                        }

                        if (test.ExpectedDateRange.ContainsKey(Element.Boe45))
                        {
                            errors.Add(TestAreEqual(test.ExpectedDateRange[Element.Boe45].Item1.Value.ToString("M/yy"), savedBoeCollection.ElementAt<BoeDTO>(4).StartDate.ToString("M/yy"), test.TestName + " Boe45 start date"));
                            errors.Add(TestAreEqual(test.ExpectedDateRange[Element.Boe45].Item2.Value.ToString("M/yy"), savedBoeCollection.ElementAt<BoeDTO>(4).EndDate.ToString("M/yy"), test.TestName + " Boe45 end date"));
                        }
                    }
                }

                // if this is a boe shift
                if (test.DateShiftLevel == Level.BOE)
                {
                    // get the current objects based on the test's ObjectId field.
                    FullBoe boe = workspace.Boes.Where(b => b.Id == test.ObjectId).First();
                    Element boeUnderTest = (Element)Enum.Parse(typeof(Element), "Boe" + test.ObjectId);
                    Element taskUnderTest = (Element)Enum.Parse(typeof(Element), "Task" + test.ObjectId);
                    Element resourceUnderTest = (Element)Enum.Parse(typeof(Element), "Resource" + test.ObjectId);

                    // test
                    sut.AdjustBoeDate(workspace, boe, boe.StartDate.AddMonths(test.StartDateShift), boe.EndDate.AddMonths(test.EndDateShift), test.FlowdownType, test.DiscreteType);

                    // verify each object has the correct date and the number of objects saved is correct
                    errors.Add(TestAreEqual(test.ExpectedDateRange[boeUnderTest].Item1.Value.ToString("M/yy"), savedBoe.StartDate.ToString("M/yy"), test.TestName + ", boe start date."));
                    errors.Add(TestAreEqual(test.ExpectedDateRange[boeUnderTest].Item2.Value.ToString("M/yy"), savedBoe.EndDate.ToString("M/yy"), test.TestName + ", boe end date."));

                    // automatic and manual continue changing dates
                    if (test.FlowdownType == DateAdjustFlowdownType.Automatic || test.FlowdownType == DateAdjustFlowdownType.Manual)
                    {
                        errors.Add(TestAreEqual(1, savedTaskCollection.Count, test.TestName + " tasks saved."));
                        errors.Add(TestAreEqual(test.ExpectedDateRange[taskUnderTest].Item1.Value.ToString("M/yy"), savedTaskCollection.ElementAt<BoeTaskElementDTO>(0).StartDate.Value.ToString("M/yy"), test.TestName + " task start date"));
                        errors.Add(TestAreEqual(test.ExpectedDateRange[taskUnderTest].Item2.Value.ToString("M/yy"), savedTaskCollection.ElementAt<BoeTaskElementDTO>(0).EndDate.Value.ToString("M/yy"), test.TestName + " task end date"));

                        // only automatic changes resource and spread dates
                        if (test.FlowdownType == DateAdjustFlowdownType.Automatic)
                        {
                            errors.Add(TestAreEqual(1, savedResourceCollection.Count, test.TestName + " resources saved."));
                            errors.Add(TestAreEqual(test.ExpectedDateRange[resourceUnderTest].Item1.Value.ToString("M/yy"), savedResourceCollection.ElementAt<ResourceTypeDto>(0).StartDate.Value.ToString("M/yy"), test.TestName + " resource start date"));
                            errors.Add(TestAreEqual(test.ExpectedDateRange[resourceUnderTest].Item2.Value.ToString("M/yy"), savedResourceCollection.ElementAt<ResourceTypeDto>(0).EndDate.Value.ToString("M/yy"), test.TestName + " resource end date"));
                        }
                        else
                        {
                            errors.Add(TestAreEqual(0, savedResourceCollection.Count, test.TestName + " no resources should have been saved."));
                        }
                    }
                    else
                    {
                        errors.Add(TestAreEqual(0, savedTaskCollection.Count, test.TestName + " no tasks should have been saved."));
                        errors.Add(TestAreEqual(0, savedResourceCollection.Count, test.TestName + " no resources should have been saved."));
                    }
                }

                if (test.DateShiftLevel == Level.Task)
                {
                    // get the current objects based on the test's ObjectId field.
                    BoeTaskElementDTO task = workspace.TaskElements.Where(t => t.Id == test.ObjectId).First();
                    Element taskUnderTest = (Element)Enum.Parse(typeof(Element), "Task" + test.ObjectId);
                    Element resourceUnderTest = (Element)Enum.Parse(typeof(Element), "Resource" + test.ObjectId);

                    // test
                    sut.AdjustTaskDate(workspace, task, task.StartDate.Value.AddMonths(test.StartDateShift), task.EndDate.Value.AddMonths(test.EndDateShift), test.FlowdownType, test.DiscreteType);

                    errors.Add(TestAreEqual(test.ExpectedDateRange[taskUnderTest].Item1.Value.ToString("M/yy"), savedTask.StartDate.Value.ToString("M/yy"), test.TestName + " task start date"));
                    errors.Add(TestAreEqual(test.ExpectedDateRange[taskUnderTest].Item2.Value.ToString("M/yy"), savedTask.EndDate.Value.ToString("M/yy"), test.TestName + " task end date"));

                    // only automatic changes resource and spread dates
                    if (test.FlowdownType == DateAdjustFlowdownType.Automatic)
                    {
                        errors.Add(TestAreEqual(1, savedResourceCollection.Count, test.TestName + " resources saved."));
                        errors.Add(TestAreEqual(test.ExpectedDateRange[resourceUnderTest].Item1.Value.ToString("M/yy"), savedResourceCollection.ElementAt<ResourceTypeDto>(0).StartDate.Value.ToString("M/yy"), test.TestName + " resource start date"));
                        errors.Add(TestAreEqual(test.ExpectedDateRange[resourceUnderTest].Item2.Value.ToString("M/yy"), savedResourceCollection.ElementAt<ResourceTypeDto>(0).EndDate.Value.ToString("M/yy"), test.TestName + " resource end date"));
                    }
                    else
                    {
                        errors.Add(TestAreEqual(0, savedResourceCollection.Count, test.TestName + " no resources should have been saved."));
                    }
                }

                // Test the spreads to make sure that they are all inclusive to the labor resources, correct count, etc.
                TestSpreads(errors, workspace, test);

            }

            // print out all of the errors at once.
            Assert.IsTrue(errors.Where(x => x != null).Count() == 0, " errors were found. " + PrintErrorList(errors));
        }

        /// <summary>
        /// Retrieves the adjustment descriptions
        /// </summary>
        [TestMethod]
        public void GetAdjustmentDescriptions()
        {
            Dictionary<DateAdjustmentType, string> adjustmentDescriptions = DateAdjuster.AdjustmentDescriptions;
            Assert.IsNotNull(adjustmentDescriptions);
            Assert.AreEqual(5, adjustmentDescriptions.Count);
        }

        /// <summary>
        /// Verifies the adjustment types based on start/end offsets
        /// </summary>
        [TestMethod]
        public void AdjustmentTypes()
        {
            Assert.AreEqual(DateAdjustmentType.None, DateAdjuster.GetAdjustmentType(0, 0));
            Assert.AreEqual(DateAdjustmentType.Shift, DateAdjuster.GetAdjustmentType(3, 3));
            Assert.AreEqual(DateAdjustmentType.Shift, DateAdjuster.GetAdjustmentType(-3, -3));
            Assert.AreEqual(DateAdjustmentType.Expand, DateAdjuster.GetAdjustmentType(0, 4));
            Assert.AreEqual(DateAdjustmentType.Expand, DateAdjuster.GetAdjustmentType(-2, 0));
            Assert.AreEqual(DateAdjustmentType.Expand, DateAdjuster.GetAdjustmentType(-1, 1));
            Assert.AreEqual(DateAdjustmentType.Compress, DateAdjuster.GetAdjustmentType(1, 0));
            Assert.AreEqual(DateAdjustmentType.Compress, DateAdjuster.GetAdjustmentType(1, -1));
            Assert.AreEqual(DateAdjustmentType.Compress, DateAdjuster.GetAdjustmentType(0, -2));
            Assert.AreEqual(DateAdjustmentType.ShiftAndCompress, DateAdjuster.GetAdjustmentType(2, 1));
            Assert.AreEqual(DateAdjustmentType.ShiftAndCompress, DateAdjuster.GetAdjustmentType(-1, -2));
            Assert.AreEqual(DateAdjustmentType.ShiftAndExpand, DateAdjuster.GetAdjustmentType(1, 2));
            Assert.AreEqual(DateAdjustmentType.ShiftAndExpand, DateAdjuster.GetAdjustmentType(-3, -2));
        }

        #region Exception Tests

        /// <summary>
        /// Test for null workspace exception when adjusting workspace date.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AdjustWorkspaceDate_NullWorkspace()
        {
            sut = new DateAdjuster(true);
            sut.AdjustWorkspaceDate(null, new DateTime(), new DateTime(), new List<ClinDateAdjustment>(), DateAdjustFlowdownType.NotSet, DateAdjustDiscreteType.NotSet);
        }

        /// <summary>
        /// Test for null workspace when adjusting clin date.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AdjustClinDate_NullWorkspace()
        {
            sut = new DateAdjuster(true);
            sut.AdjustClinDate(null, new FullClin(), new DateTime(), new DateTime(), DateAdjustFlowdownType.NotSet, DateAdjustDiscreteType.NotSet);
        }

        /// <summary>
        /// Test for null clin when adjusting clin date.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AdjustClinDate_NullClin()
        {
            sut = new DateAdjuster(true);
            sut.AdjustClinDate(new FullWorkspace(), null, null, null, DateAdjustFlowdownType.NotSet, DateAdjustDiscreteType.NotSet);
        }

        /// <summary>
        /// Test for null boe when adjusting boe date.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AdjustBoeDate_NullBoe()
        {
            sut = new DateAdjuster(true);
            sut.AdjustBoeDate(new FullWorkspace(), null, new DateTime(), new DateTime(), DateAdjustFlowdownType.NotSet, DateAdjustDiscreteType.NotSet);
        }

        /// <summary>
        /// Test for null task when adjusting task date.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AdjustTaskDate_NullTask()
        {
            sut = new DateAdjuster(true);
            sut.AdjustTaskDate(new FullWorkspace(), null, new DateTime(), new DateTime(), DateAdjustFlowdownType.NotSet, DateAdjustDiscreteType.NotSet);
        }

        #endregion Exception Tests

        /// <summary>
        /// Tests the resource spreads for equality.
        /// </summary>
        /// <param name="errors">The running list of errors.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="test">The current test case.</param>
        private void TestSpreads(List<string> errors, FullWorkspace  workspace, AdjustDateTestCase test)
        {
            foreach (FullBoe boe in workspace.Boes)
            {
                foreach (BoeTaskElementDTO te in boe.TaskElements)
                {
                    foreach (ResourceTypeDto labor in te.taskElementLabors)
                    {
                        Element element;
                        switch (labor.Id)
                        {
                            case 1:
                                element = Element.Resource1;
                                break;
                            case 2:
                                element = Element.Resource2;
                                break;
                            case 3:
                                element = Element.Resource3;
                                break;
                            default:
                                element = Element.ResourceNC;
                                break;
                        } 

                        Dictionary<DateTime, decimal> expectedSpreadValues;
                        if (test.ExpectedSpreadResults.TryGetValue(element, out expectedSpreadValues))
                        {
                            errors.Add(TestAreEqual(expectedSpreadValues.Count, labor.LaborSpreads.Count, test.TestName + " " + element.ToString() + " Spreads Count"));

                            foreach (KeyValuePair<DateTime, decimal> kvp in expectedSpreadValues)
                            {
                                ResourceSpreadDto spread = labor.LaborSpreads.FirstOrDefault(ls => ls.LaborSpreadDate == kvp.Key);
                                if (spread == null)
                                {
                                    errors.Add(test.TestName + " " + element.ToString() + " missing Spread Date Month " + kvp.Key.ToString());
                                }
                                else
                                {
                                    errors.Add(TestAreEqual(kvp.Value, spread.LaborSpreadValue, test.TestName + " " + element.ToString() + " Spread Value in Month " + kvp.Key.ToString()));
                                }
                            }
                        }
                        else
                        {
                            errors.Add(TestAreEqual(0, labor.LaborSpreads.Count, test.TestName + " " + element.ToString() + " Spreads Count"));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Create a collection of clin adjustment objects to use when adjusting the workspace date
        /// </summary>
        /// <param name="workspace">The workspace.</param>
        /// <param name="test">The test.</param>
        /// <returns>A collection of clin adjustment test objects for the specified test.</returns>
        private Collection<ClinDateAdjustment> CreateClinDateAdjustment(FullWorkspace workspace, AdjustDateTestCase test)
        {
            Collection<ClinDateAdjustment> clinDates = new Collection<ClinDateAdjustment>();

            clinDates.Add(new ClinDateAdjustment
            {
                ClinId = workspace.Clins.ElementAt<ClinDTO>(0).Id,
                StartDate = test.ExpectedDateRange[Element.Clin1].Item1,
                EndDate = test.ExpectedDateRange[Element.Clin1].Item2
            });

            clinDates.Add(new ClinDateAdjustment
            {
                ClinId = workspace.Clins.ElementAt<ClinDTO>(1).Id,
                StartDate = test.ExpectedDateRange[Element.Clin2].Item1,
                EndDate = test.ExpectedDateRange[Element.Clin2].Item2
            });

            clinDates.Add(new ClinDateAdjustment
            {
                ClinId = workspace.Clins.ElementAt<ClinDTO>(2).Id,
                StartDate = test.ExpectedDateRange[Element.Clin3].Item1,
                EndDate = test.ExpectedDateRange[Element.Clin3].Item2
            });

            return clinDates;
        }

        /// <summary>
        /// Non-breaking test condition.
        /// </summary>
        /// <typeparam name="T">Generic type to test for equality.</typeparam>
        /// <param name="expected">The expected value.</param>
        /// <param name="actual">The actual value.</param>
        /// <param name="message">The error message when values are inequal.</param>
        /// <returns></returns>
        private String TestAreEqual<T>(T expected, T actual, string message)
        {
            if (!EqualityComparer<T>.Default.Equals(expected, actual))
                return message + " expected " + expected.ToString() + " received " + actual.ToString() + ".";
            return null;
        }

        /// <summary>
        /// Create the list of errors compiled from all of the test cases.
        /// </summary>
        /// <param name="errors">A list of errors from all of the test cases.</param>
        /// <returns>A concatenated string of all the errors.</returns>
        private string PrintErrorList(List<string> errors)
        {
            StringBuilder toReturn = new StringBuilder();
            toReturn.AppendLine();
            foreach(string error in errors)
            {
                if (!String.IsNullOrEmpty(error))
                    toReturn.AppendLine(error);
            }

            return toReturn.ToString();
        }
    }
}
