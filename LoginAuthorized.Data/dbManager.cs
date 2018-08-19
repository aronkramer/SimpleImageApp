using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoginAuthorized.Data
{
    public class dbManager
    {
        private string _connectionString;

        public dbManager(string ConnectionString)
        {
            _connectionString = ConnectionString;
        }

        public void AddUser(User user, string Password)
        {
            string salt = PasswordHelper.GenerateSalt();
            string hash = PasswordHelper.HashPassword(Password, salt);
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "INSERT INTO Users " +
                                  "VALUES(@name, @email, @hash, @salt)";
                cmd.Parameters.AddWithValue("@name", user.Name);
                cmd.Parameters.AddWithValue("@email", user.Email);
                cmd.Parameters.AddWithValue("@hash", hash);
                cmd.Parameters.AddWithValue("@salt", salt);
                connection.Open();
                cmd.ExecuteNonQuery();
            }
            
        }

        public User GetUser(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "SELECT top 1 * FROM users WHERE Id = @id";
                cmd.Parameters.AddWithValue("@id", id);
                connection.Open();
                User user = new User();
                SqlDataReader reader = cmd.ExecuteReader();
                if (!reader.Read())
                {
                    return null;
                }
                while (reader.Read())
                {
                    user.Id = (int)reader["id"];
                    user.Name = (string)reader["name"];
                    user.PasswordHash = (string)reader["passwordhash"];
                }
                return user;
            }
        }

        public string GetSaltById(string email)
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "select PasswordSalt from users where email = @email";
                cmd.Parameters.AddWithValue("@email", email);
                connection.Open();
                var salt = (string)cmd.ExecuteScalar();
                if(salt == null)
                {
                    return null;
                }
                return salt;
            }
        }

        public string GetHashById(string email)
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "select PasswordHash from users where email = @email";
                cmd.Parameters.AddWithValue("@email", email);
                connection.Open();
                var hash = (string)cmd.ExecuteScalar();
                if (hash == null)
                {
                    return null;
                }
                return hash;
            }
        }

        public void AddImage(Images image)
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "INSERT INTO Images(filename, password, views, userid) " +
                                  "VALUES(@filename, @password, 0, @userid) select scope_identity()";
                cmd.Parameters.AddWithValue("@filename", image.FileName);
                cmd.Parameters.AddWithValue("@password", image.Password);
                cmd.Parameters.AddWithValue("@userid", image.UserId);
                connection.Open();
                image.Id =  (int)(decimal)cmd.ExecuteScalar();
            }
        }

        public Images ViewImage(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "SELECT top 1 * FROM Images WHERE Id = @id";
                cmd.Parameters.AddWithValue("@id", id);
                connection.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                if (!reader.Read())
                {
                    return null;
                }
                return new Images
                {
                    Id = (int)reader["Id"],
                    Password = (string)reader["Password"],
                    FileName = (string)reader["FileName"],
                    Views = (int)reader["Views"]
                };
            }
        }

        public void IncrementView(int id)
        {
            using (var con = new SqlConnection(_connectionString))
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = "UPDATE Images set views = views + 1 where id = @id";
                cmd.Parameters.AddWithValue("@id", id);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void Delete()
        {
            using (var con = new SqlConnection(_connectionString))
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = "delete images delete users";
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public User GetUserByEmail(string email)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            using (SqlCommand cmd = con.CreateCommand())
            {
                cmd.CommandText = "select top 1 * from users where email = @email";
                cmd.Parameters.AddWithValue("@email", email);
                con.Open();
                var reader = cmd.ExecuteReader();
                if(!reader.Read())
                {
                    return null;
                }
                return new User
                {
                    Email = email,
                    Id = (int)reader["id"],
                    Name = (string)reader["name"],
                    PasswordHash = (string)reader["passwordhash"],
                    PasswordSalt = (string)reader["passwordsalt"]
                };
            }
        }

        public IEnumerable<Images> GetUsersImages(string email)
        {
            using (var con = new SqlConnection(_connectionString))
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = "select i.* from images i join users u " +
                                  "on i.userid = u.id " +
                                  "where u.email = @email";
                cmd.Parameters.AddWithValue("@email", email);
                con.Open();
                var images = new List<Images>();
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    images.Add(new Images
                    {
                        Id = (int)reader["id"],
                        FileName = (string)reader["filename"],
                        Password = (string)reader["password"],
                        Views = (int)reader["views"]
                    });
                }
                return images;
            }
        }

        public void DeleteImage(int id)
        {
            using (var con = new SqlConnection(_connectionString))
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = "delete images where id = @id";
                cmd.Parameters.AddWithValue("@id", id);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }
}
