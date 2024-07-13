using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.data
{
    public class ApplicationDBContext(DbContextOptions options) : DbContext(options)
    {
        //  Properties
        public DbSet<User> Users { get; set; }
        public DbSet<Sample> Samples { get; set; }
        public DbSet<Collection> Collections { get; set; }
    }
}