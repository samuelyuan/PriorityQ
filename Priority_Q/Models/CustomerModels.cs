using System;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;

namespace Priority_Q.Models
{
    public class Customer
    {
        public int ID { get; set; }

        public int RestaurantID { get; set; } //customer goes to a specific restaurant

        [Required]
        [StringLength(100, ErrorMessage = "The customer must have a name!")]
        public string Name { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Group capacity must be a positive number")]
        [Display(Name = "Group Size")]                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                               
        public int GroupCapacity { get; set; }
    }

    public class CustomerDBContext : DbContext
    {
        public DbSet<Customer> Customers { get; set; }
    }
}