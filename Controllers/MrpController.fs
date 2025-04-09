namespace MrpService.Controllers

open Microsoft.AspNetCore.Mvc
open Microsoft.EntityFrameworkCore
open MrpService.Data
open MrpService.Models
open System.Collections.Generic
open System

[<Route("api/stages")>]
[<ApiController>]
type StagesController(context: MRPContext) =
    inherit ControllerBase()

    [<HttpGet>]
    member this.GetStages() =
        task {
            try
                let! positions = context.Positions.ToListAsync()
                let! stages =
                    context.Stages
                        .Include(fun s -> s.Positions)
                        .Include(fun s -> s.ChildStages)
                        .ToListAsync()

                let mrp =
                    { Positions = positions
                      Stages = stages }

                let response =
                    { Success = true
                      Data = Some mrp
                      Error = None }

                return this.Ok(response) :> IActionResult
            with
            | ex ->
                let response =
                    { Success = false
                      Data = None
                      Error = Some (String.Format("An error occurred: {0}", ex.Message)) }

                return this.StatusCode(500, response) :> IActionResult
        }

    [<HttpGet("{stageId}")>]
    member this.GetStageById(stageId: int64) =
        task {
            let! stage =
                context.Stages
                    .Include(fun s -> s.ChildStages) // Load child stages
                    .Include(fun s -> s.Positions) // Load positions
                    .FirstOrDefaultAsync(fun s -> s.StageId = stageId)

            if isNull stage then
                let response = 
                    { Success = false
                      Data = None
                      Error = Some "Stage not found" }
                return this.NotFound(response) :> IActionResult
            else
                let response = 
                    { Success = true
                      Data = Some 
                          {| 
                              StageId = stage.StageId
                              StageDescr = stage.StageDescr
                              StageCode = stage.StageCode
                              ChildStages = stage.ChildStages
                              Positions = stage.Positions
                          |}
                      Error = None }
                return this.Ok(response) :> IActionResult
        }

    [<HttpPost>]
    member this.CreateStage
        ([<FromBody>] payload: 
            {| StageName: string
               StageCode: string
               ParentStageId: int64 option |}) =
        task {
            if String.IsNullOrWhiteSpace(payload.StageName) || String.IsNullOrWhiteSpace(payload.StageCode) then
                let response = 
                    { Success = false
                      Data = None
                      Error = Some "Stage name and code cannot be empty" }
                return this.BadRequest(response) :> IActionResult
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
                    let response = 
                        { Success = true
                          Data = Some 
                              {| 
                                  StageId = newStage.StageId
                                  StageDescr = newStage.StageDescr
                                  StageCode = newStage.StageCode
                                  RowsAffected = rowsAffected
                              |}
                          Error = None }
                    return this.Ok(response) :> IActionResult
                with
                | ex -> 
                    let response = 
                        { Success = false
                          Data = None
                          Error = Some (String.Format("An error occurred: {0}", ex.Message)) }
                    return this.StatusCode(500, response) :> IActionResult
        }

    [<HttpDelete("{stageId}")>]
    member this.DeleteStage(stageId: int64) =
        task {
            let! stage = context.Stages.FirstOrDefaultAsync(fun s -> s.StageId = stageId)

            if isNull stage then
                let response = 
                    { Success = false
                      Data = None
                      Error = Some "Stage not found" }
                return this.NotFound(response) :> IActionResult
            else
                context.Stages.Remove(stage) |> ignore
                try
                    let rowsAffected = context.SaveChanges()
                    let response = 
                        { Success = true
                          Data = Some 
                              {| 
                                  StageId = stage.StageId
                                  RowsAffected = rowsAffected
                              |}
                          Error = None }
                    return this.Ok(response) :> IActionResult
                with
                | ex -> 
                    let response = 
                        { Success = false
                          Data = None
                          Error = Some (String.Format("An error occurred: {0}", ex.Message)) }
                    return this.StatusCode(500, response) :> IActionResult
        }

[<Route("api/positions")>]
[<ApiController>]
type PositionsController(context: MRPContext) =
    inherit ControllerBase()

    [<HttpPost>]
    member this.CreatePosition([<FromBody>] position: Position) =
        task {
            if
                String.IsNullOrWhiteSpace(position.PositionDescr)
                || String.IsNullOrWhiteSpace(position.PositionCode)
            then
                let response = 
                    { Success = false
                      Data = None
                      Error = Some "Position Description or Code cannot be empty" }
                return this.BadRequest(response) :> IActionResult
            else
                let! stage = context.Stages.FirstOrDefaultAsync(fun s -> s.StageId = position.StageId)

                if isNull stage then
                    let response = 
                        { Success = false
                          Data = None
                          Error = Some "Invalid StageId" }
                    return this.BadRequest(response) :> IActionResult
                else
                    context.Positions.Add(position) |> ignore
                    try
                        let rowsAffected = context.SaveChanges()
                        let response = 
                            { Success = true
                              Data = Some 
                                  {| 
                                      PositionId = position.PosId
                                      PositionDescr = position.PositionDescr
                                      PositionCode = position.PositionCode
                                      StageId = position.StageId
                                      RowsAffected = rowsAffected
                                  |}
                              Error = None }
                        return this.Ok(response) :> IActionResult
                    with
                    | ex -> 
                        let response = 
                            { Success = false
                              Data = None
                              Error = Some (String.Format("An error occurred: {0}", ex.Message)) }
                        return this.StatusCode(500, response) :> IActionResult
        }

[<Route("api/products")>]
[<ApiController>]
type ProductsController(context: MRPContext) =
    inherit ControllerBase()

    [<HttpPost>]
    member this.CreateProduct([<FromBody>] product: Product) =
        task {
            if String.IsNullOrWhiteSpace(product.ProductName) then
                let response = 
                    { Success = false
                      Data = None
                      Error = Some "Product Name cannot be empty" }
                return this.BadRequest(response) :> IActionResult
            else
                context.Products.Add(product) |> ignore
                try
                    let rowsAffected = context.SaveChanges()
                    let response = 
                        { Success = true
                          Data = Some 
                              {| 
                                  ProductId = product.ProductId
                                  ProductName = product.ProductName
                                  RowsAffected = rowsAffected
                              |}
                          Error = None }
                    return this.Ok(response) :> IActionResult
                with
                | ex -> 
                    let response = 
                        { Success = false
                          Data = None
                          Error = Some (String.Format("An error occurred: {0}", ex.Message)) }
                    return this.StatusCode(500, response) :> IActionResult
        }
