using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace eftest
{
    public static class Program
    {
        public static async Task Main()
        {
            await SetupDatabase();

            using (var db = new TestContext())
            {
                var queryableCount = await db.Payments.GroupBy(p => p.AccountId, (accountId, group) => new {Account = accountId, Total = group.Sum(g => g.Value)}).CountAsync();
                Console.WriteLine($"Count of queryable payments grouped by AccountId: {queryableCount}");

                var enumerableCount = db.Payments.AsEnumerable().GroupBy(p => p.AccountId, (accountId, group) => new { Account = accountId, Total = group.Sum(g => g.Value) }).Count();
                Console.WriteLine($"Count of enumerable payments grouped by AccountId: {enumerableCount}");
            }
        }

        private static async Task SetupDatabase()
        {
            using (var db = new TestContext())
            {
                if (await db.Database.EnsureCreatedAsync())
                {
                    db.Payments.AddRange(
                        new Payment {AccountId = 1, Value = 10},
                        new Payment {AccountId = 2, Value = 11},
                        new Payment {AccountId = 2, Value = 12},
                        new Payment {AccountId = 3, Value = 13},
                        new Payment {AccountId = 3, Value = 14},
                        new Payment {AccountId = 3, Value = 15});

                    await db.SaveChangesAsync();
                }
            }
        }
    }

    public class TestContext : DbContext
    {
        private static readonly ILoggerFactory LoggerFactory
            = new LoggerFactory().AddConsole((s, l) => l == LogLevel.Information && !s.EndsWith("Connection"));

        public DbSet<Payment> Payments { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseSqlServer(@"Server=(local);Database=Sample.GroupBy;Trusted_Connection=True;ConnectRetryCount=0;")
                .UseLoggerFactory(LoggerFactory);
        }
    }

    public class Payment
    {
        public int PaymentId { get; set; }
        public int AccountId { get; set; }
        public decimal Value { get; set; }
    }
}
