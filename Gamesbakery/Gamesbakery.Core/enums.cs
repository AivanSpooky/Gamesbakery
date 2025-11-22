namespace Gamesbakery.Core
{
    public enum UserRole
    {
        Guest,
        User,
        Seller,
        Admin
    }
    public static class RoleMethods
    {
        public static string UserRoleToStr(UserRole role)
        {
            return role switch
            {
                UserRole.Guest => "Guest",
                UserRole.User => "User",
                UserRole.Seller => "Seller",
                UserRole.Admin => "Admin",
                _ => "Unknown"
            };
        }
    }
    public enum GiftSource
    {
        Sent,
        Received,
        All
    }
}
