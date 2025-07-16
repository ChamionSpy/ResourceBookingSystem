using Microsoft.EntityFrameworkCore;
using ResourceBookingSystem.Models;

namespace ResourceBookingSystem.Data
{
    // This class connects the resource booking system to the database.
    // It allows reading, adding, updating and deleting data using c# instead of SQL
    public class ApplicationDbContext : DbContext
    {
        // This is the constructor. It receives settings like the database connection
        // and passes them to the base class (DbContext).
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        // This represents the 'Resource' table in the database.
        // It enables CRUD operations on the resource entities.
        public DbSet<Resource> Resource { get; set; }

        // This represents the 'Bookings' table in the database.
        // It enables CRUD operations on the booking entities.
        public DbSet<Booking> Bookings { get; set; }
    }
}
