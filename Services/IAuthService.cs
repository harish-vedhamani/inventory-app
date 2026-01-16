using System.Threading.Tasks;
using Playground.DTOs;

namespace Playground.Services
{
    public interface IAuthService
    {
        Task RegisterAsync(AuthRegisterDto dto);
        Task<AuthResponseDto> LoginAsync(AuthLoginDto dto);
    }
}
