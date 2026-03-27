using System;

namespace Onyx.Oms.Client.Desktop.Features.Users
{
    public record RegisterUserRequest
    {
        public RegisterUserDetailsDto UserDetails { get; init; } = null!;
        public RegisterComapnyDetailsDto CompanyDetails { get; init; } = null!;
        public RegisterSubscriptionPlanDetailsDto SubscriptionDetails { get; init; } = null!;
    }

    public record RegisterUserDetailsDto
    {
        public string FirstName { get; init; } = string.Empty;
        public string LastName { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;
        public string ConfirmPassword { get; init; } = string.Empty;
    }

    public record RegisterComapnyDetailsDto
    {
        public string CompanyName { get; init; } = string.Empty;
        public string ContactEmail { get; init; } = string.Empty;
    }

    public class RegisterSubscriptionPlanDetailsDto
    {
        public Guid SubscriptionId { get; init; }
    }
}
