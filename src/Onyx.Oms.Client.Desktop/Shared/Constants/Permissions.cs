namespace Onyx.Oms.Client.Desktop.Shared.Constants;
public static class Permissions
{
    public static class Platform
    {
        public const string ManageTenants = "system:tenants:manage";
        public const string ImpersonateTenant = "system:tenants:impersonate";
        // Add things like billing, global analytics, etc. here
    }

    public static class Tenants
    {
        public const string View = "tenant:tenants:view";
        public const string Create = "tenant:tenants:create";
        public const string Edit = "tenant:tenants:edit";
    }

    public static class Users
    {
        public const string View = "tenant:users:view";
        public const string Create = "tenant:users:create";
        public const string Edit = "tenant:users:edit";
        public const string Activate = "tenant:users:activate";
        public const string Deactivate = "tenant:users:deactivate";
        public const string Delete = "tenant:users:delete";
    }

    public static class Roles
    {
        public const string View = "tenant:roles:view";
        public const string Create = "tenant:roles:create";
        public const string Edit = "tenant:roles:edit";
        public const string Activate = "tenant:roles:activate";
        public const string Deactivate = "tenant:roles:deactivate";
        public const string Delete = "tenant:roles:delete";
    }

    public static class ProductCategories
    {
        public const string View = "tenant:productcategories:view";
        public const string Create = "tenant:productcategories:create";
        public const string Edit = "tenant:productcategories:edit";
        public const string Activate = "tenant:productcategories:activate";
        public const string Deactivate = "tenant:productcategories:deactivate";
        public const string Delete = "tenant:productcategories:delete";
    }

    public static class Products
    {
        public const string View = "tenant:products:view";
        public const string Create = "tenant:products:create";
        public const string Edit = "tenant:products:edit";
        public const string Activate = "tenant:products:activate";
        public const string Deactivate = "tenant:products:deactivate";
        public const string Delete = "tenant:products:delete";
    }

    public static class FulfillmentTasks
    {
        public const string View = "tenant:fulfillmenttasks:view";
        public const string Create = "tenant:fulfillmenttasks:create";
        public const string Edit = "tenant:fulfillmenttasks:edit";
        public const string Activate = "tenant:fulfillmenttasks:activate";
        public const string Deactivate = "tenant:fulfillmenttasks:deactivate";
        public const string Delete = "tenant:fulfillmenttasks:delete";
    }

    public static class Couriers
    {
        public const string View = "tenant:couriers:view";
        public const string Create = "tenant:couriers:create";
        public const string Edit = "tenant:couriers:edit";
        public const string Activate = "tenant:couriers:activate";
        public const string Deactivate = "tenant:couriers:deactivate";
        public const string Delete = "tenant:couriers:delete";
    }

    public static class Customers
    {
        public const string View = "tenant:customers:view";
        public const string Create = "tenant:customers:create";
        public const string Edit = "tenant:customers:edit";
        public const string Activate = "tenant:customers:activate";
        public const string Deactivate = "tenant:customers:deactivate";
        public const string Delete = "tenant:customers:delete";
    }

    public static class Reports
    {
        public const string MonthlyFinancialsView = "tenant:reports:monthlyfinancials";
    }

    public static class AppSequences
    {
        public const string View = "tenant:appsequences:view";
        public const string Edit = "tenant:appsequences:edit";
    }

    public static class Orders
    {
        public const string View = "tenant:orders:view";
        public const string Create = "tenant:orders:create";
        public const string Edit = "tenant:orders:edit";
        public const string Delete = "tenant:orders:delete";
    }

    public static class Expenses
    {
        public const string View = "tenant:expenses:view";
        public const string Create = "tenant:expenses:create";
        public const string Edit = "tenant:expenses:edit";
        public const string Delete = "tenant:expenses:delete";
    }

    public static class PaymentMethods
    {
        public const string View = "tenant:paymentmethods:view";
        public const string Edit = "tenant:paymentmethods:edit";
        public const string Activate = "tenant:paymentmethods:activate";
        public const string Deactivate = "tenant:paymentmethods:deactivate";
    }
}
