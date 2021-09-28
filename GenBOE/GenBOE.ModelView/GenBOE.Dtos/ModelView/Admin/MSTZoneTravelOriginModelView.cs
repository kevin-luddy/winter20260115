// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Dtos
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel.DataAnnotations;
    using IES.Common;

    public class MSTZoneTravelOriginModelView
    {
        /// <summary>
        /// Default Constructor
        /// </summary>
        public MSTZoneTravelOriginModelView()
        {
            OriginID = -1;
            Origin = string.Empty;
            Site = string.Empty;
            Deleted = false;

            ResourceIDPRZ1 = -1;
            ResourcePRZ1 = string.Empty;

            ResourceIDPRZ2 = -1;
            ResourcePRZ2 = string.Empty;

            ResourceIDPRZ3 = -1;
            ResourcePRZ3 = string.Empty;

            ResourceIDPRZ4 = -1;
            ResourcePRZ4 = string.Empty;

            ResourceIDPRZ5 = -1;
            ResourcePRZ5 = string.Empty;

            ResourceIDPRZ6 = -1;
            ResourcePRZ6 = string.Empty;

            ResourceIDTRZ1 = -1;
            ResourceTRZ1 = string.Empty;

            ResourceIDTRZ2 = -1;
            ResourceTRZ2 = string.Empty;

            ResourceIDTRZ3 = -1;
            ResourceTRZ3 = string.Empty;

            ResourceIDTRZ4 = -1;
            ResourceTRZ4 = string.Empty;

            ResourceIDTRZ5 = -1;
            ResourceTRZ5 = string.Empty;

            ResourceIDTRZ6 = -1;
            ResourceTRZ6 = string.Empty;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="origin">Origin DTO</param>
        /// <param name="resources">Collection of the Origin's (12) Resources</param>
        public MSTZoneTravelOriginModelView(
            MSTZoneTravelOriginDTO origin,
            Collection<MSTZoneTravelResourceDTO> resources
            )
            : this()
        {
            if (origin == null)
            {
                throw new ArgumentNullException(nameof(origin));
            }
            if (resources == null)
            {
                throw new ArgumentNullException(nameof(resources));
            }
            OriginID = origin.OriginID;
            Origin = origin.Origin;
            Site = origin.Site;
            Deleted = origin.Updateable == UpdateType.Deleted;

            ResourceIDPRZ1 = resources[0].ResourceID;
            ResourcePRZ1 = resources[0].Resource;

            ResourceIDPRZ2 = resources[1].ResourceID;
            ResourcePRZ2 = resources[1].Resource;

            ResourceIDPRZ3 = resources[2].ResourceID;
            ResourcePRZ3 = resources[2].Resource;

            ResourceIDPRZ4 = resources[3].ResourceID;
            ResourcePRZ4 = resources[3].Resource;

            ResourceIDPRZ5 = resources[4].ResourceID;
            ResourcePRZ5 = resources[4].Resource;

            ResourceIDPRZ6 = resources[5].ResourceID;
            ResourcePRZ6 = resources[5].Resource;

            ResourceIDTRZ1 = resources[6].ResourceID;
            ResourceTRZ1 = resources[6].Resource;

            ResourceIDTRZ2 = resources[7].ResourceID;
            ResourceTRZ2 = resources[7].Resource;

            ResourceIDTRZ3 = resources[8].ResourceID;
            ResourceTRZ3 = resources[8].Resource;

            ResourceIDTRZ4 = resources[9].ResourceID;
            ResourceTRZ4 = resources[9].Resource;

            ResourceIDTRZ5 = resources[10].ResourceID;
            ResourceTRZ5 = resources[10].Resource;

            ResourceIDTRZ6 = resources[11].ResourceID;
            ResourceTRZ6 = resources[11].Resource;
        }

        /// <summary>
        /// Gets DTO for Origin based on the model view
        /// </summary>
        /// <returns>DTO for Origin</returns>
        public MSTZoneTravelOriginDTO GetOriginDTO()
        {
            MSTZoneTravelOriginDTO toReturn = new MSTZoneTravelOriginDTO();

            toReturn.OriginID = this.OriginID;
            toReturn.Origin = this.Origin;
            toReturn.Site = this.Site;
            toReturn.Updateable = this.Deleted ? UpdateType.Deleted : UpdateType.Upsert;

            return toReturn;
        }

        /// <summary>
        /// ID of the Origin
        /// </summary>
        public int OriginID { get; set; }

        /// <summary>
        /// Name of the Origin
        /// </summary>
        [Required]
        [StringLength(100, ErrorMessage = "A maximum of 100 characters is allowed")]
        public string Origin { get; set; }

        /// <summary>
        /// Site of the Origin
        /// </summary>
        [Required]
        [StringLength(10, ErrorMessage = "A maximum of 10 characters is allowed")]
        public string Site { get; set; }
        
        /// <summary>
        /// Bool noting if Origin is to be deleted
        /// </summary>
        public bool Deleted { get; set; }

        /// <summary>
        /// ID of the Zone 1 Per Diem resource
        /// </summary>
        public int ResourceIDPRZ1 { get; set; }

        /// <summary>
        /// Zone 1 Per Diem resource
        /// </summary>
        [StringLength(20, ErrorMessage = "A maximum of 20 characters is allowed")]
        public string ResourcePRZ1 { get; set; }

        /// <summary>
        /// ID of the Zone 2 Per Diem resource
        /// </summary>
        public int ResourceIDPRZ2 { get; set; }

        /// <summary>
        /// Zone 2 Per Diem resource
        /// </summary>
        [StringLength(20, ErrorMessage = "A maximum of 20 characters is allowed")]
        public string ResourcePRZ2 { get; set; }

        /// <summary>
        /// ID of the Zone 3 Per Diem resource
        /// </summary>
        public int ResourceIDPRZ3 { get; set; }

        /// <summary>
        /// Zone 3 Per Diem resource
        /// </summary>
        [StringLength(20, ErrorMessage = "A maximum of 20 characters is allowed")]
        public string ResourcePRZ3 { get; set; }

        /// <summary>
        /// ID of the Zone 4 Per Diem resource
        /// </summary>
        public int ResourceIDPRZ4 { get; set; }

        /// <summary>
        /// Zone 4 Per Diem resource
        /// </summary>
        [StringLength(20, ErrorMessage = "A maximum of 20 characters is allowed")]
        public string ResourcePRZ4 { get; set; }

        /// <summary>
        /// ID of the Zone 5 Per Diem resource
        /// </summary>
        public int ResourceIDPRZ5 { get; set; }

        /// <summary>
        /// Zone 5 Per Diem resource
        /// </summary>
        [StringLength(20, ErrorMessage = "A maximum of 20 characters is allowed")]
        public string ResourcePRZ5 { get; set; }

        /// <summary>
        /// ID of the Zone 6 Per Diem resource
        /// </summary>
        public int ResourceIDPRZ6 { get; set; }

        /// <summary>
        /// Zone 6 Per Diem resource
        /// </summary>
        [StringLength(20, ErrorMessage = "A maximum of 20 characters is allowed")]
        public string ResourcePRZ6 { get; set; }

        /// <summary>
        /// ID of the Zone 1 Airfare resource
        /// </summary>
        public int ResourceIDTRZ1 { get; set; }

        /// <summary>
        /// ID of the Zone 1 Airfare resource
        /// </summary>
        [StringLength(20, ErrorMessage = "A maximum of 20 characters is allowed")]
        public string ResourceTRZ1 { get; set; }

        /// <summary>
        /// ID of the Zone 2 Airfare resource
        /// </summary>
        public int ResourceIDTRZ2 { get; set; }

        /// <summary>
        /// Zone 2 Airfare resource
        /// </summary>
        [StringLength(20, ErrorMessage = "A maximum of 20 characters is allowed")]
        public string ResourceTRZ2 { get; set; }

        /// <summary>
        /// ID of the Zone 3 Airfare resource
        /// </summary>
        public int ResourceIDTRZ3 { get; set; }

        /// <summary>
        /// Zone 3 Airfare resource
        /// </summary>
        [StringLength(20, ErrorMessage = "A maximum of 20 characters is allowed")]
        public string ResourceTRZ3 { get; set; }

        /// <summary>
        /// ID of the Zone 4 Airfare resource
        /// </summary>
        public int ResourceIDTRZ4 { get; set; }

        /// <summary>
        /// Zone 4 Airfare resource
        /// </summary>
        [StringLength(20, ErrorMessage = "A maximum of 20 characters is allowed")]
        public string ResourceTRZ4 { get; set; }

        /// <summary>
        /// ID of the Zone 5 Airfare resource
        /// </summary>
        public int ResourceIDTRZ5 { get; set; }

        /// <summary>
        /// Zone 5 Airfare resource
        /// </summary>
        [StringLength(20, ErrorMessage = "A maximum of 20 characters is allowed")]
        public string ResourceTRZ5 { get; set; }

        /// <summary>
        /// ID of the Zone 6 Airfare resource
        /// </summary>
        public int ResourceIDTRZ6 { get; set; }

        /// <summary>
        /// Zone 6 Airfare resource
        /// </summary>
        [StringLength(20, ErrorMessage = "A maximum of 20 characters is allowed")]
        public string ResourceTRZ6 { get; set; }

    }
}