
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
    private readonly string _connectionString;

    public TasksController()
    {
        _connectionString = "DefaultConnection";
    }

    [HttpGet("{teamMemberId}")]
    public IActionResult GetTasksForTeamMember(int teamMemberId)
    {
        try
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var query = @"
                    SELECT t.*, p.Name AS ProjectName, tt.Name AS TaskTypeName
                    FROM Task t
                    JOIN Project p ON t.IdProject = p.IdProject
                    JOIN TaskType tt ON t.IdTaskType = tt.IdTaskType
                    WHERE t.IdCreator = @IdCreator OR t.IdAssignedTo = @IdAssignedTo
                    ORDER BY t.Deadline DESC";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@IdCreator", teamMemberId);
                    command.Parameters.AddWithValue("@IdAssignedTo", teamMemberId);

                    using (var reader = command.ExecuteReader())
                    {
                        var tasks = new List<Task>();
                        while (reader.Read())
                        {
                            tasks.Add(new Task
                            {
                                IdTask = reader.GetInt32(reader.GetOrdinal("IdTask")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                Description = reader.GetString(reader.GetOrdinal("Description")),
                                Deadline = reader.GetDateTime(reader.GetOrdinal("Deadline")),
                                IdProject = reader.GetInt32(reader.GetOrdinal("IdProject")),
                                IdTaskType = reader.GetInt32(reader.GetOrdinal("IdTaskType")),
                                IdAssignedTo = reader.GetInt32(reader.GetOrdinal("IdAssignedTo")),
                                IdCreator = reader.GetInt32(reader.GetOrdinal("IdCreator")),
                                ProjectName = reader.GetString(reader.GetOrdinal("ProjectName")),
                                TaskTypeName = reader.GetString(reader.GetOrdinal("TaskTypeName"))
                            });
                        }

                        return Ok(tasks);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpPost]
    public IActionResult AddTask([FromBody] Task task)
    {
        try
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var insertQuery = @"
                    INSERT INTO Task (Name, Description, Deadline, IdProject, IdTaskType, IdAssignedTo, IdCreator)
                    OUTPUT INSERTED.IdTask
                    VALUES (@Name, @Description, @Deadline, @IdProject, @IdTaskType, @IdAssignedTo, @IdCreator)";

                using (var command = new SqlCommand(insertQuery, connection))
                {
                    command.Parameters.AddWithValue("@Name", task.Name);
                    command.Parameters.AddWithValue("@Description", task.Description);
                    command.Parameters.AddWithValue("@Deadline", task.Deadline);
                    command.Parameters.AddWithValue("@IdProject", task.IdProject);
                    command.Parameters.AddWithValue("@IdTaskType", task.IdTaskType);
                    command.Parameters.AddWithValue("@IdAssignedTo", task.IdAssignedTo);
                    command.Parameters.AddWithValue("@IdCreator", task.IdCreator);

                    var taskId = command.ExecuteScalar();

                    if (taskId!= null)
                    {
                        return CreatedAtAction(nameof(GetTasksForTeamMember), new { teamMemberId = task.IdCreator }, task);
                    }
                    else
                    {
                        return StatusCode(500, "Failed to add task.");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteTask(int id)
    {
        try
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var deleteQuery = "DELETE FROM Task WHERE IdTask = @IdTask";

                using (var command = new SqlCommand(deleteQuery, connection))
                {
                    command.Parameters.AddWithValue("@IdTask", id);

                    var rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        return NoContent();
                    }
                    else
                    {
                        return NotFound();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}

