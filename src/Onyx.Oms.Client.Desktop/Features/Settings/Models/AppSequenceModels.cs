using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using System;

namespace Onyx.Oms.Client.Desktop.Features.Settings.Models;

public partial class AppSequenceItem : ObservableObject
{
    private string _id = string.Empty;
    public string Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }
    
    private string _displayName = string.Empty;
    public string DisplayName
    {
        get => _displayName;
        set => SetProperty(ref _displayName, value);
    }

    private double _currentValue;
    public double CurrentValue
    {
        get => _currentValue;
        set => SetProperty(ref _currentValue, value);
    }
    
    private bool _isSaving;
    public bool IsSaving
    {
        get => _isSaving;
        set => SetProperty(ref _isSaving, value);
    }
    
    private bool _canEdit;
    public bool CanEdit
    {
        get => _canEdit;
        set
        {
            SetProperty(ref _canEdit, value);
            OnPropertyChanged(nameof(EditButtonVisible));
        }
    }

    private bool _isEditing;
    public bool IsEditing
    {
        get => _isEditing;
        set
        {
            if (SetProperty(ref _isEditing, value))
            {
                OnPropertyChanged(nameof(EditButtonVisible));
                OnPropertyChanged(nameof(SaveCancelVisible));
            }
        }
    }

    public Visibility EditButtonVisible => (CanEdit && !IsEditing) ? Visibility.Visible : Visibility.Collapsed;
    public Visibility SaveCancelVisible => IsEditing ? Visibility.Visible : Visibility.Collapsed;

    private double _originalValue;

    public Action? OnCancelEdit { get; set; }

    [RelayCommand]
    private void Edit()
    {
        _originalValue = CurrentValue;
        IsEditing = true;
    }

    [RelayCommand]
    private void CancelEdit()
    {
        CurrentValue = _originalValue;
        IsEditing = false;
        OnCancelEdit?.Invoke();
    }
}
