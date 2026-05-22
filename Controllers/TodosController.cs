// Controllers/TodosController.cs
using Microsoft.AspNetCore.Mvc;
using SimpleTodoAPI.Models;
using SimpleTodoAPI.Services;
using System.Threading.Tasks;

namespace SimpleTodoAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TodosController : ControllerBase
    {
        private readonly ITodoService _todoService;

        public TodosController(ITodoService todoService)
        {
            _todoService = todoService;
        }

        // GET: api/todos
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await _todoService.GetAllAsync();
            return Ok(items);
        }

        // GET: api/todos/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _todoService.GetByIdAsync(id);
            if (item == null)
                return NotFound(new { message = $"العنصر مع المعرف {id} غير موجود" });

            return Ok(item);
        }

        //// POST: api/todos
        //[HttpPost]
        //public async Task<IActionResult> Create([FromBody] TodoItem item)
        //{
        //    if (!ModelState.IsValid)
        //        return BadRequest(ModelState);

        //    var createdItem = await _todoService.CreateAsync(item);

        //    return CreatedAtAction(nameof(GetById),
        //        new { id = createdItem.Id },
        //        createdItem);
        //}

        // ثم استخدمه في الـ Controller
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTodoDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // إنشاء كائن TodoItem من DTO
            var item = new TodoItem
            {
                Title = dto.Title,
                Description = dto.Description,
                IsCompleted = dto.IsCompleted,
                CreatedDate = DateTime.UtcNow
                // لا تذكر Id هنا أبداً
            };

            var createdItem = await _todoService.CreateAsync(item);

            return CreatedAtAction(nameof(GetById), new { id = createdItem.Id }, createdItem);
        }

        // PUT: api/todos/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] TodoItem item)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updatedItem = await _todoService.UpdateAsync(id, item);
            if (updatedItem == null)
                return NotFound(new { message = $"العنصر مع المعرف {id} غير موجود" });

            return Ok(updatedItem);
        }

        // DELETE: api/todos/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _todoService.DeleteAsync(id);
            if (!result)
                return NotFound(new { message = $"العنصر مع المعرف {id} غير موجود" });

            return NoContent();
        }

        //// GET: api/todos/search?keyword=test
        //[HttpGet("search")]
        //public async Task<IActionResult> Search([FromQuery] string keyword)
        //{
        //    if (string.IsNullOrWhiteSpace(keyword))
        //        return BadRequest(new { message = "كلمة البحث مطلوبة" });

        //    var items = await _todoService.SearchAsync(keyword);
        //    return Ok(items);
        //}

        //// GET: api/todos/completed
        //[HttpGet("completed")]
        //public async Task<IActionResult> GetCompleted()
        //{
        //    var items = await _todoService.GetCompletedAsync();
        //    return Ok(items);
        //}
    }
}