using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;
using System.Globalization;

namespace RemessaSeguraBakend.Controllers {
    [ApiController]
    [Route("api/market/currency")]
    public class CurrencyController : ControllerBase {
        private readonly IHttpClientFactory _httpFactory;
        private readonly ILogger<CurrencyController> _logger;
        private readonly IMemoryCache _cache;

        public CurrencyController(IHttpClientFactory httpFactory, ILogger<CurrencyController> logger, IMemoryCache cache) {
            _httpFactory = httpFactory;
            _logger = logger;
            _cache = cache;
        }

        [HttpGet("quotes")]
        public async Task<IActionResult> Quotes() {
            try {
                const string cacheKey = "currency_quotes";
                if (_cache.TryGetValue(cacheKey, out object? cachedResult)) return Ok(cachedResult);

                var http = _httpFactory.CreateClient();
                var pairs = "USD-BRL,EUR-BRL,GBP-BRL,ARS-BRL,BTC-BRL";
                var url = $"https://economia.awesomeapi.com.br/json/last/{pairs}";

                var json = await http.GetStringAsync(url);
                using var doc = JsonDocument.Parse(json);
                var results = new List<object>();

                foreach (var prop in doc.RootElement.EnumerateObject()) {
                    var curr = prop.Value;
                    results.Add(new {
                        symbol = prop.Name.Replace("-", ""),
                        shortName = curr.GetProperty("name").GetString(),
                        regularMarketPrice = decimal.Parse(curr.GetProperty("bid").GetString()!, CultureInfo.InvariantCulture),
                        regularMarketChangePercent = decimal.Parse(curr.GetProperty("varBid").GetString()!, CultureInfo.InvariantCulture)
                    });
                }

                var response = new { results };
                _cache.Set(cacheKey, response, TimeSpan.FromMinutes(5)); // Aumentei para 5 min para evitar 429
                return Ok(response);
            } catch (Exception ex) {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("history/{pair}")]
        public async Task<IActionResult> History([FromRoute] string pair, [FromQuery] int days = 30) {
            try {
                var cacheKey = $"currency_history_{pair}_{days}";
                if (_cache.TryGetValue(cacheKey, out object? cachedResult)) return Ok(cachedResult);

                var http = _httpFactory.CreateClient();
                string formatted = pair.Length == 6 ? $"{pair.Substring(0, 3)}-{pair.Substring(3)}" : pair;

                // 1. Tenta AwesomeAPI
                var response = await http.GetAsync($"https://economia.awesomeapi.com.br/json/daily/{formatted}/{days}");

                if (response.IsSuccessStatusCode) {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = ParseAwesomeHistory(json);
                    return SaveAndReturn(cacheKey, result);
                }

                // 2. Fallback Frankfurter
                _logger.LogWarning("AwesomeAPI falhou. Tentando Frankfurter...");
                var fallback = await FetchFrankfurterFallback(pair, days);
                if (fallback != null) return SaveAndReturn(cacheKey, fallback);

                return StatusCode(503, new { error = "Cotações indisponíveis." });
            } catch (Exception ex) {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // ✅ MÉTODO QUE FALTA NO SEU CÓDIGO
        private List<object> ParseAwesomeHistory(string json) {
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.EnumerateArray().Select(item => new {
                date = long.Parse(item.GetProperty("timestamp").GetString()!),
                close = decimal.Parse(item.GetProperty("bid").GetString()!, CultureInfo.InvariantCulture)
            }).Cast<object>().ToList();
        }

        private IActionResult SaveAndReturn(string key, object data) {
            var payload = new { results = new[] { new { historicalDataPrice = data } } };
            _cache.Set(key, payload, TimeSpan.FromMinutes(30));
            return Ok(payload);
        }

        private async Task<List<object>?> FetchFrankfurterFallback(string pair, int days) {
            try {
                var http = _httpFactory.CreateClient();
                var from = pair.Substring(0, 3);
                var to = pair.Substring(3);
                var start = DateTime.UtcNow.AddDays(-days).ToString("yyyy-MM-dd");
                var json = await http.GetStringAsync($"https://api.frankfurter.app/{start}..?from={from}&to={to}");

                using var doc = JsonDocument.Parse(json);
                var rates = doc.RootElement.GetProperty("rates");
                return rates.EnumerateObject().Select(prop => new {
                    date = ((DateTimeOffset)DateTime.Parse(prop.Name)).ToUnixTimeSeconds(),
                    close = prop.Value.GetProperty(to).GetDecimal()
                }).Cast<object>().ToList();
            } catch { return null; }
        }
    }
}