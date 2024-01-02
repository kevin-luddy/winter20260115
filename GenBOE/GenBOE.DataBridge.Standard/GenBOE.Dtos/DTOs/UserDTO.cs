using System;
using System.Diagnostics.CodeAnalysis;

namespace GenBOE.Dtos
{
    /// <summary>
    /// Encapsulate a user in the system
    /// </summary>
    [ExcludeFromCodeCoverage]
    [Serializable()]
    public class UserDTO
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public UserDTO()
        {
            UserID = int.MinValue;
            NTID = string.Empty;
            DisplayName = string.Empty;
            EmailAddress = string.Empty;
            PhoneNumber = string.Empty;
            FirstName = string.Empty;
            MiddleName = string.Empty;
            LastName = string.Empty;
        }

        public string PhoneNumber { get; set; }
        public string EmailAddress { get; set; }
        public int UserID { get; set; }
        public string NTID { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public DateTime UpdateDate { get; set; }

        public string DisplayName { get; set; }

        /// <summary>
        /// Is the Person US based (vs international)
        /// 
        /// AD Property -> lmcUSAPersonIndicator -> values: Y / N
        /// </summary>
        public bool? IsUsPerson { get; set; }

        /// <summary>
        /// Is the Employee a subcontractor (vs LM employee)
        /// 
        /// AD Property -> employeeType -> values: G sub, E is emp
        /// </summary>
        public bool? IsSubcontractor { get; set; }

        public override string ToString()
        {
            return string.Format("userid {0}, displayname {1}, ntid {2}, emailaddress {3}, phonenumber {4}, firstname {5}, middlename {6}, lastname {7}",
                                UserID,
                                DisplayName,
                                NTID,
                                EmailAddress,
                                PhoneNumber,
                                FirstName,
                                MiddleName,
                                LastName);
        }
    }
}
