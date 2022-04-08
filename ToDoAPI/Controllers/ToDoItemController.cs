using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ToDoAPI.Models;

namespace ToDoAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ToDoItemController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        private readonly IConfiguration Configuration;

        public ToDoItemController(ApplicationDbContext context, IConfiguration _configuration)
        {
            _context = context;
            Configuration = _configuration;
        }

        [HttpGet]
        public ActionResult<IEnumerable<ToDoItemModel>> GetToDoItems()
        {
            SqlConnection sqlConn = new(this.Configuration.GetConnectionString("SQLConnection"));

            sqlConn.Open();

            SqlCommand cmd = new("SELECT * FROM ToDoItems WHERE ItemStatus = 0 ORDER BY ItemDate ASC", sqlConn);
            SqlDataReader reader = cmd.ExecuteReader();

            List<ToDoItemModel> list = new();

            while (reader.Read())
            {
                ToDoItemModel item = new()
                {
                    ItemId = (int)reader["ItemId"],
                    ItemName = (string)reader["ItemName"],
                    ItemDate = (string)reader["ItemDate"],
                    ItemStatus = (bool)reader["ItemStatus"]
                };

                list.Add(item);
            }

            return list;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ToDoItemModel>> GetToDoItemModel(int id)
        {
            var toDoItemModel = await _context.ToDoItems.FindAsync(id);

            if (toDoItemModel == null)
            {
                return NotFound();
            }

            return toDoItemModel;
        }

        [HttpPatch("{id:int}")]
        public async Task<IActionResult> PatchToDoItemModel(int id, [FromBody] JsonPatchDocument<ToDoItemModel> patchEntity)
        {
            var entity = await _context.ToDoItems.FindAsync(id);
            if (entity == null)
            {
                return NotFound();
            }
            patchEntity.ApplyTo(entity, ModelState);

            await _context.SaveChangesAsync();

            return Ok(entity);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutToDoItemModel(int id, ToDoItemModel toDoItemModel)
        {
            if (id != toDoItemModel.ItemId)
            {
                return BadRequest();
            }

            _context.Entry(toDoItemModel).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ToDoItemModelExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<ToDoItemModel>> PostToDoItemModel(ToDoItemModel toDoItemModel)
        {
            _context.ToDoItems.Add(toDoItemModel);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetToDoItemModel", new { id = toDoItemModel.ItemId }, toDoItemModel);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteToDoItemModel(int id)
        {
            var toDoItemModel = await _context.ToDoItems.FindAsync(id);
            if (toDoItemModel == null)
            {
                return NotFound();
            }

            _context.ToDoItems.Remove(toDoItemModel);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ToDoItemModelExists(int id)
        {
            return _context.ToDoItems.Any(e => e.ItemId == id);
        }
    }
}
