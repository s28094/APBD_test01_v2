namespace APBD_test01_v2;

public interface ITasksRepository
{
    Task<List<Task>> GetTasksForTeamMember(int teamMemberId);
    Task<int> AddTask(Task task);
    Task<bool> DeleteTask(int id);
}
