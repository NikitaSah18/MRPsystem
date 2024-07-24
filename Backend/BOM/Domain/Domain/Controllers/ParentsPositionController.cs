using BOM.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Domain.Controllers
{
    [ApiController]
    [Microsoft.AspNetCore.Mvc.Route("api/Domain/ParentsPosition")]
    public class ParentsPositionController : ControllerBase
    {
        private readonly string? _dalUrl;
        private readonly HttpClient _parentsPosition;

        public ParentsPositionController(IConfiguration conf)
        {
            _dalUrl = conf.GetValue<string>("DalUrl");
            _parentsPosition = new HttpClient();
        }

        [HttpGet]
        public async Task<ActionResult<ParentsPosition[]>> GetParentsPosition()
        {
            var response = await _parentsPosition.GetAsync($"{_dalUrl}/ParentsPosition");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            if (content == null) return NotFound();

            return JsonSerializer.Deserialize<ParentsPosition[]>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? Array.Empty<ParentsPosition>();
        }
        [HttpPost]
        public async Task<ActionResult<ParentsPosition>> PostParentsPosition(ParentsPosition parentsPosition)
        {
            JsonContent content = JsonContent.Create(parentsPosition);
            using var result = await _parentsPosition.PostAsync($"{_dalUrl}/ParentsPosition/PostParentsPosition", content);
            var dalParentsPosition = await result.Content.ReadFromJsonAsync<ParentsPosition>();
            Console.WriteLine($"{dalParentsPosition?.ParentsId}");
            if (dalParentsPosition == null)
                return BadRequest();
            else
                return dalParentsPosition;
        }

    }
}