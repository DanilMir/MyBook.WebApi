module MyBook.Tests.CatalogTests

open System.Net
open Microsoft.AspNetCore.Mvc.Testing
open MyBook.WebApi
open Xunit

[<Fact>]
let ``Get all books`` () =
    let _factory = new MyBookWebApplicationFactory()
    let client = _factory.CreateClient()
    let response = client.GetAsync($"/Catalog/GetAllBooks")
    Assert.Equal(HttpStatusCode.OK, response.Result.StatusCode)