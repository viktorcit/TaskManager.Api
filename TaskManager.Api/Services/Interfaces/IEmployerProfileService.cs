using TaskManager.Api.Model;

namespace TaskManager.Api.Services.Interfaces
{
    public interface IEmployerProfileService
    {
        EmployerProfile CreateEmployerProfile(string userId, EmployerRequest request);
        Task<EmployerProfile?> GetEmployerProfileByUserIdAsync(string userId);
    }
}
