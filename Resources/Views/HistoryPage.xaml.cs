using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using ProVoiceLedger.Core.Models;
using ProVoiceLedger.Core.Services;

namespace ProVoiceLedger.Views;

public partial class HistoryPage : ContentPage
{
    public ObservableCollection<Session> Sessions { get; set; } = new();

    public HistoryPage()
    {
        InitializeComponent();
        BindingContext = this;

        LoadSessionsAsync(); // 🎯 Async data hydration
    }

    private async void LoadSessionsAsync()
    {
        if (Application.Current is App app && app.SessionDb != null)
        {
            var allSessions = await app.SessionDb.GetSessionsAsync();
            foreach (var session in allSessions)
            {
                Sessions.Add(session);
            }
        }
    }
}
