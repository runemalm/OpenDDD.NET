﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using OpenDDD.Tests.Integration.Infrastructure.Persistence.EfCore.DbContext.Sqlite;

#nullable disable

namespace OpenDDD.Tests.Integration.Infrastructure.Persistence.EfCore.Migrations.Sqlite
{
    [DbContext(typeof(SqliteTestDbContext))]
    partial class SqliteTestDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "9.0.2");

            modelBuilder.Entity("OpenDDD.Infrastructure.TransactionalOutbox.OutboxEntry", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("TEXT");

                    b.Property<string>("EventName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("EventType")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Payload")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("ProcessedAt")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("OutboxEntries");
                });

            modelBuilder.Entity("OpenDDD.Tests.Domain.Model.TestAggregateRoot", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("TestAggregateRoots", (string)null);
                });

            modelBuilder.Entity("OpenDDD.Tests.Domain.Model.TestEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<Guid>("TestAggregateRootId")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("TestAggregateRootId");

                    b.ToTable("TestEntities", (string)null);
                });

            modelBuilder.Entity("OpenDDD.Tests.Domain.Model.TestAggregateRoot", b =>
                {
                    b.OwnsOne("OpenDDD.Tests.Domain.Model.TestValueObject", "Value", b1 =>
                        {
                            b1.Property<Guid>("TestAggregateRootId")
                                .HasColumnType("TEXT");

                            b1.Property<int>("Number")
                                .HasColumnType("INTEGER");

                            b1.Property<string>("Text")
                                .IsRequired()
                                .HasColumnType("TEXT");

                            b1.HasKey("TestAggregateRootId");

                            b1.ToTable("TestAggregateRoots");

                            b1.WithOwner()
                                .HasForeignKey("TestAggregateRootId");
                        });

                    b.Navigation("Value")
                        .IsRequired();
                });

            modelBuilder.Entity("OpenDDD.Tests.Domain.Model.TestEntity", b =>
                {
                    b.HasOne("OpenDDD.Tests.Domain.Model.TestAggregateRoot", null)
                        .WithMany("Entities")
                        .HasForeignKey("TestAggregateRootId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("OpenDDD.Tests.Domain.Model.TestAggregateRoot", b =>
                {
                    b.Navigation("Entities");
                });
#pragma warning restore 612, 618
        }
    }
}
