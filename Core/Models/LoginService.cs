// File: Core/Services/LoginService.cs
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using ProVoiceLedger.Core.Models;

namespace ProVoiceLedger.Core.Services
{
    public class LoginService
    {
        private readonly HttpClient _httpClient;
        private const string LoginEndpoint = "https://localhost:7290/api/auth/login";

        public LoginService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<LoginResponse> AttemptLoginAsync(LoginRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(LoginEndpoint, request);

                if (!response.IsSuccessStatusCode)
                {
                    return new LoginResponse
                    {
                        Success = false,
                        Message = $"Server Error: {response.StatusCode}"
                    };
                }

                var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                if (result?.Success == true && !string.IsNullOrEmpty(result.Token))
                {
                    await SecureStorage.SetAsync("auth_token", result.Token);
                }

                return result ?? new LoginResponse
                {
                    Success = false,
                    Message = "Unexpected server response"
                };
            }
            catch (Exception ex)
            {
                return new LoginResponse
                {
                    Success = false,
                    Message = $"Network Error: {ex.Message}"
                };
            }
        }

        public async Task<string?> TryGetStoredTokenAsync()
        {
            try
            {
                return await SecureStorage.GetAsync("auth_token");
            }
            catch
            {
                return null;
            }
        }

        public Task LogoutAsync()
        {
            SecureStorage.Remove("auth_token");
            return Task.CompletedTask;
        }
    }
}
