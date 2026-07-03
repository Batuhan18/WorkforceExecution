using MediatR;
using WorkforceExecution.Application.Common;
using WorkforceExecution.Application.Interfaces;

namespace WorkforceExecution.Application.Features.Auth.Commands;

public class LoginCommand : IRequest<Result<LoginResultDto>>
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}

public class LoginResultDto
{
    public string Token { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string Role { get; set; } = null!;
    public string? LocationCode { get; set; }
    public string? CrewRegion { get; set; }
}

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginResultDto>>
{
    private readonly IUserRepository _users;
    private readonly ITokenService _tokenService;
    private readonly IPasswordHasher _hasher;

    public LoginCommandHandler(IUserRepository users, ITokenService tokenService, IPasswordHasher hasher)
    {
        _users = users;
        _tokenService = tokenService;
        _hasher = hasher;
    }

    public async Task<Result<LoginResultDto>> Handle(LoginCommand request, CancellationToken ct)
    {
        var user = await _users.GetByEmailAsync(request.Email.Trim().ToLowerInvariant(), ct);
        if (user is null || !_hasher.Verify(request.Password, user.PasswordHash))
            return Result<LoginResultDto>.Failure("E-posta veya sifre hatali.");

        return Result<LoginResultDto>.Success(new LoginResultDto
        {
            Token = _tokenService.CreateToken(user),
            Email = user.Email,
            FullName = user.FullName,
            Role = user.Role.ToString(),
            LocationCode = user.Location?.Code,
            CrewRegion = user.CrewRegion?.Name
        });
    }
}
