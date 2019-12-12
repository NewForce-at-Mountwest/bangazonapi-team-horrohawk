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
    public class PaymentTypeController : ControllerBase
    {
        private readonly IConfiguration _config;

        public PaymentTypeController(IConfiguration config)
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
        public async Task<IActionResult> Get()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT Id, Name, AcctNumber, CustomerId FROM PaymentType";
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<PaymentType> paymentTypes = new List<PaymentType>();

                    while (reader.Read())
                    {
                        PaymentType currentpaymentType = new PaymentType
                        {
                            id = reader.GetInt32(reader.GetOrdinal("Id")),
                            accountNumber = reader.GetInt32(reader.GetOrdinal("AcctNumber")),
                            name = reader.GetString(reader.GetOrdinal("Name")),
                            customerId = reader.GetInt32(reader.GetOrdinal("CustomerId"))

                        };

                        paymentTypes.Add(currentpaymentType);


                    }
                    reader.Close();

                    return Ok(paymentTypes);
                }
            }
        }

        [HttpGet("{id}", Name = "GetPaymentType")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT
                            Id, Name, AcctNumber, CustomerId
                        FROM PaymentType
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    PaymentType currentPayment = null;

                    if (reader.Read())
                    {
                        currentPayment = new PaymentType
                        {
                            id = reader.GetInt32(reader.GetOrdinal("Id")),
                            accountNumber = reader.GetInt32(reader.GetOrdinal("AcctNumber")),
                            name = reader.GetString(reader.GetOrdinal("Name")),
                            customerId = reader.GetInt32(reader.GetOrdinal("CustomerId"))
                        };
                    }
                    reader.Close();

                    return Ok(currentPayment);
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] PaymentType currentPayment)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO PaymentType (Name, AcctNumber, CustomerId)
                                        OUTPUT INSERTED.Id
                                        VALUES (@Name, @AcctNumber, @CustomerId)";
                    cmd.Parameters.Add(new SqlParameter("@Name", currentPayment.name));
                    cmd.Parameters.Add(new SqlParameter("@AcctNumber", currentPayment.accountNumber));
                    cmd.Parameters.Add(new SqlParameter("@CustomerId", currentPayment.customerId));

                    int newId = (int)cmd.ExecuteScalar();
                    currentPayment.id = newId;
                    return CreatedAtRoute("GetPaymentType", new { id = newId }, currentPayment);
                }
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] PaymentType newPaymentType)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE PaymentType
                                            SET Name = @Name,
                                             AcctNumber = @AcctNumber,
                                             CustomerId = @CustomerId
                                            WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@Name", newPaymentType.name));
                        cmd.Parameters.Add(new SqlParameter("@AcctNumber", newPaymentType.accountNumber));
                        cmd.Parameters.Add(new SqlParameter("@CustomerId", newPaymentType.customerId));
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
                if (!PaymentTypeExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

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
                        cmd.CommandText = @"SELECT[Order].PaymentTypeId AS 'Order PmtType Id' FROM[Order] WHERE[Order].PaymentTypeId = @id";
                        cmd.Parameters.Add(new SqlParameter("@id", id));
                        SqlDataReader reader = cmd.ExecuteReader();
                        

                        if (reader.Read())
                        {
                            //throw error
                            throw new Exception("Cannot Delete This One");
                        }
                        
                        else
                        {
                            //DELETE IT
                            cmd.CommandText = @"DELETE FROM PaymentType WHERE Id=@id";
                        }
                        reader.Close();
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
                if (!PaymentTypeExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        private bool PaymentTypeExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, Name, AcctNumber
                        FROM PaymentType
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
    }
}