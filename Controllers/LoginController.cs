using System;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using RefactorThis.Models;

namespace RefactorThis.Controllers
{
    // Security told us we have to have a login and password to be compliant

    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase 
    {
        [HttpGet("{username}/{password}")]
        public IActionResult Login(string username, string password) 
        {
            var conn = Helpres.NewConnection<SqliteConnection>();
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = $"select APIToken,APITokenExpiry from Login where name='{username}' and password='{password}'";

            var rdr = cmd.ExecuteReader();

            if (!rdr.Read())
                return BadRequest("You can't log in");

            var APITokenExpiry = rdr["APITokenExpiry"].ToString();
            var realExpiryDate = DateTime.Today;

            // bug fix for default expiry dates from stackexchange
            if (DateTime.TryParse(APITokenExpiry, out realExpiryDate)) {
                // that's good
            }

            if (DateTime.Now < realExpiryDate) {
                return Accepted("Youare logged in and can use the API");
            }
            else 
            {
                return BadRequest("Your api token has expired, please ask Reliability for a new one to be assigned to you");
            }
        }
    }
}