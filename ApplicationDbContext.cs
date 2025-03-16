﻿using Career_Tracker_Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Career_Tracker_Backend
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Formation> Formations { get; set; }
        public DbSet<Course> Courses { get; set; } // Ensure this is defined
        public DbSet<Test> Tests { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Job> Jobs { get; set; }
        public DbSet<Inscription> Inscriptions { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<CV> CVs { get; set; }
        public DbSet<Certificat> Certificats { get; set; }
        public DbSet<Badge> Badges { get; set; }
        public DbSet<Category> Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
          /*  modelBuilder.Entity<Formation>()
                .HasMany(f => f.Courses)
                .WithOne()
                .HasForeignKey("FormationId");*/

            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithOne()
                .HasForeignKey<User>(u => u.RoleId);

            modelBuilder.Entity<Formation>()
                .HasMany(f => f.Users)
                .WithMany(u => u.Formations);
            modelBuilder.Entity<Certificat>()
                .HasOne(c => c.User)
                .WithMany(u => u.Certificats)
                .HasForeignKey(c => c.UserId);

            // Supprimez cette relation si elle existe
            // modelBuilder.Entity<Category>()
            //     .HasMany(c => c.Courses)
            //     .WithOne(c => c.Category)
            //     .HasForeignKey(c => c.CategoryId);
        }
    }
    }



