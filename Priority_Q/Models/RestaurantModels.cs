using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;

namespace Priority_Q.Models
{
    public class Restaurant
    {
        public int ID { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The restaurant must have a name!")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Street Address")]
        [StringLength(100, ErrorMessage = "Street address must be specified.")]
        public string StreetAddress { get; set; }

        [Required]
        [Display(Name = "City")]
        [StringLength(100, ErrorMessage = "City must be specified.")]
        public string City { get; set; }

        [Required]
        [Display(Name = "Phone Number")]
        [StringLength(15, ErrorMessage = "Phone number must be specified.")]
        public String PhoneNumber { get; set; }

        [Required]
        [Display(Name = "Opening Hours")]
        public int OpeningHourStart { get; set; }

        [Required]
        public int OpeningHourEnd { get; set; }

        public String UserID { get; set; } //this maps the restaurant to a certain account
    }

    public class RestaurantDBContext : DbContext
    {
        public DbSet<Restaurant> Restaurants { get; set; }
    }

    //===========================================

    public class Table
    {
        public int ID { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "The restaurant id must be a positive number")]
        public int RestaurantId { get; set; }

        [Display(Name = "Table of")]
        [Range(1, 20, ErrorMessage = "Max capacity must be between 1 to 20")]
        public int MaxCapacity { get; set; }

        [Display(Name = "Occupied")]
        public Boolean IsOccupied { get; set; }
    }

    public class TableDBContext : DbContext
    {
        public DbSet<Table> Tables { get; set; }
    }

    //===========================================

    public class Reservation
    {
        public int ID { get; set; }

        public int TableId { get; set; }

        [Required]
        public String DaySlot { get; set; }
        [Required]
        public int HourSlot { get; set; }
        [Required]
        public int MinuteSlot { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "The customer's name must be specified.")]
        public String CustomerName { get; set; } 
    }

    public class ReservationDBContext : DbContext
    {
        public DbSet<Reservation> Reservations { get; set; }
    }

    //===========================================

    public class NewsInfo
    {
        public int ID { get; set; }
        public int RestaurantId { get; set; }

        public String Date { get; set; }

        [Required]
        public String Content { get; set; }
    }

    public class NewsInfoDBContext : DbContext
    {
        public DbSet<NewsInfo> NewsInfos { get; set; }
    }
}