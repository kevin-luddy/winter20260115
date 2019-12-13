using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using IES.Common;
using GenBOE.Dtos;
using GenBOE.Objects;
using Moq;

namespace GenBOE.Tests.Business.DateChange
{
    #region Utility Objects 

    /// <summary>
    /// Each element that gets read by the test
    /// </summary>
    public enum Element
    {
        Workspace,
        Clin1,
        Clin2,
        Clin3,
        Clin4,
        Boe1,
        Boe2,
        Boe3,
        BoeNC, // boe without a clin
        Boe41,
        Boe42,
        Boe43,
        Boe44,
        Boe45,
        Task1,
        Task2,
        Task3,
        TaskNC,
        Resource1,
        Resource2,
        Resource3,
        ResourceNC,
        Spread,
        Travel1,
        Travel2,
        Travel3,
        TravelNC,
        TravelTrip1
    }

    /// <summary>
    /// Represents a single test case with expected results
    /// </summary>
    public class AdjustDateTestCase
    {
        public String TestName { get; set; }
        public int StartDateShift { get; set; }
        public int EndDateShift { get; set; }
        public Level DateShiftLevel { get; set; }
        public bool useWorkspace2 { get; set; }
        public int ObjectId { get; set; }
        public DateAdjustFlowdownType FlowdownType { get; set; }
        public DateAdjustDiscreteType DiscreteType { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public Dictionary<Element, Dictionary<DateTime, decimal>> ExpectedSpreadResults { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public Dictionary<Element, Tuple<DateTime?, DateTime?>> ExpectedDateRange { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public Dictionary<int, Tuple<DateTime, DateTime>> BoeDateOverride { get; set; }
        public bool TestFromMSTControllerLogic { get; set; }
    }

    #endregion Utility Objects 

    /// <summary>
    /// Test Case class for Date Adjuster tests.
    /// </summary>
    public class DateAdjusterTestCases
    {
        #region Enums and Private Classes

        private FullClin clin1, clin2, clin3;
        private FullBoe boe1, boe2, boe3, boeNC; // NC for No Clin
        private BoeTaskElementDTO task1, task2, task3, taskNC;
        private TravelDTO travel1, travel2, travel3, travelNC;
        private ResourceTypeDto resource1, resource2, resource3, resourceNC;
        private FullWorkspace workspace1;
        private Mock<IRetriever> retriever;

        private readonly DateTime contractStartDate, contractEndDate;
        private readonly DateTime? clinStartDate1, clinEndDate1, clinStartDate2, clinEndDate2, clinStartDate3, clinEndDate3;
        private readonly DateTime boeStartDate1, boeEndDate1, boeStartDate2, boeEndDate2, boeStartDate3, boeEndDate3, boeStartDateNC, boeEndDateNC;
        private readonly DateTime taskStartDate1, taskEndDate1, taskStartDate2, taskEndDate2, taskStartDate3, taskEndDate3, taskStartDateNC, taskEndDateNC;
        private readonly DateTime resourceStartDate1, resourceEndDate1, resourceStartDate2, resourceEndDate2, resourceStartDate3, resourceEndDate3, resourceStartDateNC, resourceEndDateNC;

        // used for edge case testing
        private FullWorkspace workspace2;
        private FullClin clin4;
        private FullBoe boe41, boe42, boe43, boe44, boe45;

        #endregion Enums and Private Classes

        #region Starting Data

        /// <summary>
        /// Prevents a default instance of the <see cref="DateAdjusterTestCases"/> class from being created.
        /// </summary>
        private DateAdjusterTestCases() { }

        /// <summary>
        /// Constructor with the retriever set.
        /// </summary>
        /// <param name="retriever">A mocked retriever used to setup retrievals for this particular test case.</param>
        public DateAdjusterTestCases(Mock<IRetriever> retriever)
        {
            if (retriever == null)
            {
                throw new ArgumentNullException("retriever");
            }

            this.retriever = retriever;

            // Set all of the starting dates for the workspace and children
            contractStartDate = GenBOEUtilities.AdjustDateTimePrecision(Convert.ToDateTime("1/15/2014"), DateTimePrecision.Month);  
            contractEndDate = GenBOEUtilities.AdjustDateTimePrecision(Convert.ToDateTime("12/15/2016"), DateTimePrecision.Month);

            clinStartDate1 = null;
            clinEndDate1 = null;
            clinStartDate2 = contractStartDate.AddMonths(4);
            clinEndDate2 = contractEndDate.AddMonths(-4);
            clinStartDate3 = contractStartDate.AddMonths(6);
            clinEndDate3 = contractEndDate.AddMonths(-6);

            boeStartDate1 = contractStartDate.AddMonths(1);
            boeEndDate1 = contractEndDate.AddMonths(-1);
            boeStartDate2 = clinStartDate2.Value.AddMonths(1);
            boeEndDate2 = clinEndDate2.Value.AddMonths(-1);
            boeStartDate3 = clinStartDate3.Value.AddMonths(1);
            boeEndDate3 = clinEndDate3.Value.AddMonths(-1);
            boeStartDateNC = contractStartDate.AddMonths(2);
            boeEndDateNC = contractEndDate.AddMonths(-2);

            taskStartDate1 = boeStartDate1.AddMonths(1);
            taskEndDate1 = boeEndDate1.AddMonths(-1);
            taskStartDate2 = boeStartDate2.AddMonths(1);
            taskEndDate2 = boeEndDate2.AddMonths(-1);
            taskStartDate3 = boeStartDate3.AddMonths(1);
            taskEndDate3 = boeEndDate3.AddMonths(-1);
            taskStartDateNC = boeStartDateNC.AddMonths(2);
            taskEndDateNC = boeEndDateNC.AddMonths(-2);

            resourceStartDate1 = taskStartDate1.AddMonths(1);
            resourceEndDate1 = taskEndDate1.AddMonths(-1);
            resourceStartDate2 = taskStartDate2.AddMonths(1);
            resourceEndDate2 = taskEndDate2.AddMonths(-1);
            resourceStartDate3 = taskStartDate3.AddMonths(1);
            resourceEndDate3 = taskEndDate3.AddMonths(-1);
            resourceStartDateNC = taskStartDateNC.AddMonths(2);
            resourceEndDateNC = taskEndDateNC.AddMonths(-2);
        }

        /// <summary>
        /// Generate the full workspace objects to start each test from.
        /// </summary>
        /// <returns>The starting workspace objects for this test case.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals")]
        public ICollection<FullWorkspace> getStartingWorkspaces()
        {
            // Setup the first workspace to hold the generic test cases
            workspace1 = new FullWorkspace(new WorkspaceDTO { Id = 1, ContractStartDate = contractStartDate, ContractEndDate = contractEndDate, UpdateDate = DateTime.Now });

            clin1 = new FullClin { Id = 1, ClinNumber = "101", ClinPaddedNumber = "101", ClinTitle = "CLIN One", StartDate = clinStartDate1, EndDate = clinEndDate1, WorkspaceID = workspace1.Id };
            clin2 = new FullClin { Id = 2, ClinNumber = "102", ClinPaddedNumber = "102", ClinTitle = "CLIN Two", StartDate = clinStartDate2, EndDate = clinEndDate2, WorkspaceID = workspace1.Id };
            clin3 = new FullClin { Id = 3, ClinNumber = "103", ClinPaddedNumber = "103", ClinTitle = "CLIN Three", StartDate = clinStartDate3, EndDate = clinEndDate3, WorkspaceID = workspace1.Id };
            Collection<FullClin> clinCollection = new Collection<FullClin> { clin1, clin2, clin3 };

            boe1 = new FullBoe { Id = 1, CLINID = clin1.Id, StartDate = boeStartDate1, EndDate = boeEndDate1, WorkspaceID = workspace1.Id };
            boe2 = new FullBoe { Id = 2, CLINID = clin2.Id, StartDate = boeStartDate2, EndDate = boeEndDate2, WorkspaceID = workspace1.Id };
            boe3 = new FullBoe { Id = 3, CLINID = clin3.Id, StartDate = boeStartDate3, EndDate = boeEndDate3, WorkspaceID = workspace1.Id };
            boeNC = new FullBoe { Id = 4, CLINID = null, StartDate = boeStartDateNC, EndDate = boeEndDateNC, WorkspaceID = workspace1.Id };
            Collection<FullBoe> boeCollection = new Collection<FullBoe>() { boe1, boe2, boe3, boeNC };

            resource1 = new ResourceTypeDto { Id = 1, BoeID = 1, TaskElementId = 1, StartDateValue = resourceStartDate1, EndDateValue = resourceEndDate1, SpreadCurveID = SpreadCurves.SpreadCurve50, ValueSpread = 100, SpreadType = SpreadType.Cost,
                LaborSpreads = new Collection<ResourceSpreadDto>()
                {
                    new ResourceSpreadDto() { LaborSpreadDate = resourceStartDate1, LaborSpreadValue = 0.02m },
                    new ResourceSpreadDto() { LaborSpreadDate = resourceStartDate1.AddMonths(1), LaborSpreadValue = 0.12m },
                    new ResourceSpreadDto() { LaborSpreadDate = resourceStartDate1.AddMonths(2), LaborSpreadValue = 0.21m },
                    new ResourceSpreadDto() { LaborSpreadDate = resourceStartDate1.AddMonths(3), LaborSpreadValue = 0.32m },
                    new ResourceSpreadDto() { LaborSpreadDate = resourceStartDate1.AddMonths(4), LaborSpreadValue = 0.46m },
                    new ResourceSpreadDto() { LaborSpreadDate = resourceStartDate1.AddMonths(5), LaborSpreadValue = 0.64m },
                    new ResourceSpreadDto() { LaborSpreadDate = resourceStartDate1.AddMonths(6), LaborSpreadValue = 0.88m },
                    new ResourceSpreadDto() { LaborSpreadDate = resourceStartDate1.AddMonths(7), LaborSpreadValue = 1.18m },
                    new ResourceSpreadDto() { LaborSpreadDate = resourceStartDate1.AddMonths(8), LaborSpreadValue = 1.56m },
                    new ResourceSpreadDto() { LaborSpreadDate = resourceStartDate1.AddMonths(9), LaborSpreadValue = 2.01m },
                    new ResourceSpreadDto() { LaborSpreadDate = resourceStartDate1.AddMonths(10), LaborSpreadValue = 2.53m },
                    new ResourceSpreadDto() { LaborSpreadDate = resourceStartDate1.AddMonths(11), LaborSpreadValue = 3.07m },
                    new ResourceSpreadDto() { LaborSpreadDate = resourceStartDate1.AddMonths(12), LaborSpreadValue = 3.59m },
                    new ResourceSpreadDto() { LaborSpreadDate = resourceStartDate1.AddMonths(13), LaborSpreadValue = 4.00m },
                    new ResourceSpreadDto() { LaborSpreadDate = resourceStartDate1.AddMonths(14), LaborSpreadValue = 4.24m },
                    new ResourceSpreadDto() { LaborSpreadDate = resourceStartDate1.AddMonths(15), LaborSpreadValue = 4.26m },
                    new ResourceSpreadDto() { LaborSpreadDate = resourceStartDate1.AddMonths(16), LaborSpreadValue = 4.08m },
                    new ResourceSpreadDto() { LaborSpreadDate = resourceStartDate1.AddMonths(17), LaborSpreadValue = 3.77m },
                    new ResourceSpreadDto() { LaborSpreadDate = resourceStartDate1.AddMonths(18), LaborSpreadValue = 3.42m },
                    new ResourceSpreadDto() { LaborSpreadDate = resourceStartDate1.AddMonths(19), LaborSpreadValue = 3.18m },
                    new ResourceSpreadDto() { LaborSpreadDate = resourceStartDate1.AddMonths(20), LaborSpreadValue = 3.26m },
                    new ResourceSpreadDto() { LaborSpreadDate = resourceStartDate1.AddMonths(21), LaborSpreadValue = 3.94m },
                    new ResourceSpreadDto() { LaborSpreadDate = resourceStartDate1.AddMonths(22), LaborSpreadValue = 5.61m },
                    new ResourceSpreadDto() { LaborSpreadDate = resourceStartDate1.AddMonths(23), LaborSpreadValue = 8.29m },
                    new ResourceSpreadDto() { LaborSpreadDate = resourceStartDate1.AddMonths(24), LaborSpreadValue = 10.73m },
                    new ResourceSpreadDto() { LaborSpreadDate = resourceStartDate1.AddMonths(25), LaborSpreadValue = 10.54m },
                    new ResourceSpreadDto() { LaborSpreadDate = resourceStartDate1.AddMonths(26), LaborSpreadValue = 7.47m },
                    new ResourceSpreadDto() { LaborSpreadDate = resourceStartDate1.AddMonths(27), LaborSpreadValue = 4.03m },
                    new ResourceSpreadDto() { LaborSpreadDate = resourceStartDate1.AddMonths(28), LaborSpreadValue = 1.83m },
                    new ResourceSpreadDto() { LaborSpreadDate = resourceStartDate1.AddMonths(29), LaborSpreadValue = 0.76m }
                }
            };
            resource2 = new ResourceTypeDto { Id = 2, BoeID = 2, TaskElementId = 2, StartDateValue = resourceStartDate2, EndDateValue = resourceEndDate2, SpreadCurveID = SpreadCurves.SpreadCurve1, ValueSpread = 100, SpreadType = SpreadType.Hours,
                LaborSpreads = new Collection<ResourceSpreadDto>()
                {
                    new ResourceSpreadDto() { LaborSpreadDate = resourceStartDate2, LaborSpreadValue = 9m },
                    new ResourceSpreadDto() { LaborSpreadDate = resourceStartDate2.AddMonths(1), LaborSpreadValue = 8m },
                    new ResourceSpreadDto() { LaborSpreadDate = resourceStartDate2.AddMonths(2), LaborSpreadValue = 8m },
                    new ResourceSpreadDto() { LaborSpreadDate = resourceStartDate2.AddMonths(3), LaborSpreadValue = 7m },
                    new ResourceSpreadDto() { LaborSpreadDate = resourceStartDate2.AddMonths(4), LaborSpreadValue = 7m },
                    new ResourceSpreadDto() { LaborSpreadDate = resourceStartDate2.AddMonths(5), LaborSpreadValue = 7m },
                    new ResourceSpreadDto() { LaborSpreadDate = resourceStartDate2.AddMonths(6), LaborSpreadValue = 6m },
                    new ResourceSpreadDto() { LaborSpreadDate = resourceStartDate2.AddMonths(7), LaborSpreadValue = 6m },
                    new ResourceSpreadDto() { LaborSpreadDate = resourceStartDate2.AddMonths(8), LaborSpreadValue = 6m },
                    new ResourceSpreadDto() { LaborSpreadDate = resourceStartDate2.AddMonths(9), LaborSpreadValue = 5m },
                    new ResourceSpreadDto() { LaborSpreadDate = resourceStartDate2.AddMonths(10), LaborSpreadValue = 5m },
                    new ResourceSpreadDto() { LaborSpreadDate = resourceStartDate2.AddMonths(11), LaborSpreadValue = 4m },
                    new ResourceSpreadDto() { LaborSpreadDate = resourceStartDate2.AddMonths(12), LaborSpreadValue = 4m },
                    new ResourceSpreadDto() { LaborSpreadDate = resourceStartDate2.AddMonths(13), LaborSpreadValue = 4m },
                    new ResourceSpreadDto() { LaborSpreadDate = resourceStartDate2.AddMonths(14), LaborSpreadValue = 3m },
                    new ResourceSpreadDto() { LaborSpreadDate = resourceStartDate2.AddMonths(15), LaborSpreadValue = 3m },
                    new ResourceSpreadDto() { LaborSpreadDate = resourceStartDate2.AddMonths(16), LaborSpreadValue = 2m },
                    new ResourceSpreadDto() { LaborSpreadDate = resourceStartDate2.AddMonths(17), LaborSpreadValue = 2m },
                    new ResourceSpreadDto() { LaborSpreadDate = resourceStartDate2.AddMonths(18), LaborSpreadValue = 2m },
                    new ResourceSpreadDto() { LaborSpreadDate = resourceStartDate2.AddMonths(19), LaborSpreadValue = 1m },
                    new ResourceSpreadDto() { LaborSpreadDate = resourceStartDate2.AddMonths(20), LaborSpreadValue = 1m },
                    new ResourceSpreadDto() { LaborSpreadDate = resourceStartDate2.AddMonths(21), LaborSpreadValue = 0m }

                }
            };
            resource3 = new ResourceTypeDto { Id = 3, BoeID = 3, TaskElementId = 3, StartDateValue = resourceStartDate3, EndDateValue = resourceEndDate3, SpreadCurveID = SpreadCurves.DiscreteHours, ValueSpread = 100, SpreadType = SpreadType.Hours,
                LaborSpreads = new Collection<ResourceSpreadDto>()
                {
                    new ResourceSpreadDto() { LaborSpreadDate = resourceStartDate3, LaborSpreadValue = 10m },
                    new ResourceSpreadDto() { LaborSpreadDate = resourceStartDate3.AddMonths(1), LaborSpreadValue = 20m },
                    new ResourceSpreadDto() { LaborSpreadDate = resourceStartDate3.AddMonths(2), LaborSpreadValue = 30m },
                    new ResourceSpreadDto() { LaborSpreadDate = resourceStartDate3.AddMonths(3), LaborSpreadValue = 0m },
                    new ResourceSpreadDto() { LaborSpreadDate = resourceStartDate3.AddMonths(4), LaborSpreadValue = 0m },
                    new ResourceSpreadDto() { LaborSpreadDate = resourceStartDate3.AddMonths(5), LaborSpreadValue = 0m },
                    new ResourceSpreadDto() { LaborSpreadDate = resourceStartDate3.AddMonths(6), LaborSpreadValue = 0m },
                    new ResourceSpreadDto() { LaborSpreadDate = resourceStartDate3.AddMonths(7), LaborSpreadValue = 0m },
                    new ResourceSpreadDto() { LaborSpreadDate = resourceStartDate3.AddMonths(8), LaborSpreadValue = 0m },
                    new ResourceSpreadDto() { LaborSpreadDate = resourceStartDate3.AddMonths(9), LaborSpreadValue = 0m },
                    new ResourceSpreadDto() { LaborSpreadDate = resourceStartDate3.AddMonths(10), LaborSpreadValue = 0m },
                    new ResourceSpreadDto() { LaborSpreadDate = resourceStartDate3.AddMonths(11), LaborSpreadValue = 0m },
                    new ResourceSpreadDto() { LaborSpreadDate = resourceStartDate3.AddMonths(12), LaborSpreadValue = 0m },
                    new ResourceSpreadDto() { LaborSpreadDate = resourceStartDate3.AddMonths(13), LaborSpreadValue = 0m },
                    new ResourceSpreadDto() { LaborSpreadDate = resourceStartDate3.AddMonths(14), LaborSpreadValue = 0m },
                    new ResourceSpreadDto() { LaborSpreadDate = resourceStartDate3.AddMonths(15), LaborSpreadValue = 0m },
                    new ResourceSpreadDto() { LaborSpreadDate = resourceStartDate3.AddMonths(16), LaborSpreadValue = 10m },
                    new ResourceSpreadDto() { LaborSpreadDate = resourceStartDate3.AddMonths(17), LaborSpreadValue = 30m }
                }
            };
            resourceNC = new ResourceTypeDto { Id = 4, BoeID = 4, TaskElementId = 4, StartDateValue = resourceStartDateNC, EndDateValue = resourceEndDateNC, SpreadCurveID = SpreadCurves.DiscreteCost, ValueSpread = 100, SpreadType = SpreadType.Cost,
                LaborSpreads = new Collection<ResourceSpreadDto>()
                {
                    new ResourceSpreadDto() { LaborSpreadDate = resourceStartDateNC, LaborSpreadValue = 0m },
                    new ResourceSpreadDto() { LaborSpreadDate = resourceStartDateNC.AddMonths(1), LaborSpreadValue = 20m },
                    new ResourceSpreadDto() { LaborSpreadDate = resourceStartDateNC.AddMonths(2), LaborSpreadValue = 20m },
                    new ResourceSpreadDto() { LaborSpreadDate = resourceStartDateNC.AddMonths(3), LaborSpreadValue = 40m },
                    new ResourceSpreadDto() { LaborSpreadDate = resourceStartDateNC.AddMonths(4), LaborSpreadValue = 0m },
                    new ResourceSpreadDto() { LaborSpreadDate = resourceStartDateNC.AddMonths(5), LaborSpreadValue = 0m },
                    new ResourceSpreadDto() { LaborSpreadDate = resourceStartDateNC.AddMonths(6), LaborSpreadValue = 0m },
                    new ResourceSpreadDto() { LaborSpreadDate = resourceStartDateNC.AddMonths(7), LaborSpreadValue = 0m },
                    new ResourceSpreadDto() { LaborSpreadDate = resourceStartDateNC.AddMonths(8), LaborSpreadValue = 0m },
                    new ResourceSpreadDto() { LaborSpreadDate = resourceStartDateNC.AddMonths(9), LaborSpreadValue = 0m },
                    new ResourceSpreadDto() { LaborSpreadDate = resourceStartDateNC.AddMonths(10), LaborSpreadValue = 0m },
                    new ResourceSpreadDto() { LaborSpreadDate = resourceStartDateNC.AddMonths(11), LaborSpreadValue = 0m },
                    new ResourceSpreadDto() { LaborSpreadDate = resourceStartDateNC.AddMonths(12), LaborSpreadValue = 0m },
                    new ResourceSpreadDto() { LaborSpreadDate = resourceStartDateNC.AddMonths(13), LaborSpreadValue = 0m },
                    new ResourceSpreadDto() { LaborSpreadDate = resourceStartDateNC.AddMonths(14), LaborSpreadValue = 0m },
                    new ResourceSpreadDto() { LaborSpreadDate = resourceStartDateNC.AddMonths(15), LaborSpreadValue = 0m },
                    new ResourceSpreadDto() { LaborSpreadDate = resourceStartDateNC.AddMonths(16), LaborSpreadValue = 0m },
                    new ResourceSpreadDto() { LaborSpreadDate = resourceStartDateNC.AddMonths(17), LaborSpreadValue = 0m },
                    new ResourceSpreadDto() { LaborSpreadDate = resourceStartDateNC.AddMonths(18), LaborSpreadValue = 0m },
                    new ResourceSpreadDto() { LaborSpreadDate = resourceStartDateNC.AddMonths(19), LaborSpreadValue = 0m },
                    new ResourceSpreadDto() { LaborSpreadDate = resourceStartDateNC.AddMonths(20), LaborSpreadValue = 0m },
                    new ResourceSpreadDto() { LaborSpreadDate = resourceStartDateNC.AddMonths(21), LaborSpreadValue = 0m },
                    new ResourceSpreadDto() { LaborSpreadDate = resourceStartDateNC.AddMonths(22), LaborSpreadValue = 20m },
                    new ResourceSpreadDto() { LaborSpreadDate = resourceStartDateNC.AddMonths(23), LaborSpreadValue = 0m }
                }
            };

            task1 = new BoeTaskElementDTO { Id = 1, BoeID = boe1.Id, StartDate = taskStartDate1, EndDate = taskEndDate1, taskElementLabors = new Collection<ResourceTypeDto> { resource1 } };
            task2 = new BoeTaskElementDTO { Id = 2, BoeID = boe2.Id, StartDate = taskStartDate2, EndDate = taskEndDate2, taskElementLabors = new Collection<ResourceTypeDto> { resource2 } };
            task3 = new BoeTaskElementDTO { Id = 3, BoeID = boe3.Id, StartDate = taskStartDate3, EndDate = taskEndDate3, taskElementLabors = new Collection<ResourceTypeDto> { resource3 } };
            taskNC = new BoeTaskElementDTO { Id = 4, BoeID = boeNC.Id, StartDate = taskStartDateNC, EndDate = taskEndDateNC, taskElementLabors = new Collection<ResourceTypeDto> { resourceNC } };
            Collection<BoeTaskElementDTO> taskCollection = new Collection<BoeTaskElementDTO>() { task1, task2, task3, taskNC };

            travel1 = new TravelDTO { Id = 1, BoeID = boe1.Id, StartDate = taskStartDate1, EndDate = taskEndDate1 };
            travel2 = new TravelDTO { Id = 2, BoeID = boe2.Id, StartDate = taskStartDate2, EndDate = taskEndDate2 };
            travel3 = new TravelDTO { Id = 3, BoeID = boe3.Id, StartDate = taskStartDate3, EndDate = taskEndDate3 };

            travel1.MSTTravelTrips = CreateTravelTrips(travel1);
            travelNC = new TravelDTO { Id = 4, BoeID = boeNC.Id, StartDate = taskStartDateNC, EndDate = taskEndDateNC};
            Collection<TravelDTO> travelCollection = new Collection<TravelDTO>() { travel1, travel2, travel3, travelNC };

            retriever.Setup(x => x.GetClinsByWorkspaceId(workspace1.Id)).Returns(clinCollection);
            retriever.Setup(x => x.GetFullBoesByWorkspaceId(workspace1.Id)).Returns(boeCollection);
            retriever.Setup(x => x.GetFullBoesByClinId(clin1.Id)).Returns(new Collection<FullBoe>() { new FullBoe(boe1) });
            retriever.Setup(x => x.GetFullBoesByClinId(clin2.Id)).Returns(new Collection<FullBoe>() { new FullBoe(boe2) });
            retriever.Setup(x => x.GetFullBoesByClinId(clin3.Id)).Returns(new Collection<FullBoe>() { new FullBoe(boe3) });
            retriever.Setup(x => x.GetTravelByWorkspaceId(workspace1.Id, It.IsAny<bool>())).Returns(travelCollection);
            retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(workspace1.Id, It.IsAny<bool>())).Returns(taskCollection);
            retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(boe1.Id, false)).Returns(new Collection<BoeTaskElementDTO>() { task1 });
            retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(boe2.Id, false)).Returns(new Collection<BoeTaskElementDTO>() { task2 });
            retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(boe3.Id, false)).Returns(new Collection<BoeTaskElementDTO>() { task3 });
            retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(boeNC.Id, false)).Returns(new Collection<BoeTaskElementDTO>() { taskNC });
            retriever.Setup(x => x.GetLaborTypesTypesForBoeId(boe1.Id)).Returns(new Collection<ResourceTypeDto>() { resource1 });
            retriever.Setup(x => x.GetLaborTypesTypesForBoeId(boe2.Id)).Returns(new Collection<ResourceTypeDto>() { resource2 });
            retriever.Setup(x => x.GetLaborTypesTypesForBoeId(boe3.Id)).Returns(new Collection<ResourceTypeDto>() { resource3 });
            retriever.Setup(x => x.GetLaborTypesTypesForBoeId(boeNC.Id)).Returns(new Collection<ResourceTypeDto>() { resourceNC });
            retriever.Setup(x => x.GetTravelCollectionByBoeID(boe1.Id, false)).Returns(new Collection<TravelDTO>() { travel1 });
            retriever.Setup(x => x.GetTravelCollectionByBoeID(boe2.Id, false)).Returns(new Collection<TravelDTO>() { travel2 });
            retriever.Setup(x => x.GetTravelCollectionByBoeID(boe3.Id, false)).Returns(new Collection<TravelDTO>() { travel3 });
            retriever.Setup(x => x.GetTravelCollectionByBoeID(boeNC.Id, false)).Returns(new Collection<TravelDTO>() { travelNC });

            // Set up the extra cases that were discussed with the PAB, from the spreadsheet.
            workspace2 = new FullWorkspace(new WorkspaceDTO { Id = 2, ContractStartDate = Convert.ToDateTime("1/1/2016"), ContractEndDate = Convert.ToDateTime("12/31/2016") });
            clin4 = new FullClin { Id = 4, WorkspaceID = workspace2.Id, StartDate = Convert.ToDateTime("1/1/16"), EndDate = Convert.ToDateTime("12/1/16") };
            boe41 = new FullBoe { Id = 41, CLINID = 4, WorkspaceID = workspace2.Id, StartDate = Convert.ToDateTime("1/1/16"), EndDate = Convert.ToDateTime("12/1/16") };
            boe42 = new FullBoe { Id = 42, CLINID = 4, WorkspaceID = workspace2.Id, StartDate = Convert.ToDateTime("1/1/16"), EndDate = Convert.ToDateTime("12/1/16") };
            boe43 = new FullBoe { Id = 43, CLINID = 4, WorkspaceID = workspace2.Id, StartDate = Convert.ToDateTime("1/1/16"), EndDate = Convert.ToDateTime("12/1/16") };
            boe44 = new FullBoe { Id = 44, CLINID = 4, WorkspaceID = workspace2.Id, StartDate = Convert.ToDateTime("1/1/16"), EndDate = Convert.ToDateTime("12/1/16") };
            boe45 = new FullBoe { Id = 45, CLINID = 4, WorkspaceID = workspace2.Id, StartDate = Convert.ToDateTime("1/1/16"), EndDate = Convert.ToDateTime("12/1/16") };
            Collection<FullBoe> boeCollection2 = new Collection<FullBoe> { boe41, boe42, boe43, boe44, boe45 };

            List<FullWorkspace> workspaces = new List<FullWorkspace> { workspace1, workspace2 };
            retriever.Setup(x => x.GetClinsByWorkspaceId(workspace2.Id)).Returns(new List<FullClin> { clin4 });
            retriever.Setup(x => x.GetFullBoesByWorkspaceId(workspace2.Id)).Returns(boeCollection2);
            retriever.Setup(x => x.GetFullBoesByClinId(clin4.Id)).Returns(boeCollection2);
            retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(boe41.Id, false)).Returns(new Collection<BoeTaskElementDTO>());
            retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(boe42.Id, false)).Returns(new Collection<BoeTaskElementDTO>());
            retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(boe43.Id, false)).Returns(new Collection<BoeTaskElementDTO>());
            retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(boe44.Id, false)).Returns(new Collection<BoeTaskElementDTO>());
            retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(boe45.Id, false)).Returns(new Collection<BoeTaskElementDTO>());
            retriever.Setup(x => x.GetTravelCollectionByBoeID(boe41.Id, false)).Returns(new Collection<TravelDTO>());
            retriever.Setup(x => x.GetTravelCollectionByBoeID(boe42.Id, false)).Returns(new Collection<TravelDTO>());
            retriever.Setup(x => x.GetTravelCollectionByBoeID(boe43.Id, false)).Returns(new Collection<TravelDTO>());
            retriever.Setup(x => x.GetTravelCollectionByBoeID(boe44.Id, false)).Returns(new Collection<TravelDTO>());
            retriever.Setup(x => x.GetTravelCollectionByBoeID(boe45.Id, false)).Returns(new Collection<TravelDTO>());

            return workspaces;
        }

        private static ICollection<MSTTravelTripType> CreateTravelTrips(TravelDTO travel)
        {
            List<MSTTravelTripType> trips = new List<MSTTravelTripType>
            {
                new MSTTravelTripType
                {
                    BoeID = travel.BoeID,
                    Cost = 10m,
                    EstimateDate = DateTime.Now,
                    Id = 1,
                    ModeID = MSTTravelMode.ZoneAirfare,
                    TripDate = travel.StartDate.Value
                }
            };

            return trips;
        }

        #endregion Starting Data

        #region Test Cases

        Collection<AdjustDateTestCase> testCases = null;

        /// <summary>
        /// Generate and return the test cases.
        /// </summary>
        /// <returns>The date adjuster test cases.</returns>
        [SuppressMessage("Microsoft.Maintainability", "CA1809:AvoidExcessiveLocals")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals")]
        public Collection<AdjustDateTestCase> getTestCases()
        {
            if (testCases == null)
            {
                testCases = new Collection<AdjustDateTestCase>();
                
                // workspace no change
                testCases.Add(new AdjustDateTestCase
                {
                    TestName = "No Change",
                    DateShiftLevel = Level.Workspace,
                    FlowdownType = DateAdjustFlowdownType.Automatic,
                    DiscreteType = DateAdjustDiscreteType.NotSet,
                    StartDateShift = 0,
                    EndDateShift = 0,
                    ExpectedDateRange = new Dictionary<Element, Tuple<DateTime?, DateTime?>>()
                    {
                        { Element.Workspace, new Tuple<DateTime?, DateTime?>(contractStartDate, contractEndDate) },
                        { Element.Clin1, new Tuple<DateTime?, DateTime?>(null, null) },
                        { Element.Clin2, new Tuple<DateTime?, DateTime?>(clinStartDate2, clinEndDate2) },
                        { Element.Clin3, new Tuple<DateTime?, DateTime?>(clinStartDate3, clinEndDate3) },
                        { Element.Boe1, new Tuple<DateTime?, DateTime?> (boeStartDate1, boeEndDate1) },
                        { Element.Boe2, new Tuple<DateTime?, DateTime?> (boeStartDate2, boeEndDate2) },
                        { Element.Boe3, new Tuple<DateTime?, DateTime?> (boeStartDate3, boeEndDate3) },
                        { Element.BoeNC, new Tuple<DateTime?, DateTime?> (boeStartDateNC, boeEndDateNC) },
                        { Element.Task1, new Tuple<DateTime?, DateTime?> (taskStartDate1, taskEndDate1) },
                        { Element.Task2, new Tuple<DateTime?, DateTime?> (taskStartDate2, taskEndDate2) },
                        { Element.Task3, new Tuple<DateTime?, DateTime?> (taskStartDate3, taskEndDate3) },
                        { Element.TaskNC, new Tuple<DateTime?, DateTime?> (taskStartDateNC, taskEndDateNC) },
                        { Element.Travel1, new Tuple<DateTime?, DateTime?> (taskStartDate1, taskEndDate1) },
                        { Element.TravelTrip1, new Tuple<DateTime?, DateTime?> (taskStartDate1, null) },
                        { Element.Travel2, new Tuple<DateTime?, DateTime?> (taskStartDate2, taskEndDate2) },
                        { Element.Travel3, new Tuple<DateTime?, DateTime?> (taskStartDate3, taskEndDate3) },
                        { Element.TravelNC, new Tuple<DateTime?, DateTime?> (taskStartDateNC, taskEndDateNC) },
                        { Element.Resource1, new Tuple<DateTime?, DateTime?> (resourceStartDate1, resourceEndDate1) },
                        { Element.Resource2, new Tuple<DateTime?, DateTime?> (resourceStartDate2, resourceEndDate2) },
                        { Element.Resource3, new Tuple<DateTime?, DateTime?> (resourceStartDate3, resourceEndDate3) },
                        { Element.ResourceNC, new Tuple<DateTime?, DateTime?> (resourceStartDateNC, resourceEndDateNC) }
                    },
                    ExpectedSpreadResults = new Dictionary<Element, Dictionary<DateTime, decimal>>()
                    {
                        { Element.Resource1, new Dictionary<DateTime, decimal>()
                            {
                                { resourceStartDate1, 0.02m },
                                { resourceStartDate1.AddMonths(1), 0.12m },
                                { resourceStartDate1.AddMonths(2), 0.21m },
                                { resourceStartDate1.AddMonths(3), 0.32m },
                                { resourceStartDate1.AddMonths(4), 0.46m },
                                { resourceStartDate1.AddMonths(5), 0.64m },
                                { resourceStartDate1.AddMonths(6), 0.88m },
                                { resourceStartDate1.AddMonths(7), 1.18m },
                                { resourceStartDate1.AddMonths(8), 1.56m },
                                { resourceStartDate1.AddMonths(9), 2.01m },
                                { resourceStartDate1.AddMonths(10), 2.53m },
                                { resourceStartDate1.AddMonths(11), 3.07m },
                                { resourceStartDate1.AddMonths(12), 3.59m },
                                { resourceStartDate1.AddMonths(13), 4.00m },
                                { resourceStartDate1.AddMonths(14), 4.24m },
                                { resourceStartDate1.AddMonths(15), 4.26m },
                                { resourceStartDate1.AddMonths(16), 4.08m },
                                { resourceStartDate1.AddMonths(17), 3.77m },
                                { resourceStartDate1.AddMonths(18), 3.42m },
                                { resourceStartDate1.AddMonths(19), 3.18m },
                                { resourceStartDate1.AddMonths(20), 3.26m },
                                { resourceStartDate1.AddMonths(21), 3.94m },
                                { resourceStartDate1.AddMonths(22), 5.61m },
                                { resourceStartDate1.AddMonths(23), 8.29m },
                                { resourceStartDate1.AddMonths(24), 10.73m },
                                { resourceStartDate1.AddMonths(25), 10.54m },
                                { resourceStartDate1.AddMonths(26), 7.47m },
                                { resourceStartDate1.AddMonths(27), 4.03m },
                                { resourceStartDate1.AddMonths(28), 1.83m },
                                { resourceStartDate1.AddMonths(29), 0.76m },
                            }
                        },
                        { Element.Resource2, new Dictionary<DateTime, decimal>()
                            {
                                { resourceStartDate2, 9m },
                                { resourceStartDate2.AddMonths(1), 8m },
                                { resourceStartDate2.AddMonths(2), 8m },
                                { resourceStartDate2.AddMonths(3), 7m },
                                { resourceStartDate2.AddMonths(4), 7m },
                                { resourceStartDate2.AddMonths(5), 7m },
                                { resourceStartDate2.AddMonths(6), 6m },
                                { resourceStartDate2.AddMonths(7), 6m },
                                { resourceStartDate2.AddMonths(8), 6m },
                                { resourceStartDate2.AddMonths(9), 5m },
                                { resourceStartDate2.AddMonths(10), 5m },
                                { resourceStartDate2.AddMonths(11), 4m },
                                { resourceStartDate2.AddMonths(12), 4m },
                                { resourceStartDate2.AddMonths(13), 4m },
                                { resourceStartDate2.AddMonths(14), 3m },
                                { resourceStartDate2.AddMonths(15), 3m },
                                { resourceStartDate2.AddMonths(16), 2m },
                                { resourceStartDate2.AddMonths(17), 2m },
                                { resourceStartDate2.AddMonths(18), 2m },
                                { resourceStartDate2.AddMonths(19), 1m },
                                { resourceStartDate2.AddMonths(20), 1m },
                                { resourceStartDate2.AddMonths(21), 0m }
                            }
                        },
                        { Element.Resource3, new Dictionary<DateTime, decimal>()
                            {
                                { resourceStartDate3, 10m },
                                { resourceStartDate3.AddMonths(1), 20m },
                                { resourceStartDate3.AddMonths(2), 30m },
                                { resourceStartDate3.AddMonths(3), 0m },
                                { resourceStartDate3.AddMonths(4), 0m },
                                { resourceStartDate3.AddMonths(5), 0m },
                                { resourceStartDate3.AddMonths(6), 0m },
                                { resourceStartDate3.AddMonths(7), 0m },
                                { resourceStartDate3.AddMonths(8), 0m },
                                { resourceStartDate3.AddMonths(9), 0m },
                                { resourceStartDate3.AddMonths(10), 0m },
                                { resourceStartDate3.AddMonths(11), 0m },
                                { resourceStartDate3.AddMonths(12), 0m },
                                { resourceStartDate3.AddMonths(13), 0m },
                                { resourceStartDate3.AddMonths(14), 0m },
                                { resourceStartDate3.AddMonths(15), 0m },
                                { resourceStartDate3.AddMonths(16), 10m },
                                { resourceStartDate3.AddMonths(17), 30m }
                            }
                        },
                        { Element.ResourceNC, new Dictionary<DateTime, decimal>()
                            {
                                { resourceStartDateNC, 0m },
                                { resourceStartDateNC.AddMonths(1), 20m },
                                { resourceStartDateNC.AddMonths(2), 20m },
                                { resourceStartDateNC.AddMonths(3), 40m },
                                { resourceStartDateNC.AddMonths(4), 0m },
                                { resourceStartDateNC.AddMonths(5), 0m },
                                { resourceStartDateNC.AddMonths(6), 0m },
                                { resourceStartDateNC.AddMonths(7), 0m },
                                { resourceStartDateNC.AddMonths(8), 0m },
                                { resourceStartDateNC.AddMonths(9), 0m },
                                { resourceStartDateNC.AddMonths(10), 0m },
                                { resourceStartDateNC.AddMonths(11), 0m },
                                { resourceStartDateNC.AddMonths(12), 0m },
                                { resourceStartDateNC.AddMonths(13), 0m },
                                { resourceStartDateNC.AddMonths(14), 0m },
                                { resourceStartDateNC.AddMonths(15), 0m },
                                { resourceStartDateNC.AddMonths(16), 0m },
                                { resourceStartDateNC.AddMonths(17), 0m },
                                { resourceStartDateNC.AddMonths(18), 0m },
                                { resourceStartDateNC.AddMonths(19), 0m },
                                { resourceStartDateNC.AddMonths(20), 0m },
                                { resourceStartDateNC.AddMonths(21), 0m },
                                { resourceStartDateNC.AddMonths(22), 20m },
                                { resourceStartDateNC.AddMonths(23), 0m }
                            }
                        }
                    }
                });

                // workspace shift
                testCases.Add(new AdjustDateTestCase
                {
                    TestName = "Shift workspace by 1 year",
                    DateShiftLevel = Level.Workspace,
                    FlowdownType = DateAdjustFlowdownType.Automatic,
                    DiscreteType = DateAdjustDiscreteType.NotSet,
                    StartDateShift = 12,
                    EndDateShift = 12,
                    ExpectedDateRange = new Dictionary<Element, Tuple<DateTime?, DateTime?>>()
                    {
                        { Element.Workspace, new Tuple<DateTime?, DateTime?>(contractStartDate.AddMonths(12), contractEndDate.AddMonths(12)) },
                        { Element.Clin1, new Tuple<DateTime?, DateTime?>(null, null) },
                        { Element.Clin2, new Tuple<DateTime?, DateTime?>(clinStartDate2.Value.AddMonths(12), clinEndDate2.Value.AddMonths(12)) },
                        { Element.Clin3, new Tuple<DateTime?, DateTime?>(clinStartDate3.Value.AddMonths(12), clinEndDate3.Value.AddMonths(12)) },
                        { Element.Boe1, new Tuple<DateTime?, DateTime?> (boeStartDate1.AddMonths(12), boeEndDate1.AddMonths(12)) },
                        { Element.Boe2, new Tuple<DateTime?, DateTime?> (boeStartDate2.AddMonths(12), boeEndDate2.AddMonths(12)) },
                        { Element.Boe3, new Tuple<DateTime?, DateTime?> (boeStartDate3.AddMonths(12), boeEndDate3.AddMonths(12)) },
                        { Element.BoeNC, new Tuple<DateTime?, DateTime?> (boeStartDateNC.AddMonths(12), boeEndDateNC.AddMonths(12)) },
                        { Element.Task1, new Tuple<DateTime?, DateTime?> (taskStartDate1.AddMonths(12), taskEndDate1.AddMonths(12)) },
                        { Element.Task2, new Tuple<DateTime?, DateTime?> (taskStartDate2.AddMonths(12), taskEndDate2.AddMonths(12)) },
                        { Element.Task3, new Tuple<DateTime?, DateTime?> (taskStartDate3.AddMonths(12), taskEndDate3.AddMonths(12)) },
                        { Element.TaskNC, new Tuple<DateTime?, DateTime?> (taskStartDateNC.AddMonths(12), taskEndDateNC.AddMonths(12)) },
                        { Element.Travel1, new Tuple<DateTime?, DateTime?> (taskStartDate1.AddMonths(12), taskEndDate1.AddMonths(12)) },
                        { Element.TravelTrip1, new Tuple<DateTime?, DateTime?> (taskStartDate1.AddMonths(12), null) },
                        { Element.Travel2, new Tuple<DateTime?, DateTime?> (taskStartDate2.AddMonths(12), taskEndDate2.AddMonths(12)) },
                        { Element.Travel3, new Tuple<DateTime?, DateTime?> (taskStartDate3.AddMonths(12), taskEndDate3.AddMonths(12)) },
                        { Element.TravelNC, new Tuple<DateTime?, DateTime?> (taskStartDateNC.AddMonths(12), taskEndDateNC.AddMonths(12)) },
                        { Element.Resource1, new Tuple<DateTime?, DateTime?> (resourceStartDate1.AddMonths(12), resourceEndDate1.AddMonths(12)) },
                        { Element.Resource2, new Tuple<DateTime?, DateTime?> (resourceStartDate2.AddMonths(12), resourceEndDate2.AddMonths(12)) },
                        { Element.Resource3, new Tuple<DateTime?, DateTime?> (resourceStartDate3.AddMonths(12), resourceEndDate3.AddMonths(12)) },
                        { Element.ResourceNC, new Tuple<DateTime?, DateTime?> (resourceStartDateNC.AddMonths(12), resourceEndDateNC.AddMonths(12)) }
                    },
                    ExpectedSpreadResults = new Dictionary<Element, Dictionary<DateTime, decimal>>()
                    {
                        { Element.Resource1, new Dictionary<DateTime, decimal>()
                            {
                                { resourceStartDate1.AddMonths(12), 0.02m },
                                { resourceStartDate1.AddMonths(13), 0.12m },
                                { resourceStartDate1.AddMonths(14), 0.21m },
                                { resourceStartDate1.AddMonths(15), 0.32m },
                                { resourceStartDate1.AddMonths(16), 0.46m },
                                { resourceStartDate1.AddMonths(17), 0.64m },
                                { resourceStartDate1.AddMonths(18), 0.88m },
                                { resourceStartDate1.AddMonths(19), 1.18m },
                                { resourceStartDate1.AddMonths(20), 1.56m },
                                { resourceStartDate1.AddMonths(21), 2.01m },
                                { resourceStartDate1.AddMonths(22), 2.53m },
                                { resourceStartDate1.AddMonths(23), 3.07m },
                                { resourceStartDate1.AddMonths(24), 3.59m },
                                { resourceStartDate1.AddMonths(25), 4.00m },
                                { resourceStartDate1.AddMonths(26), 4.24m },
                                { resourceStartDate1.AddMonths(27), 4.26m },
                                { resourceStartDate1.AddMonths(28), 4.08m },
                                { resourceStartDate1.AddMonths(29), 3.77m },
                                { resourceStartDate1.AddMonths(30), 3.42m },
                                { resourceStartDate1.AddMonths(31), 3.18m },
                                { resourceStartDate1.AddMonths(32), 3.26m },
                                { resourceStartDate1.AddMonths(33), 3.94m },
                                { resourceStartDate1.AddMonths(34), 5.61m },
                                { resourceStartDate1.AddMonths(35), 8.29m },
                                { resourceStartDate1.AddMonths(36), 10.73m },
                                { resourceStartDate1.AddMonths(37), 10.54m },
                                { resourceStartDate1.AddMonths(38), 7.47m },
                                { resourceStartDate1.AddMonths(39), 4.03m },
                                { resourceStartDate1.AddMonths(40), 1.83m },
                                { resourceStartDate1.AddMonths(41), 0.76m },
                            }
                        },
                        { Element.Resource2, new Dictionary<DateTime, decimal>()
                            {
                                { resourceStartDate2.AddMonths(12), 9m },
                                { resourceStartDate2.AddMonths(13), 8m },
                                { resourceStartDate2.AddMonths(14), 8m },
                                { resourceStartDate2.AddMonths(15), 7m },
                                { resourceStartDate2.AddMonths(16), 7m },
                                { resourceStartDate2.AddMonths(17), 7m },
                                { resourceStartDate2.AddMonths(18), 6m },
                                { resourceStartDate2.AddMonths(19), 6m },
                                { resourceStartDate2.AddMonths(20), 6m },
                                { resourceStartDate2.AddMonths(21), 5m },
                                { resourceStartDate2.AddMonths(22), 5m },
                                { resourceStartDate2.AddMonths(23), 4m },
                                { resourceStartDate2.AddMonths(24), 4m },
                                { resourceStartDate2.AddMonths(25), 4m },
                                { resourceStartDate2.AddMonths(26), 3m },
                                { resourceStartDate2.AddMonths(27), 3m },
                                { resourceStartDate2.AddMonths(28), 2m },
                                { resourceStartDate2.AddMonths(29), 2m },
                                { resourceStartDate2.AddMonths(30), 2m },
                                { resourceStartDate2.AddMonths(31), 1m },
                                { resourceStartDate2.AddMonths(32), 1m },
                                { resourceStartDate2.AddMonths(33), 0m }
                            }
                        },
                        { Element.Resource3, new Dictionary<DateTime, decimal>()
                            {
                                { resourceStartDate3.AddMonths(12), 10m },
                                { resourceStartDate3.AddMonths(13), 20m },
                                { resourceStartDate3.AddMonths(14), 30m },
                                { resourceStartDate3.AddMonths(15), 0m },
                                { resourceStartDate3.AddMonths(16), 0m },
                                { resourceStartDate3.AddMonths(17), 0m },
                                { resourceStartDate3.AddMonths(18), 0m },
                                { resourceStartDate3.AddMonths(19), 0m },
                                { resourceStartDate3.AddMonths(20), 0m },
                                { resourceStartDate3.AddMonths(21), 0m },
                                { resourceStartDate3.AddMonths(22), 0m },
                                { resourceStartDate3.AddMonths(23), 0m },
                                { resourceStartDate3.AddMonths(24), 0m },
                                { resourceStartDate3.AddMonths(25), 0m },
                                { resourceStartDate3.AddMonths(26), 0m },
                                { resourceStartDate3.AddMonths(27), 0m },
                                { resourceStartDate3.AddMonths(28), 10m },
                                { resourceStartDate3.AddMonths(29), 30m }
                            }
                        },
                        { Element.ResourceNC, new Dictionary<DateTime, decimal>()
                            {
                                { resourceStartDateNC.AddMonths(12), 0m },
                                { resourceStartDateNC.AddMonths(13), 20m },
                                { resourceStartDateNC.AddMonths(14), 20m },
                                { resourceStartDateNC.AddMonths(15), 40m },
                                { resourceStartDateNC.AddMonths(16), 0m },
                                { resourceStartDateNC.AddMonths(17), 0m },
                                { resourceStartDateNC.AddMonths(18), 0m },
                                { resourceStartDateNC.AddMonths(19), 0m },
                                { resourceStartDateNC.AddMonths(20), 0m },
                                { resourceStartDateNC.AddMonths(21), 0m },
                                { resourceStartDateNC.AddMonths(22), 0m },
                                { resourceStartDateNC.AddMonths(23), 0m },
                                { resourceStartDateNC.AddMonths(24), 0m },
                                { resourceStartDateNC.AddMonths(25), 0m },
                                { resourceStartDateNC.AddMonths(26), 0m },
                                { resourceStartDateNC.AddMonths(27), 0m },
                                { resourceStartDateNC.AddMonths(28), 0m },
                                { resourceStartDateNC.AddMonths(29), 0m },
                                { resourceStartDateNC.AddMonths(30), 0m },
                                { resourceStartDateNC.AddMonths(31), 0m },
                                { resourceStartDateNC.AddMonths(32), 0m },
                                { resourceStartDateNC.AddMonths(33), 0m },
                                { resourceStartDateNC.AddMonths(34), 20m },
                                { resourceStartDateNC.AddMonths(35), 0m }
                            }
                        }
                    }
                });

                // workspace shift
                testCases.Add(new AdjustDateTestCase
                {
                    TestName = "Shift workspace by 1 year Using MST Controller Logic",
                    TestFromMSTControllerLogic = true,
                    DateShiftLevel = Level.Workspace,
                    FlowdownType = DateAdjustFlowdownType.NotSet,  // This test is testing that the Flowdown type gets overridden and set to Automatic inside Controller Logic, and then everything flows down accordingly
                    DiscreteType = DateAdjustDiscreteType.NotSet,
                    StartDateShift = 12,
                    EndDateShift = 12,
                    ExpectedDateRange = new Dictionary<Element, Tuple<DateTime?, DateTime?>>()
                    {
                        { Element.Workspace, new Tuple<DateTime?, DateTime?>(contractStartDate.AddMonths(12), contractEndDate.AddMonths(12)) },
                        { Element.Clin1, new Tuple<DateTime?, DateTime?>(null, null) },
                        { Element.Clin2, new Tuple<DateTime?, DateTime?>(clinStartDate2.Value.AddMonths(12), clinEndDate2.Value.AddMonths(12)) },
                        { Element.Clin3, new Tuple<DateTime?, DateTime?>(clinStartDate3.Value.AddMonths(12), clinEndDate3.Value.AddMonths(12)) },
                        { Element.Boe1, new Tuple<DateTime?, DateTime?> (boeStartDate1.AddMonths(12), boeEndDate1.AddMonths(12)) },
                        { Element.Boe2, new Tuple<DateTime?, DateTime?> (boeStartDate2.AddMonths(12), boeEndDate2.AddMonths(12)) },
                        { Element.Boe3, new Tuple<DateTime?, DateTime?> (boeStartDate3.AddMonths(12), boeEndDate3.AddMonths(12)) },
                        { Element.BoeNC, new Tuple<DateTime?, DateTime?> (boeStartDateNC.AddMonths(12), boeEndDateNC.AddMonths(12)) },
                        { Element.Task1, new Tuple<DateTime?, DateTime?> (taskStartDate1.AddMonths(12), taskEndDate1.AddMonths(12)) },
                        { Element.Task2, new Tuple<DateTime?, DateTime?> (taskStartDate2.AddMonths(12), taskEndDate2.AddMonths(12)) },
                        { Element.Task3, new Tuple<DateTime?, DateTime?> (taskStartDate3.AddMonths(12), taskEndDate3.AddMonths(12)) },
                        { Element.TaskNC, new Tuple<DateTime?, DateTime?> (taskStartDateNC.AddMonths(12), taskEndDateNC.AddMonths(12)) },
                        { Element.Travel1, new Tuple<DateTime?, DateTime?> (taskStartDate1.AddMonths(12), taskEndDate1.AddMonths(12)) },
                        { Element.TravelTrip1, new Tuple<DateTime?, DateTime?> (taskStartDate1.AddMonths(12), null) },
                        { Element.Travel2, new Tuple<DateTime?, DateTime?> (taskStartDate2.AddMonths(12), taskEndDate2.AddMonths(12)) },
                        { Element.Travel3, new Tuple<DateTime?, DateTime?> (taskStartDate3.AddMonths(12), taskEndDate3.AddMonths(12)) },
                        { Element.TravelNC, new Tuple<DateTime?, DateTime?> (taskStartDateNC.AddMonths(12), taskEndDateNC.AddMonths(12)) },
                        { Element.Resource1, new Tuple<DateTime?, DateTime?> (resourceStartDate1.AddMonths(12), resourceEndDate1.AddMonths(12)) },
                        { Element.Resource2, new Tuple<DateTime?, DateTime?> (resourceStartDate2.AddMonths(12), resourceEndDate2.AddMonths(12)) },
                        { Element.Resource3, new Tuple<DateTime?, DateTime?> (resourceStartDate3.AddMonths(12), resourceEndDate3.AddMonths(12)) },
                        { Element.ResourceNC, new Tuple<DateTime?, DateTime?> (resourceStartDateNC.AddMonths(12), resourceEndDateNC.AddMonths(12)) }
                    },
                    ExpectedSpreadResults = new Dictionary<Element, Dictionary<DateTime, decimal>>()
                    {
                        { Element.Resource1, new Dictionary<DateTime, decimal>()
                            {
                                { resourceStartDate1.AddMonths(12), 0.02m },
                                { resourceStartDate1.AddMonths(13), 0.12m },
                                { resourceStartDate1.AddMonths(14), 0.21m },
                                { resourceStartDate1.AddMonths(15), 0.32m },
                                { resourceStartDate1.AddMonths(16), 0.46m },
                                { resourceStartDate1.AddMonths(17), 0.64m },
                                { resourceStartDate1.AddMonths(18), 0.88m },
                                { resourceStartDate1.AddMonths(19), 1.18m },
                                { resourceStartDate1.AddMonths(20), 1.56m },
                                { resourceStartDate1.AddMonths(21), 2.01m },
                                { resourceStartDate1.AddMonths(22), 2.53m },
                                { resourceStartDate1.AddMonths(23), 3.07m },
                                { resourceStartDate1.AddMonths(24), 3.59m },
                                { resourceStartDate1.AddMonths(25), 4.00m },
                                { resourceStartDate1.AddMonths(26), 4.24m },
                                { resourceStartDate1.AddMonths(27), 4.26m },
                                { resourceStartDate1.AddMonths(28), 4.08m },
                                { resourceStartDate1.AddMonths(29), 3.77m },
                                { resourceStartDate1.AddMonths(30), 3.42m },
                                { resourceStartDate1.AddMonths(31), 3.18m },
                                { resourceStartDate1.AddMonths(32), 3.26m },
                                { resourceStartDate1.AddMonths(33), 3.94m },
                                { resourceStartDate1.AddMonths(34), 5.61m },
                                { resourceStartDate1.AddMonths(35), 8.29m },
                                { resourceStartDate1.AddMonths(36), 10.73m },
                                { resourceStartDate1.AddMonths(37), 10.54m },
                                { resourceStartDate1.AddMonths(38), 7.47m },
                                { resourceStartDate1.AddMonths(39), 4.03m },
                                { resourceStartDate1.AddMonths(40), 1.83m },
                                { resourceStartDate1.AddMonths(41), 0.76m },
                            }
                        },
                        { Element.Resource2, new Dictionary<DateTime, decimal>()
                            {
                                { resourceStartDate2.AddMonths(12), 9m },
                                { resourceStartDate2.AddMonths(13), 8m },
                                { resourceStartDate2.AddMonths(14), 8m },
                                { resourceStartDate2.AddMonths(15), 7m },
                                { resourceStartDate2.AddMonths(16), 7m },
                                { resourceStartDate2.AddMonths(17), 7m },
                                { resourceStartDate2.AddMonths(18), 6m },
                                { resourceStartDate2.AddMonths(19), 6m },
                                { resourceStartDate2.AddMonths(20), 6m },
                                { resourceStartDate2.AddMonths(21), 5m },
                                { resourceStartDate2.AddMonths(22), 5m },
                                { resourceStartDate2.AddMonths(23), 4m },
                                { resourceStartDate2.AddMonths(24), 4m },
                                { resourceStartDate2.AddMonths(25), 4m },
                                { resourceStartDate2.AddMonths(26), 3m },
                                { resourceStartDate2.AddMonths(27), 3m },
                                { resourceStartDate2.AddMonths(28), 2m },
                                { resourceStartDate2.AddMonths(29), 2m },
                                { resourceStartDate2.AddMonths(30), 2m },
                                { resourceStartDate2.AddMonths(31), 1m },
                                { resourceStartDate2.AddMonths(32), 1m },
                                { resourceStartDate2.AddMonths(33), 0m }
                            }
                        },
                        { Element.Resource3, new Dictionary<DateTime, decimal>()
                            {
                                { resourceStartDate3.AddMonths(12), 10m },
                                { resourceStartDate3.AddMonths(13), 20m },
                                { resourceStartDate3.AddMonths(14), 30m },
                                { resourceStartDate3.AddMonths(15), 0m },
                                { resourceStartDate3.AddMonths(16), 0m },
                                { resourceStartDate3.AddMonths(17), 0m },
                                { resourceStartDate3.AddMonths(18), 0m },
                                { resourceStartDate3.AddMonths(19), 0m },
                                { resourceStartDate3.AddMonths(20), 0m },
                                { resourceStartDate3.AddMonths(21), 0m },
                                { resourceStartDate3.AddMonths(22), 0m },
                                { resourceStartDate3.AddMonths(23), 0m },
                                { resourceStartDate3.AddMonths(24), 0m },
                                { resourceStartDate3.AddMonths(25), 0m },
                                { resourceStartDate3.AddMonths(26), 0m },
                                { resourceStartDate3.AddMonths(27), 0m },
                                { resourceStartDate3.AddMonths(28), 10m },
                                { resourceStartDate3.AddMonths(29), 30m }
                            }
                        },
                        { Element.ResourceNC, new Dictionary<DateTime, decimal>()
                            {
                                { resourceStartDateNC.AddMonths(12), 0m },
                                { resourceStartDateNC.AddMonths(13), 20m },
                                { resourceStartDateNC.AddMonths(14), 20m },
                                { resourceStartDateNC.AddMonths(15), 40m },
                                { resourceStartDateNC.AddMonths(16), 0m },
                                { resourceStartDateNC.AddMonths(17), 0m },
                                { resourceStartDateNC.AddMonths(18), 0m },
                                { resourceStartDateNC.AddMonths(19), 0m },
                                { resourceStartDateNC.AddMonths(20), 0m },
                                { resourceStartDateNC.AddMonths(21), 0m },
                                { resourceStartDateNC.AddMonths(22), 0m },
                                { resourceStartDateNC.AddMonths(23), 0m },
                                { resourceStartDateNC.AddMonths(24), 0m },
                                { resourceStartDateNC.AddMonths(25), 0m },
                                { resourceStartDateNC.AddMonths(26), 0m },
                                { resourceStartDateNC.AddMonths(27), 0m },
                                { resourceStartDateNC.AddMonths(28), 0m },
                                { resourceStartDateNC.AddMonths(29), 0m },
                                { resourceStartDateNC.AddMonths(30), 0m },
                                { resourceStartDateNC.AddMonths(31), 0m },
                                { resourceStartDateNC.AddMonths(32), 0m },
                                { resourceStartDateNC.AddMonths(33), 0m },
                                { resourceStartDateNC.AddMonths(34), 20m },
                                { resourceStartDateNC.AddMonths(35), 0m }
                            }
                        }
                    }
                });

                // workspace shift
                testCases.Add(new AdjustDateTestCase
                {
                    TestName = "Shift and shorten workspace by 1 year truncate discrete",
                    DateShiftLevel = Level.Workspace,
                    FlowdownType = DateAdjustFlowdownType.Automatic,
                    DiscreteType = DateAdjustDiscreteType.Truncate,
                    StartDateShift = 12,
                    EndDateShift = 6,
                    ExpectedDateRange = new Dictionary<Element, Tuple<DateTime?, DateTime?>>()
                    {
                        { Element.Workspace, new Tuple<DateTime?, DateTime?>(contractStartDate.AddMonths(12), contractEndDate.AddMonths(6)) },
                        { Element.Clin1, new Tuple<DateTime?, DateTime?>(null, null) },
                        { Element.Clin2, new Tuple<DateTime?, DateTime?>(clinStartDate2.Value.AddMonths(12), clinEndDate2.Value.AddMonths(6)) },
                        { Element.Clin3, new Tuple<DateTime?, DateTime?>(clinStartDate3.Value.AddMonths(12), clinEndDate3.Value.AddMonths(6)) },
                        { Element.Boe1, new Tuple<DateTime?, DateTime?> (boeStartDate1.AddMonths(12), boeEndDate1.AddMonths(6)) },
                        { Element.Boe2, new Tuple<DateTime?, DateTime?> (boeStartDate2.AddMonths(12), boeEndDate2.AddMonths(6)) },
                        { Element.Boe3, new Tuple<DateTime?, DateTime?> (boeStartDate3.AddMonths(12), boeEndDate3.AddMonths(6)) },
                        { Element.BoeNC, new Tuple<DateTime?, DateTime?> (boeStartDateNC.AddMonths(12), boeEndDateNC.AddMonths(6)) },
                        { Element.Task1, new Tuple<DateTime?, DateTime?> (taskStartDate1.AddMonths(12), taskEndDate1.AddMonths(6)) },
                        { Element.Task2, new Tuple<DateTime?, DateTime?> (taskStartDate2.AddMonths(12), taskEndDate2.AddMonths(6)) },
                        { Element.Task3, new Tuple<DateTime?, DateTime?> (taskStartDate3.AddMonths(12), taskEndDate3.AddMonths(6)) },
                        { Element.TaskNC, new Tuple<DateTime?, DateTime?> (taskStartDateNC.AddMonths(12), taskEndDateNC.AddMonths(6)) },
                        { Element.Travel1, new Tuple<DateTime?, DateTime?> (taskStartDate1.AddMonths(12), taskEndDate1.AddMonths(6)) },
                        { Element.TravelTrip1, new Tuple<DateTime?, DateTime?> (taskStartDate1.AddMonths(12), null) },
                        { Element.Travel2, new Tuple<DateTime?, DateTime?> (taskStartDate2.AddMonths(12), taskEndDate2.AddMonths(6)) },
                        { Element.Travel3, new Tuple<DateTime?, DateTime?> (taskStartDate3.AddMonths(12), taskEndDate3.AddMonths(6)) },
                        { Element.TravelNC, new Tuple<DateTime?, DateTime?> (taskStartDateNC.AddMonths(12), taskEndDateNC.AddMonths(6)) },
                        { Element.Resource1, new Tuple<DateTime?, DateTime?> (resourceStartDate1.AddMonths(12), resourceEndDate1.AddMonths(6)) },
                        { Element.Resource2, new Tuple<DateTime?, DateTime?> (resourceStartDate2.AddMonths(12), resourceEndDate2.AddMonths(6)) },
                        { Element.Resource3, new Tuple<DateTime?, DateTime?> (resourceStartDate3.AddMonths(12), resourceEndDate3.AddMonths(6)) },
                        { Element.ResourceNC, new Tuple<DateTime?, DateTime?> (resourceStartDateNC.AddMonths(12), resourceEndDateNC.AddMonths(6)) }
                    },
                    ExpectedSpreadResults = new Dictionary<Element, Dictionary<DateTime, decimal>>()
                    {
                        { Element.Resource1, new Dictionary<DateTime, decimal>()
                            {
                                { resourceStartDate1.AddMonths(12), 0.04m },
                                { resourceStartDate1.AddMonths(13), 0.19m },
                                { resourceStartDate1.AddMonths(14), 0.35m },
                                { resourceStartDate1.AddMonths(15), 0.56m },
                                { resourceStartDate1.AddMonths(16), 0.84m },
                                { resourceStartDate1.AddMonths(17), 1.23m },
                                { resourceStartDate1.AddMonths(18), 1.76m },
                                { resourceStartDate1.AddMonths(19), 2.44m },
                                { resourceStartDate1.AddMonths(20), 3.24m },
                                { resourceStartDate1.AddMonths(21), 4.09m },
                                { resourceStartDate1.AddMonths(22), 4.82m },
                                { resourceStartDate1.AddMonths(23), 5.27m },
                                { resourceStartDate1.AddMonths(24), 5.31m },
                                { resourceStartDate1.AddMonths(25), 4.96m },
                                { resourceStartDate1.AddMonths(26), 4.43m },
                                { resourceStartDate1.AddMonths(27), 4.00m },
                                { resourceStartDate1.AddMonths(28), 4.15m },
                                { resourceStartDate1.AddMonths(29), 5.58m },
                                { resourceStartDate1.AddMonths(30), 9.02m },
                                { resourceStartDate1.AddMonths(31), 13.08m },
                                { resourceStartDate1.AddMonths(32), 12.76m },
                                { resourceStartDate1.AddMonths(33), 7.63m },
                                { resourceStartDate1.AddMonths(34), 3.16m },
                                { resourceStartDate1.AddMonths(35), 1.09m }
                            }
                        },
                        { Element.Resource2, new Dictionary<DateTime, decimal>()
                            {
                                { resourceStartDate2.AddMonths(12), 12m },
                                { resourceStartDate2.AddMonths(13), 11m },
                                { resourceStartDate2.AddMonths(14), 10m },
                                { resourceStartDate2.AddMonths(15), 10m },
                                { resourceStartDate2.AddMonths(16), 9m },
                                { resourceStartDate2.AddMonths(17), 8m },
                                { resourceStartDate2.AddMonths(18), 7m },
                                { resourceStartDate2.AddMonths(19), 7m },
                                { resourceStartDate2.AddMonths(20), 6m },
                                { resourceStartDate2.AddMonths(21), 5m },
                                { resourceStartDate2.AddMonths(22), 4m },
                                { resourceStartDate2.AddMonths(23), 4m },
                                { resourceStartDate2.AddMonths(24), 3m },
                                { resourceStartDate2.AddMonths(25), 2m },
                                { resourceStartDate2.AddMonths(26), 1m },
                                { resourceStartDate2.AddMonths(27), 1m }
                            }
                        },
                        { Element.Resource3, new Dictionary<DateTime, decimal>()
                            {
                                { resourceStartDate3.AddMonths(12), 10m },
                                { resourceStartDate3.AddMonths(13), 20m },
                                { resourceStartDate3.AddMonths(14), 30m },
                                { resourceStartDate3.AddMonths(15), 0m },
                                { resourceStartDate3.AddMonths(16), 0m },
                                { resourceStartDate3.AddMonths(17), 0m },
                                { resourceStartDate3.AddMonths(18), 0m },
                                { resourceStartDate3.AddMonths(19), 0m },
                                { resourceStartDate3.AddMonths(20), 0m },
                                { resourceStartDate3.AddMonths(21), 0m },
                                { resourceStartDate3.AddMonths(22), 0m },
                                { resourceStartDate3.AddMonths(23), 0m },
                            }
                        },
                        { Element.ResourceNC, new Dictionary<DateTime, decimal>()
                            {
                                { resourceStartDateNC.AddMonths(12), 0m },
                                { resourceStartDateNC.AddMonths(13), 20m },
                                { resourceStartDateNC.AddMonths(14), 20m },
                                { resourceStartDateNC.AddMonths(15), 40m },
                                { resourceStartDateNC.AddMonths(16), 0m },
                                { resourceStartDateNC.AddMonths(17), 0m },
                                { resourceStartDateNC.AddMonths(18), 0m },
                                { resourceStartDateNC.AddMonths(19), 0m },
                                { resourceStartDateNC.AddMonths(20), 0m },
                                { resourceStartDateNC.AddMonths(21), 0m },
                                { resourceStartDateNC.AddMonths(22), 0m },
                                { resourceStartDateNC.AddMonths(23), 0m },
                                { resourceStartDateNC.AddMonths(24), 0m },
                                { resourceStartDateNC.AddMonths(25), 0m },
                                { resourceStartDateNC.AddMonths(26), 0m },
                                { resourceStartDateNC.AddMonths(27), 0m },
                                { resourceStartDateNC.AddMonths(28), 0m },
                                { resourceStartDateNC.AddMonths(29), 0m }
                            }
                        }
                    }
                });

                // workspace shift
                testCases.Add(new AdjustDateTestCase
                {
                    TestName = "Shift  and shorten workspace by 1 year level curve discrete ",
                    DateShiftLevel = Level.Workspace,
                    FlowdownType = DateAdjustFlowdownType.Automatic,
                    DiscreteType = DateAdjustDiscreteType.Curve,
                    StartDateShift = 12,
                    EndDateShift = 6,
                    ExpectedDateRange = new Dictionary<Element, Tuple<DateTime?, DateTime?>>()
                    {
                        { Element.Workspace, new Tuple<DateTime?, DateTime?>(contractStartDate.AddMonths(12), contractEndDate.AddMonths(6)) },
                        { Element.Clin1, new Tuple<DateTime?, DateTime?>(null, null) },
                        { Element.Clin2, new Tuple<DateTime?, DateTime?>(clinStartDate2.Value.AddMonths(12), clinEndDate2.Value.AddMonths(6)) },
                        { Element.Clin3, new Tuple<DateTime?, DateTime?>(clinStartDate3.Value.AddMonths(12), clinEndDate3.Value.AddMonths(6)) },
                        { Element.Boe1, new Tuple<DateTime?, DateTime?> (boeStartDate1.AddMonths(12), boeEndDate1.AddMonths(6)) },
                        { Element.Boe2, new Tuple<DateTime?, DateTime?> (boeStartDate2.AddMonths(12), boeEndDate2.AddMonths(6)) },
                        { Element.Boe3, new Tuple<DateTime?, DateTime?> (boeStartDate3.AddMonths(12), boeEndDate3.AddMonths(6)) },
                        { Element.BoeNC, new Tuple<DateTime?, DateTime?> (boeStartDateNC.AddMonths(12), boeEndDateNC.AddMonths(6)) },
                        { Element.Task1, new Tuple<DateTime?, DateTime?> (taskStartDate1.AddMonths(12), taskEndDate1.AddMonths(6)) },
                        { Element.Task2, new Tuple<DateTime?, DateTime?> (taskStartDate2.AddMonths(12), taskEndDate2.AddMonths(6)) },
                        { Element.Task3, new Tuple<DateTime?, DateTime?> (taskStartDate3.AddMonths(12), taskEndDate3.AddMonths(6)) },
                        { Element.TaskNC, new Tuple<DateTime?, DateTime?> (taskStartDateNC.AddMonths(12), taskEndDateNC.AddMonths(6)) },
                        { Element.Travel1, new Tuple<DateTime?, DateTime?> (taskStartDate1.AddMonths(12), taskEndDate1.AddMonths(6)) },
                        { Element.TravelTrip1, new Tuple<DateTime?, DateTime?> (taskStartDate1.AddMonths(12), null) },
                        { Element.Travel2, new Tuple<DateTime?, DateTime?> (taskStartDate2.AddMonths(12), taskEndDate2.AddMonths(6)) },
                        { Element.Travel3, new Tuple<DateTime?, DateTime?> (taskStartDate3.AddMonths(12), taskEndDate3.AddMonths(6)) },
                        { Element.TravelNC, new Tuple<DateTime?, DateTime?> (taskStartDateNC.AddMonths(12), taskEndDateNC.AddMonths(6)) },
                        { Element.Resource1, new Tuple<DateTime?, DateTime?> (resourceStartDate1.AddMonths(12), resourceEndDate1.AddMonths(6)) },
                        { Element.Resource2, new Tuple<DateTime?, DateTime?> (resourceStartDate2.AddMonths(12), resourceEndDate2.AddMonths(6)) },
                        { Element.Resource3, new Tuple<DateTime?, DateTime?> (resourceStartDate3.AddMonths(12), resourceEndDate3.AddMonths(6)) },
                        { Element.ResourceNC, new Tuple<DateTime?, DateTime?> (resourceStartDateNC.AddMonths(12), resourceEndDateNC.AddMonths(6)) }
                    },
                    ExpectedSpreadResults = new Dictionary<Element, Dictionary<DateTime, decimal>>()
                    {
                        { Element.Resource1, new Dictionary<DateTime, decimal>()
                            {
                                { resourceStartDate1.AddMonths(12), 0.04m },
                                { resourceStartDate1.AddMonths(13), 0.19m },
                                { resourceStartDate1.AddMonths(14), 0.35m },
                                { resourceStartDate1.AddMonths(15), 0.56m },
                                { resourceStartDate1.AddMonths(16), 0.84m },
                                { resourceStartDate1.AddMonths(17), 1.23m },
                                { resourceStartDate1.AddMonths(18), 1.76m },
                                { resourceStartDate1.AddMonths(19), 2.44m },
                                { resourceStartDate1.AddMonths(20), 3.24m },
                                { resourceStartDate1.AddMonths(21), 4.09m },
                                { resourceStartDate1.AddMonths(22), 4.82m },
                                { resourceStartDate1.AddMonths(23), 5.27m },
                                { resourceStartDate1.AddMonths(24), 5.31m },
                                { resourceStartDate1.AddMonths(25), 4.96m },
                                { resourceStartDate1.AddMonths(26), 4.43m },
                                { resourceStartDate1.AddMonths(27), 4.00m },
                                { resourceStartDate1.AddMonths(28), 4.15m },
                                { resourceStartDate1.AddMonths(29), 5.58m },
                                { resourceStartDate1.AddMonths(30), 9.02m },
                                { resourceStartDate1.AddMonths(31), 13.08m },
                                { resourceStartDate1.AddMonths(32), 12.76m },
                                { resourceStartDate1.AddMonths(33), 7.63m },
                                { resourceStartDate1.AddMonths(34), 3.16m },
                                { resourceStartDate1.AddMonths(35), 1.09m }
                            }
                        },
                        { Element.Resource2, new Dictionary<DateTime, decimal>()
                            {
                                { resourceStartDate2.AddMonths(12), 12m },
                                { resourceStartDate2.AddMonths(13), 11m },
                                { resourceStartDate2.AddMonths(14), 10m },
                                { resourceStartDate2.AddMonths(15), 10m },
                                { resourceStartDate2.AddMonths(16), 9m },
                                { resourceStartDate2.AddMonths(17), 8m },
                                { resourceStartDate2.AddMonths(18), 7m },
                                { resourceStartDate2.AddMonths(19), 7m },
                                { resourceStartDate2.AddMonths(20), 6m },
                                { resourceStartDate2.AddMonths(21), 5m },
                                { resourceStartDate2.AddMonths(22), 4m },
                                { resourceStartDate2.AddMonths(23), 4m },
                                { resourceStartDate2.AddMonths(24), 3m },
                                { resourceStartDate2.AddMonths(25), 2m },
                                { resourceStartDate2.AddMonths(26), 1m },
                                { resourceStartDate2.AddMonths(27), 1m }
                            }
                        },
                        { Element.Resource3, new Dictionary<DateTime, decimal>()
                            {
                                { resourceStartDate3.AddMonths(12), 8m },
                                { resourceStartDate3.AddMonths(13), 8m },
                                { resourceStartDate3.AddMonths(14), 8m },
                                { resourceStartDate3.AddMonths(15), 8m },
                                { resourceStartDate3.AddMonths(16), 9m },
                                { resourceStartDate3.AddMonths(17), 8m },
                                { resourceStartDate3.AddMonths(18), 8m },
                                { resourceStartDate3.AddMonths(19), 9m },
                                { resourceStartDate3.AddMonths(20), 8m },
                                { resourceStartDate3.AddMonths(21), 8m },
                                { resourceStartDate3.AddMonths(22), 9m },
                                { resourceStartDate3.AddMonths(23), 9m }
                            }
                        },
                        { Element.ResourceNC, new Dictionary<DateTime, decimal>()
                            {
                                { resourceStartDateNC.AddMonths(12), 5.56m },
                                { resourceStartDateNC.AddMonths(13), 5.56m },
                                { resourceStartDateNC.AddMonths(14), 5.55m },
                                { resourceStartDateNC.AddMonths(15), 5.56m },
                                { resourceStartDateNC.AddMonths(16), 5.55m },
                                { resourceStartDateNC.AddMonths(17), 5.56m },
                                { resourceStartDateNC.AddMonths(18), 5.55m },
                                { resourceStartDateNC.AddMonths(19), 5.56m },
                                { resourceStartDateNC.AddMonths(20), 5.56m },
                                { resourceStartDateNC.AddMonths(21), 5.55m },
                                { resourceStartDateNC.AddMonths(22), 5.56m },
                                { resourceStartDateNC.AddMonths(23), 5.55m },
                                { resourceStartDateNC.AddMonths(24), 5.56m },
                                { resourceStartDateNC.AddMonths(25), 5.55m },
                                { resourceStartDateNC.AddMonths(26), 5.56m },
                                { resourceStartDateNC.AddMonths(27), 5.55m },
                                { resourceStartDateNC.AddMonths(28), 5.56m },
                                { resourceStartDateNC.AddMonths(29), 5.55m }
                            }
                        }
                    }
                });

                // workspace shift
                testCases.Add(new AdjustDateTestCase
                {
                    TestName = "Shift and shorten workspace by 1 year First Discrete",
                    DateShiftLevel = Level.Workspace,
                    FlowdownType = DateAdjustFlowdownType.Automatic,
                    DiscreteType = DateAdjustDiscreteType.First,
                    StartDateShift = 12,
                    EndDateShift = 6,
                    ExpectedDateRange = new Dictionary<Element, Tuple<DateTime?, DateTime?>>()
                    {
                        { Element.Workspace, new Tuple<DateTime?, DateTime?>(contractStartDate.AddMonths(12), contractEndDate.AddMonths(6)) },
                        { Element.Clin1, new Tuple<DateTime?, DateTime?>(null, null) },
                        { Element.Clin2, new Tuple<DateTime?, DateTime?>(clinStartDate2.Value.AddMonths(12), clinEndDate2.Value.AddMonths(6)) },
                        { Element.Clin3, new Tuple<DateTime?, DateTime?>(clinStartDate3.Value.AddMonths(12), clinEndDate3.Value.AddMonths(6)) },
                        { Element.Boe1, new Tuple<DateTime?, DateTime?> (boeStartDate1.AddMonths(12), boeEndDate1.AddMonths(6)) },
                        { Element.Boe2, new Tuple<DateTime?, DateTime?> (boeStartDate2.AddMonths(12), boeEndDate2.AddMonths(6)) },
                        { Element.Boe3, new Tuple<DateTime?, DateTime?> (boeStartDate3.AddMonths(12), boeEndDate3.AddMonths(6)) },
                        { Element.BoeNC, new Tuple<DateTime?, DateTime?> (boeStartDateNC.AddMonths(12), boeEndDateNC.AddMonths(6)) },
                        { Element.Task1, new Tuple<DateTime?, DateTime?> (taskStartDate1.AddMonths(12), taskEndDate1.AddMonths(6)) },
                        { Element.Task2, new Tuple<DateTime?, DateTime?> (taskStartDate2.AddMonths(12), taskEndDate2.AddMonths(6)) },
                        { Element.Task3, new Tuple<DateTime?, DateTime?> (taskStartDate3.AddMonths(12), taskEndDate3.AddMonths(6)) },
                        { Element.TaskNC, new Tuple<DateTime?, DateTime?> (taskStartDateNC.AddMonths(12), taskEndDateNC.AddMonths(6)) },
                        { Element.Travel1, new Tuple<DateTime?, DateTime?> (taskStartDate1.AddMonths(12), taskEndDate1.AddMonths(6)) },
                        { Element.TravelTrip1, new Tuple<DateTime?, DateTime?> (taskStartDate1.AddMonths(12), null) },
                        { Element.Travel2, new Tuple<DateTime?, DateTime?> (taskStartDate2.AddMonths(12), taskEndDate2.AddMonths(6)) },
                        { Element.Travel3, new Tuple<DateTime?, DateTime?> (taskStartDate3.AddMonths(12), taskEndDate3.AddMonths(6)) },
                        { Element.TravelNC, new Tuple<DateTime?, DateTime?> (taskStartDateNC.AddMonths(12), taskEndDateNC.AddMonths(6)) },
                        { Element.Resource1, new Tuple<DateTime?, DateTime?> (resourceStartDate1.AddMonths(12), resourceEndDate1.AddMonths(6)) },
                        { Element.Resource2, new Tuple<DateTime?, DateTime?> (resourceStartDate2.AddMonths(12), resourceEndDate2.AddMonths(6)) },
                        { Element.Resource3, new Tuple<DateTime?, DateTime?> (resourceStartDate3.AddMonths(12), resourceEndDate3.AddMonths(6)) },
                        { Element.ResourceNC, new Tuple<DateTime?, DateTime?> (resourceStartDateNC.AddMonths(12), resourceEndDateNC.AddMonths(6)) }
                    },
                    ExpectedSpreadResults = new Dictionary<Element, Dictionary<DateTime, decimal>>()
                    {
                        { Element.Resource1, new Dictionary<DateTime, decimal>()
                            {
                                { resourceStartDate1.AddMonths(12), 0.04m },
                                { resourceStartDate1.AddMonths(13), 0.19m },
                                { resourceStartDate1.AddMonths(14), 0.35m },
                                { resourceStartDate1.AddMonths(15), 0.56m },
                                { resourceStartDate1.AddMonths(16), 0.84m },
                                { resourceStartDate1.AddMonths(17), 1.23m },
                                { resourceStartDate1.AddMonths(18), 1.76m },
                                { resourceStartDate1.AddMonths(19), 2.44m },
                                { resourceStartDate1.AddMonths(20), 3.24m },
                                { resourceStartDate1.AddMonths(21), 4.09m },
                                { resourceStartDate1.AddMonths(22), 4.82m },
                                { resourceStartDate1.AddMonths(23), 5.27m },
                                { resourceStartDate1.AddMonths(24), 5.31m },
                                { resourceStartDate1.AddMonths(25), 4.96m },
                                { resourceStartDate1.AddMonths(26), 4.43m },
                                { resourceStartDate1.AddMonths(27), 4.00m },
                                { resourceStartDate1.AddMonths(28), 4.15m },
                                { resourceStartDate1.AddMonths(29), 5.58m },
                                { resourceStartDate1.AddMonths(30), 9.02m },
                                { resourceStartDate1.AddMonths(31), 13.08m },
                                { resourceStartDate1.AddMonths(32), 12.76m },
                                { resourceStartDate1.AddMonths(33), 7.63m },
                                { resourceStartDate1.AddMonths(34), 3.16m },
                                { resourceStartDate1.AddMonths(35), 1.09m }
                            }
                        },
                        { Element.Resource2, new Dictionary<DateTime, decimal>()
                            {
                                { resourceStartDate2.AddMonths(12), 12m },
                                { resourceStartDate2.AddMonths(13), 11m },
                                { resourceStartDate2.AddMonths(14), 10m },
                                { resourceStartDate2.AddMonths(15), 10m },
                                { resourceStartDate2.AddMonths(16), 9m },
                                { resourceStartDate2.AddMonths(17), 8m },
                                { resourceStartDate2.AddMonths(18), 7m },
                                { resourceStartDate2.AddMonths(19), 7m },
                                { resourceStartDate2.AddMonths(20), 6m },
                                { resourceStartDate2.AddMonths(21), 5m },
                                { resourceStartDate2.AddMonths(22), 4m },
                                { resourceStartDate2.AddMonths(23), 4m },
                                { resourceStartDate2.AddMonths(24), 3m },
                                { resourceStartDate2.AddMonths(25), 2m },
                                { resourceStartDate2.AddMonths(26), 1m },
                                { resourceStartDate2.AddMonths(27), 1m }
                            }
                        },
                        { Element.Resource3, new Dictionary<DateTime, decimal>()
                            {
                                { resourceStartDate3.AddMonths(12), 50m },
                                { resourceStartDate3.AddMonths(13), 20m },
                                { resourceStartDate3.AddMonths(14), 30m },
                                { resourceStartDate3.AddMonths(15), 0m },
                                { resourceStartDate3.AddMonths(16), 0m },
                                { resourceStartDate3.AddMonths(17), 0m },
                                { resourceStartDate3.AddMonths(18), 0m },
                                { resourceStartDate3.AddMonths(19), 0m },
                                { resourceStartDate3.AddMonths(20), 0m },
                                { resourceStartDate3.AddMonths(21), 0m },
                                { resourceStartDate3.AddMonths(22), 0m },
                                { resourceStartDate3.AddMonths(23), 0m }
                            }
                        },
                        { Element.ResourceNC, new Dictionary<DateTime, decimal>()
                            {
                                { resourceStartDateNC.AddMonths(12), 20m },
                                { resourceStartDateNC.AddMonths(13), 20m },
                                { resourceStartDateNC.AddMonths(14), 20m },
                                { resourceStartDateNC.AddMonths(15), 40m },
                                { resourceStartDateNC.AddMonths(16), 0m },
                                { resourceStartDateNC.AddMonths(17), 0m },
                                { resourceStartDateNC.AddMonths(18), 0m },
                                { resourceStartDateNC.AddMonths(19), 0m },
                                { resourceStartDateNC.AddMonths(20), 0m },
                                { resourceStartDateNC.AddMonths(21), 0m },
                                { resourceStartDateNC.AddMonths(22), 0m },
                                { resourceStartDateNC.AddMonths(23), 0m },
                                { resourceStartDateNC.AddMonths(24), 0m },
                                { resourceStartDateNC.AddMonths(25), 0m },
                                { resourceStartDateNC.AddMonths(26), 0m },
                                { resourceStartDateNC.AddMonths(27), 0m },
                                { resourceStartDateNC.AddMonths(28), 0m },
                                { resourceStartDateNC.AddMonths(29), 0m }
                            }
                        }
                    }
                });

                // workspace shift
                testCases.Add(new AdjustDateTestCase
                {
                    TestName = "Shift and shorten workspace by 1 year Last Discrete",
                    DateShiftLevel = Level.Workspace,
                    FlowdownType = DateAdjustFlowdownType.Automatic,
                    DiscreteType = DateAdjustDiscreteType.Last,
                    StartDateShift = 12,
                    EndDateShift = 6,
                    ExpectedDateRange = new Dictionary<Element, Tuple<DateTime?, DateTime?>>()
                    {
                        { Element.Workspace, new Tuple<DateTime?, DateTime?>(contractStartDate.AddMonths(12), contractEndDate.AddMonths(6)) },
                        { Element.Clin1, new Tuple<DateTime?, DateTime?>(null, null) },
                        { Element.Clin2, new Tuple<DateTime?, DateTime?>(clinStartDate2.Value.AddMonths(12), clinEndDate2.Value.AddMonths(6)) },
                        { Element.Clin3, new Tuple<DateTime?, DateTime?>(clinStartDate3.Value.AddMonths(12), clinEndDate3.Value.AddMonths(6)) },
                        { Element.Boe1, new Tuple<DateTime?, DateTime?> (boeStartDate1.AddMonths(12), boeEndDate1.AddMonths(6)) },
                        { Element.Boe2, new Tuple<DateTime?, DateTime?> (boeStartDate2.AddMonths(12), boeEndDate2.AddMonths(6)) },
                        { Element.Boe3, new Tuple<DateTime?, DateTime?> (boeStartDate3.AddMonths(12), boeEndDate3.AddMonths(6)) },
                        { Element.BoeNC, new Tuple<DateTime?, DateTime?> (boeStartDateNC.AddMonths(12), boeEndDateNC.AddMonths(6)) },
                        { Element.Task1, new Tuple<DateTime?, DateTime?> (taskStartDate1.AddMonths(12), taskEndDate1.AddMonths(6)) },
                        { Element.Task2, new Tuple<DateTime?, DateTime?> (taskStartDate2.AddMonths(12), taskEndDate2.AddMonths(6)) },
                        { Element.Task3, new Tuple<DateTime?, DateTime?> (taskStartDate3.AddMonths(12), taskEndDate3.AddMonths(6)) },
                        { Element.TaskNC, new Tuple<DateTime?, DateTime?> (taskStartDateNC.AddMonths(12), taskEndDateNC.AddMonths(6)) },
                        { Element.Travel1, new Tuple<DateTime?, DateTime?> (taskStartDate1.AddMonths(12), taskEndDate1.AddMonths(6)) },
                        { Element.TravelTrip1, new Tuple<DateTime?, DateTime?> (taskStartDate1.AddMonths(12), null) },
                        { Element.Travel2, new Tuple<DateTime?, DateTime?> (taskStartDate2.AddMonths(12), taskEndDate2.AddMonths(6)) },
                        { Element.Travel3, new Tuple<DateTime?, DateTime?> (taskStartDate3.AddMonths(12), taskEndDate3.AddMonths(6)) },
                        { Element.TravelNC, new Tuple<DateTime?, DateTime?> (taskStartDateNC.AddMonths(12), taskEndDateNC.AddMonths(6)) },
                        { Element.Resource1, new Tuple<DateTime?, DateTime?> (resourceStartDate1.AddMonths(12), resourceEndDate1.AddMonths(6)) },
                        { Element.Resource2, new Tuple<DateTime?, DateTime?> (resourceStartDate2.AddMonths(12), resourceEndDate2.AddMonths(6)) },
                        { Element.Resource3, new Tuple<DateTime?, DateTime?> (resourceStartDate3.AddMonths(12), resourceEndDate3.AddMonths(6)) },
                        { Element.ResourceNC, new Tuple<DateTime?, DateTime?> (resourceStartDateNC.AddMonths(12), resourceEndDateNC.AddMonths(6)) }
                    },
                    ExpectedSpreadResults = new Dictionary<Element, Dictionary<DateTime, decimal>>()
                    {
                        { Element.Resource1, new Dictionary<DateTime, decimal>()
                            {
                                { resourceStartDate1.AddMonths(12), 0.04m },
                                { resourceStartDate1.AddMonths(13), 0.19m },
                                { resourceStartDate1.AddMonths(14), 0.35m },
                                { resourceStartDate1.AddMonths(15), 0.56m },
                                { resourceStartDate1.AddMonths(16), 0.84m },
                                { resourceStartDate1.AddMonths(17), 1.23m },
                                { resourceStartDate1.AddMonths(18), 1.76m },
                                { resourceStartDate1.AddMonths(19), 2.44m },
                                { resourceStartDate1.AddMonths(20), 3.24m },
                                { resourceStartDate1.AddMonths(21), 4.09m },
                                { resourceStartDate1.AddMonths(22), 4.82m },
                                { resourceStartDate1.AddMonths(23), 5.27m },
                                { resourceStartDate1.AddMonths(24), 5.31m },
                                { resourceStartDate1.AddMonths(25), 4.96m },
                                { resourceStartDate1.AddMonths(26), 4.43m },
                                { resourceStartDate1.AddMonths(27), 4.00m },
                                { resourceStartDate1.AddMonths(28), 4.15m },
                                { resourceStartDate1.AddMonths(29), 5.58m },
                                { resourceStartDate1.AddMonths(30), 9.02m },
                                { resourceStartDate1.AddMonths(31), 13.08m },
                                { resourceStartDate1.AddMonths(32), 12.76m },
                                { resourceStartDate1.AddMonths(33), 7.63m },
                                { resourceStartDate1.AddMonths(34), 3.16m },
                                { resourceStartDate1.AddMonths(35), 1.09m }
                            }
                        },
                        { Element.Resource2, new Dictionary<DateTime, decimal>()
                            {
                                { resourceStartDate2.AddMonths(12), 12m },
                                { resourceStartDate2.AddMonths(13), 11m },
                                { resourceStartDate2.AddMonths(14), 10m },
                                { resourceStartDate2.AddMonths(15), 10m },
                                { resourceStartDate2.AddMonths(16), 9m },
                                { resourceStartDate2.AddMonths(17), 8m },
                                { resourceStartDate2.AddMonths(18), 7m },
                                { resourceStartDate2.AddMonths(19), 7m },
                                { resourceStartDate2.AddMonths(20), 6m },
                                { resourceStartDate2.AddMonths(21), 5m },
                                { resourceStartDate2.AddMonths(22), 4m },
                                { resourceStartDate2.AddMonths(23), 4m },
                                { resourceStartDate2.AddMonths(24), 3m },
                                { resourceStartDate2.AddMonths(25), 2m },
                                { resourceStartDate2.AddMonths(26), 1m },
                                { resourceStartDate2.AddMonths(27), 1m }
                            }
                        },
                        { Element.Resource3, new Dictionary<DateTime, decimal>()
                            {
                                { resourceStartDate3.AddMonths(12), 10m },
                                { resourceStartDate3.AddMonths(13), 20m },
                                { resourceStartDate3.AddMonths(14), 30m },
                                { resourceStartDate3.AddMonths(15), 0m },
                                { resourceStartDate3.AddMonths(16), 0m },
                                { resourceStartDate3.AddMonths(17), 0m },
                                { resourceStartDate3.AddMonths(18), 0m },
                                { resourceStartDate3.AddMonths(19), 0m },
                                { resourceStartDate3.AddMonths(20), 0m },
                                { resourceStartDate3.AddMonths(21), 0m },
                                { resourceStartDate3.AddMonths(22), 0m },
                                { resourceStartDate3.AddMonths(23), 40m }
                            }
                        },
                        { Element.ResourceNC, new Dictionary<DateTime, decimal>()
                            {
                                { resourceStartDateNC.AddMonths(12), 0m },
                                { resourceStartDateNC.AddMonths(13), 20m },
                                { resourceStartDateNC.AddMonths(14), 20m },
                                { resourceStartDateNC.AddMonths(15), 40m },
                                { resourceStartDateNC.AddMonths(16), 0m },
                                { resourceStartDateNC.AddMonths(17), 0m },
                                { resourceStartDateNC.AddMonths(18), 0m },
                                { resourceStartDateNC.AddMonths(19), 0m },
                                { resourceStartDateNC.AddMonths(20), 0m },
                                { resourceStartDateNC.AddMonths(21), 0m },
                                { resourceStartDateNC.AddMonths(22), 0m },
                                { resourceStartDateNC.AddMonths(23), 0m },
                                { resourceStartDateNC.AddMonths(24), 0m },
                                { resourceStartDateNC.AddMonths(25), 0m },
                                { resourceStartDateNC.AddMonths(26), 0m },
                                { resourceStartDateNC.AddMonths(27), 0m },
                                { resourceStartDateNC.AddMonths(28), 0m },
                                { resourceStartDateNC.AddMonths(29), 20m }
                            }
                        }
                    }
                });

                testCases.Add(new AdjustDateTestCase
                {
                    TestName = "Workspace shift without flowdown",
                    DateShiftLevel = Level.Workspace,
                    FlowdownType = DateAdjustFlowdownType.NoChange,
                    DiscreteType = DateAdjustDiscreteType.NotSet,
                    StartDateShift = 16,
                    EndDateShift = 16,
                    ExpectedDateRange = new Dictionary<Element, Tuple<DateTime?, DateTime?>>()
                    {
                        { Element.Workspace, new Tuple<DateTime?, DateTime?>(contractStartDate.AddMonths(16), contractEndDate.AddMonths(16)) },
                        { Element.Clin1, new Tuple<DateTime?, DateTime?>(null, null) },
                        { Element.Clin2, new Tuple<DateTime?, DateTime?>(clinStartDate2.Value, clinEndDate2.Value) },
                        { Element.Clin3, new Tuple<DateTime?, DateTime?>(clinStartDate3.Value, clinEndDate3.Value) }
                    },
                    ExpectedSpreadResults = new Dictionary<Element, Dictionary<DateTime, decimal>>()
                    {
                        { Element.Resource1, new Dictionary<DateTime, decimal>()
                            {
                                { resourceStartDate1, 0.02m },
                                { resourceStartDate1.AddMonths(1), 0.12m },
                                { resourceStartDate1.AddMonths(2), 0.21m },
                                { resourceStartDate1.AddMonths(3), 0.32m },
                                { resourceStartDate1.AddMonths(4), 0.46m },
                                { resourceStartDate1.AddMonths(5), 0.64m },
                                { resourceStartDate1.AddMonths(6), 0.88m },
                                { resourceStartDate1.AddMonths(7), 1.18m },
                                { resourceStartDate1.AddMonths(8), 1.56m },
                                { resourceStartDate1.AddMonths(9), 2.01m },
                                { resourceStartDate1.AddMonths(10), 2.53m },
                                { resourceStartDate1.AddMonths(11), 3.07m },
                                { resourceStartDate1.AddMonths(12), 3.59m },
                                { resourceStartDate1.AddMonths(13), 4.00m },
                                { resourceStartDate1.AddMonths(14), 4.24m },
                                { resourceStartDate1.AddMonths(15), 4.26m },
                                { resourceStartDate1.AddMonths(16), 4.08m },
                                { resourceStartDate1.AddMonths(17), 3.77m },
                                { resourceStartDate1.AddMonths(18), 3.42m },
                                { resourceStartDate1.AddMonths(19), 3.18m },
                                { resourceStartDate1.AddMonths(20), 3.26m },
                                { resourceStartDate1.AddMonths(21), 3.94m },
                                { resourceStartDate1.AddMonths(22), 5.61m },
                                { resourceStartDate1.AddMonths(23), 8.29m },
                                { resourceStartDate1.AddMonths(24), 10.73m },
                                { resourceStartDate1.AddMonths(25), 10.54m },
                                { resourceStartDate1.AddMonths(26), 7.47m },
                                { resourceStartDate1.AddMonths(27), 4.03m },
                                { resourceStartDate1.AddMonths(28), 1.83m },
                                { resourceStartDate1.AddMonths(29), 0.76m },
                            }
                        },
                        { Element.Resource2, new Dictionary<DateTime, decimal>()
                            {
                                { resourceStartDate2, 9m },
                                { resourceStartDate2.AddMonths(1), 8m },
                                { resourceStartDate2.AddMonths(2), 8m },
                                { resourceStartDate2.AddMonths(3), 7m },
                                { resourceStartDate2.AddMonths(4), 7m },
                                { resourceStartDate2.AddMonths(5), 7m },
                                { resourceStartDate2.AddMonths(6), 6m },
                                { resourceStartDate2.AddMonths(7), 6m },
                                { resourceStartDate2.AddMonths(8), 6m },
                                { resourceStartDate2.AddMonths(9), 5m },
                                { resourceStartDate2.AddMonths(10), 5m },
                                { resourceStartDate2.AddMonths(11), 4m },
                                { resourceStartDate2.AddMonths(12), 4m },
                                { resourceStartDate2.AddMonths(13), 4m },
                                { resourceStartDate2.AddMonths(14), 3m },
                                { resourceStartDate2.AddMonths(15), 3m },
                                { resourceStartDate2.AddMonths(16), 2m },
                                { resourceStartDate2.AddMonths(17), 2m },
                                { resourceStartDate2.AddMonths(18), 2m },
                                { resourceStartDate2.AddMonths(19), 1m },
                                { resourceStartDate2.AddMonths(20), 1m },
                                { resourceStartDate2.AddMonths(21), 0m }
                            }
                        },
                        { Element.Resource3, new Dictionary<DateTime, decimal>()
                            {
                                { resourceStartDate3, 10m },
                                { resourceStartDate3.AddMonths(1), 20m },
                                { resourceStartDate3.AddMonths(2), 30m },
                                { resourceStartDate3.AddMonths(3), 0m },
                                { resourceStartDate3.AddMonths(4), 0m },
                                { resourceStartDate3.AddMonths(5), 0m },
                                { resourceStartDate3.AddMonths(6), 0m },
                                { resourceStartDate3.AddMonths(7), 0m },
                                { resourceStartDate3.AddMonths(8), 0m },
                                { resourceStartDate3.AddMonths(9), 0m },
                                { resourceStartDate3.AddMonths(10), 0m },
                                { resourceStartDate3.AddMonths(11), 0m },
                                { resourceStartDate3.AddMonths(12), 0m },
                                { resourceStartDate3.AddMonths(13), 0m },
                                { resourceStartDate3.AddMonths(14), 0m },
                                { resourceStartDate3.AddMonths(15), 0m },
                                { resourceStartDate3.AddMonths(16), 10m },
                                { resourceStartDate3.AddMonths(17), 30m }
                            }
                        },
                        { Element.ResourceNC, new Dictionary<DateTime, decimal>()
                            {
                                { resourceStartDateNC, 0m },
                                { resourceStartDateNC.AddMonths(1), 20m },
                                { resourceStartDateNC.AddMonths(2), 20m },
                                { resourceStartDateNC.AddMonths(3), 40m },
                                { resourceStartDateNC.AddMonths(4), 0m },
                                { resourceStartDateNC.AddMonths(5), 0m },
                                { resourceStartDateNC.AddMonths(6), 0m },
                                { resourceStartDateNC.AddMonths(7), 0m },
                                { resourceStartDateNC.AddMonths(8), 0m },
                                { resourceStartDateNC.AddMonths(9), 0m },
                                { resourceStartDateNC.AddMonths(10), 0m },
                                { resourceStartDateNC.AddMonths(11), 0m },
                                { resourceStartDateNC.AddMonths(12), 0m },
                                { resourceStartDateNC.AddMonths(13), 0m },
                                { resourceStartDateNC.AddMonths(14), 0m },
                                { resourceStartDateNC.AddMonths(15), 0m },
                                { resourceStartDateNC.AddMonths(16), 0m },
                                { resourceStartDateNC.AddMonths(17), 0m },
                                { resourceStartDateNC.AddMonths(18), 0m },
                                { resourceStartDateNC.AddMonths(19), 0m },
                                { resourceStartDateNC.AddMonths(20), 0m },
                                { resourceStartDateNC.AddMonths(21), 0m },
                                { resourceStartDateNC.AddMonths(22), 20m },
                                { resourceStartDateNC.AddMonths(23), 0m }
                            }
                        }
                    }
                });

                // workspace expand
                testCases.Add(new AdjustDateTestCase
                {
                    TestName = "Expand Workspace",
                    DateShiftLevel = Level.Workspace,
                    FlowdownType = DateAdjustFlowdownType.Manual,
                    DiscreteType = DateAdjustDiscreteType.NotSet,
                    StartDateShift = -6,
                    EndDateShift = 6,
                    ExpectedDateRange = new Dictionary<Element, Tuple<DateTime?, DateTime?>>()
                    {
                        { Element.Workspace, new Tuple<DateTime?, DateTime?>(contractStartDate.AddMonths(-6), contractEndDate.AddMonths(6)) },
                        { Element.Clin1, new Tuple<DateTime?, DateTime?>(null, null) },
                        { Element.Clin2, new Tuple<DateTime?, DateTime?>(clinStartDate2.Value, clinEndDate2.Value) },
                        { Element.Clin3, new Tuple<DateTime?, DateTime?>(clinStartDate3.Value, clinEndDate3.Value) },
                        { Element.Boe1, new Tuple<DateTime?, DateTime?> (boeStartDate1, boeEndDate1) },
                        { Element.Boe2, new Tuple<DateTime?, DateTime?> (boeStartDate2, boeEndDate2) },
                        { Element.Boe3, new Tuple<DateTime?, DateTime?> (boeStartDate3, boeEndDate3) },
                        { Element.BoeNC, new Tuple<DateTime?, DateTime?> (boeStartDateNC, boeEndDateNC) },
                        { Element.Task1, new Tuple<DateTime?, DateTime?> (taskStartDate1, taskEndDate1) },
                        { Element.Task2, new Tuple<DateTime?, DateTime?> (taskStartDate2, taskEndDate2) },
                        { Element.Task3, new Tuple<DateTime?, DateTime?> (taskStartDate3, taskEndDate3) },
                        { Element.TaskNC, new Tuple<DateTime?, DateTime?> (taskStartDateNC, taskEndDateNC) },
                        { Element.Travel1, new Tuple<DateTime?, DateTime?> (taskStartDate1, taskEndDate1) },
                        { Element.TravelTrip1, new Tuple<DateTime?, DateTime?> (taskStartDate1, null) },
                        { Element.Travel2, new Tuple<DateTime?, DateTime?> (taskStartDate2, taskEndDate2) },
                        { Element.Travel3, new Tuple<DateTime?, DateTime?> (taskStartDate3, taskEndDate3) },
                        { Element.TravelNC, new Tuple<DateTime?, DateTime?> (taskStartDateNC, taskEndDateNC) }
                    },
                    ExpectedSpreadResults = new Dictionary<Element, Dictionary<DateTime, decimal>>()
                    {
                        { Element.Resource1, new Dictionary<DateTime, decimal>()
                            {
                                { resourceStartDate1, 0.02m },
                                { resourceStartDate1.AddMonths(1), 0.12m },
                                { resourceStartDate1.AddMonths(2), 0.21m },
                                { resourceStartDate1.AddMonths(3), 0.32m },
                                { resourceStartDate1.AddMonths(4), 0.46m },
                                { resourceStartDate1.AddMonths(5), 0.64m },
                                { resourceStartDate1.AddMonths(6), 0.88m },
                                { resourceStartDate1.AddMonths(7), 1.18m },
                                { resourceStartDate1.AddMonths(8), 1.56m },
                                { resourceStartDate1.AddMonths(9), 2.01m },
                                { resourceStartDate1.AddMonths(10), 2.53m },
                                { resourceStartDate1.AddMonths(11), 3.07m },
                                { resourceStartDate1.AddMonths(12), 3.59m },
                                { resourceStartDate1.AddMonths(13), 4.00m },
                                { resourceStartDate1.AddMonths(14), 4.24m },
                                { resourceStartDate1.AddMonths(15), 4.26m },
                                { resourceStartDate1.AddMonths(16), 4.08m },
                                { resourceStartDate1.AddMonths(17), 3.77m },
                                { resourceStartDate1.AddMonths(18), 3.42m },
                                { resourceStartDate1.AddMonths(19), 3.18m },
                                { resourceStartDate1.AddMonths(20), 3.26m },
                                { resourceStartDate1.AddMonths(21), 3.94m },
                                { resourceStartDate1.AddMonths(22), 5.61m },
                                { resourceStartDate1.AddMonths(23), 8.29m },
                                { resourceStartDate1.AddMonths(24), 10.73m },
                                { resourceStartDate1.AddMonths(25), 10.54m },
                                { resourceStartDate1.AddMonths(26), 7.47m },
                                { resourceStartDate1.AddMonths(27), 4.03m },
                                { resourceStartDate1.AddMonths(28), 1.83m },
                                { resourceStartDate1.AddMonths(29), 0.76m },
                            }
                        },
                        { Element.Resource2, new Dictionary<DateTime, decimal>()
                            {
                                { resourceStartDate2, 9m },
                                { resourceStartDate2.AddMonths(1), 8m },
                                { resourceStartDate2.AddMonths(2), 8m },
                                { resourceStartDate2.AddMonths(3), 7m },
                                { resourceStartDate2.AddMonths(4), 7m },
                                { resourceStartDate2.AddMonths(5), 7m },
                                { resourceStartDate2.AddMonths(6), 6m },
                                { resourceStartDate2.AddMonths(7), 6m },
                                { resourceStartDate2.AddMonths(8), 6m },
                                { resourceStartDate2.AddMonths(9), 5m },
                                { resourceStartDate2.AddMonths(10), 5m },
                                { resourceStartDate2.AddMonths(11), 4m },
                                { resourceStartDate2.AddMonths(12), 4m },
                                { resourceStartDate2.AddMonths(13), 4m },
                                { resourceStartDate2.AddMonths(14), 3m },
                                { resourceStartDate2.AddMonths(15), 3m },
                                { resourceStartDate2.AddMonths(16), 2m },
                                { resourceStartDate2.AddMonths(17), 2m },
                                { resourceStartDate2.AddMonths(18), 2m },
                                { resourceStartDate2.AddMonths(19), 1m },
                                { resourceStartDate2.AddMonths(20), 1m },
                                { resourceStartDate2.AddMonths(21), 0m }
                            }
                        },{ Element.Resource3, new Dictionary<DateTime, decimal>()
                            {
                                { resourceStartDate3, 10m },
                                { resourceStartDate3.AddMonths(1), 20m },
                                { resourceStartDate3.AddMonths(2), 30m },
                                { resourceStartDate3.AddMonths(3), 0m },
                                { resourceStartDate3.AddMonths(4), 0m },
                                { resourceStartDate3.AddMonths(5), 0m },
                                { resourceStartDate3.AddMonths(6), 0m },
                                { resourceStartDate3.AddMonths(7), 0m },
                                { resourceStartDate3.AddMonths(8), 0m },
                                { resourceStartDate3.AddMonths(9), 0m },
                                { resourceStartDate3.AddMonths(10), 0m },
                                { resourceStartDate3.AddMonths(11), 0m },
                                { resourceStartDate3.AddMonths(12), 0m },
                                { resourceStartDate3.AddMonths(13), 0m },
                                { resourceStartDate3.AddMonths(14), 0m },
                                { resourceStartDate3.AddMonths(15), 0m },
                                { resourceStartDate3.AddMonths(16), 10m },
                                { resourceStartDate3.AddMonths(17), 30m }
                            }
                        },
                        { Element.ResourceNC, new Dictionary<DateTime, decimal>()
                            {
                                { resourceStartDateNC, 0m },
                                { resourceStartDateNC.AddMonths(1), 20m },
                                { resourceStartDateNC.AddMonths(2), 20m },
                                { resourceStartDateNC.AddMonths(3), 40m },
                                { resourceStartDateNC.AddMonths(4), 0m },
                                { resourceStartDateNC.AddMonths(5), 0m },
                                { resourceStartDateNC.AddMonths(6), 0m },
                                { resourceStartDateNC.AddMonths(7), 0m },
                                { resourceStartDateNC.AddMonths(8), 0m },
                                { resourceStartDateNC.AddMonths(9), 0m },
                                { resourceStartDateNC.AddMonths(10), 0m },
                                { resourceStartDateNC.AddMonths(11), 0m },
                                { resourceStartDateNC.AddMonths(12), 0m },
                                { resourceStartDateNC.AddMonths(13), 0m },
                                { resourceStartDateNC.AddMonths(14), 0m },
                                { resourceStartDateNC.AddMonths(15), 0m },
                                { resourceStartDateNC.AddMonths(16), 0m },
                                { resourceStartDateNC.AddMonths(17), 0m },
                                { resourceStartDateNC.AddMonths(18), 0m },
                                { resourceStartDateNC.AddMonths(19), 0m },
                                { resourceStartDateNC.AddMonths(20), 0m },
                                { resourceStartDateNC.AddMonths(21), 0m },
                                { resourceStartDateNC.AddMonths(22), 20m },
                                { resourceStartDateNC.AddMonths(23), 0m }
                            }
                        }
                    }
                });

                // workspace shrink
                testCases.Add(new AdjustDateTestCase
                {
                    TestName = "Shrink Workspace",
                    DateShiftLevel = Level.Workspace,
                    FlowdownType = DateAdjustFlowdownType.Automatic,
                    DiscreteType = DateAdjustDiscreteType.NotSet,
                    StartDateShift = 3,
                    EndDateShift = -3,
                    ExpectedDateRange = new Dictionary<Element, Tuple<DateTime?, DateTime?>>()
                    {
                        { Element.Workspace, new Tuple<DateTime?, DateTime?>(contractStartDate.AddMonths(3), contractEndDate.AddMonths(-3)) },
                        { Element.Clin1, new Tuple<DateTime?, DateTime?>(null, null) },
                        { Element.Clin2, new Tuple<DateTime?, DateTime?>(clinStartDate2.Value.AddMonths(3), clinEndDate2.Value.AddMonths(-3)) },
                        { Element.Clin3, new Tuple<DateTime?, DateTime?>(clinStartDate3.Value.AddMonths(3), clinEndDate3.Value.AddMonths(-3)) },
                        { Element.Boe1, new Tuple<DateTime?, DateTime?> (boeStartDate1.AddMonths(3), boeEndDate1.AddMonths(-3)) },
                        { Element.Boe2, new Tuple<DateTime?, DateTime?> (boeStartDate2.AddMonths(3), boeEndDate2.AddMonths(-3)) },
                        { Element.Boe3, new Tuple<DateTime?, DateTime?> (boeStartDate3.AddMonths(3), boeEndDate3.AddMonths(-3)) },
                        { Element.BoeNC, new Tuple<DateTime?, DateTime?> (boeStartDateNC.AddMonths(3), boeEndDateNC.AddMonths(-3)) },
                        { Element.Task1, new Tuple<DateTime?, DateTime?> (taskStartDate1.AddMonths(3), taskEndDate1.AddMonths(-3)) },
                        { Element.Task2, new Tuple<DateTime?, DateTime?> (taskStartDate2.AddMonths(3), taskEndDate2.AddMonths(-3)) },
                        { Element.Task3, new Tuple<DateTime?, DateTime?> (taskStartDate3.AddMonths(3), taskEndDate3.AddMonths(-3)) },
                        { Element.TaskNC, new Tuple<DateTime?, DateTime?> (taskStartDateNC.AddMonths(3), taskEndDateNC.AddMonths(-3)) },
                        { Element.Travel1, new Tuple<DateTime?, DateTime?> (taskStartDate1.AddMonths(3), taskEndDate1.AddMonths(-3)) },
                        { Element.TravelTrip1, new Tuple<DateTime?, DateTime?> (taskStartDate1.AddMonths(3), null) },
                        { Element.Travel2, new Tuple<DateTime?, DateTime?> (taskStartDate2.AddMonths(3), taskEndDate2.AddMonths(-3)) },
                        { Element.Travel3, new Tuple<DateTime?, DateTime?> (taskStartDate3.AddMonths(3), taskEndDate3.AddMonths(-3)) },
                        { Element.TravelNC, new Tuple<DateTime?, DateTime?> (taskStartDateNC.AddMonths(3), taskEndDateNC.AddMonths(-3)) },
                        { Element.Resource1, new Tuple<DateTime?, DateTime?> (resourceStartDate1.AddMonths(3), resourceEndDate1.AddMonths(-3)) },
                        { Element.Resource2, new Tuple<DateTime?, DateTime?> (resourceStartDate2.AddMonths(3), resourceEndDate2.AddMonths(-3)) },
                        { Element.Resource3, new Tuple<DateTime?, DateTime?> (resourceStartDate3.AddMonths(3), resourceEndDate3.AddMonths(-3)) },
                        { Element.ResourceNC, new Tuple<DateTime?, DateTime?> (resourceStartDateNC.AddMonths(3), resourceEndDateNC.AddMonths(-3)) }
                    },
                    ExpectedSpreadResults = new Dictionary<Element, Dictionary<DateTime, decimal>>()
                    {
                        { Element.Resource1, new Dictionary<DateTime, decimal>()
                            {
                                { resourceStartDate1.AddMonths(3), 0.04m },
                                { resourceStartDate1.AddMonths(4), 0.19m },
                                { resourceStartDate1.AddMonths(5), 0.35m },
                                { resourceStartDate1.AddMonths(6), 0.56m },
                                { resourceStartDate1.AddMonths(7), 0.84m },
                                { resourceStartDate1.AddMonths(8), 1.23m },
                                { resourceStartDate1.AddMonths(9), 1.76m },
                                { resourceStartDate1.AddMonths(10), 2.44m },
                                { resourceStartDate1.AddMonths(11), 3.24m },
                                { resourceStartDate1.AddMonths(12), 4.09m },
                                { resourceStartDate1.AddMonths(13), 4.82m },
                                { resourceStartDate1.AddMonths(14), 5.27m },
                                { resourceStartDate1.AddMonths(15), 5.31m },
                                { resourceStartDate1.AddMonths(16), 4.96m },
                                { resourceStartDate1.AddMonths(17), 4.43m },
                                { resourceStartDate1.AddMonths(18), 4.00m },
                                { resourceStartDate1.AddMonths(19), 4.15m },
                                { resourceStartDate1.AddMonths(20), 5.58m },
                                { resourceStartDate1.AddMonths(21), 9.02m },
                                { resourceStartDate1.AddMonths(22), 13.08m },
                                { resourceStartDate1.AddMonths(23), 12.76m },
                                { resourceStartDate1.AddMonths(24), 7.63m },
                                { resourceStartDate1.AddMonths(25), 3.16m },
                                { resourceStartDate1.AddMonths(26), 1.09m }
                            }
                        },
                        { Element.Resource2, new Dictionary<DateTime, decimal>()
                            {
                                { resourceStartDate2.AddMonths(3), 12m },
                                { resourceStartDate2.AddMonths(4), 11m },
                                { resourceStartDate2.AddMonths(5), 10m },
                                { resourceStartDate2.AddMonths(6), 10m },
                                { resourceStartDate2.AddMonths(7), 9m },
                                { resourceStartDate2.AddMonths(8), 8m },
                                { resourceStartDate2.AddMonths(9), 7m },
                                { resourceStartDate2.AddMonths(10), 7m },
                                { resourceStartDate2.AddMonths(11), 6m },
                                { resourceStartDate2.AddMonths(12), 5m },
                                { resourceStartDate2.AddMonths(13), 4m },
                                { resourceStartDate2.AddMonths(14), 4m },
                                { resourceStartDate2.AddMonths(15), 3m },
                                { resourceStartDate2.AddMonths(16), 2m },
                                { resourceStartDate2.AddMonths(17), 1m },
                                { resourceStartDate2.AddMonths(18), 1m }
                            }
                        },{ Element.Resource3, new Dictionary<DateTime, decimal>()
                            {
                                { resourceStartDate3.AddMonths(3), 10m },
                                { resourceStartDate3.AddMonths(4), 20m },
                                { resourceStartDate3.AddMonths(5), 30m },
                                { resourceStartDate3.AddMonths(6), 0m },
                                { resourceStartDate3.AddMonths(7), 0m },
                                { resourceStartDate3.AddMonths(8), 0m },
                                { resourceStartDate3.AddMonths(9), 0m },
                                { resourceStartDate3.AddMonths(10), 0m },
                                { resourceStartDate3.AddMonths(11), 0m },
                                { resourceStartDate3.AddMonths(12), 0m },
                                { resourceStartDate3.AddMonths(13), 0m },
                                { resourceStartDate3.AddMonths(14), 0m }
                            }
                        },
                        { Element.ResourceNC, new Dictionary<DateTime, decimal>()
                            {
                                { resourceStartDateNC.AddMonths(3), 0m },
                                { resourceStartDateNC.AddMonths(4), 20m },
                                { resourceStartDateNC.AddMonths(5), 20m },
                                { resourceStartDateNC.AddMonths(6), 40m },
                                { resourceStartDateNC.AddMonths(7), 0m },
                                { resourceStartDateNC.AddMonths(8), 0m },
                                { resourceStartDateNC.AddMonths(9), 0m },
                                { resourceStartDateNC.AddMonths(10), 0m },
                                { resourceStartDateNC.AddMonths(11), 0m },
                                { resourceStartDateNC.AddMonths(12), 0m },
                                { resourceStartDateNC.AddMonths(13), 0m },
                                { resourceStartDateNC.AddMonths(14), 0m },
                                { resourceStartDateNC.AddMonths(15), 0m },
                                { resourceStartDateNC.AddMonths(16), 0m },
                                { resourceStartDateNC.AddMonths(17), 0m },
                                { resourceStartDateNC.AddMonths(18), 0m },
                                { resourceStartDateNC.AddMonths(19), 0m },
                                { resourceStartDateNC.AddMonths(20), 0m }
                            }
                        }
                    }
                });

                // clin collection changes
                testCases.Add(new AdjustDateTestCase
                {
                    TestName = "Clin collection changes",
                    DateShiftLevel = Level.Workspace,
                    FlowdownType = DateAdjustFlowdownType.Automatic,
                    DiscreteType = DateAdjustDiscreteType.NotSet,
                    StartDateShift = 0,
                    EndDateShift = 0,
                    ExpectedDateRange = new Dictionary<Element, Tuple<DateTime?, DateTime?>>()
                    {
                        { Element.Workspace, new Tuple<DateTime?, DateTime?>(contractStartDate, contractEndDate) },
                        { Element.Clin1, new Tuple<DateTime?, DateTime?>(contractStartDate.AddMonths(1), contractEndDate.AddMonths(-2)) },
                        { Element.Clin2, new Tuple<DateTime?, DateTime?>(clinStartDate2.Value.AddMonths(3), clinEndDate2.Value.AddMonths(-3)) },
                        { Element.Clin3, new Tuple<DateTime?, DateTime?>(null, null) },
                        { Element.Boe1, new Tuple<DateTime?, DateTime?> (boeStartDate1.AddMonths(1), boeEndDate1.AddMonths(-2)) },
                        { Element.Boe2, new Tuple<DateTime?, DateTime?> (boeStartDate2.AddMonths(3), boeEndDate2.AddMonths(-3)) },
                        { Element.Boe3, new Tuple<DateTime?, DateTime?> (boeStartDate3, boeEndDate3) },
                        { Element.BoeNC, new Tuple<DateTime?, DateTime?> (boeStartDateNC, boeEndDateNC) },
                        { Element.Task1, new Tuple<DateTime?, DateTime?> (taskStartDate1.AddMonths(1), taskEndDate1.AddMonths(-2)) },
                        { Element.Task2, new Tuple<DateTime?, DateTime?> (taskStartDate2.AddMonths(3), taskEndDate2.AddMonths(-3)) },
                        { Element.Task3, new Tuple<DateTime?, DateTime?> (taskStartDate3, taskEndDate3) },
                        { Element.TaskNC, new Tuple<DateTime?, DateTime?> (taskStartDateNC, taskEndDateNC) },
                        { Element.Travel1, new Tuple<DateTime?, DateTime?> (taskStartDate1.AddMonths(1), taskEndDate1.AddMonths(-2)) },
                        { Element.TravelTrip1, new Tuple<DateTime?, DateTime?> (taskStartDate1.AddMonths(1), null) },
                        { Element.Travel2, new Tuple<DateTime?, DateTime?> (taskStartDate2.AddMonths(3), taskEndDate2.AddMonths(-3)) },
                        { Element.Travel3, new Tuple<DateTime?, DateTime?> (taskStartDate3, taskEndDate3) },
                        { Element.TravelNC, new Tuple<DateTime?, DateTime?> (taskStartDateNC, taskEndDateNC) },
                        { Element.Resource1, new Tuple<DateTime?, DateTime?> (resourceStartDate1.AddMonths(1), resourceEndDate1.AddMonths(-2)) },
                        { Element.Resource2, new Tuple<DateTime?, DateTime?> (resourceStartDate2.AddMonths(3), resourceEndDate2.AddMonths(-3)) },
                        { Element.Resource3, new Tuple<DateTime?, DateTime?> (resourceStartDate3, resourceEndDate3) },
                        { Element.ResourceNC, new Tuple<DateTime?, DateTime?> (resourceStartDateNC, resourceEndDateNC) }
                    },
                    ExpectedSpreadResults = new Dictionary<Element, Dictionary<DateTime, decimal>>()
                    {
                        { Element.Resource1, new Dictionary<DateTime, decimal>()
                            {
                                { resourceStartDate1.AddMonths(1), 0.03m },
                                { resourceStartDate1.AddMonths(2), 0.15m },
                                { resourceStartDate1.AddMonths(3), 0.27m },
                                { resourceStartDate1.AddMonths(4), 0.41m },
                                { resourceStartDate1.AddMonths(5), 0.61m },
                                { resourceStartDate1.AddMonths(6), 0.87m },
                                { resourceStartDate1.AddMonths(7), 1.21m },
                                { resourceStartDate1.AddMonths(8), 1.66m },
                                { resourceStartDate1.AddMonths(9), 2.20m },
                                { resourceStartDate1.AddMonths(10), 2.84m },
                                { resourceStartDate1.AddMonths(11), 3.51m },
                                { resourceStartDate1.AddMonths(12), 4.12m },
                                { resourceStartDate1.AddMonths(13), 4.57m },
                                { resourceStartDate1.AddMonths(14), 4.75m },
                                { resourceStartDate1.AddMonths(15), 4.64m },
                                { resourceStartDate1.AddMonths(16), 4.29m },
                                { resourceStartDate1.AddMonths(17), 3.86m },
                                { resourceStartDate1.AddMonths(18), 3.54m },
                                { resourceStartDate1.AddMonths(19), 3.65m },
                                { resourceStartDate1.AddMonths(20), 4.62m },
                                { resourceStartDate1.AddMonths(21), 6.98m },
                                { resourceStartDate1.AddMonths(22), 10.43m },
                                { resourceStartDate1.AddMonths(23), 12.26m },
                                { resourceStartDate1.AddMonths(24), 9.83m },
                                { resourceStartDate1.AddMonths(25), 5.44m },
                                { resourceStartDate1.AddMonths(26), 2.35m },
                                { resourceStartDate1.AddMonths(27), 0.91m },
                            }
                        },
                        { Element.Resource2, new Dictionary<DateTime, decimal>()
                            {
                                { resourceStartDate2.AddMonths(3), 12m },
                                { resourceStartDate2.AddMonths(4), 11m },
                                { resourceStartDate2.AddMonths(5), 10m },
                                { resourceStartDate2.AddMonths(6), 10m },
                                { resourceStartDate2.AddMonths(7), 9m },
                                { resourceStartDate2.AddMonths(8), 8m },
                                { resourceStartDate2.AddMonths(9), 7m },
                                { resourceStartDate2.AddMonths(10), 7m },
                                { resourceStartDate2.AddMonths(11), 6m },
                                { resourceStartDate2.AddMonths(12), 5m },
                                { resourceStartDate2.AddMonths(13), 4m },
                                { resourceStartDate2.AddMonths(14), 4m },
                                { resourceStartDate2.AddMonths(15), 3m },
                                { resourceStartDate2.AddMonths(16), 2m },
                                { resourceStartDate2.AddMonths(17), 1m },
                                { resourceStartDate2.AddMonths(18), 1m }
                            }
                        },{ Element.Resource3, new Dictionary<DateTime, decimal>()
                            {
                                { resourceStartDate3, 10m },
                                { resourceStartDate3.AddMonths(1), 20m },
                                { resourceStartDate3.AddMonths(2), 30m },
                                { resourceStartDate3.AddMonths(3), 0m },
                                { resourceStartDate3.AddMonths(4), 0m },
                                { resourceStartDate3.AddMonths(5), 0m },
                                { resourceStartDate3.AddMonths(6), 0m },
                                { resourceStartDate3.AddMonths(7), 0m },
                                { resourceStartDate3.AddMonths(8), 0m },
                                { resourceStartDate3.AddMonths(9), 0m },
                                { resourceStartDate3.AddMonths(10), 0m },
                                { resourceStartDate3.AddMonths(11), 0m },
                                { resourceStartDate3.AddMonths(12), 0m },
                                { resourceStartDate3.AddMonths(13), 0m },
                                { resourceStartDate3.AddMonths(14), 0m },
                                { resourceStartDate3.AddMonths(15), 0m },
                                { resourceStartDate3.AddMonths(16), 10m },
                                { resourceStartDate3.AddMonths(17), 30m }
                            }
                        },
                        { Element.ResourceNC, new Dictionary<DateTime, decimal>()
                            {
                                { resourceStartDateNC, 0m },
                                { resourceStartDateNC.AddMonths(1), 20m },
                                { resourceStartDateNC.AddMonths(2), 20m },
                                { resourceStartDateNC.AddMonths(3), 40m },
                                { resourceStartDateNC.AddMonths(4), 0m },
                                { resourceStartDateNC.AddMonths(5), 0m },
                                { resourceStartDateNC.AddMonths(6), 0m },
                                { resourceStartDateNC.AddMonths(7), 0m },
                                { resourceStartDateNC.AddMonths(8), 0m },
                                { resourceStartDateNC.AddMonths(9), 0m },
                                { resourceStartDateNC.AddMonths(10), 0m },
                                { resourceStartDateNC.AddMonths(11), 0m },
                                { resourceStartDateNC.AddMonths(12), 0m },
                                { resourceStartDateNC.AddMonths(13), 0m },
                                { resourceStartDateNC.AddMonths(14), 0m },
                                { resourceStartDateNC.AddMonths(15), 0m },
                                { resourceStartDateNC.AddMonths(16), 0m },
                                { resourceStartDateNC.AddMonths(17), 0m },
                                { resourceStartDateNC.AddMonths(18), 0m },
                                { resourceStartDateNC.AddMonths(19), 0m },
                                { resourceStartDateNC.AddMonths(20), 0m },
                                { resourceStartDateNC.AddMonths(21), 0m },
                                { resourceStartDateNC.AddMonths(22), 20m },
                                { resourceStartDateNC.AddMonths(23), 0m }
                            }
                        }
                    }
                });

                // clin null to date
                testCases.Add(new AdjustDateTestCase
                {
                    TestName = "Assign Clin Date",
                    DateShiftLevel = Level.CLIN,
                    ObjectId = 1,
                    FlowdownType = DateAdjustFlowdownType.Automatic,
                    DiscreteType = DateAdjustDiscreteType.NoChange,
                    StartDateShift = 4,
                    EndDateShift = -2,
                    ExpectedDateRange = new Dictionary<Element, Tuple<DateTime?, DateTime?>>()
                    {
                        { Element.Clin1, new Tuple<DateTime?, DateTime?>(contractStartDate.AddMonths(4), contractEndDate.AddMonths(-2)) },
                        { Element.Boe1, new Tuple<DateTime?, DateTime?> (boeStartDate1.AddMonths(4), boeEndDate1.AddMonths(-2)) },
                        { Element.Task1, new Tuple<DateTime?, DateTime?> (taskStartDate1.AddMonths(4), taskEndDate1.AddMonths(-2)) },
                        { Element.Travel1, new Tuple<DateTime?, DateTime?> (taskStartDate1.AddMonths(4), taskEndDate1.AddMonths(-2)) },
                        { Element.TravelTrip1, new Tuple<DateTime?, DateTime?> (taskStartDate1.AddMonths(4), null) },
                        { Element.Resource1, new Tuple<DateTime?, DateTime?> (resourceStartDate1.AddMonths(4), resourceEndDate1.AddMonths(-2)) }
                    },
                    ExpectedSpreadResults = new Dictionary<Element, Dictionary<DateTime, decimal>>()
                    {
                        { Element.Resource1, new Dictionary<DateTime, decimal>()
                            {
                                { resourceStartDate1.AddMonths(4), 0.04m },
                                { resourceStartDate1.AddMonths(5), 0.19m },
                                { resourceStartDate1.AddMonths(6), 0.35m },
                                { resourceStartDate1.AddMonths(7), 0.56m },
                                { resourceStartDate1.AddMonths(8), 0.84m },
                                { resourceStartDate1.AddMonths(9), 1.23m },
                                { resourceStartDate1.AddMonths(10), 1.76m },
                                { resourceStartDate1.AddMonths(11), 2.44m },
                                { resourceStartDate1.AddMonths(12), 3.24m },
                                { resourceStartDate1.AddMonths(13), 4.09m },
                                { resourceStartDate1.AddMonths(14), 4.82m },
                                { resourceStartDate1.AddMonths(15), 5.27m },
                                { resourceStartDate1.AddMonths(16), 5.31m },
                                { resourceStartDate1.AddMonths(17), 4.96m },
                                { resourceStartDate1.AddMonths(18), 4.43m },
                                { resourceStartDate1.AddMonths(19), 4m },
                                { resourceStartDate1.AddMonths(20), 4.15m },
                                { resourceStartDate1.AddMonths(21), 5.58m },
                                { resourceStartDate1.AddMonths(22), 9.02m },
                                { resourceStartDate1.AddMonths(23), 13.08m },
                                { resourceStartDate1.AddMonths(24), 12.76m },
                                { resourceStartDate1.AddMonths(25), 7.63m },
                                { resourceStartDate1.AddMonths(26), 3.16m },
                                { resourceStartDate1.AddMonths(27), 1.09m },
                            }
                        },
                        { Element.Resource2, new Dictionary<DateTime, decimal>()
                            {
                                { resourceStartDate2, 9m },
                                { resourceStartDate2.AddMonths(1), 8m },
                                { resourceStartDate2.AddMonths(2), 8m },
                                { resourceStartDate2.AddMonths(3), 7m },
                                { resourceStartDate2.AddMonths(4), 7m },
                                { resourceStartDate2.AddMonths(5), 7m },
                                { resourceStartDate2.AddMonths(6), 6m },
                                { resourceStartDate2.AddMonths(7), 6m },
                                { resourceStartDate2.AddMonths(8), 6m },
                                { resourceStartDate2.AddMonths(9), 5m },
                                { resourceStartDate2.AddMonths(10), 5m },
                                { resourceStartDate2.AddMonths(11), 4m },
                                { resourceStartDate2.AddMonths(12), 4m },
                                { resourceStartDate2.AddMonths(13), 4m },
                                { resourceStartDate2.AddMonths(14), 3m },
                                { resourceStartDate2.AddMonths(15), 3m },
                                { resourceStartDate2.AddMonths(16), 2m },
                                { resourceStartDate2.AddMonths(17), 2m },
                                { resourceStartDate2.AddMonths(18), 2m },
                                { resourceStartDate2.AddMonths(19), 1m },
                                { resourceStartDate2.AddMonths(20), 1m },
                                { resourceStartDate2.AddMonths(21), 0m }
                            }
                        },{ Element.Resource3, new Dictionary<DateTime, decimal>()
                            {
                                { resourceStartDate3, 10m },
                                { resourceStartDate3.AddMonths(1), 20m },
                                { resourceStartDate3.AddMonths(2), 30m },
                                { resourceStartDate3.AddMonths(3), 0m },
                                { resourceStartDate3.AddMonths(4), 0m },
                                { resourceStartDate3.AddMonths(5), 0m },
                                { resourceStartDate3.AddMonths(6), 0m },
                                { resourceStartDate3.AddMonths(7), 0m },
                                { resourceStartDate3.AddMonths(8), 0m },
                                { resourceStartDate3.AddMonths(9), 0m },
                                { resourceStartDate3.AddMonths(10), 0m },
                                { resourceStartDate3.AddMonths(11), 0m },
                                { resourceStartDate3.AddMonths(12), 0m },
                                { resourceStartDate3.AddMonths(13), 0m },
                                { resourceStartDate3.AddMonths(14), 0m },
                                { resourceStartDate3.AddMonths(15), 0m },
                                { resourceStartDate3.AddMonths(16), 10m },
                                { resourceStartDate3.AddMonths(17), 30m }
                            }
                        },
                        { Element.ResourceNC, new Dictionary<DateTime, decimal>()
                            {
                                { resourceStartDateNC, 0m },
                                { resourceStartDateNC.AddMonths(1), 20m },
                                { resourceStartDateNC.AddMonths(2), 20m },
                                { resourceStartDateNC.AddMonths(3), 40m },
                                { resourceStartDateNC.AddMonths(4), 0m },
                                { resourceStartDateNC.AddMonths(5), 0m },
                                { resourceStartDateNC.AddMonths(6), 0m },
                                { resourceStartDateNC.AddMonths(7), 0m },
                                { resourceStartDateNC.AddMonths(8), 0m },
                                { resourceStartDateNC.AddMonths(9), 0m },
                                { resourceStartDateNC.AddMonths(10), 0m },
                                { resourceStartDateNC.AddMonths(11), 0m },
                                { resourceStartDateNC.AddMonths(12), 0m },
                                { resourceStartDateNC.AddMonths(13), 0m },
                                { resourceStartDateNC.AddMonths(14), 0m },
                                { resourceStartDateNC.AddMonths(15), 0m },
                                { resourceStartDateNC.AddMonths(16), 0m },
                                { resourceStartDateNC.AddMonths(17), 0m },
                                { resourceStartDateNC.AddMonths(18), 0m },
                                { resourceStartDateNC.AddMonths(19), 0m },
                                { resourceStartDateNC.AddMonths(20), 0m },
                                { resourceStartDateNC.AddMonths(21), 0m },
                                { resourceStartDateNC.AddMonths(22), 20m },
                                { resourceStartDateNC.AddMonths(23), 0m }
                            }
                        }
                    }
                });

                // clin shift
                testCases.Add(new AdjustDateTestCase
                {
                    TestName = "Shift Clin",
                    DateShiftLevel = Level.CLIN,
                    ObjectId = 3,
                    FlowdownType = DateAdjustFlowdownType.Manual,
                    DiscreteType = DateAdjustDiscreteType.NoChange,
                    StartDateShift = 3,
                    EndDateShift = 3,
                    ExpectedDateRange = new Dictionary<Element, Tuple<DateTime?, DateTime?>>()
                    {
                        { Element.Clin3, new Tuple<DateTime?, DateTime?>(clinStartDate3.Value.AddMonths(3), clinEndDate3.Value.AddMonths(3)) },
                        { Element.Boe3, new Tuple<DateTime?, DateTime?> (boeStartDate3.AddMonths(3), boeEndDate3.AddMonths(3)) },
                        { Element.Task3, new Tuple<DateTime?, DateTime?> (taskStartDate3.AddMonths(3), taskEndDate3.AddMonths(3)) },
                        { Element.Travel3, new Tuple<DateTime?, DateTime?> (taskStartDate3.AddMonths(3), taskEndDate3.AddMonths(3)) }
                    },
                    ExpectedSpreadResults = new Dictionary<Element, Dictionary<DateTime, decimal>>()
                    {
                        { Element.Resource1, new Dictionary<DateTime, decimal>()
                            {
                                { resourceStartDate1, 0.02m },
                                { resourceStartDate1.AddMonths(1), 0.12m },
                                { resourceStartDate1.AddMonths(2), 0.21m },
                                { resourceStartDate1.AddMonths(3), 0.32m },
                                { resourceStartDate1.AddMonths(4), 0.46m },
                                { resourceStartDate1.AddMonths(5), 0.64m },
                                { resourceStartDate1.AddMonths(6), 0.88m },
                                { resourceStartDate1.AddMonths(7), 1.18m },
                                { resourceStartDate1.AddMonths(8), 1.56m },
                                { resourceStartDate1.AddMonths(9), 2.01m },
                                { resourceStartDate1.AddMonths(10), 2.53m },
                                { resourceStartDate1.AddMonths(11), 3.07m },
                                { resourceStartDate1.AddMonths(12), 3.59m },
                                { resourceStartDate1.AddMonths(13), 4.00m },
                                { resourceStartDate1.AddMonths(14), 4.24m },
                                { resourceStartDate1.AddMonths(15), 4.26m },
                                { resourceStartDate1.AddMonths(16), 4.08m },
                                { resourceStartDate1.AddMonths(17), 3.77m },
                                { resourceStartDate1.AddMonths(18), 3.42m },
                                { resourceStartDate1.AddMonths(19), 3.18m },
                                { resourceStartDate1.AddMonths(20), 3.26m },
                                { resourceStartDate1.AddMonths(21), 3.94m },
                                { resourceStartDate1.AddMonths(22), 5.61m },
                                { resourceStartDate1.AddMonths(23), 8.29m },
                                { resourceStartDate1.AddMonths(24), 10.73m },
                                { resourceStartDate1.AddMonths(25), 10.54m },
                                { resourceStartDate1.AddMonths(26), 7.47m },
                                { resourceStartDate1.AddMonths(27), 4.03m },
                                { resourceStartDate1.AddMonths(28), 1.83m },
                                { resourceStartDate1.AddMonths(29), 0.76m },
                            }
                        },
                        { Element.Resource2, new Dictionary<DateTime, decimal>()
                            {
                                { resourceStartDate2, 9m },
                                { resourceStartDate2.AddMonths(1), 8m },
                                { resourceStartDate2.AddMonths(2), 8m },
                                { resourceStartDate2.AddMonths(3), 7m },
                                { resourceStartDate2.AddMonths(4), 7m },
                                { resourceStartDate2.AddMonths(5), 7m },
                                { resourceStartDate2.AddMonths(6), 6m },
                                { resourceStartDate2.AddMonths(7), 6m },
                                { resourceStartDate2.AddMonths(8), 6m },
                                { resourceStartDate2.AddMonths(9), 5m },
                                { resourceStartDate2.AddMonths(10), 5m },
                                { resourceStartDate2.AddMonths(11), 4m },
                                { resourceStartDate2.AddMonths(12), 4m },
                                { resourceStartDate2.AddMonths(13), 4m },
                                { resourceStartDate2.AddMonths(14), 3m },
                                { resourceStartDate2.AddMonths(15), 3m },
                                { resourceStartDate2.AddMonths(16), 2m },
                                { resourceStartDate2.AddMonths(17), 2m },
                                { resourceStartDate2.AddMonths(18), 2m },
                                { resourceStartDate2.AddMonths(19), 1m },
                                { resourceStartDate2.AddMonths(20), 1m },
                                { resourceStartDate2.AddMonths(21), 0m }
                            }
                        },
                        { Element.Resource3, new Dictionary<DateTime, decimal>()
                            {
                                { resourceStartDate3, 10m },
                                { resourceStartDate3.AddMonths(1), 20m },
                                { resourceStartDate3.AddMonths(2), 30m },
                                { resourceStartDate3.AddMonths(3), 0m },
                                { resourceStartDate3.AddMonths(4), 0m },
                                { resourceStartDate3.AddMonths(5), 0m },
                                { resourceStartDate3.AddMonths(6), 0m },
                                { resourceStartDate3.AddMonths(7), 0m },
                                { resourceStartDate3.AddMonths(8), 0m },
                                { resourceStartDate3.AddMonths(9), 0m },
                                { resourceStartDate3.AddMonths(10), 0m },
                                { resourceStartDate3.AddMonths(11), 0m },
                                { resourceStartDate3.AddMonths(12), 0m },
                                { resourceStartDate3.AddMonths(13), 0m },
                                { resourceStartDate3.AddMonths(14), 0m },
                                { resourceStartDate3.AddMonths(15), 0m },
                                { resourceStartDate3.AddMonths(16), 10m },
                                { resourceStartDate3.AddMonths(17), 30m }
                            }
                        },
                        { Element.ResourceNC, new Dictionary<DateTime, decimal>()
                            {
                                { resourceStartDateNC, 0m },
                                { resourceStartDateNC.AddMonths(1), 20m },
                                { resourceStartDateNC.AddMonths(2), 20m },
                                { resourceStartDateNC.AddMonths(3), 40m },
                                { resourceStartDateNC.AddMonths(4), 0m },
                                { resourceStartDateNC.AddMonths(5), 0m },
                                { resourceStartDateNC.AddMonths(6), 0m },
                                { resourceStartDateNC.AddMonths(7), 0m },
                                { resourceStartDateNC.AddMonths(8), 0m },
                                { resourceStartDateNC.AddMonths(9), 0m },
                                { resourceStartDateNC.AddMonths(10), 0m },
                                { resourceStartDateNC.AddMonths(11), 0m },
                                { resourceStartDateNC.AddMonths(12), 0m },
                                { resourceStartDateNC.AddMonths(13), 0m },
                                { resourceStartDateNC.AddMonths(14), 0m },
                                { resourceStartDateNC.AddMonths(15), 0m },
                                { resourceStartDateNC.AddMonths(16), 0m },
                                { resourceStartDateNC.AddMonths(17), 0m },
                                { resourceStartDateNC.AddMonths(18), 0m },
                                { resourceStartDateNC.AddMonths(19), 0m },
                                { resourceStartDateNC.AddMonths(20), 0m },
                                { resourceStartDateNC.AddMonths(21), 0m },
                                { resourceStartDateNC.AddMonths(22), 20m },
                                { resourceStartDateNC.AddMonths(23), 0m }
                            }
                        }
                    }
                });

                // clin expand
                testCases.Add(new AdjustDateTestCase
                {
                    TestName = "Clin Expand",
                    DateShiftLevel = Level.CLIN,
                    ObjectId = 3,
                    FlowdownType = DateAdjustFlowdownType.Automatic,
                    DiscreteType = DateAdjustDiscreteType.NoChange,
                    StartDateShift = -3,
                    EndDateShift = 3,
                    ExpectedDateRange = new Dictionary<Element, Tuple<DateTime?, DateTime?>>()
                    {
                        { Element.Clin3, new Tuple<DateTime?, DateTime?>(clinStartDate3.Value.AddMonths(-3), clinEndDate3.Value.AddMonths(3)) },
                        { Element.Boe3, new Tuple<DateTime?, DateTime?> (boeStartDate3, boeEndDate3) },
                        { Element.Task3, new Tuple<DateTime?, DateTime?> (taskStartDate3, taskEndDate3) },
                        { Element.Travel3, new Tuple<DateTime?, DateTime?> (taskStartDate3, taskEndDate3) },
                        { Element.Resource3, new Tuple<DateTime?, DateTime?> (resourceStartDate3, resourceEndDate3) }
                    },
                    ExpectedSpreadResults = new Dictionary<Element, Dictionary<DateTime, decimal>>()
                    {
                        { Element.Resource1, new Dictionary<DateTime, decimal>()
                            {
                                { resourceStartDate1, 0.02m },
                                { resourceStartDate1.AddMonths(1), 0.12m },
                                { resourceStartDate1.AddMonths(2), 0.21m },
                                { resourceStartDate1.AddMonths(3), 0.32m },
                                { resourceStartDate1.AddMonths(4), 0.46m },
                                { resourceStartDate1.AddMonths(5), 0.64m },
                                { resourceStartDate1.AddMonths(6), 0.88m },
                                { resourceStartDate1.AddMonths(7), 1.18m },
                                { resourceStartDate1.AddMonths(8), 1.56m },
                                { resourceStartDate1.AddMonths(9), 2.01m },
                                { resourceStartDate1.AddMonths(10), 2.53m },
                                { resourceStartDate1.AddMonths(11), 3.07m },
                                { resourceStartDate1.AddMonths(12), 3.59m },
                                { resourceStartDate1.AddMonths(13), 4.00m },
                                { resourceStartDate1.AddMonths(14), 4.24m },
                                { resourceStartDate1.AddMonths(15), 4.26m },
                                { resourceStartDate1.AddMonths(16), 4.08m },
                                { resourceStartDate1.AddMonths(17), 3.77m },
                                { resourceStartDate1.AddMonths(18), 3.42m },
                                { resourceStartDate1.AddMonths(19), 3.18m },
                                { resourceStartDate1.AddMonths(20), 3.26m },
                                { resourceStartDate1.AddMonths(21), 3.94m },
                                { resourceStartDate1.AddMonths(22), 5.61m },
                                { resourceStartDate1.AddMonths(23), 8.29m },
                                { resourceStartDate1.AddMonths(24), 10.73m },
                                { resourceStartDate1.AddMonths(25), 10.54m },
                                { resourceStartDate1.AddMonths(26), 7.47m },
                                { resourceStartDate1.AddMonths(27), 4.03m },
                                { resourceStartDate1.AddMonths(28), 1.83m },
                                { resourceStartDate1.AddMonths(29), 0.76m },
                            }
                        },
                        { Element.Resource2, new Dictionary<DateTime, decimal>()
                            {
                                { resourceStartDate2, 9m },
                                { resourceStartDate2.AddMonths(1), 8m },
                                { resourceStartDate2.AddMonths(2), 8m },
                                { resourceStartDate2.AddMonths(3), 7m },
                                { resourceStartDate2.AddMonths(4), 7m },
                                { resourceStartDate2.AddMonths(5), 7m },
                                { resourceStartDate2.AddMonths(6), 6m },
                                { resourceStartDate2.AddMonths(7), 6m },
                                { resourceStartDate2.AddMonths(8), 6m },
                                { resourceStartDate2.AddMonths(9), 5m },
                                { resourceStartDate2.AddMonths(10), 5m },
                                { resourceStartDate2.AddMonths(11), 4m },
                                { resourceStartDate2.AddMonths(12), 4m },
                                { resourceStartDate2.AddMonths(13), 4m },
                                { resourceStartDate2.AddMonths(14), 3m },
                                { resourceStartDate2.AddMonths(15), 3m },
                                { resourceStartDate2.AddMonths(16), 2m },
                                { resourceStartDate2.AddMonths(17), 2m },
                                { resourceStartDate2.AddMonths(18), 2m },
                                { resourceStartDate2.AddMonths(19), 1m },
                                { resourceStartDate2.AddMonths(20), 1m },
                                { resourceStartDate2.AddMonths(21), 0m }
                            }
                        },{ Element.Resource3, new Dictionary<DateTime, decimal>()
                            {
                                { resourceStartDate3, 10m },
                                { resourceStartDate3.AddMonths(1), 20m },
                                { resourceStartDate3.AddMonths(2), 30m },
                                { resourceStartDate3.AddMonths(3), 0m },
                                { resourceStartDate3.AddMonths(4), 0m },
                                { resourceStartDate3.AddMonths(5), 0m },
                                { resourceStartDate3.AddMonths(6), 0m },
                                { resourceStartDate3.AddMonths(7), 0m },
                                { resourceStartDate3.AddMonths(8), 0m },
                                { resourceStartDate3.AddMonths(9), 0m },
                                { resourceStartDate3.AddMonths(10), 0m },
                                { resourceStartDate3.AddMonths(11), 0m },
                                { resourceStartDate3.AddMonths(12), 0m },
                                { resourceStartDate3.AddMonths(13), 0m },
                                { resourceStartDate3.AddMonths(14), 0m },
                                { resourceStartDate3.AddMonths(15), 0m },
                                { resourceStartDate3.AddMonths(16), 10m },
                                { resourceStartDate3.AddMonths(17), 30m }
                            }
                        },
                        { Element.ResourceNC, new Dictionary<DateTime, decimal>()
                            {
                                { resourceStartDateNC, 0m },
                                { resourceStartDateNC.AddMonths(1), 20m },
                                { resourceStartDateNC.AddMonths(2), 20m },
                                { resourceStartDateNC.AddMonths(3), 40m },
                                { resourceStartDateNC.AddMonths(4), 0m },
                                { resourceStartDateNC.AddMonths(5), 0m },
                                { resourceStartDateNC.AddMonths(6), 0m },
                                { resourceStartDateNC.AddMonths(7), 0m },
                                { resourceStartDateNC.AddMonths(8), 0m },
                                { resourceStartDateNC.AddMonths(9), 0m },
                                { resourceStartDateNC.AddMonths(10), 0m },
                                { resourceStartDateNC.AddMonths(11), 0m },
                                { resourceStartDateNC.AddMonths(12), 0m },
                                { resourceStartDateNC.AddMonths(13), 0m },
                                { resourceStartDateNC.AddMonths(14), 0m },
                                { resourceStartDateNC.AddMonths(15), 0m },
                                { resourceStartDateNC.AddMonths(16), 0m },
                                { resourceStartDateNC.AddMonths(17), 0m },
                                { resourceStartDateNC.AddMonths(18), 0m },
                                { resourceStartDateNC.AddMonths(19), 0m },
                                { resourceStartDateNC.AddMonths(20), 0m },
                                { resourceStartDateNC.AddMonths(21), 0m },
                                { resourceStartDateNC.AddMonths(22), 20m },
                                { resourceStartDateNC.AddMonths(23), 0m }
                            }
                        }
                    }
                });

                // clin shrink
                testCases.Add(new AdjustDateTestCase
                {
                    TestName = "Clin Shrink",
                    DateShiftLevel = Level.CLIN,
                    ObjectId = 2,
                    FlowdownType = DateAdjustFlowdownType.Manual,
                    DiscreteType = DateAdjustDiscreteType.NoChange,
                    StartDateShift = 6,
                    EndDateShift = -2,
                    ExpectedDateRange = new Dictionary<Element, Tuple<DateTime?, DateTime?>>()
                    {
                        { Element.Clin2, new Tuple<DateTime?, DateTime?>(clinStartDate2.Value.AddMonths(6), clinEndDate2.Value.AddMonths(-2)) },
                        { Element.Boe2, new Tuple<DateTime?, DateTime?> (boeStartDate2.AddMonths(6), boeEndDate2.AddMonths(-2)) },
                        { Element.Task2, new Tuple<DateTime?, DateTime?> (taskStartDate2.AddMonths(6), taskEndDate2.AddMonths(-2)) },
                        { Element.Travel2, new Tuple<DateTime?, DateTime?> (taskStartDate2.AddMonths(6), taskEndDate2.AddMonths(-2)) }
                    },
                    ExpectedSpreadResults = new Dictionary<Element, Dictionary<DateTime, decimal>>()
                    {
                        { Element.Resource1, new Dictionary<DateTime, decimal>()
                            {
                                { resourceStartDate1, 0.02m },
                                { resourceStartDate1.AddMonths(1), 0.12m },
                                { resourceStartDate1.AddMonths(2), 0.21m },
                                { resourceStartDate1.AddMonths(3), 0.32m },
                                { resourceStartDate1.AddMonths(4), 0.46m },
                                { resourceStartDate1.AddMonths(5), 0.64m },
                                { resourceStartDate1.AddMonths(6), 0.88m },
                                { resourceStartDate1.AddMonths(7), 1.18m },
                                { resourceStartDate1.AddMonths(8), 1.56m },
                                { resourceStartDate1.AddMonths(9), 2.01m },
                                { resourceStartDate1.AddMonths(10), 2.53m },
                                { resourceStartDate1.AddMonths(11), 3.07m },
                                { resourceStartDate1.AddMonths(12), 3.59m },
                                { resourceStartDate1.AddMonths(13), 4.00m },
                                { resourceStartDate1.AddMonths(14), 4.24m },
                                { resourceStartDate1.AddMonths(15), 4.26m },
                                { resourceStartDate1.AddMonths(16), 4.08m },
                                { resourceStartDate1.AddMonths(17), 3.77m },
                                { resourceStartDate1.AddMonths(18), 3.42m },
                                { resourceStartDate1.AddMonths(19), 3.18m },
                                { resourceStartDate1.AddMonths(20), 3.26m },
                                { resourceStartDate1.AddMonths(21), 3.94m },
                                { resourceStartDate1.AddMonths(22), 5.61m },
                                { resourceStartDate1.AddMonths(23), 8.29m },
                                { resourceStartDate1.AddMonths(24), 10.73m },
                                { resourceStartDate1.AddMonths(25), 10.54m },
                                { resourceStartDate1.AddMonths(26), 7.47m },
                                { resourceStartDate1.AddMonths(27), 4.03m },
                                { resourceStartDate1.AddMonths(28), 1.83m },
                                { resourceStartDate1.AddMonths(29), 0.76m },
                            }
                        },
                        { Element.Resource2, new Dictionary<DateTime, decimal>()
                            {
                                { resourceStartDate2, 9m },
                                { resourceStartDate2.AddMonths(1), 8m },
                                { resourceStartDate2.AddMonths(2), 8m },
                                { resourceStartDate2.AddMonths(3), 7m },
                                { resourceStartDate2.AddMonths(4), 7m },
                                { resourceStartDate2.AddMonths(5), 7m },
                                { resourceStartDate2.AddMonths(6), 6m },
                                { resourceStartDate2.AddMonths(7), 6m },
                                { resourceStartDate2.AddMonths(8), 6m },
                                { resourceStartDate2.AddMonths(9), 5m },
                                { resourceStartDate2.AddMonths(10), 5m },
                                { resourceStartDate2.AddMonths(11), 4m },
                                { resourceStartDate2.AddMonths(12), 4m },
                                { resourceStartDate2.AddMonths(13), 4m },
                                { resourceStartDate2.AddMonths(14), 3m },
                                { resourceStartDate2.AddMonths(15), 3m },
                                { resourceStartDate2.AddMonths(16), 2m },
                                { resourceStartDate2.AddMonths(17), 2m },
                                { resourceStartDate2.AddMonths(18), 2m },
                                { resourceStartDate2.AddMonths(19), 1m },
                                { resourceStartDate2.AddMonths(20), 1m },
                                { resourceStartDate2.AddMonths(21), 0m }
                            }
                        },
                        { Element.Resource3, new Dictionary<DateTime, decimal>()
                            {
                                { resourceStartDate3, 10m },
                                { resourceStartDate3.AddMonths(1), 20m },
                                { resourceStartDate3.AddMonths(2), 30m },
                                { resourceStartDate3.AddMonths(3), 0m },
                                { resourceStartDate3.AddMonths(4), 0m },
                                { resourceStartDate3.AddMonths(5), 0m },
                                { resourceStartDate3.AddMonths(6), 0m },
                                { resourceStartDate3.AddMonths(7), 0m },
                                { resourceStartDate3.AddMonths(8), 0m },
                                { resourceStartDate3.AddMonths(9), 0m },
                                { resourceStartDate3.AddMonths(10), 0m },
                                { resourceStartDate3.AddMonths(11), 0m },
                                { resourceStartDate3.AddMonths(12), 0m },
                                { resourceStartDate3.AddMonths(13), 0m },
                                { resourceStartDate3.AddMonths(14), 0m },
                                { resourceStartDate3.AddMonths(15), 0m },
                                { resourceStartDate3.AddMonths(16), 10m },
                                { resourceStartDate3.AddMonths(17), 30m }
                            }
                        },
                        { Element.ResourceNC, new Dictionary<DateTime, decimal>()
                            {
                                { resourceStartDateNC, 0m },
                                { resourceStartDateNC.AddMonths(1), 20m },
                                { resourceStartDateNC.AddMonths(2), 20m },
                                { resourceStartDateNC.AddMonths(3), 40m },
                                { resourceStartDateNC.AddMonths(4), 0m },
                                { resourceStartDateNC.AddMonths(5), 0m },
                                { resourceStartDateNC.AddMonths(6), 0m },
                                { resourceStartDateNC.AddMonths(7), 0m },
                                { resourceStartDateNC.AddMonths(8), 0m },
                                { resourceStartDateNC.AddMonths(9), 0m },
                                { resourceStartDateNC.AddMonths(10), 0m },
                                { resourceStartDateNC.AddMonths(11), 0m },
                                { resourceStartDateNC.AddMonths(12), 0m },
                                { resourceStartDateNC.AddMonths(13), 0m },
                                { resourceStartDateNC.AddMonths(14), 0m },
                                { resourceStartDateNC.AddMonths(15), 0m },
                                { resourceStartDateNC.AddMonths(16), 0m },
                                { resourceStartDateNC.AddMonths(17), 0m },
                                { resourceStartDateNC.AddMonths(18), 0m },
                                { resourceStartDateNC.AddMonths(19), 0m },
                                { resourceStartDateNC.AddMonths(20), 0m },
                                { resourceStartDateNC.AddMonths(21), 0m },
                                { resourceStartDateNC.AddMonths(22), 20m },
                                { resourceStartDateNC.AddMonths(23), 0m }
                            }
                        }
                    }
                });

                // clin remove date
                testCases.Add(new AdjustDateTestCase
                {
                    TestName = "Clin Remove Date",
                    DateShiftLevel = Level.CLIN,
                    ObjectId = 2,
                    FlowdownType = DateAdjustFlowdownType.Automatic,
                    DiscreteType = DateAdjustDiscreteType.NoChange,
                    StartDateShift = int.MaxValue, // gets translated to a null date
                    EndDateShift = int.MaxValue, // gets translated to a null date
                    ExpectedDateRange = new Dictionary<Element, Tuple<DateTime?, DateTime?>>()
                    {
                        { Element.Clin2, new Tuple<DateTime?, DateTime?>(null, null) },
                        { Element.Boe2, new Tuple<DateTime?, DateTime?> (boeStartDate2, boeEndDate2) },
                        { Element.Task2, new Tuple<DateTime?, DateTime?> (taskStartDate2, taskEndDate2) },
                        { Element.Travel2, new Tuple<DateTime?, DateTime?> (taskStartDate2, taskEndDate2) },
                        { Element.Resource2, new Tuple<DateTime?, DateTime?> (resourceStartDate2, resourceEndDate2) }
                    },
                    ExpectedSpreadResults = new Dictionary<Element, Dictionary<DateTime, decimal>>()
                    {
                        { Element.Resource1, new Dictionary<DateTime, decimal>()
                            {
                                { resourceStartDate1, 0.02m },
                                { resourceStartDate1.AddMonths(1), 0.12m },
                                { resourceStartDate1.AddMonths(2), 0.21m },
                                { resourceStartDate1.AddMonths(3), 0.32m },
                                { resourceStartDate1.AddMonths(4), 0.46m },
                                { resourceStartDate1.AddMonths(5), 0.64m },
                                { resourceStartDate1.AddMonths(6), 0.88m },
                                { resourceStartDate1.AddMonths(7), 1.18m },
                                { resourceStartDate1.AddMonths(8), 1.56m },
                                { resourceStartDate1.AddMonths(9), 2.01m },
                                { resourceStartDate1.AddMonths(10), 2.53m },
                                { resourceStartDate1.AddMonths(11), 3.07m },
                                { resourceStartDate1.AddMonths(12), 3.59m },
                                { resourceStartDate1.AddMonths(13), 4.00m },
                                { resourceStartDate1.AddMonths(14), 4.24m },
                                { resourceStartDate1.AddMonths(15), 4.26m },
                                { resourceStartDate1.AddMonths(16), 4.08m },
                                { resourceStartDate1.AddMonths(17), 3.77m },
                                { resourceStartDate1.AddMonths(18), 3.42m },
                                { resourceStartDate1.AddMonths(19), 3.18m },
                                { resourceStartDate1.AddMonths(20), 3.26m },
                                { resourceStartDate1.AddMonths(21), 3.94m },
                                { resourceStartDate1.AddMonths(22), 5.61m },
                                { resourceStartDate1.AddMonths(23), 8.29m },
                                { resourceStartDate1.AddMonths(24), 10.73m },
                                { resourceStartDate1.AddMonths(25), 10.54m },
                                { resourceStartDate1.AddMonths(26), 7.47m },
                                { resourceStartDate1.AddMonths(27), 4.03m },
                                { resourceStartDate1.AddMonths(28), 1.83m },
                                { resourceStartDate1.AddMonths(29), 0.76m },
                            }
                        },
                        { Element.Resource2, new Dictionary<DateTime, decimal>()
                            {
                                { resourceStartDate2, 9m },
                                { resourceStartDate2.AddMonths(1), 8m },
                                { resourceStartDate2.AddMonths(2), 8m },
                                { resourceStartDate2.AddMonths(3), 7m },
                                { resourceStartDate2.AddMonths(4), 7m },
                                { resourceStartDate2.AddMonths(5), 7m },
                                { resourceStartDate2.AddMonths(6), 6m },
                                { resourceStartDate2.AddMonths(7), 6m },
                                { resourceStartDate2.AddMonths(8), 6m },
                                { resourceStartDate2.AddMonths(9), 5m },
                                { resourceStartDate2.AddMonths(10), 5m },
                                { resourceStartDate2.AddMonths(11), 4m },
                                { resourceStartDate2.AddMonths(12), 4m },
                                { resourceStartDate2.AddMonths(13), 4m },
                                { resourceStartDate2.AddMonths(14), 3m },
                                { resourceStartDate2.AddMonths(15), 3m },
                                { resourceStartDate2.AddMonths(16), 2m },
                                { resourceStartDate2.AddMonths(17), 2m },
                                { resourceStartDate2.AddMonths(18), 2m },
                                { resourceStartDate2.AddMonths(19), 1m },
                                { resourceStartDate2.AddMonths(20), 1m },
                                { resourceStartDate2.AddMonths(21), 0m }
                            }
                        },{ Element.Resource3, new Dictionary<DateTime, decimal>()
                            {
                                { resourceStartDate3, 10m },
                                { resourceStartDate3.AddMonths(1), 20m },
                                { resourceStartDate3.AddMonths(2), 30m },
                                { resourceStartDate3.AddMonths(3), 0m },
                                { resourceStartDate3.AddMonths(4), 0m },
                                { resourceStartDate3.AddMonths(5), 0m },
                                { resourceStartDate3.AddMonths(6), 0m },
                                { resourceStartDate3.AddMonths(7), 0m },
                                { resourceStartDate3.AddMonths(8), 0m },
                                { resourceStartDate3.AddMonths(9), 0m },
                                { resourceStartDate3.AddMonths(10), 0m },
                                { resourceStartDate3.AddMonths(11), 0m },
                                { resourceStartDate3.AddMonths(12), 0m },
                                { resourceStartDate3.AddMonths(13), 0m },
                                { resourceStartDate3.AddMonths(14), 0m },
                                { resourceStartDate3.AddMonths(15), 0m },
                                { resourceStartDate3.AddMonths(16), 10m },
                                { resourceStartDate3.AddMonths(17), 30m }
                            }
                        },
                        { Element.ResourceNC, new Dictionary<DateTime, decimal>()
                            {
                                { resourceStartDateNC, 0m },
                                { resourceStartDateNC.AddMonths(1), 20m },
                                { resourceStartDateNC.AddMonths(2), 20m },
                                { resourceStartDateNC.AddMonths(3), 40m },
                                { resourceStartDateNC.AddMonths(4), 0m },
                                { resourceStartDateNC.AddMonths(5), 0m },
                                { resourceStartDateNC.AddMonths(6), 0m },
                                { resourceStartDateNC.AddMonths(7), 0m },
                                { resourceStartDateNC.AddMonths(8), 0m },
                                { resourceStartDateNC.AddMonths(9), 0m },
                                { resourceStartDateNC.AddMonths(10), 0m },
                                { resourceStartDateNC.AddMonths(11), 0m },
                                { resourceStartDateNC.AddMonths(12), 0m },
                                { resourceStartDateNC.AddMonths(13), 0m },
                                { resourceStartDateNC.AddMonths(14), 0m },
                                { resourceStartDateNC.AddMonths(15), 0m },
                                { resourceStartDateNC.AddMonths(16), 0m },
                                { resourceStartDateNC.AddMonths(17), 0m },
                                { resourceStartDateNC.AddMonths(18), 0m },
                                { resourceStartDateNC.AddMonths(19), 0m },
                                { resourceStartDateNC.AddMonths(20), 0m },
                                { resourceStartDateNC.AddMonths(21), 0m },
                                { resourceStartDateNC.AddMonths(22), 20m },
                                { resourceStartDateNC.AddMonths(23), 0m }
                            }
                        }
                    }
                });

                // boe shift
                testCases.Add(new AdjustDateTestCase
                {
                    TestName = "Boe shift",
                    DateShiftLevel = Level.BOE,
                    ObjectId = 1,
                    FlowdownType = DateAdjustFlowdownType.Automatic,
                    DiscreteType = DateAdjustDiscreteType.NotSet,
                    StartDateShift = 3,
                    EndDateShift = 3,
                    ExpectedDateRange = new Dictionary<Element, Tuple<DateTime?, DateTime?>>()
                    {
                        { Element.Boe1, new Tuple<DateTime?, DateTime?>(boeStartDate1.AddMonths(3), boeEndDate1.AddMonths(3)) },
                        { Element.Task1, new Tuple<DateTime?, DateTime?>(taskStartDate1.AddMonths(3), taskEndDate1.AddMonths(3)) },
                        { Element.Travel1, new Tuple<DateTime?, DateTime?>(taskStartDate1.AddMonths(3), taskEndDate1.AddMonths(3)) },
                        { Element.TravelTrip1, new Tuple<DateTime?, DateTime?> (taskStartDate1.AddMonths(3), null) },
                        { Element.Resource1, new Tuple<DateTime?, DateTime?> (resourceStartDate1.AddMonths(3), resourceEndDate1.AddMonths(3)) }
                    },
                    ExpectedSpreadResults = new Dictionary<Element, Dictionary<DateTime, decimal>>()
                    {
                        { Element.Resource1, new Dictionary<DateTime, decimal>()
                            {
                                { resourceStartDate1.AddMonths(3), 0.02m },
                                { resourceStartDate1.AddMonths(4), 0.12m },
                                { resourceStartDate1.AddMonths(5), 0.21m },
                                { resourceStartDate1.AddMonths(6), 0.32m },
                                { resourceStartDate1.AddMonths(7), 0.46m },
                                { resourceStartDate1.AddMonths(8), 0.64m },
                                { resourceStartDate1.AddMonths(9), 0.88m },
                                { resourceStartDate1.AddMonths(10), 1.18m },
                                { resourceStartDate1.AddMonths(11), 1.56m },
                                { resourceStartDate1.AddMonths(12), 2.01m },
                                { resourceStartDate1.AddMonths(13), 2.53m },
                                { resourceStartDate1.AddMonths(14), 3.07m },
                                { resourceStartDate1.AddMonths(15), 3.59m },
                                { resourceStartDate1.AddMonths(16), 4.00m },
                                { resourceStartDate1.AddMonths(17), 4.24m },
                                { resourceStartDate1.AddMonths(18), 4.26m },
                                { resourceStartDate1.AddMonths(19), 4.08m },
                                { resourceStartDate1.AddMonths(20), 3.77m },
                                { resourceStartDate1.AddMonths(21), 3.42m },
                                { resourceStartDate1.AddMonths(22), 3.18m },
                                { resourceStartDate1.AddMonths(23), 3.26m },
                                { resourceStartDate1.AddMonths(24), 3.94m },
                                { resourceStartDate1.AddMonths(25), 5.61m },
                                { resourceStartDate1.AddMonths(26), 8.29m },
                                { resourceStartDate1.AddMonths(27), 10.73m },
                                { resourceStartDate1.AddMonths(28), 10.54m },
                                { resourceStartDate1.AddMonths(29), 7.47m },
                                { resourceStartDate1.AddMonths(30), 4.03m },
                                { resourceStartDate1.AddMonths(31), 1.83m },
                                { resourceStartDate1.AddMonths(32), 0.76m },
                            }
                        },
                        { Element.Resource2, new Dictionary<DateTime, decimal>()
                            {
                                { resourceStartDate2, 9m },
                                { resourceStartDate2.AddMonths(1), 8m },
                                { resourceStartDate2.AddMonths(2), 8m },
                                { resourceStartDate2.AddMonths(3), 7m },
                                { resourceStartDate2.AddMonths(4), 7m },
                                { resourceStartDate2.AddMonths(5), 7m },
                                { resourceStartDate2.AddMonths(6), 6m },
                                { resourceStartDate2.AddMonths(7), 6m },
                                { resourceStartDate2.AddMonths(8), 6m },
                                { resourceStartDate2.AddMonths(9), 5m },
                                { resourceStartDate2.AddMonths(10), 5m },
                                { resourceStartDate2.AddMonths(11), 4m },
                                { resourceStartDate2.AddMonths(12), 4m },
                                { resourceStartDate2.AddMonths(13), 4m },
                                { resourceStartDate2.AddMonths(14), 3m },
                                { resourceStartDate2.AddMonths(15), 3m },
                                { resourceStartDate2.AddMonths(16), 2m },
                                { resourceStartDate2.AddMonths(17), 2m },
                                { resourceStartDate2.AddMonths(18), 2m },
                                { resourceStartDate2.AddMonths(19), 1m },
                                { resourceStartDate2.AddMonths(20), 1m },
                                { resourceStartDate2.AddMonths(21), 0m }
                            }
                        },
                        { Element.Resource3, new Dictionary<DateTime, decimal>()
                            {
                                { resourceStartDate3, 10m },
                                { resourceStartDate3.AddMonths(1), 20m },
                                { resourceStartDate3.AddMonths(2), 30m },
                                { resourceStartDate3.AddMonths(3), 0m },
                                { resourceStartDate3.AddMonths(4), 0m },
                                { resourceStartDate3.AddMonths(5), 0m },
                                { resourceStartDate3.AddMonths(6), 0m },
                                { resourceStartDate3.AddMonths(7), 0m },
                                { resourceStartDate3.AddMonths(8), 0m },
                                { resourceStartDate3.AddMonths(9), 0m },
                                { resourceStartDate3.AddMonths(10), 0m },
                                { resourceStartDate3.AddMonths(11), 0m },
                                { resourceStartDate3.AddMonths(12), 0m },
                                { resourceStartDate3.AddMonths(13), 0m },
                                { resourceStartDate3.AddMonths(14), 0m },
                                { resourceStartDate3.AddMonths(15), 0m },
                                { resourceStartDate3.AddMonths(16), 10m },
                                { resourceStartDate3.AddMonths(17), 30m }
                            }
                        },
                        { Element.ResourceNC, new Dictionary<DateTime, decimal>()
                            {
                                { resourceStartDateNC, 0m },
                                { resourceStartDateNC.AddMonths(1), 20m },
                                { resourceStartDateNC.AddMonths(2), 20m },
                                { resourceStartDateNC.AddMonths(3), 40m },
                                { resourceStartDateNC.AddMonths(4), 0m },
                                { resourceStartDateNC.AddMonths(5), 0m },
                                { resourceStartDateNC.AddMonths(6), 0m },
                                { resourceStartDateNC.AddMonths(7), 0m },
                                { resourceStartDateNC.AddMonths(8), 0m },
                                { resourceStartDateNC.AddMonths(9), 0m },
                                { resourceStartDateNC.AddMonths(10), 0m },
                                { resourceStartDateNC.AddMonths(11), 0m },
                                { resourceStartDateNC.AddMonths(12), 0m },
                                { resourceStartDateNC.AddMonths(13), 0m },
                                { resourceStartDateNC.AddMonths(14), 0m },
                                { resourceStartDateNC.AddMonths(15), 0m },
                                { resourceStartDateNC.AddMonths(16), 0m },
                                { resourceStartDateNC.AddMonths(17), 0m },
                                { resourceStartDateNC.AddMonths(18), 0m },
                                { resourceStartDateNC.AddMonths(19), 0m },
                                { resourceStartDateNC.AddMonths(20), 0m },
                                { resourceStartDateNC.AddMonths(21), 0m },
                                { resourceStartDateNC.AddMonths(22), 20m },
                                { resourceStartDateNC.AddMonths(23), 0m }
                            }
                        }
                    }
                });

                // task shift
                testCases.Add(new AdjustDateTestCase
                {
                    TestName = "Task shift",
                    DateShiftLevel = Level.Task,
                    ObjectId = 2,
                    FlowdownType = DateAdjustFlowdownType.Manual,
                    DiscreteType = DateAdjustDiscreteType.NoChange,
                    StartDateShift = -2,
                    EndDateShift = -2,
                    ExpectedDateRange = new Dictionary<Element, Tuple<DateTime?, DateTime?>>()
                    {
                        { Element.Task2, new Tuple<DateTime?, DateTime?>(taskStartDate2.AddMonths(-2), taskEndDate2.AddMonths(-2)) }
                    },
                    ExpectedSpreadResults = new Dictionary<Element, Dictionary<DateTime, decimal>>()
                    {
                        { Element.Resource1, new Dictionary<DateTime, decimal>()
                            {
                                { resourceStartDate1, 0.02m },
                                { resourceStartDate1.AddMonths(1), 0.12m },
                                { resourceStartDate1.AddMonths(2), 0.21m },
                                { resourceStartDate1.AddMonths(3), 0.32m },
                                { resourceStartDate1.AddMonths(4), 0.46m },
                                { resourceStartDate1.AddMonths(5), 0.64m },
                                { resourceStartDate1.AddMonths(6), 0.88m },
                                { resourceStartDate1.AddMonths(7), 1.18m },
                                { resourceStartDate1.AddMonths(8), 1.56m },
                                { resourceStartDate1.AddMonths(9), 2.01m },
                                { resourceStartDate1.AddMonths(10), 2.53m },
                                { resourceStartDate1.AddMonths(11), 3.07m },
                                { resourceStartDate1.AddMonths(12), 3.59m },
                                { resourceStartDate1.AddMonths(13), 4.00m },
                                { resourceStartDate1.AddMonths(14), 4.24m },
                                { resourceStartDate1.AddMonths(15), 4.26m },
                                { resourceStartDate1.AddMonths(16), 4.08m },
                                { resourceStartDate1.AddMonths(17), 3.77m },
                                { resourceStartDate1.AddMonths(18), 3.42m },
                                { resourceStartDate1.AddMonths(19), 3.18m },
                                { resourceStartDate1.AddMonths(20), 3.26m },
                                { resourceStartDate1.AddMonths(21), 3.94m },
                                { resourceStartDate1.AddMonths(22), 5.61m },
                                { resourceStartDate1.AddMonths(23), 8.29m },
                                { resourceStartDate1.AddMonths(24), 10.73m },
                                { resourceStartDate1.AddMonths(25), 10.54m },
                                { resourceStartDate1.AddMonths(26), 7.47m },
                                { resourceStartDate1.AddMonths(27), 4.03m },
                                { resourceStartDate1.AddMonths(28), 1.83m },
                                { resourceStartDate1.AddMonths(29), 0.76m },
                            }
                        },
                        { Element.Resource2, new Dictionary<DateTime, decimal>()
                            {
                                { resourceStartDate2, 9m },
                                { resourceStartDate2.AddMonths(1), 8m },
                                { resourceStartDate2.AddMonths(2), 8m },
                                { resourceStartDate2.AddMonths(3), 7m },
                                { resourceStartDate2.AddMonths(4), 7m },
                                { resourceStartDate2.AddMonths(5), 7m },
                                { resourceStartDate2.AddMonths(6), 6m },
                                { resourceStartDate2.AddMonths(7), 6m },
                                { resourceStartDate2.AddMonths(8), 6m },
                                { resourceStartDate2.AddMonths(9), 5m },
                                { resourceStartDate2.AddMonths(10), 5m },
                                { resourceStartDate2.AddMonths(11), 4m },
                                { resourceStartDate2.AddMonths(12), 4m },
                                { resourceStartDate2.AddMonths(13), 4m },
                                { resourceStartDate2.AddMonths(14), 3m },
                                { resourceStartDate2.AddMonths(15), 3m },
                                { resourceStartDate2.AddMonths(16), 2m },
                                { resourceStartDate2.AddMonths(17), 2m },
                                { resourceStartDate2.AddMonths(18), 2m },
                                { resourceStartDate2.AddMonths(19), 1m },
                                { resourceStartDate2.AddMonths(20), 1m },
                                { resourceStartDate2.AddMonths(21), 0m }
                            }
                        },
                        { Element.Resource3, new Dictionary<DateTime, decimal>()
                            {
                                { resourceStartDate3, 10m },
                                { resourceStartDate3.AddMonths(1), 20m },
                                { resourceStartDate3.AddMonths(2), 30m },
                                { resourceStartDate3.AddMonths(3), 0m },
                                { resourceStartDate3.AddMonths(4), 0m },
                                { resourceStartDate3.AddMonths(5), 0m },
                                { resourceStartDate3.AddMonths(6), 0m },
                                { resourceStartDate3.AddMonths(7), 0m },
                                { resourceStartDate3.AddMonths(8), 0m },
                                { resourceStartDate3.AddMonths(9), 0m },
                                { resourceStartDate3.AddMonths(10), 0m },
                                { resourceStartDate3.AddMonths(11), 0m },
                                { resourceStartDate3.AddMonths(12), 0m },
                                { resourceStartDate3.AddMonths(13), 0m },
                                { resourceStartDate3.AddMonths(14), 0m },
                                { resourceStartDate3.AddMonths(15), 0m },
                                { resourceStartDate3.AddMonths(16), 10m },
                                { resourceStartDate3.AddMonths(17), 30m }
                            }
                        },
                        { Element.ResourceNC, new Dictionary<DateTime, decimal>()
                            {
                                { resourceStartDateNC, 0m },
                                { resourceStartDateNC.AddMonths(1), 20m },
                                { resourceStartDateNC.AddMonths(2), 20m },
                                { resourceStartDateNC.AddMonths(3), 40m },
                                { resourceStartDateNC.AddMonths(4), 0m },
                                { resourceStartDateNC.AddMonths(5), 0m },
                                { resourceStartDateNC.AddMonths(6), 0m },
                                { resourceStartDateNC.AddMonths(7), 0m },
                                { resourceStartDateNC.AddMonths(8), 0m },
                                { resourceStartDateNC.AddMonths(9), 0m },
                                { resourceStartDateNC.AddMonths(10), 0m },
                                { resourceStartDateNC.AddMonths(11), 0m },
                                { resourceStartDateNC.AddMonths(12), 0m },
                                { resourceStartDateNC.AddMonths(13), 0m },
                                { resourceStartDateNC.AddMonths(14), 0m },
                                { resourceStartDateNC.AddMonths(15), 0m },
                                { resourceStartDateNC.AddMonths(16), 0m },
                                { resourceStartDateNC.AddMonths(17), 0m },
                                { resourceStartDateNC.AddMonths(18), 0m },
                                { resourceStartDateNC.AddMonths(19), 0m },
                                { resourceStartDateNC.AddMonths(20), 0m },
                                { resourceStartDateNC.AddMonths(21), 0m },
                                { resourceStartDateNC.AddMonths(22), 20m },
                                { resourceStartDateNC.AddMonths(23), 0m }
                            }
                        }
                    }
                });

                // exception cases
                testCases.Add(new AdjustDateTestCase
                {
                    TestName = "Ex One (shrink)",
                    DateShiftLevel = Level.CLIN,
                    useWorkspace2 = true,
                    FlowdownType = DateAdjustFlowdownType.Automatic,
                    DiscreteType = DateAdjustDiscreteType.NoChange,
                    StartDateShift = 3,
                    EndDateShift = -3,
                    BoeDateOverride = new Dictionary<int, Tuple<DateTime, DateTime>>
                    {
                        { 41, new Tuple<DateTime, DateTime>(Convert.ToDateTime("5/15/16"), Convert.ToDateTime("7/1/16")) },
                        { 42, new Tuple<DateTime, DateTime>(Convert.ToDateTime("2/15/16"), Convert.ToDateTime("2/1/16")) },
                        { 43, new Tuple<DateTime, DateTime>(Convert.ToDateTime("3/15/16"), Convert.ToDateTime("12/1/16")) }
                    },
                    ExpectedDateRange = new Dictionary<Element, Tuple<DateTime?, DateTime?>>
                    {
                        { Element.Clin4, new Tuple<DateTime?, DateTime?>(Convert.ToDateTime("4/1/16"), Convert.ToDateTime("9/1/16")) },
                        { Element.Boe41, new Tuple<DateTime?, DateTime?>(Convert.ToDateTime("5/1/16"), Convert.ToDateTime("7/1/16")) },
                        { Element.Boe42, new Tuple<DateTime?, DateTime?>(Convert.ToDateTime("4/1/16"), Convert.ToDateTime("4/1/16")) },
                        { Element.Boe43, new Tuple<DateTime?, DateTime?>(Convert.ToDateTime("6/1/16"), Convert.ToDateTime("9/1/16")) }
                    }
                });

                testCases.Add(new AdjustDateTestCase
                {
                    TestName = "Ex Two (shift right)",
                    DateShiftLevel = Level.CLIN,
                    useWorkspace2 = true,
                    FlowdownType = DateAdjustFlowdownType.Automatic,
                    DiscreteType = DateAdjustDiscreteType.NoChange,
                    StartDateShift = 3,
                    EndDateShift = 3,
                    BoeDateOverride = new Dictionary<int, Tuple<DateTime, DateTime>>
                    {
                        { 41, new Tuple<DateTime, DateTime>(Convert.ToDateTime("6/15/16"), Convert.ToDateTime("10/1/16")) },
                        { 42, new Tuple<DateTime, DateTime>(Convert.ToDateTime("3/15/16"), Convert.ToDateTime("6/1/16")) },
                        { 43, new Tuple<DateTime, DateTime>(Convert.ToDateTime("1/15/16"), Convert.ToDateTime("7/1/16")) },
                        { 44, new Tuple<DateTime, DateTime>(Convert.ToDateTime("10/15/16"), Convert.ToDateTime("12/1/16")) }
                    },
                    ExpectedDateRange = new Dictionary<Element, Tuple<DateTime?, DateTime?>>
                    {
                        { Element.Clin4, new Tuple<DateTime?, DateTime?>(Convert.ToDateTime("4/1/16"), Convert.ToDateTime("3/1/17")) },
                        { Element.Boe41, new Tuple<DateTime?, DateTime?>(Convert.ToDateTime("9/1/16"), Convert.ToDateTime("1/1/17")) },
                        { Element.Boe42, new Tuple<DateTime?, DateTime?>(Convert.ToDateTime("6/1/16"), Convert.ToDateTime("9/1/16")) },
                        { Element.Boe43, new Tuple<DateTime?, DateTime?>(Convert.ToDateTime("4/1/16"), Convert.ToDateTime("10/1/16")) },
                        { Element.Boe44, new Tuple<DateTime?, DateTime?>(Convert.ToDateTime("1/1/17"), Convert.ToDateTime("3/1/17")) }
                    }
                });

                testCases.Add(new AdjustDateTestCase
                {
                    TestName = "Ex Three (shift left)",
                    DateShiftLevel = Level.CLIN,
                    useWorkspace2 = true,
                    FlowdownType = DateAdjustFlowdownType.Automatic,
                    DiscreteType = DateAdjustDiscreteType.NoChange,
                    StartDateShift = -6,
                    EndDateShift = -6,
                    BoeDateOverride = new Dictionary<int, Tuple<DateTime, DateTime>>
                    {
                        { 41, new Tuple<DateTime, DateTime>(Convert.ToDateTime("1/15/16"), Convert.ToDateTime("3/1/16")) },
                        { 42, new Tuple<DateTime, DateTime>(Convert.ToDateTime("3/15/16"), Convert.ToDateTime("9/1/16")) },
                        { 43, new Tuple<DateTime, DateTime>(Convert.ToDateTime("7/15/16"), Convert.ToDateTime("12/1/16")) }
                    },
                    ExpectedDateRange = new Dictionary<Element, Tuple<DateTime?, DateTime?>>
                    {
                        { Element.Clin4, new Tuple<DateTime?, DateTime?>(Convert.ToDateTime("7/1/15"), Convert.ToDateTime("6/1/16")) },
                        { Element.Boe41, new Tuple<DateTime?, DateTime?>(Convert.ToDateTime("7/1/15"), Convert.ToDateTime("9/1/15")) },
                        { Element.Boe42, new Tuple<DateTime?, DateTime?>(Convert.ToDateTime("9/1/15"), Convert.ToDateTime("3/1/16")) },
                        { Element.Boe43, new Tuple<DateTime?, DateTime?>(Convert.ToDateTime("1/1/16"), Convert.ToDateTime("6/1/16")) }
                    }
                });

                testCases.Add(new AdjustDateTestCase
                {
                    TestName = "Ex Four (shift right)",
                    DateShiftLevel = Level.CLIN,
                    useWorkspace2 = true,
                    FlowdownType = DateAdjustFlowdownType.Automatic,
                    DiscreteType = DateAdjustDiscreteType.NoChange,
                    StartDateShift = 12,
                    EndDateShift = 12,
                    BoeDateOverride = new Dictionary<int, Tuple<DateTime, DateTime>>
                    {
                        { 41, new Tuple<DateTime, DateTime>(Convert.ToDateTime("1/15/16"), Convert.ToDateTime("6/1/16")) },
                        { 42, new Tuple<DateTime, DateTime>(Convert.ToDateTime("7/15/16"), Convert.ToDateTime("12/1/16")) },
                        { 43, new Tuple<DateTime, DateTime>(Convert.ToDateTime("4/15/16"), Convert.ToDateTime("9/1/16")) },
                        { 44, new Tuple<DateTime, DateTime>(Convert.ToDateTime("1/15/16"), Convert.ToDateTime("12/1/16")) }
                    },
                    ExpectedDateRange = new Dictionary<Element, Tuple<DateTime?, DateTime?>>
                    {
                        { Element.Clin4, new Tuple<DateTime?, DateTime?>(Convert.ToDateTime("1/1/17"), Convert.ToDateTime("12/1/17")) },
                        { Element.Boe41, new Tuple<DateTime?, DateTime?>(Convert.ToDateTime("1/1/17"), Convert.ToDateTime("6/1/17")) },
                        { Element.Boe42, new Tuple<DateTime?, DateTime?>(Convert.ToDateTime("7/1/17"), Convert.ToDateTime("12/1/17")) },
                        { Element.Boe43, new Tuple<DateTime?, DateTime?>(Convert.ToDateTime("4/1/17"), Convert.ToDateTime("9/1/17")) },
                        { Element.Boe44, new Tuple<DateTime?, DateTime?>(Convert.ToDateTime("1/1/17"), Convert.ToDateTime("12/1/17")) }
                    }
                });

                testCases.Add(new AdjustDateTestCase
                {
                    TestName = "Ex Five (shrink)",
                    DateShiftLevel = Level.CLIN,
                    useWorkspace2 = true,
                    FlowdownType = DateAdjustFlowdownType.Automatic,
                    DiscreteType = DateAdjustDiscreteType.NoChange,
                    StartDateShift = 3,
                    EndDateShift = -3,
                    BoeDateOverride = new Dictionary<int, Tuple<DateTime, DateTime>>
                    {
                        { 41, new Tuple<DateTime, DateTime>(Convert.ToDateTime("4/15/16"), Convert.ToDateTime("4/1/16")) },
                        { 42, new Tuple<DateTime, DateTime>(Convert.ToDateTime("11/15/16"), Convert.ToDateTime("11/1/16")) },
                        { 43, new Tuple<DateTime, DateTime>(Convert.ToDateTime("7/15/16"), Convert.ToDateTime("7/1/16")) },
                        { 44, new Tuple<DateTime, DateTime>(Convert.ToDateTime("2/15/16"), Convert.ToDateTime("2/1/16")) }
                    },
                    ExpectedDateRange = new Dictionary<Element, Tuple<DateTime?, DateTime?>>
                    {
                        { Element.Clin4, new Tuple<DateTime?, DateTime?>(Convert.ToDateTime("4/1/16"), Convert.ToDateTime("9/1/16")) },
                        { Element.Boe41, new Tuple<DateTime?, DateTime?>(Convert.ToDateTime("4/1/16"), Convert.ToDateTime("4/1/16")) },
                        { Element.Boe42, new Tuple<DateTime?, DateTime?>(Convert.ToDateTime("9/1/16"), Convert.ToDateTime("9/1/16")) },
                        { Element.Boe43, new Tuple<DateTime?, DateTime?>(Convert.ToDateTime("7/1/16"), Convert.ToDateTime("7/1/16")) },
                        { Element.Boe44, new Tuple<DateTime?, DateTime?>(Convert.ToDateTime("4/1/16"), Convert.ToDateTime("4/1/16")) }
                    }
                });

                testCases.Add(new AdjustDateTestCase
                {
                    TestName = "Ex Six (shrink)",
                    DateShiftLevel = Level.CLIN,
                    useWorkspace2 = true,
                    FlowdownType = DateAdjustFlowdownType.Automatic,
                    DiscreteType = DateAdjustDiscreteType.NoChange,
                    StartDateShift = 3,
                    EndDateShift = -3,
                    BoeDateOverride = new Dictionary<int, Tuple<DateTime, DateTime>>
                    {
                        { 41, new Tuple<DateTime, DateTime>(Convert.ToDateTime("5/15/16"), Convert.ToDateTime("7/1/16")) },
                        { 42, new Tuple<DateTime, DateTime>(Convert.ToDateTime("2/15/16"), Convert.ToDateTime("11/1/16")) },
                        { 43, new Tuple<DateTime, DateTime>(Convert.ToDateTime("2/15/16"), Convert.ToDateTime("8/1/16")) },
                        { 44, new Tuple<DateTime, DateTime>(Convert.ToDateTime("3/15/16"), Convert.ToDateTime("6/1/16")) },
                        { 45, new Tuple<DateTime, DateTime>(Convert.ToDateTime("7/15/16"), Convert.ToDateTime("11/1/16")) }
                    },
                    ExpectedDateRange = new Dictionary<Element, Tuple<DateTime?, DateTime?>>
                    {
                        { Element.Clin4, new Tuple<DateTime?, DateTime?>(Convert.ToDateTime("4/1/16"), Convert.ToDateTime("9/1/16")) },
                        { Element.Boe41, new Tuple<DateTime?, DateTime?>(Convert.ToDateTime("5/1/16"), Convert.ToDateTime("7/1/16")) },
                        { Element.Boe42, new Tuple<DateTime?, DateTime?>(Convert.ToDateTime("5/1/16"), Convert.ToDateTime("8/1/16")) },
                        { Element.Boe43, new Tuple<DateTime?, DateTime?>(Convert.ToDateTime("5/1/16"), Convert.ToDateTime("5/1/16")) },
                        { Element.Boe44, new Tuple<DateTime?, DateTime?>(Convert.ToDateTime("4/1/16"), Convert.ToDateTime("7/1/16")) },
                        { Element.Boe45, new Tuple<DateTime?, DateTime?>(Convert.ToDateTime("5/1/16"), Convert.ToDateTime("9/1/16")) },
                    }
                });
                
                testCases.Add(new AdjustDateTestCase
                {
                    TestName = "Ex Seven (shift right and shrink)",
                    DateShiftLevel = Level.CLIN,
                    useWorkspace2 = true,
                    FlowdownType = DateAdjustFlowdownType.Automatic,
                    DiscreteType = DateAdjustDiscreteType.NoChange,
                    StartDateShift = 15,
                    EndDateShift = 10,
                    BoeDateOverride = new Dictionary<int, Tuple<DateTime, DateTime>>
                    {
                        { 41, new Tuple<DateTime, DateTime>(Convert.ToDateTime("2/15/16"), Convert.ToDateTime("11/1/16")) },
                        { 42, new Tuple<DateTime, DateTime>(Convert.ToDateTime("2/15/16"), Convert.ToDateTime("8/1/16")) },
                        { 43, new Tuple<DateTime, DateTime>(Convert.ToDateTime("9/15/16"), Convert.ToDateTime("12/1/16")) }
                    },
                    ExpectedDateRange = new Dictionary<Element, Tuple<DateTime?, DateTime?>>
                    {
                        { Element.Clin4, new Tuple<DateTime?, DateTime?>(Convert.ToDateTime("4/1/17"), Convert.ToDateTime("10/1/17")) },
                        { Element.Boe41, new Tuple<DateTime?, DateTime?>(Convert.ToDateTime("5/1/17"), Convert.ToDateTime("9/1/17")) },
                        { Element.Boe42, new Tuple<DateTime?, DateTime?>(Convert.ToDateTime("5/1/17"), Convert.ToDateTime("6/1/17")) },
                        { Element.Boe43, new Tuple<DateTime?, DateTime?>(Convert.ToDateTime("4/1/17"), Convert.ToDateTime("7/1/17")) }
                    }
                });

                testCases.Add(new AdjustDateTestCase
                {
                    TestName = "Ex Eight (shift left and shrink)",
                    DateShiftLevel = Level.CLIN,
                    useWorkspace2 = true,
                    FlowdownType = DateAdjustFlowdownType.Automatic,
                    DiscreteType = DateAdjustDiscreteType.NoChange,
                    StartDateShift = -9,
                    EndDateShift = -14,
                    BoeDateOverride = new Dictionary<int, Tuple<DateTime, DateTime>>
                    {
                        { 41, new Tuple<DateTime, DateTime>(Convert.ToDateTime("2/15/16"), Convert.ToDateTime("11/1/16")) },
                        { 42, new Tuple<DateTime, DateTime>(Convert.ToDateTime("2/15/16"), Convert.ToDateTime("8/1/16")) },
                        { 43, new Tuple<DateTime, DateTime>(Convert.ToDateTime("9/15/16"), Convert.ToDateTime("12/1/16")) }
                    },
                    ExpectedDateRange = new Dictionary<Element, Tuple<DateTime?, DateTime?>>
                    {
                        { Element.Clin4, new Tuple<DateTime?, DateTime?>(Convert.ToDateTime("4/1/15"), Convert.ToDateTime("10/1/15")) },
                        { Element.Boe41, new Tuple<DateTime?, DateTime?>(Convert.ToDateTime("5/1/15"), Convert.ToDateTime("9/1/15")) },
                        { Element.Boe42, new Tuple<DateTime?, DateTime?>(Convert.ToDateTime("5/1/15"), Convert.ToDateTime("6/1/15")) },
                        { Element.Boe43, new Tuple<DateTime?, DateTime?>(Convert.ToDateTime("7/1/15"), Convert.ToDateTime("10/1/15")) }
                    }
                });

                testCases.Add(new AdjustDateTestCase
                {
                    TestName = "Ex Nine (expand)",
                    DateShiftLevel = Level.CLIN,
                    useWorkspace2 = true,
                    FlowdownType = DateAdjustFlowdownType.Automatic,
                    DiscreteType = DateAdjustDiscreteType.NoChange,
                    StartDateShift = -12,
                    EndDateShift = 12,
                    BoeDateOverride = new Dictionary<int, Tuple<DateTime, DateTime>>
                    {
                        { 41, new Tuple<DateTime, DateTime>(Convert.ToDateTime("1/15/16"), Convert.ToDateTime("6/1/16")) },
                        { 42, new Tuple<DateTime, DateTime>(Convert.ToDateTime("1/15/16"), Convert.ToDateTime("12/1/16")) },
                        { 43, new Tuple<DateTime, DateTime>(Convert.ToDateTime("7/15/16"), Convert.ToDateTime("12/1/16")) },
                        { 44, new Tuple<DateTime, DateTime>(Convert.ToDateTime("4/15/16"), Convert.ToDateTime("9/1/16")) }
                    },
                    ExpectedDateRange = new Dictionary<Element, Tuple<DateTime?, DateTime?>>
                    {
                        { Element.Clin4, new Tuple<DateTime?, DateTime?>(Convert.ToDateTime("1/1/15"), Convert.ToDateTime("12/1/17")) },
                        { Element.Boe41, new Tuple<DateTime?, DateTime?>(Convert.ToDateTime("1/1/16"), Convert.ToDateTime("6/1/16")) },
                        { Element.Boe42, new Tuple<DateTime?, DateTime?>(Convert.ToDateTime("1/1/16"), Convert.ToDateTime("12/1/16")) },
                        { Element.Boe43, new Tuple<DateTime?, DateTime?>(Convert.ToDateTime("7/1/16"), Convert.ToDateTime("12/1/16")) },
                        { Element.Boe44, new Tuple<DateTime?, DateTime?>(Convert.ToDateTime("4/1/16"), Convert.ToDateTime("9/1/16")) }
                    }
                });

                testCases.Add(new AdjustDateTestCase
                {
                    TestName = "Ex Ten (shift right and expand)",
                    DateShiftLevel = Level.CLIN,
                    useWorkspace2 = true,
                    FlowdownType = DateAdjustFlowdownType.Automatic,
                    DiscreteType = DateAdjustDiscreteType.NoChange,
                    StartDateShift = 2,
                    EndDateShift = 12,
                    BoeDateOverride = new Dictionary<int, Tuple<DateTime, DateTime>>
                    {
                        { 41, new Tuple<DateTime, DateTime>(Convert.ToDateTime("1/15/16"), Convert.ToDateTime("6/1/16")) },
                        { 42, new Tuple<DateTime, DateTime>(Convert.ToDateTime("7/15/16"), Convert.ToDateTime("12/1/16")) },
                        { 43, new Tuple<DateTime, DateTime>(Convert.ToDateTime("1/15/16"), Convert.ToDateTime("12/1/16")) },
                        { 44, new Tuple<DateTime, DateTime>(Convert.ToDateTime("4/15/16"), Convert.ToDateTime("9/1/16")) }
                    },
                    ExpectedDateRange = new Dictionary<Element, Tuple<DateTime?, DateTime?>>
                    {
                        { Element.Clin4, new Tuple<DateTime?, DateTime?>(Convert.ToDateTime("3/1/16"), Convert.ToDateTime("12/1/17")) },
                        { Element.Boe41, new Tuple<DateTime?, DateTime?>(Convert.ToDateTime("3/1/16"), Convert.ToDateTime("8/1/16")) },
                        { Element.Boe42, new Tuple<DateTime?, DateTime?>(Convert.ToDateTime("7/1/16"), Convert.ToDateTime("12/1/16")) },
                        { Element.Boe43, new Tuple<DateTime?, DateTime?>(Convert.ToDateTime("3/1/16"), Convert.ToDateTime("2/1/17")) },
                        { Element.Boe44, new Tuple<DateTime?, DateTime?>(Convert.ToDateTime("4/1/16"), Convert.ToDateTime("9/1/16")) }
                    }
                });

                testCases.Add(new AdjustDateTestCase
                {
                    TestName = "Ex Eleven (shift right and expand)",
                    DateShiftLevel = Level.CLIN,
                    useWorkspace2 = true,
                    FlowdownType = DateAdjustFlowdownType.Automatic,
                    DiscreteType = DateAdjustDiscreteType.NoChange,
                    StartDateShift = 9,
                    EndDateShift = 12,
                    BoeDateOverride = new Dictionary<int, Tuple<DateTime, DateTime>>
                    {
                        { 41, new Tuple<DateTime, DateTime>(Convert.ToDateTime("1/15/16"), Convert.ToDateTime("6/1/16")) },
                        { 42, new Tuple<DateTime, DateTime>(Convert.ToDateTime("7/15/16"), Convert.ToDateTime("12/1/16")) },
                        { 43, new Tuple<DateTime, DateTime>(Convert.ToDateTime("1/15/16"), Convert.ToDateTime("12/1/16")) },
                        { 44, new Tuple<DateTime, DateTime>(Convert.ToDateTime("4/15/16"), Convert.ToDateTime("9/1/16")) }
                    },
                    ExpectedDateRange = new Dictionary<Element, Tuple<DateTime?, DateTime?>>
                    {
                        { Element.Clin4, new Tuple<DateTime?, DateTime?>(Convert.ToDateTime("10/1/16"), Convert.ToDateTime("12/1/17")) },
                        { Element.Boe41, new Tuple<DateTime?, DateTime?>(Convert.ToDateTime("10/1/16"), Convert.ToDateTime("3/1/17")) },
                        { Element.Boe42, new Tuple<DateTime?, DateTime?>(Convert.ToDateTime("10/1/16"), Convert.ToDateTime("3/1/17")) },
                        { Element.Boe43, new Tuple<DateTime?, DateTime?>(Convert.ToDateTime("10/1/16"), Convert.ToDateTime("9/1/17")) },
                        { Element.Boe44, new Tuple<DateTime?, DateTime?>(Convert.ToDateTime("10/1/16"), Convert.ToDateTime("3/1/17")) }
                    }
                });
                
                testCases.Add(new AdjustDateTestCase
                {
                    TestName = "Ex Twelve (shift left and expand)",
                    DateShiftLevel = Level.CLIN,
                    useWorkspace2 = true,
                    FlowdownType = DateAdjustFlowdownType.Automatic,
                    DiscreteType = DateAdjustDiscreteType.NoChange,
                    StartDateShift = -12,
                    EndDateShift = -3,
                    BoeDateOverride = new Dictionary<int, Tuple<DateTime, DateTime>>
                    {
                        { 41, new Tuple<DateTime, DateTime>(Convert.ToDateTime("1/15/16"), Convert.ToDateTime("6/1/16")) },
                        { 42, new Tuple<DateTime, DateTime>(Convert.ToDateTime("7/15/16"), Convert.ToDateTime("12/1/16")) },
                        { 43, new Tuple<DateTime, DateTime>(Convert.ToDateTime("1/15/16"), Convert.ToDateTime("12/1/16")) },
                        { 44, new Tuple<DateTime, DateTime>(Convert.ToDateTime("4/15/16"), Convert.ToDateTime("9/1/16")) }
                    },
                    ExpectedDateRange = new Dictionary<Element, Tuple<DateTime?, DateTime?>>
                    {
                        { Element.Clin4, new Tuple<DateTime?, DateTime?>(Convert.ToDateTime("1/1/15"), Convert.ToDateTime("9/1/16")) },
                        { Element.Boe41, new Tuple<DateTime?, DateTime?>(Convert.ToDateTime("1/1/16"), Convert.ToDateTime("6/1/16")) },
                        { Element.Boe42, new Tuple<DateTime?, DateTime?>(Convert.ToDateTime("4/1/16"), Convert.ToDateTime("9/1/16")) },
                        { Element.Boe43, new Tuple<DateTime?, DateTime?>(Convert.ToDateTime("10/1/15"), Convert.ToDateTime("9/1/16")) },
                        { Element.Boe44, new Tuple<DateTime?, DateTime?>(Convert.ToDateTime("4/1/16"), Convert.ToDateTime("9/1/16")) }
                    }
                });

                testCases.Add(new AdjustDateTestCase
                {
                    TestName = "Ex Thirteen (shift left and expand)",
                    DateShiftLevel = Level.CLIN,
                    useWorkspace2 = true,
                    FlowdownType = DateAdjustFlowdownType.Automatic,
                    DiscreteType = DateAdjustDiscreteType.NoChange,
                    StartDateShift = -12,
                    EndDateShift = -9,
                    BoeDateOverride = new Dictionary<int, Tuple<DateTime, DateTime>>
                    {
                        { 41, new Tuple<DateTime, DateTime>(Convert.ToDateTime("1/15/16"), Convert.ToDateTime("6/1/16")) },
                        { 42, new Tuple<DateTime, DateTime>(Convert.ToDateTime("7/15/16"), Convert.ToDateTime("12/1/16")) },
                        { 43, new Tuple<DateTime, DateTime>(Convert.ToDateTime("1/15/16"), Convert.ToDateTime("12/1/16")) },
                        { 44, new Tuple<DateTime, DateTime>(Convert.ToDateTime("4/15/16"), Convert.ToDateTime("9/1/16")) }
                    },
                    ExpectedDateRange = new Dictionary<Element, Tuple<DateTime?, DateTime?>>
                    {
                        { Element.Clin4, new Tuple<DateTime?, DateTime?>(Convert.ToDateTime("1/1/15"), Convert.ToDateTime("3/1/16")) },
                        { Element.Boe41, new Tuple<DateTime?, DateTime?>(Convert.ToDateTime("10/1/15"), Convert.ToDateTime("3/1/16")) },
                        { Element.Boe42, new Tuple<DateTime?, DateTime?>(Convert.ToDateTime("10/1/15"), Convert.ToDateTime("3/1/16")) },
                        { Element.Boe43, new Tuple<DateTime?, DateTime?>(Convert.ToDateTime("4/1/15"), Convert.ToDateTime("3/1/16")) },
                        { Element.Boe44, new Tuple<DateTime?, DateTime?>(Convert.ToDateTime("10/1/15"), Convert.ToDateTime("3/1/16")) }
                    }
                });

                testCases.Add(new AdjustDateTestCase
                {
                    TestName = "Ex Fourteen (over shrink)",
                    DateShiftLevel = Level.CLIN,
                    useWorkspace2 = true,
                    FlowdownType = DateAdjustFlowdownType.Automatic,
                    DiscreteType = DateAdjustDiscreteType.NoChange,
                    StartDateShift = 0,
                    EndDateShift = -7,
                    BoeDateOverride = new Dictionary<int, Tuple<DateTime, DateTime>>
                    {
                        { 41, new Tuple<DateTime, DateTime>(Convert.ToDateTime("4/15/16"), Convert.ToDateTime("9/1/16")) }
                    },
                    ExpectedDateRange = new Dictionary<Element, Tuple<DateTime?, DateTime?>>
                    {
                        { Element.Clin4, new Tuple<DateTime?, DateTime?>(Convert.ToDateTime("1/1/16"), Convert.ToDateTime("5/1/16")) },
                        { Element.Boe41, new Tuple<DateTime?, DateTime?>(Convert.ToDateTime("1/1/16"), Convert.ToDateTime("5/1/16")) }
                    }
                });

                testCases.Add(new AdjustDateTestCase
                {
                    TestName = "Ex Fifteen (over shrink)",
                    DateShiftLevel = Level.CLIN,
                    useWorkspace2 = true,
                    FlowdownType = DateAdjustFlowdownType.Automatic,
                    DiscreteType = DateAdjustDiscreteType.NoChange,
                    StartDateShift = 7,
                    EndDateShift = 0,
                    BoeDateOverride = new Dictionary<int, Tuple<DateTime, DateTime>>
                    {
                        { 41, new Tuple<DateTime, DateTime>(Convert.ToDateTime("4/15/16"), Convert.ToDateTime("9/1/16")) }
                    },
                    ExpectedDateRange = new Dictionary<Element, Tuple<DateTime?, DateTime?>>
                    {
                        { Element.Clin4, new Tuple<DateTime?, DateTime?>(Convert.ToDateTime("8/1/16"), Convert.ToDateTime("12/1/16")) },
                        { Element.Boe41, new Tuple<DateTime?, DateTime?>(Convert.ToDateTime("8/1/16"), Convert.ToDateTime("12/1/16")) }
                    }
                });
            }
            return testCases;
        }

        #endregion Test Cases
    };    
}
