using Hangfire;
using Hangfire.Storage;
using StateChangeSimulation;

var dataManager = new DataManager();
await dataManager.ClearDownLedger();

GlobalConfiguration.Configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage("Server=localhost;Database=CollectStateLedger;User Id=SA;Password=MyStrongPassword123!;TrustServerCertificate=True;Encrypt=True;");

var fiveAmEveryDay = "0 5 * * *";
var everyMinute = "0/1 * * * *";
var everyFiveMinutes = "0/5 * * * *";
using var connection = JobStorage.Current.GetConnection();
var jobs = connection.GetRecurringJobs();
foreach (var job in jobs)
{
    Console.WriteLine($"Recurring Job: {job.Id}, Cron: '{job.Cron}' next run at {job.NextExecution}");
}

RecurringJob.AddOrUpdate("5am", () => Console.WriteLine("Recurs every day at 5am"), fiveAmEveryDay);
RecurringJob.AddOrUpdate("everyMinute", () => Console.WriteLine("Occurs every minute"), everyMinute);
RecurringJob.AddOrUpdate("dataUpload", () => CensusLedgerJobs.RunUpdateCensusLedger(), everyFiveMinutes);


using var server = new BackgroundJobServer();
Console.ReadLine();