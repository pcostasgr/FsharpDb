namespace MrpService.Controllers

open Microsoft.AspNetCore.Mvc
open Microsoft.EntityFrameworkCore
open MrpService.Data
open MrpService.Models
open System.Collections.Generic
open System


[<Route("api/[controller]")>]
[<ApiController>]
type MrpController(context: MRPContext) =
    inherit ControllerBase()

    [<HttpGet>]
    member _.Get() =
        task {
            let! positions = context.Positions.ToListAsync()

            let! stages =
                context.Stages
                    .Include(fun s -> s.Positions)
                    .Include(fun s -> s.ChildStages)
                    .ToListAsync()

            let mrp =
                { Positions = positions
                  Stages = stages }

            return ActionResult<Mrp>(mrp)
        }

    [<HttpGet("GetText")>]
    member this.GetText() = this.Ok("Hello World")

    [<HttpPost("CreateStage")>]
    member this.CreateStages([<FromBody>] payload: {| StageName: string |}) =
        task {
            if String.IsNullOrWhiteSpace(payload.StageName) then
                return this.BadRequest("Stage name cannot be empty") :> IActionResult
            else
                let newStage = Stage(payload.StageName, Guid.NewGuid().ToString()) // Generate a unique StageCode
                // Add the new stage to the context
                context.Stages.Add(newStage) |> ignore
                // Save changes to the database
                let rowsAffected = context.SaveChanges()
                return this.Ok(String.Format("New Stage created with {0} rows affected", rowsAffected))
        }

    [<HttpPost("CreateChildStage")>]
    member this.CreateChildStages
        ([<FromBody>] payload:
            {| StageDescr: string
               ParentStageId: int64 |})
        =
        task {
            let! parentStage = context.Stages.FirstOrDefaultAsync(fun s -> s.StageId = payload.ParentStageId)

            if isNull parentStage then
                return this.BadRequest("Parent Stage Not Found") :> IActionResult
            else
                let newStage = Stage(payload.StageDescr, Guid.NewGuid().ToString()) // Generate a unique StageCode
                newStage.ParentStageId <- parentStage.StageId
                // Add the new stage to the context
                context.Stages.Add(newStage) |> ignore
                // Save changes to the database
                let rowsAffected = context.SaveChanges()
                return this.Ok(String.Format("Child Stage created with {0} rows affected", rowsAffected))
        }

    [<HttpGet("GetStage/{stageId}")>]
    member this.GetStageById(stageId: int64) =
        task {
            let! stage =
                context.Stages
                    .Include(fun s -> s.ChildStages) // Load child stages
                    .Include(fun s -> s.Positions) // Load positions
                    .FirstOrDefaultAsync(fun s -> s.StageId = stageId)

            match stage with
            | null -> return this.Ok "No stage"
            | _ -> return this.Ok(stage)
        }

    //Usage
    (*λ curl -X POST http://localhost:5290/api/Mrp/CreatePosition -H "Content-Type: application/json" -d "{\"positionCode\": \"P001\", \"positionDescr\": \"Sample Position\", \"stageId\": 1}"*)
    [<HttpPost("CreatePosition")>]
    member this.CreatePosition([<FromBody>] position: Position) =
        task {
            if
                String.IsNullOrWhiteSpace(position.PositionDescr)
                || String.IsNullOrWhiteSpace(position.PositionCode)
            then
                return this.BadRequest("Position Description or Code cannot be empty") :> IActionResult
            else
                let! stage = context.Stages.FirstOrDefaultAsync(fun s -> s.StageId = position.StageId)

                if isNull stage then
                    return this.BadRequest("Invalid StageId") :> IActionResult
                else
                    context.Positions.Add(position) |> ignore
                    let rowsAffected = context.SaveChanges()

                    return
                        this.Ok(String.Format("Position created successfully with {0} rows affected", rowsAffected))
                        :> IActionResult
        }

    [<HttpPost("CreateProduct")>]
    member this.CreateProduct([<FromBody>] product: Product) =
        task {
            if String.IsNullOrWhiteSpace(product.ProductName) then
                return this.BadRequest("Product Name cannot be empty") :> IActionResult
            else
                context.Products.Add(product) |> ignore
                let rowsAffected = context.SaveChanges()

                return
                    this.Ok(String.Format("Product created successfully with {0} rows affected", rowsAffected))
                    :> IActionResult
        }
