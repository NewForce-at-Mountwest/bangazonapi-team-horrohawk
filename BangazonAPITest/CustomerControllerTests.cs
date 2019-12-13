using BangazonAPI.Models;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using System.Collections.Generic;

namespace BangazonAPITest
{
    public class CustomerControllerTests
    {
        // This is going to be our test customer instance that we create and delete to make sure everything works
        private Customer dummyCustomer { get; } = new Customer
        {
            firstName = "Joe",
            lastName = "Latte"
          
        };

        // We'll store our base url for this route as a private field to avoid typos
        private string url { get; } = "/api/customer";

        // Reusable method to create a new student in the database and return it
        public async Task<Customer> CreateDummyCustomer()
        {

            using (var client = new APIClientProvider().Client)
            {

                // Serialize the C# object into a JSON string
                string JoeCustomerAsJSON = JsonConvert.SerializeObject(dummyCustomer);


                // Use the client to send the request and store the response
                HttpResponseMessage response = await client.PostAsync(
                    url,
                    new StringContent(JoeCustomerAsJSON, Encoding.UTF8, "application/json")
                );

                // Store the JSON body of the response
                string responseBody = await response.Content.ReadAsStringAsync();

                // Deserialize the JSON into an instance of Customer
                Customer newlyCreatedCustomer = JsonConvert.DeserializeObject<Customer>(responseBody);

                return newlyCreatedCustomer;
            }
        }
        // Reusable method to delete a customer from the database
        public async Task deleteDummyCustomer(Customer customerToDelete)
        {
            using (HttpClient client = new APIClientProvider().Client)
            {
                HttpResponseMessage deleteResponse = await client.DeleteAsync($"{url}/{customerToDelete.id}");

            }

        }

        //Tests START HERE
        [Fact]
        public async Task Create_Customer()
        {
            using (var client = new APIClientProvider().Client)
            {
                // Create a new student in the db
               Customer newJoeCustomer = await CreateDummyCustomer();

                // Try to get it again
                HttpResponseMessage response = await client.GetAsync($"{url}/{newJoeCustomer.id}");
                response.EnsureSuccessStatusCode();

                // Turn the response into JSON
                string responseBody = await response.Content.ReadAsStringAsync();

                // Turn the JSON into C#
                Customer newCustomer = JsonConvert.DeserializeObject<Customer>(responseBody);

                // Make sure it's really there
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal(dummyCustomer.firstName, newCustomer.firstName);
                Assert.Equal(dummyCustomer.lastName, newCustomer.lastName);

                // Clean up after ourselves
                await deleteDummyCustomer(newCustomer);

            }

        }
        [Fact]

        public async Task Delete_Customer()
        {
            // Note: with many of these methods, I'm creating dummy data and then testing to see if I can delete it. I'd rather do that for now than delete something else I (or a user) created in the database, but it's not essential-- we could test deleting anything 

            // Create a new customer in the db
            Customer newJoeCustomer = await CreateDummyCustomer();

            // Delete it
            await deleteDummyCustomer(newJoeCustomer);

            using (var client = new APIClientProvider().Client)
            {
                // Try to get it again
                HttpResponseMessage response = await client.GetAsync($"{url}{newJoeCustomer.id}");

                // Make sure it's really gone
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            }
        }
        // test to get all customers
        [Fact]
        public async Task Get_All_Customer()
        {

            using (var client = new APIClientProvider().Client)
            {

                // Try to get all of the customers from /api/customer
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                // Convert to JSON
                string responseBody = await response.Content.ReadAsStringAsync();

                // Convert from JSON to C#
                List<Customer> customers = JsonConvert.DeserializeObject<List<Customer>>(responseBody);

                // Make sure we got back a 200 OK Status and that there are more than 0 customers in our database
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.True(customers.Count > 0);

            }
        }
        //Test get single customer
        [Fact]
        public async Task Get_Single_Customer()
        {
            using (HttpClient client = new APIClientProvider().Client)
            {
                // Create a dummy customer
               Customer newJoeCustomer = await CreateDummyCustomer();

                // Try to get it
                HttpResponseMessage response = await client.GetAsync($"{url}/{newJoeCustomer.id}");
                response.EnsureSuccessStatusCode();

                // Turn the response into JSON
                string responseBody = await response.Content.ReadAsStringAsync();

                // Turn the JSON into C#
                Customer JoeCustomerFromDB = JsonConvert.DeserializeObject<Customer>(responseBody);

                // Did we get back what we expected to get back? 
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal(dummyCustomer.firstName, JoeCustomerFromDB.firstName);
                Assert.Equal(dummyCustomer.lastName, JoeCustomerFromDB.lastName);

                // Clean up after ourselves-- delete the dummy customer we just created
                await deleteDummyCustomer(JoeCustomerFromDB);

            }
        }
        [Fact]
        public async Task Update_Customer()
        {

            using (var client = new APIClientProvider().Client)
            {
                // Create a dummy customer
                Customer newJoeCustomer = await CreateDummyCustomer();

                // Make a new title and assign it to our dummy customer
                string newFirstName = "JOEY MCFRENCH ROAST";
                newJoeCustomer.firstName = newFirstName;

                // Convert it to JSON
                string modifiedJoeCustomerAsJSON = JsonConvert.SerializeObject(newJoeCustomer);

                // Try to PUT the newly edited customer
                var response = await client.PutAsync(
                    $"{url}/{newJoeCustomer.id}",
                    new StringContent(modifiedJoeCustomerAsJSON, Encoding.UTF8, "application/json")
                );

                // See what comes back from the PUT. Is it a 204? 
                string responseBody = await response.Content.ReadAsStringAsync();
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

                // Get the edited customer back from the database after the PUT
                var getModifiedCustomer = await client.GetAsync($"{url}/{newJoeCustomer.id}");
                getModifiedCustomer.EnsureSuccessStatusCode();

                // Convert it to JSON
                string getCustomerBody = await getModifiedCustomer.Content.ReadAsStringAsync();

                // Convert it from JSON to C#
               Customer newlyEditedCustomer = JsonConvert.DeserializeObject<Customer>(getCustomerBody);

                // Make sure the title was modified correctly
                Assert.Equal(HttpStatusCode.OK, getModifiedCustomer.StatusCode);
                Assert.Equal(newFirstName, newlyEditedCustomer.firstName);

                // Clean up after yourself
                await deleteDummyCustomer(newlyEditedCustomer);
            }
        }
        [Fact]
        public async Task Test_Get_NonExitent_Customer_Fails()
        {

            using (var client = new APIClientProvider().Client)
            {
                // Try to get a customer with an Id that could never exist
                HttpResponseMessage response = await client.GetAsync($"{url}/00000000");

                // It should bring back a 204 no content error
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            }
        }
        [Fact]
        public async Task Test_Delete_NonExistent_Customer_Fails()
        {
            using (var client = new APIClientProvider().Client)
            {
                // Try to delete an Id that shouldn't exist 
                HttpResponseMessage deleteResponse = await client.DeleteAsync($"{url}0000000000");

                Assert.False(deleteResponse.IsSuccessStatusCode);
                Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);
            }
        }
    }
}
