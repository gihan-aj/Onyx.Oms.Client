using Microsoft.UI.Xaml.Controls;
using System.Text;

namespace Onyx.Oms.Client.Desktop.Features.Customers;

public sealed partial class CustomerDetailsDialog : ContentDialog
{
    public CustomerDto Customer { get; }
    public string FullAddress { get; }

    public CustomerDetailsDialog(CustomerDto customer)
    {
        Customer = customer;
        
        var sb = new StringBuilder();
        if (customer.Address != null)
        {
            if (!string.IsNullOrWhiteSpace(customer.Address.Street)) sb.AppendLine(customer.Address.Street);
            
            var line2 = $"{customer.Address.City}, {customer.Address.State} {customer.Address.PostalCode}".Trim().Trim(',').Trim();
            if (!string.IsNullOrWhiteSpace(line2)) sb.AppendLine(line2);
            
            if (!string.IsNullOrWhiteSpace(customer.Address.Country)) sb.AppendLine(customer.Address.Country);
        }
        else
        {
            sb.Append("No address provided");
        }
        FullAddress = sb.ToString().Trim();

        this.InitializeComponent();
    }
}
