module MyBook.Tests.ProfileTests

open System.Collections.Generic
open System.IO
open System.Net
open System.Net.Http
open System.Net.Http.Headers
open System.Reflection
open System.Text
open Microsoft.AspNetCore.Http
open MyBook.Models
open Newtonsoft.Json
open Xunit
open MyBook.Tests.AuthorizeUser


type ProfileTests(factory: MyBookWebApplicationFactory) =
    class
        member this._factory = factory

        interface IClassFixture<MyBookWebApplicationFactory>

        [<Fact>]
        member this.GetCurrentUser() =
            let client = this._factory.CreateClient()
            let token = AuthorizeUser
            client.DefaultRequestHeaders.Authorization <- new AuthenticationHeaderValue("Bearer", token)

            let response =
                client.GetAsync($"/Profile/GetCurrentUser")

            Assert.Equal(HttpStatusCode.OK, response.Result.StatusCode)

        [<Fact>]
        member this.``Change password with valid password``() =
            let client = this._factory.CreateClient()
            let token = AuthorizeUser
            client.DefaultRequestHeaders.Authorization <- new AuthenticationHeaderValue("Bearer", token)

            let contestObject =
                new ChangePasswordViewModel()

            contestObject.NewPassword <- "qwe123QWE__"
            contestObject.OldPassword <- "qwe123QWE_"

            let contestJson =
                JsonConvert.SerializeObject(contestObject)

            let content =
                new StringContent(contestJson, Encoding.UTF8, "application/json")

            let response =
                client.PostAsync($"/Profile/ChangePassword", content)

            Assert.Equal(HttpStatusCode.OK, response.Result.StatusCode)

        [<Fact>]
        member this.``Change password with invalid password return conflict``() =
            let client = this._factory.CreateClient()
            let token = AuthorizeUser
            client.DefaultRequestHeaders.Authorization <- new AuthenticationHeaderValue("Bearer", token)

            let contestObject =
                new ChangePasswordViewModel()

            contestObject.NewPassword <- "qwe123QWE__"
            contestObject.OldPassword <- "qwe123QWE__"

            let contestJson =
                JsonConvert.SerializeObject(contestObject)

            let content =
                new StringContent(contestJson, Encoding.UTF8, "application/json")

            let response =
                client.PostAsync($"/Profile/ChangePassword", content)

            Assert.Equal(HttpStatusCode.Conflict, response.Result.StatusCode)


        [<Fact>]
        member this.``Remove From Favorites``() =
            let client = this._factory.CreateClient()
            let token = AuthorizeUser
            client.DefaultRequestHeaders.Authorization <- new AuthenticationHeaderValue("Bearer", token)

            let id =
                "3cb92c37-ec67-4720-af23-d7f4d4096109"

            let response =
                client.GetAsync($"/Profile/RemoveFromFavorites?id={id}")

            Assert.Equal(HttpStatusCode.OK, response.Result.StatusCode)


        [<Fact>]
        member this.``Get Favorites``() =
            let client = this._factory.CreateClient()
            let token = AuthorizeUser
            client.DefaultRequestHeaders.Authorization <- new AuthenticationHeaderValue("Bearer", token)

            let response =
                client.GetAsync($"/Profile/Favorites")

            Assert.Equal(HttpStatusCode.OK, response.Result.StatusCode)
            
            
        [<Fact>]
        member this.``Add To Favorites``() =
            let myFactory = new MyBookWebApplicationFactory()
            let client = myFactory.CreateClient()
            let token = AuthorizeUser
            client.DefaultRequestHeaders.Authorization <- new AuthenticationHeaderValue("Bearer", token)

            let id =
               "8faa5631-6f76-437a-a924-1c5ad5806a5e"
            
            let response =
                client.GetAsync($"/Profile/AddToFavorites?id={id}")

            Assert.Equal(HttpStatusCode.OK, response.Result.StatusCode)
            
        
        [<Fact>]
        member this.``Add To Favorites a nonexistent book should return BadRequest``() =
            let myFactory = new MyBookWebApplicationFactory()
            let client = myFactory.CreateClient()
            let token = AuthorizeUser
            client.DefaultRequestHeaders.Authorization <- new AuthenticationHeaderValue("Bearer", token)

            let id =
               "8faa5631-6f76-437a-a924-1c5ad5826a5e"
            
            let response =
                client.GetAsync($"/Profile/AddToFavorites?id={id}")

            Assert.Equal(HttpStatusCode.BadRequest, response.Result.StatusCode)
           
        
        [<Fact>]
        member this.``Remove from favorites a nonexistent book should return BadRequest``() =
            let myFactory = new MyBookWebApplicationFactory()
            let client = myFactory.CreateClient()
            let token = AuthorizeUser
            client.DefaultRequestHeaders.Authorization <- new AuthenticationHeaderValue("Bearer", token)

            let id =
               "8faa5631-6f76-437a-a924-1c5ad5826a5e"
            
            let response =
                client.GetAsync($"/Profile/RemoveFromFavorites?id={id}")

            Assert.Equal(HttpStatusCode.BadRequest, response.Result.StatusCode)
        
        [<Fact>]
        member this.``Remove from favorites a book that not in fav should return BadRequest``() =
            let myFactory = new MyBookWebApplicationFactory()
            let client = myFactory.CreateClient()
            let token = AuthorizeUser
            client.DefaultRequestHeaders.Authorization <- new AuthenticationHeaderValue("Bearer", token)

            let id =
               "2a4751dc-1779-4bd4-a876-dbafa232e5cf"
            
            let response =
                client.GetAsync($"/Profile/RemoveFromFavorites?id={id}")

            Assert.Equal(HttpStatusCode.BadRequest, response.Result.StatusCode)
        
        [<Fact>]
        member this.``EditProfile``() =

            let myFactory = new MyBookWebApplicationFactory()
            let client = myFactory.CreateClient()
            let token = AuthorizeUser
            client.DefaultRequestHeaders.Authorization <- new AuthenticationHeaderValue("Bearer", token)

            let content =
                new MultipartFormDataContent()
                
            content.Add(new StringContent("hb5"), "name")
            content.Add(new StringContent("hb4"), "lastname")
            content.Add(new StringContent("2@mail.ru"), "email")

            

            let response =
                client.PostAsync($"/Profile/EditProfile", content)

            Assert.Equal(HttpStatusCode.OK, response.Result.StatusCode)
    end
