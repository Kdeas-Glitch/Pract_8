using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Pract_8_Demin.Models;
using Pract_8_Demin.Service;
using System.Net.Http.Headers;
using System.Security.Claims;

namespace Pract_8_Demin.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class tasksController : ControllerBase
    {
        static private readonly DeminDbContext _db = DBService.Instance.Context;
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public IActionResult GetAll()
        {
            return Ok(_db.Tasks.ToList());
        }

        [HttpGet]
        [Authorize(Roles = "Admin, User")]
        public IActionResult Get()
        {
            if (User.FindFirstValue(ClaimTypes.Role) == "User")
            {
                string? userId = User.FindFirstValue(ClaimTypes.Name);
                return Ok(_db.Tasks.ToList().Where(item => item.OwnerLogin == userId));
            }
            else
            {
                return Ok(_db.Tasks.ToList());
            }
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin, User")]
        public IActionResult GetId(int id)
        {
            var task = _db.Tasks.FirstOrDefault(item => item.Id == id);

            if (task == null)
                return NotFound();

            if (User.FindFirstValue(ClaimTypes.Role) == "User")
            {
                string? userId = User.FindFirstValue(ClaimTypes.Name);

                if (task.OwnerLogin == userId)
                    return Ok(task);
                else
                    return Forbid();
            }
            else
            {
                return Ok(task);
            }
        }


        [HttpPost]
        [Authorize(Roles = "Admin, User")]
        public IActionResult Post(CreateFor c)
        {
            if (c.title.IsNullOrEmpty() || c.description.IsNullOrEmpty())
                return Conflict("Поля description и title должны быть заполнены");

            Models.Task task = new Models.Task();

            task.Id = 0;
            task.Title = c.title;
            task.Description = c.description;
            task.OwnerLogin = User.FindFirstValue(ClaimTypes.Name);
            task.IsCompleted = false;

            _db.Tasks.Add(task);
            _db.SaveChanges();



            return Created(string.Empty, task);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin, User")]

        public IActionResult Patch(int id,PutFor p)
        {
            var task = _db.Tasks.FirstOrDefault(item => item.Id == id);

            if (task == null)
                return NotFound();

            if (User.FindFirstValue(ClaimTypes.Role) == "User")
            {
                string? userId = User.FindFirstValue(ClaimTypes.Name);

                if (task.OwnerLogin == userId)
                {
                    task.Title = p.title;
                    task.Description= p.description;
                    task.IsCompleted = p.isCom;

                    _db.SaveChanges();
                    return Ok(task);
                }
                else
                    return Forbid();
            }
            else
            {
                task.Title = p.title;
                task.Description = p.description;
                task.IsCompleted = p.isCom;

                _db.SaveChanges();
                return Ok(task);
            }
        }


        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin, User")]
        public IActionResult Delete(int id)
        {
            var task = _db.Tasks.FirstOrDefault(item => item.Id == id);

            if (task == null)
                return NotFound();

            if (User.FindFirstValue(ClaimTypes.Role) == "User")
            {
                string? userId = User.FindFirstValue(ClaimTypes.Name);

                if (task.OwnerLogin == userId)
                {
                    _db.Tasks.Remove(task);
                    _db.SaveChanges();
                    return NoContent();
                }
                return Forbid();
            }
            else
            {
                _db.Tasks.Remove(task);
                _db.SaveChanges();
                return NoContent();
            }
        }



    }

}
public record CreateFor(string title, string description);
public record PutFor(string title, string description,bool isCom);