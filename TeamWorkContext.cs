namespace APBD_test01_v2;

using Microsoft.EntityFrameworkCore;

public class TeamWorkContext : DbContext
{
    public TeamWorkContext(DbContextOptions<TeamWorkContext> options) : base(options) { }

    public DbSet<Project> Projects { get; set; }
    public DbSet<Task> Tasks { get; set; }
    public DbSet<TaskType> TaskTypes { get; set; }
    public DbSet<TeamMember> TeamMembers { get; set; }
}
