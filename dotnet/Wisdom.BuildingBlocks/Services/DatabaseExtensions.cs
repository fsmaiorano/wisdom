using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
namespace Wisdom.BuildingBlocks.Services
{
    public static class DatabaseExtensions
    {
        private static readonly SemaphoreSlim MigrationLock = new(1, 1);

        public static async Task InitialiseDatabaseAsync<TContext>(this WebApplication app)
            where TContext : DbContext
        {
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<TContext>();

            await MigrationLock.WaitAsync();
            try
            {
                if (context.Database.IsRelational())
                {
                    var databaseExists = false;
                    try
                    {
                        databaseExists = await context.Database.CanConnectAsync();
                        if (!databaseExists)
                        {
                            Console.WriteLine($"{typeof(TContext).Name}: Database does not exist, will be created and migrated.");
                        }
                    }
                    catch (Exception)
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
                                await context.Database.MigrateAsync();
                                Console.WriteLine($"{typeof(TContext).Name} database creation and migration completed successfully.");
                                break;
                            }
                            catch (Exception ex) when (retries > 1)
                            {
                                retries--;
                                Console.WriteLine($"{typeof(TContext).Name} migration attempt failed, retrying... ({retries} attempts left). Error: {ex.Message}");
                                await Task.Delay(TimeSpan.FromSeconds(2));
                            }
                        }
                    }
                    else
                    {
                        var pendingMigrations = (await context.Database.GetPendingMigrationsAsync()).ToList();
                        if (pendingMigrations.Any())
                        {
                            Console.WriteLine($"{typeof(TContext).Name}: Applying {pendingMigrations.Count} pending migration(s)...");
                            var retries = 5;
                            while (retries > 0)
                            {
                                try
                                {
                                    await context.Database.MigrateAsync();
                                    Console.WriteLine($"{typeof(TContext).Name} database migration completed successfully.");
                                    break;
                                }
                                catch (Exception ex) when (retries > 1)
                                {
                                    retries--;
                                    Console.WriteLine($"{typeof(TContext).Name} migration attempt failed, retrying... ({retries} attempts left). Error: {ex.Message}");
                                    await Task.Delay(TimeSpan.FromSeconds(2));
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
                    await context.Database.EnsureCreatedAsync();
                    Console.WriteLine($"{typeof(TContext).Name} non-relational database ensured.");
                }
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
