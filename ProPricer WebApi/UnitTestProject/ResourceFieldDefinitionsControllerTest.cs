using System.Collections.Generic;
using APTSPropricerApi.Connection;
using APTSPropricerApi.Controllers;
using APTSPropricerApi.DTOs;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject
{
    [TestClass]
    public class ResourceFieldDefinitionsControllerTest
    {
        [TestMethod]
        public void Get_Resource_Field_Definitions()
        {
            /// Arrange
            using (ResourceFieldDefinitionsController controller = new ResourceFieldDefinitionsController())
            {

                /// Act
                IEnumerable<ResourceFieldDefinitionsDto> resFieldDefs = controller.Get(TestConstants.InstanceId);

                /// Assert
                int count = 0;
                using (IEnumerator<ResourceFieldDefinitionsDto> enumerator = resFieldDefs.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        count++;
                    }
                }

                Assert.IsTrue(count > 0);
            }
        }

        [TestMethod]
        public void Get_Resource_Field_Definitions_For_Proposal()
        {
            string prid = "65e1eb88-ca1a-e711-82a5-847beb323d31";

            /// Arrange
            using (ResourceFieldDefinitionsController controller = new ResourceFieldDefinitionsController())
            {

                /// Act
                IEnumerable<ResourceFieldDefinitionsDto> resFieldDefs = controller.Get(TestConstants.InstanceId, prid);

                /// Assert
                int count = 0;
                using (IEnumerator<ResourceFieldDefinitionsDto> enumerator = resFieldDefs.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        count++;
                    }
                }

                Assert.IsTrue(count > 0);
            }
        }

        // ToDo: Dusan - we do not have the data to be able to do this, not sure about the goal..
        // [TestMethod]
        public void Put_Resource_Field_Definition_Value()
        {
            /// Arrange
            using (ResourceFieldDefinitionsController controller = new ResourceFieldDefinitionsController())
            {
                ResourceFieldsDto rfdto = new ResourceFieldsDto
                {
                    Key = "WBS1",
                    Value = "mike1"
                };

                /// Act
                controller.Put(TestConstants.InstanceId, rfdto);
            }
        }
    }
}