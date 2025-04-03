namespace MrpService.Models

open System
open System.ComponentModel.DataAnnotations
open System.ComponentModel.DataAnnotations.Schema
open System.Collections.Generic
open System.ComponentModel.DataAnnotations
open System.Runtime.Serialization
open Microsoft.EntityFrameworkCore
open EntityFrameworkCore.FSharp.Extensions

[<CLIMutable>]
[<Table("Positions")>]
type Position = {
    [<Key>]
    [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>]
    PosId: int

    [<StringLength(10)>]
    PositionCode: string

    [<StringLength(50)>]
    PositionDescr: string
    StageId: int64  
}


[<Table("Stages")>]
[<AllowNullLiteral>]
type Stage( stageDescr: string,stageCode:string ) =
    [<Key>]
     [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] 
    member val StageId:Int64=0 with get, set

    [<StringLength(50)>]
    member val StageDescr = stageDescr with get, set

    [<StringLength(50)>]
    member val StageCode = stageCode with get, set

    [<ForeignKey("ParentStage")>]
    member val ParentStageId: Nullable<Int64> =  Nullable<Int64>() with get, set


    member val Positions: List<Position> = List<Position>() with get, set

    member val ChildStages: List<Stage> = List<Stage>() with get, set

[<NotMapped>]
type Mrp = {
    Positions:  List<Position> 
    Stages: List<Stage> 
}

type Product =
    {
        [<Key>]
        [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>] // Set ProductId as an identity column
        ProductId: int64

        [<Required>]
        [<MaxLength(100)>]
        ProductName: string
    }
