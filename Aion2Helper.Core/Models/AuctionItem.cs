using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Aion2Helper.Utils;

namespace Aion2Helper.Models;

/// <summary>
/// 拍卖行物品模型
/// </summary>
[Table("auction_items")]
public class AuctionItem
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
    [Column("name")]
    [MaxLength(200)]
    [Required]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 价格
    /// </summary>
    [Column("price")]
    [Precision(18, 2)]
    public decimal Price { get; set; }

    /// <summary>
    /// 数量
    /// </summary>
    [Column("quantity")]
    public int Quantity { get; set; }

    /// <summary>
    /// 卖家名称
    /// </summary>
    [Column("seller_name")]
    [MaxLength(100)]
    public string SellerName { get; set; } = string.Empty;

    /// <summary>
    /// 剩余时间（小时）
    /// </summary>
    [Column("remaining_hours")]
    public int RemainingHours { get; set; }

    /// <summary>
    /// 发现时间
    /// </summary>
    [Column("discovered_at")]
    public DateTime DiscoveredAt { get; set; } = DateTime.Now;

    /// <summary>
    /// 屏幕位置X坐标
    /// </summary>
    [Column("position_x")]
    public int PositionX { get; set; }

    /// <summary>
    /// 屏幕位置Y坐标
    /// </summary>
    [Column("position_y")]
    public int PositionY { get; set; }

    /// <summary>
    /// 屏幕位置
    /// </summary>
    [NotMapped]
    public System.Drawing.Point Position 
    { 
        get => new(PositionX, PositionY);
        set { PositionX = value.X; PositionY = value.Y; }
    }

    /// <summary>
    /// 是否为异常低价
    /// </summary>
    [Column("is_abnormal_price")]
    public bool IsAbnormalPrice { get; set; }

    /// <summary>
    /// 价格偏差百分比
    /// </summary>
    [Column("price_deviation")]
    public double PriceDeviation { get; set; }

    /// <summary>
    /// 总价值
    /// </summary>
    public decimal TotalValue => Price * Quantity;

    public override string ToString()
    {
        return $"{Name} x{Quantity} @ {Price:N0} ({SellerName})";
    }
}
