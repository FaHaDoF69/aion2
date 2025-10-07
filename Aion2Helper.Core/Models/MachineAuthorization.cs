using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Aion2Helper.Models
{
    /// <summary>
    /// 机器授权表
    /// 用于控制哪些机器可以运行客户端
    /// </summary>
    [Table("machine_authorizations")]
    public class MachineAuthorization
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        [Key]
        [Column("id")]
        public long Id { get; set; }

        /// <summary>
        /// 机器码
        /// </summary>
        [Required]
        [MaxLength(100)]
        [Column("machine_code")]
        public string MachineCode { get; set; } = string.Empty;

        /// <summary>
        /// 是否启用
        /// </summary>
        [Column("is_enabled")]
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// 授权开始时间
        /// </summary>
        [Column("start_time")]
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// 授权结束时间
        /// </summary>
        [Column("end_time")]
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// 机器名称/备注
        /// </summary>
        [MaxLength(100)]
        [Column("machine_name")]
        public string? MachineName { get; set; }

        /// <summary>
        /// 最大并发运行数
        /// </summary>
        [Column("max_concurrent_runs")]
        public int MaxConcurrentRuns { get; set; } = 1;

        /// <summary>
        /// 备注说明
        /// </summary>
        [MaxLength(500)]
        [Column("notes")]
        public string? Notes { get; set; }

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
        /// 最后使用时间
        /// </summary>
        [Column("last_used_at")]
        public DateTime? LastUsedAt { get; set; }

        /// <summary>
        /// 检查当前是否在授权期内
        /// </summary>
        public bool IsInAuthorizationPeriod()
        {
            var now = DateTime.Now;
            
            if (!IsEnabled)
                return false;
            
            if (StartTime.HasValue && now < StartTime.Value)
                return false;
            
            if (EndTime.HasValue && now > EndTime.Value)
                return false;
            
            return true;
        }
    }
}

