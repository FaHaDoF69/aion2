using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Aion2Helper.Utils;

namespace Aion2Helper.Models;

/// <summary>
/// 监控历史记录模型
/// </summary>
[Table("monitoring_history")]
public class MonitoringHistory
{
    /// <summary>
    /// 记录ID
    /// </summary>
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    /// <summary>
    /// 机器码（用于区分不同机器的实例）
    /// </summary>
    [Column("machine_code")]
    [MaxLength(32)]
    [Required]
    public string MachineCode { get; set; } = MachineCodeHelper.GetMachineCode();

    /// <summary>
    /// 物品名称
    /// </summary>
    [Column("item_name")]
    [MaxLength(200)]
    [Required]
    public string ItemName { get; set; } = string.Empty;

    /// <summary>
    /// 物品等级
    /// </summary>
    [Column("item_level")]
    public int? ItemLevel { get; set; }

    /// <summary>
    /// 当前价格
    /// </summary>
    [Column("current_price")]
    [Precision(18, 2)]
    public decimal CurrentPrice { get; set; }

    /// <summary>
    /// 预期价格
    /// </summary>
    [Column("expected_price")]
    [Precision(18, 2)]
    public decimal ExpectedPrice { get; set; }

    /// <summary>
    /// 预期利润
    /// </summary>
    [Column("expected_profit")]
    [Precision(18, 2)]
    public decimal ExpectedProfit { get; set; }

    /// <summary>
    /// 利润率
    /// </summary>
    [Column("profit_rate")]
    [Precision(5, 4)]
    public decimal ProfitRate { get; set; }

    /// <summary>
    /// 风险等级
    /// </summary>
    [Column("risk_level")]
    public RiskLevel RiskLevel { get; set; }

    /// <summary>
    /// 监控策略
    /// </summary>
    [Column("strategy")]
    [MaxLength(50)]
    public string Strategy { get; set; } = string.Empty;

    /// <summary>
    /// 卖家名称
    /// </summary>
    [Column("seller_name")]
    [MaxLength(100)]
    public string? SellerName { get; set; }

    /// <summary>
    /// 物品数量
    /// </summary>
    [Column("quantity")]
    public int Quantity { get; set; } = 1;

    /// <summary>
    /// 发现时间
    /// </summary>
    [Column("discovered_at")]
    public DateTime DiscoveredAt { get; set; } = DateTime.Now;

    /// <summary>
    /// 是否已处理（购买或忽略）
    /// </summary>
    [Column("is_processed")]
    public bool IsProcessed { get; set; } = false;

    /// <summary>
    /// 处理状态
    /// </summary>
    [Column("process_status")]
    public MonitoringProcessStatus ProcessStatus { get; set; } = MonitoringProcessStatus.Pending;

    /// <summary>
    /// 处理时间
    /// </summary>
    [Column("processed_at")]
    public DateTime? ProcessedAt { get; set; }

    /// <summary>
    /// 关联的购买记录ID（如果已购买）
    /// </summary>
    [Column("purchase_record_id")]
    public long? PurchaseRecordId { get; set; }

    /// <summary>
    /// 监控规则ID（关联到监控物品）
    /// </summary>
    [Column("monitored_item_id")]
    public long? MonitoredItemId { get; set; }

    /// <summary>
    /// 价格偏差（相对于预期价格的偏差百分比）
    /// </summary>
    [Column("price_deviation")]
    [Precision(10, 4)]
    public decimal PriceDeviation { get; set; }

    /// <summary>
    /// 是否为异常价格
    /// </summary>
    [Column("is_abnormal_price")]
    public bool IsAbnormalPrice { get; set; } = false;

    /// <summary>
    /// 备注
    /// </summary>
    [Column("notes")]
    [MaxLength(500)]
    public string? Notes { get; set; }

    /// <summary>
    /// 关联的监控物品（导航属性）
    /// </summary>
    [ForeignKey("MonitoredItemId")]
    public virtual MonitoredItem? MonitoredItem { get; set; }

    /// <summary>
    /// 关联的购买记录（导航属性）
    /// </summary>
    [ForeignKey("PurchaseRecordId")]
    public virtual PurchaseRecord? PurchaseRecord { get; set; }

    public override string ToString()
    {
        return $"{DiscoveredAt:MM-dd HH:mm} | {ItemName} @ {CurrentPrice:N0} (预期: {ExpectedPrice:N0}, 利润: {ExpectedProfit:N0})";
    }
}

/// <summary>
/// 监控处理状态枚举
/// </summary>
public enum MonitoringProcessStatus
{
    /// <summary>
    /// 待处理
    /// </summary>
    Pending = 0,

    /// <summary>
    /// 已购买
    /// </summary>
    Purchased = 1,

    /// <summary>
    /// 已忽略
    /// </summary>
    Ignored = 2,

    /// <summary>
    /// 价格已变化
    /// </summary>
    PriceChanged = 3,

    /// <summary>
    /// 已售出（物品不再可用）
    /// </summary>
    SoldOut = 4,

    /// <summary>
    /// 自动处理失败
    /// </summary>
    AutoProcessFailed = 5
}
