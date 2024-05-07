using Microsoft.Data.SqlClient;

namespace APBD_test01_v2;

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

public class TasksRepository : ITasksRepository
{
    private readonly string _connectionString;

    public TasksRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<List<Task>> GetTasksForTeamMember(int teamMemberId)
    {
        var tasks = new List<Task>();

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

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

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
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
                }
            }
        }

        return tasks;
    }

    public async Task<int> AddTask(Task task)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

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

                return (int)await command.ExecuteScalarAsync();
            }
        }
    }

    public async Task<bool> DeleteTask(int id)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            var deleteQuery = "DELETE FROM Task WHERE IdTask = @IdTask";

            using (var command = new SqlCommand(deleteQuery, connection))
            {
                command.Parameters.AddWithValue("@IdTask", id);

                var rowsAffected = await command.ExecuteNonQueryAsync();

                return rowsAffected > 0;
            }
        }
    }
}
