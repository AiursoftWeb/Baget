﻿// <auto-generated />
using System;
using Aiursoft.BaGet.Database.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Aiursoft.BaGet.Database.Sqlite.Migrations
{
    [DbContext(typeof(SqliteContext))]
    partial class SqliteContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "9.0.3");

            modelBuilder.Entity("Aiursoft.BaGet.Core.Entities.Package", b =>
                {
                    b.Property<int>("Key")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Authors")
                        .HasMaxLength(4000)
                        .HasColumnType("TEXT");

                    b.Property<string>("Description")
                        .HasMaxLength(4000)
                        .HasColumnType("TEXT");

                    b.Property<bool>("HasEmbeddedIcon")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("HasReadme")
                        .HasColumnType("INTEGER");

                    b.Property<string>("IconUrl")
                        .HasColumnType("TEXT");

                    b.Property<string>("Id")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("TEXT COLLATE NOCASE");

                    b.Property<bool>("IsPrerelease")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Language")
                        .HasMaxLength(20)
                        .HasColumnType("TEXT");

                    b.Property<string>("LicenseUrl")
                        .HasColumnType("TEXT");

                    b.Property<bool>("Listed")
                        .HasColumnType("INTEGER");

                    b.Property<string>("MinClientVersion")
                        .HasMaxLength(44)
                        .HasColumnType("TEXT");

                    b.Property<string>("NormalizedVersionString")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("TEXT COLLATE NOCASE")
                        .HasColumnName("Version");

                    b.Property<string>("OriginalVersionString")
                        .HasMaxLength(64)
                        .HasColumnType("TEXT")
                        .HasColumnName("OriginalVersion");

                    b.Property<string>("ProjectUrl")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("Published")
                        .HasColumnType("TEXT");

                    b.Property<string>("ReleaseNotes")
                        .HasColumnType("TEXT")
                        .HasColumnName("ReleaseNotes");

                    b.Property<string>("RepositoryType")
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<string>("RepositoryUrl")
                        .HasColumnType("TEXT");

                    b.Property<bool>("RequireLicenseAcceptance")
                        .HasColumnType("INTEGER");

                    b.Property<byte[]>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("BLOB");

                    b.Property<int>("SemVerLevel")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Summary")
                        .HasMaxLength(4000)
                        .HasColumnType("TEXT");

                    b.Property<string>("Tags")
                        .HasMaxLength(4000)
                        .HasColumnType("TEXT");

                    b.Property<string>("Title")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.HasKey("Key");

                    b.HasIndex("Id");

                    b.HasIndex("Id", "NormalizedVersionString")
                        .IsUnique();

                    b.ToTable("Packages");
                });

            modelBuilder.Entity("Aiursoft.BaGet.Core.Entities.PackageDependency", b =>
                {
                    b.Property<int>("Key")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Id")
                        .HasMaxLength(128)
                        .HasColumnType("TEXT COLLATE NOCASE");

                    b.Property<int?>("PackageKey")
                        .HasColumnType("INTEGER");

                    b.Property<string>("TargetFramework")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.Property<string>("VersionRange")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.HasKey("Key");

                    b.HasIndex("Id");

                    b.HasIndex("PackageKey");

                    b.ToTable("PackageDependencies");
                });

            modelBuilder.Entity("Aiursoft.BaGet.Core.Entities.PackageType", b =>
                {
                    b.Property<int>("Key")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .HasMaxLength(512)
                        .HasColumnType("TEXT COLLATE NOCASE");

                    b.Property<int>("PackageKey")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Version")
                        .HasMaxLength(64)
                        .HasColumnType("TEXT");

                    b.HasKey("Key");

                    b.HasIndex("Name");

                    b.HasIndex("PackageKey");

                    b.ToTable("PackageTypes");
                });

            modelBuilder.Entity("Aiursoft.BaGet.Core.Entities.TargetFramework", b =>
                {
                    b.Property<int>("Key")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Moniker")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT COLLATE NOCASE");

                    b.Property<int>("PackageKey")
                        .HasColumnType("INTEGER");

                    b.HasKey("Key");

                    b.HasIndex("Moniker");

                    b.HasIndex("PackageKey");

                    b.ToTable("TargetFrameworks");
                });

            modelBuilder.Entity("Aiursoft.BaGet.Core.Entities.PackageDependency", b =>
                {
                    b.HasOne("Aiursoft.BaGet.Core.Entities.Package", "Package")
                        .WithMany("Dependencies")
                        .HasForeignKey("PackageKey");

                    b.Navigation("Package");
                });

            modelBuilder.Entity("Aiursoft.BaGet.Core.Entities.PackageType", b =>
                {
                    b.HasOne("Aiursoft.BaGet.Core.Entities.Package", "Package")
                        .WithMany("PackageTypes")
                        .HasForeignKey("PackageKey")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Package");
                });

            modelBuilder.Entity("Aiursoft.BaGet.Core.Entities.TargetFramework", b =>
                {
                    b.HasOne("Aiursoft.BaGet.Core.Entities.Package", "Package")
                        .WithMany("TargetFrameworks")
                        .HasForeignKey("PackageKey")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Package");
                });

            modelBuilder.Entity("Aiursoft.BaGet.Core.Entities.Package", b =>
                {
                    b.Navigation("Dependencies");

                    b.Navigation("PackageTypes");

                    b.Navigation("TargetFrameworks");
                });
#pragma warning restore 612, 618
        }
    }
}
