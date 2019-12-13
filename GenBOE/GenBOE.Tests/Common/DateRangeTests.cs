using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GenBOE.Common.classes;

namespace GenBOE.Tests.Common
{
    
    [TestClass]
    public class DateRangeTests
    {
        [TestMethod]
        public void DateRangeDefaultConstructorTest()
        {
            DateRange dateRange = new DateRange();

            Assert.IsNotNull(dateRange);
            Assert.IsNull(dateRange.StartDate);
            Assert.IsNull(dateRange.EndDate);


        }

        [TestMethod]
        public void DateRangeNullNullTest()
        {
            DateRange dateRange = new DateRange(null, null);

            Assert.IsNotNull(dateRange);
            Assert.IsNull(dateRange.StartDate);
            Assert.IsNull(dateRange.EndDate);

        }


        [TestMethod]
        public void DateRangeStartNullTest()
        {
            DateTime start = new DateTime(2013, 8, 30, 12, 35, 56);
            DateRange dateRange = new DateRange(start, null);

            Assert.IsNotNull(dateRange);
            Assert.AreEqual(dateRange.StartDate, start);
            Assert.IsNull(dateRange.EndDate);

        }

        [TestMethod]
        public void DateRangeStartEndSameTest()
        {
            DateTime start = new DateTime(2013, 8, 30, 12, 35, 56);
            DateTime end = new DateTime(2013, 8, 30, 12, 35, 56);
            DateRange dateRange = new DateRange(start, end);

            Assert.IsNotNull(dateRange);
            Assert.AreEqual(dateRange.StartDate, start);
            Assert.AreEqual(dateRange.EndDate, end);
            Assert.IsNotNull(dateRange.EndDate);
            TimeSpan exp = new TimeSpan(0);
            Assert.AreEqual(exp, dateRange.TimeSpan);
        }


        [TestMethod]
        public void DateRangeEqualsTrue()
        {
            DateTime start = new DateTime(2013, 8, 30, 12, 35, 56);
            DateTime end = new DateTime(2013, 8, 30, 13, 35, 56);
            DateRange dateRange = new DateRange(start, end);

            DateTime start2 = new DateTime(2013, 8, 30, 12, 35, 56);
            DateTime end2 = new DateTime(2013, 8, 30, 13, 35, 56);
            DateRange dateRange2 = new DateRange(start2, end2);

            Assert.IsTrue(dateRange.Equals(dateRange2));

            
        }

        [TestMethod]
        public void DateRangeIntersectionSameRangeTest()
        {
            DateTime start = new DateTime(2013, 8, 30, 12, 35, 56);
            DateTime end = new DateTime(2013, 8, 30, 13, 35, 56);
            DateRange dateRange = new DateRange(start, end);

            DateTime start2 = new DateTime(2013, 8, 30, 12, 35, 56);
            DateTime end2 = new DateTime(2013, 8, 30, 13, 35, 56);
            DateRange dateRange2 = new DateRange(start2, end2);

            DateRange res = dateRange.GetIntersection(dateRange2);

            DateTime startExp = new DateTime(2013, 8, 30, 12, 35, 56);
            DateTime endExp = new DateTime(2013, 8, 30, 13, 35, 56);
            DateRange expected = new DateRange(startExp, endExp);

            Assert.IsTrue(expected.Equals(res));
            Assert.AreEqual(expected.StartDate, res.StartDate);
            Assert.AreEqual(expected.EndDate, res.EndDate);
            Assert.AreEqual(expected.TimeSpan, res.TimeSpan);
        }

        [TestMethod]
        public void DateRangeIntersectionNoEnd2()
        {
            DateTime start = new DateTime(2013, 8, 30, 12, 35, 56);
            DateTime end = new DateTime(2013, 8, 30, 13, 35, 56);
            DateRange dateRange = new DateRange(start, end);

            DateTime start2 = new DateTime(2013, 8, 30, 12, 35, 56);
            DateTime? end2 = null;
            DateRange dateRange2 = new DateRange(start2, end2);

            DateRange res = dateRange.GetIntersection(dateRange2);

            DateTime startExp = new DateTime(2013, 8, 30, 12, 35, 56);
            DateTime endExp = new DateTime(2013, 8, 30, 13, 35, 56);
            DateRange expected = new DateRange(startExp, endExp);

            Assert.IsTrue(expected.Equals(res));
            Assert.AreEqual(expected.StartDate, res.StartDate);
            Assert.AreEqual(expected.EndDate, res.EndDate);
            Assert.AreEqual(expected.TimeSpan, res.TimeSpan);
        }


        //This is on purpose to make sure correct exception is thrown and not just any.  
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes"), TestMethod]
        public void DateRangeTooEarlyEndDate()
        {
            DateTime start = new DateTime(2013, 8, 30, 12, 35, 56);
            DateTime end = new DateTime(2013, 8, 30, 12, 35, 55);
            try
            {
                DateRange dateRange = new DateRange(start, end);
                if (dateRange != null)
                {
                    Assert.Fail();
                }
                else
                {
                    Assert.Fail();
                }
            }
            catch (InvalidOperationException e)
            {
                Assert.IsNotNull(e);
            }
            catch (Exception )
            {
                Assert.Fail();
            }
            
        }


        [TestMethod]
        public void DateRangeIntersectionNoEnd()
        {
            DateTime start = new DateTime(2013, 8, 30, 12, 35, 56);
            DateTime? end = null;
            DateRange dateRange = new DateRange(start, end);

            DateTime start2 = new DateTime(2013, 8, 30, 12, 35, 56);
            DateTime? end2 = new DateTime(2013, 8, 30, 13, 35, 56); ;
            DateRange dateRange2 = new DateRange(start2, end2);

            DateRange res = dateRange.GetIntersection(dateRange2);

            DateTime startExp = new DateTime(2013, 8, 30, 12, 35, 56);
            DateTime endExp = new DateTime(2013, 8, 30, 13, 35, 56);
            DateRange expected = new DateRange(startExp, endExp);

            Assert.IsTrue(expected.Equals(res));
            Assert.AreEqual(expected.StartDate, res.StartDate);
            Assert.AreEqual(expected.EndDate, res.EndDate);
            Assert.AreEqual(expected.TimeSpan, res.TimeSpan);
        }


        [TestMethod]
        public void DateRangeSetters()
        {
            DateTime start = new DateTime(2013, 8, 30, 12, 35, 56);
            DateTime? end = null;
            DateRange dateRange = new DateRange(start, end);


            DateTime startnew = new DateTime(2012, 8, 30, 12, 35, 56);
            dateRange.StartDate = startnew;

            DateTime endNew = new DateTime(2012, 8, 30, 12, 35, 56);
            dateRange.EndDate = endNew;

            TimeSpan updateTimeSpace = new TimeSpan(endNew.Ticks - startnew.Ticks);

            

            Assert.AreEqual(startnew, dateRange.StartDate);
            Assert.AreEqual(endNew, dateRange.EndDate);
            Assert.AreEqual(updateTimeSpace, dateRange.TimeSpan);
        }


    }
     
}
