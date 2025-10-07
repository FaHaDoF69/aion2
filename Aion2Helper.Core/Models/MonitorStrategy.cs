using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Aion2Helper.Models
{
    /// <summary>
    /// 监控策略枚举
    /// </summary>
    public enum MonitorStrategy
    {
        /// <summary>
        /// 价格监控 - 监控价格是否在目标范围内
        /// </summary>
        [Description("价格监控 - 监控价格是否在目标范围内")]
        PriceMonitor = 1,

        /// <summary>
        /// 低价抢购 - 发现低价立即购买
        /// </summary>
        [Description("低价抢购 - 发现低价立即购买")]
        LowPriceBuy = 2,

        /// <summary>
        /// 稳健收购 - 确保利润后购买
        /// </summary>
        [Description("稳健收购 - 确保利润后购买")]
        StableBuy = 3,

        /// <summary>
        /// 批量收购 - 大量购买囤货
        /// </summary>
        [Description("批量收购 - 大量购买囤货")]
        BulkPurchase = 4,

        /// <summary>
        /// 趋势观察 - 观察价格走势后决策
        /// </summary>
        [Description("趋势观察 - 观察价格走势后决策")]
        TrendWatch = 5,

        /// <summary>
        /// 定时检查 - 定期检查不紧急物品
        /// </summary>
        [Description("定时检查 - 定期检查不紧急物品")]
        TimedCheck = 6,

        /// <summary>
        /// 自定义 - 用户自定义策略
        /// </summary>
        [Description("自定义... - 输入自定义策略")]
        Custom = 99
    }

    /// <summary>
    /// 监控策略扩展方法
    /// </summary>
    public static class MonitorStrategyExtensions
    {
        /// <summary>
        /// 获取策略描述
        /// </summary>
        public static string GetDescription(this MonitorStrategy strategy)
        {
            var field = strategy.GetType().GetField(strategy.ToString());
            if (field != null)
            {
                var attribute = (DescriptionAttribute?)Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));
                if (attribute != null)
                {
                    return attribute.Description;
                }
            }
            return strategy.ToString();
        }

        /// <summary>
        /// 获取策略显示名称（不含描述）
        /// </summary>
        public static string GetDisplayName(this MonitorStrategy strategy)
        {
            return strategy switch
            {
                MonitorStrategy.PriceMonitor => "价格监控",
                MonitorStrategy.LowPriceBuy => "低价抢购",
                MonitorStrategy.StableBuy => "稳健收购",
                MonitorStrategy.BulkPurchase => "批量收购",
                MonitorStrategy.TrendWatch => "趋势观察",
                MonitorStrategy.TimedCheck => "定时检查",
                MonitorStrategy.Custom => "自定义",
                _ => strategy.ToString()
            };
        }

        /// <summary>
        /// 获取风险等级（1-5）
        /// </summary>
        public static int GetRiskLevel(this MonitorStrategy strategy)
        {
            return strategy switch
            {
                MonitorStrategy.PriceMonitor => 1,
                MonitorStrategy.LowPriceBuy => 3,
                MonitorStrategy.StableBuy => 1,
                MonitorStrategy.BulkPurchase => 2,
                MonitorStrategy.TrendWatch => 2,
                MonitorStrategy.TimedCheck => 1,
                MonitorStrategy.Custom => 0,
                _ => 0
            };
        }

        /// <summary>
        /// 获取风险等级星级显示
        /// </summary>
        public static string GetRiskStars(this MonitorStrategy strategy)
        {
            var level = strategy.GetRiskLevel();
            return level switch
            {
                0 => "-",
                1 => "⭐",
                2 => "⭐⭐",
                3 => "⭐⭐⭐",
                4 => "⭐⭐⭐⭐",
                5 => "⭐⭐⭐⭐⭐",
                _ => ""
            };
        }
    }

    /// <summary>
    /// 监控策略下拉列表项
    /// </summary>
    public class MonitorStrategyItem
    {
        public MonitorStrategy Strategy { get; set; }
        public string DisplayName => Strategy.GetDisplayName();
        public string Description => Strategy.GetDescription();
        public int RiskLevel => Strategy.GetRiskLevel();
        
        public override string ToString()
        {
            return Description;
        }
    }

    /// <summary>
    /// 监控策略辅助类
    /// </summary>
    public static class MonitorStrategyHelper
    {
        /// <summary>
        /// 获取所有监控策略
        /// </summary>
        public static List<MonitorStrategyItem> GetAllStrategies()
        {
            var strategies = new List<MonitorStrategyItem>();
            
            foreach (MonitorStrategy strategy in Enum.GetValues(typeof(MonitorStrategy)))
            {
                strategies.Add(new MonitorStrategyItem { Strategy = strategy });
            }
            
            return strategies;
        }

        /// <summary>
        /// 根据字符串获取策略
        /// </summary>
        public static MonitorStrategy? ParseStrategy(string? strategyString)
        {
            if (string.IsNullOrWhiteSpace(strategyString))
                return null;

            // 尝试按名称解析
            if (Enum.TryParse<MonitorStrategy>(strategyString, true, out var strategy))
            {
                return strategy;
            }

            // 尝试按显示名称匹配
            foreach (MonitorStrategy s in Enum.GetValues(typeof(MonitorStrategy)))
            {
                if (s.GetDisplayName().Equals(strategyString, StringComparison.OrdinalIgnoreCase))
                {
                    return s;
                }
            }

            return null;
        }

        /// <summary>
        /// 策略描述映射（兼容旧代码）
        /// </summary>
        public static Dictionary<MonitorStrategy, string> StrategyDescriptions => new()
        {
            { MonitorStrategy.PriceMonitor, "价格监控 - 监控价格是否在目标范围内" },
            { MonitorStrategy.LowPriceBuy, "低价抢购 - 发现低价立即购买" },
            { MonitorStrategy.StableBuy, "稳健收购 - 确保利润后购买" },
            { MonitorStrategy.BulkPurchase, "批量收购 - 大量购买囤货" },
            { MonitorStrategy.TrendWatch, "趋势观察 - 观察价格走势" },
            { MonitorStrategy.TimedCheck, "定时检查 - 定期检查不紧急" },
            { MonitorStrategy.Custom, "自定义... - 输入自定义策略" }
        };
    }
}


