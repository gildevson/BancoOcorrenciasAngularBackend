using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace RemessaSeguraBakend.Controllers {
    [ApiController]
    [Route("api/market/currency")]
    public class CurrencyController : ControllerBase {
        private readonly IHttpClientFactory _httpFactory;
        private readonly ILogger<CurrencyController> _logger;
        private readonly IMemoryCache _cache;

        public CurrencyController(
            IHttpClientFactory httpFactory,
            ILogger<CurrencyController> logger,
            IMemoryCache cache) {
            _httpFactory = httpFactory;
            _logger = logger;
            _cache = cache;
        }
        [HttpGet("quotes")]
        public async Task<IActionResult> Quotes() {
            try {
                const string cacheKey = "currency_quotes";

                if (_cache.TryGetValue(cacheKey, out object? cachedResult)) {
                    _logger.LogInformation("💾 Cache hit: quotes");
                    return Ok(cachedResult);
                }

                var http = _httpFactory.CreateClient();
                var pairs = "USD-BRL,EUR-BRL,GBP-BRL,ARS-BRL,BTC-BRL";
                var url = $"https://economia.awesomeapi.com.br/json/last/{pairs}";

                _logger.LogInformation("🌐 Fetching quotes from AwesomeAPI");

                var json = await http.GetStringAsync(url);
                using var doc = JsonDocument.Parse(json);

                var results = new List<object>();

                foreach (var prop in doc.RootElement.EnumerateObject()) {
                    var curr = prop.Value;

                    // ✅ CORRIGIDO: Usa prop.Name que contém "USDBRL" ao invés de "code" que tem só "USD"
                    var symbol = prop.Name.Replace("-", ""); // USDBRL, EURBRL, etc
                    var name = curr.GetProperty("name").GetString() ?? "";
                    var bid = curr.GetProperty("bid").GetString() ?? "0";
                    var varBid = curr.GetProperty("varBid").GetString() ?? "0";

                    results.Add(new {
                        symbol = symbol,
                        shortName = name,
                        longName = name,
                        regularMarketPrice = decimal.Parse(bid, System.Globalization.CultureInfo.InvariantCulture),
                        regularMarketChangePercent = decimal.Parse(varBid, System.Globalization.CultureInfo.InvariantCulture)
                    });
                }

                var response = new { results };
                _cache.Set(cacheKey, response, TimeSpan.FromSeconds(30));

                _logger.LogInformation("✅ Returning {Count} currencies", results.Count);
                return Ok(response);
            } catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.TooManyRequests) {
                _logger.LogWarning("⚠️ Rate limit hit");
                return StatusCode(429, new {
                    error = "Muitas requisições. Aguarde alguns instantes.",
                    retryAfter = 60
                });
            } catch (Exception ex) {
                _logger.LogError(ex, "❌ Error in /quotes");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("history/{pair}")]
        public async Task<IActionResult> History([FromRoute] string pair, [FromQuery] int days = 30) {
            try {
                var cacheKey = $"currency_history_{pair}_{days}";

                if (_cache.TryGetValue(cacheKey, out object? cachedResult)) {
                    _logger.LogInformation("💾 Cache hit: {Pair} history", pair);
                    return Ok(cachedResult);
                }

                var http = _httpFactory.CreateClient();

                var formatted = pair.Length == 6
                    ? $"{pair.Substring(0, 3)}-{pair.Substring(3)}"
                    : pair;

                var url = $"https://economia.awesomeapi.com.br/json/daily/{formatted}/{days}";

                _logger.LogInformation("🌐 Fetching history: {Pair} ({Days} days)", pair, days);

                var json = await http.GetStringAsync(url);
                using var doc = JsonDocument.Parse(json);

                var points = new List<object>();

                foreach (var item in doc.RootElement.EnumerateArray()) {
                    var ts = item.GetProperty("timestamp").GetString();
                    var bid = item.GetProperty("bid").GetString();

                    if (long.TryParse(ts, out var tsLong) &&
                        decimal.TryParse(bid, System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out var bidDec)) {
                        points.Add(new { date = tsLong, close = bidDec });
                    }
                }

                var payload = new {
                    results = new[] { new { historicalDataPrice = points } }
                };

                _cache.Set(cacheKey, payload, TimeSpan.FromSeconds(60));

                _logger.LogInformation("✅ Returning {Count} historical points", points.Count);
                return Ok(payload);
            } catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.TooManyRequests) {
                _logger.LogWarning("⚠️ Rate limit hit for {Pair}", pair);
                return StatusCode(429, new {
                    error = "Muitas requisições. Aguarde alguns instantes.",
                    retryAfter = 60
                });
            } catch (Exception ex) {
                _logger.LogError(ex, "❌ Error in /history/{Pair}", pair);
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}