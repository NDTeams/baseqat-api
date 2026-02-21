using Baseqat.EF.Consts;
using Baseqat.EF.Models;
using Baseqat.EF.Models.Auth;
using Baseqat.EF.Models.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;

namespace Baseqat.EF.DATA
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            modelBuilder.Entity<IdentityRole>().HasData(
                new IdentityRole
                {
                    Id = "f0cfb77b-0890-4124-8598-6ff387e3d64e",
                    ConcurrencyStamp = "ed39a736-39f1-4d3e-b68b-afa868b31682",
                    Name = IntitalRoles.SuperAdmin,
                    NormalizedName = IntitalRoles.SuperAdmin.ToUpper()
                },
                new IdentityRole
                {
                    Id = "b6b1b6fd-ca5f-4efc-ae56-48bd128a9df7",
                    ConcurrencyStamp = "4dab779b-3fdc-477c-b0c3-5ee7ce7ca187",
                    Name = IntitalRoles.Admin,
                    NormalizedName = IntitalRoles.Admin.ToUpper()
                },
                new IdentityRole
                {
                    Id = "a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d",
                    ConcurrencyStamp = "e3842702-39ac-465b-9857-1f06a0458fae",
                    Name = IntitalRoles.BaseqatEmployee,
                    NormalizedName = IntitalRoles.BaseqatEmployee.ToUpper()
                },
                new IdentityRole
                {
                    Id = "c2d3e4f5-a6b7-4c8d-9e0f-1a2b3c4d5e6f",
                    ConcurrencyStamp = "914a769b-29be-4662-8a56-6c43afdc46f9",
                    Name = IntitalRoles.Trainer,
                    NormalizedName = IntitalRoles.Trainer.ToUpper()
                },
                new IdentityRole
                {
                    Id = "d3e4f5a6-b7c8-4d9e-0f1a-2b3c4d5e6f7a",
                    ConcurrencyStamp = "a77220f3-6932-4c98-8953-955905d53612",
                    Name = IntitalRoles.Consultant,
                    NormalizedName = IntitalRoles.Consultant.ToUpper()
                },
                new IdentityRole
                {
                    Id = "e4f5a6b7-c8d9-4e0f-1a2b-3c4d5e6f7a8b",
                    ConcurrencyStamp = "005eaacd-078a-4010-89da-338939a5302f",
                    Name = IntitalRoles.Client,
                    NormalizedName = IntitalRoles.Client.ToUpper()
                }
            );

            //addRelations
            modelBuilder.Entity<Privileges>().HasMany(e => e.Privileges_UserBased).WithOne(e => e.Privileges).HasForeignKey(e => e.PrivilegesId).OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<Privileges>().HasMany(e => e.Privileges_RoleBased).WithOne(e => e.Privileges).HasForeignKey(e => e.PrivilegesId).OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<CourseInstructor>()
                .HasKey(ci => new { ci.CourseId, ci.InstructorId });

            modelBuilder.Entity<CourseInstructor>()
                .HasOne(ci => ci.Course)
                .WithMany(c => c.CourseInstructors)
                .HasForeignKey(ci => ci.CourseId)
                .OnDelete(DeleteBehavior.Restrict); // أو NoAction

            modelBuilder.Entity<CourseInstructor>()
                .HasOne(ci => ci.Instructor)
                .WithMany(i => i.CourseInstructors)
                .HasForeignKey(ci => ci.InstructorId)
                .OnDelete(DeleteBehavior.Restrict); // أو NoAction

            modelBuilder.Entity<StudentReview>()
                .HasOne(sr => sr.Instructor)
                .WithMany(i => i.StudentReviews)
                .HasForeignKey(sr => sr.InstructorId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ContactRequest>()
                .Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(50);

            modelBuilder.Entity<ContactRequest>()
                .Property(x => x.PreferredReplyChannel)
                .HasConversion<string>()
                .HasMaxLength(50);

            modelBuilder.Entity<ContactRequest>()
                .Property(x => x.RepliedVia)
                .HasConversion<string>()
                .HasMaxLength(50);

            modelBuilder.Entity<Instructor>()
                .Property(x => x.InfoUpdateRequestStatus)
                .HasConversion<string>()
                .HasMaxLength(50);


        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            optionsBuilder.ConfigureWarnings(warnings =>
                warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        }

        //Priviliges
        public DbSet<Privileges> Privileges { get; set; }
        public DbSet<Privileges_RoleBased> Privileges_RoleBased { get; set; }
        public DbSet<Privileges_UserBased> Privileges_UserBased { get; set; }

        public DbSet<CourseCategory> CoursesCategory { get; set; }
        public DbSet<Course> Courses { get; set; }
        
        public DbSet<CourseEnrollment> CoursesEnrollment { get; set; }
        public DbSet<CourseLesson> CourseLessons { get; set; }
        public DbSet<CourseInstructor> CourseInstructors { get; set; }
        public DbSet<CourseRequirement> CourseRequirements { get; set; }
        public DbSet<CourseReview> CourseReviews { get; set; }
        public DbSet<CourseSection> CourseSections { get; set; } 
        public DbSet<Instructor> Instructors { get; set; }
        public DbSet<InstructorSkill> InstructorsSkill { get; set; }
        public DbSet<StudentReview> StudentReviews { get; set; }
        public DbSet<ContactRequest> ContactRequests { get; set; }
    }
}
