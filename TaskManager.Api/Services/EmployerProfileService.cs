using Microsoft.EntityFrameworkCore;
using TaskManager.Api.Data;
using TaskManager.Api.Model;
using TaskManager.Api.Services.Interfaces;

namespace TaskManager.Api.Services
{
    public class EmployerProfileService : IEmployerProfileService
    {
        private readonly AppDbContext _db;

        public EmployerProfileService(AppDbContext db)
        {
            _db = db;
        }


        public EmployerProfile CreateEmployerProfile(string userId, EmployerRequest request)
        {
            var profile = new EmployerProfile
            {
                UserId = userId,
                CompanyName = request.CompanyName,
                Website = request.Website,
                Description = request.Description,
                CreatedAt = DateTimeOffset.UtcNow
            };

            return profile;
        }

        public async Task<EmployerProfile?> GetEmployerProfileByUserIdAsync(string userId)
        {
            var profile = await _db.EmployerProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            if (profile == null) return null;
            return profile;
        }
    }
}
