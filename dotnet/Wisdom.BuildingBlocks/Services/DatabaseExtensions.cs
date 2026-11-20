using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Wisdom.BuildingBlocks.Services
{
    public static class DatabaseExtensions
    {
        private static readonly SemaphoreSlim MigrationLock = new(1, 1);

        // Bounds the *entire* init (all retries combined) so a cold/unreachable Postgres
        // fails predictably in seconds instead of drifting toward Npgsql's default
        // per-attempt timeouts (15s connect / 30s command) × 5 retries — worst case
        // that used to look like the process "never came up" instead of just being slow.
        private static readonly TimeSpan OverallDeadline = TimeSpan.FromSeconds(30);

        public static async Task InitialiseDatabaseAsync<TContext>(this WebApplication app)
            where TContext : DbContext
        {
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<TContext>();

            using var cts = new CancellationTokenSource(OverallDeadline);
            var ct = cts.Token;

            await MigrationLock.WaitAsync(ct);
            try
            {
                if (context.Database.IsRelational())
                {
                    var databaseExists = false;
                    try
                    {
                        databaseExists = await context.Database.CanConnectAsync(ct);
                        if (!databaseExists)
                        {
                            Console.WriteLine($"{typeof(TContext).Name}: Database does not exist, will be created and migrated.");
                        }
                    }
                    catch (Exception) when (!ct.IsCancellationRequested)
                    {
                        Console.WriteLine($"{typeof(TContext).Name}: Database will be created and migrated.");
                    }

                    if (!databaseExists)
                    {

                        var retries = 5;
                        while (retries > 0)
                        {
                            try
                            {
                                await context.Database.MigrateAsync(ct);
                                Console.WriteLine($"{typeof(TContext).Name} database creation and migration completed successfully.");
                                break;
                            }
                            catch (Exception ex) when (retries > 1 && !ct.IsCancellationRequested)
                            {
                                retries--;
                                Console.WriteLine($"{typeof(TContext).Name} migration attempt failed, retrying... ({retries} attempts left). Error: {ex.Message}");
                                await Task.Delay(TimeSpan.FromSeconds(2), ct);
                            }
                        }
                    }
                    else
                    {
                        var pendingMigrations = (await context.Database.GetPendingMigrationsAsync(ct)).ToList();
                        if (pendingMigrations.Count != 0)
                        {
                            Console.WriteLine($"{typeof(TContext).Name}: Applying {pendingMigrations.Count} pending migration(s)...");
                            var retries = 5;
                            while (retries > 0)
                            {
                                try
                                {
                                    await context.Database.MigrateAsync(ct);
                                    Console.WriteLine($"{typeof(TContext).Name} database migration completed successfully.");
                                    break;
                                }
                                catch (Exception ex) when (retries > 1 && !ct.IsCancellationRequested)
                                {
                                    retries--;
                                    Console.WriteLine($"{typeof(TContext).Name} migration attempt failed, retrying... ({retries} attempts left). Error: {ex.Message}");
                                    await Task.Delay(TimeSpan.FromSeconds(2), ct);
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine($"{typeof(TContext).Name} database is up to date, no migrations needed.");
                        }
                    }
                }
                else
                {
                    await context.Database.EnsureCreatedAsync(ct);
                    Console.WriteLine($"{typeof(TContext).Name} non-relational database ensured.");
                }
            }
            catch (OperationCanceledException) when (cts.IsCancellationRequested)
            {
                Console.WriteLine($"{typeof(TContext).Name} database initialization timed out after {OverallDeadline.TotalSeconds:0}s.");
                Console.WriteLine("Application will continue without database initialization.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{typeof(TContext).Name} database initialization failed: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                Console.WriteLine("Application will continue without database initialization.");
            }
            finally
            {
                MigrationLock.Release();
            }
        }
    }
}
