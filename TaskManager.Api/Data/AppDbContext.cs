using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TaskManager.Api.Entity;

namespace TaskManager.Api.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<EmployerProfile> EmployerProfiles => Set<EmployerProfile>();
        public DbSet<EmployerRequest> EmployerRequests => Set<EmployerRequest>();
        public DbSet<JoinToTaskRequest> JoinToTaskRequests => Set<JoinToTaskRequest>();
        public DbSet<TaskModel> Tasks => base.Set<TaskModel>();
        public DbSet<TaskPerformer> TaskPerformers => Set<TaskPerformer>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //User
            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(e => e.Name).IsRequired().HasMaxLength(25);
                //entity.Property(e => e.UserName).IsRequired();

                entity.HasIndex(e => new
                {
                    e.UserName,
                    e.Email,
                    e.NormalizedUserName,
                }).IsUnique();
            });

            modelBuilder.Entity<ApplicationUser>()
                .HasMany<JoinToTaskRequest>()
                .WithOne()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ApplicationUser>()
                .HasOne<EmployerRequest>()
                .WithOne()
                .HasForeignKey<EmployerRequest>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);


            //Employer Profile
            modelBuilder.Entity<EmployerProfile>()
                .HasOne<ApplicationUser>()
                .WithOne()
                .HasForeignKey<EmployerProfile>(e => e.EmployerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EmployerProfile>(entity =>
            {
                entity.HasAlternateKey(e => e.EmployerId);
            });

            //Task Model
            modelBuilder.Entity<TaskModel>(entity =>
            {
                entity.Property(e => e.Title).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
            });

            modelBuilder.Entity<TaskModel>()
                .HasOne<EmployerProfile>()
                .WithMany()
                .HasPrincipalKey(e => e.EmployerId)
                .HasForeignKey(e => e.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);


            //Task Performer
            modelBuilder.Entity<TaskPerformer>(entity =>
            {
                entity.HasIndex(e => new
                {
                    e.PerformerId,
                    e.TaskId
                }).IsUnique();
            });

            //Join to task request

            modelBuilder.Entity<JoinToTaskRequest>(entity =>
            {
                entity.HasIndex(e => new
                {
                    e.UserId,
                    e.TaskId
                }).IsUnique();
            }); 

            modelBuilder.Entity<JoinToTaskRequest>()
                .HasOne<TaskModel>()
                .WithMany()
                .HasForeignKey(e => e.TaskId)
                .OnDelete(DeleteBehavior.Cascade);


            //Employer request
            modelBuilder.Entity<EmployerRequest>(entity =>
            {
                entity.HasIndex(e => new
                {
                    e.UserId
                }).IsUnique();
            });


            modelBuilder.Entity<EmployerRequest>()
                .HasOne<ApplicationUser>()
                .WithOne()
                .HasForeignKey<EmployerRequest>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }

    }
}
