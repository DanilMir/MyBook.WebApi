namespace MyBook.Tests

open System
open System.Reflection
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Mvc.Testing
open Microsoft.EntityFrameworkCore
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.DependencyInjection.Extensions
open MyBook.DataAccess
open MyBook.WebApi

type MyBookWebApplicationFactory() =
    inherit WebApplicationFactory<Startup>()

    override this.ConfigureWebHost(builder: IWebHostBuilder) =
        ``base``.ConfigureWebHost(builder)
        builder.UseEnvironment "Development" |> ignore

        builder.ConfigureServices (fun ctx services ->
            services
            |> ServiceCollectionDescriptorExtensions.RemoveAll<DbContextOptions<ApplicationContext>>
            |> ServiceCollectionDescriptorExtensions.RemoveAll<ApplicationContext>
            |> (fun services ->
                EntityFrameworkServiceCollectionExtensions.AddDbContext<ApplicationContext>(
                    services,
                    (fun config ->
                        //                        let connectionString =
//                            ctx.Configuration.GetConnectionString("sqlConnection")

                        config.UseInMemoryDatabase("InMemoryDbForTesting")
                        |> ignore
                        //                        config.UseNpgsql(connectionString) |> ignore

                        config.UseOpenIddict() |> ignore)
                ))
            |> (fun services ->
                let sp = services.BuildServiceProvider()
                use scope = sp.CreateScope()

                let context =
                    scope.ServiceProvider.GetRequiredService<ApplicationContext>()

                context.Database.EnsureDeleted() |> ignore
                context.Database.EnsureCreated() |> ignore
                services)
            |> ignore)
        |> ignore
