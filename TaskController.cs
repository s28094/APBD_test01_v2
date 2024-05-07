using Microsoft.AspNetCore.Mvc;

namespace APBD_test01_v2;


[ApiController]
[Route("api/[controller]")]
public class TaskController : ControllerBase
{
    private readonly string _connectionString;

    public TaskController(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    [HttpGet("team-member/{id}")]
    public IActionResult GetTasksForTeamMember(int id)
    {
        var tasks = _connectionString.Tasks
            .Where(t => t.IdAssignedTo == id || t.IdCreator == id)
            .Include(t => t.Project)
            .Include(t => t.TaskType)
            .OrderByDescending(t => t.Deadline)
            .ToList();

        if (tasks == null || tasks.Count == 0)
            return NotFound();

        return Ok(tasks);
    }

    [HttpPost("add-task")]
    public IActionResult AddTask(Task task)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        
        _context.Tasks.Add(task);
        _context.SaveChanges();

        return Ok(task.IdTask);
    }

    [HttpDelete("tasks/{id}")]
    public IActionResult DeleteTask(int id)
    {
        var task = _context.Tasks.Find(id);
        if (task == null)
            return NotFound();

        _context.Tasks.Remove(task);
        _context.SaveChanges();

        return NoContent();
    }
}

