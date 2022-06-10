module MyBook.Tests.CatalogTests

open System.Net
open System.Net.Http.Headers
open System.Text.Json
open Microsoft.AspNetCore.Mvc.Testing
open MyBook.Entity
open MyBook.WebApi
open Newtonsoft.Json
open Xunit
open MyBook.Tests.AuthorizeUserWithSub
open MyBook.Tests.AuthorizeUser

type responseBooks = List<Book>
type responseAuthors = List<Author>

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
        member this.``Get all premium books``() =
            let myFactory = new MyBookWebApplicationFactory()
            let client = myFactory.CreateClient()

            let token = AuthorizeUserWithSub
            client.DefaultRequestHeaders.Authorization <- new AuthenticationHeaderValue("Bearer", token)
            
            let response =
                client.GetAsync($"/Catalog/Premium")
                

            Assert.Equal(HttpStatusCode.OK, response.Result.StatusCode)

            let responseJson =
                response.Result.Content.ReadAsStringAsync().Result

            Assert.NotEmpty(responseJson)

            let responseData =
                JsonConvert.DeserializeObject<responseBooks> responseJson

            Assert.Equal(5, responseData.Length)
            
        
        [<Fact>]
        member this.``Get book by id``() =
            let myFactory = new MyBookWebApplicationFactory()
            let client = myFactory.CreateClient()
            
            let token = AuthorizeUser
            client.DefaultRequestHeaders.Authorization <- new AuthenticationHeaderValue("Bearer", token)

            let id =
                "2a4751dc-1779-4bd4-a876-dbafa232e5cf"

            let response =
                client.GetAsync($"/Catalog/GetBook/{id}")

            Assert.Equal(HttpStatusCode.OK, response.Result.StatusCode)

            let responseJson =
                response.Result.Content.ReadAsStringAsync().Result

            let responseData =
                JsonConvert.DeserializeObject<Book> responseJson

            Assert.Equal("Преступление и наказание", responseData.Title)
            
        [<Fact>]
        member this.``Get premium book by id with premium``() =
            let myFactory = new MyBookWebApplicationFactory()
            let client = myFactory.CreateClient()
            
            let token = AuthorizeUser
            client.DefaultRequestHeaders.Authorization <- new AuthenticationHeaderValue("Bearer", token)

            let id =
                "cca20620-56c4-40d3-bfc3-7d88bff9ea1f"

            let response =
                client.GetAsync($"/Catalog/GetBook/{id}")

            Assert.Equal(HttpStatusCode.OK, response.Result.StatusCode)

            let responseJson =
                response.Result.Content.ReadAsStringAsync().Result

            let responseData =
                JsonConvert.DeserializeObject<Book> responseJson

            Assert.Equal("1984", responseData.Title)

        [<Fact>]
        member this.``Get book by wrong id returns NotFound``() =
            let myFactory = new MyBookWebApplicationFactory()
            let client = myFactory.CreateClient()
            
            let token = AuthorizeUser
            client.DefaultRequestHeaders.Authorization <- new AuthenticationHeaderValue("Bearer", token)

            let id =
                "10005631-6f76-437a-a924-1c5ad5806a5e"

            let response =
                client.GetAsync($"/Catalog/GetBook/{id}")

            Assert.Equal(HttpStatusCode.NotFound, response.Result.StatusCode)

        [<Fact>]
        member this.``Get author by id``() =
            let client = this._factory.CreateClient()

            let id =
                "02788b50-5eae-42ce-a375-c0416840d687"

            let response =
                client.GetAsync($"/Catalog/GetAuthor/{id}")

            Assert.Equal(HttpStatusCode.OK, response.Result.StatusCode)

            let responseJson =
                response.Result.Content.ReadAsStringAsync().Result

            let responseData =
                JsonConvert.DeserializeObject<Author> responseJson

            Assert.Equal("Марк Мэнсон", responseData.FullName)

        [<Fact>]
        member this.``Get author by wrong id returns NotFound``() =
            let client = this._factory.CreateClient()

            let id =
                "10005631-6f76-437a-a924-1c5ad5806a5e"

            let response =
                client.GetAsync($"/Catalog/GetAuthor/{id}")

            Assert.Equal(HttpStatusCode.NotFound, response.Result.StatusCode)


        [<Theory>]
        [<InlineData(1, "НИ", 2)>]
        [<InlineData(1, "искусство", 1)>]
        [<InlineData(1, "тонкое", 1)>]
        [<InlineData(1, "Рагнарёк", 1)>]
        [<InlineData(1, "Обладать", 1)>]
        [<InlineData(1, "Преступление", 1)>]
        [<InlineData(1, "н", 4)>]
        [<InlineData(1, "ва", 0)>]
        member this.``Search books``(selectId: int, keyword: string, count: int) =
            let client = this._factory.CreateClient()

            let response =
                client.GetAsync($"/Catalog/Search/?selectId={selectId}&keyword={keyword}")

            Assert.Equal(HttpStatusCode.OK, response.Result.StatusCode)

            let responseJson =
                response.Result.Content.ReadAsStringAsync().Result

            Assert.NotEmpty(responseJson)

            let responseData =
                JsonConvert.DeserializeObject<responseBooks> responseJson

            Assert.Equal(count, responseData.Length)



        [<Theory>]
        [<InlineData(2, "ва", 0)>]
        [<InlineData(2, "марк", 1)>]
        [<InlineData(2, "а", 2)>]
        [<InlineData(2, "Антония", 1)>]
        [<InlineData(2, "Сьюзен", 1)>]
        [<InlineData(2, "и", 3)>]
        [<InlineData(2, "Достоевский", 1)>]
        [<InlineData(2, "авы", 0)>]
        [<InlineData(2, "ъ", 0)>]
        member this.``Search authors``(selectId: int, keyword: string, count: int) =
            let client = this._factory.CreateClient()

            let response =
                client.GetAsync($"/Catalog/Search/?selectId={selectId}&keyword={keyword}")

            Assert.Equal(HttpStatusCode.OK, response.Result.StatusCode)

            let responseJson =
                response.Result.Content.ReadAsStringAsync().Result

            Assert.NotEmpty(responseJson)

            let responseData =
                JsonConvert.DeserializeObject<responseAuthors> responseJson

            Assert.Equal(count, responseData.Length)

        [<Fact>]
        member this.``Wrong selectId``() =
            let client = this._factory.CreateClient()

            let response =
                client.GetAsync($"/Catalog/Search/?selectId={0}&keyword=кто")

            Assert.Equal(HttpStatusCode.NotFound, response.Result.StatusCode)

        [<Fact>]
        member this.``Wrong keyword``() =
            let client = this._factory.CreateClient()

            let response =
                client.GetAsync($"/Catalog/Search/?selectId={1}&keyword=")

            Assert.Equal(HttpStatusCode.NotFound, response.Result.StatusCode)
    end
