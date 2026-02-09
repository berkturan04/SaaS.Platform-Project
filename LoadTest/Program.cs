using System.Net.Http.Json;

var client = new HttpClient();

for (int i = 1; i <= 101; i++)
{
    var request = new HttpRequestMessage(
        HttpMethod.Post,
        "http://localhost:5002/api/resources"
    );

    request.Headers.Add(
        "X-Tenant-Id",
        "11111111-1111-1111-1111-111111111111"
    );

    request.Content = JsonContent.Create(new
    {
        name = "test"
    });

    var response = await client.SendAsync(request);

    Console.WriteLine($"{i}. request → {(int)response.StatusCode}");
}
