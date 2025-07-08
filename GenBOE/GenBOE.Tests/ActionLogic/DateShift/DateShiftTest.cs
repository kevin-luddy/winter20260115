// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.ActionLogic
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using GenBOE.ActionLogic.DateShift;
    using GenBOE.DataBridge.Common;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common;
    using IES.Common.classes;
    using Microsoft.Practices.Unity;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    /// <summary>
    /// Tests for DateShift
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode"), TestClass]
    public class DateShiftTest
    {
        private static Mock<IFullObjectFactory> factory = new Mock<IFullObjectFactory>();
        private static Mock<IRetriever> retriever = new Mock<IRetriever>();
        private static Mock<IPermissionsDTODataLoader> _permissions = new Mock<IPermissionsDTODataLoader>();
        private static Mock<ICommonDataMapper> commonDataMapper = new Mock<ICommonDataMapper>();
        private static DateTime StartDate = new DateTime(2019, 1, 15).Normalize();
        private static DateTime EndDate = new DateTime(2019, 6, 15).Normalize();
        private const decimal DISCRETE_HOURS = 250;
		private static Mock<IActiveDirectoryUtilities> activeDirectoryUtilities = new Mock<IActiveDirectoryUtilities>();
		private static Dictionary<Operation, Dictionary<int, ICollection<Tuple<DateTime, DateTime>>>> FlowdownExpectedResults = new Dictionary<Operation, Dictionary<int, ICollection<Tuple<DateTime, DateTime>>>>();

        /// <summary>
        /// Initializes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), _permissions.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), commonDataMapper.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IActiveDirectoryUtilities), activeDirectoryUtilities.Object);

			FlowdownExpectedResults.Add(Operation.Shift, new Dictionary<int, ICollection<Tuple<DateTime, DateTime>>>());
            FlowdownExpectedResults.Add(Operation.DurationChange, new Dictionary<int, ICollection<Tuple<DateTime, DateTime>>>());

            DateShiftDTO parent = CreateParentShiftable();
            for (int i = 1; i < 13; i++)
            {
                FlowdownExpectedResults[Operation.Shift].Add(i,
                    parent.Children.Select(c => new Tuple<DateTime, DateTime>(c.StartDate.Value.AddMonths(i), c.EndDate.Value.AddMonths(i))).ToList()
                );
                FlowdownExpectedResults[Operation.Shift].Add(-1 * i,
                    parent.Children.Select(c => new Tuple<DateTime, DateTime>(c.StartDate.Value.AddMonths(-1 * i), c.EndDate.Value.AddMonths(-1 * i))).ToList()
                );
                FlowdownExpectedResults[Operation.DurationChange].Add(i,
                    parent.Children.Select(c => new Tuple<DateTime, DateTime>(c.StartDate.Value.AddMonths(0), c.EndDate.Value.AddMonths(i))).ToList()
                );

                if (i < 4)
                {
                    FlowdownExpectedResults[Operation.DurationChange].Add(-1 * i,
                        parent.Children.Select(c => new Tuple<DateTime, DateTime>(c.StartDate.Value.AddMonths(0), c.EndDate.Value.AddMonths(-1 * i))).ToList()
                    );
                }
            }
        }

        /// <summary>
        /// Gets an instance of a workspace
        /// </summary>
        /// <returns></returns>
        private static FullWorkspace GetWorkspace()
        {
            return new FullWorkspace();
        }

        /// <summary>
        /// Creates the shiftable.
        /// </summary>
        /// <returns></returns>
        private static DateShiftDTO CreateShiftable()
        {
            DateShiftDTO dto = new DateShiftDTO();
            dto.StartDate = StartDate.Normalize();
            dto.EndDate = dto.StartDate.Value.AddMonths(5);

            return dto;
        }

        /// <summary>
        /// Creates the discrete parent shiftable.
        /// </summary>
        /// <returns></returns>
        private static DateShiftDTO CreateDiscreteParentShiftable()
        {
			DateShiftDTO task = new DateShiftDTO
			{
                BoeId = 678,
                StartDate = StartDate,
                EndDate = EndDate,
                TaskElementLabors = new Collection<ResourceTypeDto>
                {
                    new ResourceTypeDto
                    {
                        StartDate = StartDate,
                        EndDate = EndDate,
                        SpreadCurveID = SpreadCurves.DiscreteHours,
                        SpreadType = SpreadType.Hours,
                        LaborSpreads = SpreadCurve.CalculateLaborSpreadsBasedOnCurve(new LaborSpreadRequest(SpreadCurves.SpreadCurve3, StartDate, EndDate, DISCRETE_HOURS), 0),
                        ValueSpread = DISCRETE_HOURS
                    }
                }
            };

            return task;
        }

        /// <summary>
        /// Creates the parent shiftable.
        /// </summary>
        /// <returns></returns>
        private static DateShiftDTO CreateParentShiftable()
        {
			DateShiftDTO dto = new DateShiftDTO();
            dto.Id = 678;
            dto.StartDate = StartDate.Normalize();
            dto.EndDate = dto.StartDate.Value.AddMonths(5);

            dto.Children.AddRange(new List<DateShiftDTO>
            {
                new DateShiftDTO
				{
					BoeId = dto.Id,
                    StartDate = dto.StartDate.Value.AddMonths(1),
                    EndDate = dto.EndDate.Value.AddMonths(0)
                },
                new DateShiftDTO
				{
					BoeId = dto.Id,
                    StartDate = dto.StartDate.Value.AddMonths(0),
                    EndDate = dto.EndDate.Value.AddMonths(0)
                },
                new DateShiftDTO
				{
					BoeId = dto.Id,
                    StartDate = dto.StartDate.Value.AddMonths(0),
                    EndDate = dto.EndDate.Value.AddMonths(-1)
                },
                new DateShiftDTO
				{
					BoeId = dto.Id,
                    StartDate = dto.StartDate.Value.AddMonths(1),
                    EndDate = dto.EndDate.Value.AddMonths(-1)
                },
                new DateShiftDTO
				{
					BoeId = dto.Id,
                    StartDate = dto.StartDate.Value.AddMonths(1),
                    EndDate = dto.StartDate.Value.AddMonths(1)
                },
                new DateShiftDTO
				{
					BoeId = dto.Id,
                    StartDate = dto.StartDate.Value.AddMonths(0),
                    EndDate = dto.StartDate.Value.AddMonths(0)
                },
                new DateShiftDTO
				{
                    BoeId = dto.Id,
                    StartDate = dto.EndDate.Value.AddMonths(0),
                    EndDate = dto.EndDate.Value.AddMonths(0)
                }
            });

            return dto;
        }

        /// <summary>
        /// Creates the shift.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="monthChange">The month change.</param>
        /// <returns></returns>
        private static DateShiftDetailModelView CreateShift(ChildModificationType type, int monthChange)
        {
            return new DateShiftDetailModelView
            {
                ChildModificationType = type,
                Error1FixSingleMonth = false,
                Error2Handling = ChildModificationType.NoChange,
                MonthChange = monthChange,
                Operation = Operation.Shift,
                SpreadHandling = SpreadHandling.DiscreteToError
            };
        }

        /// <summary>
        /// Creates the expand right.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        private static DateShiftDetailModelView CreateExpandRight(ChildModificationType type)
        {
            return new DateShiftDetailModelView
            {
                ChildModificationType = type,
                Error1FixSingleMonth = false,
                Error2Handling = ChildModificationType.NoChange,
                MonthChange = 3,
                Operation = Operation.DurationChange,
                SpreadHandling = SpreadHandling.DiscreteToError
            };
        }

        /// <summary>
        /// Creates the shrink left.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        private static DateShiftDetailModelView CreateShrinkLeft(ChildModificationType type)
        {
            return new DateShiftDetailModelView
            {
                ChildModificationType = type,
                Error1FixSingleMonth = false,
                Error2Handling = ChildModificationType.NoChange,
                MonthChange = -3,
                Operation = Operation.DurationChange,
                SpreadHandling = SpreadHandling.DiscreteToError
            };
        }

        /// <summary>
        /// Test for DateShift
        /// </summary>
        [TestMethod]
        public void ShiftRight()
        {
            for (int i = 1; i < 13; i++)
            {
                DateShiftDTO obj = CreateShiftable();
                DateShiftDetailModelView detail = CreateShift(ChildModificationType.FlowDown, i);

                DateShiftCalculation.PerformShiftOperation(obj, detail);

				DateShiftDTO obj2 = CreateShiftable();

                Assert.AreEqual(obj2.StartDate.Value.AddMonths(detail.MonthChange), obj.StartDate.Value);
                Assert.AreEqual(obj2.EndDate.Value.AddMonths(detail.MonthChange), obj.EndDate.Value);
            }
        }

        /// <summary>
        /// Test for DateShift
        /// </summary>
        [TestMethod]
        public void ShiftLeft()
        {
            for (int i = 1; i < 4; i++)
            {
				DateShiftDTO obj = CreateShiftable();
                DateShiftDetailModelView detail = CreateShift(ChildModificationType.FlowDown, -1 * i);

                DateShiftCalculation.PerformShiftOperation(obj, detail);

				DateShiftDTO obj2 = CreateShiftable();

                Assert.AreEqual(obj2.StartDate.Value.AddMonths(detail.MonthChange), obj.StartDate.Value);
                Assert.AreEqual(obj2.EndDate.Value.AddMonths(detail.MonthChange), obj.EndDate.Value);
            }
        }

        /// <summary>
        /// Test for DateShift
        /// </summary>
        [TestMethod]
        public void ExpandRight()
        {
			DateShiftDTO obj = CreateShiftable();
            DateShiftDetailModelView detail = CreateExpandRight(ChildModificationType.FlowDown);

            DateShiftCalculation.PerformShiftOperation(obj, detail);

			DateShiftDTO obj2 = CreateShiftable();

            Assert.AreEqual(obj2.StartDate.Value, obj.StartDate.Value);
            Assert.AreEqual(obj2.EndDate.Value.AddMonths(detail.MonthChange), obj.EndDate.Value);
        }

        /// <summary>
        /// Test for DateShift
        /// </summary>
        [TestMethod]
        public void ShrinkLeft()
        {
			DateShiftDTO obj = CreateShiftable();
            DateShiftDetailModelView detail = CreateShrinkLeft(ChildModificationType.FlowDown);

            DateShiftCalculation.PerformShiftOperation(obj, detail);

			DateShiftDTO obj2 = CreateShiftable();

            Assert.AreEqual(obj2.StartDate.Value, obj.StartDate.Value);
            Assert.AreEqual(obj2.EndDate.Value.AddMonths(detail.MonthChange), obj.EndDate.Value);
        }

        #region Children Flowdown

        /// <summary>
        /// Test for DateShift
        /// </summary>
        [TestMethod]
        public void ShiftRightChildrenFlowdown()
        {
            for (int i = 1; i < 13; i++)
            {
				DateShiftDTO obj = CreateParentShiftable();
                DateShiftDetailModelView detail = CreateShift(ChildModificationType.FlowDown, i);

                DateShiftCalculation.PerformShifts(obj, new DateShiftModelView { Details = new DateShiftDetailModelView[] { detail } , Workspace = GetWorkspace() }, null, null, null, Level.Workspace, string.Empty);

                DateShiftDTO obj2 = CreateParentShiftable();

                Assert.AreEqual(obj2.StartDate.Value.AddMonths(detail.MonthChange), obj.StartDate.Value);
                Assert.AreEqual(obj2.EndDate.Value.AddMonths(detail.MonthChange), obj.EndDate.Value);
                Assert.IsFalse(detail.Errors.AffectedBoeIds.Any());

                CompareChildren(obj2, obj, detail);
            }
        }

        /// <summary>
        /// Test for DateShift
        /// </summary>
        [TestMethod]
        public void ShiftLeftChildrenFlowdown()
        {
            for (int i = 1; i < 4; i++)
            {
				DateShiftDTO obj = CreateParentShiftable();
                DateShiftDetailModelView detail = CreateShift(ChildModificationType.FlowDown, -1 * i);

                DateShiftCalculation.PerformShifts(obj, new DateShiftModelView { Details = new DateShiftDetailModelView[] { detail } , Workspace = GetWorkspace() }, null, null, null, Level.Workspace, string.Empty);

				DateShiftDTO obj2 = CreateParentShiftable();

                Assert.AreEqual(obj2.StartDate.Value.AddMonths(detail.MonthChange), obj.StartDate.Value);
                Assert.AreEqual(obj2.EndDate.Value.AddMonths(detail.MonthChange), obj.EndDate.Value);
                Assert.IsFalse(detail.Errors.AffectedBoeIds.Any());

                CompareChildren(obj2, obj, detail);
            }
        }

        /// <summary>
        /// Test for DateShift
        /// </summary>
        [TestMethod]
        public void ExpandRightChildrenFlowdown()
        {
            DateShiftDTO obj = CreateParentShiftable();
            DateShiftDetailModelView detail = CreateExpandRight(ChildModificationType.FlowDown);

            DateShiftCalculation.PerformShifts(obj, new DateShiftModelView { Details = new DateShiftDetailModelView[] { detail } , Workspace = GetWorkspace() }, null, null, null, Level.Workspace, string.Empty);

			DateShiftDTO obj2 = CreateParentShiftable();

            Assert.AreEqual(obj2.StartDate.Value, obj.StartDate.Value);
            Assert.AreEqual(obj2.EndDate.Value.AddMonths(detail.MonthChange), obj.EndDate.Value);
            Assert.IsFalse(detail.Errors.AffectedBoeIds.Any());

            CompareChildren(obj2, obj, detail);
        }

        /// <summary>
        /// Test for DateShift
        /// </summary>
        [TestMethod]
        public void ShrinkLeftChildrenFlowdown()
        {
			DateShiftDTO obj = CreateParentShiftable();
            DateShiftDetailModelView detail = CreateShrinkLeft(ChildModificationType.FlowDown);

            DateShiftCalculation.PerformShifts(obj, new DateShiftModelView { Details = new DateShiftDetailModelView[] { detail } , Workspace = GetWorkspace() }, null, null, null, Level.Workspace, string.Empty);

			DateShiftDTO obj2 = CreateParentShiftable();

            Assert.AreEqual(obj2.StartDate.Value, obj.StartDate.Value);
            Assert.AreEqual(obj2.EndDate.Value.AddMonths(detail.MonthChange), obj.EndDate.Value);
            
            CompareChildren(obj2, obj, detail);

            Assert.IsTrue(detail.Errors.HasError1);
            Assert.IsFalse(detail.Errors.HasError2);
            Assert.IsTrue(detail.Errors.AffectedBoeIds.Any());

            // try again with error1 fixed
            obj = CreateParentShiftable();
            detail = CreateShrinkLeft(ChildModificationType.FlowDown);
            detail.Error1FixSingleMonth = true;
            DateShiftCalculation.PerformShifts(obj, new DateShiftModelView { Details = new DateShiftDetailModelView[] { detail } , Workspace = GetWorkspace() }, null, null, null, Level.Workspace, string.Empty);

            Assert.AreEqual(obj2.StartDate.Value, obj.StartDate.Value);
            Assert.AreEqual(obj2.EndDate.Value.AddMonths(detail.MonthChange), obj.EndDate.Value);

            CompareChildren(obj2, obj, detail);

            Assert.IsFalse(detail.Errors.HasError1);
            Assert.IsFalse(detail.Errors.HasError2);
            Assert.IsTrue(detail.Errors.AffectedBoeIds.Any());

            // try again with error2 

            obj = CreateParentShiftable();
            detail = CreateShift(ChildModificationType.NoChange, -6);
            detail.Error2Handling = null;
            DateShiftCalculation.PerformShifts(obj, new DateShiftModelView { Details = new DateShiftDetailModelView[] { detail } , Workspace = GetWorkspace() }, null, null, null, Level.Workspace, string.Empty);

            Assert.AreEqual(obj2.StartDate.Value.AddMonths(detail.MonthChange), obj.StartDate.Value);
            Assert.AreEqual(obj2.EndDate.Value.AddMonths(detail.MonthChange), obj.EndDate.Value);

            CompareChildren(obj2, obj, detail);

            Assert.IsFalse(detail.Errors.HasError1);
            Assert.IsTrue(detail.Errors.HasError2);
            Assert.IsTrue(detail.Errors.AffectedBoeIds.Any());

            // try again with error2 fixed
            obj = CreateParentShiftable();
            detail = CreateShift(ChildModificationType.NoChange, -6);
            detail.Error2Handling = ChildModificationType.NoChange;
            DateShiftCalculation.PerformShifts(obj, new DateShiftModelView { Details = new DateShiftDetailModelView[] { detail } , Workspace = GetWorkspace() }, null, null, null, Level.Workspace, string.Empty);

            Assert.AreEqual(obj2.StartDate.Value.AddMonths(detail.MonthChange), obj.StartDate.Value);
            Assert.AreEqual(obj2.EndDate.Value.AddMonths(detail.MonthChange), obj.EndDate.Value);

            CompareChildren(obj2, obj, detail);

            Assert.IsFalse(detail.Errors.HasError1);
            Assert.IsFalse(detail.Errors.HasError2);
            Assert.IsTrue(detail.Errors.AffectedBoeIds.Any());

            // try again with error2 fixed
            obj = CreateParentShiftable();
            detail = CreateShift(ChildModificationType.NoChange, -6);
            detail.Error2Handling = ChildModificationType.ToPoP;
            DateShiftCalculation.PerformShifts(obj, new DateShiftModelView { Details = new DateShiftDetailModelView[] { detail } , Workspace = GetWorkspace() }, null, null, null, Level.Workspace, string.Empty);

            Assert.AreEqual(obj2.StartDate.Value.AddMonths(detail.MonthChange), obj.StartDate.Value);
            Assert.AreEqual(obj2.EndDate.Value.AddMonths(detail.MonthChange), obj.EndDate.Value);

            CompareChildren(obj2, obj, detail);

            Assert.IsFalse(detail.Errors.HasError1);
            Assert.IsFalse(detail.Errors.HasError2);
            Assert.IsFalse(detail.Errors.AffectedBoeIds.Any());

            // try again with error2 fixed
            obj = CreateParentShiftable();
            detail = CreateShift(ChildModificationType.NoChange, -6);
            detail.Error2Handling = ChildModificationType.ToEnd;
            DateShiftCalculation.PerformShifts(obj, new DateShiftModelView { Details = new DateShiftDetailModelView[] { detail } , Workspace = GetWorkspace() }, null, null, null, Level.Workspace, string.Empty);

            Assert.AreEqual(obj2.StartDate.Value.AddMonths(detail.MonthChange), obj.StartDate.Value);
            Assert.AreEqual(obj2.EndDate.Value.AddMonths(detail.MonthChange), obj.EndDate.Value);

            CompareChildren(obj2, obj, detail);

            Assert.IsFalse(detail.Errors.HasError1);
            Assert.IsFalse(detail.Errors.HasError2);
            Assert.IsFalse(detail.Errors.AffectedBoeIds.Any());
        }

        /// <summary>
        /// Test for DateShift
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void Error2NotSupported()
        {
			DateShiftDTO obj = CreateParentShiftable();
            DateShiftDetailModelView detail = CreateShift(ChildModificationType.NoChange, -6);
            detail.Error2Handling = ChildModificationType.FlowDown;
            DateShiftCalculation.PerformShifts(obj, new DateShiftModelView { Details = new DateShiftDetailModelView[] { detail } , Workspace = GetWorkspace() }, null, null, null, Level.Workspace, string.Empty);
        }

        #endregion Children Flowdown

        #region Children NoChange

        /// <summary>
        /// Test for DateShift
        /// </summary>
        [TestMethod]
        public void ShiftRightChildrenNoChange()
        {
            for (int i = 1; i < 13; i++)
            {
				DateShiftDTO obj = CreateParentShiftable();
                DateShiftDetailModelView detail = CreateShift(ChildModificationType.NoChange, i);

                DateShiftCalculation.PerformShifts(obj, new DateShiftModelView { Details = new DateShiftDetailModelView[] { detail } , Workspace = GetWorkspace() }, null, null, null, Level.Workspace, string.Empty);

				DateShiftDTO obj2 = CreateParentShiftable();

                Assert.AreEqual(obj2.StartDate.Value.AddMonths(detail.MonthChange), obj.StartDate.Value);
                Assert.AreEqual(obj2.EndDate.Value.AddMonths(detail.MonthChange), obj.EndDate.Value);

                CompareChildren(obj2, obj, detail);
            }
        }

        /// <summary>
        /// Test for DateShift
        /// </summary>
        [TestMethod]
        public void ShiftLeftChildrenNoChange()
        {
            for (int i = 1; i < 4; i++)
            {
				DateShiftDTO obj = CreateParentShiftable();
                DateShiftDetailModelView detail = CreateShift(ChildModificationType.NoChange, -1 * i);


                DateShiftCalculation.PerformShifts(obj, new DateShiftModelView { Details = new DateShiftDetailModelView[] { detail } , Workspace = GetWorkspace() }, null, null, null, Level.Workspace, string.Empty);

				DateShiftDTO obj2 = CreateParentShiftable();

                Assert.AreEqual(obj2.StartDate.Value.AddMonths(detail.MonthChange), obj.StartDate.Value);
                Assert.AreEqual(obj2.EndDate.Value.AddMonths(detail.MonthChange), obj.EndDate.Value);

                CompareChildren(obj2, obj, detail);
            }
        }

        /// <summary>
        /// Test for DateShift
        /// </summary>
        [TestMethod]
        public void ExpandRightChildrenNoChange()
        {
			DateShiftDTO obj = CreateParentShiftable();
            DateShiftDetailModelView detail = CreateExpandRight(ChildModificationType.NoChange);

            DateShiftCalculation.PerformShifts(obj, new DateShiftModelView { Details = new DateShiftDetailModelView[] { detail } , Workspace = GetWorkspace() }, null, null, null, Level.Workspace, string.Empty);

			DateShiftDTO obj2 = CreateParentShiftable();

            Assert.AreEqual(obj2.StartDate.Value, obj.StartDate.Value);
            Assert.AreEqual(obj2.EndDate.Value.AddMonths(detail.MonthChange), obj.EndDate.Value);

            CompareChildren(obj2, obj, detail);
        }

        /// <summary>
        /// Test for DateShift
        /// </summary>
        [TestMethod]
        public void ShrinkLeftChildrenNoChange()
        {
			DateShiftDTO obj = CreateParentShiftable();
            DateShiftDetailModelView detail = CreateShrinkLeft(ChildModificationType.NoChange);

            DateShiftCalculation.PerformShifts(obj, new DateShiftModelView { Details = new DateShiftDetailModelView[] { detail } , Workspace = GetWorkspace() }, null, null, null, Level.Workspace, string.Empty);

			DateShiftDTO obj2 = CreateParentShiftable();

            Assert.AreEqual(obj2.StartDate.Value, obj.StartDate.Value);
            Assert.AreEqual(obj2.EndDate.Value.AddMonths(detail.MonthChange), obj.EndDate.Value);

            CompareChildren(obj2, obj, detail);
        }

        #endregion Children NoChange

        #region Children ToPoP

        /// <summary>
        /// Test for DateShift
        /// </summary>
        [TestMethod]
        public void ShiftRightChildrenToPoP()
        {
            for (int i = 1; i < 13; i++)
            {
				DateShiftDTO obj = CreateParentShiftable();
                DateShiftDetailModelView detail = CreateShift(ChildModificationType.ToPoP, i);

                DateShiftCalculation.PerformShifts(obj, new DateShiftModelView { Details = new DateShiftDetailModelView[] { detail } , Workspace = GetWorkspace() }, null, null, null, Level.Workspace, string.Empty);

				DateShiftDTO obj2 = CreateParentShiftable();
                obj2.StartDate = obj2.StartDate.Value.AddMonths(detail.MonthChange);
                obj2.EndDate = obj2.EndDate.Value.AddMonths(detail.MonthChange);

                Assert.AreEqual(obj2.StartDate.Value, obj.StartDate.Value);
                Assert.AreEqual(obj2.EndDate.Value, obj.EndDate.Value);

                CompareChildren(obj2, obj, detail);
            }
        }

        /// <summary>
        /// Test for DateShift
        /// </summary>
        [TestMethod]
        public void ShiftLeftChildrenToPoP()
        {
            for (int i = 1; i < 4; i++)
            {
				DateShiftDTO obj = CreateParentShiftable();
                DateShiftDetailModelView detail = CreateShift(ChildModificationType.ToPoP, -1 * i);

                DateShiftCalculation.PerformShifts(obj, new DateShiftModelView { Details = new DateShiftDetailModelView[] { detail } , Workspace = GetWorkspace() }, null, null, null, Level.Workspace, string.Empty);

				DateShiftDTO obj2 = CreateParentShiftable();

                Assert.AreEqual(obj2.StartDate.Value.AddMonths(detail.MonthChange), obj.StartDate.Value);
                Assert.AreEqual(obj2.EndDate.Value.AddMonths(detail.MonthChange), obj.EndDate.Value);

                CompareChildren(obj2, obj, detail);
            }
        }

        /// <summary>
        /// Test for DateShift
        /// </summary>
        [TestMethod]
        public void ExpandRightChildrenToPoP()
        {
			DateShiftDTO obj = CreateParentShiftable();
            DateShiftDetailModelView detail = CreateExpandRight(ChildModificationType.ToPoP);

            DateShiftCalculation.PerformShifts(obj, new DateShiftModelView { Details = new DateShiftDetailModelView[] { detail } , Workspace = GetWorkspace() }, null, null, null, Level.Workspace, string.Empty);

			DateShiftDTO obj2 = CreateParentShiftable();

            Assert.AreEqual(obj2.StartDate.Value, obj.StartDate.Value);
            Assert.AreEqual(obj2.EndDate.Value.AddMonths(detail.MonthChange), obj.EndDate.Value);

            CompareChildren(obj2, obj, detail);
        }

        /// <summary>
        /// Test for DateShift
        /// </summary>
        [TestMethod]
        public void ShrinkLeftChildrenToPoP()
        {
			DateShiftDTO obj = CreateParentShiftable();
            DateShiftDetailModelView detail = CreateShrinkLeft(ChildModificationType.ToPoP);

            DateShiftCalculation.PerformShifts(obj, new DateShiftModelView { Details = new DateShiftDetailModelView[] { detail } , Workspace = GetWorkspace() }, null, null, null, Level.Workspace, string.Empty);

			DateShiftDTO obj2 = CreateParentShiftable();

            Assert.AreEqual(obj2.StartDate.Value, obj.StartDate.Value);
            Assert.AreEqual(obj2.EndDate.Value.AddMonths(detail.MonthChange), obj.EndDate.Value);

            CompareChildren(obj2, obj, detail);
        }

        #endregion Children ToPoP

        #region Children ToStart

        /// <summary>
        /// Test for DateShift
        /// </summary>
        [TestMethod]
        public void ShiftRightChildrenToStart()
        {
            for (int i = 1; i < 13; i++)
            {
				DateShiftDTO obj = CreateParentShiftable();
                DateShiftDetailModelView detail = CreateShift(ChildModificationType.ToStart, i);

                DateShiftCalculation.PerformShifts(obj, new DateShiftModelView { Details = new DateShiftDetailModelView[] { detail } , Workspace = GetWorkspace() }, null, null, null, Level.Workspace, string.Empty);

				DateShiftDTO obj2 = CreateParentShiftable();
                obj2.StartDate = obj2.StartDate.Value.AddMonths(detail.MonthChange);
                obj2.EndDate = obj2.EndDate.Value.AddMonths(detail.MonthChange);

                Assert.AreEqual(obj2.StartDate.Value, obj.StartDate.Value);
                Assert.AreEqual(obj2.EndDate.Value, obj.EndDate.Value);

                CompareChildren(obj2, obj, detail);
            }
        }

        /// <summary>
        /// Test for DateShift
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void ShiftLeftChildrenToStart()
        {
            for (int i = 1; i < 4; i++)
            {
				DateShiftDTO obj = CreateParentShiftable();
                DateShiftDetailModelView detail = CreateShift(ChildModificationType.ToStart, -1 * i);

                DateShiftCalculation.PerformShifts(obj, new DateShiftModelView { Details = new DateShiftDetailModelView[] { detail } , Workspace = GetWorkspace() }, null, null, null, Level.Workspace, string.Empty);

				DateShiftDTO obj2 = CreateParentShiftable();

                Assert.AreEqual(obj2.StartDate.Value.AddMonths(detail.MonthChange), obj.StartDate.Value);
                Assert.AreEqual(obj2.EndDate.Value.AddMonths(detail.MonthChange), obj.EndDate.Value);

                CompareChildren(obj2, obj, detail);
            }
        }

        /// <summary>
        /// Test for DateShift
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void ExpandRightChildrenToStart()
        {
            DateShiftDTO obj = CreateParentShiftable();
            DateShiftDetailModelView detail = CreateExpandRight(ChildModificationType.ToStart);

            DateShiftCalculation.PerformShifts(obj, new DateShiftModelView { Details = new DateShiftDetailModelView[] { detail } , Workspace = GetWorkspace() }, null, null, null, Level.Workspace, string.Empty);

            DateShiftDTO obj2 = CreateParentShiftable();

            Assert.AreEqual(obj2.StartDate.Value, obj.StartDate.Value);
            Assert.AreEqual(obj2.EndDate.Value.AddMonths(detail.MonthChange), obj.EndDate.Value);

            CompareChildren(obj2, obj, detail);
        }

        /// <summary>
        /// Test for DateShift
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void ShrinkLeftChildrenToStart()
        {
            DateShiftDTO obj = CreateParentShiftable();
            DateShiftDetailModelView detail = CreateShrinkLeft(ChildModificationType.ToStart);

            DateShiftCalculation.PerformShifts(obj, new DateShiftModelView { Details = new DateShiftDetailModelView[] { detail } , Workspace = GetWorkspace() }, null, null, null, Level.Workspace, string.Empty);

            DateShiftDTO obj2 = CreateParentShiftable();

            Assert.AreEqual(obj2.StartDate.Value, obj.StartDate.Value);
            Assert.AreEqual(obj2.EndDate.Value.AddMonths(detail.MonthChange), obj.EndDate.Value);

            CompareChildren(obj2, obj, detail);
        }

        #endregion Children ToStart

        #region Children ToEnd

        /// <summary>
        /// Test for DateShift
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void ShiftRightChildrenToEnd()
        {
            for (int i = 1; i < 13; i++)
            {
                DateShiftDTO obj = CreateParentShiftable();
                DateShiftDetailModelView detail = CreateShift(ChildModificationType.ToEnd, i);

                DateShiftCalculation.PerformShifts(obj, new DateShiftModelView { Details = new DateShiftDetailModelView[] { detail } , Workspace = GetWorkspace() }, null, null, null, Level.Workspace, string.Empty);

                DateShiftDTO obj2 = CreateParentShiftable();

                Assert.AreEqual(obj2.StartDate.Value.AddMonths(detail.MonthChange), obj.StartDate.Value);
                Assert.AreEqual(obj2.EndDate.Value.AddMonths(detail.MonthChange), obj.EndDate.Value);

                CompareChildren(obj2, obj, detail);
            }
        }

        /// <summary>
        /// Test for DateShift
        /// </summary>
        [TestMethod]
        public void ShiftLeftChildrenToEnd()
        {
            for (int i = 1; i < 4; i++)
            {
                DateShiftDTO obj = CreateParentShiftable();
                DateShiftDetailModelView detail = CreateShift(ChildModificationType.ToEnd, -1 * i);

                DateShiftCalculation.PerformShifts(obj, new DateShiftModelView { Details = new DateShiftDetailModelView[] { detail } , Workspace = GetWorkspace() }, null, null, null, Level.Workspace, string.Empty);

                DateShiftDTO obj2 = CreateParentShiftable();

                Assert.AreEqual(obj2.StartDate.Value.AddMonths(detail.MonthChange), obj.StartDate.Value);
                Assert.AreEqual(obj2.EndDate.Value.AddMonths(detail.MonthChange), obj.EndDate.Value);

                CompareChildren(obj2, obj, detail);
            }
        }

        /// <summary>
        /// Test for DateShift
        /// </summary>
        [TestMethod]
        public void ExpandRightChildrenToEnd()
        {
            DateShiftDTO obj = CreateParentShiftable();
            DateShiftDetailModelView detail = CreateExpandRight(ChildModificationType.ToEnd);

            DateShiftCalculation.PerformShifts(obj, new DateShiftModelView { Details = new DateShiftDetailModelView[] { detail } , Workspace = GetWorkspace() }, null, null, null, Level.Workspace, string.Empty);

            DateShiftDTO obj2 = CreateParentShiftable();

            Assert.AreEqual(obj2.StartDate.Value, obj.StartDate.Value);
            Assert.AreEqual(obj2.EndDate.Value.AddMonths(detail.MonthChange), obj.EndDate.Value);

            CompareChildren(obj2, obj, detail);
        }

        /// <summary>
        /// Test for DateShift
        /// </summary>
        [TestMethod]
        public void ShrinkLeftChildrenToEnd()
        {
            DateShiftDTO obj = CreateParentShiftable();
            DateShiftDetailModelView detail = CreateShrinkLeft(ChildModificationType.ToEnd);

            DateShiftCalculation.PerformShifts(obj, new DateShiftModelView { Details = new DateShiftDetailModelView[] { detail } , Workspace = GetWorkspace() }, null, null, null, Level.Workspace, string.Empty);

            DateShiftDTO obj2 = CreateParentShiftable();

            Assert.AreEqual(obj2.StartDate.Value, obj.StartDate.Value);
            Assert.AreEqual(obj2.EndDate.Value.AddMonths(detail.MonthChange), obj.EndDate.Value);

            CompareChildren(obj2, obj, detail);
        }

        #endregion Children ToEnd

        #region Spread Tests

        /// <summary>
        /// Call Task Spread Shift with something that is not a Task
        /// </summary>
        [TestMethod, ExpectedException(typeof(NotSupportedException))]
        public void InvalidTaskSpreadsTest()
        {
            DateShiftDTO ds = new DateShiftDTO();
            DateShiftCalculation.PerformLaborSpreadShift(ds, CreateShift(ChildModificationType.FlowDown, 1), new DateShiftModelView());
        }

        /// <summary>
        /// The Task Spreads Test.
        /// </summary>
        [TestMethod]
        public void TaskSpreadsTest()
        {
			DateShiftDTO task = new DateShiftDTO
			{
                StartDate = StartDate,
                EndDate = EndDate,
                TaskElementLabors = new Collection<ResourceTypeDto>
                {
                    new ResourceTypeDto
                    {
                        StartDate = StartDate,
                        EndDate = EndDate,
                        ValueSpread = 100,
                        SpreadCurveID = SpreadCurves.SpreadCurve3,
                        LaborSpreads = SpreadCurve.CalculateLaborSpreadsBasedOnCurve(new LaborSpreadRequest(SpreadCurves.SpreadCurve3, StartDate, EndDate, 100), 2)
                    }
                }
            };

            DateShiftDetailModelView detail = CreateExpandRight(ChildModificationType.FlowDown);
            detail.ChildModificationType = ChildModificationType.FlowDown;
            detail.SpreadHandling = SpreadHandling.NotSet;
            int difference = StartDate.MonthDifference(EndDate) + 1;

            DateShiftCalculation.PerformShifts(task, new DateShiftModelView { Details = new DateShiftDetailModelView[] { detail } , Workspace = GetWorkspace() }, null, null, null, Level.Workspace, string.Empty);

            // the spreads that are auto-calculated are not saved to DB, so the # and values should not change from the beginning
            Assert.AreEqual(difference, task.TaskElementLabors.First().LaborSpreads.Count);
            Assert.AreEqual(100, task.TaskElementLabors.First().ValueSpread);
            Assert.AreEqual(100, task.TaskElementLabors.First().LaborSpreads.Sum(s => s.LaborSpreadValue));
        }

        /// <summary>
        /// Testing Discrete Spreads with no changes.
        /// </summary>
        [TestMethod]
        public void TaskDiscreteNoChangeSpreadsTest()
        {
			DateShiftDTO DateShiftDTO = CreateDiscreteParentShiftable();
            DateShiftDetailModelView detail = CreateExpandRight(ChildModificationType.FlowDown);
            detail.SpreadHandling = SpreadHandling.DiscreteToError;
            int difference = StartDate.MonthDifference(EndDate) + 1;

            DateShiftCalculation.PerformShifts(DateShiftDTO, new DateShiftModelView { Details = new DateShiftDetailModelView[] { detail }, Workspace = GetWorkspace() }, null, null, null, Level.Workspace, string.Empty);

            // expanded right, so no change to discrete to original discrete, just adding zeros to end
            Assert.AreEqual(difference, DateShiftDTO.TaskElementLabors.First().LaborSpreads.Count);
            Assert.AreEqual(StartDate, DateShiftDTO.TaskElementLabors.First().LaborSpreads.Min(l => l.LaborSpreadDate));
            Assert.AreEqual(EndDate.AddMonths(detail.MonthChange), DateShiftDTO.TaskElementLabors.First().LaborSpreads.Max(l => l.LaborSpreadDate));
            Assert.AreEqual(0m, DateShiftDTO.TaskElementLabors.First().LaborSpreads.Last().LaborSpreadValue);
        }

        /// <summary>
        /// Testing Discrete ToError
        /// </summary>
        [TestMethod]
        public void TaskDiscreteToErrorSpreadsTest()
        {
            DateShiftDTO DateShiftDTO = CreateDiscreteParentShiftable();
            DateShiftDetailModelView detail = CreateShrinkLeft(ChildModificationType.FlowDown);
            detail.SpreadHandling = SpreadHandling.DiscreteToError;
            int difference = StartDate.MonthDifference(EndDate) + 1;

            DateShiftCalculation.PerformShifts(DateShiftDTO, new DateShiftModelView { Details = new DateShiftDetailModelView[] { detail }, Workspace = GetWorkspace() }, null, null, null, Level.Workspace, string.Empty);

            Assert.AreEqual(difference + detail.MonthChange, DateShiftDTO.TaskElementLabors.First().LaborSpreads.Count);
            Assert.IsTrue(DISCRETE_HOURS > DateShiftDTO.TaskElementLabors.First().ValueSpread);
            Assert.IsTrue(DISCRETE_HOURS > DateShiftDTO.TaskElementLabors.First().LaborSpreads.Sum(s => s.LaborSpreadValue));
        }

        /// <summary>
        /// Testing Discrete ToFirst
        /// </summary>
        [TestMethod]
        public void TaskDiscreteToFirstSpreadsTest()
        {
            DateShiftDTO DateShiftDTO = CreateDiscreteParentShiftable();
            DateShiftDetailModelView detail = CreateShrinkLeft(ChildModificationType.FlowDown);
            detail.SpreadHandling = SpreadHandling.DiscreteToFirst;
            int difference = StartDate.MonthDifference(EndDate) + 1;

            decimal value = DateShiftDTO.TaskElementLabors.First().LaborSpreads.First().LaborSpreadValue;

            DateShiftCalculation.PerformShifts(DateShiftDTO, new DateShiftModelView { Details = new DateShiftDetailModelView[] { detail }, Workspace = GetWorkspace() }, null, null, null, Level.Workspace, string.Empty);

            Assert.AreEqual(difference + detail.MonthChange, DateShiftDTO.TaskElementLabors.First().LaborSpreads.Count);
            Assert.AreEqual(DISCRETE_HOURS, DateShiftDTO.TaskElementLabors.First().ValueSpread);
            Assert.AreEqual(DISCRETE_HOURS, DateShiftDTO.TaskElementLabors.First().LaborSpreads.Sum(s => s.LaborSpreadValue));
            Assert.AreNotEqual(value, DateShiftDTO.TaskElementLabors.First().LaborSpreads.First().LaborSpreadValue);
        }

        /// <summary>
        /// Testing Discrete ToLast
        /// </summary>
        [TestMethod]
        public void TaskDiscreteToLastSpreadsTest()
        {
            DateShiftDTO DateShiftDTO = CreateDiscreteParentShiftable();
            DateShiftDetailModelView detail = CreateShrinkLeft(ChildModificationType.FlowDown);
            detail.SpreadHandling = SpreadHandling.DiscreteToLast;
            int difference = StartDate.MonthDifference(EndDate) + 1;
            decimal value = DateShiftDTO.TaskElementLabors.First().LaborSpreads.Last().LaborSpreadValue;

            DateShiftCalculation.PerformShifts(DateShiftDTO, new DateShiftModelView { Details = new DateShiftDetailModelView[] { detail }, Workspace = GetWorkspace() }, null, null, null, Level.Workspace, string.Empty);

            Assert.AreEqual(difference + detail.MonthChange, DateShiftDTO.TaskElementLabors.First().LaborSpreads.Count);
            Assert.AreEqual(DISCRETE_HOURS, DateShiftDTO.TaskElementLabors.First().ValueSpread);
            Assert.AreEqual(DISCRETE_HOURS, DateShiftDTO.TaskElementLabors.First().LaborSpreads.Sum(s => s.LaborSpreadValue));
            Assert.AreNotEqual(value, DateShiftDTO.TaskElementLabors.First().LaborSpreads.Last().LaborSpreadValue);
        }

        /// <summary>
        /// Invalid Discrete DateShift test
        /// </summary>
        [TestMethod, ExpectedException(typeof(NotSupportedException))]
        public void TaskDiscreteSpreadInvalidSpreadHandlingTest1()
        {
            DateShiftDTO DateShiftDTO = CreateDiscreteParentShiftable();
            DateShiftDetailModelView detail = CreateExpandRight(ChildModificationType.FlowDown);
            detail.ChildModificationType = ChildModificationType.FlowDown;
            detail.SpreadHandling = SpreadHandling.NotSet;

            DateShiftCalculation.PerformShifts(DateShiftDTO, new DateShiftModelView { Details = new DateShiftDetailModelView[] { detail }, Workspace = GetWorkspace() }, null, null, null, Level.Workspace, string.Empty);
        }

        /// <summary>
        /// Invalid Discrete DateShift test
        /// </summary>
        [TestMethod, ExpectedException(typeof(NotSupportedException))]
        public void TaskDiscreteSpreadInvalidSpreadHandlingTest2()
        {
            DateShiftDTO DateShiftDTO = CreateDiscreteParentShiftable();
            DateShiftDetailModelView detail = CreateExpandRight(ChildModificationType.FlowDown);
            detail.ChildModificationType = ChildModificationType.FlowDown;
            detail.SpreadHandling = null;

            DateShiftCalculation.PerformShifts(DateShiftDTO, new DateShiftModelView { Details = new DateShiftDetailModelView[] { detail }, Workspace = GetWorkspace() }, null, null, null, Level.Workspace, string.Empty);
        }

        /// <summary>
        /// Invalid Discrete DateShift test
        /// </summary>
        [TestMethod, ExpectedException(typeof(NotSupportedException))]
        public void TaskDiscreteSpreadInvalidSpreadHandlingTest3()
        {
			DateShiftDTO DateShiftDTO = CreateDiscreteParentShiftable();
            DateShiftDetailModelView detail = CreateExpandRight(ChildModificationType.FlowDown);
            detail.ChildModificationType = ChildModificationType.FlowDown;
            detail.SpreadHandling = SpreadHandling.NewCurve;
            detail.NewCurve = null;

            DateShiftCalculation.PerformShifts(DateShiftDTO, new DateShiftModelView { Details = new DateShiftDetailModelView[] { detail }, Workspace = GetWorkspace() }, null, null, null, Level.Workspace, string.Empty);
        }

        /// <summary>
        /// Invalid Discrete DateShift test
        /// </summary>
        [TestMethod, ExpectedException(typeof(NotSupportedException))]
        public void TaskDiscreteSpreadInvalidSpreadHandlingTest4()
        {
            DateShiftDTO DateShiftDTO = CreateDiscreteParentShiftable();
            DateShiftDetailModelView detail = CreateExpandRight(ChildModificationType.FlowDown);
            detail.ChildModificationType = ChildModificationType.FlowDown;
            detail.SpreadHandling = SpreadHandling.NewCurve;
            detail.NewCurve = SpreadCurves.None;

            DateShiftCalculation.PerformShifts(DateShiftDTO, new DateShiftModelView { Details = new DateShiftDetailModelView[] { detail }, Workspace = GetWorkspace() }, null, null, null, Level.Workspace, string.Empty);
        }

        /// <summary>
        /// Invalid Discrete DateShift test
        /// </summary>
        [TestMethod, ExpectedException(typeof(NotSupportedException))]
        public void TaskDiscreteSpreadInvalidSpreadHandlingTest5()
        {
            DateShiftDTO DateShiftDTO = CreateDiscreteParentShiftable();
            DateShiftDetailModelView detail = CreateExpandRight(ChildModificationType.FlowDown);
            detail.ChildModificationType = ChildModificationType.FlowDown;
            detail.SpreadHandling = SpreadHandling.NewCurve;
            detail.NewCurve = SpreadCurves.DiscreteCost;

            DateShiftCalculation.PerformShifts(DateShiftDTO, new DateShiftModelView { Details = new DateShiftDetailModelView[] { detail }, Workspace = GetWorkspace() }, null, null, null, Level.Workspace, string.Empty);
        }

        /// <summary>
        /// Invalid Discrete DateShift test
        /// </summary>
        [TestMethod, ExpectedException(typeof(NotSupportedException))]
        public void TaskDiscreteSpreadInvalidSpreadHandlingTest6()
        {
            DateShiftDTO DateShiftDTO = CreateDiscreteParentShiftable();
            DateShiftDetailModelView detail = CreateExpandRight(ChildModificationType.FlowDown);
            detail.ChildModificationType = ChildModificationType.FlowDown;
            detail.SpreadHandling = SpreadHandling.NewCurve;
            detail.NewCurve = SpreadCurves.DiscreteHours;

            DateShiftCalculation.PerformShifts(DateShiftDTO, new DateShiftModelView { Details = new DateShiftDetailModelView[] { detail }, Workspace = GetWorkspace() }, null, null, null, Level.Workspace, string.Empty);
        }

        /// <summary>
        /// Invalid Discrete DateShift test
        /// </summary>
        [TestMethod, ExpectedException(typeof(NotSupportedException))]
        public void TaskDiscreteSpreadInvalidSpreadHandlingTest7()
        {
            DateShiftDTO DateShiftDTO = CreateDiscreteParentShiftable();
            DateShiftDetailModelView detail = CreateExpandRight(ChildModificationType.FlowDown);
            detail.ChildModificationType = ChildModificationType.FlowDown;
            detail.SpreadHandling = SpreadHandling.NewCurve;
            detail.NewCurve = SpreadCurves.Level;

            DateShiftCalculation.PerformShifts(DateShiftDTO, new DateShiftModelView { Details = new DateShiftDetailModelView[] { detail }, Workspace = GetWorkspace() }, null, null, null, Level.Workspace, string.Empty);
        }

        /// <summary>
        /// Invalid Discrete DateShift test
        /// </summary>
        [TestMethod, ExpectedException(typeof(NotSupportedException))]
        public void TaskDiscreteSpreadInvalidSpreadHandlingTest8()
        {
            DateShiftDTO DateShiftDTO = CreateDiscreteParentShiftable();
            DateShiftDetailModelView detail = CreateExpandRight(ChildModificationType.FlowDown);
            detail.ChildModificationType = ChildModificationType.FlowDown;
            detail.SpreadHandling = SpreadHandling.NewCurve;
            detail.NewCurve = SpreadCurves.Load;

            DateShiftCalculation.PerformShifts(DateShiftDTO, new DateShiftModelView { Details = new DateShiftDetailModelView[] { detail }, Workspace = GetWorkspace() }, null, null, null, Level.Workspace, string.Empty);
        }

        #endregion Spread Tests

        #region DateShiftDTO tests

        /// <summary>
        /// Dateshift recursive test
        /// </summary>
        [TestMethod]
        public void DateShiftRecursiveTest1()
        {
            DateShiftDTO ds = CreateDateShiftDTO();

            DateShiftDetailModelView detail = CreateShrinkLeft(ChildModificationType.FlowDown);
            
            DateShiftCalculation.PerformShifts(ds, new DateShiftModelView { Details = new DateShiftDetailModelView[] { detail }, Workspace = GetWorkspace() }, null, null, null, Level.Workspace, string.Empty);

            Assert.IsTrue(detail.Errors.HasError1);
            Assert.IsFalse(detail.Errors.HasError2);
            Assert.IsTrue(detail.Errors.AffectedBoeIds.Any());
            Assert.AreEqual(StartDate, ds.StartDate);
            Assert.AreEqual(StartDate.AddMonths(1), ds.Children.ElementAt(0).StartDate);
            Assert.AreEqual(StartDate, ds.Children.ElementAt(1).StartDate);
            Assert.AreEqual(StartDate, ds.Children.ElementAt(1).Children.First().StartDate);
            Assert.AreEqual(EndDate, ds.Children.ElementAt(2).StartDate);
            Assert.AreEqual(StartDate, ds.Children.ElementAt(3).StartDate);
            Assert.AreEqual(StartDate.AddMonths(2), ds.Children.ElementAt(4).StartDate);
            Assert.AreEqual(StartDate, ds.Children.ElementAt(5).StartDate);

            DateTime newEndDate = EndDate.AddMonths(detail.MonthChange);
            Assert.AreEqual(newEndDate, ds.EndDate);
            Assert.AreEqual(newEndDate.AddMonths(-1), ds.Children.ElementAt(0).EndDate);
            Assert.AreEqual(newEndDate, ds.Children.ElementAt(1).EndDate);
            Assert.AreEqual(newEndDate, ds.Children.ElementAt(1).Children.First().EndDate);
            Assert.AreEqual(newEndDate, ds.Children.ElementAt(2).EndDate);
            Assert.AreEqual(StartDate.AddMonths(detail.MonthChange), ds.Children.ElementAt(3).EndDate);
            Assert.AreEqual(newEndDate, ds.Children.ElementAt(4).EndDate);
            Assert.AreEqual(newEndDate.AddMonths(-2), ds.Children.ElementAt(5).EndDate);

        }

        /// <summary>
        /// Dateshift recursive test
        /// </summary>
        [TestMethod]
        public void DateShiftRecursiveTest2()
        {
            DateShiftDTO ds = CreateDateShiftDTO();

            DateShiftDetailModelView detail = CreateShrinkLeft(ChildModificationType.FlowDown);
            detail.Error1FixSingleMonth = true;
            detail.Error2Handling = null;

            DateShiftCalculation.PerformShifts(ds, new DateShiftModelView { Details = new DateShiftDetailModelView[] { detail }, Workspace = GetWorkspace() }, null, null, null, Level.Workspace, string.Empty);

            Assert.IsFalse(detail.Errors.HasError1);
            Assert.IsTrue(detail.Errors.HasError2);
            Assert.IsTrue(detail.Errors.AffectedBoeIds.Any());
            Assert.AreEqual(StartDate, ds.StartDate);
            Assert.AreEqual(StartDate.AddMonths(1), ds.Children.ElementAt(0).StartDate);
            Assert.AreEqual(StartDate, ds.Children.ElementAt(1).StartDate);
            Assert.AreEqual(StartDate, ds.Children.ElementAt(1).Children.First().StartDate);
            Assert.AreEqual(EndDate, ds.Children.ElementAt(2).StartDate);
            Assert.AreEqual(StartDate, ds.Children.ElementAt(3).StartDate);
            Assert.AreEqual(StartDate.AddMonths(2), ds.Children.ElementAt(4).StartDate);
            Assert.AreEqual(StartDate, ds.Children.ElementAt(5).StartDate);

            DateTime newEndDate = EndDate.AddMonths(detail.MonthChange);
            Assert.AreEqual(newEndDate, ds.EndDate);
            Assert.AreEqual(newEndDate.AddMonths(-1), ds.Children.ElementAt(0).EndDate);
            Assert.AreEqual(newEndDate, ds.Children.ElementAt(1).EndDate);
            Assert.AreEqual(newEndDate, ds.Children.ElementAt(1).Children.First().EndDate);
            Assert.AreEqual(EndDate, ds.Children.ElementAt(2).EndDate); // outside POP
            Assert.AreEqual(StartDate, ds.Children.ElementAt(3).EndDate); // fixed by error1 
            Assert.AreEqual(newEndDate, ds.Children.ElementAt(4).EndDate);
            Assert.AreEqual(newEndDate.AddMonths(-2), ds.Children.ElementAt(5).EndDate);

        }

        /// <summary>
        /// Dateshift recursive test
        /// </summary>
        [TestMethod]
        public void DateShiftRecursiveTest3()
        {
            DateShiftDTO ds = CreateDateShiftDTO();
            DateShiftDTO ds2 = CreateDateShiftDTO();
            DateShiftDetailModelView detail = CreateShrinkLeft(ChildModificationType.NoChange);
            detail.Error1FixSingleMonth = false;
            detail.Error2Handling = null;

            DateShiftCalculation.PerformShifts(ds, new DateShiftModelView { Details = new DateShiftDetailModelView[] { detail }, Workspace = GetWorkspace() }, null, null, null, Level.Workspace, string.Empty);

            Assert.IsFalse(detail.Errors.HasError1);
            Assert.IsTrue(detail.Errors.HasError2);
            Assert.IsTrue(detail.Errors.AffectedBoeIds.Any());
            Assert.AreEqual(StartDate, ds.StartDate);
            Assert.AreEqual(ds2.Children.ElementAt(0).StartDate, ds.Children.ElementAt(0).StartDate);
            Assert.AreEqual(ds2.Children.ElementAt(1).StartDate, ds.Children.ElementAt(1).StartDate);
            Assert.AreEqual(ds2.Children.ElementAt(1).Children.First().StartDate, ds.Children.ElementAt(1).Children.First().StartDate);
            Assert.AreEqual(ds2.Children.ElementAt(2).StartDate, ds.Children.ElementAt(2).StartDate);
            Assert.AreEqual(ds2.Children.ElementAt(3).StartDate, ds.Children.ElementAt(3).StartDate);
            Assert.AreEqual(ds2.Children.ElementAt(4).StartDate, ds.Children.ElementAt(4).StartDate);
            Assert.AreEqual(ds2.Children.ElementAt(5).StartDate, ds.Children.ElementAt(5).StartDate);

            DateTime newEndDate = EndDate.AddMonths(detail.MonthChange);
            Assert.AreEqual(newEndDate, ds.EndDate);
            Assert.AreEqual(ds2.Children.ElementAt(0).EndDate, ds.Children.ElementAt(0).EndDate);
            Assert.AreEqual(ds2.Children.ElementAt(1).EndDate, ds.Children.ElementAt(1).EndDate);
            Assert.AreEqual(ds2.Children.ElementAt(1).Children.First().EndDate, ds.Children.ElementAt(1).Children.First().EndDate);
            Assert.AreEqual(ds2.Children.ElementAt(2).EndDate, ds.Children.ElementAt(2).EndDate); // outside POP
            Assert.AreEqual(ds2.Children.ElementAt(3).EndDate, ds.Children.ElementAt(3).EndDate); // fixed by error1 
            Assert.AreEqual(ds2.Children.ElementAt(4).EndDate, ds.Children.ElementAt(4).EndDate);
            Assert.AreEqual(ds2.Children.ElementAt(5).EndDate, ds.Children.ElementAt(5).EndDate);

        }

        /// <summary>
        /// Dateshift recursive test
        /// </summary>
        [TestMethod]
        public void DateShiftRecursiveTest4()
        {
            DateShiftDTO ds = CreateDateShiftDTO();
            DateShiftDTO ds2 = CreateDateShiftDTO();
            DateShiftDetailModelView detail = CreateShrinkLeft(ChildModificationType.NoChange);
            detail.Error1FixSingleMonth = false;
            detail.Error2Handling = ChildModificationType.NoChange;

            DateShiftCalculation.PerformShifts(ds, new DateShiftModelView { Details = new DateShiftDetailModelView[] { detail }, Workspace = GetWorkspace() }, null, null, null, Level.Workspace, string.Empty);

            Assert.IsFalse(detail.Errors.HasError1);
            Assert.IsFalse(detail.Errors.HasError2);
            Assert.IsTrue(detail.Errors.AffectedBoeIds.Any());
            Assert.AreEqual(StartDate, ds.StartDate);
            Assert.AreEqual(ds2.Children.ElementAt(0).StartDate, ds.Children.ElementAt(0).StartDate);
            Assert.AreEqual(ds2.Children.ElementAt(1).StartDate, ds.Children.ElementAt(1).StartDate);
            Assert.AreEqual(ds2.Children.ElementAt(1).Children.First().StartDate, ds.Children.ElementAt(1).Children.First().StartDate);
            Assert.AreEqual(ds2.Children.ElementAt(2).StartDate, ds.Children.ElementAt(2).StartDate);
            Assert.AreEqual(ds2.Children.ElementAt(3).StartDate, ds.Children.ElementAt(3).StartDate);
            Assert.AreEqual(ds2.Children.ElementAt(4).StartDate, ds.Children.ElementAt(4).StartDate);
            Assert.AreEqual(ds2.Children.ElementAt(5).StartDate, ds.Children.ElementAt(5).StartDate);

            DateTime newEndDate = EndDate.AddMonths(detail.MonthChange);
            Assert.AreEqual(newEndDate, ds.EndDate);
            Assert.AreEqual(ds2.Children.ElementAt(0).EndDate, ds.Children.ElementAt(0).EndDate);
            Assert.AreEqual(ds2.Children.ElementAt(1).EndDate, ds.Children.ElementAt(1).EndDate);
            Assert.AreEqual(ds2.Children.ElementAt(1).Children.First().EndDate, ds.Children.ElementAt(1).Children.First().EndDate);
            Assert.AreEqual(ds2.Children.ElementAt(2).EndDate, ds.Children.ElementAt(2).EndDate); // outside POP
            Assert.AreEqual(ds2.Children.ElementAt(3).EndDate, ds.Children.ElementAt(3).EndDate); // fixed by error1 
            Assert.AreEqual(ds2.Children.ElementAt(4).EndDate, ds.Children.ElementAt(4).EndDate);
            Assert.AreEqual(ds2.Children.ElementAt(5).EndDate, ds.Children.ElementAt(5).EndDate);

        }

        /// <summary>
        /// Dateshift recursive test
        /// </summary>
        [TestMethod]
        public void DateShiftRecursiveTest5()
        {
            DateShiftDTO ds = CreateDateShiftDTO();
            DateShiftDTO ds2 = CreateDateShiftDTO();
            DateShiftDetailModelView detail = CreateShrinkLeft(ChildModificationType.NoChange);
            detail.Error1FixSingleMonth = false;
            detail.Error2Handling = ChildModificationType.ToPoP;

            DateShiftCalculation.PerformShifts(ds, new DateShiftModelView { Details = new DateShiftDetailModelView[] { detail }, Workspace = GetWorkspace() }, null, null, null, Level.Workspace, string.Empty);

            Assert.IsFalse(detail.Errors.HasError1);
            Assert.IsFalse(detail.Errors.HasError2);
            Assert.IsFalse(detail.Errors.AffectedBoeIds.Any());
            Assert.AreEqual(StartDate, ds.StartDate);
            Assert.AreEqual(StartDate, ds.Children.ElementAt(0).StartDate); // error 2 to pop
            Assert.AreEqual(StartDate, ds.Children.ElementAt(1).StartDate); // error 2 to pop
            Assert.AreEqual(StartDate, ds.Children.ElementAt(1).Children.First().StartDate); // error 2 to pop
            Assert.AreEqual(StartDate, ds.Children.ElementAt(2).StartDate); // error 2 to pop
            Assert.AreEqual(ds2.Children.ElementAt(3).StartDate, ds.Children.ElementAt(3).StartDate); 
            Assert.AreEqual(StartDate, ds.Children.ElementAt(4).StartDate); // error 2 to pop
            Assert.AreEqual(StartDate, ds.Children.ElementAt(5).StartDate); // error 2 to pop

            DateTime newEndDate = EndDate.AddMonths(detail.MonthChange);
            Assert.AreEqual(newEndDate, ds.EndDate);
            Assert.AreEqual(newEndDate, ds.Children.ElementAt(0).EndDate); // error 2 to pop
            Assert.AreEqual(newEndDate, ds.Children.ElementAt(1).EndDate); // error 2 to pop
            Assert.AreEqual(newEndDate, ds.Children.ElementAt(1).Children.First().EndDate); // error 2 to pop
            Assert.AreEqual(newEndDate, ds.Children.ElementAt(2).EndDate); // error 2 to pop
            Assert.AreEqual(ds2.Children.ElementAt(3).EndDate, ds.Children.ElementAt(3).EndDate);  
            Assert.AreEqual(newEndDate, ds.Children.ElementAt(4).EndDate); // error 2 to pop
            Assert.AreEqual(newEndDate, ds.Children.ElementAt(5).EndDate); // error 2 to pop

        }

        /// <summary>
        /// Dateshift recursive test
        /// </summary>
        [TestMethod]
        public void DateShiftRecursiveTest6()
        {
            DateShiftDTO ds = CreateDateShiftDTO();
            DateShiftDTO ds2 = CreateDateShiftDTO();
            DateShiftDetailModelView detail = CreateShrinkLeft(ChildModificationType.ToPoP);
            detail.Error1FixSingleMonth = false;
            detail.Error2Handling = null;

            DateShiftCalculation.PerformShifts(ds, new DateShiftModelView { Details = new DateShiftDetailModelView[] { detail }, Workspace = GetWorkspace() }, null, null, null, Level.Workspace, string.Empty);

            Assert.IsFalse(detail.Errors.HasError1);
            Assert.IsFalse(detail.Errors.HasError2);
            Assert.IsFalse(detail.Errors.AffectedBoeIds.Any());
            Assert.AreEqual(StartDate, ds.StartDate);
            Assert.AreEqual(StartDate, ds.Children.ElementAt(0).StartDate); // error 2 to pop
            Assert.AreEqual(StartDate, ds.Children.ElementAt(1).StartDate); // error 2 to pop
            Assert.AreEqual(StartDate, ds.Children.ElementAt(1).Children.First().StartDate); // error 2 to pop
            Assert.AreEqual(StartDate, ds.Children.ElementAt(2).StartDate); // error 2 to pop
            Assert.AreEqual(StartDate, ds.Children.ElementAt(3).StartDate);
            Assert.AreEqual(StartDate, ds.Children.ElementAt(4).StartDate); // error 2 to pop
            Assert.AreEqual(StartDate, ds.Children.ElementAt(5).StartDate); // error 2 to pop

            DateTime newEndDate = EndDate.AddMonths(detail.MonthChange);
            Assert.AreEqual(newEndDate, ds.EndDate);
            Assert.AreEqual(newEndDate, ds.Children.ElementAt(0).EndDate); // error 2 to pop
            Assert.AreEqual(newEndDate, ds.Children.ElementAt(1).EndDate); // error 2 to pop
            Assert.AreEqual(newEndDate, ds.Children.ElementAt(1).Children.First().EndDate); // error 2 to pop
            Assert.AreEqual(newEndDate, ds.Children.ElementAt(2).EndDate); // error 2 to pop
            Assert.AreEqual(newEndDate, ds.Children.ElementAt(3).EndDate);
            Assert.AreEqual(newEndDate, ds.Children.ElementAt(4).EndDate); // error 2 to pop
            Assert.AreEqual(newEndDate, ds.Children.ElementAt(5).EndDate); // error 2 to pop

        }

        /// <summary>
        /// Dateshift recursive test
        /// </summary>
        [TestMethod]
        public void DateShiftRecursiveTest7()
        {
            DateShiftDTO ds = CreateDateShiftDTO();
            DateShiftDTO ds2 = CreateDateShiftDTO();
            DateShiftDetailModelView detail = CreateShrinkLeft(ChildModificationType.NoChange);
            detail.Error1FixSingleMonth = false;
            detail.Error2Handling = ChildModificationType.ToEnd;

            DateShiftCalculation.PerformShifts(ds, new DateShiftModelView { Details = new DateShiftDetailModelView[] { detail }, Workspace = GetWorkspace() }, null, null, null, Level.Workspace, string.Empty);

            Assert.IsFalse(detail.Errors.HasError1);
            Assert.IsFalse(detail.Errors.HasError2);
            Assert.IsFalse(detail.Errors.AffectedBoeIds.Any());
            Assert.AreEqual(StartDate, ds.StartDate);
            Assert.AreEqual(ds2.Children.ElementAt(0).StartDate, ds.Children.ElementAt(0).StartDate);
            Assert.AreEqual(ds2.Children.ElementAt(1).StartDate, ds.Children.ElementAt(1).StartDate);
            Assert.AreEqual(ds2.Children.ElementAt(1).Children.First().StartDate, ds.Children.ElementAt(1).Children.First().StartDate);
            Assert.AreEqual(ds2.Children.ElementAt(2).StartDate, ds.Children.ElementAt(2).StartDate);
            Assert.AreEqual(ds2.Children.ElementAt(3).StartDate, ds.Children.ElementAt(3).StartDate);
            Assert.AreEqual(ds2.Children.ElementAt(4).StartDate, ds.Children.ElementAt(4).StartDate);
            Assert.AreEqual(ds2.Children.ElementAt(5).StartDate, ds.Children.ElementAt(5).StartDate);


            DateTime newEndDate = EndDate.AddMonths(detail.MonthChange);
            Assert.AreEqual(newEndDate, ds.EndDate);
            Assert.AreEqual(newEndDate, ds.Children.ElementAt(0).EndDate); // error 2 to end
            Assert.AreEqual(newEndDate, ds.Children.ElementAt(1).EndDate); // error 2 to end
            Assert.AreEqual(newEndDate, ds.Children.ElementAt(1).Children.First().EndDate); // error 2 to end
            Assert.AreEqual(newEndDate, ds.Children.ElementAt(2).EndDate); // error 2 to end
            Assert.AreEqual(ds2.Children.ElementAt(3).EndDate, ds.Children.ElementAt(3).EndDate);
            Assert.AreEqual(newEndDate, ds.Children.ElementAt(4).EndDate); // error 2 to end
            Assert.AreEqual(newEndDate, ds.Children.ElementAt(5).EndDate); // error 2 to end

        }

        /// <summary>
        /// Dateshift recursive test
        /// </summary>
        [TestMethod]
        public void DateShiftRecursiveTest8()
        {
            DateShiftDTO ds = CreateDateShiftDTO();
            DateShiftDTO ds2 = CreateDateShiftDTO();
            DateShiftDetailModelView detail = CreateExpandRight(ChildModificationType.NoChange);
            detail.Error1FixSingleMonth = false;
            detail.Error2Handling = null;

            DateShiftCalculation.PerformShifts(ds, new DateShiftModelView { Details = new DateShiftDetailModelView[] { detail }, Workspace = GetWorkspace() }, null, null, null, Level.Workspace, string.Empty);

            Assert.IsFalse(detail.Errors.HasError1);
            Assert.IsFalse(detail.Errors.HasError2);
            Assert.IsFalse(detail.Errors.AffectedBoeIds.Any());
            Assert.AreEqual(StartDate, ds.StartDate);
            Assert.AreEqual(ds2.Children.ElementAt(0).StartDate, ds.Children.ElementAt(0).StartDate);
            Assert.AreEqual(ds2.Children.ElementAt(1).StartDate, ds.Children.ElementAt(1).StartDate);
            Assert.AreEqual(ds2.Children.ElementAt(1).Children.First().StartDate, ds.Children.ElementAt(1).Children.First().StartDate);
            Assert.AreEqual(ds2.Children.ElementAt(2).StartDate, ds.Children.ElementAt(2).StartDate);
            Assert.AreEqual(ds2.Children.ElementAt(3).StartDate, ds.Children.ElementAt(3).StartDate);
            Assert.AreEqual(ds2.Children.ElementAt(4).StartDate, ds.Children.ElementAt(4).StartDate);
            Assert.AreEqual(ds2.Children.ElementAt(5).StartDate, ds.Children.ElementAt(5).StartDate);


            DateTime newEndDate = EndDate.AddMonths(detail.MonthChange);
            Assert.AreEqual(newEndDate, ds.EndDate);
            Assert.AreEqual(ds2.Children.ElementAt(0).EndDate, ds.Children.ElementAt(0).EndDate);
            Assert.AreEqual(ds2.Children.ElementAt(1).EndDate, ds.Children.ElementAt(1).EndDate);
            Assert.AreEqual(ds2.Children.ElementAt(1).Children.First().EndDate, ds.Children.ElementAt(1).Children.First().EndDate);
            Assert.AreEqual(ds2.Children.ElementAt(2).EndDate, ds.Children.ElementAt(2).EndDate);
            Assert.AreEqual(ds2.Children.ElementAt(3).EndDate, ds.Children.ElementAt(3).EndDate);
            Assert.AreEqual(ds2.Children.ElementAt(4).EndDate, ds.Children.ElementAt(4).EndDate);
            Assert.AreEqual(ds2.Children.ElementAt(5).EndDate, ds.Children.ElementAt(5).EndDate);

        }

        /// <summary>
        /// Dateshift recursive test
        /// </summary>
        [TestMethod]
        public void DateShiftRecursiveTest9()
        {
            DateShiftDTO ds = CreateDateShiftDTO();
            DateShiftDTO ds2 = CreateDateShiftDTO();
            DateShiftDetailModelView detail = CreateExpandRight(ChildModificationType.ToEnd);
            detail.Error1FixSingleMonth = false;
            detail.Error2Handling = null;

            DateShiftCalculation.PerformShifts(ds, new DateShiftModelView { Details = new DateShiftDetailModelView[] { detail }, Workspace = GetWorkspace() }, null, null, null, Level.Workspace, string.Empty);

            Assert.IsFalse(detail.Errors.HasError1);
            Assert.IsFalse(detail.Errors.HasError2);
            Assert.IsFalse(detail.Errors.AffectedBoeIds.Any());
            Assert.AreEqual(StartDate, ds.StartDate);
            Assert.AreEqual(ds2.Children.ElementAt(0).StartDate, ds.Children.ElementAt(0).StartDate);
            Assert.AreEqual(ds2.Children.ElementAt(1).StartDate, ds.Children.ElementAt(1).StartDate);
            Assert.AreEqual(ds2.Children.ElementAt(1).Children.First().StartDate, ds.Children.ElementAt(1).Children.First().StartDate);
            Assert.AreEqual(ds2.Children.ElementAt(2).StartDate, ds.Children.ElementAt(2).StartDate);
            Assert.AreEqual(ds2.Children.ElementAt(3).StartDate, ds.Children.ElementAt(3).StartDate);
            Assert.AreEqual(ds2.Children.ElementAt(4).StartDate, ds.Children.ElementAt(4).StartDate);
            Assert.AreEqual(ds2.Children.ElementAt(5).StartDate, ds.Children.ElementAt(5).StartDate);


            DateTime newEndDate = EndDate.AddMonths(detail.MonthChange);
            Assert.AreEqual(newEndDate, ds.EndDate);
            Assert.AreEqual(newEndDate, ds.Children.ElementAt(0).EndDate);
            Assert.AreEqual(newEndDate, ds.Children.ElementAt(1).EndDate);
            Assert.AreEqual(newEndDate, ds.Children.ElementAt(1).Children.First().EndDate);
            Assert.AreEqual(newEndDate, ds.Children.ElementAt(2).EndDate);
            Assert.AreEqual(newEndDate, ds.Children.ElementAt(3).EndDate);
            Assert.AreEqual(newEndDate, ds.Children.ElementAt(4).EndDate);
            Assert.AreEqual(newEndDate, ds.Children.ElementAt(5).EndDate);

        }

        /// <summary>
        /// Dateshift recursive test
        /// </summary>
        [TestMethod]
        public void DateShiftRecursiveTest10()
        {
            DateShiftDTO ds = CreateDateShiftDTO();
            DateShiftDTO ds2 = CreateDateShiftDTO();
            DateShiftDetailModelView detail = CreateExpandRight(ChildModificationType.ToPoP);
            detail.Error1FixSingleMonth = false;
            detail.Error2Handling = null;

            DateShiftCalculation.PerformShifts(ds, new DateShiftModelView { Details = new DateShiftDetailModelView[] { detail }, Workspace = GetWorkspace() }, null, null, null, Level.Workspace, string.Empty);

            Assert.IsFalse(detail.Errors.HasError1);
            Assert.IsFalse(detail.Errors.HasError2);
            Assert.IsFalse(detail.Errors.AffectedBoeIds.Any());
            Assert.AreEqual(StartDate, ds.StartDate);
            Assert.AreEqual(StartDate, ds.Children.ElementAt(0).StartDate); 
            Assert.AreEqual(StartDate, ds.Children.ElementAt(1).StartDate); 
            Assert.AreEqual(StartDate, ds.Children.ElementAt(1).Children.First().StartDate); 
            Assert.AreEqual(StartDate, ds.Children.ElementAt(2).StartDate); 
            Assert.AreEqual(StartDate, ds.Children.ElementAt(3).StartDate);
            Assert.AreEqual(StartDate, ds.Children.ElementAt(4).StartDate); 
            Assert.AreEqual(StartDate, ds.Children.ElementAt(5).StartDate); 


            DateTime newEndDate = EndDate.AddMonths(detail.MonthChange);
            Assert.AreEqual(newEndDate, ds.EndDate);
            Assert.AreEqual(newEndDate, ds.Children.ElementAt(0).EndDate);
            Assert.AreEqual(newEndDate, ds.Children.ElementAt(1).EndDate);
            Assert.AreEqual(newEndDate, ds.Children.ElementAt(1).Children.First().EndDate);
            Assert.AreEqual(newEndDate, ds.Children.ElementAt(2).EndDate);
            Assert.AreEqual(newEndDate, ds.Children.ElementAt(3).EndDate);
            Assert.AreEqual(newEndDate, ds.Children.ElementAt(4).EndDate);
            Assert.AreEqual(newEndDate, ds.Children.ElementAt(5).EndDate);

        }

        /// <summary>
        /// Dateshift recursive test
        /// </summary>
        [TestMethod]
        public void DateShiftRecursiveTest11()
        {
            DateShiftDTO ds = CreateDateShiftDTO();
            DateShiftDTO ds2 = CreateDateShiftDTO();
            DateShiftDetailModelView detail = CreateExpandRight(ChildModificationType.FlowDown);
            detail.Error1FixSingleMonth = false;
            detail.Error2Handling = null;

            DateShiftCalculation.PerformShifts(ds, new DateShiftModelView { Details = new DateShiftDetailModelView[] { detail }, Workspace = GetWorkspace() }, null, null, null, Level.Workspace, string.Empty);

            Assert.IsFalse(detail.Errors.HasError1);
            Assert.IsFalse(detail.Errors.HasError2);
            Assert.IsFalse(detail.Errors.AffectedBoeIds.Any());
            Assert.AreEqual(StartDate, ds.StartDate);
            Assert.AreEqual(ds2.Children.ElementAt(0).StartDate, ds.Children.ElementAt(0).StartDate);
            Assert.AreEqual(ds2.Children.ElementAt(1).StartDate, ds.Children.ElementAt(1).StartDate);
            Assert.AreEqual(ds2.Children.ElementAt(1).Children.First().StartDate, ds.Children.ElementAt(1).Children.First().StartDate);
            Assert.AreEqual(ds2.Children.ElementAt(2).StartDate, ds.Children.ElementAt(2).StartDate);
            Assert.AreEqual(ds2.Children.ElementAt(3).StartDate, ds.Children.ElementAt(3).StartDate);
            Assert.AreEqual(ds2.Children.ElementAt(4).StartDate, ds.Children.ElementAt(4).StartDate);
            Assert.AreEqual(ds2.Children.ElementAt(5).StartDate, ds.Children.ElementAt(5).StartDate);


            DateTime newEndDate = EndDate.AddMonths(detail.MonthChange);
            Assert.AreEqual(newEndDate, ds.EndDate);
            Assert.AreEqual(ds2.Children.ElementAt(0).EndDate.Value.AddMonths(detail.MonthChange), ds.Children.ElementAt(0).EndDate);
            Assert.AreEqual(ds2.Children.ElementAt(1).EndDate.Value.AddMonths(detail.MonthChange), ds.Children.ElementAt(1).EndDate);
            Assert.AreEqual(ds2.Children.ElementAt(1).Children.First().EndDate.Value.AddMonths(detail.MonthChange), ds.Children.ElementAt(1).Children.First().EndDate);
            Assert.AreEqual(ds2.Children.ElementAt(2).EndDate.Value.AddMonths(detail.MonthChange), ds.Children.ElementAt(2).EndDate);
            Assert.AreEqual(ds2.Children.ElementAt(3).EndDate.Value.AddMonths(detail.MonthChange), ds.Children.ElementAt(3).EndDate);
            Assert.AreEqual(ds2.Children.ElementAt(4).EndDate.Value.AddMonths(detail.MonthChange), ds.Children.ElementAt(4).EndDate);
            Assert.AreEqual(ds2.Children.ElementAt(5).EndDate.Value.AddMonths(detail.MonthChange), ds.Children.ElementAt(5).EndDate);

        }

        /// <summary>
        /// Dateshift recursive test
        /// </summary>
        [TestMethod]
        public void DateShiftRecursiveTest12()
        {
            DateShiftDTO ds = CreateDateShiftDTO();
            DateShiftDTO ds2 = CreateDateShiftDTO();
            DateShiftDetailModelView detail = CreateShift(ChildModificationType.NoChange, 5);
            detail.Error1FixSingleMonth = false;
            detail.Error2Handling = null;

            DateShiftCalculation.PerformShifts(ds, new DateShiftModelView { Details = new DateShiftDetailModelView[] { detail }, Workspace = GetWorkspace() }, null, null, null, Level.Workspace, string.Empty);

            Assert.IsFalse(detail.Errors.HasError1);
            Assert.IsTrue(detail.Errors.HasError2);
            Assert.IsTrue(detail.Errors.AffectedBoeIds.Any());
            Assert.AreEqual(StartDate.AddMonths(detail.MonthChange), ds.StartDate);
            Assert.AreEqual(ds2.Children.ElementAt(0).StartDate, ds.Children.ElementAt(0).StartDate);
            Assert.AreEqual(ds2.Children.ElementAt(1).StartDate, ds.Children.ElementAt(1).StartDate);
            Assert.AreEqual(ds2.Children.ElementAt(1).Children.First().StartDate, ds.Children.ElementAt(1).Children.First().StartDate);
            Assert.AreEqual(ds2.Children.ElementAt(2).StartDate, ds.Children.ElementAt(2).StartDate);
            Assert.AreEqual(ds2.Children.ElementAt(3).StartDate, ds.Children.ElementAt(3).StartDate);
            Assert.AreEqual(ds2.Children.ElementAt(4).StartDate, ds.Children.ElementAt(4).StartDate);
            Assert.AreEqual(ds2.Children.ElementAt(5).StartDate, ds.Children.ElementAt(5).StartDate);


            DateTime newEndDate = EndDate.AddMonths(detail.MonthChange);
            Assert.AreEqual(newEndDate, ds.EndDate);
            Assert.AreEqual(ds2.Children.ElementAt(0).EndDate, ds.Children.ElementAt(0).EndDate);
            Assert.AreEqual(ds2.Children.ElementAt(1).EndDate, ds.Children.ElementAt(1).EndDate);
            Assert.AreEqual(ds2.Children.ElementAt(1).Children.First().EndDate, ds.Children.ElementAt(1).Children.First().EndDate);
            Assert.AreEqual(ds2.Children.ElementAt(2).EndDate, ds.Children.ElementAt(2).EndDate);
            Assert.AreEqual(ds2.Children.ElementAt(3).EndDate, ds.Children.ElementAt(3).EndDate);
            Assert.AreEqual(ds2.Children.ElementAt(4).EndDate, ds.Children.ElementAt(4).EndDate);
            Assert.AreEqual(ds2.Children.ElementAt(5).EndDate, ds.Children.ElementAt(5).EndDate);

        }

        /// <summary>
        /// Dateshift recursive test
        /// </summary>
        [TestMethod]
        public void DateShiftRecursiveTest13()
        {
            DateShiftDTO ds = CreateDateShiftDTO();
            DateShiftDTO ds2 = CreateDateShiftDTO();
            DateShiftDetailModelView detail = CreateShift(ChildModificationType.FlowDown, 5);
            detail.Error1FixSingleMonth = false;
            detail.Error2Handling = null;

            DateShiftCalculation.PerformShifts(ds, new DateShiftModelView { Details = new DateShiftDetailModelView[] { detail }, Workspace = GetWorkspace() }, null, null, null, Level.Workspace, string.Empty);

            Assert.IsFalse(detail.Errors.HasError1);
            Assert.IsFalse(detail.Errors.HasError2);
            Assert.IsFalse(detail.Errors.AffectedBoeIds.Any());
            Assert.AreEqual(StartDate.AddMonths(detail.MonthChange), ds.StartDate);
            Assert.AreEqual(ds2.Children.ElementAt(0).StartDate.Value.AddMonths(detail.MonthChange), ds.Children.ElementAt(0).StartDate);
            Assert.AreEqual(ds2.Children.ElementAt(1).StartDate.Value.AddMonths(detail.MonthChange), ds.Children.ElementAt(1).StartDate);
            Assert.AreEqual(ds2.Children.ElementAt(1).Children.First().StartDate.Value.AddMonths(detail.MonthChange), ds.Children.ElementAt(1).Children.First().StartDate);
            Assert.AreEqual(ds2.Children.ElementAt(2).StartDate.Value.AddMonths(detail.MonthChange), ds.Children.ElementAt(2).StartDate);
            Assert.AreEqual(ds2.Children.ElementAt(3).StartDate.Value.AddMonths(detail.MonthChange), ds.Children.ElementAt(3).StartDate);
            Assert.AreEqual(ds2.Children.ElementAt(4).StartDate.Value.AddMonths(detail.MonthChange), ds.Children.ElementAt(4).StartDate);
            Assert.AreEqual(ds2.Children.ElementAt(5).StartDate.Value.AddMonths(detail.MonthChange), ds.Children.ElementAt(5).StartDate);


            DateTime newEndDate = EndDate.AddMonths(detail.MonthChange);
            Assert.AreEqual(newEndDate, ds.EndDate);
            Assert.AreEqual(ds2.Children.ElementAt(0).EndDate.Value.AddMonths(detail.MonthChange), ds.Children.ElementAt(0).EndDate);
            Assert.AreEqual(ds2.Children.ElementAt(1).EndDate.Value.AddMonths(detail.MonthChange), ds.Children.ElementAt(1).EndDate);
            Assert.AreEqual(ds2.Children.ElementAt(1).Children.First().EndDate.Value.AddMonths(detail.MonthChange), ds.Children.ElementAt(1).Children.First().EndDate);
            Assert.AreEqual(ds2.Children.ElementAt(2).EndDate.Value.AddMonths(detail.MonthChange), ds.Children.ElementAt(2).EndDate);
            Assert.AreEqual(ds2.Children.ElementAt(3).EndDate.Value.AddMonths(detail.MonthChange), ds.Children.ElementAt(3).EndDate);
            Assert.AreEqual(ds2.Children.ElementAt(4).EndDate.Value.AddMonths(detail.MonthChange), ds.Children.ElementAt(4).EndDate);
            Assert.AreEqual(ds2.Children.ElementAt(5).EndDate.Value.AddMonths(detail.MonthChange), ds.Children.ElementAt(5).EndDate);

        }

        /// <summary>
        /// Dateshift recursive test
        /// </summary>
        [TestMethod]
        public void DateShiftRecursiveTest14()
        {
            DateShiftDTO ds = CreateDateShiftDTO();
            DateShiftDTO ds2 = CreateDateShiftDTO();
            DateShiftDetailModelView detail = CreateShift(ChildModificationType.ToPoP, 5);
            detail.Error1FixSingleMonth = false;
            detail.Error2Handling = null;

            DateShiftCalculation.PerformShifts(ds, new DateShiftModelView { Details = new DateShiftDetailModelView[] { detail }, Workspace = GetWorkspace() }, null, null, null, Level.Workspace, string.Empty);

            Assert.IsFalse(detail.Errors.HasError1);
            Assert.IsFalse(detail.Errors.HasError2);
            Assert.IsFalse(detail.Errors.AffectedBoeIds.Any());
            Assert.AreEqual(StartDate.AddMonths(detail.MonthChange), ds.StartDate);
            Assert.AreEqual(ds.StartDate, ds.Children.ElementAt(0).StartDate);
            Assert.AreEqual(ds.StartDate, ds.Children.ElementAt(1).StartDate);
            Assert.AreEqual(ds.StartDate, ds.Children.ElementAt(1).Children.First().StartDate);
            Assert.AreEqual(ds.StartDate, ds.Children.ElementAt(2).StartDate);
            Assert.AreEqual(ds.StartDate, ds.Children.ElementAt(3).StartDate);
            Assert.AreEqual(ds.StartDate, ds.Children.ElementAt(4).StartDate);
            Assert.AreEqual(ds.StartDate, ds.Children.ElementAt(5).StartDate);


            DateTime newEndDate = EndDate.AddMonths(detail.MonthChange);
            Assert.AreEqual(newEndDate, ds.EndDate);
            Assert.AreEqual(newEndDate, ds.Children.ElementAt(0).EndDate);
            Assert.AreEqual(newEndDate, ds.Children.ElementAt(1).EndDate);
            Assert.AreEqual(newEndDate, ds.Children.ElementAt(1).Children.First().EndDate);
            Assert.AreEqual(newEndDate, ds.Children.ElementAt(2).EndDate);
            Assert.AreEqual(newEndDate, ds.Children.ElementAt(3).EndDate);
            Assert.AreEqual(newEndDate, ds.Children.ElementAt(4).EndDate);
            Assert.AreEqual(newEndDate, ds.Children.ElementAt(5).EndDate);

        }

        /// <summary>
        /// Dateshift recursive test
        /// </summary>
        [TestMethod]
        public void DateShiftRecursiveTest15()
        {
            DateShiftDTO ds = CreateDateShiftDTO();
            DateShiftDTO ds2 = CreateDateShiftDTO();
            DateShiftDetailModelView detail = CreateShift(ChildModificationType.ToStart, 5);
            detail.Error1FixSingleMonth = false;
            detail.Error2Handling = null;

            DateShiftCalculation.PerformShifts(ds, new DateShiftModelView { Details = new DateShiftDetailModelView[] { detail }, Workspace = GetWorkspace() }, null, null, null, Level.Workspace, string.Empty);

            Assert.IsFalse(detail.Errors.HasError1);
            Assert.IsFalse(detail.Errors.HasError2);
            Assert.IsFalse(detail.Errors.AffectedBoeIds.Any());
            Assert.AreEqual(StartDate.AddMonths(detail.MonthChange), ds.StartDate);
            Assert.AreEqual(ds.StartDate, ds.Children.ElementAt(0).StartDate);
            Assert.AreEqual(ds.StartDate, ds.Children.ElementAt(1).StartDate);
            Assert.AreEqual(ds.StartDate, ds.Children.ElementAt(1).Children.First().StartDate);
            Assert.AreEqual(ds.StartDate, ds.Children.ElementAt(2).StartDate);
            Assert.AreEqual(ds.StartDate, ds.Children.ElementAt(3).StartDate);
            Assert.AreEqual(ds.StartDate, ds.Children.ElementAt(4).StartDate);
            Assert.AreEqual(ds.StartDate, ds.Children.ElementAt(5).StartDate);


            DateTime newEndDate = EndDate.AddMonths(detail.MonthChange);
            Assert.AreEqual(newEndDate, ds.EndDate);
            // make sure duration is the same as it was
            Assert.AreEqual(ds.Children.ElementAt(0).StartDate.Value.AddMonths(GetDuration(ds2.Children.ElementAt(0))), ds.Children.ElementAt(0).EndDate);
            Assert.AreEqual(ds.Children.ElementAt(1).StartDate.Value.AddMonths(GetDuration(ds2.Children.ElementAt(1))), ds.Children.ElementAt(1).EndDate);
            Assert.AreEqual(ds.Children.ElementAt(1).Children.First().StartDate.Value.AddMonths(GetDuration(ds2.Children.ElementAt(1).Children.First())), ds.Children.ElementAt(1).Children.First().EndDate);
            Assert.AreEqual(ds.Children.ElementAt(2).StartDate.Value.AddMonths(GetDuration(ds2.Children.ElementAt(2))), ds.Children.ElementAt(2).EndDate);
            Assert.AreEqual(ds.Children.ElementAt(3).StartDate.Value.AddMonths(GetDuration(ds2.Children.ElementAt(3))), ds.Children.ElementAt(3).EndDate);
            Assert.AreEqual(ds.Children.ElementAt(4).StartDate.Value.AddMonths(GetDuration(ds2.Children.ElementAt(4))), ds.Children.ElementAt(4).EndDate);
            Assert.AreEqual(ds.Children.ElementAt(5).StartDate.Value.AddMonths(GetDuration(ds2.Children.ElementAt(5))), ds.Children.ElementAt(5).EndDate);

        }

        /// <summary>
        /// Dateshift recursive test
        /// </summary>
        [TestMethod]
        public void DateShiftRecursiveTest16()
        {
			DateShiftDTO ds = CreateDateShiftDTO();
			DateShiftDTO ds2 = CreateDateShiftDTO();
            DateShiftDetailModelView detail = CreateShift(ChildModificationType.FlowDown, -5);
            detail.Error1FixSingleMonth = false;
            detail.Error2Handling = null;

            DateShiftCalculation.PerformShifts(ds, new DateShiftModelView { Details = new DateShiftDetailModelView[] { detail }, Workspace = GetWorkspace() }, null, null, null, Level.Workspace, string.Empty);

            Assert.IsFalse(detail.Errors.HasError1);
            Assert.IsFalse(detail.Errors.HasError2);
            Assert.IsFalse(detail.Errors.AffectedBoeIds.Any());
            Assert.AreEqual(StartDate.AddMonths(detail.MonthChange), ds.StartDate);
            Assert.AreEqual(ds2.Children.ElementAt(0).StartDate.Value.AddMonths(detail.MonthChange), ds.Children.ElementAt(0).StartDate);
            Assert.AreEqual(ds2.Children.ElementAt(1).StartDate.Value.AddMonths(detail.MonthChange), ds.Children.ElementAt(1).StartDate);
            Assert.AreEqual(ds2.Children.ElementAt(1).Children.First().StartDate.Value.AddMonths(detail.MonthChange), ds.Children.ElementAt(1).Children.First().StartDate);
            Assert.AreEqual(ds2.Children.ElementAt(2).StartDate.Value.AddMonths(detail.MonthChange), ds.Children.ElementAt(2).StartDate);
            Assert.AreEqual(ds2.Children.ElementAt(3).StartDate.Value.AddMonths(detail.MonthChange), ds.Children.ElementAt(3).StartDate);
            Assert.AreEqual(ds2.Children.ElementAt(4).StartDate.Value.AddMonths(detail.MonthChange), ds.Children.ElementAt(4).StartDate);
            Assert.AreEqual(ds2.Children.ElementAt(5).StartDate.Value.AddMonths(detail.MonthChange), ds.Children.ElementAt(5).StartDate);


            DateTime newEndDate = EndDate.AddMonths(detail.MonthChange);
            Assert.AreEqual(newEndDate, ds.EndDate);
            Assert.AreEqual(ds2.Children.ElementAt(0).EndDate.Value.AddMonths(detail.MonthChange), ds.Children.ElementAt(0).EndDate);
            Assert.AreEqual(ds2.Children.ElementAt(1).EndDate.Value.AddMonths(detail.MonthChange), ds.Children.ElementAt(1).EndDate);
            Assert.AreEqual(ds2.Children.ElementAt(1).Children.First().EndDate.Value.AddMonths(detail.MonthChange), ds.Children.ElementAt(1).Children.First().EndDate);
            Assert.AreEqual(ds2.Children.ElementAt(2).EndDate.Value.AddMonths(detail.MonthChange), ds.Children.ElementAt(2).EndDate);
            Assert.AreEqual(ds2.Children.ElementAt(3).EndDate.Value.AddMonths(detail.MonthChange), ds.Children.ElementAt(3).EndDate);
            Assert.AreEqual(ds2.Children.ElementAt(4).EndDate.Value.AddMonths(detail.MonthChange), ds.Children.ElementAt(4).EndDate);
            Assert.AreEqual(ds2.Children.ElementAt(5).EndDate.Value.AddMonths(detail.MonthChange), ds.Children.ElementAt(5).EndDate);

        }

        /// <summary>
        /// Dateshift recursive test
        /// </summary>
        [TestMethod]
        public void DateShiftRecursiveTest17()
        {
            DateShiftDTO ds = CreateDateShiftDTO();
            DateShiftDTO ds2 = CreateDateShiftDTO();
            DateShiftDetailModelView detail = CreateShift(ChildModificationType.ToPoP, -5);
            detail.Error1FixSingleMonth = false;
            detail.Error2Handling = null;

            DateShiftCalculation.PerformShifts(ds, new DateShiftModelView { Details = new DateShiftDetailModelView[] { detail }, Workspace = GetWorkspace() }, null, null, null, Level.Workspace, string.Empty);

            Assert.IsFalse(detail.Errors.HasError1);
            Assert.IsFalse(detail.Errors.HasError2);
            Assert.IsFalse(detail.Errors.AffectedBoeIds.Any());
            Assert.AreEqual(StartDate.AddMonths(detail.MonthChange), ds.StartDate);
            Assert.AreEqual(ds.StartDate, ds.Children.ElementAt(0).StartDate);
            Assert.AreEqual(ds.StartDate, ds.Children.ElementAt(1).StartDate);
            Assert.AreEqual(ds.StartDate, ds.Children.ElementAt(1).Children.First().StartDate);
            Assert.AreEqual(ds.StartDate, ds.Children.ElementAt(2).StartDate);
            Assert.AreEqual(ds.StartDate, ds.Children.ElementAt(3).StartDate);
            Assert.AreEqual(ds.StartDate, ds.Children.ElementAt(4).StartDate);
            Assert.AreEqual(ds.StartDate, ds.Children.ElementAt(5).StartDate);


            DateTime newEndDate = EndDate.AddMonths(detail.MonthChange);
            Assert.AreEqual(newEndDate, ds.EndDate);
            Assert.AreEqual(newEndDate, ds.Children.ElementAt(0).EndDate);
            Assert.AreEqual(newEndDate, ds.Children.ElementAt(1).EndDate);
            Assert.AreEqual(newEndDate, ds.Children.ElementAt(1).Children.First().EndDate);
            Assert.AreEqual(newEndDate, ds.Children.ElementAt(2).EndDate);
            Assert.AreEqual(newEndDate, ds.Children.ElementAt(3).EndDate);
            Assert.AreEqual(newEndDate, ds.Children.ElementAt(4).EndDate);
            Assert.AreEqual(newEndDate, ds.Children.ElementAt(5).EndDate);

        }

        /// <summary>
        /// Dateshift recursive test
        /// </summary>
        [TestMethod]
        public void DateShiftRecursiveTest18()
        {
			DateShiftDTO ds = CreateDateShiftDTO();
			DateShiftDTO ds2 = CreateDateShiftDTO();
            DateShiftDetailModelView detail = CreateShift(ChildModificationType.ToEnd, -5);
            detail.Error1FixSingleMonth = false;
            detail.Error2Handling = null;

            DateShiftCalculation.PerformShifts(ds, new DateShiftModelView { Details = new DateShiftDetailModelView[] { detail }, Workspace = GetWorkspace() }, null, null, null, Level.Workspace, string.Empty);

            Assert.IsFalse(detail.Errors.HasError1);
            Assert.IsFalse(detail.Errors.HasError2);
            Assert.IsFalse(detail.Errors.AffectedBoeIds.Any());
            Assert.AreEqual(EndDate.AddMonths(detail.MonthChange), ds.EndDate);
            Assert.AreEqual(ds.EndDate, ds.Children.ElementAt(0).EndDate);
            Assert.AreEqual(ds.EndDate, ds.Children.ElementAt(1).EndDate);
            Assert.AreEqual(ds.EndDate, ds.Children.ElementAt(1).Children.First().EndDate);
            Assert.AreEqual(ds.EndDate, ds.Children.ElementAt(2).EndDate);
            Assert.AreEqual(ds.EndDate, ds.Children.ElementAt(3).EndDate);
            Assert.AreEqual(ds.EndDate, ds.Children.ElementAt(4).EndDate);
            Assert.AreEqual(ds.EndDate, ds.Children.ElementAt(5).EndDate);


            DateTime newStartDate = StartDate.AddMonths(detail.MonthChange);
            Assert.AreEqual(newStartDate, ds.StartDate);
            // make sure duration is the same as it was
            Assert.AreEqual(ds.Children.ElementAt(0).StartDate.Value.AddMonths(GetDuration(ds2.Children.ElementAt(0))), ds.Children.ElementAt(0).EndDate);
            Assert.AreEqual(ds.Children.ElementAt(1).StartDate.Value.AddMonths(GetDuration(ds2.Children.ElementAt(1))), ds.Children.ElementAt(1).EndDate);
            Assert.AreEqual(ds.Children.ElementAt(1).Children.First().StartDate.Value.AddMonths(GetDuration(ds2.Children.ElementAt(1).Children.First())), ds.Children.ElementAt(1).Children.First().EndDate);
            Assert.AreEqual(ds.Children.ElementAt(2).StartDate.Value.AddMonths(GetDuration(ds2.Children.ElementAt(2))), ds.Children.ElementAt(2).EndDate);
            Assert.AreEqual(ds.Children.ElementAt(3).StartDate.Value.AddMonths(GetDuration(ds2.Children.ElementAt(3))), ds.Children.ElementAt(3).EndDate);
            Assert.AreEqual(ds.Children.ElementAt(4).StartDate.Value.AddMonths(GetDuration(ds2.Children.ElementAt(4))), ds.Children.ElementAt(4).EndDate);
            Assert.AreEqual(ds.Children.ElementAt(5).StartDate.Value.AddMonths(GetDuration(ds2.Children.ElementAt(5))), ds.Children.ElementAt(5).EndDate);

        }

        #endregion DateShiftDTO tests

        #region utility methods

        private static DateShiftDTO CreateDateShiftDTO()
        {
			// start is Jan 2019, end is jun 19
			DateShiftDTO ds = new DateShiftDTO
			{
                DateShiftLevel = Level.Workspace,
                StartDate = StartDate,
                EndDate = EndDate,
                HasSpread = false
            };

			DateShiftDTO child1 = new DateShiftDTO
			{
                DateShiftLevel = Level.CLIN,
                StartDate = StartDate.AddMonths(1),
                EndDate = EndDate.AddMonths(-1),
                HasSpread = false
            };


			DateShiftDTO child2 = new DateShiftDTO
			{
                DateShiftLevel = Level.BOE,
                BoeId = 2,
                StartDate = StartDate,
                EndDate = EndDate,
                HasSpread = false
            };

            child2.Children.Add(new DateShiftDTO
			{
                DateShiftLevel = Level.Task,
                BoeId = 2,
                StartDate = StartDate,
                EndDate = EndDate,
                HasSpread = false
            });

			DateShiftDTO child3 = new DateShiftDTO
			{
                DateShiftLevel = Level.BOE,
                BoeId = 3,
                StartDate = EndDate,
                EndDate = EndDate,
                HasSpread = false
            };

			DateShiftDTO child4 = new DateShiftDTO
			{
                DateShiftLevel = Level.BOE,
                BoeId = 4,
                StartDate = StartDate,
                EndDate = StartDate,
                HasSpread = false
            };

			DateShiftDTO child5 = new DateShiftDTO
			{
                DateShiftLevel = Level.BOE,
                BoeId = 5,
                StartDate = StartDate.AddMonths(2),
                EndDate = EndDate,
                HasSpread = false
            };

			DateShiftDTO child6 = new DateShiftDTO
			{
                DateShiftLevel = Level.BOE,
                BoeId = 6,
                StartDate = StartDate,
                EndDate = EndDate.AddMonths(-2),
                HasSpread = false
            };

            ds.Children.AddRange(new DateShiftDTO[] { child1, child2, child3, child4, child5, child6 });

            return ds;
        }

        /// <summary>
        /// Gets the duration.
        /// </summary>
        /// <param name="DateShiftDTO">The date shiftable.</param>
        /// <returns></returns>
        private int GetDuration(DateShiftDTO DateShiftDTO)
        {
            return DateShiftDTO.StartDate.Value.MonthDifference(DateShiftDTO.EndDate.Value);
        }

        /// <summary>
        /// Compares the children.
        /// </summary>
        /// <param name="expected">The expected.</param>
        /// <param name="result">The result.</param>
        /// <param name="detail">The detail.</param>
        private static void CompareChildren(DateShiftDTO expected, DateShiftDTO result, DateShiftDetailModelView detail)
        {
            if (expected.Children.Any())
            {
                for (int i = 0; i < expected.Children.Count; i++)
                {
					DateShiftDTO expectedChild = expected.Children.ElementAt(i);
					DateShiftDTO resultChild = result.Children.ElementAt(i);
                    switch (detail.ChildModificationType)
                    {
                        case ChildModificationType.NoChange:
                            if (expectedChild.StartDate < result.StartDate || expectedChild.EndDate > result.EndDate)
                            {
                                // error 2
                                VerifyError2(expectedChild, resultChild, detail, result);
                            }
                            else
                            {
                                Assert.AreEqual(expectedChild.StartDate, resultChild.StartDate);
                                Assert.AreEqual(expectedChild.EndDate, resultChild.EndDate);
                            }
                            break;
                        case ChildModificationType.ToPoP:
                            Assert.AreEqual(result.StartDate, resultChild.StartDate);
                            Assert.AreEqual(result.EndDate, resultChild.EndDate);
                            break;
                        case ChildModificationType.ToStart:
                            Assert.AreEqual(result.StartDate, resultChild.StartDate);
                            Assert.AreEqual(expectedChild.StartDate.Value.MonthDifference(expectedChild.EndDate.Value), resultChild.StartDate.Value.MonthDifference(resultChild.EndDate.Value));
                            break;
                        case ChildModificationType.ToEnd:
                            Assert.AreEqual(result.EndDate, resultChild.EndDate);
                            if (detail.Operation == Operation.DurationChange)
                            {
                                Assert.AreEqual(expectedChild.StartDate, resultChild.StartDate);
                            }
                            else
                            {
                                if (detail.MonthChange < 0)
                                {
                                    int duration = expectedChild.StartDate.Value.MonthDifference(expectedChild.EndDate.Value);
                                    DateTime start = result.EndDate.Value.AddMonths(-1 * duration);
                                    if (start < result.StartDate)
                                    {
                                        // This is Error2
                                        Assert.IsTrue(detail.Errors.HasError2);
                                        if (detail.Error2Handling.HasValue)
                                        {
                                            switch (detail.Error2Handling.Value)
                                            {
                                                case ChildModificationType.NoChange:
                                                    Assert.IsTrue(detail.Errors.HasError2);
                                                    break;
                                                case ChildModificationType.ToStart:
                                                case ChildModificationType.ToPoP:
                                                    Assert.AreEqual(result.StartDate, resultChild.StartDate);
                                                    break;
                                                default:
                                                    Assert.Fail("Invalid Child modification type for Error 2 handling");
                                                    break;
                                            }
                                        }
                                        else
                                        {
                                            Assert.Fail("this is invalid, should pop up to user invalid");
                                        }
                                    }
                                    else
                                    {
                                        Assert.AreEqual(start, resultChild.StartDate);
                                    }
                                }
                                else
                                {
                                    Assert.Fail("This is an invalid combo");
                                }
                            }
                            break;
                        case ChildModificationType.FlowDown:
                            Tuple<DateTime, DateTime> expectedFlowdown = FlowdownExpectedResults[detail.Operation][detail.MonthChange].ElementAt(i);
                            if (detail.Error1FixSingleMonth == true && expectedFlowdown.Item1 > expectedFlowdown.Item2)
                            {
                                Assert.AreEqual(expectedFlowdown.Item1, resultChild.StartDate.Value);
                                Assert.AreEqual(expectedFlowdown.Item1, resultChild.EndDate.Value);

                            }
                            else
                            {
                                Assert.AreEqual(expectedFlowdown.Item1, resultChild.StartDate.Value);
                                Assert.AreEqual(expectedFlowdown.Item2, resultChild.EndDate.Value);
                            }
                            break;
                    }

                    // recursive
                    CompareChildren(expectedChild, resultChild, detail);
                }
            }
        }

        /// <summary>
        /// Verifies error2.
        /// </summary>
        /// <param name="expectedChild">The expected child.</param>
        /// <param name="resultChild">The result child.</param>
        /// <param name="detail">The detail.</param>
        /// <param name="parent">The parent.</param>
        /// <exception cref="NotSupportedException"></exception>
        private static void VerifyError2(DateShiftDTO expectedChild, DateShiftDTO resultChild, DateShiftDetailModelView detail, DateShiftDTO parent)
        {
            if (detail.Error2Handling.HasValue)
            {
                switch(detail.Error2Handling.Value)
                {
                    case ChildModificationType.NoChange:
                        Assert.AreEqual(expectedChild.StartDate, resultChild.StartDate);
                        Assert.AreEqual(expectedChild.EndDate, resultChild.EndDate);
                        break;
                    case ChildModificationType.ToEnd:
                        Assert.AreEqual(parent.EndDate, resultChild.EndDate);
                        break;
                    case ChildModificationType.ToPoP:
                        Assert.AreEqual(parent.StartDate, resultChild.StartDate);
                        Assert.AreEqual(parent.EndDate, resultChild.EndDate);
                        break;
                    default:
                        throw new NotSupportedException();
                }
            }
            else
            {
                Assert.IsTrue(detail.Errors.HasError2);
            }
        }
        #endregion utility methods
    }
}












