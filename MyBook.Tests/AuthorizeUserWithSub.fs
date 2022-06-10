module MyBook.Tests.AuthorizeUserWithSub

open System.Collections.Generic
open System.Net.Http
open System.Text.Json
open MyBook.Tests.ResponseToken

let AuthorizeUserWithSub =
    let _factory =
        new MyBookWebApplicationFactory()

    let client = _factory.CreateClient()

    let values =
        [| KeyValuePair<string, string>("grant_type", "password")
           KeyValuePair<string, string>("username", "user@mybook.ru")
           KeyValuePair<string, string>("password", "qwe123QWE_") |]

    let content =
        new FormUrlEncodedContent(values)

    content.Headers.ContentType <- Headers.MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded")

    let response =
        client.PostAsync($"/Auth/Login", content)

    let responseJson =
        response.Result.Content.ReadAsStringAsync().Result

    let responseData =
        JsonSerializer.Deserialize<ResponseToken> responseJson

    responseData.access_token
