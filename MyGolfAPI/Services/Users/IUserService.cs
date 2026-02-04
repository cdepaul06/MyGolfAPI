using MyGolfAPI.DTOs.Users;

namespace MyGolfAPI.Services.Users
{
    public interface IUserService
    {
        Task<List<UserReadDto>> GetAllAsync(CancellationToken ct = default);
    }
}
