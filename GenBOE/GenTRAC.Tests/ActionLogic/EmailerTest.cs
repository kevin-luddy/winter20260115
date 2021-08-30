// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.ActionLogic
{
    using System;
    using System.Collections.ObjectModel;
    using GenTRAC.ActionLogic.Email;
    using GenTRAC.DataBridge.DTO;
    using IES.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    /// <summary>
    /// Test the emailer class
    /// </summary>
    [TestClass]
    public class EmailerTest
    {
        /// <summary>
        /// Security
        /// </summary>
        private static Mock<ISecurityInformation> secInfo;

        /// <summary>
        /// Data Fetching Scheduler
        /// </summary>
        private static Mock<IDataFetchingScheduler> dataFetchingScheduler;

        /// <summary>
        /// Send an email with the CC list valued
        /// </summary>
        [TestMethod]
        public void SendEmailWithCC()
        {
            this.SendEmail(true);
            ////Assert.IsTrue(this.SendEmail(true));
        }

        /// <summary>
        /// Send an email without the CC list being valued
        /// </summary>
        [TestMethod]
        public void SendEmailWithoutCC()
        {
            this.SendEmail(false);
            ////Assert.IsTrue(this.SendEmail(false));
        }

        /// <summary>
        /// Setup the emailer per input parms and pass along results in out parms
        /// </summary>
        /// <param name="valueCC">create a CC list</param>
        /// <param name="cces">the CC list</param>
        /// <param name="email">the email used</param>
        /// <param name="sut">the system under test (emailer class)</param>
        private static void _SetupEmailer(bool valueCC, out Collection<UserDTO> cces, out EmailContent email, out PtmEmailer sut)
        {
            secInfo = new Mock<ISecurityInformation>();
            dataFetchingScheduler = new Mock<IDataFetchingScheduler>();

            UserDTO[] ccesArray = new UserDTO[] { };

            if (valueCC)
            {
                ccesArray = new UserDTO[] 
                {
                    new UserDTO 
                    { 
                        EmailAddress = "cc1@lmco.com" 
                    },
                    new UserDTO 
                    { 
                        EmailAddress = "cc2@lmco.com" 
                    }
                };
            }

            cces = new Collection<UserDTO>(ccesArray);
            email = new EmailContent
            {
                Body = "generation (trac) TEST - Hello {0}.  How are you?  I am {1}!",
                Subject = "generation (trac) TEST - Hi {0}, let me introduce myself"
            };

            secInfo.Setup(x => x.ActiveUserData).Returns(new IES.Common.UserData() { Email = string.Empty });
            sut = new PtmEmailer(secInfo.Object, dataFetchingScheduler.Object);
        }

        /// <summary>
        /// Send an email, optionally valuing the CC list
        /// </summary>
        /// <param name="inDoCC">true/false if CC list desired</param>
        /// <returns>True if email sent, false if failed</returns>
        private bool SendEmail(bool inDoCC)
        {
            // test sending an email successfully ... 
            // includes using replacement tokens
            Collection<UserDTO> cces;
            EmailContent email;
            PtmEmailer sut;
            _SetupEmailer(inDoCC, out cces, out email, out sut);
            secInfo.Setup(x => x.ActiveUserData).Returns(new IES.Common.UserData() { Email = "Dusan.Palider@lmco.com" });

            // sender will be swapped out b/c resource accounts are equal
            // pass in cclist if specified
            return sut.SendEmail(email,
                           "willbereplaced@lmco.com",
                           inDoCC ? cces : new Collection<UserDTO>(),
                           new string[] { "SubjectName" },
                           new string[] { "BodyName", "Tired" },
                           null);
        }

        /// <summary>
        /// Send an email with invalid tokens, expect exception
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void SendEmailInvalidTokensBody()
        {
            // attempt to send an email with invalid string tokens in the body
            // (should fail before getting very far)
            Collection<UserDTO> cces;
            EmailContent email;
            PtmEmailer sut;
            _SetupEmailer(false, out cces, out email, out sut);
            sut.SendEmail(email,
                           "donotdeliver@lmco.com",
                           null, // no CC's
                           new string[] { "SubjectName" },
                           new string[] { "BodyName", "Tired", "InvalidBodyToken" },
                           null);
        }

        /// <summary>
        /// Send an email with null body tokens, expect exception
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SendEmailNullBodyTokens()
        {
            // attempt to send an email with null string tokens in the body
            // (should fail before getting very far)
            Collection<UserDTO> cces;
            EmailContent email;
            PtmEmailer sut;
            _SetupEmailer(false, out cces, out email, out sut);
            sut.SendEmail(email,
                           "donotdeliver@lmco.com",
                           null, // no CC's
                           new string[] { "SubjectName" },
                           null,
                           null);
        }

        /// <summary>
        /// Send an email with null subject tokens, expect exception
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SendEmailNullSubjectTokens()
        {
            // attempt to send an email with invalid string tokens in the body
            // (should fail before getting very far)
            Collection<UserDTO> cces;
            EmailContent email;
            PtmEmailer sut;
            _SetupEmailer(false, out cces, out email, out sut);
            sut.SendEmail(email,
                           "donotdeliver@lmco.com",
                           null, // no CC's
                           null,
                           new string[] { "BodyName", "Tired" },
                           null);
        }

        /// <summary>
        /// Send an email with invalid recipient
        /// </summary>
        [TestMethod]
        public void SendEmailInvalidRecipient()
        {
            // attempt to send an email with blank recipient
            Collection<UserDTO> cces;
            EmailContent email;
            PtmEmailer sut;
            _SetupEmailer(false, out cces, out email, out sut);
            secInfo.Setup(x => x.ActiveUserData).Returns(new IES.Common.UserData() { Email = "Dusan.Palider@lmco.com" });

            ////bool emailSent = 
            sut.SendEmail(email,
                           string.Empty,
                           null, // no CC's
                           new string[] { "SubjectName" },
                           new string[] { "BodyName", "Tired"},
                           null);
            // Should return false, but because EmailsToCurrentlyLoggedInUser is set to true, the recipient will be replaced and the email will send
            ////Assert.IsTrue(emailSent);
        }

        /// <summary>
        /// Send an email with invalid tokens, expect exception
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void SendEmailInvalidTokensSubject()
        {
            // attempt to send an email with invalid string tokens in the body
            // (should fail before getting very far)
            Collection<UserDTO> cces;
            EmailContent email;
            PtmEmailer sut;
            _SetupEmailer(false, out cces, out email, out sut);

            // no CC's
            sut.SendEmail(email,
                           "donotdeliver@lmco.com",
                           null,
                           new string[] { "SubjectName", "InvalidSubjectToken" },
                           new string[] { "BodyName", "Tired" },
                           null);
        }

        /// <summary>
        /// Tests the op_Equality method
        /// </summary>
        [TestMethod]
        public void CheckEmailContentEquality()
        {
            EmailContent emailOne = new EmailContent
            {
                Body = "generation (trac) TEST - Hello {0}.  How are you?  I am {1}!",
                Subject = "generation (trac) TEST - Hi {0}, let me introduce myself"
            };

            EmailContent emailtwo = new EmailContent
            {
                Body = "generation (trac) TEST - Hello {0}.  How are you?  I am {1}!",
                Subject = "generation (trac) TEST - Hi {0}, let me introduce myself"
            };

            Assert.IsTrue(emailOne == emailtwo);
        }

        /// <summary>
        /// Tests the op_InEquality method
        /// </summary>
        [TestMethod]
        public void CheckEmailContentInEquality()
        {
            EmailContent emailOne = new EmailContent
            {
                Body = "generation (trac) TEST - Hello {0}.  How are you?  I am {1}!",
                Subject = "generation (trac) TEST - Hi {0}, let me introduce myself"
            };

            EmailContent emailtwo = new EmailContent
            {
                Body = "generation (trac) - Hello {0}.  How are you?  I am {1}!",
                Subject = "generation (trac) - Hi {0}, let me introduce myself"
            };

            Assert.IsTrue(emailOne != emailtwo);
        }
    }
}
