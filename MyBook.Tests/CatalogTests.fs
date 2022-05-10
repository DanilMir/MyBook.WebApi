module MyBook.Tests.CatalogTests

open System.Net
open System.Text.Json
open Microsoft.AspNetCore.Mvc.Testing
open MyBook.Entity
open MyBook.WebApi
open Newtonsoft.Json
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
                JsonConvert.DeserializeObject<responseBooks> responseJson

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
                JsonConvert.DeserializeObject<responseBooks> responseJson

            Assert.Equal(1, responseData.Length)
            
        [<Fact>]
        member this.``Get book by id``() =
            let client = this._factory.CreateClient()

            let id = "8faa5631-6f76-437a-a924-1c5ad5806a5e";
            let response =
                client.GetAsync($"/Catalog/GetBook/{id}")

            Assert.Equal(HttpStatusCode.OK, response.Result.StatusCode)
            
            let responseJson = response.Result.Content.ReadAsStringAsync().Result
            let responseData = JsonConvert.DeserializeObject<Book> responseJson
            Assert.Equal("Тонкое искусство пофигизма", responseData.Title)
            
        [<Fact>]
        member this.``Get book by wrong id returns NotFound``() =
            let client = this._factory.CreateClient()

            let id = "10005631-6f76-437a-a924-1c5ad5806a5e";
            let response =
                client.GetAsync($"/Catalog/GetBook/{id}")

            Assert.Equal(HttpStatusCode.NotFound, response.Result.StatusCode)
            
        [<Fact>]
        member this.``Get author by id``() =
            let client = this._factory.CreateClient()

            let id = "02788b50-5eae-42ce-a375-c0416840d687";
            let response =
                client.GetAsync($"/Catalog/GetAuthor/{id}")

            Assert.Equal(HttpStatusCode.OK, response.Result.StatusCode)
            
            let responseJson = response.Result.Content.ReadAsStringAsync().Result
            let responseData = JsonConvert.DeserializeObject<Author> responseJson
            Assert.Equal("Марк Мэнсон", responseData.FullName)
            
        [<Fact>]
        member this.``Get author by wrong id returns NotFound``() =
            let client = this._factory.CreateClient()

            let id = "10005631-6f76-437a-a924-1c5ad5806a5e";
            let response =
                client.GetAsync($"/Catalog/GetAuthor/{id}")

            Assert.Equal(HttpStatusCode.NotFound, response.Result.StatusCode)
            
            
        [<Fact>]
        member this.``Search``() =
            let client = this._factory.CreateClient()

            let id = "02788b50-5eae-42ce-a375-c0416840d687";
            let response =
                client.GetAsync($"/Catalog/GetAuthor/{id}")

            Assert.Equal(HttpStatusCode.OK, response.Result.StatusCode)
            
            let responseJson = response.Result.Content.ReadAsStringAsync().Result
            let responseData = JsonConvert.DeserializeObject<Author> responseJson
            Assert.Equal("Марк Мэнсон", responseData.FullName)
    end
