using System.Net.Http;
using System.Text.Json;
using System.Text;

namespace YourApp.Pages
{
    public partial class LoginPage : ContentPage
    {
        public LoginPage()
        {
            InitializeComponent(); // 🔥 This activates the visual tree from XAML
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            ErrorLabel.IsVisible = false;
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;
            LoginButton.IsEnabled = false;

            var username = UsernameEntry.Text?.Trim();
            var password = PasswordEntry.Text?.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ErrorLabel.Text = "Username and password are required.";
                ErrorLabel.IsVisible = true;
                ResetUI();
                return;
            }

            try
            {
                var response = await VerifyLoginAsync(username, password);

                if (response.status == "success")
                {
                    await DisplayAlert("Login Success", $"Welcome {response.displayName}!", "Continue");
                    // await Navigation.PushAsync(new HomePage());
                }
                else
                {
                    ErrorLabel.Text = response.message ?? "Invalid login.";
                    ErrorLabel.IsVisible = true;
                }
            }
            catch (Exception)
            {
                ErrorLabel.Text = "Server error. Try again later.";
                ErrorLabel.IsVisible = true;
            }

            ResetUI();
        }

        private void ResetUI()
        {
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
            LoginButton.IsEnabled = true;
        }

        private async Task<LoginResponse> VerifyLoginAsync(string username, string password)
        {
            var payload = new { username, password };
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var client = new HttpClient();
            var response = await client.PostAsync("https://yourdomain.com/api/login", content);
            var resultJson = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<LoginResponse>(resultJson);
        }

        public class LoginResponse
        {
            public string status { get; set; }
            public string displayName { get; set; }
            public int userId { get; set; }
            public string message { get; set; }
        }
    }
}
