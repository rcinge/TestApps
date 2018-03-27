﻿// <auto-generated />
using ConsoleApp.NetCore2;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using System;

namespace PocApp02.Migrations
{
    [DbContext(typeof(BloggingContext))]
    [Migration("20180326222048_init")]
    partial class init
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.0.2-rtm-10011")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("ConsoleApp.NetCore2.Blog", b =>
                {
                    b.Property<int>("BlogId")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("Rating");

                    b.Property<string>("Url");

                    b.HasKey("BlogId");

                    b.ToTable("Blogs");
                });

            modelBuilder.Entity("ConsoleApp.NetCore2.Post", b =>
                {
                    b.Property<int>("PostId")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("BlogId");

                    b.Property<string>("Content");

                    b.Property<string>("Title");

                    b.HasKey("PostId");

                    b.HasIndex("BlogId");

                    b.ToTable("Posts");
                });

            modelBuilder.Entity("ConsoleApp.NetCore2.PostTag", b =>
                {
                    b.Property<int>("PostTagId")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("PostId");

                    b.Property<string>("Tag");

                    b.HasKey("PostTagId");

                    b.HasIndex("PostId");

                    b.ToTable("PostTags");
                });

            modelBuilder.Entity("ConsoleApp.NetCore2.Post", b =>
                {
                    b.HasOne("ConsoleApp.NetCore2.Blog", "Blog")
                        .WithMany("Posts")
                        .HasForeignKey("BlogId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("ConsoleApp.NetCore2.PostTag", b =>
                {
                    b.HasOne("ConsoleApp.NetCore2.Post", "Post")
                        .WithMany()
                        .HasForeignKey("PostId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}