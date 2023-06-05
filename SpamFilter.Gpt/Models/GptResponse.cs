namespace SpamFilter.GptApi.Models;

using System.Text.Json.Serialization;

public class GptResponse
{
    public string Id { get; set; }
    public string Object { get; set; }
    public string Created { get; set; }
    public string Model { get; set; }
    public List<Choice> Choices { get; set; }
}
