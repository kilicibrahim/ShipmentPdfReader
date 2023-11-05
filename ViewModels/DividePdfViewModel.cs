using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Maui.Storage;
using ShipmentPdfReader.Services.Pdf;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ShipmentPdfReader.ViewModels
{
    public class DividePdfViewModel : BaseViewModel
    {
        public ICommand SelectPdfCommand
        {
            get;
        }
        public ICommand SelectJsonCommand
        {
            get;
        }
        public ICommand DividePdfCommand
        {
            get;
        }

        private string _selectedPdfFilePath;
        public string SelectedPdfFilePath
        {
            get => _selectedPdfFilePath;
            set => SetProperty(ref _selectedPdfFilePath, value);
        }

        private string _selectedJsonFilePath;
        public string SelectedJsonFilePath
        {
            get => _selectedJsonFilePath;
            set => SetProperty(ref _selectedJsonFilePath, value);
        }

        public DividePdfViewModel()
        {
            SelectPdfCommand = new RelayCommand(async () => await SelectFile(".pdf", "selectedPdfFilePath"));
            SelectJsonCommand = new RelayCommand(async () => await SelectFile(".json", "selectedJsonFilePath"));
            DividePdfCommand = new RelayCommand(async () => await DividePdf(),
                () => !string.IsNullOrWhiteSpace(SelectedPdfFilePath) && !string.IsNullOrWhiteSpace(SelectedJsonFilePath));
        }

        private async Task SelectFile(string fileType, string key)
        {
            try
            {
                var options = new PickOptions
                {
                    PickerTitle = $"Please select a {fileType.ToUpper()} file",
                    FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                    {
                        { DevicePlatform.WinUI, new[] { fileType } },
                        { DevicePlatform.MacCatalyst, new[] { fileType.TrimStart('.') } },
                    }),
                };

                var result = await FilePicker.PickAsync(options);
                if (result != null && !string.IsNullOrWhiteSpace(result.FullPath))
                {
                    if (key == "selectedPdfFilePath")
                    {
                        SelectedPdfFilePath = result.FullPath;
                    }
                    else if (key == "selectedJsonFilePath")
                    {
                        SelectedJsonFilePath = result.FullPath;
                    }
                }
                else
                {
                    WeakReferenceMessenger.Default.Send(new Messages($"{fileType.ToUpper()} file selection was cancelled."));
                }
            }
            catch (Exception ex)
            {
                WeakReferenceMessenger.Default.Send(new Messages($"{fileType.ToUpper()} selection failed: {ex.Message}"));
            }
        }

        private async Task DividePdf()
        {
            if (string.IsNullOrWhiteSpace(SelectedPdfFilePath) || string.IsNullOrWhiteSpace(SelectedJsonFilePath))
            {
                WeakReferenceMessenger.Default.Send(new Messages("Select a PDF and a JSON file to Start Division"));
                return;
            }

            try
            {
                PdfProcessor pdfProcessor = new PdfProcessor(SelectedPdfFilePath);
                await pdfProcessor.DividePdfByUserSelectionAsync(SelectedJsonFilePath);
                WeakReferenceMessenger.Default.Send(new Messages("PDF division completed successfully."));
            }
            catch (Exception ex)
            {
                WeakReferenceMessenger.Default.Send(new Messages($"Failed to divide the PDF: {ex.Message}"));
            }
        }
    }
}
