module MyBook.Tests.AuthorizationTests

open System.Collections.Generic
open System.Net
open System.Net.Http
open System.Text.Json
open Microsoft.AspNetCore.Mvc.Testing
open Microsoft.Extensions.DependencyInjection
open MyBook.DataAccess
open MyBook.Tests.ResponseToken
open MyBook.WebApi
open System.Linq
open Xunit

type ResponseError =
    { error: string
      error_description: string
      error_uri: string }


type AuthorizationControllerTests(factory: MyBookWebApplicationFactory) =
    class
        member this._factory = factory


        interface IClassFixture<MyBookWebApplicationFactory>

        [<Fact>]
        member this.``Register user with valid data should create new user``() =

            let myFactory = new MyBookWebApplicationFactory()
            let client = myFactory.CreateClient()

            let form =
                [| KeyValuePair<string, string>("grant_type", "password")
                   KeyValuePair<string, string>("username", "hb4")
                   KeyValuePair<string, string>("password", "qwe123QWE_")
                   KeyValuePair<string, string>("name", "Name")
                   KeyValuePair<string, string>("lastname", "Lastname")
                   KeyValuePair<string, string>("email", "hb3@mybook.ru") |]

            let content =
                new FormUrlEncodedContent(form)

            content.Headers.ContentType <- Headers.MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded")

            let response =
                client.PostAsync($"/Auth/SignUp", content)

            Assert.Equal(HttpStatusCode.OK, response.Result.StatusCode)

            let responseJson =
                response.Result.Content.ReadAsStringAsync().Result

            let responseData =
                JsonSerializer.Deserialize<ResponseToken> responseJson

            Assert.NotNull(responseData.access_token)


        [<Fact>]
        member this.``Register user with invalid data should return BadRequest``() =

            let client = this._factory.CreateClient()

            let form =
                [| KeyValuePair<string, string>("grant_type", "password")
                   KeyValuePair<string, string>("username", "hb5")
                   KeyValuePair<string, string>("password", "123")
                   KeyValuePair<string, string>("name", "Name")
                   KeyValuePair<string, string>("lastname", "Lastname")
                   KeyValuePair<string, string>("email", "hb5@mybook.ru") |]

            let content =
                new FormUrlEncodedContent(form)

            content.Headers.ContentType <- Headers.MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded")

            let response =
                client.PostAsync($"/Auth/SignUp", content)

            Assert.Equal(HttpStatusCode.BadRequest, response.Result.StatusCode)


        [<Fact>]
        member this.``Register with already registered username should return BadRequest``() =

            let client = this._factory.CreateClient()

            let form =
                [| KeyValuePair<string, string>("grant_type", "password")
                   KeyValuePair<string, string>("username", "admin@mybook.ru")
                   KeyValuePair<string, string>("password", "123")
                   KeyValuePair<string, string>("name", "Name")
                   KeyValuePair<string, string>("lastname", "Lastname")
                   KeyValuePair<string, string>("email", "hb5@mybook.ru") |]

            let content =
                new FormUrlEncodedContent(form)

            content.Headers.ContentType <- Headers.MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded")

            let response =
                client.PostAsync($"/Auth/SignUp", content)

            Assert.Equal(HttpStatusCode.BadRequest, response.Result.StatusCode)

        [<Fact>]
        member this.``Correct Login should returns valid JWT``() =

            let client = this._factory.CreateClient()

            let form =
                [| KeyValuePair<string, string>("grant_type", "password")
                   KeyValuePair<string, string>("username", "admin@mybook.ru")
                   KeyValuePair<string, string>("password", "qwe123QWE_") |]

            let content =
                new FormUrlEncodedContent(form)

            content.Headers.ContentType <- Headers.MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded")

            let response =
                client.PostAsync($"/Auth/Login", content)

            Assert.Equal(HttpStatusCode.OK, response.Result.StatusCode)

            let responseJson =
                response.Result.Content.ReadAsStringAsync().Result

            let responseData =
                JsonSerializer.Deserialize<ResponseToken> responseJson

            Assert.NotNull(responseData.access_token)


        [<Fact>]
        member this.``Incorrect user name data should return BadRequest ``() =

            let client = this._factory.CreateClient()

            let form =
                [| KeyValuePair<string, string>("grant_type", "password")
                   KeyValuePair<string, string>("username", "rick@mybook.ru")
                   KeyValuePair<string, string>("password", "qwe123QWE_") |]

            let content =
                new FormUrlEncodedContent(form)

            content.Headers.ContentType <- Headers.MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded")

            let response =
                client.PostAsync($"/Auth/Login", content)

            Assert.Equal(HttpStatusCode.BadRequest, response.Result.StatusCode)



        [<Fact>]
        member this.``Incorrect password data should return BadRequest ``() =

            let client = this._factory.CreateClient()

            let form =
                [| KeyValuePair<string, string>("grant_type", "password")
                   KeyValuePair<string, string>("username", "admin@mybook.ru")
                   KeyValuePair<string, string>("password", "qwe123QWE") |]

            let content =
                new FormUrlEncodedContent(form)

            content.Headers.ContentType <- Headers.MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded")

            let response =
                client.PostAsync($"/Auth/Login", content)

            Assert.Equal(HttpStatusCode.BadRequest, response.Result.StatusCode)
    end
