using Microsoft.AspNetCore.Mvc; //Ben
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.IO;

namespace FirstWebApplication.Controllers
{
    [Route("[controller]")]
    public class MapController : Controller
    {
        private readonly string _filePath = "wwwroot/mapdata.json";

        [HttpPost("Save")]
        public IActionResult Save([FromBody] List<MapObject> objects)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(objects, options);
            System.IO.File.WriteAllText(_filePath, json);
            return Ok();
        }

        [HttpGet("Load")]
        public IActionResult Load()
        {
            if (System.IO.File.Exists(_filePath))
            {
                var json = System.IO.File.ReadAllText(_filePath);
                return Content(json, "application/json");
            }
            return Content("[]", "application/json");
        }
    }

    public class MapObject
    {
        public string Type { get; set; }
        public LatLng LatLng { get; set; }
        public List<LatLng> LatLngs { get; set; }
        public Details Details { get; set; }
    }

    public class LatLng
    {
        [JsonPropertyName("lat")] 
        public double Lat { get; set; }

        [JsonPropertyName("lng")]
        public double Lng { get; set; }
    }

    public class Details
    {
        public string Navn { get; set; }
        public string Hoyde { get; set; }
        public string Bredde { get; set; }
        public string Beskrivelse { get; set; }
    }
}
