namespace MrpService

open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.EntityFrameworkCore
open Microsoft.AspNetCore.Hosting
open MrpService.Data

type Startup(configuration: IConfiguration) =
    member _.Configuration = configuration

    member _.ConfigureServices(services: IServiceCollection) =
        services.AddControllers() |> ignore
        services.AddDbContext<MRPContext>(fun options ->
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")) |> ignore
        ) |> ignore

    member _.Configure(app: IApplicationBuilder, env: IWebHostEnvironment) =
        if env.EnvironmentName = "Development" then
            app.UseDeveloperExceptionPage() |> ignore
        else
            app.UseExceptionHandler("/error") |> ignore

        app.UseRouting()
           .UseEndpoints(fun endpoints ->
               endpoints.MapControllers() |> ignore) |> ignore