using ShipmentPdfReader.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ShipmentPdfReader.ViewModels
{
    public class SizeInfoSelectorViewModel : BaseViewModel  // Ensure you have a ViewModel base for common functionalities
    {
        public ObservableCollection<SizeInfo> Configurations { get; set; }
        public SizeInfo NewSizeEntry { get; set; }

        public ICommand AddNewEntryCommand { get; set; }

        public SizeInfoSelectorViewModel()
        {
            // Initialize properties
            Configurations = new ObservableCollection<SizeInfo>();
            NewSizeEntry = new SizeInfo();

            // Initialize commands
            AddNewEntryCommand = new Command(AddNewEntry);

            // Load existing configurations
            LoadConfigurations();
        }

        private void AddNewEntry()
        {
            // Validate and add the new entry to Configurations
        }

        private void LoadConfigurations()
        {
            // Load existing configurations from storage
        }
    }

}
