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
            let! stages = context.Stages
                            .Include(fun s -> s.Positions)
                            .Include(fun s -> s.ChildStages)
                            .ToListAsync()
            let mrp = { Positions = positions; Stages = stages }
            return ActionResult<Mrp>(mrp)
        }
    
    [<HttpGet("GetText")>]
    member this.GetText() =
        this.Ok("Hello World")

    [<HttpPost("CreateStage")>]
    member this.CreateStages() =
        
        let newStage = Stage("Initial Stage", "STG001")
    
        // Add the new stage to the context
        context.Stages.Add(newStage) |> ignore
    //
        // Save changes to the database
        let rowsAffected = context.SaveChanges() |> ignore


        this.Ok(String.Format("NewStage{0}", rowsAffected))

    [<HttpPost("CreateChildStage")>]
    member this.CreateChildStages() =
        task {
            let! parentStage = context.Stages.FirstOrDefaultAsync(fun s -> s.StageCode = "STG001")
        
            if isNull parentStage then
                return this.Ok(String.Format("Parent Stage Not Found"))
            else
                let newStage = Stage("Second Stage", "STG002")
                newStage.ParentStageId <- parentStage.StageId
                // Add the new stage to the context
                context.Stages.Add(newStage) |> ignore
                //
                // Save changes to the database
                let rowsAffected = context.SaveChanges()
                return this.Ok(String.Format("Child Stage{0}", rowsAffected))
            
        }

    [<HttpGet("GetStage/{stageId}")>]
    member this.GetStageById(stageId: int64) =
        task {
            let! stage = context.Stages
                            .Include(fun s -> s.ChildStages)  // Load child stages
                            .Include(fun s -> s.Positions)    // Load positions
                            .FirstOrDefaultAsync(fun s -> s.StageId = stageId)
            match stage with
            | null -> return this.Ok "No stage"
            | _ -> return this.Ok(stage)
        }

    //Usage
    (*λ curl -X POST http://localhost:5290/api/Mrp/CreatePosition -H "Content-Type: application/json" -d "{\"positionCode\": \"P001\", \"positionDescr\": \"Sample Position\", \"stageId\": 1}"*)
    [<HttpPost("CreatePosition")>]
    member this.CreatePosition([<FromBody>] position: Position) =
        Console.WriteLine(position)

        if position.PositionDescr |> String.IsNullOrWhiteSpace || position.PositionCode |> String.IsNullOrWhiteSpace then
            this.Ok("Position Description or Code cannot be empty")
        else
            context.Positions.Add(position) |> ignore
            context.SaveChanges() |> ignore
            this.Ok("Position created successfully")