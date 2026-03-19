using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TaskManager.Api.Model;

namespace TaskManager.Api.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<TaskItem> Tasks => Set<TaskItem>();
        public DbSet<EmployerRequest> EmployerRequests => Set<EmployerRequest>();
        public DbSet<EmployerProfile> EmployerProfiles => Set<EmployerProfile>();
        public DbSet<JoinToTaskRequest> JoinToTaskRequests => Set<JoinToTaskRequest>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(e => e.Name).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Age);
            });

            modelBuilder.Entity<TaskItem>(entity =>
            {
                entity.Property(e => e.Title).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
            });
            modelBuilder.Entity<TaskItem>()
                .HasMany(t => t.Performers)
                .WithMany(u => u.PerformerTasks)
                .UsingEntity(j => j.ToTable("TaskPerformers"));

            modelBuilder.Entity<TaskItem>()
                .HasOne(t => t.Owner)
                .WithMany(u => u.OwnerTasks)
                .HasForeignKey(t => t.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);
        }

    }
}
