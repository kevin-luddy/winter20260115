// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.ActionLogic
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using GenBOE.Dtos;
    using IES.Common;
	using IES.Common.classes;
	using IES.Common.Exceptions;
	using Microsoft.Practices.Unity;
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using Moq;

    [TestClass]
    public class SpreadCurveTest
    {
		private Mock<IActiveDirectoryUtilities> activeDirectoryUtilities = new Mock<IActiveDirectoryUtilities>();
		[TestInitialize]
		public void CreateSystem()
		{
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IActiveDirectoryUtilities), activeDirectoryUtilities.Object);
		}

		// This function will call the Business Layer for the Labor Spread Calculations and
		// will be used by all 50 spread curve tests
		public Collection<ResourceSpreadDto> LaborSpreadCalcs(DateTime inStart, DateTime inEnd, long inHoursSpread, SpreadCurves inCurveID, WorkspaceDTO ws)
        {
            if (ws == null)
            {
                throw new ArgumentNullException(nameof(ws));
            }

            LaborSpreadRequest laborS = new LaborSpreadRequest();
            Collection<ResourceSpreadDto> sut = new Collection<ResourceSpreadDto>();
            laborS.StartDate = inStart;
            laborS.EndDate = inEnd;
            laborS.CurveID = inCurveID;
            laborS.HourSpread = inHoursSpread;
            sut = SpreadCurve.CalculateLaborSpreadsBasedOnCurve(laborS, ws.DecimalPrecision);

            return sut;
        }

        /// <summary>
        /// This test case will test Curve Test 1. It will verify that null data was not
        /// returned from the Labor Spread calculation and that the total value returned
        /// equalled the HoursSpread that was inputted
        /// </summary>
        [TestMethod]
        public void CurveTest_1()
        {

			Collection<ResourceSpreadDto> sut = new Collection<ResourceSpreadDto>();
            DateTime start = Convert.ToDateTime("09/2010");
            DateTime end  = Convert.ToDateTime("08/2011");
            SpreadCurves CurveID = SpreadCurves.SpreadCurve1;
            long HoursSpread = 1000;

            WorkspaceDTO workspace = new WorkspaceDTO() { ResourceDecimalPrecision = 0 };

            sut = LaborSpreadCalcs(start, end, HoursSpread, CurveID, workspace);

            Assert.IsNotNull(sut, "The value returned from Spread Curve BL was bad");

            // Make sure that all the values returned sum up to HoursSpread
            long Cumulative = 0;
            for (int x = 0; x < sut.Count(); x++)
            {
                Cumulative = Convert.ToInt64(sut[x].LaborSpreadValue) + Cumulative;
            }
            Assert.IsTrue(Cumulative == HoursSpread, "The return value did not equal HoursSpread");
        }

        /// <summary>
        /// This test case will test Curve Test 1. It will verify that null data was not
        /// returned from the Labor Spread calculation and that the total value returned
        /// equalled the HoursSpread that was inputted (while exercising Int64 values)
        /// </summary>
        [TestMethod]
        public void CurveTest_1_Int64()
        {

			Collection<ResourceSpreadDto> sut = new Collection<ResourceSpreadDto>();
            DateTime start = Convert.ToDateTime("09/2010");
            DateTime end = Convert.ToDateTime("08/2011");
            SpreadCurves CurveID = SpreadCurves.SpreadCurve1;
            long HoursSpread = 8888888888;

            WorkspaceDTO workspace = new WorkspaceDTO() { ResourceDecimalPrecision = 0 };

            sut = LaborSpreadCalcs(start, end, HoursSpread, CurveID, workspace);

            Assert.IsNotNull(sut, "The value returned from Spread Curve BL was bad");

            // Make sure that all the values returned sum up to HoursSpread
            long Cumulative = 0;
            for (int x = 0; x < sut.Count(); x++)
            {
                Cumulative = Convert.ToInt64(sut[x].LaborSpreadValue) + Cumulative;
            }
            Assert.IsTrue(Cumulative == HoursSpread, "The return value did not equal HoursSpread");
        }

        /// <summary>
        /// This test case will test Curve Test 2. It will verify that null data was not
        /// returned from the Labor Spread calculation and that the total value returned
        /// equalled the HoursSpread that was inputted
        /// </summary>
        [TestMethod]
        public void CurveTest_2()
        {

			Collection<ResourceSpreadDto> sut = new Collection<ResourceSpreadDto>();
            DateTime start = Convert.ToDateTime("09/2010");
            DateTime end = Convert.ToDateTime("12/2010");
            SpreadCurves CurveID = SpreadCurves.SpreadCurve2;
            long HoursSpread = 9000004000;

            WorkspaceDTO workspace = new WorkspaceDTO() { ResourceDecimalPrecision = 0 };

            sut = LaborSpreadCalcs(start, end, HoursSpread, CurveID, workspace);

            Assert.IsNotNull(sut, "The value returned from Spread Curve BL was bad");

            // Make sure that all the values returned sum up to HoursSpread
            long Cumulative = 0;
            for (int x = 0; x < sut.Count(); x++)
            {
                Cumulative = Convert.ToInt64(sut[x].LaborSpreadValue) + Cumulative;
            }
            Assert.IsTrue(Cumulative == HoursSpread, "The return value did not equal HoursSpread");
        }

        /// <summary>
        /// This test case will test Curve Test 3. It will verify that null data was not
        /// returned from the Labor Spread calculation and that the total value returned
        /// equalled the HoursSpread that was inputted
        /// </summary>
        [TestMethod]
        public void CurveTest_3()
        {
			// Spread Curve ID = 3
			Collection<ResourceSpreadDto> sut = new Collection<ResourceSpreadDto>();
           DateTime start = Convert.ToDateTime("09/2010");
            DateTime end = Convert.ToDateTime("08/2011"); // should produce 12 months
            SpreadCurves CurveID = SpreadCurves.SpreadCurve3;
            long HoursSpread = 9000000100;

            WorkspaceDTO workspace = new WorkspaceDTO() { ResourceDecimalPrecision = 0 };

            sut = LaborSpreadCalcs(start, end, HoursSpread, CurveID, workspace);

            Assert.IsNotNull(sut, "The value returned from Spread Curve BL was bad");

            // Make sure that all the values returned sum up to HoursSpread
            long Cumulative = 0;
            for (int x = 0; x < sut.Count(); x++)
            {
                Cumulative = Convert.ToInt64(sut[x].LaborSpreadValue) + Cumulative;
            }
            Assert.IsTrue(Cumulative == HoursSpread, "The return value did not equal HoursSpread");
        }

        /// <summary>
        /// This test case will test Curve Test 4. It will verify that null data was not
        /// returned from the Labor Spread calculation and that the total value returned
        /// equalled the HoursSpread that was inputted
        /// </summary>
        [TestMethod]
        public void CurveTest_4()
        {

			Collection<ResourceSpreadDto> sut = new Collection<ResourceSpreadDto>();
            DateTime start = Convert.ToDateTime("09/2010");
            DateTime end = Convert.ToDateTime("09/2011");
            SpreadCurves CurveID = SpreadCurves.SpreadCurve4;
            long HoursSpread = 9000085000;

            WorkspaceDTO workspace = new WorkspaceDTO() { ResourceDecimalPrecision = 0 };

            sut = LaborSpreadCalcs(start, end, HoursSpread, CurveID, workspace);

            Assert.IsNotNull(sut, "The value returned from Spread Curve BL was bad");

            // Make sure that all the values returned sum up to HoursSpread
            long Cumulative = 0;
            for (int x = 0; x < sut.Count(); x++)
            {
                Cumulative = Convert.ToInt64(sut[x].LaborSpreadValue) + Cumulative;
            }
            Assert.IsTrue(Cumulative == HoursSpread, "The return value did not equal HoursSpread");
        }

        /// <summary>
        /// This test case will test Curve Test 5. It will verify that null data was not
        /// returned from the Labor Spread calculation and that the total value returned
        /// equalled the HoursSpread that was inputted
        /// </summary>
        [TestMethod]
        public void CurveTest_5()
        {

			Collection<ResourceSpreadDto> sut = new Collection<ResourceSpreadDto>();
            DateTime start = Convert.ToDateTime("09/2010");
            DateTime end = Convert.ToDateTime("09/2012");
            SpreadCurves CurveID = SpreadCurves.SpreadCurve5;
            long HoursSpread = 9000062578;

            WorkspaceDTO workspace = new WorkspaceDTO() { ResourceDecimalPrecision = 0 };

            sut = LaborSpreadCalcs(start, end, HoursSpread, CurveID, workspace);

            Assert.IsNotNull(sut, "The value returned from Spread Curve BL was bad");

            // Make sure that all the values returned sum up to HoursSpread
            long Cumulative = 0;
            for (int x = 0; x < sut.Count(); x++)
            {
                Cumulative = Convert.ToInt64(sut[x].LaborSpreadValue) + Cumulative;
            }
            Assert.IsTrue(Cumulative == HoursSpread, "The return value did not equal HoursSpread");
        }

        /// <summary>
        /// This test case will test Curve Test 6. It will verify that null data was not
        /// returned from the Labor Spread calculation and that the total value returned
        /// equalled the HoursSpread that was inputted
        /// </summary>
        [TestMethod]
        public void CurveTest_6()
        {

			Collection<ResourceSpreadDto> sut = new Collection<ResourceSpreadDto>();
            DateTime start = Convert.ToDateTime("10/2010");
            DateTime end = Convert.ToDateTime("03/2012");
            SpreadCurves CurveID = SpreadCurves.SpreadCurve6;
            long HoursSpread = 9000007002;

            WorkspaceDTO workspace = new WorkspaceDTO() { ResourceDecimalPrecision = 0 };

            sut = LaborSpreadCalcs(start, end, HoursSpread, CurveID, workspace);

            Assert.IsNotNull(sut, "The value returned from Spread Curve BL was bad");

            // Make sure that all the values returned sum up to HoursSpread
            long Cumulative = 0;
            for (int x = 0; x < sut.Count(); x++)
            {
                Cumulative = Convert.ToInt64(sut[x].LaborSpreadValue) + Cumulative;
            }
            Assert.IsTrue(Cumulative == HoursSpread, "The return value did not equal HoursSpread");
        }

        /// <summary>
        /// This test case will test Curve Test 7. It will verify that null data was not
        /// returned from the Labor Spread calculation and that the total value returned
        /// equalled the HoursSpread that was inputted
        /// </summary>
        [TestMethod]
        public void CurveTest_7()
        {

			Collection<ResourceSpreadDto> sut = new Collection<ResourceSpreadDto>();
            DateTime start = Convert.ToDateTime("11/2011");
            DateTime end = Convert.ToDateTime("03/2012");
            SpreadCurves CurveID = SpreadCurves.SpreadCurve7;
            long HoursSpread = 9000003230;

            WorkspaceDTO workspace = new WorkspaceDTO() { ResourceDecimalPrecision = 0 };

            sut = LaborSpreadCalcs(start, end, HoursSpread, CurveID, workspace);

            Assert.IsNotNull(sut, "The value returned from Spread Curve BL was bad");

            // Make sure that all the values returned sum up to HoursSpread
            long Cumulative = 0;
            for (int x = 0; x < sut.Count(); x++)
            {
                Cumulative = Convert.ToInt64(sut[x].LaborSpreadValue) + Cumulative;
            }
            Assert.IsTrue(Cumulative == HoursSpread, "The return value did not equal HoursSpread");
        }

        /// <summary>
        /// This test case will test Curve Test 8. It will verify that null data was not
        /// returned from the Labor Spread calculation and that the total value returned
        /// equalled the HoursSpread that was inputted
        /// </summary>
        [TestMethod]
        public void CurveTest_8()
        {

			Collection<ResourceSpreadDto> sut = new Collection<ResourceSpreadDto>();
            DateTime start = Convert.ToDateTime("03/2010");
            DateTime end = Convert.ToDateTime("03/2011");
            SpreadCurves CurveID = SpreadCurves.SpreadCurve8;
            long HoursSpread = 9000000650;

            WorkspaceDTO workspace = new WorkspaceDTO() { ResourceDecimalPrecision = 0 };

            sut = LaborSpreadCalcs(start, end, HoursSpread, CurveID, workspace);

            Assert.IsNotNull(sut, "The value returned from Spread Curve BL was bad");

            // Make sure that all the values returned sum up to HoursSpread
            long Cumulative = 0;
            for (int x = 0; x < sut.Count(); x++)
            {
                Cumulative = Convert.ToInt64(sut[x].LaborSpreadValue) + Cumulative;
            }
            Assert.IsTrue(Cumulative == HoursSpread, "The return value did not equal HoursSpread");
        }

        /// <summary>
        /// This test case will test Curve Test 9. It will verify that null data was not
        /// returned from the Labor Spread calculation and that the total value returned
        /// equalled the HoursSpread that was inputted
        /// </summary>
        [TestMethod]
        public void CurveTest_9()
        {

			Collection<ResourceSpreadDto> sut = new Collection<ResourceSpreadDto>();
            DateTime start = Convert.ToDateTime("09/2010");
            DateTime end = Convert.ToDateTime("05/2011");
            SpreadCurves CurveID = SpreadCurves.SpreadCurve9;
            long HoursSpread = 9000065843;

            WorkspaceDTO workspace = new WorkspaceDTO() { ResourceDecimalPrecision = 0 };

            sut = LaborSpreadCalcs(start, end, HoursSpread, CurveID, workspace);

            Assert.IsNotNull(sut, "The value returned from Spread Curve BL was bad");

            // Make sure that all the values returned sum up to HoursSpread
            long Cumulative = 0;
            for (int x = 0; x < sut.Count(); x++)
            {
                Cumulative = Convert.ToInt64(sut[x].LaborSpreadValue) + Cumulative;
            }
            Assert.IsTrue(Cumulative == HoursSpread, "The return value did not equal HoursSpread");
        }

        /// <summary>
        /// This test case will test Curve Test 10. It will verify that null data was not
        /// returned from the Labor Spread calculation and that the total value returned
        /// equalled the HoursSpread that was inputted
        /// </summary>
        [TestMethod]
        public void CurveTest_10()
        {

			Collection<ResourceSpreadDto> sut = new Collection<ResourceSpreadDto>();
            DateTime start = Convert.ToDateTime("09/2010");
            DateTime end = Convert.ToDateTime("12/2012");
            SpreadCurves CurveID = SpreadCurves.SpreadCurve10;
            long HoursSpread = 9000046540;

            WorkspaceDTO workspace = new WorkspaceDTO() { ResourceDecimalPrecision = 0 };

            sut = LaborSpreadCalcs(start, end, HoursSpread, CurveID, workspace);

            Assert.IsNotNull(sut, "The value returned from Spread Curve BL was bad");

            // Make sure that all the values returned sum up to HoursSpread
            long Cumulative = 0;
            for (int x = 0; x < sut.Count(); x++)
            {
                Cumulative = Convert.ToInt64(sut[x].LaborSpreadValue) + Cumulative;
            }
            Assert.IsTrue(Cumulative == HoursSpread, "The return value did not equal HoursSpread");
        }

        /// <summary>
        /// This test case will test Curve Test 11. It will verify that null data was not
        /// returned from the Labor Spread calculation and that the total value returned
        /// equalled the HoursSpread that was inputted
        /// </summary>
        [TestMethod]
        public void CurveTest_11()
        {

			Collection<ResourceSpreadDto> sut = new Collection<ResourceSpreadDto>();
            DateTime start = Convert.ToDateTime("09/2010");
            DateTime end = Convert.ToDateTime("12/2013");
            SpreadCurves CurveID = SpreadCurves.SpreadCurve11;
            long HoursSpread = 9000100000;

            WorkspaceDTO workspace = new WorkspaceDTO() { ResourceDecimalPrecision = 0 };
            sut = LaborSpreadCalcs(start, end, HoursSpread, CurveID, workspace);

            Assert.IsNotNull(sut, "The value returned from Spread Curve BL was bad");

            // Make sure that all the values returned sum up to HoursSpread
            long Cumulative = 0;
            for (int x = 0; x < sut.Count(); x++)
            {
                Cumulative = Convert.ToInt64(sut[x].LaborSpreadValue) + Cumulative;
            }
            Assert.IsTrue(Cumulative == HoursSpread, "The return value did not equal HoursSpread");
        }

        /// <summary>
        /// This test case will test Curve Test 12. It will verify that null data was not
        /// returned from the Labor Spread calculation and that the total value returned
        /// equalled the HoursSpread that was inputted
        /// </summary>
        [TestMethod]
        public void CurveTest_12()
        {

			Collection<ResourceSpreadDto> sut = new Collection<ResourceSpreadDto>();
            DateTime start = Convert.ToDateTime("03/2010");
            DateTime end = Convert.ToDateTime("03/2012");
            SpreadCurves CurveID = SpreadCurves.SpreadCurve12;
            long HoursSpread = 9000078009;

            WorkspaceDTO workspace = new WorkspaceDTO() { ResourceDecimalPrecision = 0 };

            sut = LaborSpreadCalcs(start, end, HoursSpread, CurveID, workspace);

            Assert.IsNotNull(sut, "The value returned from Spread Curve BL was bad");

            // Make sure that all the values returned sum up to HoursSpread
            long Cumulative = 0;
            for (int x = 0; x < sut.Count(); x++)
            {
                Cumulative = Convert.ToInt64(sut[x].LaborSpreadValue) + Cumulative;
            }
            Assert.IsTrue(Cumulative == HoursSpread, "The return value did not equal HoursSpread");
        }

        /// <summary>
        /// This test case will test Curve Test 13. It will verify that null data was not
        /// returned from the Labor Spread calculation and that the total value returned
        /// equalled the HoursSpread that was inputted
        /// </summary>
        [TestMethod]
        public void CurveTest_13()
        {

			Collection<ResourceSpreadDto> sut = new Collection<ResourceSpreadDto>();
            DateTime start = Convert.ToDateTime("11/2010");
            DateTime end = Convert.ToDateTime("03/2012");
            SpreadCurves CurveID = SpreadCurves.SpreadCurve13;
            long HoursSpread = 9000062350;

            WorkspaceDTO workspace = new WorkspaceDTO() { ResourceDecimalPrecision = 0 };

            sut = LaborSpreadCalcs(start, end, HoursSpread, CurveID, workspace);

            Assert.IsNotNull(sut, "The value returned from Spread Curve BL was bad");

            // Make sure that all the values returned sum up to HoursSpread
            long Cumulative = 0;
            for (int x = 0; x < sut.Count(); x++)
            {
                Cumulative = Convert.ToInt64(sut[x].LaborSpreadValue) + Cumulative;
            }
            Assert.IsTrue(Cumulative == HoursSpread, "The return value did not equal HoursSpread");
        }

        /// <summary>
        /// This test case will test Curve Test 14. It will verify that null data was not
        /// returned from the Labor Spread calculation and that the total value returned
        /// equalled the HoursSpread that was inputted
        /// </summary>
        [TestMethod]
        public void CurveTest_14()
        {

			Collection<ResourceSpreadDto> sut = new Collection<ResourceSpreadDto>();
            DateTime start = Convert.ToDateTime("01/2010");
            DateTime end = Convert.ToDateTime("03/2012");
            SpreadCurves CurveID = SpreadCurves.SpreadCurve14;
            long HoursSpread = 9000004000;

            WorkspaceDTO workspace = new WorkspaceDTO() { ResourceDecimalPrecision = 0 };

            sut = LaborSpreadCalcs(start, end, HoursSpread, CurveID, workspace);

            Assert.IsNotNull(sut, "The value returned from Spread Curve BL was bad");

            // Make sure that all the values returned sum up to HoursSpread
            long Cumulative = 0;
            for (int x = 0; x < sut.Count(); x++)
            {
                Cumulative = Convert.ToInt64(sut[x].LaborSpreadValue) + Cumulative;
            }
            Assert.IsTrue(Cumulative == HoursSpread, "The return value did not equal HoursSpread");
        }

        /// <summary>
        /// This test case will test Curve Test 15. It will verify that null data was not
        /// returned from the Labor Spread calculation and that the total value returned
        /// equalled the HoursSpread that was inputted
        /// </summary>
        [TestMethod]
        public void CurveTest_15()
        {

			Collection<ResourceSpreadDto> sut = new Collection<ResourceSpreadDto>();
            DateTime start = Convert.ToDateTime("12/2011");
            DateTime end = Convert.ToDateTime("12/2014");
            SpreadCurves CurveID = SpreadCurves.SpreadCurve15;
            long HoursSpread = 9000075600;

            WorkspaceDTO workspace = new WorkspaceDTO() { ResourceDecimalPrecision = 0 };
            sut = LaborSpreadCalcs(start, end, HoursSpread, CurveID, workspace);

            Assert.IsNotNull(sut, "The value returned from Spread Curve BL was bad");

            // Make sure that all the values returned sum up to HoursSpread
            long Cumulative = 0;
            for (int x = 0; x < sut.Count(); x++)
            {
                Cumulative = Convert.ToInt64(sut[x].LaborSpreadValue) + Cumulative;
            }
            Assert.IsTrue(Cumulative == HoursSpread, "The return value did not equal HoursSpread");
        }

        /// <summary>
        /// This test case will test Curve Test 16. It will verify that null data was not
        /// returned from the Labor Spread calculation and that the total value returned
        /// equalled the HoursSpread that was inputted
        /// </summary>
        [TestMethod]
        public void CurveTest_16()
        {

			Collection<ResourceSpreadDto> sut = new Collection<ResourceSpreadDto>();
            DateTime start = Convert.ToDateTime("09/2010");
            DateTime end = Convert.ToDateTime("03/2012");
            SpreadCurves CurveID = SpreadCurves.SpreadCurve16;
            long HoursSpread = 9000004650;

            WorkspaceDTO workspace = new WorkspaceDTO() { ResourceDecimalPrecision = 0 };

            sut = LaborSpreadCalcs(start, end, HoursSpread, CurveID, workspace);

            Assert.IsNotNull(sut, "The value returned from Spread Curve BL was bad");

            // Make sure that all the values returned sum up to HoursSpread
            long Cumulative = 0;
            for (int x = 0; x < sut.Count(); x++)
            {
                Cumulative = Convert.ToInt64(sut[x].LaborSpreadValue) + Cumulative;
            }
            Assert.IsTrue(Cumulative == HoursSpread, "The return value did not equal HoursSpread");
        }

        /// <summary>
        /// This test case will test Curve Test 17. It will verify that null data was not
        /// returned from the Labor Spread calculation and that the total value returned
        /// equalled the HoursSpread that was inputted
        /// </summary>
        [TestMethod]
        public void CurveTest_17()
        {

			Collection<ResourceSpreadDto> sut = new Collection<ResourceSpreadDto>();
            DateTime start = Convert.ToDateTime("09/2010");
            DateTime end = Convert.ToDateTime("03/2012");
            SpreadCurves CurveID = SpreadCurves.SpreadCurve17;
            long HoursSpread = 9000004650;

            WorkspaceDTO workspace = new WorkspaceDTO() { ResourceDecimalPrecision = 0 };

            sut = LaborSpreadCalcs(start, end, HoursSpread, CurveID, workspace);

            Assert.IsNotNull(sut, "The value returned from Spread Curve BL was bad");

            // Make sure that all the values returned sum up to HoursSpread
            long Cumulative = 0;
            for (int x = 0; x < sut.Count(); x++)
            {
                Cumulative = Convert.ToInt64(sut[x].LaborSpreadValue) + Cumulative;
            }
            Assert.IsTrue(Cumulative == HoursSpread, "The return value did not equal HoursSpread");
        }
        /// <summary>
        /// This test case will test Curve Test 18. It will verify that null data was not
        /// returned from the Labor Spread calculation and that the total value returned
        /// equalled the HoursSpread that was inputted
        /// </summary>
        [TestMethod]
        public void CurveTest_18()
        {

			Collection<ResourceSpreadDto> sut = new Collection<ResourceSpreadDto>();
            DateTime start = Convert.ToDateTime("09/2010");
            DateTime end = Convert.ToDateTime("03/2012");
            SpreadCurves CurveID = SpreadCurves.SpreadCurve18;
            long HoursSpread = 9000004650;

            WorkspaceDTO workspace = new WorkspaceDTO() { ResourceDecimalPrecision = 0 };
            sut = LaborSpreadCalcs(start, end, HoursSpread, CurveID, workspace);

            Assert.IsNotNull(sut, "The value returned from Spread Curve BL was bad");

            // Make sure that all the values returned sum up to HoursSpread
            long Cumulative = 0;
            for (int x = 0; x < sut.Count(); x++)
            {
                Cumulative = Convert.ToInt64(sut[x].LaborSpreadValue) + Cumulative;
            }
            Assert.IsTrue(Cumulative == HoursSpread, "The return value did not equal HoursSpread");
        }

        /// <summary>
        /// This test case will test Curve Test 19. It will verify that null data was not
        /// returned from the Labor Spread calculation and that the total value returned
        /// equalled the HoursSpread that was inputted
        /// </summary>
        [TestMethod]
        public void CurveTest_19()
        {

			Collection<ResourceSpreadDto> sut = new Collection<ResourceSpreadDto>();
            DateTime start = Convert.ToDateTime("09/2010");
            DateTime end = Convert.ToDateTime("09/2012");
            SpreadCurves CurveID = SpreadCurves.SpreadCurve19;
            long HoursSpread = 9000006540;

            WorkspaceDTO workspace = new WorkspaceDTO() { ResourceDecimalPrecision = 0 };

            sut = LaborSpreadCalcs(start, end, HoursSpread, CurveID, workspace);

            Assert.IsNotNull(sut, "The value returned from Spread Curve BL was bad");

            // Make sure that all the values returned sum up to HoursSpread
            long Cumulative = 0;
            for (int x = 0; x < sut.Count(); x++)
            {
                Cumulative = Convert.ToInt64(sut[x].LaborSpreadValue) + Cumulative;
            }
            Assert.IsTrue(Cumulative == HoursSpread, "The return value did not equal HoursSpread");
        }

        /// <summary>
        /// This test case will test Curve Test 20. It will verify that null data was not
        /// returned from the Labor Spread calculation and that the total value returned
        /// equalled the HoursSpread that was inputted
        /// </summary>
        [TestMethod]
        public void CurveTest_20()
        {

			Collection<ResourceSpreadDto> sut = new Collection<ResourceSpreadDto>();
            DateTime start = Convert.ToDateTime("07/2010");
            DateTime end = Convert.ToDateTime("07/2012");
            SpreadCurves CurveID = SpreadCurves.SpreadCurve20;
            long HoursSpread = 9000007000;

            WorkspaceDTO workspace = new WorkspaceDTO() { ResourceDecimalPrecision = 0 };
            sut = LaborSpreadCalcs(start, end, HoursSpread, CurveID, workspace);

            Assert.IsNotNull(sut, "The value returned from Spread Curve BL was bad");

            // Make sure that all the values returned sum up to HoursSpread
            long Cumulative = 0;
            for (int x = 0; x < sut.Count(); x++)
            {
                Cumulative = Convert.ToInt64(sut[x].LaborSpreadValue) + Cumulative;
            }
            Assert.IsTrue(Cumulative == HoursSpread, "The return value did not equal HoursSpread");
        }

        /// <summary>
        /// This test case will test Curve Test 21. It will verify that null data was not
        /// returned from the Labor Spread calculation and that the total value returned
        /// equalled the HoursSpread that was inputted
        /// </summary>
        [TestMethod]
        public void CurveTest_21()
        {

			Collection<ResourceSpreadDto> sut = new Collection<ResourceSpreadDto>();
            DateTime start = Convert.ToDateTime("12/2010");
            DateTime end = Convert.ToDateTime("11/2011");
            SpreadCurves CurveID = SpreadCurves.SpreadCurve21;
            long HoursSpread = 9000072560;

            WorkspaceDTO workspace = new WorkspaceDTO() { ResourceDecimalPrecision = 0 };

            sut = LaborSpreadCalcs(start, end, HoursSpread, CurveID, workspace);

            Assert.IsNotNull(sut, "The value returned from Spread Curve BL was bad");

            // Make sure that all the values returned sum up to HoursSpread
            long Cumulative = 0;
            for (int x = 0; x < sut.Count(); x++)
            {
                Cumulative = Convert.ToInt64(sut[x].LaborSpreadValue) + Cumulative;
            }
            Assert.IsTrue(Cumulative == HoursSpread, "The return value did not equal HoursSpread");
        }

        /// <summary>
        /// This test case will test Curve Test 22. It will verify that null data was not
        /// returned from the Labor Spread calculation and that the total value returned
        /// equalled the HoursSpread that was inputted
        /// </summary>
        [TestMethod]
        public void CurveTest_22()
        {

			Collection<ResourceSpreadDto> sut = new Collection<ResourceSpreadDto>();
            DateTime start = Convert.ToDateTime("09/2010");
            DateTime end = Convert.ToDateTime("03/2012");
            SpreadCurves CurveID = SpreadCurves.SpreadCurve22;
            long HoursSpread = 9000758034;

            WorkspaceDTO workspace = new WorkspaceDTO() { ResourceDecimalPrecision = 0 };
            sut = LaborSpreadCalcs(start, end, HoursSpread, CurveID, workspace);

            Assert.IsNotNull(sut, "The value returned from Spread Curve BL was bad");

            // Make sure that all the values returned sum up to HoursSpread
            long Cumulative = 0;
            for (int x = 0; x < sut.Count(); x++)
            {
                Cumulative = Convert.ToInt64(sut[x].LaborSpreadValue) + Cumulative;
            }
            Assert.IsTrue(Cumulative == HoursSpread, "The return value did not equal HoursSpread");
        }

        /// <summary>
        /// This test case will test Curve Test 23. It will verify that null data was not
        /// returned from the Labor Spread calculation and that the total value returned
        /// equalled the HoursSpread that was inputted
        /// </summary>
        [TestMethod]
        public void CurveTest_23()
        {

			Collection<ResourceSpreadDto> sut = new Collection<ResourceSpreadDto>();
            DateTime start = Convert.ToDateTime("09/2010");
            DateTime end = Convert.ToDateTime("09/2015");
            SpreadCurves CurveID = SpreadCurves.SpreadCurve23;
            long HoursSpread = 9000420000;

            WorkspaceDTO workspace = new WorkspaceDTO() { ResourceDecimalPrecision = 0 };
            sut = LaborSpreadCalcs(start, end, HoursSpread, CurveID, workspace);

            Assert.IsNotNull(sut, "The value returned from Spread Curve BL was bad");

            // Make sure that all the values returned sum up to HoursSpread
            long Cumulative = 0;
            for (int x = 0; x < sut.Count(); x++)
            {
                Cumulative = Convert.ToInt64(sut[x].LaborSpreadValue) + Cumulative;
            }
            Assert.IsTrue(Cumulative == HoursSpread, "The return value did not equal HoursSpread");
        }

        /// <summary>
        /// This test case will test Curve Test 24. It will verify that null data was not
        /// returned from the Labor Spread calculation and that the total value returned
        /// equalled the HoursSpread that was inputted
        /// </summary>
        [TestMethod]
        public void CurveTest_24()
        {

			Collection<ResourceSpreadDto> sut = new Collection<ResourceSpreadDto>();
            DateTime start = Convert.ToDateTime("09/2010");
            DateTime end = Convert.ToDateTime("03/2012");
            SpreadCurves CurveID = SpreadCurves.SpreadCurve24;
            long HoursSpread = 9000004650;

            WorkspaceDTO workspace = new WorkspaceDTO() { ResourceDecimalPrecision = 0 };
            sut = LaborSpreadCalcs(start, end, HoursSpread, CurveID, workspace);

            Assert.IsNotNull(sut, "The value returned from Spread Curve BL was bad");

            // Make sure that all the values returned sum up to HoursSpread
            long Cumulative = 0;
            for (int x = 0; x < sut.Count(); x++)
            {
                Cumulative = Convert.ToInt64(sut[x].LaborSpreadValue) + Cumulative;
            }
            Assert.IsTrue(Cumulative == HoursSpread, "The return value did not equal HoursSpread");
        }

        /// <summary>
        /// This test case will test Curve Test 25. It will verify that null data was not
        /// returned from the Labor Spread calculation and that the total value returned
        /// equalled the HoursSpread that was inputted
        /// </summary>
        [TestMethod]
        public void CurveTest_25()
        {

			Collection<ResourceSpreadDto> sut = new Collection<ResourceSpreadDto>();
            DateTime start = Convert.ToDateTime("01/2011");
            DateTime end = Convert.ToDateTime("03/2012");
            SpreadCurves CurveID = SpreadCurves.SpreadCurve25;
            long HoursSpread = 9000062000;

            WorkspaceDTO workspace = new WorkspaceDTO() { ResourceDecimalPrecision = 0 };
            sut = LaborSpreadCalcs(start, end, HoursSpread, CurveID, workspace);

            Assert.IsNotNull(sut, "The value returned from Spread Curve BL was bad");

            // Make sure that all the values returned sum up to HoursSpread
            long Cumulative = 0;
            for (int x = 0; x < sut.Count(); x++)
            {
                Cumulative = Convert.ToInt64(sut[x].LaborSpreadValue) + Cumulative;
            }
            Assert.IsTrue(Cumulative == HoursSpread, "The return value did not equal HoursSpread");
        }

        /// <summary>
        /// This test case will test Curve Test 26. It will verify that null data was not
        /// returned from the Labor Spread calculation and that the total value returned
        /// equalled the HoursSpread that was inputted
        /// </summary>
        [TestMethod]
        public void CurveTest_26()
        {

			Collection<ResourceSpreadDto> sut = new Collection<ResourceSpreadDto>();
            DateTime start = Convert.ToDateTime("10/2010");
            DateTime end = Convert.ToDateTime("04/2012");
            SpreadCurves CurveID = SpreadCurves.SpreadCurve26;
            long HoursSpread = 9000004650;

            WorkspaceDTO workspace = new WorkspaceDTO() { ResourceDecimalPrecision = 0 };
            sut = LaborSpreadCalcs(start, end, HoursSpread, CurveID, workspace);

            Assert.IsNotNull(sut, "The value returned from Spread Curve BL was bad");

            // Make sure that all the values returned sum up to HoursSpread
            long Cumulative = 0;
            for (int x = 0; x < sut.Count(); x++)
            {
                Cumulative = Convert.ToInt64(sut[x].LaborSpreadValue) + Cumulative;
            }
            Assert.IsTrue(Cumulative == HoursSpread, "The return value did not equal HoursSpread");
        }

        /// <summary>
        /// This test case will test Curve Test 27. It will verify that null data was not
        /// returned from the Labor Spread calculation and that the total value returned
        /// equalled the HoursSpread that was inputted
        /// </summary>
        [TestMethod]
        public void CurveTest_27()
        {

			Collection<ResourceSpreadDto> sut = new Collection<ResourceSpreadDto>();
            DateTime start = Convert.ToDateTime("11/2010");
            DateTime end = Convert.ToDateTime("12/2012");
            SpreadCurves CurveID = SpreadCurves.SpreadCurve27;
            long HoursSpread = 9000465000;

            WorkspaceDTO workspace = new WorkspaceDTO() { ResourceDecimalPrecision = 0 };
            sut = LaborSpreadCalcs(start, end, HoursSpread, CurveID, workspace);

            Assert.IsNotNull(sut, "The value returned from Spread Curve BL was bad");

            // Make sure that all the values returned sum up to HoursSpread
            long Cumulative = 0;
            for (int x = 0; x < sut.Count(); x++)
            {
                Cumulative = Convert.ToInt64(sut[x].LaborSpreadValue) + Cumulative;
            }
            Assert.IsTrue(Cumulative == HoursSpread, "The return value did not equal HoursSpread");
        }

        /// <summary>
        /// This test case will test Curve Test 28. It will verify that null data was not
        /// returned from the Labor Spread calculation and that the total value returned
        /// equalled the HoursSpread that was inputted
        /// </summary>
        [TestMethod]
        public void CurveTest_28()
        {

			Collection<ResourceSpreadDto> sut = new Collection<ResourceSpreadDto>();
            DateTime start = Convert.ToDateTime("09/2010");
            DateTime end = Convert.ToDateTime("03/2012");
            SpreadCurves CurveID = SpreadCurves.SpreadCurve28;
            long HoursSpread = 9000078650;

            WorkspaceDTO workspace = new WorkspaceDTO() { ResourceDecimalPrecision = 0 };
            sut = LaborSpreadCalcs(start, end, HoursSpread, CurveID, workspace);

            Assert.IsNotNull(sut, "The value returned from Spread Curve BL was bad");

            // Make sure that all the values returned sum up to HoursSpread
            long Cumulative = 0;
            for (int x = 0; x < sut.Count(); x++)
            {
                Cumulative = Convert.ToInt64(sut[x].LaborSpreadValue) + Cumulative;
            }
            Assert.IsTrue(Cumulative == HoursSpread, "The return value did not equal HoursSpread");
        }

        /// <summary>
        /// This test case will test Curve Test 29. It will verify that null data was not
        /// returned from the Labor Spread calculation and that the total value returned
        /// equalled the HoursSpread that was inputted
        /// </summary>
        [TestMethod]
        public void CurveTest_29()
        {

			Collection<ResourceSpreadDto> sut = new Collection<ResourceSpreadDto>();
            DateTime start = Convert.ToDateTime("09/2010");
            DateTime end = Convert.ToDateTime("03/2013");
            SpreadCurves CurveID = SpreadCurves.SpreadCurve29;
            long HoursSpread = 9000088888;

            WorkspaceDTO workspace = new WorkspaceDTO() { ResourceDecimalPrecision = 0 };
            sut = LaborSpreadCalcs(start, end, HoursSpread, CurveID, workspace);

            Assert.IsNotNull(sut, "The value returned from Spread Curve BL was bad");

            // Make sure that all the values returned sum up to HoursSpread
            long Cumulative = 0;
            for (int x = 0; x < sut.Count(); x++)
            {
                Cumulative = Convert.ToInt64(sut[x].LaborSpreadValue) + Cumulative;
            }
            Assert.IsTrue(Cumulative == HoursSpread, "The return value did not equal HoursSpread");
        }
        /// <summary>
        /// This test case will test Curve Test 30. It will verify that null data was not
        /// returned from the Labor Spread calculation and that the total value returned
        /// equalled the HoursSpread that was inputted
        /// </summary>
        [TestMethod]
        public void CurveTest_30()
        {

			Collection<ResourceSpreadDto> sut = new Collection<ResourceSpreadDto>();
            DateTime start = Convert.ToDateTime("09/2010");
            DateTime end = Convert.ToDateTime("03/2012");
            SpreadCurves CurveID = SpreadCurves.SpreadCurve30;
            long HoursSpread = 9000004650;

            WorkspaceDTO workspace = new WorkspaceDTO() { ResourceDecimalPrecision = 0 }; 
            sut = LaborSpreadCalcs(start, end, HoursSpread, CurveID, workspace);

            Assert.IsNotNull(sut, "The value returned from Spread Curve BL was bad");

            // Make sure that all the values returned sum up to HoursSpread
            long Cumulative = 0;
            for (int x = 0; x < sut.Count(); x++)
            {
                Cumulative = Convert.ToInt64(sut[x].LaborSpreadValue) + Cumulative;
            }
            Assert.IsTrue(Cumulative == HoursSpread, "The return value did not equal HoursSpread");
        }
        /// <summary>
        /// This test case will test Curve Test 31. It will verify that null data was not
        /// returned from the Labor Spread calculation and that the total value returned
        /// equalled the HoursSpread that was inputted
        /// </summary>
        [TestMethod]
        public void CurveTest_31()
        {

			Collection<ResourceSpreadDto> sut = new Collection<ResourceSpreadDto>();
            DateTime start = Convert.ToDateTime("07/2011");
            DateTime end = Convert.ToDateTime("03/2012");
            SpreadCurves CurveID = SpreadCurves.SpreadCurve31;
            long HoursSpread = 9000065000;

            WorkspaceDTO workspace = new WorkspaceDTO() { ResourceDecimalPrecision = 0 };
            sut = LaborSpreadCalcs(start, end, HoursSpread, CurveID, workspace);

            Assert.IsNotNull(sut, "The value returned from Spread Curve BL was bad");

            // Make sure that all the values returned sum up to HoursSpread
            long Cumulative = 0;
            for (int x = 0; x < sut.Count(); x++)
            {
                Cumulative = Convert.ToInt64(sut[x].LaborSpreadValue) + Cumulative;
            }
            Assert.IsTrue(Cumulative == HoursSpread, "The return value did not equal HoursSpread");
        }
        /// <summary>
        /// This test case will test Curve Test 32. It will verify that null data was not
        /// returned from the Labor Spread calculation and that the total value returned
        /// equalled the HoursSpread that was inputted
        /// </summary>
        [TestMethod]
        public void CurveTest_32()
        {

			Collection<ResourceSpreadDto> sut = new Collection<ResourceSpreadDto>();
            DateTime start = Convert.ToDateTime("06/2010");
            DateTime end = Convert.ToDateTime("06/2012");
            SpreadCurves CurveID = SpreadCurves.SpreadCurve32;
            long HoursSpread = 9000066666;

            WorkspaceDTO workspace = new WorkspaceDTO() { ResourceDecimalPrecision = 0 };
            sut = LaborSpreadCalcs(start, end, HoursSpread, CurveID, workspace);

            Assert.IsNotNull(sut, "The value returned from Spread Curve BL was bad");

            // Make sure that all the values returned sum up to HoursSpread
            long Cumulative = 0;
            for (int x = 0; x < sut.Count(); x++)
            {
                Cumulative = Convert.ToInt64(sut[x].LaborSpreadValue) + Cumulative;
            }
            Assert.IsTrue(Cumulative == HoursSpread, "The return value did not equal HoursSpread");
        }
        /// <summary>
        /// This test case will test Curve Test 33. It will verify that null data was not
        /// returned from the Labor Spread calculation and that the total value returned
        /// equalled the HoursSpread that was inputted
        /// </summary>
        [TestMethod]
        public void CurveTest_33()
        {

			Collection<ResourceSpreadDto> sut = new Collection<ResourceSpreadDto>();
            DateTime start = Convert.ToDateTime("03/2010");
            DateTime end = Convert.ToDateTime("05/2012");
            SpreadCurves CurveID = SpreadCurves.SpreadCurve33;
            long HoursSpread = 9000000800;

            WorkspaceDTO workspace = new WorkspaceDTO() { ResourceDecimalPrecision = 0 };
            sut = LaborSpreadCalcs(start, end, HoursSpread, CurveID, workspace);

            Assert.IsNotNull(sut, "The value returned from Spread Curve BL was bad");

            // Make sure that all the values returned sum up to HoursSpread
            long Cumulative = 0;
            for (int x = 0; x < sut.Count(); x++)
            {
                Cumulative = Convert.ToInt64(sut[x].LaborSpreadValue) + Cumulative;
            }
            Assert.IsTrue(Cumulative == HoursSpread, "The return value did not equal HoursSpread");
        }

        /// <summary>
        /// This test case will test Curve Test 34. It will verify that null data was not
        /// returned from the Labor Spread calculation and that the total value returned
        /// equalled the HoursSpread that was inputted
        /// </summary>
        [TestMethod]
        public void CurveTest_34()
        {

			Collection<ResourceSpreadDto> sut = new Collection<ResourceSpreadDto>();
            DateTime start = Convert.ToDateTime("09/2010");
            DateTime end = Convert.ToDateTime("03/2012");
            SpreadCurves CurveID = SpreadCurves.SpreadCurve34;
            long HoursSpread = 9000004650;

            WorkspaceDTO workspace = new WorkspaceDTO() { ResourceDecimalPrecision = 0 };
            sut = LaborSpreadCalcs(start, end, HoursSpread, CurveID, workspace);

            Assert.IsNotNull(sut, "The value returned from Spread Curve BL was bad");

            // Make sure that all the values returned sum up to HoursSpread
            long Cumulative = 0;
            for (int x = 0; x < sut.Count(); x++)
            {
                Cumulative = Convert.ToInt64(sut[x].LaborSpreadValue) + Cumulative;
            }
            Assert.IsTrue(Cumulative == HoursSpread, "The return value did not equal HoursSpread");
        }
        /// <summary>
        /// This test case will test Curve Test 35. It will verify that null data was not
        /// returned from the Labor Spread calculation and that the total value returned
        /// equalled the HoursSpread that was inputted
        /// </summary>
        [TestMethod]
        public void CurveTest_35()
        {

			Collection<ResourceSpreadDto> sut = new Collection<ResourceSpreadDto>();
            DateTime start = Convert.ToDateTime("11/2010");
            DateTime end = Convert.ToDateTime("03/2011");
            SpreadCurves CurveID = SpreadCurves.SpreadCurve35;
            long HoursSpread = 9000045685;

            WorkspaceDTO workspace = new WorkspaceDTO() { ResourceDecimalPrecision = 0 };
            sut = LaborSpreadCalcs(start, end, HoursSpread, CurveID, workspace);

            Assert.IsNotNull(sut, "The value returned from Spread Curve BL was bad");

            // Make sure that all the values returned sum up to HoursSpread
            long Cumulative = 0;
            for (int x = 0; x < sut.Count(); x++)
            {
                Cumulative = Convert.ToInt64(sut[x].LaborSpreadValue) + Cumulative;
            }
            Assert.IsTrue(Cumulative == HoursSpread, "The return value did not equal HoursSpread");
        }
        /// <summary>
        /// This test case will test Curve Test 36. It will verify that null data was not
        /// returned from the Labor Spread calculation and that the total value returned
        /// equalled the HoursSpread that was inputted
        /// </summary>
        [TestMethod]
        public void CurveTest_36()
        {

			Collection<ResourceSpreadDto> sut = new Collection<ResourceSpreadDto>();
            DateTime start = Convert.ToDateTime("09/2010");
            DateTime end = Convert.ToDateTime("03/2012");
            SpreadCurves CurveID = SpreadCurves.SpreadCurve36;
            long HoursSpread = 9000000100;

            WorkspaceDTO workspace = new WorkspaceDTO() { ResourceDecimalPrecision = 0 };
            sut = LaborSpreadCalcs(start, end, HoursSpread, CurveID, workspace);

            Assert.IsNotNull(sut, "The value returned from Spread Curve BL was bad");

            // Make sure that all the values returned sum up to HoursSpread
            long Cumulative = 0;
            for (int x = 0; x < sut.Count(); x++)
            {
                Cumulative = Convert.ToInt64(sut[x].LaborSpreadValue) + Cumulative;
            }
            Assert.IsTrue(Cumulative == HoursSpread, "The return value did not equal HoursSpread");
        }
        /// <summary>
        /// This test case will test Curve Test 37. It will verify that null data was not
        /// returned from the Labor Spread calculation and that the total value returned
        /// equalled the HoursSpread that was inputted
        /// </summary>
        [TestMethod]
        public void CurveTest_37()
        {

			Collection<ResourceSpreadDto> sut = new Collection<ResourceSpreadDto>();
            DateTime start = Convert.ToDateTime("09/2010");
            DateTime end = Convert.ToDateTime("09/2012");
            SpreadCurves CurveID = SpreadCurves.SpreadCurve37;
            long HoursSpread = 9000206798;

            WorkspaceDTO workspace = new WorkspaceDTO() { ResourceDecimalPrecision = 0 };
            sut = LaborSpreadCalcs(start, end, HoursSpread, CurveID, workspace);

            Assert.IsNotNull(sut, "The value returned from Spread Curve BL was bad");

            // Make sure that all the values returned sum up to HoursSpread
            long Cumulative = 0;
            for (int x = 0; x < sut.Count(); x++)
            {
                Cumulative = Convert.ToInt64(sut[x].LaborSpreadValue) + Cumulative;
            }
            Assert.IsTrue(Cumulative == HoursSpread, "The return value did not equal HoursSpread");
        }
        /// <summary>
        /// This test case will test Curve Test 38. It will verify that null data was not
        /// returned from the Labor Spread calculation and that the total value returned
        /// equalled the HoursSpread that was inputted
        /// </summary>
        [TestMethod]
        public void CurveTest_38()
        {

			Collection<ResourceSpreadDto> sut = new Collection<ResourceSpreadDto>();
            DateTime start = Convert.ToDateTime("09/2010");
            DateTime end = Convert.ToDateTime("03/2011");
            SpreadCurves CurveID = SpreadCurves.SpreadCurve38;
            long HoursSpread = 9000006650;

            WorkspaceDTO workspace = new WorkspaceDTO() { ResourceDecimalPrecision = 0 };
            sut = LaborSpreadCalcs(start, end, HoursSpread, CurveID, workspace);

            Assert.IsNotNull(sut, "The value returned from Spread Curve BL was bad");

            // Make sure that all the values returned sum up to HoursSpread
            long Cumulative = 0;
            for (int x = 0; x < sut.Count(); x++)
            {
                Cumulative = Convert.ToInt64(sut[x].LaborSpreadValue) + Cumulative;
            }
            Assert.IsTrue(Cumulative == HoursSpread, "The return value did not equal HoursSpread");
        }
        /// <summary>
        /// This test case will test Curve Test 39. It will verify that null data was not
        /// returned from the Labor Spread calculation and that the total value returned
        /// equalled the HoursSpread that was inputted
        /// </summary>
        [TestMethod]
        public void CurveTest_39()
        {

			Collection<ResourceSpreadDto> sut = new Collection<ResourceSpreadDto>();
            DateTime start = Convert.ToDateTime("12/2010");
            DateTime end = Convert.ToDateTime("12/2012");
            SpreadCurves CurveID = SpreadCurves.SpreadCurve39;
            long HoursSpread = 9000012000;

            WorkspaceDTO workspace = new WorkspaceDTO() { ResourceDecimalPrecision = 0 };
            sut = LaborSpreadCalcs(start, end, HoursSpread, CurveID, workspace);

            Assert.IsNotNull(sut, "The value returned from Spread Curve BL was bad");

            // Make sure that all the values returned sum up to HoursSpread
            long Cumulative = 0;
            for (int x = 0; x < sut.Count(); x++)
            {
                Cumulative = Convert.ToInt64(sut[x].LaborSpreadValue) + Cumulative;
            }
            Assert.IsTrue(Cumulative == HoursSpread, "The return value did not equal HoursSpread");
        }

        /// <summary>
        /// This test case will test Curve Test 40. It will verify that null data was not
        /// returned from the Labor Spread calculation and that the total value returned
        /// equalled the HoursSpread that was inputted
        /// </summary>
        [TestMethod]
        public void CurveTest_40()
        {

			Collection<ResourceSpreadDto> sut = new Collection<ResourceSpreadDto>();
            DateTime start = Convert.ToDateTime("09/2010");
            DateTime end = Convert.ToDateTime("03/2012");
            SpreadCurves CurveID = SpreadCurves.SpreadCurve40;
            long HoursSpread = 9000004650;

            WorkspaceDTO workspace = new WorkspaceDTO() { ResourceDecimalPrecision = 0 };
            sut = LaborSpreadCalcs(start, end, HoursSpread, CurveID, workspace);

            Assert.IsNotNull(sut, "The value returned from Spread Curve BL was bad");

            // Make sure that all the values returned sum up to HoursSpread
            long Cumulative = 0;
            for (int x = 0; x < sut.Count(); x++)
            {
                Cumulative = Convert.ToInt64(sut[x].LaborSpreadValue) + Cumulative;
            }
            Assert.IsTrue(Cumulative == HoursSpread, "The return value did not equal HoursSpread");
        }

        /// <summary>
        /// This test case will test Curve Test 41. It will verify that null data was not
        /// returned from the Labor Spread calculation and that the total value returned
        /// equalled the HoursSpread that was inputted
        /// </summary>
        [TestMethod]
        public void CurveTest_41()
        {

			Collection<ResourceSpreadDto> sut = new Collection<ResourceSpreadDto>();
            DateTime start = Convert.ToDateTime("01/2011");
            DateTime end = Convert.ToDateTime("03/2012");
            SpreadCurves CurveID = SpreadCurves.SpreadCurve41;
            long HoursSpread = 9000150000;

            WorkspaceDTO workspace = new WorkspaceDTO() { ResourceDecimalPrecision = 0 };
            sut = LaborSpreadCalcs(start, end, HoursSpread, CurveID, workspace);

            Assert.IsNotNull(sut, "The value returned from Spread Curve BL was bad");

            // Make sure that all the values returned sum up to HoursSpread
            long Cumulative = 0;
            for (int x = 0; x < sut.Count(); x++)
            {
                Cumulative = Convert.ToInt64(sut[x].LaborSpreadValue) + Cumulative;
            }
            Assert.IsTrue(Cumulative == HoursSpread, "The return value did not equal HoursSpread");
        }

        /// <summary>
        /// This test case will test Curve Test 42. It will verify that null data was not
        /// returned from the Labor Spread calculation and that the total value returned
        /// equalled the HoursSpread that was inputted
        /// </summary>
        [TestMethod]
        public void CurveTest_42()
        {

			Collection<ResourceSpreadDto> sut = new Collection<ResourceSpreadDto>();
            DateTime start = Convert.ToDateTime("02/2010");
            DateTime end = Convert.ToDateTime("02/2012");
            SpreadCurves CurveID = SpreadCurves.SpreadCurve42;
            long HoursSpread = 9000024000;

            WorkspaceDTO workspace = new WorkspaceDTO() { ResourceDecimalPrecision = 0 };
            sut = LaborSpreadCalcs(start, end, HoursSpread, CurveID, workspace);

            Assert.IsNotNull(sut, "The value returned from Spread Curve BL was bad");

            // Make sure that all the values returned sum up to HoursSpread
            long Cumulative = 0;
            for (int x = 0; x < sut.Count(); x++)
            {
                Cumulative = Convert.ToInt64(sut[x].LaborSpreadValue) + Cumulative;
            }
            Assert.IsTrue(Cumulative == HoursSpread, "The return value did not equal HoursSpread");
        }

        /// <summary>
        /// This test case will test Curve Test 43. It will verify that null data was not
        /// returned from the Labor Spread calculation and that the total value returned
        /// equalled the HoursSpread that was inputted
        /// </summary>
        [TestMethod]
        public void CurveTest_43()
        {

			Collection<ResourceSpreadDto> sut = new Collection<ResourceSpreadDto>();
            DateTime start = Convert.ToDateTime("09/2010");
            DateTime end = Convert.ToDateTime("08/2012");
            SpreadCurves CurveID = SpreadCurves.SpreadCurve43;
            long HoursSpread = 9000079850;

            WorkspaceDTO workspace = new WorkspaceDTO() { ResourceDecimalPrecision = 0 };
            sut = LaborSpreadCalcs(start, end, HoursSpread, CurveID, workspace);

            Assert.IsNotNull(sut, "The value returned from Spread Curve BL was bad");

            // Make sure that all the values returned sum up to HoursSpread
            long Cumulative = 0;
            for (int x = 0; x < sut.Count(); x++)
            {
                Cumulative = Convert.ToInt64(sut[x].LaborSpreadValue) + Cumulative;
            }
            Assert.IsTrue(Cumulative == HoursSpread, "The return value did not equal HoursSpread");
        }

        /// <summary>
        /// This test case will test Curve Test 44. It will verify that null data was not
        /// returned from the Labor Spread calculation and that the total value returned
        /// equalled the HoursSpread that was inputted
        /// </summary>
        [TestMethod]
        public void CurveTest_44()
        {

			Collection<ResourceSpreadDto> sut = new Collection<ResourceSpreadDto>();
            DateTime start = Convert.ToDateTime("04/2010");
            DateTime end = Convert.ToDateTime("03/2012");
            SpreadCurves CurveID = SpreadCurves.SpreadCurve44;
            long HoursSpread = 9000087900;

            WorkspaceDTO workspace = new WorkspaceDTO() { ResourceDecimalPrecision = 0 };
            sut = LaborSpreadCalcs(start, end, HoursSpread, CurveID, workspace);

            Assert.IsNotNull(sut, "The value returned from Spread Curve BL was bad");

            // Make sure that all the values returned sum up to HoursSpread
            long Cumulative = 0;
            for (int x = 0; x < sut.Count(); x++)
            {
                Cumulative = Convert.ToInt64(sut[x].LaborSpreadValue) + Cumulative;
            }
            Assert.IsTrue(Cumulative == HoursSpread, "The return value did not equal HoursSpread");
        }

        /// <summary>
        /// This test case will test Curve Test 45. It will verify that null data was not
        /// returned from the Labor Spread calculation and that the total value returned
        /// equalled the HoursSpread that was inputted
        /// </summary>
        [TestMethod]
        public void CurveTest_45()
        {

			Collection<ResourceSpreadDto> sut = new Collection<ResourceSpreadDto>();
            DateTime start = Convert.ToDateTime("09/2010");
            DateTime end = Convert.ToDateTime("04/2012"); // should produce 20 months
            SpreadCurves CurveID = SpreadCurves.SpreadCurve45;
            long HoursSpread = 9000004650;

            WorkspaceDTO workspace = new WorkspaceDTO() { ResourceDecimalPrecision = 0 };
            sut = LaborSpreadCalcs(start, end, HoursSpread, CurveID, workspace);

            Assert.IsNotNull(sut, "The value returned from Spread Curve BL was bad");

            // Make sure that all the values returned sum up to HoursSpread
            long Cumulative = 0;
            for (int x = 0; x < sut.Count(); x++)
            {
                Cumulative = Convert.ToInt64(sut[x].LaborSpreadValue) + Cumulative;
            }
            Assert.IsTrue(Cumulative == HoursSpread, "The return value did not equal HoursSpread");
        }

        /// <summary>
        /// This test case will test Curve Test 46. It will verify that null data was not
        /// returned from the Labor Spread calculation and that the total value returned
        /// equalled the HoursSpread that was inputted
        /// </summary>
        [TestMethod]
        public void CurveTest_46()
        {

			Collection<ResourceSpreadDto> sut = new Collection<ResourceSpreadDto>();
            DateTime start = Convert.ToDateTime("09/2010");
            DateTime end = Convert.ToDateTime("04/2012"); // should produce 20 months
            SpreadCurves CurveID = SpreadCurves.SpreadCurve46;
            long HoursSpread = 9000010000;

            WorkspaceDTO workspace = new WorkspaceDTO() { ResourceDecimalPrecision = 0 };
            sut = LaborSpreadCalcs(start, end, HoursSpread, CurveID, workspace);

            Assert.IsNotNull(sut, "The value returned from Spread Curve BL was bad");

            // Make sure that all the values returned sum up to HoursSpread
            long Cumulative = 0;
            for (int x = 0; x < sut.Count(); x++)
            {
                Cumulative = Convert.ToInt64(sut[x].LaborSpreadValue) + Cumulative;
            }
            Assert.IsTrue(Cumulative == HoursSpread, "The return value did not equal HoursSpread");
        }
        /// <summary>
        /// This test case will test Curve Test 47. It will verify that null data was not
        /// returned from the Labor Spread calculation and that the total value returned
        /// equalled the HoursSpread that was inputted
        /// </summary>
        [TestMethod]
        public void CurveTest_47()
        {

			Collection<ResourceSpreadDto> sut = new Collection<ResourceSpreadDto>();
            DateTime start = Convert.ToDateTime("11/2010");
            DateTime end = Convert.ToDateTime("11/2011"); // should produce 20 months
            SpreadCurves CurveID = SpreadCurves.SpreadCurve47;
            long HoursSpread = 9000011111;

            WorkspaceDTO workspace = new WorkspaceDTO() { ResourceDecimalPrecision = 0 };
            sut = LaborSpreadCalcs(start, end, HoursSpread, CurveID, workspace);

            Assert.IsNotNull(sut, "The value returned from Spread Curve BL was bad");

            // Make sure that all the values returned sum up to HoursSpread
            long Cumulative = 0;
            for (int x = 0; x < sut.Count(); x++)
            {
                Cumulative = Convert.ToInt64(sut[x].LaborSpreadValue) + Cumulative;
            }
            Assert.IsTrue(Cumulative == HoursSpread, "The return value did not equal HoursSpread");
        }

        /// <summary>
        /// This test case will test Curve Test 48. It will verify that null data was not
        /// returned from the Labor Spread calculation and that the total value returned
        /// equalled the HoursSpread that was inputted
        /// </summary>
        [TestMethod]
        public void CurveTest_48()
        {

			Collection<ResourceSpreadDto> sut = new Collection<ResourceSpreadDto>();
            DateTime start = Convert.ToDateTime("04/2008");
            DateTime end = Convert.ToDateTime("04/2012"); // should produce 20 months
            SpreadCurves CurveID = SpreadCurves.SpreadCurve48;
            long HoursSpread = 9000800000;

            WorkspaceDTO workspace = new WorkspaceDTO() { ResourceDecimalPrecision = 0 };
            sut = LaborSpreadCalcs(start, end, HoursSpread, CurveID, workspace);

            Assert.IsNotNull(sut, "The value returned from Spread Curve BL was bad");

            // Make sure that all the values returned sum up to HoursSpread
            long Cumulative = 0;
            for (int x = 0; x < sut.Count(); x++)
            {
                Cumulative = Convert.ToInt64(sut[x].LaborSpreadValue) + Cumulative;
            }
            Assert.IsTrue(Cumulative == HoursSpread, "The return value did not equal HoursSpread");
        }

        /// <summary>
        /// This test case will test Curve Test 49. It will verify that null data was not
        /// returned from the Labor Spread calculation and that the total value returned
        /// equalled the HoursSpread that was inputted
        /// </summary>
        [TestMethod]
        public void CurveTest_49()
        {

			Collection<ResourceSpreadDto> sut = new Collection<ResourceSpreadDto>();
            DateTime start = Convert.ToDateTime("09/2010");
            DateTime end = Convert.ToDateTime("09/2013"); // should produce 20 months
            SpreadCurves CurveID = SpreadCurves.SpreadCurve49;
            long HoursSpread = 9000050000;

            WorkspaceDTO workspace = new WorkspaceDTO() { ResourceDecimalPrecision = 0 };
            sut = LaborSpreadCalcs(start, end, HoursSpread, CurveID, workspace);

            Assert.IsNotNull(sut, "The value returned from Spread Curve BL was bad");

            // Make sure that all the values returned sum up to HoursSpread
            long Cumulative = 0;
            for (int x = 0; x < sut.Count(); x++)
            {
                Cumulative = Convert.ToInt64(sut[x].LaborSpreadValue) + Cumulative;
            }
            Assert.IsTrue(Cumulative == HoursSpread, "The return value did not equal HoursSpread");
        }

        /// <summary>
        /// This test case will test Curve Test 50. It will verify that null data was not
        /// returned from the Labor Spread calculation and that the total value returned
        /// equalled the HoursSpread that was inputted
        /// </summary>
        [TestMethod]
        public void CurveTest_50()
        {

			Collection<ResourceSpreadDto> sut = new Collection<ResourceSpreadDto>();
            DateTime start = Convert.ToDateTime("05/2010");
            DateTime end = Convert.ToDateTime("05/2012"); // should produce 20 months
            SpreadCurves CurveID = SpreadCurves.SpreadCurve50;
            long HoursSpread = 9000005000;

            WorkspaceDTO workspace = new WorkspaceDTO() { ResourceDecimalPrecision = 0 };
            sut = LaborSpreadCalcs(start, end, HoursSpread, CurveID, workspace);

            Assert.IsNotNull(sut, "The value returned from Spread Curve BL was bad");

            // Make sure that all the values returned sum up to HoursSpread
            long Cumulative = 0;
            for (int x = 0; x < sut.Count(); x++)
            {
                Cumulative = Convert.ToInt64(sut[x].LaborSpreadValue) + Cumulative;
            }
            Assert.IsTrue(Cumulative == HoursSpread, "The return value did not equal HoursSpread");
        }

        /// <summary>
        /// This test case will test Curve Test 51. It will verify that null data was not
        /// returned from the Labor Spread calculation and that the total value returned
        /// equalled the HoursSpread that was inputted
        /// </summary>
        [TestMethod]
        public void CurveTest_51()
        {

			Collection<ResourceSpreadDto> sut = new Collection<ResourceSpreadDto>();
            DateTime start = Convert.ToDateTime("05/2010");
            DateTime end = Convert.ToDateTime("05/2012"); // should produce 20 months
            SpreadCurves CurveID = SpreadCurves.SpreadCurve51;
            long HoursSpread = 9000005000;

            WorkspaceDTO workspace = new WorkspaceDTO() { ResourceDecimalPrecision = 0 };
            sut = LaborSpreadCalcs(start, end, HoursSpread, CurveID, workspace);

            Assert.IsNotNull(sut, "The value returned from Spread Curve BL was bad");

            // Make sure that all the values returned sum up to HoursSpread
            long Cumulative = 0;
            for (int x = 0; x < sut.Count(); x++)
            {
                Cumulative = Convert.ToInt64(sut[x].LaborSpreadValue) + Cumulative;
            }
            Assert.IsTrue(Cumulative == HoursSpread, "The return value did not equal HoursSpread");

            HoursSpread = 0;

            sut = LaborSpreadCalcs(start, end, HoursSpread, CurveID, workspace);

            Assert.IsNotNull(sut, "The value returned from Spread Curve BL was bad");

            // Make sure that all the values returned sum up to HoursSpread
            Cumulative = 0;
            for (int x = 0; x < sut.Count(); x++)
            {
                Cumulative = Convert.ToInt64(sut[x].LaborSpreadValue) + Cumulative;
            }
            Assert.IsTrue(Cumulative == HoursSpread, "The return value did not equal HoursSpread");
        }

        /// <summary>
        /// Tests when null is passed in to the bookend spread (Curves 51, 52 or 53) logic
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ManipulatedLaborSpreadRequestNullTest()
        {
            ManipulatedLaborSpreadRequest sut = new ManipulatedLaborSpreadRequest(null);
            Assert.IsNull(sut);
        }

        /// <summary>
        /// Tests that SpreadCurves.SpreadCurve3 is returned when SpreadCurves.SpreadCurve51, SpreadCurves.SpreadCurve52, SpreadCurves.SpreadCurve53 are passed in
        /// and there are less than 3 spread months.  Bookend spreads are not valid with less then 3 months
        /// </summary>
        [TestMethod]
        public void ManipulatedLaborSpreadRequestWithLessThan3Months()
        {
            // test curve 51
            LaborSpreadRequest lsr = new LaborSpreadRequest(SpreadCurves.SpreadCurve51, new DateTime(2014, 6, 15), new DateTime(2014, 7, 15),100);
            ManipulatedLaborSpreadRequest sut = new ManipulatedLaborSpreadRequest(lsr);
            Assert.AreEqual(((int)SpreadCurves.SpreadCurve3) - 1, sut.ManipulatedCurveID, "The ManipulatedCurveID is not SpreadCurve3 as expected when testing curve 51");

            //test curve 52
            lsr = new LaborSpreadRequest(SpreadCurves.SpreadCurve52, new DateTime(2014, 6, 15), new DateTime(2014, 7, 15), 100);
            sut = new ManipulatedLaborSpreadRequest(lsr);
            Assert.AreEqual(((int)SpreadCurves.SpreadCurve3) - 1, sut.ManipulatedCurveID, "The ManipulatedCurveID is not SpreadCurve3 as expected when testing curve 52");

            //test curve 53
            lsr = new LaborSpreadRequest(SpreadCurves.SpreadCurve53, new DateTime(2014, 6, 15), new DateTime(2014, 7, 15), 100);
            sut = new ManipulatedLaborSpreadRequest(lsr);
            Assert.AreEqual(((int)SpreadCurves.SpreadCurve3) - 1, sut.ManipulatedCurveID, "The ManipulatedCurveID is not SpreadCurve3 as expected when testing curve 53");
        }

        /// <summary>
        /// This test case will test Curve Test 52. It will verify that null data was not
        /// returned from the Labor Spread calculation and that the total value returned
        /// equalled the HoursSpread that was inputted
        /// </summary>
        [TestMethod]
        public void CurveTest_52()
        {

			Collection<ResourceSpreadDto> sut = new Collection<ResourceSpreadDto>();
            DateTime start = Convert.ToDateTime("05/2010");
            DateTime end = Convert.ToDateTime("05/2012"); // should produce 20 months
            SpreadCurves CurveID = SpreadCurves.SpreadCurve52;
            long HoursSpread = 9000005000;

            WorkspaceDTO workspace = new WorkspaceDTO() { ResourceDecimalPrecision = 0 };
            sut = LaborSpreadCalcs(start, end, HoursSpread, CurveID, workspace);

            Assert.IsNotNull(sut, "The value returned from Spread Curve BL was bad");

            // Make sure that all the values returned sum up to HoursSpread
            long Cumulative = 0;
            for (int x = 0; x < sut.Count(); x++)
            {
                Cumulative = Convert.ToInt64(sut[x].LaborSpreadValue) + Cumulative;
            }
            Assert.IsTrue(Cumulative == HoursSpread, "The return value did not equal HoursSpread");

            HoursSpread = 0;

            sut = LaborSpreadCalcs(start, end, HoursSpread, CurveID, workspace);

            Assert.IsNotNull(sut, "The value returned from Spread Curve BL was bad");

            // Make sure that all the values returned sum up to HoursSpread
            Cumulative = 0;
            for (int x = 0; x < sut.Count(); x++)
            {
                Cumulative = Convert.ToInt64(sut[x].LaborSpreadValue) + Cumulative;
            }
            Assert.IsTrue(Cumulative == HoursSpread, "The return value did not equal HoursSpread");
        }

        /// <summary>
        /// This test case will test Curve Test 53. It will verify that null data was not
        /// returned from the Labor Spread calculation and that the total value returned
        /// equalled the HoursSpread that was inputted
        /// </summary>
        [TestMethod]
        public void CurveTest_53()
        {

			Collection<ResourceSpreadDto> sut = new Collection<ResourceSpreadDto>();
            DateTime start = Convert.ToDateTime("05/2010");
            DateTime end = Convert.ToDateTime("05/2012"); // should produce 20 months
            SpreadCurves CurveID = SpreadCurves.SpreadCurve53;
            long HoursSpread = 9000005000;

            WorkspaceDTO workspace = new WorkspaceDTO() { ResourceDecimalPrecision = 0 };
            sut = LaborSpreadCalcs(start, end, HoursSpread, CurveID, workspace);

            Assert.IsNotNull(sut, "The value returned from Spread Curve BL was bad");

            // Make sure that all the values returned sum up to HoursSpread
            long Cumulative = 0;
            for (int x = 0; x < sut.Count(); x++)
            {
                Cumulative = Convert.ToInt64(sut[x].LaborSpreadValue) + Cumulative;
            }
            Assert.IsTrue(Cumulative == HoursSpread, "The return value did not equal HoursSpread");

            HoursSpread = 0;

            sut = LaborSpreadCalcs(start, end, HoursSpread, CurveID, workspace);

            Assert.IsNotNull(sut, "The value returned from Spread Curve BL was bad");

            // Make sure that all the values returned sum up to HoursSpread
            Cumulative = 0;
            for (int x = 0; x < sut.Count(); x++)
            {
                Cumulative = Convert.ToInt64(sut[x].LaborSpreadValue) + Cumulative;
            }
            Assert.IsTrue(Cumulative == HoursSpread, "The return value did not equal HoursSpread");
        }

        /// <summary>
        /// This test case will correctly throw a null argument exception; No null data should be passed in to calculate labor spreads
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullSpreadData()
        {
            //Act
            SpreadCurve.CalculateLaborSpreadsBasedOnCurve(null, 0);
        }

        /// <summary>
        /// This test case will make sure a NullReferenceException occurs if the end date is less than the start date for the labor spread
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(GenValidationException))]
        public void EndLessThanStartDate_SpreadBL()
        {
            DateTime start = Convert.ToDateTime("05/2011");
            DateTime end = Convert.ToDateTime("05/2010");
            SpreadCurves CurveID = SpreadCurves.SpreadCurve50;
            long HoursSpread = 9000005000;

            WorkspaceDTO workspace = new WorkspaceDTO() { ResourceDecimalPrecision = 0 };
            LaborSpreadCalcs(start, end, HoursSpread, CurveID, workspace);
        }

        /// <summary>
        /// This test case will verify if the passed in HoursSpread is 0, then there is nothing to spread and all 0's will be returned
        /// </summary>
        [TestMethod]
        public void NoHourSpread_SpreadBL()
        {
			Collection<ResourceSpreadDto> sut = new Collection<ResourceSpreadDto>();
            DateTime start = Convert.ToDateTime("05/2010");
            DateTime end = Convert.ToDateTime("05/2012"); // should produce 20 months
            SpreadCurves CurveID = SpreadCurves.SpreadCurve50;
            long HoursSpread = 0;

            WorkspaceDTO workspace = new WorkspaceDTO() { ResourceDecimalPrecision = 0 };
            sut = LaborSpreadCalcs(start, end, HoursSpread, CurveID, workspace);

            Assert.IsNotNull(sut, "The value returned from Spread Curve BL was bad");

            // Make sure that all the values returned sum up to HoursSpread
            long Cumulative = 0;
            for (int x = 0; x < sut.Count(); x++)
            {
                Cumulative = Convert.ToInt64(sut[x].LaborSpreadValue) + Cumulative;
            }
            Assert.IsTrue(Cumulative == HoursSpread, "The return value did not equal HoursSpread");
        }

        /// <summary>
        /// Test case handling negative labor spreads
        /// </summary>
        [TestMethod]
        public void NegativeCurveTest_3()
        {

			Collection<ResourceSpreadDto> sut = new Collection<ResourceSpreadDto>();
            DateTime start = Convert.ToDateTime("05/2010");
            DateTime end = Convert.ToDateTime("05/2015"); // should produce 20 months
            SpreadCurves CurveID = SpreadCurves.SpreadCurve3;
            long HoursSpread = -9000021258;

            WorkspaceDTO workspace = new WorkspaceDTO() { ResourceDecimalPrecision = 0 };
            sut = LaborSpreadCalcs(start, end, HoursSpread, CurveID, workspace);

            Assert.IsNotNull(sut, "The value returned from Spread Curve BL was bad");

            // Make sure that all the values returned sum up to HoursSpread
            long Cumulative = 0;
            for (int x = 0; x < sut.Count(); x++)
            {
                Cumulative = Convert.ToInt64(sut[x].LaborSpreadValue) + Cumulative;
            }

            Assert.IsTrue(Cumulative == HoursSpread, "The return value did not equal HoursSpread");
        }

        /// <summary>
        /// Test case handling negative labor spreads
        /// </summary>
        [TestMethod]
        public void NegativeCurveTest_50()
        {

			Collection<ResourceSpreadDto> sut = new Collection<ResourceSpreadDto>();
            DateTime start = Convert.ToDateTime("05/2010");
            DateTime end = Convert.ToDateTime("05/2015"); // should produce 20 months
            SpreadCurves CurveID = SpreadCurves.SpreadCurve50;
            long HoursSpread = -9000021258;

            WorkspaceDTO workspace = new WorkspaceDTO() { ResourceDecimalPrecision = 0 };
            sut = LaborSpreadCalcs(start, end, HoursSpread, CurveID, workspace);

            Assert.IsNotNull(sut, "The value returned from Spread Curve BL was bad");

            // Make sure that all the values returned sum up to HoursSpread
            long Cumulative = 0;
            for (int x = 0; x < sut.Count(); x++)
            {
                Cumulative = Convert.ToInt64(sut[x].LaborSpreadValue) + Cumulative;
            }

            Assert.IsTrue(Cumulative == HoursSpread, "The return value did not equal HoursSpread");
        }

        /// <summary>
        /// Test case handling negative labor spreads
        /// </summary>
        [TestMethod]
        public void CurveTest_51Negative()
        {

			Collection<ResourceSpreadDto> sut = new Collection<ResourceSpreadDto>();
            DateTime start = Convert.ToDateTime("05/2010");
            DateTime end = Convert.ToDateTime("05/2015"); // should produce 20 months
            SpreadCurves CurveID = SpreadCurves.SpreadCurve51;
            long HoursSpread = -9000021258;

            WorkspaceDTO workspace = new WorkspaceDTO() { ResourceDecimalPrecision = 0 };
            sut = LaborSpreadCalcs(start, end, HoursSpread, CurveID, workspace);

            Assert.IsNotNull(sut, "The value returned from Spread Curve BL was bad");

            // Make sure that all the values returned sum up to HoursSpread
            long Cumulative = 0;
            for (int x = 0; x < sut.Count(); x++)
            {
                Cumulative = Convert.ToInt64(sut[x].LaborSpreadValue) + Cumulative;
            }

            Assert.IsTrue(Cumulative == HoursSpread, "The return value did not equal HoursSpread");
        }

        /// <summary>
        /// Test case handling negative labor spreads
        /// </summary>
        [TestMethod]
        public void CurveTest_52Negative()
        {

			Collection<ResourceSpreadDto> sut = new Collection<ResourceSpreadDto>();
            DateTime start = Convert.ToDateTime("05/2010");
            DateTime end = Convert.ToDateTime("05/2015"); // should produce 20 months
            SpreadCurves CurveID = SpreadCurves.SpreadCurve52;
            long HoursSpread = -9000021258;

            WorkspaceDTO workspace = new WorkspaceDTO() { ResourceDecimalPrecision = 0 };
            sut = LaborSpreadCalcs(start, end, HoursSpread, CurveID, workspace);

            Assert.IsNotNull(sut, "The value returned from Spread Curve BL was bad");

            // Make sure that all the values returned sum up to HoursSpread
            long Cumulative = 0;
            for (int x = 0; x < sut.Count(); x++)
            {
                Cumulative = Convert.ToInt64(sut[x].LaborSpreadValue) + Cumulative;
            }

            Assert.IsTrue(Cumulative == HoursSpread, "The return value did not equal HoursSpread");
        }

        /// <summary>
        /// Test case handling negative labor spreads
        /// </summary>
        [TestMethod]
        public void CurveTest_53Negative()
        {

			Collection<ResourceSpreadDto> sut = new Collection<ResourceSpreadDto>();
            DateTime start = Convert.ToDateTime("05/2010");
            DateTime end = Convert.ToDateTime("05/2015"); // should produce 20 months
            SpreadCurves CurveID = SpreadCurves.SpreadCurve53;
            long HoursSpread = -9000021258;

            WorkspaceDTO workspace = new WorkspaceDTO() { ResourceDecimalPrecision = 0 };
            sut = LaborSpreadCalcs(start, end, HoursSpread, CurveID, workspace);

            Assert.IsNotNull(sut, "The value returned from Spread Curve BL was bad");

            // Make sure that all the values returned sum up to HoursSpread
            long Cumulative = 0;
            for (int x = 0; x < sut.Count(); x++)
            {
                Cumulative = Convert.ToInt64(sut[x].LaborSpreadValue) + Cumulative;
            }

            Assert.IsTrue(Cumulative == HoursSpread, "The return value did not equal HoursSpread");
        }

        /// <summary>
        /// This test case was created for bug 2277 when the returning value from Sread Curve is actually more than Hour Spread
        /// </summary>
        [TestMethod]
        public void SpreadCurveOverHourSpread()
        {
			Collection<ResourceSpreadDto> sut = new Collection<ResourceSpreadDto>();
            DateTime start = Convert.ToDateTime("01/2011");
            DateTime end = Convert.ToDateTime("12/2015");
            SpreadCurves CurveID = SpreadCurves.SpreadCurve26;
            long HoursSpread = 90009000000;

            WorkspaceDTO workspace = new WorkspaceDTO() { ResourceDecimalPrecision = 0 };
            sut = LaborSpreadCalcs(start, end, HoursSpread, CurveID, workspace);

            Assert.IsNotNull(sut, "The value returned from Spread Curve BL was bad");

            // Make sure that all the values returned sum up to HoursSpread
            long Cumulative = 0;
            for (int x = 0; x < sut.Count(); x++)
            {
                Cumulative = Convert.ToInt64(sut[x].LaborSpreadValue) + Cumulative;
            }

     
          Assert.IsTrue(Cumulative == HoursSpread, "The return value did not equal HoursSpread");
       
        }

        /// <summary>
        /// This test case was created for bug 2277 when the returning value from Sread Curve is actually more than Hour Spread
        /// </summary>
        [TestMethod]
        public void SpreadCurveOverNegativeHourSpread()
        {
			Collection<ResourceSpreadDto> sut = new Collection<ResourceSpreadDto>();
            DateTime start = Convert.ToDateTime("01/2011");
            DateTime end = Convert.ToDateTime("12/2015");
            SpreadCurves CurveID = SpreadCurves.SpreadCurve46;
            long HoursSpread = -90009000000;

            WorkspaceDTO workspace = new WorkspaceDTO() { ResourceDecimalPrecision = 0 };
            sut = LaborSpreadCalcs(start, end, HoursSpread, CurveID, workspace);

            Assert.IsNotNull(sut, "The value returned from Spread Curve BL was bad");

            // Make sure that all the values returned sum up to HoursSpread
            long Cumulative = 0;
            for (int x = 0; x < sut.Count(); x++)
            {
                Cumulative = Convert.ToInt64(sut[x].LaborSpreadValue) + Cumulative;
            }


            Assert.IsTrue(Cumulative == HoursSpread, "The return value did not equal HoursSpread");

        }

        /// <summary>
        /// Testing with decimal spreads..
        /// </summary>
        [TestMethod]
        public void SpreadWithDecimalPrecision_1()
        {
			Collection<ResourceSpreadDto> sut = new Collection<ResourceSpreadDto>();
            DateTime start = Convert.ToDateTime("01/2011");
            DateTime end = Convert.ToDateTime("12/2011");
            SpreadCurves CurveID = SpreadCurves.SpreadCurve3; // level
            long HoursSpread = 1000;

            WorkspaceDTO workspace = new WorkspaceDTO() { ResourceDecimalPrecision = 3 };
            sut = LaborSpreadCalcs(start, end, HoursSpread, CurveID, workspace);

            Assert.IsNotNull(sut, "The value returned from Spread Curve BL was bad");

            // Make sure that all the values returned sum up to HoursSpread
            decimal cumulative = 0;
            for (int x = 0; x < sut.Count(); x++)
            {
                cumulative = Convert.ToDecimal(sut[x].LaborSpreadValue) + cumulative;
            }

            Assert.IsTrue(cumulative == Convert.ToDecimal(HoursSpread), "The return value did not equal HoursSpread");

            // check each one, to make sure it spread the right way..
            Assert.AreEqual((decimal)83.333, sut[0].LaborSpreadValue);
            Assert.AreEqual((decimal)83.333, sut[1].LaborSpreadValue);
            Assert.AreEqual((decimal)83.334, sut[2].LaborSpreadValue);
            Assert.AreEqual((decimal)83.333, sut[3].LaborSpreadValue);
            Assert.AreEqual((decimal)83.333, sut[4].LaborSpreadValue);
            Assert.AreEqual((decimal)83.334, sut[5].LaborSpreadValue);
            Assert.AreEqual((decimal)83.333, sut[6].LaborSpreadValue);
            Assert.AreEqual((decimal)83.333, sut[7].LaborSpreadValue);
            Assert.AreEqual((decimal)83.334, sut[8].LaborSpreadValue);
            Assert.AreEqual((decimal)83.333, sut[9].LaborSpreadValue);
            Assert.AreEqual((decimal)83.333, sut[10].LaborSpreadValue);
            Assert.AreEqual((decimal)83.334, sut[11].LaborSpreadValue);
        }

        /// <summary>
        /// Testing with decimal spreads..
        /// </summary>
        [TestMethod]
        public void SpreadWithDecimalPrecision_2()
        {
			Collection<ResourceSpreadDto> sut = new Collection<ResourceSpreadDto>();
            DateTime start = Convert.ToDateTime("01/2011");
            DateTime end = Convert.ToDateTime("12/2011");
            SpreadCurves CurveID = SpreadCurves.SpreadCurve3; // level
            long HoursSpread = 5000;

            WorkspaceDTO workspace = new WorkspaceDTO() { ResourceDecimalPrecision = 3 };
            sut = LaborSpreadCalcs(start, end, HoursSpread, CurveID, workspace);

            Assert.IsNotNull(sut, "The value returned from Spread Curve BL was bad");

            // Make sure that all the values returned sum up to HoursSpread
            decimal cumulative = 0;
            for (int x = 0; x < sut.Count(); x++)
            {
                cumulative = Convert.ToDecimal(sut[x].LaborSpreadValue) + cumulative;
            }

            Assert.IsTrue(cumulative == Convert.ToDecimal(HoursSpread), "The return value did not equal HoursSpread");

            // check each one, to make sure it spread the right way..
            Assert.AreEqual((decimal)416.667, sut[0].LaborSpreadValue);
            Assert.AreEqual((decimal)416.667, sut[1].LaborSpreadValue);
            Assert.AreEqual((decimal)416.667, sut[2].LaborSpreadValue);
            Assert.AreEqual((decimal)416.666, sut[3].LaborSpreadValue);
            Assert.AreEqual((decimal)416.667, sut[4].LaborSpreadValue);
            Assert.AreEqual((decimal)416.667, sut[5].LaborSpreadValue);
            Assert.AreEqual((decimal)416.666, sut[6].LaborSpreadValue);
            Assert.AreEqual((decimal)416.667, sut[7].LaborSpreadValue);
            Assert.AreEqual((decimal)416.667, sut[8].LaborSpreadValue);
            Assert.AreEqual((decimal)416.666, sut[9].LaborSpreadValue);
            Assert.AreEqual((decimal)416.667, sut[10].LaborSpreadValue);
            Assert.AreEqual((decimal)416.666, sut[11].LaborSpreadValue);
        }

        /// <summary>
        /// Testing with decimal spreads..
        /// </summary>
        [TestMethod]
        public void SpreadWithDecimalPrecision_3()
        {
			Collection<ResourceSpreadDto> sut = new Collection<ResourceSpreadDto>();
            DateTime start = Convert.ToDateTime("01/2011");
            DateTime end = Convert.ToDateTime("12/2011");
            SpreadCurves CurveID = SpreadCurves.SpreadCurve3; // level
            long HoursSpread = 1;

            WorkspaceDTO workspace = new WorkspaceDTO() { ResourceDecimalPrecision = 2 };
            sut = LaborSpreadCalcs(start, end, HoursSpread, CurveID, workspace);

            Assert.IsNotNull(sut, "The value returned from Spread Curve BL was bad");

            // Make sure that all the values returned sum up to HoursSpread
            decimal cumulative = 0;
            for (int x = 0; x < sut.Count(); x++)
            {
                cumulative = Convert.ToDecimal(sut[x].LaborSpreadValue) + cumulative;
            }

            Assert.IsTrue(cumulative == Convert.ToDecimal(HoursSpread), "The return value did not equal HoursSpread");

            // check each one, to make sure it spread the right way..
            Assert.AreEqual((decimal)0.08, sut[0].LaborSpreadValue);
            Assert.AreEqual((decimal)0.08, sut[1].LaborSpreadValue);
            Assert.AreEqual((decimal)0.09, sut[2].LaborSpreadValue);
            Assert.AreEqual((decimal)0.08, sut[3].LaborSpreadValue);
            Assert.AreEqual((decimal)0.08, sut[4].LaborSpreadValue);
            Assert.AreEqual((decimal)0.09, sut[5].LaborSpreadValue);
            Assert.AreEqual((decimal)0.08, sut[6].LaborSpreadValue);
            Assert.AreEqual((decimal)0.08, sut[7].LaborSpreadValue);
            Assert.AreEqual((decimal)0.09, sut[8].LaborSpreadValue);
            Assert.AreEqual((decimal)0.08, sut[9].LaborSpreadValue);
            Assert.AreEqual((decimal)0.08, sut[10].LaborSpreadValue);
            Assert.AreEqual((decimal)0.09, sut[11].LaborSpreadValue);
        }

        /// <summary>
        /// This test case will test Curve Test 51. It will verify that null data was not
        /// returned from the Labor Spread calculation and that the total value returned
        /// equalled the HoursSpread that was inputted
        /// </summary>
        [TestMethod]
        public void SpreadWithDecimalPrecision_Curve51()
        {
			Collection<ResourceSpreadDto> sut = new Collection<ResourceSpreadDto>();
            DateTime start = Convert.ToDateTime("05/2010");
            DateTime end = Convert.ToDateTime("05/2012");
            SpreadCurves CurveID = SpreadCurves.SpreadCurve51;
            long HoursSpread = 1000;

            WorkspaceDTO workspace = new WorkspaceDTO() { ResourceDecimalPrecision = 1 };
            sut = LaborSpreadCalcs(start, end, HoursSpread, CurveID, workspace);

            Assert.IsNotNull(sut, "The value returned from Spread Curve BL was bad");

            // Make sure that all the values returned sum up to HoursSpread
            decimal cumulative = 0;
            for (int x = 0; x < sut.Count(); x++)
            {
                cumulative = Convert.ToDecimal(sut[x].LaborSpreadValue) + cumulative;
            }

            Assert.IsTrue(cumulative == Convert.ToDecimal(HoursSpread), "The return value did not equal HoursSpread");

            // check each one, to make sure it spread the right way..
            Assert.AreEqual((decimal)20.8, sut[0].LaborSpreadValue);
            Assert.AreEqual((decimal)41.7, sut[1].LaborSpreadValue);
            Assert.AreEqual((decimal)41.7, sut[2].LaborSpreadValue);
            Assert.AreEqual((decimal)41.7, sut[3].LaborSpreadValue);
            Assert.AreEqual((decimal)41.6, sut[4].LaborSpreadValue);
            Assert.AreEqual((decimal)41.7, sut[5].LaborSpreadValue);
            Assert.AreEqual((decimal)41.7, sut[6].LaborSpreadValue);
            Assert.AreEqual((decimal)41.6, sut[7].LaborSpreadValue);
            Assert.AreEqual((decimal)41.7, sut[8].LaborSpreadValue);
            Assert.AreEqual((decimal)41.7, sut[9].LaborSpreadValue);
            Assert.AreEqual((decimal)41.6, sut[10].LaborSpreadValue);
            Assert.AreEqual((decimal)41.7, sut[11].LaborSpreadValue);
            Assert.AreEqual((decimal)41.7, sut[12].LaborSpreadValue);
            Assert.AreEqual((decimal)41.7, sut[13].LaborSpreadValue);
            Assert.AreEqual((decimal)41.6, sut[14].LaborSpreadValue);
            Assert.AreEqual((decimal)41.7, sut[15].LaborSpreadValue);
            Assert.AreEqual((decimal)41.7, sut[16].LaborSpreadValue);
            Assert.AreEqual((decimal)41.6, sut[17].LaborSpreadValue);
            Assert.AreEqual((decimal)41.7, sut[18].LaborSpreadValue);
            Assert.AreEqual((decimal)41.7, sut[19].LaborSpreadValue);
            Assert.AreEqual((decimal)41.6, sut[20].LaborSpreadValue);
            Assert.AreEqual((decimal)41.7, sut[21].LaborSpreadValue);
            Assert.AreEqual((decimal)41.7, sut[22].LaborSpreadValue);
            Assert.AreEqual((decimal)41.6, sut[23].LaborSpreadValue);
            Assert.AreEqual((decimal)20.8, sut[24].LaborSpreadValue);
        }

        /// <summary>
        /// This test case will test Curve Test 52. It will verify that null data was not
        /// returned from the Labor Spread calculation and that the total value returned
        /// equalled the HoursSpread that was inputted
        /// </summary>
        [TestMethod]
        public void SpreadWithDecimalPrecision_Curve52()
        {
			Collection<ResourceSpreadDto> sut = new Collection<ResourceSpreadDto>();
            DateTime start = Convert.ToDateTime("05/2010");
            DateTime end = Convert.ToDateTime("05/2012");
            SpreadCurves CurveID = SpreadCurves.SpreadCurve52;
            long HoursSpread = 1000;

            WorkspaceDTO workspace = new WorkspaceDTO() { ResourceDecimalPrecision = 1 };
            sut = LaborSpreadCalcs(start, end, HoursSpread, CurveID, workspace);

            Assert.IsNotNull(sut, "The value returned from Spread Curve BL was bad");

            // Make sure that all the values returned sum up to HoursSpread
            decimal cumulative = 0;
            for (int x = 0; x < sut.Count(); x++)
            {
                cumulative = Convert.ToDecimal(sut[x].LaborSpreadValue) + cumulative;
            }

            Assert.IsTrue(cumulative == Convert.ToDecimal(HoursSpread), "The return value did not equal HoursSpread");

            // check each one, to make sure it spread the right way..
            Assert.AreEqual((decimal)31.3, sut[0].LaborSpreadValue);
            Assert.AreEqual((decimal)41.7, sut[1].LaborSpreadValue);
            Assert.AreEqual((decimal)41.7, sut[2].LaborSpreadValue);
            Assert.AreEqual((decimal)41.6, sut[3].LaborSpreadValue);
            Assert.AreEqual((decimal)41.7, sut[4].LaborSpreadValue);
            Assert.AreEqual((decimal)41.7, sut[5].LaborSpreadValue);
            Assert.AreEqual((decimal)41.6, sut[6].LaborSpreadValue);
            Assert.AreEqual((decimal)41.7, sut[7].LaborSpreadValue);
            Assert.AreEqual((decimal)41.7, sut[8].LaborSpreadValue);
            Assert.AreEqual((decimal)41.6, sut[9].LaborSpreadValue);
            Assert.AreEqual((decimal)41.7, sut[10].LaborSpreadValue);
            Assert.AreEqual((decimal)41.7, sut[11].LaborSpreadValue);
            Assert.AreEqual((decimal)41.6, sut[12].LaborSpreadValue);
            Assert.AreEqual((decimal)41.7, sut[13].LaborSpreadValue);
            Assert.AreEqual((decimal)41.7, sut[14].LaborSpreadValue);
            Assert.AreEqual((decimal)41.6, sut[15].LaborSpreadValue);
            Assert.AreEqual((decimal)41.7, sut[16].LaborSpreadValue);
            Assert.AreEqual((decimal)41.7, sut[17].LaborSpreadValue);
            Assert.AreEqual((decimal)41.6, sut[18].LaborSpreadValue);
            Assert.AreEqual((decimal)41.7, sut[19].LaborSpreadValue);
            Assert.AreEqual((decimal)41.7, sut[20].LaborSpreadValue);
            Assert.AreEqual((decimal)41.6, sut[21].LaborSpreadValue);
            Assert.AreEqual((decimal)41.7, sut[22].LaborSpreadValue);
            Assert.AreEqual((decimal)41.6, sut[23].LaborSpreadValue);
            Assert.AreEqual((decimal)10.4, sut[24].LaborSpreadValue);
        }
    }
}
