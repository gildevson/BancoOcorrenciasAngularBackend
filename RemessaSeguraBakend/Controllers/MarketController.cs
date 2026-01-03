using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace RemessaSeguraBakend.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class MarketController : ControllerBase {
    private readonly IHttpClientFactory _http;
    private readonly IConfiguration _configuration;

    public MarketController(IHttpClientFactory http, IConfiguration configuration) {
        _http = http;
        _configuration = configuration;
    }

    // ✅ Helper para adicionar token automaticamente
    private string? GetBrapiToken() {
        return _configuration["BrapiSettings:ApiKey"];
    }

    // GET /api/market/quote?symbols=ITUB4
    [HttpGet("quote")]
    public async Task<IActionResult> Quote([FromQuery] string symbols) {
        var client = _http.CreateClient("brapi");

        var query = new Dictionary<string, string?>();
        var token = GetBrapiToken();
        if (!string.IsNullOrEmpty(token))
            query["token"] = token;

        var url = QueryHelpers.AddQueryString($"quote/{symbols}", query);
        var res = await client.GetAsync(url);
        var json = await res.Content.ReadAsStringAsync();

        if (!res.IsSuccessStatusCode)
            return StatusCode((int)res.StatusCode, json);

        return Content(json, "application/json");
    }

    // GET /api/market/list
    [HttpGet("list")]
    public async Task<IActionResult> List(
        [FromQuery] string sector = "Finance",
        [FromQuery] string sortBy = "change",
        [FromQuery] string sortOrder = "desc",
        [FromQuery] int limit = 50,
        [FromQuery] int page = 1,
        [FromQuery] string? search = null,
        [FromQuery] string? type = null
    ) {
        var client = _http.CreateClient("brapi");

        var query = new Dictionary<string, string?> {
            ["sector"] = sector,
            ["sortBy"] = sortBy,
            ["sortOrder"] = sortOrder,
            ["limit"] = limit.ToString(),
            ["page"] = page.ToString(),
            ["search"] = search,
            ["type"] = type
        };

        // ✅ Adiciona token se existir
        var token = GetBrapiToken();
        if (!string.IsNullOrEmpty(token))
            query["token"] = token;

        var url = QueryHelpers.AddQueryString("quote/list", query);
        var res = await client.GetAsync(url);
        var json = await res.Content.ReadAsStringAsync();

        if (!res.IsSuccessStatusCode)
            return StatusCode((int)res.StatusCode, json);

        return Content(json, "application/json");
    }

    // GET /api/market/history/ITUB4?range=1d&interval=5m
    [HttpGet("history/{ticker}")]
    public async Task<IActionResult> History(
        string ticker,
        [FromQuery] string range,
        [FromQuery] string interval
    ) {
        var client = _http.CreateClient("brapi");
        var token = GetBrapiToken();

        async Task<(HttpResponseMessage Res, string Body)> Call(string r, string i) {
            var query = new Dictionary<string, string?> {
                ["range"] = r,
                ["interval"] = i
            };

            if (!string.IsNullOrEmpty(token))
                query["token"] = token;

            var url = QueryHelpers.AddQueryString($"quote/{ticker}", query);
            var res = await client.GetAsync(url);
            var body = await res.Content.ReadAsStringAsync();
            return (res, body);
        }

        // ✅ CORRIGIDO - Fallbacks com intervalos válidos da BRAPI
        var attempts = new List<(string r, string i)> {
            (range, interval),
            ("1d", "5m"),       // ✅ Corrigido
            ("1mo", "1d"),      // ✅ OK
            ("1y", "1wk")       // ✅ OK
        };

        var seen = new HashSet<string>();
        HttpResponseMessage? lastRes = null;
        string lastBody = "";

        // Dentro do método History no MarketController.cs
        foreach (var (r, i) in attempts) {
            var key = $"{r}|{i}";
            if (!seen.Add(key)) continue;

            var (res, body) = await Call(r, i);
            lastRes = res;
            lastBody = body;

            if (res.IsSuccessStatusCode) {
                // ✅ NOVA VALIDAÇÃO: Verifica se há dados reais no JSON
                if (body.Contains("\"historicalDataPrice\":[]") || body.Contains("\"historicalDataPrice\": null")) {
                    continue; // Se estiver vazio, tenta o próximo (ex: pula de 1d para 1mo)
                }
                return Content(body, "application/json");
            }

            // Se for erro de Token, para imediatamente
            if ((int)res.StatusCode == 401) return StatusCode(401, lastBody);

            // Continua tentando se for erro de parâmetro
            if ((int)res.StatusCode is not (400 or 403 or 404 or 417)) break;
        }

        return StatusCode((int)(lastRes?.StatusCode ?? System.Net.HttpStatusCode.BadGateway), lastBody);
    }
}