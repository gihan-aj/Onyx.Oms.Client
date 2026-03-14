using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;

namespace Onyx.Oms.Client.Desktop.Features.Products
{
    public partial class FormTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? TextTemplate {  get; set; }
        public DataTemplate? ComboBoxTemplate {  get; set; }
        public DataTemplate? ToggleTemplate {  get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            if(item is SpecFieldViewItem vi)
            {
                return vi.Type switch
                {
                    SpecType.Select => ComboBoxTemplate!,
                    SpecType.Toggle => ToggleTemplate!,
                    _ => TextTemplate!
                };
            }

            return TextTemplate!;
        }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            return SelectTemplateCore(item);
        }
    }

    public partial class SpecFieldViewItem : ObservableObject
    {
        public string Key { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public SpecType Type { get; set; }
        public bool IsRequired { get; set; }
        public ObservableCollection<string> Options { get; set; } = new();

        // User's input
        private string _value = string.Empty;
        public string Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }
    }
}
