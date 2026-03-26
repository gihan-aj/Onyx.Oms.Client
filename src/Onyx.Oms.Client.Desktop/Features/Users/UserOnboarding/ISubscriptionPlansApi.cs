using Refit;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Onyx.Oms.Client.Desktop.Features.Users.UserOnboarding
{
    public interface ISubscriptionPlansApi
    {
        [Get("/api/v1/subscription-plans")]
        Task<List<SubscriptionPlanDto>> GetSubscriptionsPlanAsync([Body] GetSubsriptionPlansRequest request);
    }
}
