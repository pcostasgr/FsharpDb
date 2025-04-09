namespace MrpService.Data

open Microsoft.EntityFrameworkCore
open MrpService.Models

type MRPContext(options: DbContextOptions<MRPContext>) =
    inherit DbContext(options)

    [<DefaultValue>]
    val mutable private positions: DbSet<Position>
    member this.Positions
        with get() = this.positions
        and set v = this.positions <- v

    [<DefaultValue>]
    val mutable private stages: DbSet<Stage>
    member this.Stages
        with get() = this.stages
        and set v = this.stages <- v

    [<DefaultValue>]
    val mutable private products: DbSet<Product>
    member this.Products
        with get() = this.products
        and set v = this.products <- v

    override this.OnModelCreating(modelBuilder: ModelBuilder) =
        base.OnModelCreating(modelBuilder)
        modelBuilder.Entity<Stage>()
            .HasIndex(fun s -> s.StageCode)
            .IsUnique() |> ignore