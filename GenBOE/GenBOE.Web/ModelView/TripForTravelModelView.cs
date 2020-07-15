// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using GenBOE.ActionLogic.ModelView;
using GenBOE.Dtos;
using IES.Common;

namespace GenBOE.Web.ModelView
{
    public class TripForTravelModelView : PersistedDataModelView, IComparable
    {
        public TripForTravelModelView()
        {
            TripID = -1;
            MiscTravelRateID = -1;
            Mode = string.Empty;

            DepartureLocationID = -1;
            DepartureLocationName = string.Empty;
            DepartureLocationCode = string.Empty;

            DestinationLocationID = -1;
            DestinationLocationName = string.Empty;
            DestinationLocationCode = string.Empty;

            PerDiemID = -1;
            PerDiemDestination = string.Empty;
            Qualification = string.Empty;
            HotelRate = -1;
            MIERate = -1;
            RentalCar = -1;
            Fare = -1;
            RTMIles = -1;
            PerDiemNotes = string.Empty;

            TripLastUsed = string.Empty;
            FareLastUpdated = string.Empty;
            PerDiemLastUpdated = string.Empty;

            PerDiemUpdateDate = DateTime.MinValue;

            TripCount = 0;

            Deleted = false;
        }

        public TripForTravelModelView(
            TripDTO inTripDTO,
            MiscTravelRateDTO inIMiscTravelRateDTO,
            UserDTO inFareUpdatingUser,
            PerDiemDTO inPerDiemDto,
            LocationDTO inDepartureLocationDto,
            LocationDTO inDesintationLocationDto)
            : this()
        {
            if (inTripDTO == null)
            {
                throw new ArgumentNullException(nameof(inTripDTO));
            }
            if (inPerDiemDto == null)
            {
                throw new ArgumentNullException(nameof(inPerDiemDto));
            }

            TripID = inTripDTO.TripID;

            MiscTravelRateID = inTripDTO.MiscTravelRateID;



            if (inIMiscTravelRateDTO != null)
            {
                MiscRate = inIMiscTravelRateDTO.MiscTravelRate;
                Mode = inIMiscTravelRateDTO.MiscTravelRateMode;
            }

           
            DepartureLocationID = inTripDTO.DepartureLocationID;
            if (inDepartureLocationDto != null)
            {
                DepartureLocationName = inDepartureLocationDto.LocationName;
                DepartureLocationCode = inTripDTO.DepartureLocationCode;
            }

            DestinationLocationID = inTripDTO.DestinationLocationID;

            if (inDesintationLocationDto != null)
            {
                DestinationLocationName = inDesintationLocationDto.LocationName;
                DestinationLocationCode = inTripDTO.DestinationLocationCode;
            }

            Qualification = inPerDiemDto.Qualification;

            PerDiemID = inTripDTO.PerDiemID;
            PerDiemDestination = inPerDiemDto.PerDiemDestination;
            HotelRate = inPerDiemDto.HotelRate;
            TripLastUsed = inTripDTO.LastUsedDate.HasValue ? inTripDTO.LastUsedDate.Value.ToString("MM/dd/yyyy") : string.Empty;
            MIERate = inPerDiemDto.MIERate;
            RentalCar = inTripDTO.RentalCarRate;
            Fare = inTripDTO.Fare;
            RTMIles = inTripDTO.RTMiles;
            PerDiemNotes = inPerDiemDto.PerDiemNotes;
            PerDiemUpdateDate = inPerDiemDto.UpdateDate;

            if (inTripDTO.FareLastUpdatedDate.HasValue && inFareUpdatingUser != null)
            {
                var lastUpdatedByUserName = inFareUpdatingUser.DisplayName;
                FareLastUpdated = lastUpdatedByUserName + " - " + inTripDTO.FareLastUpdatedDate.Value.ToString("MM/dd/yyyy");
            }

            if (inPerDiemDto.LastUpdatedBy.HasValue && inFareUpdatingUser != null)
            {
                var lastUpdatedByUserName = inFareUpdatingUser.DisplayName;
                PerDiemLastUpdated = lastUpdatedByUserName + " - " + inPerDiemDto.PerDiemLastUpdatedDate.ToString("MM/dd/yyyy");
            }

            inUse = inTripDTO.InUse;
            TripCount = inTripDTO.TripCount;

            UpdateDate = inTripDTO.UpdateDate;
            Deleted = inTripDTO.Updateable == UpdateType.Deleted;
        }

        public LocationDTO GetDepartureLocationDTO()
        {
            var toReturn = new LocationDTO();

            //toReturn.LocationCode = this.DepartureLocationCode;
            toReturn.Id = this.DepartureLocationID;
            toReturn.LocationName = this.DepartureLocationName;

            return toReturn;
        }

        public LocationDTO GetDestinationLocationDTO()
        {
            LocationDTO toReturn = new LocationDTO(); ;

            toReturn.Id = this.DestinationLocationID;
            toReturn.LocationName = this.DestinationLocationName;

            return toReturn;
        }

        public PerDiemDTO GetPerDiemDTO()
        {
            var toReturn = new PerDiemDTO();

            toReturn.HotelRate = this.HotelRate;
            toReturn.MIERate = this.MIERate;
            toReturn.PerDiemDestination = this.PerDiemDestination;
            toReturn.Id = this.PerDiemID;
            toReturn.PerDiemNotes = this.PerDiemNotes;
            toReturn.Qualification = this.Qualification;
            toReturn.Updateable = UpdateType.Upsert;
            toReturn.UpdateDate = new DateTime(long.Parse(this.PerDiemUpdateDateLong));

            return toReturn;
        }

        public TripDTO GetTripDTO()
        {
            var toReturn = new TripDTO();

            toReturn.DepartureLocationID = this.DepartureLocationID;
            toReturn.DestinationLocationID = this.DestinationLocationID;
            toReturn.Fare = this.Fare;
            toReturn.InUse = this.inUse;
            toReturn.MiscTravelRateID = this.MiscTravelRateID;
            toReturn.PerDiemID = this.PerDiemID;
            toReturn.RTMiles = this.RTMIles;
            toReturn.TripID = this.TripID;
            toReturn.TripCount = this.TripCount;
            toReturn.Updateable = this.Deleted ? UpdateType.Deleted : UpdateType.Upsert;
            toReturn.UpdateDate = new DateTime(this.UpdateDate.Ticks);
            toReturn.RentalCarRate = this.RentalCar;
            toReturn.DepartureLocationCode = this.DepartureLocationCode;
            toReturn.DestinationLocationCode = this.DestinationLocationCode;

            return toReturn;
        }

        public int TripID { get; set; }

        [Required]
        [DisplayName("Mode")]
        public int MiscTravelRateID { get; set; }

        [StringLength(25, ErrorMessage = "A maximum of 25 characters are allowed")]
        public string Mode { get; set; }

        // Departure location can either be an exisiting departure or a brand new one if the user starts to type
        public int DepartureLocationID { get; set; }

        [Required]
        [StringLength(40, ErrorMessage = "A maximum of 40 characters are allowed")]
        [DisplayName("Departure Loc")]
        public string DepartureLocationName { get; set; }

        [StringLength(10, ErrorMessage = "A maximum of 10 characters are allowed")]
        [DisplayName("Departure Loc Code")]
        public string DepartureLocationCode { get; set; }

        // Desintation location can either be an exisiting departure or a brand new one if the user starts to type
        public int DestinationLocationID { get; set; }

        [Required]
        [StringLength(40, ErrorMessage = "A maximum of 40 characters are allowed")]
        [DisplayName("Destination")]
        public string DestinationLocationName { get; set; }

        [StringLength(10, ErrorMessage = "A maximum of 10 characters are allowed")]
        [DisplayName("Destination Code")]
        public string DestinationLocationCode { get; set; }

        public int PerDiemID { get; set; }

        [Required]
        [StringLength(40, ErrorMessage = "A maximum of 40 characters are allowed")]
        [DisplayName("Per Diem Dest")]
        public string PerDiemDestination { get; set; }

        [StringLength(40, ErrorMessage = "A maximum of 40 characters are allowed")]
        public string Qualification { get; set; }

        [Required]
        [RegularExpression("^[0-9]{1,6}(\\.[0-9]{1,2})?$", ErrorMessage = "Fare must be a valid and reasonable monetary figure.")]
        public decimal Fare { get; set; }

        [Required]
        [RegularExpression("^[0-9]{1,5}(\\.[0-9]{1,2})?$", ErrorMessage = "Hotel Rate must be a valid and reasonable monetary figure.")]
        [DisplayName("Hotel")]
        public decimal HotelRate { get; set; }

        [Required]
        [RegularExpression("^[0-9]{1,4}(\\.[0-9]{1,2})?$", ErrorMessage = "MIE Rate must be a valid and reasonable monetary figure.")]
        [DisplayName("MIE Rate")]
        public decimal MIERate { get; set; }

        [StringLength(100, ErrorMessage = "A maximum of 100 characters are allowed")]
        [DisplayName("Per Diem Notes")]
        public string PerDiemNotes { get; set; }

        [Required]
        [RegularExpression("^[0-9]{1,4}(\\.[0-9]{1,2})?$", ErrorMessage = "Rental Car price must be a valid and reasonable monetary figure.")]
        [DisplayName("Rental Car")]
        public decimal RentalCar { get; set; }

        [Required]
        [RegularExpression("^[0-9]{1,5}$", ErrorMessage = "R/T Miles must be a valid whole number, reasonable for mileage.")]
        [DisplayName("R/T Miles")]
        public int RTMIles { get; set; }

        public decimal MiscRate { get; set; }

        public string FareLastUpdated { get; set; }
        public string PerDiemLastUpdated { get; set; }
        public string TripLastUsed { get; set; }

        public int TripCount { get; set; }

        public bool inUse { get; set; }
        public bool Deleted { get; set; }

        public DateTime PerDiemUpdateDate { get; set; }

        public string PerDiemUpdateDateLong
        {
            get
            {
                return this.PerDiemUpdateDate.Ticks.ToString();
            }
            set
            {
                this.PerDiemUpdateDate = new DateTime(long.Parse(value));
            }
        }

        public int CompareTo(object obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            if (obj.GetType() == typeof(TripForTravelModelView))
            {
                return CompareTo((TripForTravelModelView)obj);
            }

            else
            {
                throw new NotImplementedException(
                    String.Format(
                        "Cannot compare to {0} type.",
                        obj.GetType()));
            }
        }

        // Custom comparer to implement desired multi-sort order
        private int CompareTo(TripForTravelModelView otherModelView)
        {
            int toReturn = this.DepartureLocationName.CompareTo(otherModelView.DepartureLocationName);

            if (toReturn == 0)
            {
                toReturn = this.PerDiemDestination.CompareTo(otherModelView.PerDiemDestination);
            }

            if (toReturn == 0)
            {
                toReturn = this.Mode.CompareTo(otherModelView.Mode);
            }

            if (toReturn == 0 && this.Qualification != null)
            {
                toReturn = this.Qualification.CompareTo(otherModelView.Qualification);
            }

            if (toReturn == 0)
            {
                toReturn = this.Fare.CompareTo(otherModelView.Fare);
            }

            if (toReturn == 0)
            {
                toReturn = this.HotelRate.CompareTo(otherModelView.HotelRate);
            }

            if (toReturn == 0)
            {
                toReturn = this.MIERate.CompareTo(otherModelView.MIERate);
            }

            if (toReturn == 0)
            {
                toReturn = this.RentalCar.CompareTo(otherModelView.RentalCar);
            }

            if (toReturn == 0)
            {
                toReturn = this.MiscRate.CompareTo(otherModelView.MiscRate);
            }

            return toReturn;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            return this.CompareTo(obj) == 0;
        }

        public static bool operator ==(TripForTravelModelView first, TripForTravelModelView second)
        {
            if ((object)first == null)
            {
                if ((object)second == null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if ((object)second == null)
            {
                return false;
            }

            return first.CompareTo(second) == 0;
        }

        public static bool operator !=(TripForTravelModelView first, TripForTravelModelView second)
        {
            if (first == null)
            {
                if (second == null)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else if (second == null)
            {
                return true;
            }

            return first.CompareTo(second) != 0;
        }

        public static bool operator <(TripForTravelModelView first, TripForTravelModelView second)
        {
            if (first == null)
            {
                if (second == null)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else if (second == null)
            {
                return false;
            }

            return first.CompareTo(second) < 0;
        }

        public static bool operator >(TripForTravelModelView first, TripForTravelModelView second)
        {
            if (first == null)
            {
                return false;
            }
            else if (second == null)
            {
                return true;
            }

            return first.CompareTo(second) > 0;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
