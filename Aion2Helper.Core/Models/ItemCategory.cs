using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Aion2Helper.Models
{
    /// <summary>
    /// 物品分类
    /// </summary>
    [Table("item_categories")]
    public class ItemCategory
    {
        /// <summary>
        /// 分类ID
        /// </summary>
        [Key]
        [Column("id")]
        public int Id { get; set; }

        /// <summary>
        /// 分类名称
        /// </summary>
        [Required]
        [MaxLength(50)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 分类描述
        /// </summary>
        [MaxLength(200)]
        [Column("description")]
        public string? Description { get; set; }

        /// <summary>
        /// 排序顺序
        /// </summary>
        [Column("sort_order")]
        public int SortOrder { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        [Column("is_enabled")]
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// 创建时间
        /// </summary>
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}

