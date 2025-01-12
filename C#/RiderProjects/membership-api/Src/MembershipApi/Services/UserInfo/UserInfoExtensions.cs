using Sigma.BoostApp.Shared;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MembershipAPI.Services.UserInfo
{
    using Models;

    public static class UserInfoExtensions
    {
        public static async Task<Name[]> GetNamesAsync(this IUserInfoService service, IEnumerable<string> userIds, bool silentlyFail)
            => await Task.WhenAll(
                userIds.Select(async userId =>
                    await service.GetNameAsync(userId, silentlyFail)
                ));

        public static async Task<IdName[]> GetIdNamesAsync(this IUserInfoService service, IEnumerable<string> userIds, bool silentlyFailNames)
            => await Task.WhenAll(
                userIds.Select(async userId =>
                    new IdName(id: userId, await service.GetNameAsync(userId, silentlyFailNames))
                ));

        public static async Task<Member[]> GetMembersAsync(this IUserInfoService service,
            IEnumerable<string> userIds,
            bool silentlyFailNames,
            PhotoSize size,
            PhotoResizeCondition photoResizeCondition = PhotoResizeCondition.ResizeIfDefault,
            int photoResizeQuality = 85
            )
            => await Task.WhenAll(
                userIds.Select(async userId =>
                    await service.GetMemberAsync(
                        userId: userId,
                        silentlyFailNames: silentlyFailNames,
                        size: size,
                        photoResizeCondition: photoResizeCondition,
                        photoResizeQuality: photoResizeQuality)
                ));

        public static async Task<Photo?[]> GetPhotosAsync(this IUserInfoService service,
            IEnumerable<string> userIds,
            PhotoSize size,
            PhotoResizeCondition resizeCondition = PhotoResizeCondition.ResizeIfDefault,
            int resizeQuality = 85
            )
            => await Task.WhenAll(
                userIds.Select(async userId =>
                    await service.GetPhotoAsync(userId, size)
                ));

        public static async Task<Member> GetMemberAsync(this IUserInfoService service,
            string userId,
            bool silentlyFailNames,
            PhotoSize size,
            PhotoResizeCondition photoResizeCondition = PhotoResizeCondition.ResizeIfDefault,
            int photoResizeQuality = 85)
        {
            Photo? photo = await service.GetPhotoAsync(
                userId: userId,
                size: size,
                resizeCondition: photoResizeCondition,
                resizeQuality: photoResizeQuality);
            var name = await service.GetNameAsync(
                userId: userId,
                silentlyFail: silentlyFailNames);
            return new Member(
                id: userId,
                name: name,
                photo: photo
                );
        }

        public static async Task<Sigma.BoostApp.Contracts.UserInfo> GetFullUserInfoAsync(this IUserInfoService service,
            string userId,
            bool silentlyFailNames,
            PhotoSize size,
            PhotoResizeCondition photoResizeCondition = PhotoResizeCondition.ResizeIfDefault,
            int photoResizeQuality = 85)
        {
            Photo? photo = await service.GetPhotoAsync(
                userId: userId,
                size: size,
                resizeCondition: photoResizeCondition,
                resizeQuality: photoResizeQuality);
            var name = await service.GetNameAsync(
                userId: userId,
                silentlyFail: silentlyFailNames);
            return new Sigma.BoostApp.Contracts.UserInfo{
                UserId = userId, 
                DisplayName = name.ToString(),
                Image = new Sigma.BoostApp.Contracts.ProfileImage
                {
                    Data = photo?.Base64,
                    DataType = photo?.MimeType
                }
            };
        }
    }
}
