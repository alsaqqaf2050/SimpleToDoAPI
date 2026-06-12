using Microsoft.AspNetCore.Mvc;
using SimpleToDoAPI.DTOs;
using SimpleToDoAPI.Services.Interfaces;
using SimpleToDoAPI.Helpers;
using Microsoft.AspNetCore.Authorization;
using SimpleToDoAPI.Constants;

namespace SimpleToDoAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TodosController : ControllerBase
    {
        private readonly ITodoService _todoService;
        private readonly ILogger<TodosController> _logger;

        // هنا يتم حقن todoService و ال logger تلقائيًا عبر الـ Constructor Injection
        public TodosController(ITodoService todoService,ILogger<TodosController> logger)
        {
            _todoService = todoService;
            _logger = logger;
        }

        // =========================================
        // GET: api/todos
        // =========================================
        //[HttpGet]
        //public async Task<IActionResult> GetAll()
        //{
        //    var items = await _todoService.GetAllAsync();

        //    //return Ok(new
        //    //{
        //    //    success = true,
        //    //    count = items.Count(),
        //    //    data = items
        //    //});

        //    return Ok(new ApiResponse<IEnumerable<TodoResponseDto>>(true,"تم جلب المهام بنجاح",items));
        //}

        //[HttpGet]
        //public async Task<IActionResult> GetAll( [FromQuery] TodoQueryParametersDto parameters)
        //{
        //    var items = await _todoService.GetAllAsync(parameters);

        //    return Ok(new ApiResponse<IEnumerable<TodoResponseDto>>(true,"تم جلب المهام بنجاح",items));
        //}

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] TodoQueryParametersDto parameters)
        {
            var result = await _todoService.GetAllAsync(parameters);

            var pagination = new PaginationMetadata
            {
                PageNumber = result.PageNumber,
                PageSize = result.PageSize,
                TotalRecords = result.TotalRecords,
                TotalPages = result.TotalPages
            };

            return Ok(new ApiResponse<IEnumerable<TodoResponseDto>>(true, "تم جلب المهام بنجاح", result.Items, pagination));
        }

        // =========================================
        // GET: api/todos/5
        // =========================================
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _todoService.GetByIdAsync(id);

            if (item == null)
            {
                //return NotFound(new
                //{
                //    success = false,
                //    message = $"المهمة بالمعرف {id} غير موجودة"
                //});

                return NotFound(new ApiErrorResponse($"المهمة بالمعرف {id} غير موجودة"));
            }

            //return Ok(new
            //{
            //    success = true,
            //    data = item
            //});

            return Ok(new ApiResponse<TodoResponseDto>( true,"تم جلب المهمة بنجاح",item));
        }

        // =========================================
        // POST: api/todos
        // =========================================
        [Authorize(Policy = Policies.TodoCreate)]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTodoDto dto)
        {
            if (!ModelState.IsValid)
            {
                //return BadRequest(new
                //{
                //    success = false,
                //    errors = ModelState
                //});

                return BadRequest(new ApiErrorResponse("البيانات غير صحيحة",ModelState));
            }

            var createdItem = await _todoService.CreateAsync(dto);

            //return CreatedAtAction(
            //    nameof(GetById),
            //    new { id = createdItem.Id },
            //    new
            //    {
            //        success = true,
            //        message = "تم إنشاء المهمة بنجاح",
            //        data = createdItem
            //    });

            return CreatedAtAction( nameof(GetById),new { id = createdItem.Id },new ApiResponse<TodoResponseDto>( true,"تم إنشاء المهمة بنجاح",createdItem));
        }

        // =========================================
        // PUT: api/todos/5
        // =========================================
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id,[FromBody] UpdateTodoDto dto)
        {
            if (!ModelState.IsValid)
            {
                //return BadRequest(new
                //{
                //    success = false,
                //    errors = ModelState
                //});

                return BadRequest(new ApiErrorResponse("البيانات غير صحيحة", ModelState));
            }

            var updatedItem = await _todoService.UpdateAsync(id, dto);

            if (updatedItem == null)
            {
                //return NotFound(new
                //{
                //    success = false,
                //    message = $"المهمة بالمعرف {id} غير موجودة"
                //});

                return NotFound(new ApiErrorResponse($"المهمة بالمعرف {id} غير موجودة"));
            }

            return Ok(new
            {
                success = true,
                message = "تم تحديث المهمة بنجاح",
                data = updatedItem
            });
        }

        // =========================================
        // DELETE: api/todos/5
        // =========================================
        //[HttpDelete("{id}")]
        [Authorize(Policy = Policies.TodoDelete)]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _todoService.DeleteAsync(id);

            if (!result)
            {
                //return NotFound(new
                //{
                //    success = false,
                //    message = $"المهمة بالمعرف {id} غير موجودة"
                //});

                return NotFound(new ApiErrorResponse($"المهمة بالمعرف {id} غير موجودة"));
            }

            //return Ok(new
            //{
            //    success = true,
            //    message = "تم حذف المهمة بنجاح"
            //});

            return Ok(new ApiResponse<object>(true,"تم حذف المهمة بنجاح",null));
        }
    }
}