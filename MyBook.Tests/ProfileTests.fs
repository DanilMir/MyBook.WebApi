module MyBook.Tests.ProfileTests

open System.Net
open System.Net.Http
open System.Net.Http.Headers
open System.Text
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
    end
