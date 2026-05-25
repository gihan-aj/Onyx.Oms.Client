namespace Onyx.Oms.Client.Desktop.Features.Settings
{
    public class PaymentMethodGridItem : PaymentMethodConfigDto
    {
        public bool CanEdit { get; set; }
        public bool CanActivate { get; set; }
        public bool CanDeactivate { get; set; }
    }

    public static class PaymentMethodMappingExtensions
    {
        public static PaymentMethodGridItem ToGridItem(
            this PaymentMethodConfigDto dto,
            bool canEdit,
            bool canActivate,
            bool canDeactivate)
        {
            return new PaymentMethodGridItem
            {
                Id = dto.Id,
                Type = dto.Type,
                DisplayName = dto.DisplayName,
                FeeRate = dto.FeeRate,
                IsActive = dto.IsActive,
                // Map UI/Permission properties
                CanEdit = canEdit,
                CanActivate = canActivate && !dto.IsActive, // Example logic: can only activate if inactive
                CanDeactivate = canDeactivate && dto.IsActive,
            };
        }
    }   
}
