﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using OddsRepository.Context;
using System;

namespace OddsWebsite.Migrations
{
    [DbContext(typeof(ArchiveContext))]
    [Migration("20170819200041_InitialCreate")]
    partial class InitialCreate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.0.0-rtm-26452")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("OddsRepository.Models.Country", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.ToTable("Countries");
                });

            modelBuilder.Entity("OddsRepository.Models.GameInfo", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<double>("AwayOdd");

                    b.Property<string>("AwayTeam");

                    b.Property<double>("HomeOdd");

                    b.Property<string>("HomeTeam");

                    b.Property<int>("Winner");

                    b.HasKey("Id");

                    b.ToTable("Games");
                });

            modelBuilder.Entity("OddsRepository.Models.LeagueInfo", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.Property<int?>("SportId");

                    b.HasKey("Id");

                    b.HasIndex("SportId");

                    b.ToTable("Leagues");
                });

            modelBuilder.Entity("OddsRepository.Models.Sport", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.ToTable("Sports");
                });

            modelBuilder.Entity("OddsRepository.Models.LeagueInfo", b =>
                {
                    b.HasOne("OddsRepository.Models.Sport", "Sport")
                        .WithMany()
                        .HasForeignKey("SportId");
                });
#pragma warning restore 612, 618
        }
    }
}
