﻿// <auto-generated />
using System;
using MBTIBot;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace MBTIBot.Migrations
{
    [DbContext(typeof(BotContext))]
    [Migration("20220508100559_AddTrackedMessageGuild")]
    partial class AddTrackedMessageGuild
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "6.0.4");

            modelBuilder.Entity("MBTIBot.Models.GuildSettings", b =>
                {
                    b.Property<ulong>("GuildId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<ulong?>("IntroductionChannel")
                        .HasColumnType("INTEGER");

                    b.HasKey("GuildId");

                    b.ToTable("GuildSettings");
                });

            modelBuilder.Entity("MBTIBot.Models.RemindedUsers", b =>
                {
                    b.Property<ulong>("UserId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.HasKey("UserId");

                    b.ToTable("RemindedUsers");
                });

            modelBuilder.Entity("MBTIBot.Models.TrackedMessage", b =>
                {
                    b.Property<ulong>("MessageId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("Author")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("Guild")
                        .HasColumnType("INTEGER");

                    b.HasKey("MessageId");

                    b.ToTable("TrackedMessages");
                });
#pragma warning restore 612, 618
        }
    }
}
