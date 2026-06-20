namespace Template.API.Authorization;

public static class Permissions
{
    public static class Employees
    {
        public const string View   = "Employees.View";
        public const string Create = "Employees.Create";
        public const string Update = "Employees.Update";
        public const string Delete = "Employees.Delete";
    }

    public static class HR
    {
        public const string View   = "HR.View";
        public const string Create = "HR.Create";
        public const string Update = "HR.Update";
        public const string Delete = "HR.Delete";
    }

    public static class Roles
    {
        public const string View   = "Roles.View";
        public const string Create = "Roles.Create";
        public const string Update = "Roles.Update";
        public const string Delete = "Roles.Delete";
    }
}
