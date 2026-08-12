using CallCenter.Domain.Entities;

namespace CallCenter.Application.Interfaces;

public interface ITokenService
{
    string GenerateToken(Agent agent);
}
