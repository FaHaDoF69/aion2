using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Aion2Helper.Models
{
    /// <summary>
    /// 物品表
    /// </summary>
    [Table("items")]
    public class Item
    {
        /// <summary>
        /// 物品ID
        /// </summary>
        [Key]
        [Column("id")]
        public int Id { get; set; }

        /// <summary>
        /// 物品名称
        /// </summary>
        [Required]
        [MaxLength(100)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 物品分类ID
        /// </summary>
        [Required]
        [Column("category_id")]
        public int CategoryId { get; set; }

        /// <summary>
        /// 物品分类（导航属性）
        /// </summary>
        [ForeignKey("CategoryId")]
        public ItemCategory? Category { get; set; }

        /// <summary>
        /// 物品等级
        /// </summary>
        [Column("item_level")]
        public int? ItemLevel { get; set; }

        /// <summary>
        /// 物品品质（如：普通、高级、稀有、史诗、传说）
        /// </summary>
        [MaxLength(20)]
        [Column("quality")]
        public string? Quality { get; set; }

        /// <summary>
        /// 参考价格（市场平均价格）
        /// </summary>
        [Column("reference_price")]
        public decimal? ReferencePrice { get; set; }

        /// <summary>
        /// 物品描述
        /// </summary>
        [MaxLength(500)]
        [Column("description")]
        public string? Description { get; set; }

        /// <summary>
        /// 物品图标路径
        /// </summary>
        [MaxLength(255)]
        [Column("icon_path")]
        public string? IconPath { get; set; }

        /// <summary>
        /// 是否可交易
        /// </summary>
        [Column("is_tradable")]
        public bool IsTradable { get; set; } = true;

        /// <summary>
        /// 是否启用
        /// </summary>
        [Column("is_enabled")]
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// 排序顺序
        /// </summary>
        [Column("sort_order")]
        public int SortOrder { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [MaxLength(255)]
        [Column("remarks")]
        public string? Remarks { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// 更新时间
        /// </summary>
        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }
    }
}

