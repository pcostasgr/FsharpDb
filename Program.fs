open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting
open MrpService

[<EntryPoint>]
let main args =
    Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(fun webBuilder ->
            webBuilder.UseStartup<Startup>() |> ignore)
        .Build()
        .Run()
    0