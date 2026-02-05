using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Frakture_Tweaks
{
    public class FixItem : INotifyPropertyChanged
    {
        private string _name = "";
        private string _description = "";
        private string _category = "";
        private bool _isSelected;
        private bool _isExecuting;

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        public string Description
        {
            get => _description;
            set { _description = value; OnPropertyChanged(); }
        }

        public string Category
        {
            get => _category;
            set { _category = value; OnPropertyChanged(); }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(); }
        }

        public bool IsExecuting
        {
            get => _isExecuting;
            set { _isExecuting = value; OnPropertyChanged(); }
        }

        public bool RequiresLog { get; set; } = true;

        public Func<LogWindow, Task>? Action { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

