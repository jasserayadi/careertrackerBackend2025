﻿// <auto-generated />
using System;
using Career_Tracker_Backend;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Career_Tracker_Backend.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20250304003041_MakeColumnsNullable")]
    partial class MakeColumnsNullable
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("Career_Tracker_Backend.Models.Badge", b =>
                {
                    b.Property<int>("BadgeId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("BadgeName")
                        .IsRequired()
                        .HasColumnType("nvarchar(24)");

                    b.HasKey("BadgeId");

                    b.ToTable("Badges");
                });

            modelBuilder.Entity("Career_Tracker_Backend.Models.CV", b =>
                {
                    b.Property<int>("CvId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("CvFile")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Experiences")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Skills")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("CvId");

                    b.HasIndex("UserId")
                        .IsUnique();

                    b.ToTable("CVs");
                });

            modelBuilder.Entity("Career_Tracker_Backend.Models.Category", b =>
                {
                    b.Property<int>("CategoryId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("CategoryId");

                    b.ToTable("Categories");
                });

            modelBuilder.Entity("Career_Tracker_Backend.Models.Certificat", b =>
                {
                    b.Property<int>("CertificatId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("CertificatName")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("varchar(200)");

                    b.Property<int>("UserFk")
                        .HasColumnType("int");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("CertificatId");

                    b.HasIndex("UserId");

                    b.ToTable("Certificats");
                });

            modelBuilder.Entity("Career_Tracker_Backend.Models.Course", b =>
                {
                    b.Property<int>("CourseId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(1000)
                        .HasColumnType("varchar(1000)");

                    b.Property<int>("FormationFk")
                        .HasColumnType("int");

                    b.Property<int>("FormationId")
                        .HasColumnType("int");

                    b.Property<int>("FormationId2")
                        .HasColumnType("int");

                    b.Property<int>("MoodleCourseId")
                        .HasColumnType("int");

                    b.Property<int>("MoodleSectionId")
                        .HasColumnType("int");

                    b.Property<int>("Order")
                        .HasColumnType("int");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("varchar(200)");

                    b.HasKey("CourseId");

                    b.HasIndex("FormationFk");

                    b.HasIndex("FormationId");

                    b.ToTable("Courses");
                });

            modelBuilder.Entity("Career_Tracker_Backend.Models.Feedback", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<int>("UserFk")
                        .HasColumnType("int");

                    b.Property<string>("message")
                        .IsRequired()
                        .HasMaxLength(1000)
                        .HasColumnType("varchar(1000)");

                    b.HasKey("Id");

                    b.HasIndex("UserFk");

                    b.ToTable("Feedbacks");
                });

            modelBuilder.Entity("Career_Tracker_Backend.Models.Formation", b =>
                {
                    b.Property<int>("FormationId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<int>("CategoryFk")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(1000)
                        .HasColumnType("varchar(1000)");

                    b.Property<int>("MoodleCategoryId")
                        .HasColumnType("int");

                    b.Property<int>("MoodleCourseId")
                        .HasColumnType("int");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("varchar(200)");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("datetime(6)");

                    b.HasKey("FormationId");

                    b.HasIndex("CategoryFk");

                    b.ToTable("Formations");
                });

            modelBuilder.Entity("Career_Tracker_Backend.Models.Inscription", b =>
                {
                    b.Property<int>("InscriptionId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<int>("FormationFk")
                        .HasColumnType("int");

                    b.Property<DateTime>("InscriptionDate")
                        .HasColumnType("datetime(6)");

                    b.Property<int>("MoodleEnrollmentId")
                        .HasColumnType("int");

                    b.Property<int>("UserFk")
                        .HasColumnType("int");

                    b.HasKey("InscriptionId");

                    b.HasIndex("FormationFk");

                    b.HasIndex("UserFk");

                    b.ToTable("Inscriptions");
                });

            modelBuilder.Entity("Career_Tracker_Backend.Models.Job", b =>
                {
                    b.Property<int>("JobId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("JobDescription")
                        .IsRequired()
                        .HasMaxLength(1000)
                        .HasColumnType("varchar(1000)");

                    b.Property<string>("JobName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)");

                    b.HasKey("JobId");

                    b.ToTable("Jobs");
                });

            modelBuilder.Entity("Career_Tracker_Backend.Models.Question", b =>
                {
                    b.Property<int>("QuestionId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<float>("Rate")
                        .HasColumnType("float");

                    b.Property<int>("TestFk")
                        .HasColumnType("int");

                    b.Property<string>("Text")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("varchar(500)");

                    b.HasKey("QuestionId");

                    b.HasIndex("TestFk");

                    b.ToTable("Questions");
                });

            modelBuilder.Entity("Career_Tracker_Backend.Models.Role", b =>
                {
                    b.Property<int>("IdRole")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("RoleName")
                        .IsRequired()
                        .HasColumnType("nvarchar(24)");

                    b.HasKey("IdRole");

                    b.ToTable("Roles");
                });

            modelBuilder.Entity("Career_Tracker_Backend.Models.Test", b =>
                {
                    b.Property<int>("TestId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<int>("CourseId")
                        .HasColumnType("int");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("varchar(500)");

                    b.Property<int>("MoodleQuizId")
                        .HasColumnType("int");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)");

                    b.HasKey("TestId");

                    b.HasIndex("CourseId")
                        .IsUnique();

                    b.ToTable("Tests");
                });

            modelBuilder.Entity("Career_Tracker_Backend.Models.User", b =>
                {
                    b.Property<int>("UserId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<int?>("BadgeId")
                        .HasColumnType("int");

                    b.Property<DateTimeOffset?>("DateCreation")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Firstname")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)");

                    b.Property<int?>("JobId")
                        .HasColumnType("int");

                    b.Property<string>("Lastname")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<int?>("RoleId")
                        .HasColumnType("int");

                    b.Property<int?>("RoleIdRole")
                        .HasColumnType("int");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)");

                    b.HasKey("UserId");

                    b.HasIndex("BadgeId");

                    b.HasIndex("JobId");

                    b.HasIndex("RoleId")
                        .IsUnique();

                    b.HasIndex("RoleIdRole")
                        .IsUnique();

                    b.ToTable("Users");
                });

            modelBuilder.Entity("FormationUser", b =>
                {
                    b.Property<int>("FormationsFormationId")
                        .HasColumnType("int");

                    b.Property<int>("UsersUserId")
                        .HasColumnType("int");

                    b.HasKey("FormationsFormationId", "UsersUserId");

                    b.HasIndex("UsersUserId");

                    b.ToTable("FormationUser");
                });

            modelBuilder.Entity("Career_Tracker_Backend.Models.CV", b =>
                {
                    b.HasOne("Career_Tracker_Backend.Models.User", "User")
                        .WithOne("CV")
                        .HasForeignKey("Career_Tracker_Backend.Models.CV", "UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("Career_Tracker_Backend.Models.Certificat", b =>
                {
                    b.HasOne("Career_Tracker_Backend.Models.User", "User")
                        .WithMany("Certificats")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("Career_Tracker_Backend.Models.Course", b =>
                {
                    b.HasOne("Career_Tracker_Backend.Models.Formation", "Formation")
                        .WithMany()
                        .HasForeignKey("FormationFk")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Career_Tracker_Backend.Models.Formation", null)
                        .WithMany("Courses")
                        .HasForeignKey("FormationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Formation");
                });

            modelBuilder.Entity("Career_Tracker_Backend.Models.Feedback", b =>
                {
                    b.HasOne("Career_Tracker_Backend.Models.User", "User")
                        .WithMany("Feedbacks")
                        .HasForeignKey("UserFk")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("Career_Tracker_Backend.Models.Formation", b =>
                {
                    b.HasOne("Career_Tracker_Backend.Models.Category", "Category")
                        .WithMany()
                        .HasForeignKey("CategoryFk")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Category");
                });

            modelBuilder.Entity("Career_Tracker_Backend.Models.Inscription", b =>
                {
                    b.HasOne("Career_Tracker_Backend.Models.Formation", "Formation")
                        .WithMany()
                        .HasForeignKey("FormationFk")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Career_Tracker_Backend.Models.User", "User")
                        .WithMany("Inscriptions")
                        .HasForeignKey("UserFk")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Formation");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Career_Tracker_Backend.Models.Question", b =>
                {
                    b.HasOne("Career_Tracker_Backend.Models.Test", "Test")
                        .WithMany("Questions")
                        .HasForeignKey("TestFk")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Test");
                });

            modelBuilder.Entity("Career_Tracker_Backend.Models.Test", b =>
                {
                    b.HasOne("Career_Tracker_Backend.Models.Course", "Course")
                        .WithOne("Test")
                        .HasForeignKey("Career_Tracker_Backend.Models.Test", "CourseId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Course");
                });

            modelBuilder.Entity("Career_Tracker_Backend.Models.User", b =>
                {
                    b.HasOne("Career_Tracker_Backend.Models.Badge", "Badge")
                        .WithMany("Users")
                        .HasForeignKey("BadgeId");

                    b.HasOne("Career_Tracker_Backend.Models.Job", "Job")
                        .WithMany("Users")
                        .HasForeignKey("JobId");

                    b.HasOne("Career_Tracker_Backend.Models.Role", "Role")
                        .WithOne()
                        .HasForeignKey("Career_Tracker_Backend.Models.User", "RoleId");

                    b.HasOne("Career_Tracker_Backend.Models.Role", null)
                        .WithOne("User")
                        .HasForeignKey("Career_Tracker_Backend.Models.User", "RoleIdRole");

                    b.Navigation("Badge");

                    b.Navigation("Job");

                    b.Navigation("Role");
                });

            modelBuilder.Entity("FormationUser", b =>
                {
                    b.HasOne("Career_Tracker_Backend.Models.Formation", null)
                        .WithMany()
                        .HasForeignKey("FormationsFormationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Career_Tracker_Backend.Models.User", null)
                        .WithMany()
                        .HasForeignKey("UsersUserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Career_Tracker_Backend.Models.Badge", b =>
                {
                    b.Navigation("Users");
                });

            modelBuilder.Entity("Career_Tracker_Backend.Models.Course", b =>
                {
                    b.Navigation("Test")
                        .IsRequired();
                });

            modelBuilder.Entity("Career_Tracker_Backend.Models.Formation", b =>
                {
                    b.Navigation("Courses");
                });

            modelBuilder.Entity("Career_Tracker_Backend.Models.Job", b =>
                {
                    b.Navigation("Users");
                });

            modelBuilder.Entity("Career_Tracker_Backend.Models.Role", b =>
                {
                    b.Navigation("User")
                        .IsRequired();
                });

            modelBuilder.Entity("Career_Tracker_Backend.Models.Test", b =>
                {
                    b.Navigation("Questions");
                });

            modelBuilder.Entity("Career_Tracker_Backend.Models.User", b =>
                {
                    b.Navigation("CV")
                        .IsRequired();

                    b.Navigation("Certificats");

                    b.Navigation("Feedbacks");

                    b.Navigation("Inscriptions");
                });
#pragma warning restore 612, 618
        }
    }
}
