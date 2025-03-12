using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using CompanyManager.RestMVVMApp.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace CompanyManager.RestMVVMApp.ViewModels
{
    public partial class CompaniesViewModel : ViewModelBase
    {
        #region fields
        private string _filter = string.Empty;
        private Models.Company? selectedItem;
        private readonly List<Models.Company> _companies = [];
        #endregion fields

        #region properties
        public string Filter
        {
            get
            {
                return _filter;
            }
            set
            {
                _filter = value;
                ApplyFilter(value);
                OnPropertyChanged();
            }
        }
        public Models.Company? SelectedItem
        {
            get => selectedItem;
            set
            {
                selectedItem = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<Models.Company> Companies { get; } = [];
        #endregion properties

        public CompaniesViewModel()
        {
            _ = LoadCompaniesAsync();
        }

        #region commands
        [RelayCommand]
        public async Task LoadItems()
        {
            await LoadCompaniesAsync();
        }
        [RelayCommand]
        public async Task AddItem()
        {
            var companyWindow = new CompanyWindow();
            var viewModel = new CompanyViewModel { CloseAction = companyWindow.Close };

            companyWindow.DataContext = viewModel;
            // Aktuelles Hauptfenster als Parent setzen
            var mainWindow = (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            if (mainWindow != null)
            {
                await companyWindow.ShowDialog(mainWindow);
                _ = LoadCompaniesAsync();
            }
        }
        [RelayCommand]
        public async Task EditItem(Models.Company company)
        {
            var companyWindow = new CompanyWindow();
            var viewModel = new CompanyViewModel { Model = company, CloseAction = companyWindow.Close };

            companyWindow.DataContext = viewModel;
            // Aktuelles Hauptfenster als Parent setzen
            var mainWindow = (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;

            if (mainWindow != null)
            {
                await companyWindow.ShowDialog(mainWindow);
                _ = LoadCompaniesAsync();
            }
        }
        [RelayCommand]
        public async Task DeleteItem(Models.Company company)
        {
            var messageDialog = new MessageDialog("Delete", $"Wollen Sie die Firma '{company.Name}' löschen?", MessageType.Question);
            var mainWindow = (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;

            // Aktuelles Hauptfenster als Parent setzen
            await messageDialog.ShowDialog(mainWindow!);

            if (messageDialog.Result == MessageResult.Yes)
            {
                using var httpClient = new HttpClient { BaseAddress = new Uri(API_BASE_URL) };


                var response = await httpClient.DeleteAsync($"companies/{company.Id}");

                if (response.IsSuccessStatusCode == false)
                {
                    messageDialog = new MessageDialog("Error", "Beim Löschen ist ein Fehler aufgetreten!", MessageType.Error);
                    await messageDialog.ShowDialog(mainWindow!);
                }
                else
                {
                    _ = LoadCompaniesAsync();
                }
            }
        }
        #endregion commands

        private async void ApplyFilter(string filter)
        {
            // UI-Update sicherstellen
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                var selectedItem = SelectedItem;

                Companies.Clear();
                foreach (var company in _companies)
                {
                    if (company.ToString().Contains(filter, StringComparison.OrdinalIgnoreCase))
                    {
                        Companies.Add(company);
                    }
                }
                if (selectedItem != null)
                {
                    SelectedItem = Companies.FirstOrDefault(e => e.Id == selectedItem.Id);
                }
            });
        }
        private async Task LoadCompaniesAsync()
        {
            try
            {
                using var httpClient = new HttpClient { BaseAddress = new Uri(API_BASE_URL) };
                var response = await httpClient.GetStringAsync("companies");
                var companies = JsonSerializer.Deserialize<List<Models.Company>>(response, _jsonSerializerOptions);

                if (companies != null)
                {
                    _companies.Clear();
                    foreach (var company in companies)
                    {
                        _companies.Add(company);
                    }
                    ApplyFilter(Filter);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading companies: {ex.Message}");
            }
        }
    }
}
