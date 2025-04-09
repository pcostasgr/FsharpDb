namespace MrpService.Models

type ApiResponse<'T> =
    {
        Success: bool
        Data: 'T option
        Error: string option
    }
