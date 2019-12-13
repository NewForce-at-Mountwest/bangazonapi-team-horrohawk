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
    public class ProductControllerTests
    {
        // Testing GET POST PUT & DELETE of Product Controller
        private Product dummyProduct { get; } = new Product
        {
            price = 3.99m,
            title ="spongebob backpack",
            description ="everybody's favorite yellow friend to carry books!",
            quantity = 1,
            productTypeId = 1,
            customerId = 1

        };
        // We'll store our base url for this route as a private field to avoid typos
        private string url { get; } = "/api/product";
        // Reusable method to create a new product in the database and return it
        public async Task<Product> CreateDummyProduct()
        {
            using (var client = new APIClientProvider().Client)
            {
                // Serialize the C# object into a JSON string
                string SpongebobBackpackAsJSON = JsonConvert.SerializeObject(dummyProduct);
                // Use the client to send the request and store the response
                HttpResponseMessage response = await client.PostAsync(
                    url,
                    new StringContent(SpongebobBackpackAsJSON, Encoding.UTF8, "application/json")
                );
                // Store the JSON body of the response
                string responseBody = await response.Content.ReadAsStringAsync();
                // Deserialize the JSON into an instance of Product
                Product newlyCreatedProduct = JsonConvert.DeserializeObject<Product>(responseBody);
                return newlyCreatedProduct;
            }
        }
        // Reusable method to delete a product from the database
        public async Task DeleteDummyProduct(Product productToDelete)
        {
            using (HttpClient client = new APIClientProvider().Client)
            {
                HttpResponseMessage deleteResponse = await client.DeleteAsync($"{url}/{productToDelete.id}");
            }
        }
        //Tests START HERE
        [Fact]
        public async Task Create_Product()
        {
            using (var client = new APIClientProvider().Client)
            {
                // Create a new product in the database
                Product newBackpackProduct = await CreateDummyProduct();
                // Try to get it again
                HttpResponseMessage response = await client.GetAsync($"{url}/{newBackpackProduct.id}");
                response.EnsureSuccessStatusCode();
                // Turn the response into JSON
                string responseBody = await response.Content.ReadAsStringAsync();
                // Turn the JSON into C#
                Product newProduct = JsonConvert.DeserializeObject<Product>(responseBody);
                // Make sure it's really there
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal(dummyProduct.price, newProduct.price);
                Assert.Equal(dummyProduct.title, newProduct.title);
                Assert.Equal(dummyProduct.description, newProduct.description);
                Assert.Equal(dummyProduct.quantity, newProduct.quantity);
                Assert.Equal(dummyProduct.productTypeId, newProduct.productTypeId);
                Assert.Equal(dummyProduct.customerId, newProduct.customerId);






                // Clean up after ourselves
                await DeleteDummyProduct(newProduct);
            }
        }
        // test to get all products
        [Fact]
        public async Task Get_All_Products()
        {
            using (var client = new APIClientProvider().Client)
            {
                // Try to get all of the students from /api/product
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                // Convert to JSON
                string responseBody = await response.Content.ReadAsStringAsync();
                // Convert from JSON to C#
                List<Product> products = JsonConvert.DeserializeObject<List<Product>>(responseBody);
                // Make sure we got back a 200 OK Status and that there are more than 0 products in our database
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.True(products.Count > 0);
            }
        }
        //Test get single customer
        [Fact]
        public async Task Get_Single_Product()
        {
            using (HttpClient client = new APIClientProvider().Client)
            {
                // Create a dummy product
                Product newProduct = await CreateDummyProduct();
                // Try to get it
                HttpResponseMessage response = await client.GetAsync($"{url}/{newProduct.id}");
                response.EnsureSuccessStatusCode();
                // Turn the response into JSON
                string responseBody = await response.Content.ReadAsStringAsync();
                // Turn the JSON into C#
                Product ProductFromDB = JsonConvert.DeserializeObject<Product>(responseBody);
                // Did we get back what we expected to get back?
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal(dummyProduct.price, newProduct.price);
                Assert.Equal(dummyProduct.title, newProduct.title);
                Assert.Equal(dummyProduct.description, newProduct.description);
                Assert.Equal(dummyProduct.quantity, newProduct.quantity);
                Assert.Equal(dummyProduct.productTypeId, newProduct.productTypeId);
                Assert.Equal(dummyProduct.customerId, newProduct.customerId);
                // Clean up after ourselves-- delete the dummy product we just created
                await DeleteDummyProduct(ProductFromDB);
            }
        }
        [Fact]
        public async Task Update_Product()
        {
            using (var client = new APIClientProvider().Client)
            {
                // Create a dummy product
                Product newProduct = await CreateDummyProduct();
                // Make a new title and assign it to our dummy product
                string newTitle = "Ninja Backpack";
                newProduct.title = newTitle;
                // Convert it to JSON
                string productUpdateAsJSON = JsonConvert.SerializeObject(newProduct);
                // Try to PUT the newly edited product
                var response = await client.PutAsync(
                    $"{url}/{newProduct.id}",
                    new StringContent(productUpdateAsJSON, Encoding.UTF8, "application/json")
                );
                // See what comes back from the PUT. Is it a 204?
                string responseBody = await response.Content.ReadAsStringAsync();
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
                // Get the edited product back from the database after the PUT
                var getUpdatedProduct = await client.GetAsync($"{url}/{newProduct.id}");
                getUpdatedProduct.EnsureSuccessStatusCode();
                // Convert it to JSON
                string getProductBody = await getUpdatedProduct.Content.ReadAsStringAsync();
                // Convert it from JSON to C#
                Product newlyEditedProduct = JsonConvert.DeserializeObject<Product>(getProductBody);
                // Make sure the title was modified correctly
                Assert.Equal(HttpStatusCode.OK, getUpdatedProduct.StatusCode);
                Assert.Equal(newProduct.title, newlyEditedProduct.title);
                // Clean up after yourself
                await DeleteDummyProduct(newlyEditedProduct);
            }
        }
        [Fact]
        public async Task Test_Get_NonExitent_Product_Fails()
        {
            using (var client = new APIClientProvider().Client)
            {
                // Try to get a product with an Id that could never exist
                HttpResponseMessage response = await client.GetAsync($"{url}/00000000");
                // It should bring back a 204 no content error
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            }
        }
        [Fact]
        public async Task Test_Delete_NonExistent_Product_Fails()
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
