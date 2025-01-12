using System.Threading.Tasks;

namespace MembershipAPI.Services.UserInfo
{
    using Models;

    public interface IUserInfoService
    {
        Task<Name> GetNameAsync(string userId, bool silentlyFail);

        Task<Photo?> GetPhotoAsync(
            string userId,
            PhotoSize size = null,
            PhotoResizeCondition resizeCondition = PhotoResizeCondition.ResizeIfDefault,
            int resizeQuality = 85
            );
    }
}
