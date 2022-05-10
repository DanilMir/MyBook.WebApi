module MyBook.Tests.CatalogTests

open System.Net
open System.Text.Json
open Microsoft.AspNetCore.Mvc.Testing
open MyBook.Entity
open MyBook.WebApi
open Xunit

type responseBooks = List<Book>

[<Collection("Catalog tests")>]
type CatalogControllerTests(factory: MyBookWebApplicationFactory) =
    class
        member this._factory = factory

        interface IClassFixture<MyBookWebApplicationFactory>

        [<Fact>]
        member this.``Get all books``() =
            let client = this._factory.CreateClient()

            let response =
                client.GetAsync($"/Catalog/GetAllBooks")

            Assert.Equal(HttpStatusCode.OK, response.Result.StatusCode)

            let responseJson =
                response.Result.Content.ReadAsStringAsync().Result

            Assert.NotEmpty(responseJson)

            let responseData =
                JsonSerializer.Deserialize<responseBooks> responseJson

            Assert.Equal(6, responseData.Length)
            
            
        [<Fact>]
        member this.``Get all free books``() =
            let client = this._factory.CreateClient()

            let response =
                client.GetAsync($"/Catalog/GetAllFreeBooks")

            Assert.Equal(HttpStatusCode.OK, response.Result.StatusCode)

            let responseJson =
                response.Result.Content.ReadAsStringAsync().Result

            Assert.NotEmpty(responseJson)

            let responseData =
                JsonSerializer.Deserialize<responseBooks> responseJson

            Assert.Equal(1, responseData.Length)
    
    end
