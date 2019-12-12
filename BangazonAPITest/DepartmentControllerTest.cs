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
    public class DepartmentControllerTest
    {
        // This is going to be our test instance that we create and delete
        private Department dummyDepartment { get; } = new Department
        {
            name = "Rock Media",
            budget = 789465893
        };
        // We'll store our base url for this route as a private field to avoid typos
        private string url { get; } = "/api/Department";


        // Reusable method to create a new Department in the db and return it
        public async Task<Department> CreateDummyDepartment()
        {

            using (var client = new APIClientProvider().Client)
            {

                // Serialize the C# object into a JSON string
                string deptAsJSON = JsonConvert.SerializeObject(dummyDepartment);


                // Use the client to send the request and store the response
                HttpResponseMessage response = await client.PostAsync(
                    url,
                    new StringContent(deptAsJSON, Encoding.UTF8, "application/json")
                );

                // Store the JSON body of the response
                string responseBody = await response.Content.ReadAsStringAsync();

                // Deserialize the JSON into an instance of Cohort
                Department newlyCreatedDepartment = JsonConvert.DeserializeObject<Department>(responseBody);

                return newlyCreatedDepartment;
            }
        }

        // Reusable method to delete a Department from the database
        public async Task deleteDummyDepartment(Department DepartmentToDelete)
        {
            using (HttpClient client = new APIClientProvider().Client)
            {
                HttpResponseMessage deleteResponse = await client.DeleteAsync($"{url}/{DepartmentToDelete.id}");

            }

        }


        /* TESTS START HERE */


        [Fact]
        public async Task Create_Department()
        {
            using (var client = new APIClientProvider().Client)
            {
                // Create new in the db
                Department deptType = await CreateDummyDepartment();

                // Try to get it again
                HttpResponseMessage response = await client.GetAsync($"{url}/{deptType.id}");
                response.EnsureSuccessStatusCode();

                // Turn the response into JSON
                string responseBody = await response.Content.ReadAsStringAsync();

                // Turn the JSON into C#
                Department newDepartment = JsonConvert.DeserializeObject<Department>(responseBody);

                // Make sure it's really there
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal(dummyDepartment.name, deptType.name);

                // Clean up after ourselves
                await deleteDummyDepartment(deptType);

            }

        }


        [Fact]

        public async Task Delete_Department()
        {
            // Note: with many of these methods, I'm creating dummy data and then testing to see if I can delete it. I'd rather do that for now than delete something else I (or a user) created in the database, but it's not essential-- we could test deleting anything 

            // Create new  in the db
            Department deptType = await CreateDummyDepartment();

            // Delete it
            await deleteDummyDepartment(deptType);

            using (var client = new APIClientProvider().Client)
            {
                // Try to get it again
                HttpResponseMessage response = await client.GetAsync($"{url}{deptType.id}");

                // Make sure it's really gone
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            }
        }

        [Fact]
        public async Task Get_All_Departments()
        {

            using (var client = new APIClientProvider().Client)
            {

                // Try to get all from /api/Department
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                // Convert to JSON
                string responseBody = await response.Content.ReadAsStringAsync();

                // Convert from JSON to C#
                List<Department> Department = JsonConvert.DeserializeObject<List<Department>>(responseBody);

                // Make sure we got back a 200 OK Status and that there are more than 0 things in our database
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.True(Department.Count > 0);

            }
        }

        [Fact]
        public async Task Get_Single_Department()
        {
            using (HttpClient client = new APIClientProvider().Client)
            {
                // Create a dummy 
                Department deptType = await CreateDummyDepartment();

                // Try to get it
                HttpResponseMessage response = await client.GetAsync($"{url}/{deptType.id}");
                response.EnsureSuccessStatusCode();

                // Turn the response into JSON
                string responseBody = await response.Content.ReadAsStringAsync();

                // Turn the JSON into C#
                Department DepartmentFromDB = JsonConvert.DeserializeObject<Department>(responseBody);

                // Did we get back what we expected to get back? 
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal(dummyDepartment.name, DepartmentFromDB.name);

                // Clean up after ourselves-- delete the dummy  we just created
                await deleteDummyDepartment(DepartmentFromDB);

            }
        }




        [Fact]
        public async Task Update_Department()
        {

            using (var client = new APIClientProvider().Client)
            {
                // Create a dummy 
                Department deptType = await CreateDummyDepartment();

                // Make a new title and assign it to our dummy 
                string newName = "dept";
                deptType.name = newName;

                // Convert it to JSON
                string modifiedDeptAsJSON = JsonConvert.SerializeObject(deptType);

                // Try to PUT the newly edited 
                var response = await client.PutAsync(
                    $"{url}/{deptType.id}",
                    new StringContent(modifiedDeptAsJSON, Encoding.UTF8, "application/json")
                );

                // See what comes back from the PUT. Is it a 204? 
                string responseBody = await response.Content.ReadAsStringAsync();
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

                // Get the edited item back from the database after the PUT
                var getModifiedDept = await client.GetAsync($"{url}/{deptType.id}");
                getModifiedDept.EnsureSuccessStatusCode();

                // Convert it to JSON
                string getDepartmentBody = await getModifiedDept.Content.ReadAsStringAsync();

                // Convert it from JSON to C#
                Department newlyEditedDept = JsonConvert.DeserializeObject<Department>(getDepartmentBody);

                // Make sure the title was modified correctly
                Assert.Equal(HttpStatusCode.OK, getModifiedDept.StatusCode);
                Assert.Equal(newName, newlyEditedDept.name);

                // Clean up after yourself
                await deleteDummyDepartment(newlyEditedDept);
            }
        }

        [Fact]
        public async Task Test_Get_NonExitant_Department_Fails()
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
        public async Task Test_Delete_NonExistent_Department_Fails()
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
