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
    public class DepartmentController : ControllerBase
    {
        private readonly IConfiguration _config;

        public DepartmentController(IConfiguration config)
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
        public async Task<IActionResult> Get(string _filter, int _gt, string _include)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    string query = "SELECT Department.Id AS 'Department Id', Department.Name AS 'Department Name', Department.Budget AS 'Department Budget' FROM Department";

                    if (_include == "employees")
                    {
                        query = @"SELECT Department.Id AS 'Department Id', Department.Name AS 'Department Name', Department.Budget AS 'Department Budget', Employee.Id AS 'Employee Id', Employee.FirstName AS 'Employee First Name', Employee.LastName AS 'Employee Last Name', Employee.DepartmentId AS 'Employee Department', Employee.IsSuperVisor AS 'Supervisor Status' FROM Department RIGHT JOIN Employee ON Department.Id=Employee.DepartmentId";
                    }

                    if (_filter == "budget")
                    {
                        query += $" WHERE Department.Budget >= '{_gt}'";
                    }


                    
                    cmd.CommandText = query;
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<Department> departments = new List<Department>();

                    while (reader.Read())
                    {
                        Department singleDept = new Department
                        {
                            id = reader.GetInt32(reader.GetOrdinal("Department Id")),
                            name = reader.GetString(reader.GetOrdinal("Department Name")),
                            budget = reader.GetInt32(reader.GetOrdinal("Department Budget"))

                        };

                        if (_filter == "budget")
                            {
                                departments.Add(singleDept);
                            };

                        if (_include == "employees")

                        {
                            Employee currentEmployee = new Employee
                            {
                                id = reader.GetInt32(reader.GetOrdinal("Employee Id")),
                                firstName = reader.GetString(reader.GetOrdinal("Employee First Name")),
                                lastName = reader.GetString(reader.GetOrdinal("Employee Last Name")),
                                isSupervisor = reader.GetBoolean(reader.GetOrdinal("Supervisor Status")),
                                departmentId = reader.GetInt32(reader.GetOrdinal("Employee Department"))

                            };


                            // If the Employee is already on the list, don't add them again!
                            if (departments.Any(d => d.id == singleDept.id))
                            {
                                Department thisDepartment = departments.Where(d => d.id == singleDept.id).FirstOrDefault();
                                thisDepartment.employees.Add(currentEmployee);
                            }
                            else
                            {
                                singleDept.employees.Add(currentEmployee);
                                departments.Add(singleDept);
                            }
                        }
                        else
                        {
                            departments.Add(singleDept);
                        }


                    }
                    reader.Close();

                    return Ok(departments);
                }
            }
        }

        [HttpGet("{id}", Name = "GetDepartment")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT Department.Id AS 'Department Id', Department.Name AS 'Department Name', Department.Budget AS 'Department Budget' FROM Department
                        WHERE Department.Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Department department = null;

                    if (reader.Read())
                    {
                        department = new Department
                        {
                            id = reader.GetInt32(reader.GetOrdinal("Department Id")),
                            name = reader.GetString(reader.GetOrdinal("Department Name")),
                            budget = reader.GetInt32(reader.GetOrdinal("Department Budget"))
                            
                        };
                    }
                    reader.Close();

                    return Ok(department);
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Department department)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Department (Name, Budget)
                                        OUTPUT INSERTED.Id
                                        VALUES (@name, @budget)";
                    cmd.Parameters.Add(new SqlParameter("@name", department.name));
                    cmd.Parameters.Add(new SqlParameter("@budget", department.budget));

                    int newId = (int)cmd.ExecuteScalar();
                    department.id = newId;
                    return CreatedAtRoute("GetDepartment", new { id = newId }, department);
                }
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Department department)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE Department
                                            SET Name = @name,
                                                Budget = @budget
                                            WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@id", id));
                        cmd.Parameters.Add(new SqlParameter("@name", department.name));
                        cmd.Parameters.Add(new SqlParameter("@budget", department.budget));

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
                if (!DepartmentExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        private bool DepartmentExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, Name, Budget
                        FROM Department
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
    }
}