using Microsoft.EntityFrameworkCore;
using WebApp.Entities;

namespace WebApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets for each entity
        public DbSet<Project> Projects { get; set; } = null!;
        public DbSet<Member> Members { get; set; } = null!;
        public DbSet<Client> Clients { get; set; } = null!;
        public DbSet<Status> Statuses { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Project → Client (many-to-one)
            modelBuilder.Entity<Project>()
                .HasOne(p => p.Client)
                .WithMany(c => c.Projects)
                .HasForeignKey(p => p.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            // Project → Status (many-to-one)
            modelBuilder.Entity<Project>()
                .HasOne(p => p.Status)
                .WithMany() // no need to reference back from Status
                .HasForeignKey(p => p.StatusId)
                .OnDelete(DeleteBehavior.Restrict); // or Cascade, based on how you want deletes to behave

            // User email should be unique
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Seed initial statuses
            modelBuilder.Entity<Status>().HasData(
                new Status { Id = new Guid("11111111-1111-1111-1111-111111111111"), Name = "Not Started" },
                new Status { Id = new Guid("22222222-2222-2222-2222-222222222222"), Name = "In Progress" },
                new Status { Id = new Guid("33333333-3333-3333-3333-333333333333"), Name = "Completed" },
                new Status { Id = new Guid("44444444-4444-4444-4444-444444444444"), Name = "On Hold" }
            );
        }
    }
}
