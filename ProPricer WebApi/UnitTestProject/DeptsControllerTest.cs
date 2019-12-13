using System.Collections.Generic;
using APTSPropricerApi.Connection;
using APTSPropricerApi.Controllers;
using APTSPropricerApi.DTOs;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject
{
    [TestClass]
    public class DeptsControllerTest
    {
        //Dusan - our DB setup isn't using any departments, so the test fails
        //[TestMethod]
        //public void Get_Depts()
        //{
        //    /// Arrange
        //    DeptsController controller = new DeptsController();

        //    /// Act
        //    IEnumerable<DeptsDto> depts = controller.Get(TestConstants.InstanceId);

        //    /// Assert
        //    int count = 0;
        //    using (IEnumerator<DeptsDto> enumerator = depts.GetEnumerator())
        //    {
        //        while (enumerator.MoveNext())
        //        {
        //            count++;
        //        }
        //    }

        //    Assert.IsTrue(count > 0);
        //}
    }
}