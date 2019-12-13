using System.Collections.Generic;
using APTSPropricerApi.Connection;
using APTSPropricerApi.Controllers;
using APTSPropricerApi.DTOs;
using EBS.ProPricer.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject
{
    [TestClass]
    public class SummaryFieldDefinitionsControllerTest
    {
        [TestMethod]
        public void Get_Default_Summary_Field_Definitions()
        {
            /// Arrange
            using (SummaryFieldDefinitionsController controller = new SummaryFieldDefinitionsController())
            {

                /// Act
                IEnumerable<SummaryFieldDefinitionsDto> sumFieldDefs = controller.Get(TestConstants.InstanceId);

                /// Assert
                int count = 0;
                using (IEnumerator<SummaryFieldDefinitionsDto> enumerator = sumFieldDefs.GetEnumerator())
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
        public void Get_Summary_Field_Definitions_For_Proposal()
        {
            string prid = "65e1eb88-ca1a-e711-82a5-847beb323d31";

            /// Arrange
            using (SummaryFieldDefinitionsController controller = new SummaryFieldDefinitionsController())
            {

                /// Act
                IEnumerable<SummaryFieldDefinitionsDto> sumFieldDefs = controller.Get(TestConstants.InstanceId, prid);

                /// Assert
                int count = 0;
                using (IEnumerator<SummaryFieldDefinitionsDto> enumerator = sumFieldDefs.GetEnumerator())
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
        public void Add_Summary_Field_Definitions()
        {
            /// Arrange
            Proposal pr = null;
            string template = "AUTO TEMPLATE TEST";
            //  template = "F16 from template";
            string version = "0";
            using (IProPricerConnection _ppc = (IProPricerConnection)PoolManager.GetInstance(TestConstants.InstanceId).GetObjectsFromPool())
            {
                if (_ppc.Workspace.Proposals.Find(template, version).HasValue)
                {
                    pr = _ppc.Workspace.Proposals.Find(template, version).Value;
                }
            }

            Assert.IsFalse(pr == null);

            using (SummaryFieldDefinitionsController controller = new SummaryFieldDefinitionsController())
            {

                /// Act
                IEnumerable<SummaryFieldDefinitionsDto> sumFieldDefs = controller.Get(TestConstants.InstanceId, pr.Id.ToString());

                int oldcount = 0;
                using (IEnumerator<SummaryFieldDefinitionsDto> enumerator = sumFieldDefs.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        enumerator.Current.Name = "new" + enumerator.Current.Name;
                        oldcount++;
                    }
                }

                Assert.IsTrue(oldcount > 0);

                ProposalDto prop = new ProposalDto
                {
                    Id = pr.Id.ToString(),
                    SumFieldDefs = sumFieldDefs
                };
                controller.Post(TestConstants.InstanceId, prop);

                sumFieldDefs = controller.Get(TestConstants.InstanceId, pr.Id.ToString());

                int newcount = 0;
                using (IEnumerator<SummaryFieldDefinitionsDto> enumerator = sumFieldDefs.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        if (newcount < oldcount)
                        {
                            enumerator.Current.Id = "zzzz" + enumerator.Current.Name;
                        }

                        newcount++;
                    }
                }

                /// Assert
                Assert.IsTrue(newcount == 2 * oldcount);

                //delete the six new ones 
                prop.Id = pr.Id.ToString();
                prop.SumFieldDefs = sumFieldDefs;
                controller.Delete(TestConstants.InstanceId, prop);

                newcount = 0;
                sumFieldDefs = controller.Get(TestConstants.InstanceId, pr.Id.ToString());
                using (IEnumerator<SummaryFieldDefinitionsDto> enumerator = sumFieldDefs.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        newcount++;
                    }
                }

                /// Assert
                Assert.IsTrue(newcount == oldcount);
            }
        }

        // ToDo: Dusan - we do not have the data to be able to do this, not sure about the goal..
        // [TestMethod]
        public void Delete_Summary_Field_Definitions()
        {
            /// Arrange
            Proposal pr = null;
            string template = "AUTO TEMPLATE TEST";
            string version = "0";
            using (IProPricerConnection _ppc = (IProPricerConnection)PoolManager.GetInstance(TestConstants.InstanceId).GetObjectsFromPool())
            {
                if (_ppc.Workspace.Proposals.Find(template, version).HasValue)
                {
                    pr = _ppc.Workspace.Proposals.Find(template, version).Value;
                }
            }

            Assert.IsFalse(pr == null);

            SummaryFieldDefinitionsController controller = new SummaryFieldDefinitionsController();

            /// Act
            IEnumerable<SummaryFieldDefinitionsDto> sumFieldDefs = controller.Get(TestConstants.InstanceId, pr.Id.ToString());

            int newcount = 0;
            using (IEnumerator<SummaryFieldDefinitionsDto> enumerator = sumFieldDefs.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (newcount < 6)
                    {
                        enumerator.Current.Id = "zzzz" + enumerator.Current.Name;
                    }

                    newcount++;
                }
            }

            //delete the six new ones 
            ProposalDto prop = new ProposalDto
            {
                Id = pr.Id.ToString(),
                SumFieldDefs = sumFieldDefs
            };
            controller.Delete(TestConstants.InstanceId, prop);

            using (IEnumerator<SummaryFieldDefinitionsDto> enumerator = sumFieldDefs.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Name = "new" + enumerator.Current.Name;
                    newcount++;
                }
            }

            /// Assert
            Assert.IsTrue(newcount == 6);
        }

        // ToDo: Dusan - we do not have the data to be able to do this, not sure about the goal..
        // [TestMethod]
        public void Delete_Summary_Field_Definitions_For_Proposal()
        {
            string prid = "c6f6eeb5-0775-e611-8c99-0205857feb80";

            /// Arrange
            SummaryFieldDefinitionsController controller = new SummaryFieldDefinitionsController();

            /// Act

            controller.Delete(TestConstants.InstanceId, prid);
            IEnumerable<SummaryFieldDefinitionsDto> sumFieldDefs = controller.Get(TestConstants.InstanceId, prid);

            /// Assert
            int count = 0;
            using (IEnumerator<SummaryFieldDefinitionsDto> enumerator = sumFieldDefs.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    count++;
                }
            }

            Assert.IsTrue(count == 0);
        }

        // ToDo: Dusan - we do not have the data to be able to do this, not sure about the goal..
        // [TestMethod]
        public void Update_Summary_Field_Definitions()
        {
            /// Arrange
            Proposal pr = null;
            string template = "AUTO TEMPLATE TEST";
            //   template = "F16 from template";
            string version = "0";
            using (IProPricerConnection _ppc = (IProPricerConnection)PoolManager.GetInstance(TestConstants.InstanceId).GetObjectsFromPool())
            {
                if (_ppc.Workspace.Proposals.Find(template, version).HasValue)
                {
                    pr = _ppc.Workspace.Proposals.Find(template, version).Value;
                }
            }

            Assert.IsFalse(pr == null);

            SummaryFieldDefinitionsController controller = new SummaryFieldDefinitionsController();

            /// Act
            IEnumerable<SummaryFieldDefinitionsDto> sumFieldDefs = controller.Get(TestConstants.InstanceId, pr.Id.ToString());
            IEnumerable<SummaryFieldDefinitionsDto> oldFieldDefs = controller.Get(TestConstants.InstanceId, pr.Id.ToString());
            string oldnames = string.Empty;
            string newnames = string.Empty;

            using (IEnumerator<SummaryFieldDefinitionsDto> enumerator = sumFieldDefs.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    oldnames = oldnames + enumerator.Current.Name;
                    enumerator.Current.Name = enumerator.Current.Name + "$$$";
                    newnames = newnames + enumerator.Current.Name;
                }
            }

            ProposalDto prop = new ProposalDto
            {
                Id = pr.Id.ToString(),
                SumFieldDefs = sumFieldDefs
            };
            controller.Put(TestConstants.InstanceId, prop);
            sumFieldDefs = controller.Get(TestConstants.InstanceId, pr.Id.ToString());
            string chgnames = string.Empty;
            using (IEnumerator<SummaryFieldDefinitionsDto> enumerator = sumFieldDefs.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    chgnames = chgnames + enumerator.Current.Name;
                }
            }

            Assert.IsTrue(chgnames == newnames);

            //   Prop = null;
            prop.Id = pr.Id.ToString();
            prop.SumFieldDefs = oldFieldDefs;
            controller.Put(TestConstants.InstanceId, prop);
            sumFieldDefs = controller.Get(TestConstants.InstanceId, pr.Id.ToString());

            /// Assert
            chgnames = string.Empty;
            using (IEnumerator<SummaryFieldDefinitionsDto> enumerator = sumFieldDefs.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    chgnames = chgnames + enumerator.Current.Name;
                }
            }

            Assert.IsTrue(chgnames == oldnames);
        }
    }
}