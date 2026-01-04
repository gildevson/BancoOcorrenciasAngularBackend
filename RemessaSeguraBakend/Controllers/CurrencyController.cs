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

                // ✅ Verifica cache primeiro (15 minutos para evitar 429)
                if (_cache.TryGetValue(cacheKey, out object? cachedResult)) {
                    _logger.LogInformation("Retornando cotações do cache");
                    return Ok(cachedResult);
                }

                var http = _httpFactory.CreateClient();
                http.Timeout = TimeSpan.FromSeconds(10);

                var pairs = "USD-BRL,EUR-BRL,GBP-BRL,ARS-BRL,BTC-BRL";
                var url = $"https://economia.awesomeapi.com.br/json/last/{pairs}";

                try {
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

                    // ✅ Cache por 15 minutos (aumentado de 5 para evitar rate limit)
                    _cache.Set(cacheKey, response, TimeSpan.FromMinutes(15));

                    _logger.LogInformation("Cotações obtidas com sucesso da AwesomeAPI");
                    return Ok(response);

                } catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.TooManyRequests) {
                    _logger.LogWarning("Rate limit atingido (429). Tentando APIs alternativas...");
                    return await GetQuotesFromFallback();
                }

            } catch (Exception ex) {
                _logger.LogError(ex, "Erro ao obter cotações");
                return StatusCode(500, new { error = "Erro ao buscar cotações. Tente novamente em alguns instantes." });
            }
        }

        // ✅ FALLBACK: Busca cotações de APIs alternativas
        private async Task<IActionResult> GetQuotesFromFallback() {
            try {
                var http = _httpFactory.CreateClient();
                var results = new List<object>();

                // Lista de moedas para buscar
                var currencies = new[] { "USD", "EUR", "GBP" };

                foreach (var currency in currencies) {
                    try {
                        // Tenta Frankfurter (API gratuita sem rate limit agressivo)
                        var url = $"https://api.frankfurter.app/latest?from={currency}&to=BRL";
                        var json = await http.GetStringAsync(url);
                        using var doc = JsonDocument.Parse(json);

                        var rate = doc.RootElement.GetProperty("rates").GetProperty("BRL").GetDecimal();

                        results.Add(new {
                            symbol = $"{currency}BRL",
                            shortName = $"{GetCurrencyName(currency)}/Real Brasileiro",
                            regularMarketPrice = rate,
                            regularMarketChangePercent = 0m // Frankfurter não fornece variação
                        });

                        _logger.LogInformation($"Cotação {currency}/BRL obtida do Frankfurter: {rate}");

                    } catch (Exception ex) {
                        _logger.LogWarning($"Erro ao buscar {currency} do Frankfurter: {ex.Message}");
                    }
                }

                if (results.Count > 0) {
                    var response = new { results };
                    _cache.Set("currency_quotes", response, TimeSpan.FromMinutes(15));
                    return Ok(response);
                }

                return StatusCode(503, new {
                    error = "Serviços de cotação temporariamente indisponíveis. Tente novamente em alguns minutos.",
                    retryAfter = 300 // 5 minutos
                });

            } catch (Exception ex) {
                _logger.LogError(ex, "Erro no fallback de cotações");
                return StatusCode(503, new { error = "Cotações temporariamente indisponíveis" });
            }
        }

        [HttpGet("history/{pair}")]
        public async Task<IActionResult> History([FromRoute] string pair, [FromQuery] int days = 30) {
            try {
                var cacheKey = $"currency_history_{pair}_{days}";

                // Cache por 30 minutos
                if (_cache.TryGetValue(cacheKey, out object? cachedResult)) {
                    _logger.LogInformation($"Retornando histórico {pair} do cache");
                    return Ok(cachedResult);
                }

                var http = _httpFactory.CreateClient();
                http.Timeout = TimeSpan.FromSeconds(15);

                string formatted = pair.Length == 6 ? $"{pair.Substring(0, 3)}-{pair.Substring(3)}" : pair;

                // 1. Tenta AwesomeAPI
                try {
                    var response = await http.GetAsync($"https://economia.awesomeapi.com.br/json/daily/{formatted}/{days}");

                    if (response.IsSuccessStatusCode) {
                        var json = await response.Content.ReadAsStringAsync();
                        var result = ParseAwesomeHistory(json);
                        return SaveAndReturn(cacheKey, result);
                    }
                } catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.TooManyRequests) {
                    _logger.LogWarning("Rate limit atingido (429) no histórico. Usando Frankfurter...");
                }

                // 2. Fallback Frankfurter
                _logger.LogInformation("Tentando Frankfurter para histórico...");
                var fallback = await FetchFrankfurterFallback(pair, days);
                if (fallback != null) return SaveAndReturn(cacheKey, fallback);

                return StatusCode(503, new {
                    error = "Histórico temporariamente indisponível. Tente novamente em alguns minutos.",
                    retryAfter = 300
                });

            } catch (Exception ex) {
                _logger.LogError(ex, $"Erro ao buscar histórico de {pair}");
                return StatusCode(500, new { error = "Erro ao buscar histórico" });
            }
        }

        [HttpGet("convert")]
        public async Task<IActionResult> Convert([FromQuery] string from, [FromQuery] string to, [FromQuery] decimal amount) {
            try {
                if (string.IsNullOrEmpty(from) || string.IsNullOrEmpty(to) || amount <= 0) {
                    return BadRequest(new { error = "Parâmetros inválidos. Use: from, to e amount > 0" });
                }

                from = from.ToUpper();
                to = to.ToUpper();

                var cacheKey = $"conversion_rate_{from}_{to}";
                decimal rate;

                // ✅ Cache de 15 minutos para taxa de conversão
                if (_cache.TryGetValue(cacheKey, out rate)) {
                    _logger.LogInformation($"Taxa {from}/{to} obtida do cache: {rate}");
                } else {
                    rate = await GetConversionRate(from, to);
                    if (rate == 0) {
                        return StatusCode(503, new {
                            error = "Serviço de conversão temporariamente indisponível",
                            retryAfter = 300
                        });
                    }
                    _cache.Set(cacheKey, rate, TimeSpan.FromMinutes(15));
                }

                var converted = amount * rate;

                return Ok(new {
                    from,
                    to,
                    amount,
                    rate,
                    converted = Math.Round(converted, 2),
                    timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                });

            } catch (Exception ex) {
                _logger.LogError(ex, "Erro na conversão");
                return StatusCode(500, new { error = "Erro ao converter moeda" });
            }
        }

        // ✅ MÉTODO PARA OBTER TAXA COM FALLBACK AUTOMÁTICO
        private async Task<decimal> GetConversionRate(string from, string to) {
            var http = _httpFactory.CreateClient();
            http.Timeout = TimeSpan.FromSeconds(10);

            // 1. Tenta AwesomeAPI primeiro
            try {
                var pair = $"{from}-{to}";
                var url = $"https://economia.awesomeapi.com.br/json/last/{pair}";
                var json = await http.GetStringAsync(url);

                using var doc = JsonDocument.Parse(json);
                var pairKey = pair.Replace("-", "");

                if (doc.RootElement.TryGetProperty(pairKey, out var curr)) {
                    var rate = decimal.Parse(curr.GetProperty("bid").GetString()!, CultureInfo.InvariantCulture);
                    _logger.LogInformation($"Taxa {from}/{to} obtida da AwesomeAPI: {rate}");
                    return rate;
                }
            } catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.TooManyRequests) {
                _logger.LogWarning($"Rate limit (429) ao buscar taxa {from}/{to}. Tentando Frankfurter...");
            } catch (Exception ex) {
                _logger.LogWarning($"Erro ao buscar taxa {from}/{to} na AwesomeAPI: {ex.Message}");
            }

            // 2. Fallback: Frankfurter
            try {
                var url = $"https://api.frankfurter.app/latest?from={from}&to={to}";
                var json = await http.GetStringAsync(url);
                using var doc = JsonDocument.Parse(json);

                var rate = doc.RootElement.GetProperty("rates").GetProperty(to).GetDecimal();
                _logger.LogInformation($"Taxa {from}/{to} obtida do Frankfurter: {rate}");
                return rate;

            } catch (Exception ex) {
                _logger.LogError($"Erro ao buscar taxa {from}/{to} no Frankfurter: {ex.Message}");
                return 0;
            }
        }

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
            } catch (Exception ex) {
                _logger.LogError($"Erro no Frankfurter fallback: {ex.Message}");
                return null;
            }
        }

        private string GetCurrencyName(string code) => code switch {
            "USD" => "Dólar Americano",
            "EUR" => "Euro",
            "GBP" => "Libra Esterlina",
            "ARS" => "Peso Argentino",
            "BTC" => "Bitcoin",
            _ => code
        };
    }
}