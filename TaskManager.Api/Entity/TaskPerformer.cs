using TaskManager.Api.Data.Contracts;

namespace TaskManager.Api.Entity
{
    public class TaskPerformer
    {
        public int Id { get; set; }
        public required int TaskId { get; set; }
        public required TaskModel Task { get; set; }
        public required string PerformerId { get; set; }
        public required ApplicationUser Performer { get; set; }
    }
}
