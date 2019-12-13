using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;
using BangazonAPI.Models;
using Microsoft.AspNetCore.Http;

namespace BangazonAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly IConfiguration _config;

        public CustomerController(IConfiguration config)
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

        [HttpGet]
        //Get all customers back from the database based on query paramaters
        public async Task<IActionResult> Get(string include, string q)
        {
            //open connction to SQL database
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    //query to bring back all customers
                    string query = @"SELECT Customer.Id AS 'Customer Id', Customer.FirstName, Customer.LastName
FROM Customer ";
                    // query to include products for sale by cutomer if include=product
                    if (include == "products")
                    {
                        query = @"SELECT Customer.Id AS 'Customer Id', Customer.FirstName, Customer.LastName,
Product.Id AS 'Product Id', Product.Title, Product.Description, Product.Quantity, Product.Price  FROM Customer JOIN Product ON Product.CustomerId = Customer.Id";
                    }
                    //query to bring back list of payment types with cutomer if include=payments
                    if (include == "payments")
                    {
                        query = @"SELECT Customer.Id AS 'Customer Id', Customer.FirstName, Customer.LastName,
PaymentType.Id AS 'PaymentType Id', PaymentType.Name, PaymentType.AcctNumber FROM Customer JOIN PaymentType ON PaymentType.CustomerId = Customer.Id";
                    }
                    //query to find any customer property based on q parameters
                    if (q != null)
                    {
                        query = $"SELECT Customer.Id AS 'Customer Id', Customer.FirstName, Customer.LastName FROM Customer WHERE Customer.FirstName LIKE '%{q}%' OR Customer.LastName LIKE '%{q}%'";
                    }

                    cmd.CommandText = query;
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<Customer> customers = new List<Customer>();

                    while (reader.Read())
                    {
                        Customer currentCustomer = new Customer
                        {
                            id = reader.GetInt32(reader.GetOrdinal("Customer Id")),
                            firstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            lastName = reader.GetString(reader.GetOrdinal("LastName"))
                        };

                        if (include== null)
                        {
                            //add customers to list of customers
                            customers.Add(currentCustomer);

                        }
                        //if query include = product then run second query to retrieve products assigned information back
                        if (include == "products")
                        {
                            Product currentProduct = new Product
                            {
                                id = reader.GetInt32(reader.GetOrdinal("Product Id")),
                                title = reader.GetString(reader.GetOrdinal("Title")),
                                price = reader.GetDecimal(reader.GetOrdinal("Price")),
                                quantity = reader.GetInt32(reader.GetOrdinal("Quantity")),
                                description = reader.GetString(reader.GetOrdinal("Description"))
                            };

                            //if customer is on the list do not add again
                            if (customers.Any(customer => customer.id == currentCustomer.id))

                            {
                                Customer thisCustomer = customers.Where(c => c.id == currentCustomer.id).FirstOrDefault();
                                thisCustomer.productsForSale.Add(currentProduct);
                            }
                            else

                           {
                                currentCustomer.productsForSale.Add(currentProduct);
                                //add customers to list of customers
                                customers.Add(currentCustomer);

                            }
                        }
                        // if query include= paymentType bring back all customers with list of payment types
                        if (include == "payments")
                        {
                            PaymentType currentPayment = new PaymentType
                            {
                                id = reader.GetInt32(reader.GetOrdinal("PaymentType Id")),
                                name = reader.GetString(reader.GetOrdinal("Name")),
                                accountNumber = reader.GetInt32(reader.GetOrdinal("AcctNumber"))
                            };

                            //if customer is on the list do not add again
                            if (customers.Any(customer => customer.id == currentCustomer.id))

                            {
                                Customer thisCustomer = customers.Where(c => c.id == currentCustomer.id).FirstOrDefault();
                                thisCustomer.paymentTypes.Add(currentPayment);
                            }
                            else

                            {
                                currentCustomer.paymentTypes.Add(currentPayment);
                                customers.Add(currentCustomer);
                            }

                        }
                        else
                        {
                            //add customers to list of customers
                            customers.Add(currentCustomer);
                        }
                    }
                    reader.Close();
                    //return a list of customers
                    return Ok(customers);
                }
            }
        }

        // get one customer from the database by id and route

        [HttpGet("{id}", Name = "GetCustomer")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    //query to find the single customer by id
                    cmd.CommandText = @"
                        SELECT
                            Id, FirstName, LastName
                        FROM Customer
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Customer customer = null;

                    if (reader.Read())
                    {
                        customer = new Customer
                        {
                            id = reader.GetInt32(reader.GetOrdinal("Id")),
                            firstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            lastName = reader.GetString(reader.GetOrdinal("LastName"))
                        };
                    }
                    reader.Close();
                    //return the single customer by id
                    return Ok(customer);
                }
            }
        }
        // Post a new customer to the database
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Customer customer)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Customer (FirstName, LastName)
                                        OUTPUT INSERTED.Id
                                        VALUES (@FirstName, @LastName)";
                    cmd.Parameters.Add(new SqlParameter("@Firstname", customer.firstName));
                    cmd.Parameters.Add(new SqlParameter("@LastName", customer.lastName));

                    //give the new customer an id
                    int newId = (int)cmd.ExecuteScalar();
                    customer.id = newId;
                    return CreatedAtRoute("GetCustomer", new { id = newId }, customer);
                }
            }
        }

        //edit the customer by customer id

        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Customer customer)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE Customer
                                            SET FirstName = @FirstName,
                                                LastName = @LastName
                                               WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@FirstName", customer.firstName));
                        cmd.Parameters.Add(new SqlParameter("@LastName", customer.lastName));
                        cmd.Parameters.Add(new SqlParameter("@id", id));

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
                if (!CustomerExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

      //method to check for existing customer in the database

        private bool CustomerExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, FirstName, LastName
                        FROM Customer
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
    }
}
