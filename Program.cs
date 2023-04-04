using System.Configuration;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RPM_exercise;
using RPM_exercise.Database;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var services = new ServiceCollection();

        services.AddDbContext<DatabaseContext>(options => options.UseSqlServer(ConfigurationManager.AppSettings["dbConnection"]));

        var delay = ConfigurationManager.AppSettings["delay"];
        var days = ConfigurationManager.AppSettings["days"];
        var url = ConfigurationManager.AppSettings["apiUrl"];
        var apiParams = ConfigurationManager.AppSettings["apiParams"];

        Console.WriteLine($"Initializing application with delay of {delay} seconds.");

        var timer = new PeriodicTimer(TimeSpan.FromSeconds(Convert.ToDouble(delay)));

        while (await timer.WaitForNextTickAsync())
        {
            try
            {
                var prices = GetFuelPrices(url, apiParams);

                var inserted = await RecordFuelPrices(prices, Convert.ToInt32(days));

                Console.WriteLine($"Done. Inserted {inserted} rows.");

            } catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

    }

    private static List<Data> GetFuelPrices(string url, string apiParams, CancellationToken ct = default)
    {
        var data = new List<Data>();

        HttpClient client = new HttpClient();
        client.BaseAddress = new Uri(url);

        Console.WriteLine("Querying API.");

        HttpResponseMessage response = client.GetAsync(apiParams, ct).Result;

        if (response.IsSuccessStatusCode)
        {
            data = response.Content.ReadFromJsonAsync<JsonModel>(cancellationToken: ct).Result?.response?.data;

            Console.WriteLine($"API returned {data?.Count} rows.");
        }
        else
        {
            Console.WriteLine("API error.");
        }

        client.Dispose();

        return data;
    }

    private static async Task<int> RecordFuelPrices(List<Data> prices, int days)
    {
        var cutoffDate = DateTime.Now.AddDays(-1 * days);

        var filteredPrices = prices.Where(p => DateTime.Parse(p.period) > cutoffDate).ToList();

        Console.WriteLine($"Found {filteredPrices.Count} prices within date range.");

        using (var db = new DatabaseContext())
        {
            foreach (var item in filteredPrices)
            {
                var exists = db.Prices.Where(p => p.Period == item.period).AsNoTracking().FirstOrDefault();

                if (exists == null)
                {
                    var newPrice = new Price
                    {
                        Id = Guid.NewGuid(),
                        Period = item.period,
                        Value = item.value,
                    };

                    db.Prices.Add(newPrice);
                }

            }

            return await db.SaveChangesAsync();
        }
    }

}