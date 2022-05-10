module MyBook.Tests.ExampleTest

open System.Net
open Xunit

type ExampleTests(factory: MyBookWebApplicationFactory) =
    class
        member this._factory = factory

        interface IClassFixture<MyBookWebApplicationFactory>

        //        [<Fact>]
//        member this. ``Get all books`` () =
//            let client = this._factory.CreateClient()
//            let response = client.GetAsync($"/Catalog/GetAllBooks")
//            Assert.Equal(HttpStatusCode.OK, response.Result.StatusCode)

    end
