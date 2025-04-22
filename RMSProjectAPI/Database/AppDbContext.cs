using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RMSProjectAPI.Database.Entity;

namespace RMSProjectAPI.Database
{
    public class AppDbContext : IdentityDbContext<User,IdentityRole<Guid>, Guid>
    {
       
        public DbSet<Chat> Chats { get; set; }
        
        public DbSet<Message> Messages { get; set; }
       
        public DbSet<PhoneNumber> PhoneNumbers { get; set; }
     

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Disable cascade delete globally to prevent multiple cascade paths


          
            foreach (var foreignKey in modelBuilder.Model.GetEntityTypes()
                .SelectMany(e => e.GetForeignKeys()))
            {
                foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
            }

        }

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    base.OnConfiguring(optionsBuilder);
        //}
    }
}
