namespace DotnetNiger.UI.Services.Api;

public static class ApiEndpoints
{
    public const string Events = "api/events";
    public const string Posts = "api/posts";
    public const string Resources = "api/resources";
    public const string Projects = "api/projects";
    public const string Partners = "api/partners";
    public const string Members = "api/members";
    public const string MembersTeam = "api/members/team";
    public const string Search = "api/search";
    public const string Contact = "api/contact";
    public const string Notifications = "api/notification";
    public const string Newsletters = "api/newsletter";
    public const string Upload = "api/upload";
    public const string UploadBase64 = "api/upload/base64";
    public const string Profile = "api/profile";
    public const string SocialLinks = "api/profile/social-links";
    public const string Certificates = "api/certificates";
    public const string Comments = "api/comments";
    public const string Categories = "api/categories";
    public const string Tags = "api/tags";
    public const string Stats = "api/stats";
    public const string UserInfo = "api/auth/userinfo";

    public const string AdminEvents = "api/events";
    public const string AdminPosts = "api/posts";
    public const string AdminComments = "api/comments";
    public const string AdminSettings = "api/admin/settings";
    public const string AdminCertificates = "api/certificates";
    public const string AdminUsers = "api/admin/users";
    public const string CommunityAdminUsers = "api/admin/users";
    public const string AdminUserRoles = "api/admin/users/{0}/roles";
    public const string AdminUserRole = "api/admin/users/{0}/roles/{1}";
    public const string AdminUserTeam = "api/admin/users/{0}/team";

    public static class Auth
    {
        public const string Token = "api/auth/login";
        public const string Refresh = "api/auth/refresh";
        public const string Register = "api/auth/register";
        public const string Logout = "api/auth/logout";
        public const string ForgotPassword = "api/auth/forgot-password";
        public const string ResetPassword = "api/auth/reset-password";
        public const string ResendCode = "api/auth/resend-code";
        public const string ConfirmEmail = "api/auth/confirm-email";
        public const string RequestEmailVerification = "api/auth/request-email-verification";
        public const string VerifyEmail = "api/auth/verify-email";
    }

    public static class Admin
    {
        public const string Users = "api/admin/users";
        public const string UserRoles = "api/admin/users/{0}/roles";
        public const string UserRole = "api/admin/users/{0}/roles/{1}";
        public const string Stats = "api/admin/stats";
        public const string Settings = "api/admin/settings";
    }
}
