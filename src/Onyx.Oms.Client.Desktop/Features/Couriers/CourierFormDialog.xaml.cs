using Microsoft.UI.Xaml.Controls;
using System;

namespace Onyx.Oms.Client.Desktop.Features.Couriers;

public sealed partial class CourierFormDialog : ContentDialog
{
    public CourierFormDialog(CourierDto? courier = null, bool isReadOnly = false)
    {
        InitializeComponent();
        
        if (courier != null)
        {
            Title = isReadOnly ? "Courier Details" : "Edit Courier";
            NameBox.Text = courier.Name ?? string.Empty;
            ContactPersonBox.Text = courier.ContactPerson ?? string.Empty;
            PrimaryPhoneBox.Text = courier.PrimaryPhone ?? string.Empty;
            SecondaryPhoneBox.Text = courier.SecondaryPhone ?? string.Empty;
            WebsiteUrlBox.Text = courier.WebsiteUrl ?? string.Empty;
            IsActiveBox.IsChecked = courier.IsActive;
        }
        else
        {
            Title = "New Courier";
        }

        if (isReadOnly)
        {
            PrimaryButtonText = string.Empty;
            IsPrimaryButtonEnabled = false;
            NameBox.IsReadOnly = true;
            ContactPersonBox.IsReadOnly = true;
            PrimaryPhoneBox.IsReadOnly = true;
            SecondaryPhoneBox.IsReadOnly = true;
            WebsiteUrlBox.IsReadOnly = true;
            IsActiveBox.IsEnabled = false;
        }
    }

    public CreateCourierDto GetCreateDto()
    {
        return new CreateCourierDto
        {
            Name = NameBox.Text,
            ContactPerson = ContactPersonBox.Text,
            PrimaryPhone = PrimaryPhoneBox.Text,
            SecondaryPhone = SecondaryPhoneBox.Text,
            WebsiteUrl = WebsiteUrlBox.Text,
            IsActive = IsActiveBox.IsChecked ?? false
        };
    }

    public UpdateCourierDto GetUpdateDto(Guid id)
    {
        return new UpdateCourierDto
        {
            Id = id,
            Name = NameBox.Text,
            ContactPerson = ContactPersonBox.Text,
            PrimaryPhone = PrimaryPhoneBox.Text,
            SecondaryPhone = SecondaryPhoneBox.Text,
            WebsiteUrl = WebsiteUrlBox.Text,
            IsActive = IsActiveBox.IsChecked ?? false
        };
    }
}
