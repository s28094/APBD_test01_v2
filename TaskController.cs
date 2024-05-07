
using Microsoft.Data.SqlClient;

namespace APBD_test01_v2;

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class TasksController : ControllerBase
{
    private readonly ITasksRepository _tasksRepository;

    public TasksController(ITasksRepository tasksRepository)
    {
        _tasksRepository = tasksRepository;
    }

    [HttpGet("{teamMemberId}")]
    public async Task<IActionResult> GetTasksForTeamMember(int teamMemberId)
    {
        var tasks = await _tasksRepository.GetTasksForTeamMember(teamMemberId);
        if (tasks == null || tasks.Count == 0)
        {
            return NotFound();
        }
        return Ok(tasks);
    }

    [HttpPost]
    public async Task<IActionResult> AddTask([FromBody] Task task)
    {
        var newTaskId = await _tasksRepository.AddTask(task);
        if (newTaskId <= 0)
        {
            return StatusCode(500, "Failed to add task.");
        }
        return CreatedAtAction(nameof(GetTasksForTeamMember), new { teamMemberId = task.IdCreator }, task);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTask(int id)
    {
        var success = await _tasksRepository.DeleteTask(id);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }
}


