using CommunityToolkit.Mvvm.Input;
using CompanyManager.RestMVVMApp.Models;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using System.Windows.Input;
using CompanyManager.RestMVVMApp.Views;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia;

namespace CompanyManager.RestMVVMApp.ViewModels
{
    public partial class CompanyViewModel : ViewModelBase
    {
        #region fields
        private Company model = new();
        #endregion fields

        #region properties
        public Action? CloseAction { get; set; }
        public Company Model
        {
            get => model;
            set => model = value ?? new();
        }
        public string Name
        {
            get => Model.Name;
            set => Model.Name = value;
        }
        public string? Address
        {
            get => Model.Address;
            set => Model.Address = value;
        }
        public string? Description
        {
            get => Model.Description;
            set => Model.Description = value;
        }
        #endregion properties

        #region commands
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        #endregion commands
        public CompanyViewModel()
        {
            CancelCommand = new RelayCommand(() => Close());
            SaveCommand = new RelayCommand(() => Save());
        }
        private void Close()
        {
            CloseAction?.Invoke();
        }
        private async void Save()
        {
            bool canClose = false;
            using var httpClient = new HttpClient { BaseAddress = new Uri(API_BASE_URL) };

            try
            {
                if (Model.Id == 0)
                {
                    var response = httpClient.PostAsync($"Companies", new StringContent(JsonSerializer.Serialize(Model), Encoding.UTF8, "application/json")).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        canClose = true;
                    }
                    else
                    {
                        var messageDialog = new MessageDialog("Fehler", "Beim Speichern ist ein Fehler aufgetreten", MessageType.Error);
                        var mainWindow = (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;

                        await messageDialog.ShowDialog(mainWindow!);
                        Console.WriteLine($"Fehler beim Abrufen der Companies. Status: {response.StatusCode}");
                    }
                }
                else
                {
                    var response = httpClient.PutAsync($"Companies/{Model.Id}", new StringContent(JsonSerializer.Serialize(Model), Encoding.UTF8, "application/json")).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        canClose = true;
                    }
                    else
                    {
                        Console.WriteLine($"Fehler beim Abrufen der Companies. Status: {response.StatusCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            if (canClose)
            {
                CloseAction?.Invoke();
            }
        }
    }
}
