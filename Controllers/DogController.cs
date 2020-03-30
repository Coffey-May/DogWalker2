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


namespace DogWalker2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DogController : ControllerBase
    {
        private readonly IConfiguration _config;

        public DogController(IConfiguration config)
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
        public async Task<IActionResult> Get([FromQuery] Int32? neighborhoodId)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT d.Id, d.Name, d.Breed, d.OwnerId, o.Name AS OwnerName, o.NeighborhoodId, o.Address, o.Phone, d.Notes FROM Dog d
                                        LEFT JOIN Owner o ON d.OwnerId = o.Id
                                        WHERE 1 = 1";

                    if (neighborhoodId != null)
                    {
                        cmd.CommandText += " AND NeighborhoodId = @neighborhoodId";
                        cmd.Parameters.Add(new SqlParameter("@neighborhoodId", neighborhoodId));
                    }

                    SqlDataReader reader = cmd.ExecuteReader();
                    List<Dog> dogs = new List<Dog>();

                    while (reader.Read())
                    {
                        Dog dog = new Dog
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            Breed = reader.GetString(reader.GetOrdinal("Breed")),
                            OwnerId = reader.GetInt32(reader.GetOrdinal("OwnerId")),
                            Owner = new Owner
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Name = reader.GetString(reader.GetOrdinal("OwnerName")),
                                Address = reader.GetString(reader.GetOrdinal("Address")),
                                NeighborhoodId = reader.GetInt32(reader.GetOrdinal("NeighborhoodId")),
                                Phone = reader.GetString(reader.GetOrdinal("Phone"))
                            }
                        };
                        if (!reader.IsDBNull(reader.GetOrdinal("Notes")))
                        {
                            dog.Notes = reader.GetString(reader.GetOrdinal("Notes"));
                        }
                        dogs.Add(dog);
                    }
                    reader.Close();

                    return Ok(dogs);
                }
            }
        }


        [HttpGet("{id}", Name = "GetDog")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT
                            Id, Name, OwnerId, Breed, Notes
                        FROM Dog
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Dog dog = null;

                    if (reader.Read())
                    {
                        dog = new Dog
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            OwnerId = reader.GetInt32(reader.GetOrdinal("OwnerId")),
                            Breed = reader.GetString(reader.GetOrdinal("Breed")),
                            Notes = reader.GetString(reader.GetOrdinal("Notes")),
                        };
                    }
                    reader.Close();

                    return Ok(dog);
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Dog dog)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Dog ( Id, Name, OwnerId, Breed, Notes)
                                        OUTPUT INSERTED.Id
                                        VALUES (@name, @ownerId, @breed, @notes)";
                    cmd.Parameters.Add(new SqlParameter("@name", dog.Name));
                    cmd.Parameters.Add(new SqlParameter("@ownerId", dog.OwnerId));
                    cmd.Parameters.Add(new SqlParameter("@breed", dog.Breed));
                    cmd.Parameters.Add(new SqlParameter("@notes", dog.Notes));

                    int newId = (int)cmd.ExecuteScalar();
                    dog.Id = newId;
                    return CreatedAtRoute("GetDog", new { id = newId }, dog);
                }
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Dog dog)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE Dog
                                            SET Name = @name,
                                                OwnerId = @ownerid
                                                Breed = @breed
                                                Notes = @notes
                                            WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@name", dog.Name));
                        cmd.Parameters.Add(new SqlParameter("@ownerId", dog.OwnerId));
                        cmd.Parameters.Add(new SqlParameter("@breed", dog.Breed));
                        cmd.Parameters.Add(new SqlParameter("@notes", dog.Notes));

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
                if (!DogExists(id))
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
                        cmd.CommandText = @"DELETE FROM Dog WHERE Id = @id";
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
                if (!DogExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        private bool DogExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, Name, OwnerId, Breed, Notes
                        FROM Dog
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
    }
}