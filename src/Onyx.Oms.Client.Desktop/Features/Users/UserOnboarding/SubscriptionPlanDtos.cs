using System;

namespace Onyx.Oms.Client.Desktop.Features.Users.UserOnboarding
{
    public class SubscriptionPlanDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public MoneyDto MonthleyPrice { get; set; } = null!;
        public int MaxUsersAllowed { get; set; }
        public int MaxOrdersAllowed { get; set; }
        public int TrialPeriodInDays { get; set; }
        public bool IsActive { get; set; }
    }

    public class MoneyDto
    {
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "LKR";
    }
}
