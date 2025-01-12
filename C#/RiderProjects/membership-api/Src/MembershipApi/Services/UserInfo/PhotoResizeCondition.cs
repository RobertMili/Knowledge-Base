namespace MembershipAPI.Services.UserInfo
{
    public enum PhotoResizeCondition
    {
        /// <summary>
        /// If a photo exist in the preferred size or any of the fallback sizes it will be returned as is;
        /// Other sizes will be resized to the preferred size.
        /// </summary>
        ResizeIfDefault = 0,

        /// <summary>
        /// If a photo exist (any size) it will always be resized to the preferred size.
        /// </summary>
        ResizeAlways = 1,

        /// <summary>
        /// If a photo exist (any size) it will be returned as is.
        /// </summary>
        NoResizeAcceptAny = 2,

        /// <summary>
        /// If a photo exist in the preferred size or any of the fallback sizes it will be returned as is;
        /// Otherwise no photo will be returned.
        /// </summary>
        NoResizeNoDefault = 3,
    }
}
