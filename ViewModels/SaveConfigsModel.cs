using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace ShipmentPdfReader.ViewModels
{
    public class SaveConfigsModel: BaseViewModel
    {
        private string _sourceDirectory;
        private string _destinationDirectory;
        private string _filePath;

        public string SourceDirectory
        {
            get => _sourceDirectory;
            set => SetProperty(ref _sourceDirectory, value);
        }

        public string DestinationDirectory
        {
            get => _destinationDirectory;
            set => SetProperty(ref _destinationDirectory, value);
        }

        public string FilePath
        {
            get => _filePath;
            set => SetProperty(ref _filePath, value);
        }

    }
}
