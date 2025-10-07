using System;

namespace Aion2Helper.Models
{
    /// <summary>
    /// AI配置模型
    /// </summary>
    public class AIConfiguration
    {
        public int Id { get; set; }
        public string MachineCode { get; set; } = string.Empty;
        
        // AI模式设置
        public string AIMode { get; set; } = "AI辅助决策";
        public bool AutoBuyEnabled { get; set; } = false;
        public int AutoBuyMinScore { get; set; } = 75;
        public int AutoIgnoreMaxScore { get; set; } = 60;
        
        // 权重配置
        public int PriceWeight { get; set; } = 30;
        public int MarketWeight { get; set; } = 20;
        public int ProfitWeight { get; set; } = 30;
        public int TimingWeight { get; set; } = 10;
        public int HistoryWeight { get; set; } = 10;
        
        // 安全限制
        public decimal MaxSingleInvestment { get; set; } = 5000000;
        public decimal RequireManualConfirmAmount { get; set; } = 1000000;
        public bool EnableAIBlacklist { get; set; } = true;
        
        // 触发条件
        public bool TriggerOnStrategy { get; set; } = true;
        public bool TriggerOnEveryCheck { get; set; } = false;
        public bool ManualTriggerOnly { get; set; } = false;
        
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        
        /// <summary>
        /// 验证权重总和是否为100
        /// </summary>
        public bool ValidateWeights()
        {
            return (PriceWeight + MarketWeight + ProfitWeight + TimingWeight + HistoryWeight) == 100;
        }
    }
    
    /// <summary>
    /// AI分析模式枚举
    /// </summary>
    public enum AIAnalysisMode
    {
        /// <summary>
        /// 不使用AI
        /// </summary>
        Disabled,
        
        /// <summary>
        /// AI建议 - 显示建议，用户决定
        /// </summary>
        Suggestion,
        
        /// <summary>
        /// AI辅助决策 - AI+规则综合（推荐）
        /// </summary>
        Assisted,
        
        /// <summary>
        /// AI自动决策 - 完全由AI决定
        /// </summary>
        Automatic,
        
        /// <summary>
        /// AI深度分析 - 深度学习模型
        /// </summary>
        DeepAnalysis
    }
}

