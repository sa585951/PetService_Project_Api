using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PetService_Project.Models;

namespace PetService_Project_Api.WebSockets
{
    public class MemberSourceBroadcaster:BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public MemberSourceBroadcaster(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<dbPetService_ProjectContext>();

                // 1. 取得所有被記錄的來源管道的總次數，作為計算百分比的基數
                var totalSources = await db.TMemberSources.CountAsync();

                // 2. 查詢每個來源管道被選擇的次數，並聯結來源名稱
                var sourceCounts = await db.TMemberSources
                    .GroupBy(x => x.FSourceId)
                    .Select(g => new { fSourceId = g.Key, source_count = g.Count() }) // 計算每個來源 ID 出現的次數
                    .OrderByDescending(x => x.source_count)
                    .Join(
                        db.TSourceLists,
                        sourceCount => sourceCount.fSourceId,
                        sourceList => sourceList.FSourceId,
                        (sourceCount, sourceList) => new
                        {
                            fSourceId = sourceCount.fSourceId,
                            sourceName = sourceList.FSourceName, // 假設 tSourceList 中來源名稱的欄位是 FName
                            source_count = sourceCount.source_count
                        })
                    .ToListAsync();

                // 3. 計算每個來源管道的百分比
                var statsWithPercentage = sourceCounts.Select(s => new
                {
                    sourceName = s.sourceName,
                    percentage = totalSources > 0 ? (double)s.source_count / totalSources * 100 : 0
                }).OrderByDescending(s => s.percentage).ToList();

                // 4. 將包含來源名稱和百分比的資料序列化為 JSON
                string json = JsonSerializer.Serialize(statsWithPercentage);
                await WebSocketHandler.BroadcastAsync(json);

                await Task.Delay(5000, stoppingToken); // 每 5 秒更新一次
            }
        }
    }
}
