module MyBook.Tests.SubTests

open System.Net
open System.Net.Http.Headers
open Xunit
open MyBook.Tests.AuthorizeUser
open MyBook.Tests.AuthorizeUserWithSub

type SubTests(factory: MyBookWebApplicationFactory) =
    class
        member this._factory = factory

        interface IClassFixture<MyBookWebApplicationFactory>

        
        [<Fact>]
        member this. ``Pay for sub should return OK`` () =
            let client = this._factory.CreateClient()
            
            let token = AuthorizeUser
            client.DefaultRequestHeaders.Authorization <- new AuthenticationHeaderValue("Bearer", token)
            
            let response = client.GetAsync($"/Sub/Pay?subId=2")
            Assert.Equal(HttpStatusCode.OK, response.Result.StatusCode)
            
        [<Fact>]
        member this. ``Pay for sub if already subscriber should return Conflict`` () =
            let myFactory = new MyBookWebApplicationFactory()
            let client = myFactory.CreateClient()
            
            let token = AuthorizeUserWithSub
            client.DefaultRequestHeaders.Authorization <- new AuthenticationHeaderValue("Bearer", token)
            
            let response = client.GetAsync($"/Sub/Pay?subId=2")
            Assert.Equal(HttpStatusCode.Conflict, response.Result.StatusCode)
            
            
        [<Fact>]
        member this. ``Reset sub should return conflict`` () =
            let myFactory = new MyBookWebApplicationFactory()
            let client = myFactory.CreateClient()
            
            let token = AuthorizeUser
            client.DefaultRequestHeaders.Authorization <- new AuthenticationHeaderValue("Bearer", token)
            
            let response = client.GetAsync($"/Sub/ResetSub")
            Assert.Equal(HttpStatusCode.Conflict, response.Result.StatusCode)
            
            
        [<Fact>]
        member this. ``Reset sub should return ok`` () =
            let myFactory = new MyBookWebApplicationFactory()
            let client = myFactory.CreateClient()
            
            let token = AuthorizeUserWithSub
            client.DefaultRequestHeaders.Authorization <- new AuthenticationHeaderValue("Bearer", token)
            
            let response = client.GetAsync($"/Sub/ResetSub")
            Assert.Equal(HttpStatusCode.OK, response.Result.StatusCode)

    end