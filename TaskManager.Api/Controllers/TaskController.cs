using Microsoft.AspNetCore.Mvc;
using TaskManager.Api.Data;

namespace TaskManager.Api.Controllers
{
    [ApiController]
    public class TaskController : ControllerBase
    {
        AppDbContext _db;

        public TaskController(AppDbContext db)
        {
            _db = db;
        }





    }
}
