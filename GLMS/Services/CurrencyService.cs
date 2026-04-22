using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace GLMS.Services
{
    public class CurrencyService : ICurrencyService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CurrencyService> _logger;
        private readonly IMemoryCache _cache;
        private const string CACHE_KEY = "USD_ZAR_RATE";
        private const int CACHE_DURATION_MINUTES = 60;

        //free API and also using fallback rate
        private const string API_URL = "https://api.exchangerate-api.com/v4/latest/USD";

        public CurrencyService(HttpClient httpClient, ILogger<CurrencyService> logger, IMemoryCache cache)
        {
            _httpClient = httpClient;
            _logger = logger;
            _cache = cache;
        }

        public async Task<decimal> GetUsdToZarRateAsync()
        {
            // Check cache
            if (_cache.TryGetValue(CACHE_KEY, out decimal cachedRate))
            {
                _logger.LogInformation("Returning cached USD/ZAR rate: {Rate}", cachedRate);
                return cachedRate;
            }

            try
            {
                _logger.LogInformation("Fetching live USD/ZAR exchange rate from API");
                var response = await _httpClient.GetAsync(API_URL);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;

                    if (root.TryGetProperty("rates", out var rates) && rates.TryGetProperty("ZAR", out var zarRate))
                    {
                        var rate = zarRate.GetDecimal();

                        // Cache rate
                        _cache.Set(CACHE_KEY, rate, TimeSpan.FromMinutes(CACHE_DURATION_MINUTES));

                        _logger.LogInformation("Successfully fetched USD/ZAR rate: {Rate}", rate);
                        return rate;
                    }
                }

                // if API fails
                _logger.LogWarning("API call failed or invalid response, using fallback rate");
                const decimal fallbackRate = 16.50m;
                _cache.Set(CACHE_KEY, fallbackRate, TimeSpan.FromMinutes(15));
                return fallbackRate;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching exchange rate");
                return 16.50m;
            }
        }

        public async Task<decimal> ConvertUsdToZarAsync(decimal usdAmount)
        {
            var rate = await GetUsdToZarRateAsync();
            return usdAmount * rate;
        }
    }
}