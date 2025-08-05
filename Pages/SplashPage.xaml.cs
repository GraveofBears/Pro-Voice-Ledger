using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;

namespace ProVoiceLedger.Pages
{
    public partial class SplashPage : ContentPage
    {
        public SplashPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            await AnimateLogoIntro();

            // 🌟 Optional delay for dramatic effect
            await Task.Delay(900);

            // 🚀 Navigate to LoginPage with stacked transition
            await Navigation.PushAsync(new LoginPage());
        }

        private async Task AnimateLogoIntro()
        {
            if (LogoImage == null) return;

            // 🌀 Start with initial scale and fade
            LogoImage.Opacity = 0;
            LogoImage.Scale = 0.5;

            // 🎬 Smooth fade-in and scale-up animation
            await LogoImage.FadeTo(1, 700, Easing.CubicInOut);
            await LogoImage.ScaleTo(1.0, 700, Easing.CubicInOut);

            // ✨ Optional bounce effect (can be disabled for subtlety)
            await LogoImage.ScaleTo(1.05, 150, Easing.SinOut);
            await LogoImage.ScaleTo(1.0, 150, Easing.SinIn);
        }
    }
}
