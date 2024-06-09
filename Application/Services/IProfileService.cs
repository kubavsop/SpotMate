using SpotMate.Application.DTOs.Requests;
using SpotMate.Application.DTOs.Responses;
using SpotMate.Application.OperationResult;

namespace SpotMate.Application.Services;

public interface IProfileService
{
    Task<Result<UserDto>> GetProfileAsync(Guid userId);
    Task<Result> EditProfileAsync(EditUserDto dto, Guid userId);
    Task<Result> UploadAvatar(UploadAvatarDto uploadAvatarDto, Guid userId);
    Task<Result> DeleteAvatar(Guid userId);
}