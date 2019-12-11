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
    public class ProductTypeControllerTests
    {
        // This is going to be our test product type instance that we create and delete to make sure everything works
        private ProductType dummyProductType { get; } = new ProductType
        {
           name = "Household Items"

        };

        // We'll store our base url for this route as a private field to avoid typos
        private string url { get; } = "/api/productType";

        // Reusable method to create a new product type in the database and return it
        public async Task<ProductType> CreateDummyProductType()
        {

            using (var client = new APIClientProvider().Client)
            {

                // Serialize the C# object into a JSON string
                string JoeProductTypeAsJSON = JsonConvert.SerializeObject(dummyProductType);


                // Use the client to send the request and store the response
                HttpResponseMessage response = await client.PostAsync(
                    url,
                    new StringContent(JoeProductTypeAsJSON, Encoding.UTF8, "application/json")
                );

                // Store the JSON body of the response
                string responseBody = await response.Content.ReadAsStringAsync();

                // Deserialize the JSON into an instance of Product Type
                ProductType newlyCreatedProductType = JsonConvert.DeserializeObject<ProductType>(responseBody);

                return newlyCreatedProductType;
            }
        }
        // Reusable method to deelte a product type from the database
        public async Task deleteDummyProductType(ProductType productTypeToDelete)
        {
            using (HttpClient client = new APIClientProvider().Client)
            {
                HttpResponseMessage deleteResponse = await client.DeleteAsync($"{url}/{productTypeToDelete.id}");

            }

        }

        //Tests START HERE
        [Fact]
        public async Task Create_ProductType()
        {
            using (var client = new APIClientProvider().Client)
            {
                // Create a new product type in the db
                ProductType newJoeProductType = await CreateDummyProductType();

                // Try to get it again
                HttpResponseMessage response = await client.GetAsync($"{url}/{newJoeProductType.id}");
                response.EnsureSuccessStatusCode();

                // Turn the response into JSON
                string responseBody = await response.Content.ReadAsStringAsync();

                // Turn the JSON into C#
                ProductType newProductType = JsonConvert.DeserializeObject<ProductType>(responseBody);

                // Make sure it's really there
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal(dummyProductType.name, newProductType.name);

                // Clean up after ourselves
                await deleteDummyProductType(newProductType);

            }

        }

        // test to get all product types
        [Fact]
        public async Task Get_All_ProductTypes()
        {

            using (var client = new APIClientProvider().Client)
            {

                // Try to get all of the product types from /api/customer
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                // Convert to JSON
                string responseBody = await response.Content.ReadAsStringAsync();

                // Convert from JSON to C#
                List<Customer> customers = JsonConvert.DeserializeObject<List<Customer>>(responseBody);

                // Make sure we got back a 200 OK Status and that there are more than 0 coffees in our database
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.True(customers.Count > 0);

            }
        }
        //Test get single product type
        [Fact]
        public async Task Get_Single_ProductType()
        {
            using (HttpClient client = new APIClientProvider().Client)
            {
                // Create a dummy product type
                ProductType newJoeProductType = await CreateDummyProductType();

                // Try to get it
                HttpResponseMessage response = await client.GetAsync($"{url}/{newJoeProductType.id}");
                response.EnsureSuccessStatusCode();

                // Turn the response into JSON
                string responseBody = await response.Content.ReadAsStringAsync();

                // Turn the JSON into C#
                ProductType JoeProductTypeFromDB = JsonConvert.DeserializeObject<ProductType>(responseBody);

                // Did we get back what we expected to get back? 
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal(dummyProductType.name, JoeProductTypeFromDB.name);

                // Clean up after ourselves-- delete the dummy customer we just created
                await deleteDummyProductType(JoeProductTypeFromDB);

            }
        }
        [Fact]
        public async Task Update_ProductType()
        {

            using (var client = new APIClientProvider().Client)
            {
                // Create a dummy product type
                ProductType newJoeProductType = await CreateDummyProductType();

                // Make a new title and assign it to our dummy product type
                string newName = "JOEY MCFRENCH ROAST";
                newJoeProductType.name = newName;

                // Convert it to JSON
                string modifiedJoeProductTypeAsJSON = JsonConvert.SerializeObject(newJoeProductType);

                // Try to PUT the newly edited product type
                var response = await client.PutAsync(
                    $"{url}/{newJoeProductType.id}",
                    new StringContent(modifiedJoeProductTypeAsJSON, Encoding.UTF8, "application/json")
                );

                // See what comes back from the PUT. Is it a 204? 
                string responseBody = await response.Content.ReadAsStringAsync();
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

                // Get the edited product type back from the database after the PUT
                var getModifiedProductType = await client.GetAsync($"{url}/{newJoeProductType.id}");
                getModifiedProductType.EnsureSuccessStatusCode();

                // Convert it to JSON
                string getProductTypeBody = await getModifiedProductType.Content.ReadAsStringAsync();

                // Convert it from JSON to C#
                ProductType newlyEditedProductType = JsonConvert.DeserializeObject<ProductType>(getProductTypeBody);

                // Make sure the name was modified correctly
                Assert.Equal(HttpStatusCode.OK, getModifiedProductType.StatusCode);

                // Clean up after yourself
                await deleteDummyProductType(newlyEditedProductType);
            }
        }
        [Fact]
        public async Task Test_Get_NonExitent_ProductType_Fails()
        {

            using (var client = new APIClientProvider().Client)
            {
                // Try to get a product type with an Id that could never exist
                HttpResponseMessage response = await client.GetAsync($"{url}/00000000");

                // It should bring back a 204 no content error
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            }
        }
        [Fact]
        public async Task Test_Delete_NonExistent_ProductType_Fails()
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
