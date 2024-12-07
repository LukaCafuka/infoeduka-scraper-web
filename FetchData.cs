using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json;

namespace InfoedukaScraper;
public class FetchData
{
    private readonly string _url = "https://student.racunarstvo.hr/digitalnareferada/api/student/predmeti/?dodatno=materijali";
    private readonly HttpClient _httpClient;
    
    public FetchData(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> GetJson(string? cookie)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, _url);
        
        if (!string.IsNullOrEmpty(cookie))
        {
            request.Headers.Add("Cookie", cookie);
        }
        
        
        
        var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        
        if (!response.IsSuccessStatusCode)
        {
            Debug.WriteLine($"Failed to fetch data. Status code: {response.StatusCode}");
            throw new HttpRequestException($"Request failed with status code {response.StatusCode}");
        }
        
        using var stream = await response.Content.ReadAsStreamAsync();
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }
    
    /// <summary>
    /// Extracts a list of study names from the JSON data.
    /// </summary>
    public List<string> ExtractStudyNames(string json)
    {
        var studyList = new List<string>();

        var bytes = Encoding.UTF8.GetBytes(json);
        var reader = new Utf8JsonReader(bytes);

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.PropertyName && reader.GetString() == "predmet")
            {
                reader.Read(); // Move to the value of "predmet"
                studyList.Add(reader.GetString() ?? "Unknown Study");
            }
        }

        return studyList;
    }


    /// <summary>
    /// Extracts all links from the JSON data for the given study.
    /// </summary>
    public List<string> ExtractLinks(string json)
    {
        var linksList = new List<string>();

        var bytes = Encoding.UTF8.GetBytes(json);
        var reader = new Utf8JsonReader(bytes);

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.PropertyName && reader.GetString() == "link")
            {
                reader.Read(); // Move to the value of "link"
                linksList.Add(reader.GetString() ?? "Unknown Link");
            }
        }

        return linksList;
    }
    
    private async Task DownloadFileAsync(string url, string directory)
    {
        try
        {
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return;

            var fileName = Path.GetFileName(url) ?? Guid.NewGuid().ToString();
            var filePath = Path.Combine(directory, fileName);

            await using var fileStream = new FileStream(filePath, FileMode.Create);
            await response.Content.CopyToAsync(fileStream);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to download file from {url}: {ex.Message}");
        }
    }

}