using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data;
using Microsoft.Data.SqlClient;
using DogWalker2.Models;
using Microsoft.AspNetCore.Http;
namespace DogWalkerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OwnersController : ControllerBase
    {
        private readonly IConfiguration _config;
        public OwnersController(IConfiguration config)
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
        public async Task<IActionResult> Get(
                [FromQuery] string include,
                [FromQuery] string q)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT o.Id, o.Name, o.Address, o.NeighborhoodId, o.Phone ";
                    if (include == "neighborhood")
                    {
                        cmd.CommandText += ", n.Id, n.Name AS Neighborhood ";
                    }
                    cmd.CommandText += "FROM Owner o ";
                    if (include == "neighborhood")
                    {
                        cmd.CommandText += "LEFT JOIN Neighborhood n ON o.NeighborhoodId = n.Id ";
                    }
                    cmd.CommandText += "WHERE 1 = 1";

                    if (q != null)
                    {
                        cmd.CommandText += " AND Name LIKE @name";
                        cmd.Parameters.Add(new SqlParameter("@name", "%" + q + "%"));
                    }

                    SqlDataReader reader = cmd.ExecuteReader();
                    List<Owner> owners = new List<Owner>();

                    Owner owner = null;

                    while (reader.Read())
                    {
                        if (include == "neighborhood")
                        {
                            owner = new Owner
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                Address = reader.GetString(reader.GetOrdinal("Address")),
                                Phone = reader.GetString(reader.GetOrdinal("Phone")),
                                NeighborhoodId = reader.GetInt32(reader.GetOrdinal("NeighborhoodId")),
                                Neighborhood = new Neighborhood()
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("NeighborhoodId")),
                                    Name = reader.GetString(reader.GetOrdinal("Neighborhood"))
                                }
                            };
                        }
                        else
                        {
                            owner = new Owner
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                Address = reader.GetString(reader.GetOrdinal("Address")),
                                Phone = reader.GetString(reader.GetOrdinal("Phone")),
                                NeighborhoodId = reader.GetInt32(reader.GetOrdinal("NeighborhoodId")),
                            };
                        }

                        owners.Add(owner);
                    }
                    reader.Close();

                    return Ok(owners);
                }
            }
        }


        [HttpGet("{id}", Name = "GetOwners")]
        public async Task<IActionResult> Get(
            [FromRoute] int id,
            [FromQuery] string include
            )
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT o.Id, o.Name, o.Address, o.NeighborhoodId, o.Phone, d.id AS DogId, d.Name AS DogName, d.Breed, d.OwnerId ";
                    if (include == "neighborhood")
                    {
                        cmd.CommandText += ", n.Id, n.Name AS Neighborhood ";
                    }
                    cmd.CommandText += "FROM Owner o LEFT JOIN Dog d ON o.Id = d.OwnerId ";
                    if (include == "neighborhood")
                    {
                        cmd.CommandText += "LEFT JOIN Neighborhood n ON o.NeighborhoodId = n.Id ";
                    }
                    cmd.CommandText += "WHERE o.Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Owner owner = null;

                    if (include == "neighborhood")
                    {
                        while (reader.Read())
                        {
                            if (owner == null)
                            {
                                owner = new Owner
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                    Name = reader.GetString(reader.GetOrdinal("Name")),
                                    Address = reader.GetString(reader.GetOrdinal("Address")),
                                    Phone = reader.GetString(reader.GetOrdinal("Phone")),
                                    NeighborhoodId = reader.GetInt32(reader.GetOrdinal("NeighborhoodId")),
                                    Neighborhood = new Neighborhood()
                                    {
                                        Id = reader.GetInt32(reader.GetOrdinal("NeighborhoodId")),
                                        Name = reader.GetString(reader.GetOrdinal("Neighborhood"))
                                    },
                                    DogsL = new List<Dog>()
                                };
                            }

                            owner.DogsL.Add(new Dog()
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("DogId")),
                                Name = reader.GetString(reader.GetOrdinal("DogName")),
                                Breed = reader.GetString(reader.GetOrdinal("Breed")),
                                OwnerId = reader.GetInt32(reader.GetOrdinal("OwnerId"))
                            });
                        }
                    }
                    else
                    {
                        while (reader.Read())
                        {
                            if (owner == null)
                            {
                                owner = new Owner
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                    Name = reader.GetString(reader.GetOrdinal("Name")),
                                    Address = reader.GetString(reader.GetOrdinal("Address")),
                                    Phone = reader.GetString(reader.GetOrdinal("Phone")),
                                    NeighborhoodId = reader.GetInt32(reader.GetOrdinal("NeighborhoodId")),
                                    DogsL = new List<Dog>()
                                };
                            }

                            owner.DogsL.Add(new Dog()
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("DogId")),
                                Name = reader.GetString(reader.GetOrdinal("DogName")),
                                Breed = reader.GetString(reader.GetOrdinal("Breed")),
                                OwnerId = reader.GetInt32(reader.GetOrdinal("OwnerId"))
                            });
                        }
                    }
                    reader.Close();

                    return Ok(owner);
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Owner owner)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Owner (Name, Address, Phone, NeighborhoodId)
                                        OUTPUT INSERTED.Id
                                        VALUES (@Name, @Address, @Phone, @NeighborhoodId)";
                    cmd.Parameters.Add(new SqlParameter("@Name", owner.Name));
                    cmd.Parameters.Add(new SqlParameter("@Address", owner.Address));
                    cmd.Parameters.Add(new SqlParameter("@Phone", owner.Phone));
                    cmd.Parameters.Add(new SqlParameter("@NeighborhoodId", owner.NeighborhoodId));
                    int newId = (int)cmd.ExecuteScalar();
                    owner.Id = newId;
                    return CreatedAtRoute("GetOwner", new { id = newId }, owner);
                }
            }
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Owner owner)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE Owner
                                            SET Name = @Name,
                                                Address = @Address,
                                                Phone = @Phone,
                                                NeighborhoodId = @NeighborhoodId
                                            WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@Name", owner.Name));
                        cmd.Parameters.Add(new SqlParameter("@Address", owner.Address));
                        cmd.Parameters.Add(new SqlParameter("@Phone", owner.Phone));
                        cmd.Parameters.Add(new SqlParameter("@NeighborhoodId", owner.NeighborhoodId));
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
                if (!OwnerExists(id))
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
                        cmd.CommandText = @"DELETE FROM Owner WHERE Id = @id";
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
                if (!OwnerExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }
        private bool OwnerExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, Name, Address, Phone, NeighborhoodId
                        FROM Owner
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
    }
}