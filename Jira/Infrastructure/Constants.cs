namespace Jira.Infrastructure
{
    public static class Constants
    {
        public static string ProjectNotFoundError => "Такого Project не существует";

        public static string CommentNotFoundError => "Такого Comment не существует";

        public static string BoardNotFoundError => "Такого Board не существует";

        public static string TaskItemNotFoundError => "Такого Task не существует";

        public static string ColumnNotFoundError => "Такого Column не существует";

        public static string UserNotFoundError => "Такого User не существует";

        public static string ProjectMemberNotFoundError => "Такого ProjectMember не существует";

        public static string NoProblem => "Ok";

        public static string[] OwnerOnly => ["Owner"];

        public static string[] OwnerAndManagers => ["Owner", "Manager"];

        public static string[] OwnerAndUser => ["Owner", "User"];

        public static string[] AllMembers => ["Owner", "User", "Manager"];

        public static object ErrorBuilder(string error, string? subject = null) => new { Error = subject != null? error + $": {subject}" : error};
    }
}
