using SpotMate.Application.DTOs.Requests;
using SpotMate.Application.DTOs.Responses;
using SpotMate.Application.OperationResult;

namespace SpotMate.Application.Services.Impl;

public class UserService: IUserService
{
    public Task<Result<UserDto>> GetProfileAsync(Guid userId)
    {
        throw new NotImplementedException();
    }

    public Task<Result> EditProfileAsync(EditUserDto dto, Guid userId)
    {
        throw new NotImplementedException();
    }
}