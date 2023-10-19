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
    public class SizeInfoSelectorViewModel : BaseViewModel 
    {
        public ObservableCollection<SizeInfo> Configurations { get; set; }
        public SizeInfo NewSizeEntry { get; set; }

        public ICommand AddNewEntryCommand { get; set; }

        public SizeInfoSelectorViewModel()
        {
            Configurations = new ObservableCollection<SizeInfo>();
            NewSizeEntry = new SizeInfo();
            AddNewEntryCommand = new Command(AddNewEntry);

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
