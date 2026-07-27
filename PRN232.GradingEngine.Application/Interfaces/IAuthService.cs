using System.Threading.Tasks;
using PRN232.GradingEngine.Application.DTOs;

namespace PRN232.GradingEngine.Application.Interfaces;

public interface IAuthService
{
    Task<UserClaimsDto> VerifyTokenAsync(string token);
}
