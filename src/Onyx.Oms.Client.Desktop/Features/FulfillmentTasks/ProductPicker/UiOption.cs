using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Onyx.Oms.Client.Desktop.Features.FulfillmentTasks.ProductPicker
{
    public partial class UiOption : ObservableObject
    {
        public string Name { get; }
        public ObservableCollection<UiOptionValue> Values { get; }
        public UiOptionValue? SelectedValue => Values.FirstOrDefault(v => v.IsSelected);

        public UiOption(string name, IEnumerable<string> values)
        {
            Name = name;
            Values = new ObservableCollection<UiOptionValue>(values.Select(v => new UiOptionValue(v)));

            foreach(var val in Values)
            {
                val.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(UiOptionValue.IsSelected) && val.IsSelected)
                    {
                        foreach (var other in Values.Where(v => v != val))
                        {
                            other.IsSelected = false;
                        }
                    }
                };
            }
        }
    }
}
