namespace ProVoiceLedger.Views;

public partial class SessionDetailPage : ContentPage
{
    public SessionDetailPage()
    {
        InitializeComponent();
    }

    private void OnPlayClicked(object sender, EventArgs e)
    {
        // TODO: Audio playback logic
    }

    private void OnRenameClicked(object sender, EventArgs e)
    {
        // TODO: Prompt for rename and update SQLite
    }

    private void OnSendClicked(object sender, EventArgs e)
    {
        // TODO: Invoke secure file share / email
    }
}
