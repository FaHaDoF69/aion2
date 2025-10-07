using Microsoft.EntityFrameworkCore;
using Aion2Helper.Data;
using Aion2Helper.Models;

namespace Aion2Helper.Services
{
    /// <summary>
    /// AI配置服务
    /// </summary>
    public class AIConfigurationService
    {
        private readonly Aion2DbContext _context;

        public AIConfigurationService()
        {
            _context = new Aion2DbContext();
        }

        public AIConfigurationService(Aion2DbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 根据机器码获取AI配置
        /// </summary>
        /// <param name="machineCode">机器码</param>
        /// <returns>AI配置，如果不存在则返回null</returns>
        public async Task<AIConfiguration?> GetConfigurationByMachineCodeAsync(string machineCode)
        {
            try
            {
                return await _context.AIConfigurations
                    .FirstOrDefaultAsync(c => c.MachineCode == machineCode);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"获取AI配置失败: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 保存或更新AI配置
        /// </summary>
        /// <param name="config">AI配置</param>
        /// <param name="errorMessage">错误信息</param>
        /// <returns>是否成功</returns>
        public async Task<(bool Success, string? ErrorMessage)> SaveConfigurationAsync(AIConfiguration config)
        {
            try
            {
                // 验证权重总和
                if (!config.ValidateWeights())
                {
                    string error = "权重总和必须等于100%";
                    Console.WriteLine(error);
                    return (false, error);
                }

                // 检查是否已存在
                var existing = await _context.AIConfigurations
                    .FirstOrDefaultAsync(c => c.MachineCode == config.MachineCode);

                if (existing != null)
                {
                    // 更新现有配置
                    existing.AIMode = config.AIMode;
                    existing.AutoBuyEnabled = config.AutoBuyEnabled;
                    existing.AutoBuyMinScore = config.AutoBuyMinScore;
                    existing.AutoIgnoreMaxScore = config.AutoIgnoreMaxScore;
                    existing.PriceWeight = config.PriceWeight;
                    existing.MarketWeight = config.MarketWeight;
                    existing.ProfitWeight = config.ProfitWeight;
                    existing.TimingWeight = config.TimingWeight;
                    existing.HistoryWeight = config.HistoryWeight;
                    existing.MaxSingleInvestment = config.MaxSingleInvestment;
                    existing.RequireManualConfirmAmount = config.RequireManualConfirmAmount;
                    existing.EnableAIBlacklist = config.EnableAIBlacklist;
                    existing.TriggerOnStrategy = config.TriggerOnStrategy;
                    existing.TriggerOnEveryCheck = config.TriggerOnEveryCheck;
                    existing.ManualTriggerOnly = config.ManualTriggerOnly;
                    existing.IsActive = config.IsActive;
                    existing.UpdatedAt = DateTime.Now;

                    _context.AIConfigurations.Update(existing);
                    Console.WriteLine($"更新AI配置: {config.MachineCode}");
                }
                else
                {
                    // 添加新配置
                    _context.AIConfigurations.Add(config);
                    Console.WriteLine($"添加新AI配置: {config.MachineCode}");
                }

                await _context.SaveChangesAsync();
                Console.WriteLine("AI配置保存成功");
                return (true, null);
            }
            catch (Exception ex)
            {
                string error = $"保存AI配置失败: {ex.Message}\n详细信息: {ex.InnerException?.Message}";
                Console.WriteLine(error);
                Console.WriteLine($"堆栈跟踪: {ex.StackTrace}");
                return (false, error);
            }
        }

        /// <summary>
        /// 获取所有AI配置
        /// </summary>
        /// <returns>AI配置列表</returns>
        public async Task<List<AIConfiguration>> GetAllConfigurationsAsync()
        {
            try
            {
                return await _context.AIConfigurations
                    .OrderBy(c => c.MachineCode)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"获取AI配置列表失败: {ex.Message}");
                return new List<AIConfiguration>();
            }
        }

        /// <summary>
        /// 删除AI配置
        /// </summary>
        /// <param name="machineCode">机器码</param>
        /// <returns>是否成功</returns>
        public async Task<bool> DeleteConfigurationAsync(string machineCode)
        {
            try
            {
                var config = await _context.AIConfigurations
                    .FirstOrDefaultAsync(c => c.MachineCode == machineCode);

                if (config != null)
                {
                    _context.AIConfigurations.Remove(config);
                    await _context.SaveChangesAsync();
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"删除AI配置失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 创建默认AI配置
        /// </summary>
        /// <param name="machineCode">机器码</param>
        /// <returns>默认AI配置</returns>
        public static AIConfiguration CreateDefaultConfiguration(string machineCode)
        {
            return new AIConfiguration
            {
                MachineCode = machineCode,
                AIMode = "AI辅助决策",
                AutoBuyEnabled = false,
                AutoBuyMinScore = 75,
                AutoIgnoreMaxScore = 60,
                PriceWeight = 30,
                MarketWeight = 20,
                ProfitWeight = 30,
                TimingWeight = 10,
                HistoryWeight = 10,
                MaxSingleInvestment = 5000000,
                RequireManualConfirmAmount = 1000000,
                EnableAIBlacklist = true,
                TriggerOnStrategy = true,
                TriggerOnEveryCheck = false,
                ManualTriggerOnly = false,
                IsActive = true
            };
        }
    }
}

