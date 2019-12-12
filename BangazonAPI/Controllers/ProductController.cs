using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using BangazonAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace BangazonAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        //establishing SQL Connection using Microsoft.extensions.configuration
        private readonly IConfiguration _config;

        public ProductController(IConfiguration config)
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
        // GET: api/Product
        [HttpGet]
        public async Task<IActionResult> Get(int id,double price, string title, string description, int quantity, int productTypeId, int customerId)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    string query = "SELECT Id, Title, Description, Quantity, Price, CustomerId, ProductTypeId FROM Product ";

                 


                    cmd.CommandText = query;
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<Product> products = new List<Product>();

                    while (reader.Read())
                    {
                        Product product = new Product
                        {
                            id = reader.GetInt32(reader.GetOrdinal("id")),
                            title = reader.GetString(reader.GetOrdinal("title")),
                            description = reader.GetString(reader.GetOrdinal("description")),
                            quantity = reader.GetInt32(reader.GetOrdinal("quantity")),
                            price = reader.GetDecimal(reader.GetOrdinal("price")),
                            customerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                            productTypeId = reader.GetInt32(reader.GetOrdinal("ProductTypeId"))
                        };

                        products.Add(product);
                    }
                    reader.Close();

                    return Ok(products);
                }
            }
        }

        // GET: api/Product/5
        [HttpGet("{id}", Name = "GetSingleProduct")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT
                            Id, title, description, quantity, price, productTypeId, customerId
                        FROM Product
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Product product = null;

                    if (reader.Read())
                    {
                        product = new Product
                       
                        {
                            id = reader.GetInt32(reader.GetOrdinal("id")),
                            title = reader.GetString(reader.GetOrdinal("title")),
                            description = reader.GetString(reader.GetOrdinal("description")),
                            quantity = reader.GetInt32(reader.GetOrdinal("quantity")),
                            price = reader.GetDecimal(reader.GetOrdinal("price")),
                            productTypeId = reader.GetInt32(reader.GetOrdinal("ProductTypeId")),
                            customerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                        };
                    }
                    reader.Close();

                    return Ok(product);
                }
            }
        }

        // POST: api/Product
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Product product)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Product (Price, Title, Description, Quantity, productTypeId, customerId)
                                        OUTPUT INSERTED.Id
                                        VALUES (@price, @title, @description, @quantity, @productTypeId, @customerId)";
                    cmd.Parameters.Add(new SqlParameter("@price", product.price));
                    cmd.Parameters.Add(new SqlParameter("@title", product.title));
                    cmd.Parameters.Add(new SqlParameter("@description", product.description));
                    cmd.Parameters.Add(new SqlParameter("@quantity", product.quantity));
                    cmd.Parameters.Add(new SqlParameter("@productTypeId", product.productTypeId));
                    cmd.Parameters.Add(new SqlParameter("@customerId", product.customerId));

                    int newId = (int)cmd.ExecuteScalar();
                    product.id = newId;
                    return CreatedAtRoute("GetSingleProduct", new { id = newId }, product);
                }
            }
        }

        // PUT: api/Product/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Product product)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE Product
                                            SET price = @price,
                                                title = @title,
                                                description = @description,
                                                quantity = @quantity,
                                                productTypeId = @productTypeId,
                                                customerId = @customerId
                                            WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@id", id));
                        cmd.Parameters.Add(new SqlParameter("@price", product.price));
                        cmd.Parameters.Add(new SqlParameter("@title", product.title));
                        cmd.Parameters.Add(new SqlParameter("@description", product.description));
                        cmd.Parameters.Add(new SqlParameter("@quantity", product.quantity));
                        cmd.Parameters.Add(new SqlParameter("@productTypeId", product.productTypeId));
                        cmd.Parameters.Add(new SqlParameter("@customerId", product.customerId));

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
                if (!ProductExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }
        private bool ProductExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, price, title, description, quantity, productTypeId, customerId
                        FROM Product
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }

        // DELETE: api/product/5
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
                        //FIND any instances of connections to other tables
                        cmd.CommandText = @"SELECT ProductTypeId FROM Product WHERE Product.ProductTypeId = @id";
                        cmd.Parameters.Add(new SqlParameter("@id", id));
                        SqlDataReader reader = cmd.ExecuteReader();


                        if (reader.Read())
                        {
                            //throw error
                            throw new Exception("Cannot Delete This Product");
                        }

                        else
                        {
                            //DELETE IT
                            cmd.CommandText = @"DELETE FROM Product WHERE Id=@id";
                        }
                        reader.Close();
                        //cmd.Parameters.Add(new SqlParameter("@id", id));
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
                if (!ProductExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        
        }
    }

    

