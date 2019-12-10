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
    public class PaymentTypeControllerTest
    {
        // This is going to be our test instance that we create and delete
        private PaymentType dummyPaymentType { get; } = new PaymentType
        {
            name = "Mock CC",
            accountNumber = 789465893,
            customerId = 1
        };
        // We'll store our base url for this route as a private field to avoid typos
        private string url { get; } = "/api/PaymentType";


        // Reusable method to create a new PaymentType in the db and return it
        public async Task<PaymentType> CreateDummyPaymentType()
        {

            using (var client = new APIClientProvider().Client)
            {

                // Serialize the C# object into a JSON string
                string pmtTypeAsJSON = JsonConvert.SerializeObject(dummyPaymentType);


                // Use the client to send the request and store the response
                HttpResponseMessage response = await client.PostAsync(
                    url,
                    new StringContent(pmtTypeAsJSON, Encoding.UTF8, "application/json")
                );

                // Store the JSON body of the response
                string responseBody = await response.Content.ReadAsStringAsync();

                // Deserialize the JSON into an instance of Cohort
                PaymentType newlyCreatedPaymentType = JsonConvert.DeserializeObject<PaymentType>(responseBody);

                return newlyCreatedPaymentType;
            }
        }

        // Reusable method to delete a PaymentType from the database
        public async Task deleteDummyPaymentType(PaymentType PaymentTypeToDelete)
        {
            using (HttpClient client = new APIClientProvider().Client)
            {
                HttpResponseMessage deleteResponse = await client.DeleteAsync($"{url}/{PaymentTypeToDelete.id}");

            }

        }


        /* TESTS START HERE */


        [Fact]
        public async Task Create_PaymentType()
        {
            using (var client = new APIClientProvider().Client)
            {
                // Create new in the db
                PaymentType newPmtType = await CreateDummyPaymentType();

                // Try to get it again
                HttpResponseMessage response = await client.GetAsync($"{url}/{newPmtType.id}");
                response.EnsureSuccessStatusCode();

                // Turn the response into JSON
                string responseBody = await response.Content.ReadAsStringAsync();

                // Turn the JSON into C#
                PaymentType newPaymentType = JsonConvert.DeserializeObject<PaymentType>(responseBody);

                // Make sure it's really there
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal(dummyPaymentType.name, newPmtType.name);

                // Clean up after ourselves
                await deleteDummyPaymentType(newPmtType);

            }

        }


        [Fact]

        public async Task Delete_PaymentType()
        {
            // Note: with many of these methods, I'm creating dummy data and then testing to see if I can delete it. I'd rather do that for now than delete something else I (or a user) created in the database, but it's not essential-- we could test deleting anything 

            // Create a new coffee in the db
            PaymentType newPmtType = await CreateDummyPaymentType();

            // Delete it
            await deleteDummyPaymentType(newPmtType);

            using (var client = new APIClientProvider().Client)
            {
                // Try to get it again
                HttpResponseMessage response = await client.GetAsync($"{url}{newPmtType.id}");

                // Make sure it's really gone
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            }
        }

        [Fact]
        public async Task Get_All_PaymentTypes()
        {

            using (var client = new APIClientProvider().Client)
            {

                // Try to get all of the coffees from /api/PaymentType
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                // Convert to JSON
                string responseBody = await response.Content.ReadAsStringAsync();

                // Convert from JSON to C#
                List<PaymentType> PaymentType = JsonConvert.DeserializeObject<List<PaymentType>>(responseBody);

                // Make sure we got back a 200 OK Status and that there are more than 0 coffees in our database
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.True(PaymentType.Count > 0);

            }
        }

        [Fact]
        public async Task Get_Single_PaymentType()
        {
            using (HttpClient client = new APIClientProvider().Client)
            {
                // Create a dummy coffee
                PaymentType newPmtType = await CreateDummyPaymentType();

                // Try to get it
                HttpResponseMessage response = await client.GetAsync($"{url}/{newPmtType.id}");
                response.EnsureSuccessStatusCode();

                // Turn the response into JSON
                string responseBody = await response.Content.ReadAsStringAsync();

                // Turn the JSON into C#
                PaymentType PaymentTypeFromDB = JsonConvert.DeserializeObject<PaymentType>(responseBody);

                // Did we get back what we expected to get back? 
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal(dummyPaymentType.name, PaymentTypeFromDB.name);

                // Clean up after ourselves-- delete the dummy coffee we just created
                await deleteDummyPaymentType(PaymentTypeFromDB);

            }
        }




        [Fact]
        public async Task Update_PaymentType()
        {

            using (var client = new APIClientProvider().Client)
            {
                // Create a dummy 
                PaymentType newPmtType = await CreateDummyPaymentType();

                // Make a new title and assign it to our dummy 
                string newName = "newPmt";
                newPmtType.name = newName;

                // Convert it to JSON
                string modifiedPmtTypeAsJSON = JsonConvert.SerializeObject(newPmtType);

                // Try to PUT the newly edited 
                var response = await client.PutAsync(
                    $"{url}/{newPmtType.id}",
                    new StringContent(modifiedPmtTypeAsJSON, Encoding.UTF8, "application/json")
                );

                // See what comes back from the PUT. Is it a 204? 
                string responseBody = await response.Content.ReadAsStringAsync();
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

                // Get the edited item back from the database after the PUT
                var getModifiedPmtType = await client.GetAsync($"{url}/{newPmtType.id}");
                getModifiedPmtType.EnsureSuccessStatusCode();

                // Convert it to JSON
                string getPaymentTypeBody = await getModifiedPmtType.Content.ReadAsStringAsync();

                // Convert it from JSON to C#
                PaymentType newlyEditedPmtType = JsonConvert.DeserializeObject<PaymentType>(getPaymentTypeBody);

                // Make sure the title was modified correctly
                Assert.Equal(HttpStatusCode.OK, getModifiedPmtType.StatusCode);
                Assert.Equal(newName, newlyEditedPmtType.name);

                // Clean up after yourself
                await deleteDummyPaymentType(newlyEditedPmtType);
            }
        }

        [Fact]
        public async Task Test_Get_NonExitant_PaymentType_Fails()
        {

            using (var client = new APIClientProvider().Client)
            {
                // Try to get a coffee with an Id that could never exist
                HttpResponseMessage response = await client.GetAsync($"{url}/00000000");

                // It should bring back a 204 no content error
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            }
        }

        [Fact]
        public async Task Test_Delete_NonExistent_PaymentType_Fails()
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
