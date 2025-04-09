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
    member this.CreateStage
        ([<FromBody>] payload: 
            {| StageName: string
               StageCode: string
               ParentStageId: int64 option |}) =
        task {
            if String.IsNullOrWhiteSpace(payload.StageName) || String.IsNullOrWhiteSpace(payload.StageCode) then
                return this.BadRequest("Stage name and code cannot be empty") :> IActionResult
            else
                let newStage = Stage(payload.StageName, payload.StageCode)

                match payload.ParentStageId with
                | Some parentId ->
                    let! parentStage = context.Stages.FirstOrDefaultAsync(fun s -> s.StageId = parentId)
                    if isNull parentStage then
                        newStage.ParentStageId <- Nullable()
                    else
                        newStage.ParentStageId <- parentStage.StageId
                | None -> () // No parent stage, proceed as a root stage

                // Add the new stage to the context
                context.Stages.Add(newStage) |> ignore
                try
                    // Save changes to the database
                    let rowsAffected = context.SaveChanges()
                    return this.Ok(String.Format("Stage created with {0} rows affected", rowsAffected))
                with
                | ex -> return this.StatusCode(500, String.Format("An error occurred: {0}", ex.Message)) :> IActionResult
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
                    try
                        let rowsAffected = context.SaveChanges()
                        return
                            this.Ok(String.Format("Position created successfully with {0} rows affected", rowsAffected))
                            :> IActionResult
                    with
                    | ex -> return this.StatusCode(500, String.Format("An error occurred: {0}", ex.Message)) :> IActionResult
        }

    [<HttpPost("CreateProduct")>]
    member this.CreateProduct([<FromBody>] product: Product) =
        task {
            if String.IsNullOrWhiteSpace(product.ProductName) then
                return this.BadRequest("Product Name cannot be empty") :> IActionResult
            else
                context.Products.Add(product) |> ignore
                try
                    let rowsAffected = context.SaveChanges()
                    return
                        this.Ok(String.Format("Product created successfully with {0} rows affected", rowsAffected))
                        :> IActionResult
                with
                | ex -> return this.StatusCode(500, String.Format("An error occurred: {0}", ex.Message)) :> IActionResult
        }

    [<HttpDelete("DeleteStage/{stageId}")>]
    member this.DeleteStage(stageId: int64) =
        task {
            let! stage = context.Stages.FirstOrDefaultAsync(fun s -> s.StageId = stageId)

            if isNull stage then
                return this.NotFound("Stage not found") :> IActionResult
            else
                context.Stages.Remove(stage) |> ignore
                try
                    let rowsAffected = context.SaveChanges()
                    return this.Ok(String.Format("Stage deleted with {0} rows affected", rowsAffected))
                with
                | ex -> return this.StatusCode(500, String.Format("An error occurred: {0}", ex.Message)) :> IActionResult
        }
