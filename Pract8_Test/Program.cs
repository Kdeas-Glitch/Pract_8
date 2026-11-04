using System.Net.Http.Headers;
using System.Net.Http.Json;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var client = new HttpClient();

        var response = await client.PostAsJsonAsync(
            "https://localhost:7006/api/v1/auth/login",
            new
            {
                //Login = "string",
                //Password = "string"
                Login = "admin123",
                Password = "P@s$w0rd"
            }
        );
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine("Ошибка аутентификации");
            return;
        }
        var result = await response.Content
        .ReadFromJsonAsync<AuthResponse>();
        string token = result!.access_token;


        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response1 = await client.DeleteAsync(
                "https://localhost:7006/api/v1/tasks/all"

           );
        Console.WriteLine(response1.StatusCode);

        if (response1.IsSuccessStatusCode)
        {
            Console.WriteLine(
            await response1.Content.ReadAsStringAsync()
            );
        }

    }
}

public record AuthResponse(string access_token);

