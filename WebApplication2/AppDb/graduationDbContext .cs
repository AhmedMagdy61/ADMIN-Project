using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;
using WebApplication2.Models;
namespace WebApplication2.AppDb
{
  

    public class graduationDbContext : DbContext
    {
        public graduationDbContext(DbContextOptions<graduationDbContext> options) : base(options) { }

        public DbSet<Admin> Admins { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Lecture> Lectures { get; set; }
        public DbSet<Section> Sections { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Optional: Define relationships explicitly if needed
            modelBuilder.Entity<Admin>()
                .HasMany(a => a.Courses)
                .WithOne(c => c.Admin)
                .HasForeignKey(c => c.AdminId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Admin>()
                .HasMany(a => a.Lectures)
                .WithOne(l => l.Admin)
                .HasForeignKey(l => l.AdminId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Admin>()
                .HasMany(a => a.Sections)
                .WithOne(s => s.Admin)
                .HasForeignKey(s => s.AdminId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

}
