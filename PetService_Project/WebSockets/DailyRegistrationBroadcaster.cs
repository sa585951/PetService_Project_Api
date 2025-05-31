using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PetService_Project.Models;

namespace PetService_Project_Api.WebSockets
{
    public class DailyRegistrationBroadcaster: IHostedService, IDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly int _broadcastIntervalSeconds = 86400; // 每天廣播一次
        private Timer _timer;
        private bool _isRunning = false;

        public DailyRegistrationBroadcaster(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(_broadcastIntervalSeconds));
            _isRunning = true;
            return Task.CompletedTask;
        }

        private async void DoWork(object state)
        {
            if (!_isRunning) return;

            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<dbPetService_ProjectContext>();
                var today = DateTime.Now.Date;
                var tomorrow = today.AddDays(1);

                // 方法 1: 使用日期範圍查詢（推薦）
                var dailyCounts = await dbContext.TMembers
                    .Where(m => m.FCreatedDate >= today && m.FCreatedDate < tomorrow)
                    .GroupBy(m => new {
                        Year = m.FCreatedDate.Value.Year,
                        Month = m.FCreatedDate.Value.Month,
                        Day = m.FCreatedDate.Value.Day,
                    })
                    .Select(g => new {
                        RegistrationDate = $"{g.Key.Year:0000}-{g.Key.Month:00}-{g.Key.Day:00}",
                        Count = g.Count()
                    })
                    .ToDictionaryAsync(x => x.RegistrationDate, x => x.Count);

                var message = JsonConvert.SerializeObject(new
                {
                    type = "daily_registrations",
                    data = dailyCounts
                });

                await WebSocketHandler.BroadcastAsync(message);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            _isRunning = false;
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
