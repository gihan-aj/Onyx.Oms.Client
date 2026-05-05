using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Onyx.Oms.Client.Desktop.Features.Orders.Edit
{
    public sealed partial class AddPaymentDialog : ContentDialog
    {
        public decimal PaymentAmount { get; set; }
        public string BaseCurrency { get; set; } = "LKR";
        public DateTimeOffset PaymentDate { get; set; } = DateTimeOffset.Now;
        public TimeSpan PaymentTime { get; set; } = DateTime.Now.TimeOfDay;
        public PaymentMethod SelectedMethod { get; set; } = PaymentMethod.BankTransfer;
        public string? ReferenceNumber { get; set; }
        public List<PaymentMethod> PaymentMethods { get; set; } = new(); 

        public AddPaymentDialog(decimal dueBalance, string baseCurrency, bool isCashOnDelivery)
        {
            InitializeComponent();
            
            PaymentAmount = dueBalance;
            BaseCurrency = baseCurrency;
            if (isCashOnDelivery)
                SelectedMethod = PaymentMethod.CashOnDelivery;

            foreach( PaymentMethod paymentMethod in Enum.GetValues(typeof(PaymentMethod)))
            {
                PaymentMethods.Add(paymentMethod);
            }
        }
    }
}
