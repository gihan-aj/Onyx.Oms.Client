using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Linq;

namespace Onyx.Oms.Client.Desktop.Features.Roles;

public partial class PermissionTreeItemViewModel : ObservableObject
{
    private PermissionTreeItemViewModel? _parent;

    public PermissionTreeItemViewModel(PermissionTreeItemViewModel? parent = null)
    {
        _parent = parent;
        Children = new ObservableCollection<PermissionTreeItemViewModel>();
        Children.CollectionChanged += (s, e) =>
        {
            if (e.NewItems != null)
            {
                foreach (PermissionTreeItemViewModel item in e.NewItems)
                {
                    item._parent = this;
                }
            }
        };
    }

    private string _name = string.Empty;
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    private string? _value;
    public string? Value
    {
        get => _value;
        set => SetProperty(ref _value, value);
    }

    private bool _isReadOnly;
    public bool IsReadOnly
    {
        get => _isReadOnly;
        set
        {
            if (SetProperty(ref _isReadOnly, value))
            {
                OnPropertyChanged(nameof(IsNotReadOnly));
            }
        }

    }

    public bool IsNotReadOnly => !IsReadOnly;

    private bool? _isChecked = false;
    public bool? IsChecked
    {
        get => _isChecked;
        set
        {
            if (SetProperty(ref _isChecked, value))
            {
                UpdateChildren(value);
                _parent?.UpdateParent();
            }
        }
    }

    public ObservableCollection<PermissionTreeItemViewModel> Children { get; }

    // When a parent is checked/unchecked, apply to all children (tunneling)
    private void UpdateChildren(bool? isChecked)
    {
        if (isChecked == null) return; // Don't tunnel an indeterminate state down

        foreach (var child in Children)
        {
            // Set underlying field and raise event directly to avoid recursive loops
            if (child._isChecked != isChecked)
            {
                child._isChecked = isChecked;
                child.OnPropertyChanged(nameof(IsChecked));
                child.UpdateChildren(isChecked); // Recursively update children
            }
        }
    }

    // When a child changes, parent recalculates state (bubbling)
    public void UpdateParent()
    {
        if (Children.Count == 0) return;

        bool allChecked = Children.All(c => c.IsChecked == true);
        bool allUnchecked = Children.All(c => c.IsChecked == false);

        bool? newState = null; // Indeterminate
        if (allChecked) newState = true;
        else if (allUnchecked) newState = false;

        if (_isChecked != newState)
        {
            _isChecked = newState;
            OnPropertyChanged(nameof(IsChecked));
            _parent?.UpdateParent(); // Bubble up again
        }
    }
}
