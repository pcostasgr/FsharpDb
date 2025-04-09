namespace MrpService.Models

open System
open System.Collections.Generic
open System.ComponentModel.DataAnnotations

type Stage(stageName: string, stageCode: string) =
    member val StageId = 0L with get, set
    member val StageName = stageName with get, set
    [<Required>]
    [<StringLength(50)>]
    member val StageCode = stageCode with get, set
    member val ParentStageId = Nullable<int64>() with get, set
    member val ChildStages = List<Stage>() with get, set
    member val Positions = List<Position>() with get, set
