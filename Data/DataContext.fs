namespace MrpService.Data

open Microsoft.EntityFrameworkCore
open System.Collections.Generic
open MrpService.Models
open System.ComponentModel.DataAnnotations
open EntityFrameworkCore.FSharp.Extensions

type MRPContext(options: DbContextOptions<MRPContext>) =
    inherit DbContext(options)

    [<DefaultValue>]
    val mutable positions: DbSet<Position>

    member this.Positions
        with get () = this.positions
        and set v = this.positions <- v

    [<DefaultValue>]
    val mutable stages: DbSet<Stage>

    member this.Stages
        with get () = this.stages
        and set v = this.stages <- v

    [<DefaultValue>]
    val mutable products: DbSet<Product>

    member this.Products
        with get () = this.products
        and set v = this.products <- v

    member this.GetDbContext() = this :> DbContext

    override _.OnModelCreating(modelBuilder: ModelBuilder) =
        // Configure Position entity
        //printfn "ModelBuilder is null: %b" (isNull modelBuilder)

        modelBuilder.RegisterOptionTypes() |> ignore

        modelBuilder.Entity<Position>(fun entity ->
            entity.HasKey(fun p -> p.PosId :> obj) |> ignore
            entity.Property(fun p -> p.PositionDescr).IsRequired() |> ignore
            entity.Property(fun p -> p.PositionCode).IsRequired() |> ignore

            // Add unique constraint for PositionCode
            entity.HasIndex(fun p -> p.PositionCode :> obj).IsUnique() |> ignore

            entity
                .HasOne<Stage>()
                .WithMany(fun s -> s.Positions :> IEnumerable<Position>)
                .HasForeignKey(fun p -> p.StageId :> obj)
            |> ignore)
        |> ignore


        modelBuilder.Entity<Stage>(fun entity ->
            entity.HasKey(fun s -> s.StageId :> obj) |> ignore

            entity.Property(fun stage -> stage.StageId)
            |> fun property ->
                property
                    .ValueGeneratedOnAdd() // Make it an identity column
                    .HasColumnType("bigint") // Set the column type to bigint
                    .IsRequired()
                |> ignore // Ensure the primary key is required

            entity.Property(fun s -> s.StageDescr).IsRequired() |> ignore
            entity.Property(fun s -> s.StageCode).IsRequired() |> ignore

            // Add unique constraint for StageCode
            entity.HasIndex(fun s -> s.StageCode :> obj).IsUnique() |> ignore

            printfn "Pass Position" |> ignore

            entity
                .HasMany(fun s -> s.ChildStages :> IEnumerable<Stage>)
                .WithOne()
                .HasForeignKey(fun s -> s.ParentStageId :> obj)
                .OnDelete(DeleteBehavior.Restrict) // Prevent cascading deletes to avoid circular reference issues
            |> ignore)
        |> ignore

        modelBuilder.Entity<Product>(fun entity ->
            entity.HasKey(fun p -> p.ProductId :> obj) |> ignore // Set ProductId as the primary key

            entity
                .Property(fun p -> p.ProductId)
                .ValueGeneratedOnAdd() // Make ProductId an identity column
                .HasColumnType("bigint") // Set the column type to bigint
                .IsRequired()
            |> ignore // Ensure the primary key is required

            entity
                .Property(fun p -> p.ProductName)
                .IsRequired() // Ensure ProductName is required
                .HasMaxLength(100)
            |> ignore // Set the maximum length of ProductName to 100
        )
        |> ignore
