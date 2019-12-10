using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using BangazonAPI.Models;

namespace BangazonAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IConfiguration _config;

        public OrderController(IConfiguration config)
        {
            _config = config;
        }

        public SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }

        //Code for getting a list of instructors

        // GET: api/StudentExercisesAPI
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = $@"SELECT Id, CustomerId, PaymentTypeId
                                           FROM [Order]";

                    SqlDataReader reader = cmd.ExecuteReader();
                    List<Order> orders = new List<Order>();

                    while (reader.Read())
                    {
                        Order order = new Order
                        {
                            id = reader.GetInt32(reader.GetOrdinal("Id")),
                            customerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                            paymentTypeId = reader.GetInt32(reader.GetOrdinal("PaymentTypeId")),
                           
                        };

                        orders.Add(order);
                    }
                    reader.Close();

                    return Ok(orders);
                }
            }
        }

        // GET: api/Order/5
        [HttpGet("{id}", Name = "GetOrder")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = $@"SELECT Id, CustomerId, PaymentTypeId
                                           FROM [Order]
                                           WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Order order = null;

                    if (reader.Read())
                    {
                        order = new Order
                        {
                            id = reader.GetInt32(reader.GetOrdinal("Id")),
                            customerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                            paymentTypeId = reader.GetInt32(reader.GetOrdinal("PaymentTypeId"))

                        };
                    }
                    reader.Close();

                    return Ok(order);
                }
            }
        }

        // POST: api/Order
        public async Task<IActionResult> Post([FromBody] Order order)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO [Order] (CustomerId, PaymentTypeId)
                                        OUTPUT INSERTED.Id
                                        VALUES (@customerId, @paymentTypeId)";
                    cmd.Parameters.Add(new SqlParameter("@customerId", order.customerId));
                    cmd.Parameters.Add(new SqlParameter("@paymentTypeId", order.paymentTypeId));
                    

                    int newId = (int)cmd.ExecuteScalar();
                    order.id = newId;
                    return CreatedAtRoute("GetOrder", new { id = newId }, order);
                }
            }
        }

        // PUT: api/Order/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Order order)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE [Order]
                                            SET paymentTypeId = @paymentTypeId,
                                                customerId = @customerId
                                            WHERE id = @id";
                        cmd.Parameters.Add(new SqlParameter("@id", id));
                        cmd.Parameters.Add(new SqlParameter("@customerId", order.customerId));
                        cmd.Parameters.Add(new SqlParameter("@paymentTypeId", order.paymentTypeId));

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }
                        throw new Exception("No rows affected");
                    }
                }
            }
            catch (Exception)
            {
                if (!OrderExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }

        private bool OrderExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = 
                       $@"SELECT Id, CustomerId, PaymentTypeId
                                           FROM[Order]                       
                                           WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
    }
}


//Verbs to be supported

//GET
//POST
//PUT
//DELETE
//User should be able to GET a list, and GET a single item.
//When an order is deleted, every line item (i.e.entry in OrderProduct) should be removed

//Should be able to filter out completed orders with the ? completed = false query string parameter.

//  If the parameter value is true, then only completed order should be returned.

//  If the query string parameter of? _include = products is in the URL, then the list of products in the order should be returned.

//  If the query string parameter of? _include = customers is in the URL, then the customer representation should be included in the response.


//  Testing Criteria
//  Write a testing class and test methods that validate the GET, POST, PUT, and DELETE operations work as expected.