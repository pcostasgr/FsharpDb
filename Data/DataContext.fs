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
    member this.Positions with get() = this.positions and set v = this.positions <- v

    [<DefaultValue>]
    val mutable stages: DbSet<Stage>
    member this.Stages with get() = this.stages and set v = this.stages <- v

    member this.GetDbContext() = this :> DbContext

    override _.OnModelCreating(modelBuilder: ModelBuilder) =
        // Configure Position entity
         
        
        
        //printfn "ModelBuilder is null: %b" (isNull modelBuilder)

        modelBuilder.RegisterOptionTypes() |> ignore

        modelBuilder.Entity<Position>(fun entity ->
            entity.HasKey(fun p -> p.PosId :> obj) |> ignore
            entity.Property(fun p -> p.PositionDescr).IsRequired() |> ignore
            entity.Property(fun p -> p.PositionCode).IsRequired() |> ignore
            entity.HasOne<Stage>().WithMany(fun s -> s.Positions :> IEnumerable<Position>).HasForeignKey(fun p -> p.StageId :> obj) |> ignore
        ) |> ignore

        printfn "Pass Position" 

       

        modelBuilder.Entity<Stage>(fun entity ->
            entity.HasKey(fun s -> s.StageId :> obj) |> ignore
            entity.Property(fun stage -> stage.StageId)
                |> fun property -> 
                    property.ValueGeneratedOnAdd() // Make it an identity column
                            .HasColumnType("bigint") // Set the column type to bigint
                            .IsRequired() |> ignore // Ensure the primary key is required
            entity.Property(fun s -> s.StageDescr).IsRequired() |> ignore
            entity.Property(fun s -> s.StageCode).IsRequired() |> ignore
            printfn "Pass Position" |> ignore
            entity.HasMany(fun s -> s.ChildStages :> IEnumerable<Stage>).WithOne().HasForeignKey(fun s -> s.ParentStageId :> obj) |> ignore
        ) |> ignore



