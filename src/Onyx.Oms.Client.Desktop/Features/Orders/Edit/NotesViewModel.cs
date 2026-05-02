using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace Onyx.Oms.Client.Desktop.Features.Orders.Edit
{
    public partial class NotesViewModel : ObservableObject
    {
        private string? _originalNotes;

        private bool _isEditing = false;
        public bool IsEditing
        {
            get => _isEditing;
            set
            {
                if (SetProperty(ref _isEditing, value))
                {
                    OnPropertyChanged(nameof(IsReadonly));
                }
            }
        }

        public bool IsReadonly => !IsEditing;

        private string? _notes;
        public string? Notes
        {
            get => _notes;
            set
            {
                if(SetProperty(ref _notes, value))
                {
                    OnPropertyChanged(nameof(HasNotes));
                }
            }
        }

        public bool HasNotes => !string.IsNullOrWhiteSpace(Notes);

        public IRelayCommand BeginEditCommand { get; }
        public IRelayCommand CancelEditCommand { get; }
        public NotesViewModel(OrderDetailsDto order)
        {
            _originalNotes = order.Notes;

            Notes = order.Notes;

            BeginEditCommand = new RelayCommand(BeginEdit);
            CancelEditCommand = new RelayCommand(CancelEdit);
        }

        private void BeginEdit()
        {
            IsEditing = true;
        }

        private void CancelEdit()
        {
            Notes = _originalNotes;
            IsEditing = false;
        }

        public UpdateOrderNotesCommand GetUpdateDto()
        {
            return new UpdateOrderNotesCommand(Notes);
        }
    }
}
