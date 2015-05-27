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
        public int NumTables { get; set; }
    }

    public class RestaurantDBContext : DbContext
    {
        public DbSet<Restaurant> Restaurants { get; set; }
    }

    public class Table
    {
        public int ID { get; set; }
        public int RestaurantId { get; set; }
        public int MaxCapacity { get; set; }
        public Boolean IsOccupied { get; set; }
        public int OccupationStartTime { get; set; }
    }

    public class TableDBContext : DbContext
    {
        public DbSet<Table> Tables { get; set; }
    }
}