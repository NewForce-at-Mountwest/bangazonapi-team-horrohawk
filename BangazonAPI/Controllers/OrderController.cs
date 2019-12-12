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

        //Code for getting a list of orders

        // GET: api/BangazonAPI
        [HttpGet]
        public async Task<IActionResult> Get(string completed, string q)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                                                          
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    string command = "";

                    //Should be able to filter out completed orders with the ? completed = false query string parameter.

                    if (completed == "true")
                    {
                        string ordersCompleted = @"SELECT Id, CustomerId, PaymentTypeId                                     
                       FROM [Order] WHERE PaymentTypeId IS NOT NULL";


                        command = ordersCompleted;

                    }
                    else if (completed == "false")
                    {
                        string ordersNotCompleted = $@"SELECT Id, CustomerId, PaymentTypeId                                     
                        FROM [Order] WHERE PaymentTypeId IS NULL";

                        command = ordersNotCompleted;
                    }
                    else
                    {
                        command = $@"SELECT Id, CustomerId, PaymentTypeId                                     
                       FROM [Order]";

                      
                    }


                    cmd.CommandText = command;
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<Order> orders = new List<Order>();

                    while (reader.Read())
                    {

                        Order currentOrder = new Order
                        {
                            id = reader.GetInt32(reader.GetOrdinal("Id")),
                            customerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                            paymentTypeId = reader.GetInt32(reader.GetOrdinal("PaymentTypeId")),
                        };
                        orders.Add(currentOrder);
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
        public async Task<IActionResult> Delete([FromRoute] int id)

        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                        {                        
                        cmd.CommandText = @"SELECT OrderId
                                          FROM OrderProduct 
                                          WHERE OrderId = @id";

                       
                        cmd.Parameters.Add(new SqlParameter("@id", id));
                       
                        SqlDataReader reader = cmd.ExecuteReader();
                        
                        if (reader.Read())
                        {
                            cmd.CommandText = @"DELETE FROM OrderProduct WHERE OrderId = @id1";
                            cmd.Parameters.Add(new SqlParameter("@id1", id));

                            cmd.CommandText += @" DELETE FROM [Order] WHERE Id = @id2";
                            cmd.Parameters.Add(new SqlParameter("@id2", id));
                        }
                        else
                        {
                            cmd.CommandText = @"DELETE FROM [Order] WHERE Id = @id3";
                            cmd.Parameters.Add(new SqlParameter("@id3", id));
                        }
                        reader.Close();

                        //throw new Exception("No rows affected");

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
       

        private bool OrderExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText =  @"SELECT Id, CustomerId, PaymentTypeId
                                         FROM [Order]
                                         WHERE Id = @id";
                                                             
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
    }
}


