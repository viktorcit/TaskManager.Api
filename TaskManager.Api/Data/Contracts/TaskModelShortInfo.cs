namespace TaskManager.Api.Data.Contracts
{
    public class TaskModelShortInfo
    {
        public required int TaskId { get; set; }
        public required string Title { get; set; }
        public Enums.TaskStatus Status { get; set; }
    }
}
