namespace TaskManager.Api.Data.DTO.TasksDto
{
    public class TaskItemShortDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public Model.TaskStatus Status { get; set; }
    }
}
