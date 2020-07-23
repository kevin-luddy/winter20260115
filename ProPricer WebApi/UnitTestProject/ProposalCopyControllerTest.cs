using System;
using APTSPropricerApi.Connection;
using APTSPropricerApi.Controllers;
using APTSPropricerApi.DTOs;
using EBS.Core;
using EBS.ProPricer.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject
{
    [TestClass]
    public class ProposalCopyControllerTest
    {
        // ToDo: Dusan - we do not have the data to be able to do this, not sure about the goal..
        // [TestMethod]
        public void Copy_Proposal_From_Template()
        {
            using (IProPricerConnection ppc = (IProPricerConnection)PoolManager.GetInstance(TestConstants.InstanceId).GetObjectsFromPool())
            {
                Proposal pr = null;
                /// Arrange
                //by id 
                string template = "F16 TEMPLATE";
                string version = "0";
                if (ppc.Workspace.Proposals.Find(template, version).HasValue())
                {
                    pr = ppc.Workspace.Proposals.Find(template, version).Value();
                }

                /// Act
                ProposalCopyController prop = new ProposalCopyController();

                // setup a new proposal
                ProposalDto newProp = new ProposalDto
                {
                    Id = pr.Id.ToString(), ///copy from id
                    Name = "_COPY FROM TEMPLATE_",
                    //  newProp.name = "80 12-00103";
                    Version = string.Empty, // "1";
                    Description = "Created from automated test project. It can safely be deleted.",
                    ParentFolderName = "Test-Dusan"
                };

                ReturnDto newid = prop.Post(TestConstants.InstanceId, newProp);

                /// Assert
                pr = null;
                EBS.ProPricer.Data.EntityId pEntityId = new EBS.ProPricer.Data.EntityId(new Guid(newid.Retmsg));
                if (ppc.Workspace.Proposals.Find(pEntityId).HasValue())
                {
                    pr = ppc.Workspace.Proposals.Find(pEntityId).Value();
                }

                Assert.IsFalse(pr == null);

                // delete our test proposal
                pr.Delete();
                Assert.IsFalse(ppc.Workspace.Proposals.Find(pEntityId).HasValue());

                //by name
                template = "F16 TEMPLATE|0";
                version = "0";

                /// Act

                // setup a new proposal
                newProp = new ProposalDto
                {
                    Id = template, ///copy from id
                    Name = "_COPY FROM TEMPLATE_",
                    //  newProp.name = "80 12-00103";
                    Version = string.Empty, // "1";
                    Description = "Created from automated test project. It can safely be deleted.",
                    ParentFolderName = "Test-Dusan"
                };

                newid = prop.Post(TestConstants.InstanceId, newProp);

                /// Assert
                pr = null;
                pEntityId = new EBS.ProPricer.Data.EntityId(new Guid(newid.Retmsg));
                if (ppc.Workspace.Proposals.Find(pEntityId).HasValue())
                {
                    pr = ppc.Workspace.Proposals.Find(pEntityId).Value();
                }

                Assert.IsFalse(pr == null);

                // delete our test proposal
                pr.Delete();
                Assert.IsFalse(ppc.Workspace.Proposals.Find(pEntityId).HasValue());
            }
        }
    }
}