using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Aion2Helper.Utils;

namespace Aion2Helper.Models;

/// <summary>
/// 被监控物品模型
/// </summary>
[Table("monitored_items")]
public class MonitoredItem
{
    /// <summary>
    /// 主键ID
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
    /// 物品类别（字符串，已废弃，使用 CategoryId 代替）
    /// </summary>
    [Obsolete("使用 CategoryId 和 ItemCategory 代替")]
    [Column("category")]
    [MaxLength(50)]
    public string? Category { get; set; }

    /// <summary>
    /// 物品分类ID（关联到物品分类表）
    /// </summary>
    [Column("category_id")]
    public int? CategoryId { get; set; }

    /// <summary>
    /// 物品分类（导航属性）
    /// </summary>
    [ForeignKey("CategoryId")]
    public ItemCategory? ItemCategory { get; set; }

    /// <summary>
    /// 物品等级
    /// </summary>
    [Column("item_level")]
    public int? ItemLevel { get; set; }

    /// <summary>
    /// 目标最低价格（低于此价格时触发购买）
    /// </summary>
    [Column("target_min_price")]
    [Precision(18, 2)]
    public decimal? TargetMinPrice { get; set; }

    /// <summary>
    /// 目标最高价格（高于此价格时不购买）
    /// </summary>
    [Column("target_max_price")]
    [Precision(18, 2)]
    public decimal? TargetMaxPrice { get; set; }

    /// <summary>
    /// 优先级（1-10，数字越大优先级越高）
    /// </summary>
    [Column("priority")]
    public int Priority { get; set; } = 5;

    /// <summary>
    /// 是否启用监控
    /// </summary>
    [Column("is_enabled")]
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// 是否启用自动购买
    /// </summary>
    [Column("auto_purchase_enabled")]
    public bool AutoPurchaseEnabled { get; set; } = false;

    /// <summary>
    /// 监控策略
    /// </summary>
    [Column("monitor_strategy")]
    [MaxLength(50)]
    public string MonitorStrategy { get; set; } = "价格监控";

    /// <summary>
    /// 创建时间
    /// </summary>
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// 更新时间
    /// </summary>
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// 最后监控时间
    /// </summary>
    [Column("last_monitored_at")]
    public DateTime? LastMonitoredAt { get; set; }

    /// <summary>
    /// 最后发现时间
    /// </summary>
    [Column("last_found_at")]
    public DateTime? LastFoundAt { get; set; }

    /// <summary>
    /// 最后发现的价格
    /// </summary>
    [Column("last_found_price")]
    [Precision(18, 2)]
    public decimal? LastFoundPrice { get; set; }

    /// <summary>
    /// 总发现次数
    /// </summary>
    [Column("total_found_count")]
    public int TotalFoundCount { get; set; } = 0;

    /// <summary>
    /// 总购买次数
    /// </summary>
    [Column("total_purchase_count")]
    public int TotalPurchaseCount { get; set; } = 0;

    /// <summary>
    /// 备注
    /// </summary>
    [Column("notes")]
    [MaxLength(500)]
    public string? Notes { get; set; }

    /// <summary>
    /// 是否在目标价格范围内
    /// </summary>
    public bool IsInTargetPriceRange(decimal price)
    {
        if (TargetMinPrice.HasValue && price < TargetMinPrice.Value)
            return false;
        
        if (TargetMaxPrice.HasValue && price > TargetMaxPrice.Value)
            return false;
        
        return true;
    }

    /// <summary>
    /// 计算期望利润（基于价格范围）
    /// </summary>
    public decimal CalculateExpectedProfit(decimal currentPrice)
    {
        // 如果设置了最高价格，使用最高价格作为预期售价
        if (TargetMaxPrice.HasValue && TargetMaxPrice.Value > currentPrice)
        {
            return TargetMaxPrice.Value - currentPrice;
        }
        
        // 否则使用默认20%的利润率
        return currentPrice * 0.2m;
    }

    /// <summary>
    /// 获取显示名称
    /// </summary>
    [NotMapped]
    public string DisplayName => ItemCategory != null ? $"{ItemName} ({ItemCategory.Name})" : ItemName;

    public override string ToString()
    {
        var status = IsEnabled ? "启用" : "禁用";
        var autoBuy = AutoPurchaseEnabled ? "自动购买" : "仅监控";
        return $"{DisplayName} - {status} - {autoBuy} - 优先级:{Priority}";
    }
}
