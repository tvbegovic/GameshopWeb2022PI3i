using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using Dapper;
using Microsoft.AspNetCore.Authorization;

namespace GameshopWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GameController : ControllerBase
    {
        private IConfiguration _configuration;
        public GameController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("genres")]        
        public List<Genre> GetGenres()
        {
            using(var conn = new SqlConnection(_configuration.GetConnectionString("gameshop")))
            {
                return conn.Query<Genre>("SELECT * FROM Genre").ToList();
            }
        }

        [HttpGet("companies")]
        public List<Company> GetCompanies()
        {
            using (var conn = new SqlConnection(_configuration.GetConnectionString("gameshop")))
            {
                return conn.Query<Company>("SELECT * FROM Company").ToList();
            }
        }

        [HttpGet("bygenre/{id}")]
        public List<Game> GetByGenre(int id)
        {
            using (var conn = new SqlConnection(_configuration.GetConnectionString("gameshop")))
            {
                return conn.Query<Game>("SELECT * FROM Game WHERE idGenre = @id", new { id }).ToList();
            }
        }

        [HttpGet("bycompany/{id}")]
        public List<Game> GetByCompany(int id)
        {
            using (var conn = new SqlConnection(_configuration.GetConnectionString("gameshop")))
            {
                return conn.Query<Game>("SELECT * FROM Game WHERE idDeveloper = @id OR idPublisher = @id",
                    new { id }).ToList();
            }
        }

        [HttpGet("search/{text}")]
        public List<Game> Search(string text)
        {
            using (var conn = new SqlConnection(_configuration.GetConnectionString("gameshop")))
            {
                return conn.Query<Game>(
                    @"SELECT game.* FROM Game
                     INNER JOIN Genre ON Game.idGenre = Genre.id
                     INNER JOIN Company Developer ON Game.idDeveloper = Developer.id
                     INNER JOIN Company Publisher ON Game.idPublisher = Publisher.id
                     WHERE title LIKE @text OR Genre.name LIKE @text OR Developer.name LIKE @text 
                     OR Publisher.name LIKE @text",
                    new { text = $"%{text}%" }).ToList();
            }
        }

        [HttpPost("")]
        [Authorize]
        public Game Create(Game game)
        {
            using (var conn = new SqlConnection(_configuration.GetConnectionString("gameshop")))
            {
                var sql = @"INSERT INTO Game(
                    title,idGenre,idPublisher,price,idDeveloper,releaseDate,image
                    ) OUTPUT inserted.id VALUES(
                    @title,@idGenre,@idPublisher,@price,@idDeveloper,@releaseDate,@image
                    )";
                game.Id = conn.ExecuteScalar<int>(sql, game);
                return game;
            }
        }

        [HttpPut("")]
        [Authorize]
        public void Update(Game game)
        {
            using (var conn = new SqlConnection(_configuration.GetConnectionString("gameshop")))
            {
                var sql = @"UPDATE Game SET 
                title=@title,idGenre=@idGenre,idPublisher=@idPublisher,price=@price,idDeveloper=@idDeveloper,
                releaseDate=@releaseDate,image=@image
                WHERE id = @id
                ";
                conn.Execute(sql, game);
            }
        }

        [HttpDelete("{id}")]
        [Authorize]
        public void Delete(int id)
        {
            using (var conn = new SqlConnection(_configuration.GetConnectionString("gameshop")))
            {
                var sql = "DELETE FROM Game WHERE id = @id";
                conn.Execute(sql, new { id });
            }
        }


    }
}
