using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using Dapper;

namespace GameshopWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private IConfiguration _configuration;
        public OrderController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("")]
        public Order Create(Order order)
        {
            using (var conn = new SqlConnection(_configuration.GetConnectionString("gameshop")))
            {
                conn.Open();
                var tr = conn.BeginTransaction();
                try
                {
                    var userSql = @"INSERT INTO [dbo].[User]
                        ([firstname],[lastname],[address],[email],[password],[admin],[City]) OUTPUT inserted.id
                        VALUES
                        (@firstname,@lastname, @address, @email, @password, @admin, @city)";
                    order.IdUser = conn.ExecuteScalar<int>(userSql, order.User,tr);
                    order.User.Id = order.IdUser.Value;
                    var orderSql = @"INSERT INTO [Order] (idUser, dateOrdered) OUTPUT inserted.id
                        VALUES(@idUser, @dateOrdered)";
                    order.DateOrdered = DateTime.Now;
                    order.Id = conn.ExecuteScalar<int>(orderSql, order, tr);
                    var detailSql = @"INSERT INTO OrderDetail(idOrder, idGame, quantity, unitprice) OUTPUT inserted.id
                        VALUES(@idOrder, @idGame, @quantity, @unitprice)";
                    foreach (var detail in order.Details)
                    {
                        detail.IdOrder = order.Id;
                        detail.Id = conn.ExecuteScalar<int>(detailSql, detail, tr);
                    }
                    tr.Commit();
                    return order;
                }
                catch (Exception)
                {
                    tr.Rollback();
                    throw;
                }
            }
        }
    }
}
