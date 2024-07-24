using BOM.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Domain.Controllers
{
    [ApiController]
    [Microsoft.AspNetCore.Mvc.Route("api/Domain/Specification")]
    public class SpecificationController : ControllerBase
    {
        private readonly string? _dalUrl;
        private readonly HttpClient _specification;

        public SpecificationController(IConfiguration conf)
        {
            _dalUrl = conf.GetValue<string>("DalUrl");
            _specification = new HttpClient();
        }

        [HttpGet]
        public async Task<ActionResult<Specification[]>> GetSpecification()
        {
            var response = await _specification.GetAsync($"{_dalUrl}/Specification");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            if (content == null) return NotFound();

            return JsonSerializer.Deserialize<Specification[]>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? Array.Empty<Specification>();
        }
        [HttpPost]
        public async Task<ActionResult<Specification>> PostClient(Specification specification)
        {
            JsonContent content = JsonContent.Create(specification);
            using var result = await _specification.PostAsync($"{_dalUrl}/Specification/PostSpecification", content);
            var dalSpecification = await result.Content.ReadFromJsonAsync<Specification>();
            Console.WriteLine($"{dalSpecification?.PostionId}");
            if (dalSpecification == null)
                return BadRequest();
            else
                return dalSpecification;
        }

    }
}