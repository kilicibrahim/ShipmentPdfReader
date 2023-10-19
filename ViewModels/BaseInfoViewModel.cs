using ShipmentPdfReader.Models;
using ShipmentPdfReader.ViewModels;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Input;

namespace ShipmentPdfReader.ViewModels
{
    public abstract class BaseInfoViewModel<TModel> : BaseViewModel
    where TModel : ObservableObject, new()
    {
        protected ObservableCollection<TModel> _configurations;
        protected TModel _newEntry;

        public ICommand AddNewEntryCommand
        {
            get;
        }
        public ICommand SaveCommand
        {
            get;
        }
        public ICommand DeleteCommand
        {
            get;
        }

        public ObservableCollection<TModel> Configurations
        {
            get => _configurations;
            set => SetProperty(ref _configurations, value);
        }

        public TModel NewEntry
        {
            get => _newEntry;
            set
            {
                if (_newEntry != value)
                {
                    if (_newEntry != null)
                    {
                        _newEntry.PropertyChanged -= NewEntry_PropertyChanged;
                    }

                    SetProperty(ref _newEntry, value);

                    if (_newEntry != null)
                    {
                        _newEntry.PropertyChanged += NewEntry_PropertyChanged;
                    }

                    OnPropertyChanged(nameof(CanAddEntry));
                }
            }
        }

        public bool CanAddEntry => IsValidEntry(NewEntry);
        public bool CanSave => Configurations.All(IsValidEntry);

        protected readonly Func<TModel> _createModelInstance;

        protected BaseInfoViewModel(IEnumerable<TModel> initialData, Func<TModel> createModelInstance)
        {
            _configurations = new ObservableCollection<TModel>(initialData.Where(item => item != null));
            _createModelInstance = createModelInstance ?? throw new ArgumentNullException(nameof(createModelInstance));
            NewEntry = _createModelInstance();  // Ensure NewEntry is initialized
            AddNewEntryCommand = new Command(AddNewEntry);
            SaveCommand = new Command(SaveConfigurationsToFile);
            DeleteCommand = new Command<TModel>(DeleteEntry);
            NewEntry.PropertyChanged += NewEntry_PropertyChanged;
            Configurations.CollectionChanged += Configurations_CollectionChanged;
        }

        private void NewEntry_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(CanAddEntry));
        }
        private void Configurations_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(CanSave));
        }
        private void DeleteEntry(TModel entry)
        {
            Configurations.Remove(entry);
        }

        protected abstract void AddNewEntry();

        protected abstract void SaveConfigurationsToFile();

        protected abstract bool IsValidEntry(TModel entry);

    }
}