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
    public class OrderControllerTest
    {
        // This is going to be the test order instance that is created and deleted to make sure everything works
        private Order dummyOrder { get; } = new Order
        {
            customerId = 2,
            paymentTypeId = 3
        };

        // We'll store our base url for this route as a private field to avoid typos
        private string url { get; } = "/api/order";


        // Reusable method to create a new order in the database and return it
        public async Task<Order> CreateDummyOrder()
        {

            using (var client = new APIClientProvider().Client)
            {

                // Serialize the C# object into a JSON string
                string newOrderAsJSON = JsonConvert.SerializeObject(dummyOrder);


                // Use the client to send the request and store the response
                HttpResponseMessage response = await client.PostAsync(
                    url,
                    new StringContent(newOrderAsJSON, Encoding.UTF8, "application/json")
                );

                // Store the JSON body of the response
                string responseBody = await response.Content.ReadAsStringAsync();

                // Deserialize the JSON into an instance of order
                Order newlyCreatedOrder = JsonConvert.DeserializeObject<Order>(responseBody);

                return newlyCreatedOrder;
            }
        }

        // Reusable method to deelte a order from the database
        public async Task deleteDummyOrder(Order orderToDelete)
        {
            using (HttpClient client = new APIClientProvider().Client)
            {
                HttpResponseMessage deleteResponse = await client.DeleteAsync($"{url}/{orderToDelete.id}");

            }

        }


        /* TESTS START HERE */


        [Fact]
        public async Task Create_Order()
        {
            using (var client = new APIClientProvider().Client)
            {
                // Create a new order in the db
                Order neworder = await CreateDummyOrder();

                // Try to get it again
                HttpResponseMessage response = await client.GetAsync($"{url}/{neworder.id}");
                response.EnsureSuccessStatusCode();

                // Turn the response into JSON
                string responseBody = await response.Content.ReadAsStringAsync();

                // Turn the JSON into C#
                Order newOrder = JsonConvert.DeserializeObject<Order>(responseBody);

                // Make sure it's really there
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal(dummyOrder.customerId, newOrder.customerId);
                Assert.Equal(dummyOrder.paymentTypeId, newOrder.paymentTypeId);

                // Clean up after ourselves
                await deleteDummyOrder(newOrder);

            }

        }


        [Fact]

        public async Task Delete_Order()
        {
            // Note: with many of these methods, I'm creating dummy data and then testing to see if I can delete it. I'd rather do that for now than delete something else I (or a user) created in the database, but it's not essential-- we could test deleting anything 

            // Create a new order in the db
            Order newOrder = await CreateDummyOrder();

            // Delete it
            await deleteDummyOrder(newOrder);

            using (var client = new APIClientProvider().Client)
            {
                // Try to get it again
                HttpResponseMessage response = await client.GetAsync($"{url}{newOrder.id}");

                // Make sure it's really gone
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            }
        }

        [Fact]
        public async Task Get_All_Orders()
        {

            using (var client = new APIClientProvider().Client)
            {

                // Try to get all of the orders from /api/orders
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                // Convert to JSON
                string responseBody = await response.Content.ReadAsStringAsync();

                // Convert from JSON to C#
                List<Order> orders = JsonConvert.DeserializeObject<List<Order>>(responseBody);

                // Make sure we got back a 200 OK Status and that there are more than 0 orders in our database
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.True(orders.Count > 0);

            }
        }

        [Fact]
        public async Task Get_Single_Order()
        {
            using (HttpClient client = new APIClientProvider().Client)
            {
                // Create a dummy order
                Order newOrder = await CreateDummyOrder();

                // Try to get it
                HttpResponseMessage response = await client.GetAsync($"{url}/{newOrder.id}");
                response.EnsureSuccessStatusCode();

                // Turn the response into JSON
                string responseBody = await response.Content.ReadAsStringAsync();

                // Turn the JSON into C#
                Order orderFromDB = JsonConvert.DeserializeObject<Order>(responseBody);

                // Did we get back what we expected to get back? 
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal(dummyOrder.customerId, orderFromDB.customerId);
                Assert.Equal(dummyOrder.paymentTypeId, orderFromDB.paymentTypeId);

                // Clean up after ourselves-- delete the dummy order we just created
                await deleteDummyOrder(orderFromDB);

            }
        }




        [Fact]
        public async Task Update_Order()
        {

            using (var client = new APIClientProvider().Client)
            {
                // Create a dummy order
                Order newOrder = await CreateDummyOrder();

                // Make a new title and assign it to our dummy order
                int newCustomerId = 1;
                newOrder.customerId = newCustomerId;

                // Convert it to JSON
                string modifiedOrderAsJSON = JsonConvert.SerializeObject(newOrder);

                // Try to PUT the newly edited order
                var response = await client.PutAsync(
                    $"{url}/{newOrder.id}",
                    new StringContent(modifiedOrderAsJSON, Encoding.UTF8, "application/json")
                );

                // See what comes back from the PUT. Is it a 204? 
                string responseBody = await response.Content.ReadAsStringAsync();
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

                // Get the edited order back from the database after the PUT
                var getModifiedOrder = await client.GetAsync($"{url}/{newOrder.id}");
                getModifiedOrder.EnsureSuccessStatusCode();

                // Convert it to JSON
                string getOrderBody = await getModifiedOrder.Content.ReadAsStringAsync();

                // Convert it from JSON to C#
                Order newlyEditedOrder = JsonConvert.DeserializeObject<Order>(getOrderBody);

                // Make sure the title was modified correctly
                Assert.Equal(HttpStatusCode.OK, getModifiedOrder.StatusCode);
                Assert.Equal(newCustomerId, newlyEditedOrder.customerId);

                // Clean up after yourself
                await deleteDummyOrder(newlyEditedOrder);
            }
        }

        [Fact]
        public async Task Test_Get_NonExitant_Order_Fails()
        {

            using (var client = new APIClientProvider().Client)
            {
                // Try to get a order with an Id that could never exist
                HttpResponseMessage response = await client.GetAsync($"{url}/00000000");

                // It should bring back a 204 no content error
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            }
        }

        [Fact]
        public async Task Test_Delete_NonExistent_Order_Fails()
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

    

