namespace UnitTestProject
{
    using System.Collections.Generic;
    using APTSPropricerApi.Connection;
    using APTSPropricerApi.Controllers;
    using APTSPropricerApi.DTOs;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class TasksControllerTest
    {
        //[TestMethod]
        //public void Get_Tasks()
        //{
        //    using (ProposalsController controller = new ProposalsController())
        //    {

        //        /// Act
        //        ICollection<ProposalDto> proposals = controller.Get(TestConstants.InstanceId);

        //        /// Assert

        //        List<string> prfolders = new List<string>();
        //        List<string> prid = new List<string>();

        //        using (IEnumerator<ProposalDto> enumerator = proposals.GetEnumerator())
        //        {
        //            while (enumerator.MoveNext())
        //            {
        //                if (!prfolders.Contains(enumerator.Current.ParentFolderName) &&
        //                    (enumerator.Current.ParentFolderName.StartsWith("ADP") ||
        //                     enumerator.Current.ParentFolderName.StartsWith("C5") ||
        //                     enumerator.Current.ParentFolderName.StartsWith("C130") ||
        //                     enumerator.Current.ParentFolderName.StartsWith("F16") ||
        //                     enumerator.Current.ParentFolderName.StartsWith("F22") ||
        //                     enumerator.Current.ParentFolderName.StartsWith("F35") ||
        //                     enumerator.Current.ParentFolderName.StartsWith("P3") ||
        //                     enumerator.Current.ParentFolderName.StartsWith("U2") ||
        //                     enumerator.Current.ParentFolderName.StartsWith("Test")
        //                    ))
        //                {
        //                    prid.Add(enumerator.Current.Id);
        //                    prfolders.Add(enumerator.Current.ParentFolderName);
        //                }
        //            }
        //        }

        //        Assert.IsTrue(prfolders.Count > 0);

        //        /// Act
        //        using (TasksController tcontroller = new TasksController())
        //        {
        //            using (IEnumerator<string> enumerator = prid.GetEnumerator())
        //            {
        //                while (enumerator.MoveNext())
        //                {
        //                    IEnumerable<TaskDto> tasks = tcontroller.Get(TestConstants.InstanceId, enumerator.Current);
        //                }

        //                //                       Assert.IsTrue(count > 0);
        //            }
        //        }
        //    }
        //}

        [TestMethod]
        public void Get_Tasks_Prop_ID()
        {
            string prid = "65e1eb88-ca1a-e711-82a5-847beb323d31";

            using (TasksController tcontroller = new TasksController())
            {

                /// Act

                IEnumerable<TaskDto> tasks = tcontroller.Get(TestConstants.InstanceId, prid);

                /// Assert
                int count = 0;
                using (IEnumerator<TaskDto> enumerator = tasks.GetEnumerator())
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
        public void Post_C130_Spares_Tasks()
        {
            string fromprid = "a42e27ad-7dfe-e411-9431-6cc2177e33b0Direct"; //C130 Spare M&B Parts, Ver. 0   C130 folder; Direct resoure assignments
            string toprid = "fbe33b2d-98eb-e511-ac9c-005056c00008"; //_Auto_C130 Spares, Ver. 0   TestMike folder
            
            /// Act
            using (TasksController tcontroller = new TasksController())
            {
                IEnumerable<TaskDto> tasks = tcontroller.Get(TestConstants.InstanceId, fromprid);

                /// Assert
                int fromcount = 0;
                using (IEnumerator<TaskDto> enumerator = tasks.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        fromcount++;
                    }
                }

                Assert.IsTrue(fromcount > 0);

                ProposalDto proptasks = new ProposalDto
                {
                    Id = toprid,
                    Tasks = tasks
                };
                tcontroller.Post(TestConstants.InstanceId, proptasks);

                /// Assert
                tasks = tcontroller.Get(TestConstants.InstanceId, toprid);
                int tocount = 0;
                using (IEnumerator<TaskDto> enumerator = tasks.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        tocount++;
                    }
                }

                Assert.IsTrue(tocount == fromcount);
            }
        }

        // ToDo: Dusan - we do not have the data to be able to do this, not sure about the goal..
        // [TestMethod]
        public void Post_F35_Tasks()
        {
            string fromprid = "0c0fb3ff-dd12-11e3-8362-2477031c37c4Direct"; //LRIP 8 , Ver. 4   F35 folder Test Lisa Data; Direct resoure assignments
            string toprid = "78869dfa-a4ef-e511-a6ef-005056c00008"; //_Auto_F35_LRIP8, Ver. 0   Test-Dusan folder
                       
            /// Act
            using (TasksController tcontroller = new TasksController())
            {
                IEnumerable<TaskDto> tasks = tcontroller.Get(TestConstants.InstanceId, fromprid);

                /// Assert
                int fromcount = 0;
                using (IEnumerator<TaskDto> enumerator = tasks.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        fromcount++;
                    }
                }

                Assert.IsTrue(fromcount > 0);

                ProposalDto proptasks = new ProposalDto
                {
                    Id = toprid,
                    Tasks = tasks
                };
                tcontroller.Post(TestConstants.InstanceId, proptasks);

                /// Assert
                tasks = tcontroller.Get(TestConstants.InstanceId, toprid);
                int tocount = 0;
                using (IEnumerator<TaskDto> enumerator = tasks.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        tocount++;
                    }
                }

                Assert.IsTrue(tocount == fromcount);
            }
        }

        // ToDo: Dusan - we do not have the data to be able to do this, not sure about the goal..
        // [TestMethod]
        public void Post_Travel_Tasks()
        {
            string fromprid = "abe39606-86fa-e411-ace1-6cc2177e33b0Direct"; //Travel Test - RSRC Test Lisa Proposals; Direct resoure assignments
            string toprid = "da941043-1ff1-e511-90a4-005056c00008"; //_Auto_Travel, Ver. 0   Test-Dusan folder
            // string toprid = "47a6e18d-6d61-e611-92e4-005056ae34ae";
            
            /// Act
            using (TasksController tcontroller = new TasksController())
            {
                IEnumerable<TaskDto> tasks = tcontroller.Get(TestConstants.InstanceId, fromprid);

                /// Assert
                int fromcount = 0;
                using (IEnumerator<TaskDto> enumerator = tasks.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        fromcount++;
                    }
                }

                Assert.IsTrue(fromcount > 0);

                ProposalDto proptasks = new ProposalDto
                {
                    Id = toprid,
                    Tasks = tasks
                };
                tcontroller.Post(TestConstants.InstanceId, proptasks);

                /// Assert
                tasks = tcontroller.Get(TestConstants.InstanceId, toprid);
                int tocount = 0;
                using (IEnumerator<TaskDto> enumerator = tasks.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        tocount++;
                    }
                }

                Assert.IsTrue(tocount == fromcount);
            }
        }

        // ToDo: Dusan - we do not have the data to be able to do this, not sure about the goal..
        // [TestMethod]
        public void Delete_C130_Spares_Tasks()
        {
            string toprid = "fbe33b2d-98eb-e511-ac9c-005056c00008"; //_Auto_C130 Spares, Ver. 0
            
            /// Act
            using (TasksController tcontroller = new TasksController())
            {

                tcontroller.Delete(TestConstants.InstanceId, toprid);

                IEnumerable<TaskDto> tasks = tcontroller.Get(TestConstants.InstanceId, toprid);

                /// Assert
                int tocount = 0;
                using (IEnumerator<TaskDto> enumerator = tasks.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        tocount++;
                    }
                }

                Assert.IsTrue(tocount == 0);
            }
        }

        // ToDo: Dusan - we do not have the data to be able to do this, not sure about the goal..
        // [TestMethod]
        public void Debug_Post_Tasks()
        {
            string fromprid = "0b08382c-1771-e611-badd-005056ae34ae";
            string toprid = "82a15833-2980-e611-b82b-0205857feb80";

            /// Act
            using (TasksController tcontroller = new TasksController())
            {
                IEnumerable<TaskDto> tasks = tcontroller.Get(TestConstants.InstanceId, fromprid);

                /// Assert
                int fromcount = 0;
                using (IEnumerator<TaskDto> enumerator = tasks.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        fromcount++;
                    }
                }

                Assert.IsTrue(fromcount > 0);

                ProposalDto proptasks = new ProposalDto
                {
                    Id = toprid,
                    Tasks = tasks
                };
                tcontroller.Post(TestConstants.InstanceId, proptasks);

                /// Assert
                tasks = tcontroller.Get(TestConstants.InstanceId, toprid);
                int tocount = 0;
                using (IEnumerator<TaskDto> enumerator = tasks.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        tocount++;
                    }
                }

                Assert.IsTrue(tocount == fromcount);
            }
        }

        // ToDo: Dusan - we do not have the data to be able to do this, not sure about the goal..
        // [TestMethod]
        public void Delete_F35_Tasks()
        {
            string toprid = "78869dfa-a4ef-e511-a6ef-005056c00008"; //_Auto_F35_LRIP8, Ver. 0   Test-Dusan folder
            
            /// Act
            using (TasksController tcontroller = new TasksController())
            {

                tcontroller.Delete(TestConstants.InstanceId, toprid);

                IEnumerable<TaskDto> tasks = tcontroller.Get(TestConstants.InstanceId, toprid);

                /// Assert
                int tocount = 0;
                using (IEnumerator<TaskDto> enumerator = tasks.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        tocount++;
                    }
                }

                Assert.IsTrue(tocount == 0);
            }
        }

        // ToDo: Dusan - we do not have the data to be able to do this, not sure about the goal..
        // [TestMethod]
        public void Delete_Travel_Tasks()
        {
            string toprid = "da941043-1ff1-e511-90a4-005056c00008"; //_Auto_Travel, Ver. 0
            
            /// Act
            using (TasksController tcontroller = new TasksController())
            {

                tcontroller.Delete(TestConstants.InstanceId, toprid);

                IEnumerable<TaskDto> tasks = tcontroller.Get(TestConstants.InstanceId, toprid);

                /// Assert
                int tocount = 0;
                using (IEnumerator<TaskDto> enumerator = tasks.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        tocount++;
                    }
                }

                Assert.IsTrue(tocount == 0);
            }
        }
    }
}