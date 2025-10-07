using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Aion2Helper.Utils;

namespace Aion2Helper.Models;

/// <summary>
/// 购买记录模型
/// </summary>
[Table("purchase_records")]
public class PurchaseRecord
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
    /// 购买价格
    /// </summary>
    [Column("price")]
    [Precision(18, 2)]
    public decimal Price { get; set; }

    /// <summary>
    /// 购买数量
    /// </summary>
    [Column("quantity")]
    public int Quantity { get; set; }

    /// <summary>
    /// 总金额
    /// </summary>
    [Column("total_amount")]
    [Precision(18, 2)]
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// 购买时间
    /// </summary>
    [Column("purchase_time")]
    public DateTime PurchaseTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 卖家名称
    /// </summary>
    [Column("seller_name")]
    [MaxLength(100)]
    public string? SellerName { get; set; }

    /// <summary>
    /// 预期利润
    /// </summary>
    [Column("expected_profit")]
    [Precision(18, 2)]
    public decimal ExpectedProfit { get; set; }

    /// <summary>
    /// 实际利润
    /// </summary>
    [Column("actual_profit")]
    [Precision(18, 2)]
    public decimal? ActualProfit { get; set; }

    /// <summary>
    /// 购买策略
    /// </summary>
    [Column("strategy")]
    [MaxLength(100)]
    public string Strategy { get; set; } = string.Empty;

    /// <summary>
    /// 执行状态
    /// </summary>
    [Column("status")]
    public PurchaseStatus Status { get; set; } = PurchaseStatus.Pending;

    /// <summary>
    /// 执行耗时（毫秒）
    /// </summary>
    [Column("execution_time_ms")]
    public long ExecutionTimeMs { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    [Column("notes")]
    [MaxLength(500)]
    public string? Notes { get; set; }

    public override string ToString()
    {
        return $"{PurchaseTime:MM-dd HH:mm} | {ItemName} x{Quantity} @ {Price:N0} = {TotalAmount:N0}";
    }
}

/// <summary>
/// 购买状态枚举
/// </summary>
public enum PurchaseStatus
{
    Pending = 0,    // 待处理
    Executing = 1,  // 执行中
    Completed = 2,  // 已完成
    Failed = 3,     // 失败
    Cancelled = 4   // 已取消
}
