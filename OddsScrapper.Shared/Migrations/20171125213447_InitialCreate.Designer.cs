﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using OddsWebsite.Models;
using System;

namespace OddsScrapper.Shared.Migrations
{
    [DbContext(typeof(ArchiveContext))]
    [Migration("20171125213447_InitialCreate")]
    partial class InitialCreate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.0.1-rtm-125");

            modelBuilder.Entity("OddsWebsite.Models.Country", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name")
                        .IsRequired();

                    b.HasKey("Id");

                    b.ToTable("Countries");
                });

            modelBuilder.Entity("OddsWebsite.Models.Game", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<double>("AwayOdd");

                    b.Property<int?>("AwayTeamId");

                    b.Property<int>("AwayTeamScore");

                    b.Property<DateTime>("Date");

                    b.Property<double>("DrawOdd");

                    b.Property<double>("HomeOdd");

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

            modelBuilder.Entity("OddsWebsite.Models.League", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("CountryId");

                    b.Property<bool>("IsCup");

                    b.Property<bool>("IsFirst");

                    b.Property<bool>("IsWomen");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<int>("SportId");

                    b.HasKey("Id");

                    b.HasIndex("CountryId");

                    b.HasIndex("SportId");

                    b.ToTable("Leagues");
                });

            modelBuilder.Entity("OddsWebsite.Models.Sport", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name")
                        .IsRequired();

                    b.HasKey("Id");

                    b.ToTable("Sports");
                });

            modelBuilder.Entity("OddsWebsite.Models.Team", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("LeagueId");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("LeagueId");

                    b.ToTable("Teams");
                });

            modelBuilder.Entity("OddsWebsite.Models.Game", b =>
                {
                    b.HasOne("OddsWebsite.Models.Team", "AwayTeam")
                        .WithMany()
                        .HasForeignKey("AwayTeamId");

                    b.HasOne("OddsWebsite.Models.Team", "HomeTeam")
                        .WithMany()
                        .HasForeignKey("HomeTeamId");

                    b.HasOne("OddsWebsite.Models.League", "League")
                        .WithMany()
                        .HasForeignKey("LeagueId");
                });

            modelBuilder.Entity("OddsWebsite.Models.League", b =>
                {
                    b.HasOne("OddsWebsite.Models.Country", "Country")
                        .WithMany()
                        .HasForeignKey("CountryId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("OddsWebsite.Models.Sport", "Sport")
                        .WithMany()
                        .HasForeignKey("SportId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("OddsWebsite.Models.Team", b =>
                {
                    b.HasOne("OddsWebsite.Models.League", "League")
                        .WithMany()
                        .HasForeignKey("LeagueId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
