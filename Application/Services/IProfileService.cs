using SpotMate.Application.DTOs.Requests;
using SpotMate.Application.DTOs.Responses;
using SpotMate.Application.OperationResult;
using SpotMate.Domain.Enums;

namespace SpotMate.Application.Services;

public interface IProfileService
{
    Task<Result<UserDto>> GetProfileAsync(Guid userId);
    Task<Result> EditProfileAsync(EditUserDto dto, Guid userId);
    Task<Result> UploadAvatarAsync(UploadAvatarDto uploadAvatarDto, Guid userId);
    Task<Result> DeleteAvatarAsync(Guid userId);
    Task<Result> MakeVisibleAsync(Guid userId);
    Task<Result> MakeInvisibleAsync(Guid userId);
    Task<Result> EditUserStatus(UserStatus? userStatus, Guid userId);
    Task<Result> ShareInterestBasedLocation(Guid userId);
    Task<Result> DisableInterestBasedLocation(Guid userId);
}