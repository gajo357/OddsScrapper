﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using OddsScrapper.Shared.Repository;
using System;

namespace OddsScrapper.Shared.Migrations
{
    [DbContext(typeof(ArchiveContext))]
    partial class ArchiveContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.0.1-rtm-125");

            modelBuilder.Entity("OddsScrapper.Shared.Models.Bookkeeper", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name")
                        .IsRequired();

                    b.HasKey("Id");

                    b.ToTable("Bookers");
                });

            modelBuilder.Entity("OddsScrapper.Shared.Models.Country", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name")
                        .IsRequired();

                    b.HasKey("Id");

                    b.ToTable("Countries");
                });

            modelBuilder.Entity("OddsScrapper.Shared.Models.Game", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int?>("AwayTeamId");

                    b.Property<int>("AwayTeamScore");

                    b.Property<DateTime?>("Date");

                    b.Property<string>("GameLink");

                    b.Property<int?>("HomeTeamId");

                    b.Property<int>("HomeTeamScore");

                    b.Property<bool>("IsOvertime");

                    b.Property<bool>("IsPlayoffs");

                    b.Property<int?>("LeagueId");

                    b.HasKey("Id");

                    b.HasIndex("AwayTeamId");

                    b.HasIndex("HomeTeamId");

                    b.HasIndex("LeagueId");

                    b.ToTable("Games");
                });

            modelBuilder.Entity("OddsScrapper.Shared.Models.GameOdds", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<double>("AwayOdd");

                    b.Property<int?>("BookkeeperId");

                    b.Property<double>("DrawOdd");

                    b.Property<int?>("GameId");

                    b.Property<double>("HomeOdd");

                    b.Property<bool>("IsValid");

                    b.HasKey("Id");

                    b.HasIndex("BookkeeperId");

                    b.HasIndex("GameId");

                    b.ToTable("GameOdds");
                });

            modelBuilder.Entity("OddsScrapper.Shared.Models.League", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("CountryId");

                    b.Property<bool>("IsFirst");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<int>("SportId");

                    b.HasKey("Id");

                    b.HasIndex("CountryId");

                    b.HasIndex("SportId");

                    b.ToTable("Leagues");
                });

            modelBuilder.Entity("OddsScrapper.Shared.Models.Sport", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name")
                        .IsRequired();

                    b.HasKey("Id");

                    b.ToTable("Sports");
                });

            modelBuilder.Entity("OddsScrapper.Shared.Models.Team", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name")
                        .IsRequired();

                    b.HasKey("Id");

                    b.ToTable("Teams");
                });

            modelBuilder.Entity("OddsScrapper.Shared.Models.Game", b =>
                {
                    b.HasOne("OddsScrapper.Shared.Models.Team", "AwayTeam")
                        .WithMany()
                        .HasForeignKey("AwayTeamId");

                    b.HasOne("OddsScrapper.Shared.Models.Team", "HomeTeam")
                        .WithMany()
                        .HasForeignKey("HomeTeamId");

                    b.HasOne("OddsScrapper.Shared.Models.League", "League")
                        .WithMany()
                        .HasForeignKey("LeagueId");
                });

            modelBuilder.Entity("OddsScrapper.Shared.Models.GameOdds", b =>
                {
                    b.HasOne("OddsScrapper.Shared.Models.Bookkeeper", "Bookkeeper")
                        .WithMany()
                        .HasForeignKey("BookkeeperId");

                    b.HasOne("OddsScrapper.Shared.Models.Game")
                        .WithMany("Odds")
                        .HasForeignKey("GameId");
                });

            modelBuilder.Entity("OddsScrapper.Shared.Models.League", b =>
                {
                    b.HasOne("OddsScrapper.Shared.Models.Country", "Country")
                        .WithMany()
                        .HasForeignKey("CountryId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("OddsScrapper.Shared.Models.Sport", "Sport")
                        .WithMany()
                        .HasForeignKey("SportId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
