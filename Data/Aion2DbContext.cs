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

            // 创建索引
            entity.HasIndex(e => e.MachineCode);
            entity.HasIndex(e => e.ItemName);
            entity.HasIndex(e => e.PurchaseTime);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.Strategy);
            // 创建复合索引，用于按机器码和时间查询
            entity.HasIndex(e => new { e.MachineCode, e.PurchaseTime });
            // 创建复合索引，用于按机器码和状态查询
            entity.HasIndex(e => new { e.MachineCode, e.Status });
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

            entity.Property(e => e.Category)
                .HasMaxLength(50)
                .HasCharSet("utf8mb4")
                .HasCollation("utf8mb4_unicode_ci");

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

            // 创建索引
            entity.HasIndex(e => e.MachineCode);
            entity.HasIndex(e => e.ItemName);
            entity.HasIndex(e => e.IsEnabled);
            entity.HasIndex(e => e.Priority);
            entity.HasIndex(e => e.Category);
            
            // 创建复合索引
            entity.HasIndex(e => new { e.MachineCode, e.IsEnabled });
            entity.HasIndex(e => new { e.MachineCode, e.ItemName });
            entity.HasIndex(e => new { e.MachineCode, e.Priority, e.IsEnabled });
            entity.HasIndex(e => new { e.ItemName, e.Category });
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
            entity.HasIndex(e => e.MonitoredItemId);
            entity.HasIndex(e => e.PurchaseRecordId);
            entity.HasIndex(e => e.IsAbnormalPrice);
            
            // 创建复合索引
            entity.HasIndex(e => new { e.MachineCode, e.DiscoveredAt });
            entity.HasIndex(e => new { e.MachineCode, e.IsProcessed });
            entity.HasIndex(e => new { e.MachineCode, e.ProcessStatus });
            entity.HasIndex(e => new { e.MachineCode, e.ItemName });
            entity.HasIndex(e => new { e.MonitoredItemId, e.DiscoveredAt });

            // 配置外键关系
            entity.HasOne(e => e.MonitoredItem)
                .WithMany()
                .HasForeignKey(e => e.MonitoredItemId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.PurchaseRecord)
                .WithMany()
                .HasForeignKey(e => e.PurchaseRecordId)
                .OnDelete(DeleteBehavior.SetNull);
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
        }
    }
}
