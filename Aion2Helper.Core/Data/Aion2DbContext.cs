using Microsoft.EntityFrameworkCore;
using Aion2Helper.Models;
using Aion2Helper.Config;

namespace Aion2Helper.Data;

/// <summary>
/// Aion2 数据库上下文
/// </summary>
public class Aion2DbContext : DbContext
{
    /// <summary>
    /// 拍卖行物品数据集
    /// </summary>
    public DbSet<AuctionItem> AuctionItems { get; set; }

    /// <summary>
    /// 购买记录数据集
    /// </summary>
    public DbSet<PurchaseRecord> PurchaseRecords { get; set; }

    /// <summary>
    /// 被监控物品数据集
    /// </summary>
    public DbSet<MonitoredItem> MonitoredItems { get; set; }

    /// <summary>
    /// 监控历史记录数据集
    /// </summary>
    public DbSet<MonitoringHistory> MonitoringHistory { get; set; }

    /// <summary>
    /// 物品分类数据集
    /// </summary>
    public DbSet<ItemCategory> ItemCategories { get; set; }

    /// <summary>
    /// 物品数据集
    /// </summary>
    public DbSet<Item> Items { get; set; }

    /// <summary>
    /// 机器授权数据集
    /// </summary>
    public DbSet<MachineAuthorization> MachineAuthorizations { get; set; }

    /// <summary>
    /// AI配置数据集
    /// </summary>
    public DbSet<AIConfiguration> AIConfigurations { get; set; }

    /// <summary>
    /// 构造函数
    /// </summary>
    public Aion2DbContext() : base()
    {
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="options">数据库上下文选项</param>
    public Aion2DbContext(DbContextOptions<Aion2DbContext> options) : base(options)
    {
    }

    /// <summary>
    /// 配置数据库连接
    /// </summary>
    /// <param name="optionsBuilder">选项构建器</param>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var serverVersion = new MySqlServerVersion(new Version(8, 0, 21));
            optionsBuilder.UseMySql(DatabaseConfig.ConnectionString, serverVersion, options =>
            {
                options.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
            });

            // 在开发环境启用敏感数据日志记录
            #if DEBUG
            optionsBuilder.EnableSensitiveDataLogging();
            optionsBuilder.EnableDetailedErrors();
            #endif
        }
    }

    /// <summary>
    /// 配置实体模型
    /// </summary>
    /// <param name="modelBuilder">模型构建器</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 配置 AuctionItem 实体
        modelBuilder.Entity<AuctionItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();

            entity.Property(e => e.MachineCode)
                .IsRequired()
                .HasMaxLength(32)
                .HasCharSet("utf8mb4")
                .HasCollation("utf8mb4_unicode_ci");
            
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200)
                .HasCharSet("utf8mb4")
                .HasCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.SellerName)
                .HasMaxLength(100)
                .HasCharSet("utf8mb4")
                .HasCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.Price)
                .HasPrecision(18, 2);

            entity.Property(e => e.PriceDeviation)
                .HasPrecision(10, 4);

            entity.Property(e => e.DiscoveredAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // 创建索引
            entity.HasIndex(e => e.MachineCode);
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.Price);
            entity.HasIndex(e => e.DiscoveredAt);
            entity.HasIndex(e => e.IsAbnormalPrice);
            // 创建复合索引，用于按机器码和时间查询
            entity.HasIndex(e => new { e.MachineCode, e.DiscoveredAt });
            // 创建复合索引，用于按机器码和物品名称查询
            entity.HasIndex(e => new { e.MachineCode, e.Name });
        });

        // 配置 PurchaseRecord 实体
        modelBuilder.Entity<PurchaseRecord>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();

            entity.Property(e => e.MachineCode)
                .IsRequired()
                .HasMaxLength(32)
                .HasCharSet("utf8mb4")
                .HasCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.ItemName)
                .IsRequired()
                .HasMaxLength(200)
                .HasCharSet("utf8mb4")
                .HasCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.SellerName)
                .HasMaxLength(100)
                .HasCharSet("utf8mb4")
                .HasCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.Strategy)
                .HasMaxLength(100)
                .HasCharSet("utf8mb4")
                .HasCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.Notes)
                .HasMaxLength(500)
                .HasCharSet("utf8mb4")
                .HasCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.Price)
                .HasPrecision(18, 2);

            entity.Property(e => e.TotalAmount)
                .HasPrecision(18, 2);

            entity.Property(e => e.ExpectedProfit)
                .HasPrecision(18, 2);

            entity.Property(e => e.ActualProfit)
                .HasPrecision(18, 2);

            entity.Property(e => e.PurchaseTime)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.Status)
                .HasConversion<int>();

            // 配置外键关系
            entity.HasOne(e => e.ItemCategory)
                .WithMany()
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            // 创建索引
            entity.HasIndex(e => e.MachineCode);
            entity.HasIndex(e => e.ItemName);
            entity.HasIndex(e => e.PurchaseTime);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.Strategy);
            entity.HasIndex(e => e.CategoryId);
            // 创建复合索引，用于按机器码和时间查询
            entity.HasIndex(e => new { e.MachineCode, e.PurchaseTime });
            // 创建复合索引，用于按机器码和状态查询
            entity.HasIndex(e => new { e.MachineCode, e.Status });
            // 创建复合索引，用于按分类和时间查询
            entity.HasIndex(e => new { e.CategoryId, e.PurchaseTime });
        });

        // 配置 MonitoredItem 实体
        modelBuilder.Entity<MonitoredItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();

            entity.Property(e => e.MachineCode)
                .IsRequired()
                .HasMaxLength(32)
                .HasCharSet("utf8mb4")
                .HasCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.ItemName)
                .IsRequired()
                .HasMaxLength(200)
                .HasCharSet("utf8mb4")
                .HasCollation("utf8mb4_unicode_ci");

            // Category 字段已废弃，不映射到数据库
            entity.Ignore(e => e.Category);

            entity.Property(e => e.ItemLevel);

            entity.Property(e => e.MonitorStrategy)
                .HasMaxLength(50)
                .HasCharSet("utf8mb4")
                .HasCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.Notes)
                .HasMaxLength(500)
                .HasCharSet("utf8mb4")
                .HasCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.TargetMinPrice)
                .HasPrecision(18, 2);

            entity.Property(e => e.TargetMaxPrice)
                .HasPrecision(18, 2);

            entity.Property(e => e.LastFoundPrice)
                .HasPrecision(18, 2);

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");

            // 外键关系
            entity.HasOne(e => e.ItemCategory)
                .WithMany()
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            // 创建索引
            entity.HasIndex(e => e.MachineCode);
            entity.HasIndex(e => e.ItemName);
            entity.HasIndex(e => e.IsEnabled);
            entity.HasIndex(e => e.Priority);
            // entity.HasIndex(e => e.Category); // 已废弃，使用 CategoryId 代替
            entity.HasIndex(e => e.CategoryId);
            
            // 创建复合索引
            entity.HasIndex(e => new { e.MachineCode, e.IsEnabled });
            entity.HasIndex(e => new { e.MachineCode, e.ItemName });
            entity.HasIndex(e => new { e.MachineCode, e.Priority, e.IsEnabled });
            entity.HasIndex(e => new { e.MachineCode, e.CategoryId });
        });

        // 配置 MonitoringHistory 实体
        modelBuilder.Entity<MonitoringHistory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();

            entity.Property(e => e.MachineCode)
                .IsRequired()
                .HasMaxLength(32)
                .HasCharSet("utf8mb4")
                .UseCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.ItemName)
                .IsRequired()
                .HasMaxLength(200)
                .HasCharSet("utf8mb4")
                .UseCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.ItemLevel);

            entity.Property(e => e.Strategy)
                .HasMaxLength(50)
                .HasCharSet("utf8mb4")
                .UseCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.SellerName)
                .HasMaxLength(100)
                .HasCharSet("utf8mb4")
                .UseCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.Notes)
                .HasMaxLength(500)
                .HasCharSet("utf8mb4")
                .UseCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.CurrentPrice)
                .HasPrecision(18, 2);

            entity.Property(e => e.ExpectedPrice)
                .HasPrecision(18, 2);

            entity.Property(e => e.ExpectedProfit)
                .HasPrecision(18, 2);

            entity.Property(e => e.ProfitRate)
                .HasPrecision(5, 4);

            entity.Property(e => e.PriceDeviation)
                .HasPrecision(10, 4);

            entity.Property(e => e.DiscoveredAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.RiskLevel)
                .HasConversion<int>();

            entity.Property(e => e.ProcessStatus)
                .HasConversion<int>();

            // 创建索引
            entity.HasIndex(e => e.MachineCode);
            entity.HasIndex(e => e.ItemName);
            entity.HasIndex(e => e.DiscoveredAt);
            entity.HasIndex(e => e.IsProcessed);
            entity.HasIndex(e => e.ProcessStatus);
            entity.HasIndex(e => e.IsAbnormalPrice);
            
            // 创建复合索引
            entity.HasIndex(e => new { e.MachineCode, e.DiscoveredAt });
            entity.HasIndex(e => new { e.MachineCode, e.IsProcessed });
            entity.HasIndex(e => new { e.MachineCode, e.ProcessStatus });
            entity.HasIndex(e => new { e.MachineCode, e.ItemName });
        });

        // 配置 ItemCategory 实体
        modelBuilder.Entity<ItemCategory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(50)
                .HasCharSet("utf8mb4")
                .UseCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.Description)
                .HasMaxLength(200)
                .HasCharSet("utf8mb4")
                .UseCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // 创建索引
            entity.HasIndex(e => e.Name).IsUnique();
            entity.HasIndex(e => e.SortOrder);
            entity.HasIndex(e => e.IsEnabled);
        });

        // 配置 Item 实体
        modelBuilder.Entity<Item>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100)
                .HasCharSet("utf8mb4")
                .UseCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.Quality)
                .HasMaxLength(20)
                .HasCharSet("utf8mb4")
                .UseCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .HasCharSet("utf8mb4")
                .UseCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.IconPath)
                .HasMaxLength(255);

            entity.Property(e => e.Remarks)
                .HasMaxLength(255)
                .HasCharSet("utf8mb4")
                .UseCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.ReferencePrice)
                .HasPrecision(18, 2);

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");

            // 外键关系
            entity.HasOne(e => e.Category)
                .WithMany()
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // 创建索引
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.CategoryId);
            entity.HasIndex(e => e.IsEnabled);
            entity.HasIndex(e => new { e.CategoryId, e.Name });
        });

        // 配置 MachineAuthorization 实体
        modelBuilder.Entity<MachineAuthorization>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();

            entity.Property(e => e.MachineCode)
                .IsRequired()
                .HasMaxLength(100)
                .HasCharSet("utf8mb4")
                .UseCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.MachineName)
                .HasMaxLength(100)
                .HasCharSet("utf8mb4")
                .UseCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.Notes)
                .HasMaxLength(500)
                .HasCharSet("utf8mb4")
                .UseCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");

            // 创建唯一索引
            entity.HasIndex(e => e.MachineCode).IsUnique();
            
            // 创建普通索引
            entity.HasIndex(e => e.IsEnabled);
            entity.HasIndex(e => new { e.IsEnabled, e.MachineCode });
        });

        // 配置 AIConfiguration 实体
        modelBuilder.Entity<AIConfiguration>(entity =>
        {
            entity.ToTable("ai_configurations");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd().HasColumnName("id");

            entity.Property(e => e.MachineCode)
                .HasColumnName("machine_code")
                .IsRequired()
                .HasMaxLength(100)
                .HasCharSet("utf8mb4")
                .UseCollation("utf8mb4_unicode_ci");

            // AI模式设置
            entity.Property(e => e.AIMode)
                .HasColumnName("ai_mode")
                .IsRequired()
                .HasMaxLength(50)
                .HasCharSet("utf8mb4")
                .UseCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.AutoBuyEnabled).HasColumnName("auto_buy_enabled");
            entity.Property(e => e.AutoBuyMinScore).HasColumnName("auto_buy_min_score");
            entity.Property(e => e.AutoIgnoreMaxScore).HasColumnName("auto_ignore_max_score");

            // 权重配置
            entity.Property(e => e.PriceWeight).HasColumnName("price_weight");
            entity.Property(e => e.MarketWeight).HasColumnName("market_weight");
            entity.Property(e => e.ProfitWeight).HasColumnName("profit_weight");
            entity.Property(e => e.TimingWeight).HasColumnName("timing_weight");
            entity.Property(e => e.HistoryWeight).HasColumnName("history_weight");

            // 安全限制
            entity.Property(e => e.MaxSingleInvestment)
                .HasColumnName("max_single_investment")
                .HasPrecision(15, 2);

            entity.Property(e => e.RequireManualConfirmAmount)
                .HasColumnName("require_manual_confirm_amount")
                .HasPrecision(15, 2);

            entity.Property(e => e.EnableAIBlacklist).HasColumnName("enable_ai_blacklist");

            // 触发条件
            entity.Property(e => e.TriggerOnStrategy).HasColumnName("trigger_on_strategy");
            entity.Property(e => e.TriggerOnEveryCheck).HasColumnName("trigger_on_every_check");
            entity.Property(e => e.ManualTriggerOnly).HasColumnName("manual_trigger_only");

            // 状态和时间
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            
            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");

            // 创建唯一索引
            entity.HasIndex(e => e.MachineCode).IsUnique();
            
            // 创建普通索引
            entity.HasIndex(e => e.IsActive);
        });
    }

    /// <summary>
    /// 保存更改前的处理
    /// </summary>
    /// <returns></returns>
    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    /// <summary>
    /// 异步保存更改前的处理
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return await base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// 更新时间戳
    /// </summary>
    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.Entity is AuctionItem auctionItem && entry.State == EntityState.Added)
            {
                auctionItem.DiscoveredAt = DateTime.Now;
            }

            if (entry.Entity is PurchaseRecord purchaseRecord && entry.State == EntityState.Added)
            {
                purchaseRecord.PurchaseTime = DateTime.Now;
            }

            if (entry.Entity is MonitoredItem monitoredItem)
            {
                if (entry.State == EntityState.Added)
                {
                    monitoredItem.CreatedAt = DateTime.Now;
                    monitoredItem.UpdatedAt = DateTime.Now;
                }
                else if (entry.State == EntityState.Modified)
                {
                    monitoredItem.UpdatedAt = DateTime.Now;
                }
            }

            if (entry.Entity is Item item)
            {
                if (entry.State == EntityState.Added)
                {
                    item.CreatedAt = DateTime.Now;
                    item.UpdatedAt = DateTime.Now;
                }
                else if (entry.State == EntityState.Modified)
                {
                    item.UpdatedAt = DateTime.Now;
                }
            }

            if (entry.Entity is ItemCategory itemCategory && entry.State == EntityState.Added)
            {
                itemCategory.CreatedAt = DateTime.Now;
            }

            if (entry.Entity is MachineAuthorization machineAuth)
            {
                if (entry.State == EntityState.Added)
                {
                    machineAuth.CreatedAt = DateTime.Now;
                    machineAuth.UpdatedAt = DateTime.Now;
                }
                else if (entry.State == EntityState.Modified)
                {
                    machineAuth.UpdatedAt = DateTime.Now;
                }
            }

            if (entry.Entity is AIConfiguration aiConfig)
            {
                if (entry.State == EntityState.Added)
                {
                    aiConfig.CreatedAt = DateTime.Now;
                    aiConfig.UpdatedAt = DateTime.Now;
                }
                else if (entry.State == EntityState.Modified)
                {
                    aiConfig.UpdatedAt = DateTime.Now;
                }
            }
        }
    }
}
