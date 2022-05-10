module MyBook.Tests.CatalogTests

open System.Net
open Microsoft.AspNetCore.Mvc.Testing
open MyBook.WebApi
open Xunit

[<Collection("Catalog tests")>]
type CatalogControllerTests(factory: MyBookWebApplicationFactory) =
    class
        member this._factory = factory
                
        interface IClassFixture<MyBookWebApplicationFactory>

        [<Fact>]
        member this. ``Get all books`` () =
            let client = this._factory.CreateClient()
            let response = client.GetAsync($"/Catalog/GetAllBooks")
            Assert.Equal(HttpStatusCode.OK, response.Result.StatusCode)
    end