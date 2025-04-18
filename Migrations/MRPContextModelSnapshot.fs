﻿// <auto-generated />
namespace MrpService.Migrations

open System
open Microsoft.EntityFrameworkCore
open Microsoft.EntityFrameworkCore.Infrastructure
open Microsoft.EntityFrameworkCore.Metadata
open Microsoft.EntityFrameworkCore.Migrations
open Microsoft.EntityFrameworkCore.Storage.ValueConversion
open MrpService.Data

[<DbContext(typeof<MRPContext>)>]
type MRPContextModelSnapshot() =
    inherit ModelSnapshot()

    override this.BuildModel(modelBuilder: ModelBuilder) =
        modelBuilder
            .HasAnnotation("ProductVersion", "6.0.1")
            .HasAnnotation("Relational:MaxIdentifierLength", 128) |> ignore

        modelBuilder.Entity("MrpService.Models.Position", (fun b ->

            b.Property<int>("PosId")
                .IsRequired(true)
                .ValueGeneratedOnAdd()
                .HasColumnType("int")
                |> ignore

            SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("PosId"), 1L, 1) |> ignore

            b.Property<string>("PositionCode")
                .IsRequired(true)
                .HasMaxLength(10)
                .HasColumnType("nvarchar(10)")
                |> ignore

            b.Property<string>("PositionDescr")
                .IsRequired(true)
                .HasMaxLength(50)
                .HasColumnType("nvarchar(50)")
                |> ignore

            b.Property<Int64>("StageId")
                .IsRequired(true)
                .HasColumnType("bigint")
                |> ignore

            b.HasKey("PosId")
                |> ignore


            b.HasIndex("PositionCode")
                .IsUnique()
                |> ignore


            b.HasIndex("StageId")
                |> ignore

            b.ToTable("Positions") |> ignore

        )) |> ignore

        modelBuilder.Entity("MrpService.Models.Product", (fun b ->

            b.Property<Int64>("ProductId")
                .IsRequired(true)
                .ValueGeneratedOnAdd()
                .HasColumnType("bigint")
                |> ignore

            SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<Int64>("ProductId"), 1L, 1) |> ignore

            b.Property<string>("ProductName")
                .IsRequired(true)
                .HasMaxLength(100)
                .HasColumnType("nvarchar(100)")
                |> ignore

            b.HasKey("ProductId")
                |> ignore


            b.ToTable("Products") |> ignore

        )) |> ignore

        modelBuilder.Entity("MrpService.Models.Stage", (fun b ->

            b.Property<Int64>("StageId")
                .IsRequired(true)
                .ValueGeneratedOnAdd()
                .HasColumnType("bigint")
                |> ignore

            SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<Int64>("StageId"), 1L, 1) |> ignore

            b.Property<Nullable<Int64>>("ParentStageId")
                .IsRequired(false)
                .HasColumnType("bigint")
                |> ignore

            b.Property<string>("StageCode")
                .IsRequired(true)
                .HasMaxLength(50)
                .HasColumnType("nvarchar(50)")
                |> ignore

            b.Property<string>("StageDescr")
                .IsRequired(true)
                .HasMaxLength(50)
                .HasColumnType("nvarchar(50)")
                |> ignore

            b.HasKey("StageId")
                |> ignore


            b.HasIndex("ParentStageId")
                |> ignore


            b.HasIndex("StageCode")
                .IsUnique()
                |> ignore

            b.ToTable("Stages") |> ignore

        )) |> ignore
        modelBuilder.Entity("MrpService.Models.Position", (fun b ->
            b.HasOne("MrpService.Models.Stage", null)
                .WithMany("Positions")
                .HasForeignKey("StageId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired()
                |> ignore

        )) |> ignore
        modelBuilder.Entity("MrpService.Models.Stage", (fun b ->
            b.HasOne("MrpService.Models.Stage", null)
                .WithMany("ChildStages")
                .HasForeignKey("ParentStageId")
                .OnDelete(DeleteBehavior.Restrict)
                |> ignore

        )) |> ignore
        modelBuilder.Entity("MrpService.Models.Stage", (fun b ->

            b.Navigation("ChildStages")
            |> ignore

            b.Navigation("Positions")
            |> ignore
        )) |> ignore

