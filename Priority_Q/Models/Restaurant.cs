using System;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;

namespace Priority_Q.Models
{
    public class Restaurant
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "The number of tables must be a positive number")]
        public int NumTables { get; set; }
    }

    public class RestaurantDBContext : DbContext
    {
        public DbSet<Restaurant> Restaurants { get; set; }
    }

    public class Table
    {
        public int ID { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "The restaurant id must be a positive number")]
        public int RestaurantId { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Max capacity must be a positive number")]
        public int MaxCapacity { get; set; }

        public Boolean IsOccupied { get; set; }
        public int OccupationStartTime { get; set; }
    }

    public class TableDBContext : DbContext
    {
        public DbSet<Table> Tables { get; set; }
    }
}