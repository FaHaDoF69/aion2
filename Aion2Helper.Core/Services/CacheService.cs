using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Aion2Helper.Data;
using Aion2Helper.Models;
using Microsoft.EntityFrameworkCore;

namespace Aion2Helper.Services
{
    /// <summary>
    /// 缓存服务
    /// 用于缓存常用数据，减少数据库查询
    /// </summary>
    public class CacheService
    {
        private static CacheService? _instance;
        private static readonly object _lock = new object();

        private List<ItemCategory> _categories = new List<ItemCategory>();
        private List<Item> _items = new List<Item>();
        private List<MachineAuthorization> _machines = new List<MachineAuthorization>();
        private List<MonitoredItem> _monitoredItems = new List<MonitoredItem>();
        private DateTime? _categoriesLastUpdated;
        private DateTime? _itemsLastUpdated;
        private DateTime? _machinesLastUpdated;
        private DateTime? _monitoredItemsLastUpdated;

        /// <summary>
        /// 获取单例实例
        /// </summary>
        public static CacheService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new CacheService();
                        }
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// 私有构造函数
        /// </summary>
        private CacheService()
        {
        }

        #region 物品分类缓存

        /// <summary>
        /// 获取所有物品分类（从缓存）
        /// </summary>
        /// <returns>物品分类列表</returns>
        public List<ItemCategory> GetCategories()
        {
            return _categories.ToList();
        }

        /// <summary>
        /// 根据ID获取分类（从缓存）
        /// </summary>
        /// <param name="categoryId">分类ID</param>
        /// <returns>物品分类，未找到返回null</returns>
        public ItemCategory? GetCategoryById(int categoryId)
        {
            return _categories.FirstOrDefault(c => c.Id == categoryId);
        }

        /// <summary>
        /// 加载物品分类到缓存
        /// </summary>
        /// <returns>加载的分类数量</returns>
        public async Task<int> LoadCategoriesAsync()
        {
            try
            {
                using var context = new Aion2DbContext();
                using var itemService = new ItemService(context);

                _categories = await itemService.GetAllCategoriesAsync();
                _categoriesLastUpdated = DateTime.Now;

                #if DEBUG
                Console.WriteLine($"[缓存服务] 成功加载 {_categories.Count} 个物品分类到缓存");
                #endif

                return _categories.Count;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[缓存服务] 加载物品分类失败: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// 刷新物品分类缓存
        /// </summary>
        /// <returns>是否成功</returns>
        public async Task<bool> RefreshCategoriesAsync()
        {
            try
            {
                var count = await LoadCategoriesAsync();
                return count > 0;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 获取分类缓存最后更新时间
        /// </summary>
        public DateTime? CategoriesLastUpdated => _categoriesLastUpdated;

        #endregion

        #region 物品缓存

        /// <summary>
        /// 获取所有物品（从缓存）
        /// </summary>
        /// <returns>物品列表</returns>
        public List<Item> GetItems()
        {
            return _items.ToList();
        }

        /// <summary>
        /// 根据ID获取物品（从缓存）
        /// </summary>
        /// <param name="itemId">物品ID</param>
        /// <returns>物品信息，未找到返回null</returns>
        public Item? GetItemById(int itemId)
        {
            return _items.FirstOrDefault(i => i.Id == itemId);
        }

        /// <summary>
        /// 根据名称获取物品（从缓存）
        /// </summary>
        /// <param name="itemName">物品名称</param>
        /// <returns>物品信息，未找到返回null</returns>
        public Item? GetItemByName(string itemName)
        {
            return _items.FirstOrDefault(i => i.Name.Equals(itemName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// 根据分类ID获取物品（从缓存）
        /// </summary>
        /// <param name="categoryId">分类ID</param>
        /// <returns>物品列表</returns>
        public List<Item> GetItemsByCategory(int categoryId)
        {
            return _items.Where(i => i.CategoryId == categoryId).ToList();
        }

        /// <summary>
        /// 搜索物品（从缓存）
        /// </summary>
        /// <param name="keyword">关键词</param>
        /// <returns>物品列表</returns>
        public List<Item> SearchItems(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return _items.ToList();
            }

            return _items
                .Where(i => i.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                           (i.Description != null && i.Description.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        /// <summary>
        /// 加载物品到缓存
        /// </summary>
        /// <returns>加载的物品数量</returns>
        public async Task<int> LoadItemsAsync()
        {
            try
            {
                using var context = new Aion2DbContext();
                using var itemService = new ItemService(context);

                _items = await itemService.GetAllItemsAsync();
                _itemsLastUpdated = DateTime.Now;

                #if DEBUG
                Console.WriteLine($"[缓存服务] 成功加载 {_items.Count} 个物品到缓存");
                #endif

                return _items.Count;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[缓存服务] 加载物品失败: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// 刷新物品缓存
        /// </summary>
        /// <returns>是否成功</returns>
        public async Task<bool> RefreshItemsAsync()
        {
            try
            {
                var count = await LoadItemsAsync();
                return count > 0;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 获取物品缓存最后更新时间
        /// </summary>
        public DateTime? ItemsLastUpdated => _itemsLastUpdated;

        #endregion

        #region 机器授权缓存

        /// <summary>
        /// 获取所有机器授权（从缓存）
        /// </summary>
        /// <returns>机器授权列表</returns>
        public List<MachineAuthorization> GetMachines()
        {
            return _machines.ToList();
        }

        /// <summary>
        /// 根据机器码获取授权（从缓存）
        /// </summary>
        /// <param name="machineCode">机器码</param>
        /// <returns>机器授权信息，未找到返回null</returns>
        public MachineAuthorization? GetMachineByCode(string machineCode)
        {
            return _machines.FirstOrDefault(m => m.MachineCode == machineCode);
        }

        /// <summary>
        /// 获取启用的机器授权（从缓存）
        /// </summary>
        /// <returns>启用的机器授权列表</returns>
        public List<MachineAuthorization> GetEnabledMachines()
        {
            return _machines.Where(m => m.IsEnabled).ToList();
        }

        /// <summary>
        /// 加载机器授权到缓存
        /// </summary>
        /// <returns>加载的机器数量</returns>
        public async Task<int> LoadMachinesAsync()
        {
            try
            {
                using var context = new Aion2DbContext();
                
                _machines = await context.MachineAuthorizations
                    .OrderBy(m => m.MachineName)
                    .ThenBy(m => m.CreatedAt)
                    .ToListAsync();
                    
                _machinesLastUpdated = DateTime.Now;

                #if DEBUG
                Console.WriteLine($"[缓存服务] 成功加载 {_machines.Count} 个机器授权到缓存");
                #endif

                return _machines.Count;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[缓存服务] 加载机器授权失败: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// 刷新机器授权缓存
        /// </summary>
        /// <returns>是否成功</returns>
        public async Task<bool> RefreshMachinesAsync()
        {
            try
            {
                var count = await LoadMachinesAsync();
                return count >= 0; // 即使是0也算成功
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 获取机器授权缓存最后更新时间
        /// </summary>
        public DateTime? MachinesLastUpdated => _machinesLastUpdated;

        #endregion

        #region 监控物品缓存

        /// <summary>
        /// 获取所有监控物品（从缓存）
        /// </summary>
        /// <returns>监控物品列表</returns>
        public List<MonitoredItem> GetMonitoredItems()
        {
            return _monitoredItems.ToList();
        }

        /// <summary>
        /// 根据机器码获取监控物品（从缓存）
        /// </summary>
        /// <param name="machineCode">机器码</param>
        /// <returns>监控物品列表</returns>
        public List<MonitoredItem> GetMonitoredItemsByMachine(string machineCode)
        {
            return _monitoredItems.Where(m => m.MachineCode == machineCode).ToList();
        }

        /// <summary>
        /// 根据分类ID获取监控物品（从缓存）
        /// </summary>
        /// <param name="categoryId">分类ID</param>
        /// <returns>监控物品列表</returns>
        public List<MonitoredItem> GetMonitoredItemsByCategory(int categoryId)
        {
            return _monitoredItems.Where(m => m.CategoryId == categoryId).ToList();
        }

        /// <summary>
        /// 根据物品名称获取监控物品（从缓存）
        /// </summary>
        /// <param name="itemName">物品名称</param>
        /// <returns>监控物品列表</returns>
        public List<MonitoredItem> GetMonitoredItemsByName(string itemName)
        {
            return _monitoredItems.Where(m => m.ItemName == itemName).ToList();
        }

        /// <summary>
        /// 加载监控物品到缓存
        /// </summary>
        /// <returns>加载的监控物品数量</returns>
        public async Task<int> LoadMonitoredItemsAsync()
        {
            try
            {
                using var context = new Aion2DbContext();
                
                _monitoredItems = await context.MonitoredItems
                    .Include(m => m.ItemCategory)
                    .OrderByDescending(m => m.Priority)
                    .ThenBy(m => m.ItemName)
                    .ToListAsync();
                    
                _monitoredItemsLastUpdated = DateTime.Now;

                #if DEBUG
                Console.WriteLine($"[缓存服务] 成功加载 {_monitoredItems.Count} 个监控物品到缓存");
                #endif

                return _monitoredItems.Count;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[缓存服务] 加载监控物品失败: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// 刷新监控物品缓存
        /// </summary>
        /// <returns>是否成功</returns>
        public async Task<bool> RefreshMonitoredItemsAsync()
        {
            try
            {
                var count = await LoadMonitoredItemsAsync();
                return count >= 0;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 获取监控物品缓存最后更新时间
        /// </summary>
        public DateTime? MonitoredItemsLastUpdated => _monitoredItemsLastUpdated;

        #endregion

        #region 缓存管理

        /// <summary>
        /// 初始化所有缓存
        /// </summary>
        /// <returns>是否成功</returns>
        public async Task<bool> InitializeAsync()
        {
            try
            {
                #if DEBUG
                Console.WriteLine("[缓存服务] 开始初始化缓存...");
                #endif

                var categoriesTask = LoadCategoriesAsync();
                var itemsTask = LoadItemsAsync();
                var machinesTask = LoadMachinesAsync();
                var monitoredItemsTask = LoadMonitoredItemsAsync();

                await Task.WhenAll(categoriesTask, itemsTask, machinesTask, monitoredItemsTask);

                var categoriesCount = await categoriesTask;
                var itemsCount = await itemsTask;
                var machinesCount = await machinesTask;
                var monitoredItemsCount = await monitoredItemsTask;

                #if DEBUG
                Console.WriteLine($"[缓存服务] 缓存初始化完成 - 分类: {categoriesCount}, 物品: {itemsCount}, 机器: {machinesCount}, 监控物品: {monitoredItemsCount}");
                #endif

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[缓存服务] 缓存初始化失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 刷新所有缓存
        /// </summary>
        /// <returns>是否成功</returns>
        public async Task<bool> RefreshAllAsync()
        {
            try
            {
                #if DEBUG
                Console.WriteLine("[缓存服务] 开始刷新所有缓存...");
                #endif

                var categoriesTask = LoadCategoriesAsync();
                var itemsTask = LoadItemsAsync();
                var machinesTask = LoadMachinesAsync();
                var monitoredItemsTask = LoadMonitoredItemsAsync();

                await Task.WhenAll(categoriesTask, itemsTask, machinesTask, monitoredItemsTask);

                var categoriesCount = await categoriesTask;
                var itemsCount = await itemsTask;
                var machinesCount = await machinesTask;
                var monitoredItemsCount = await monitoredItemsTask;

                #if DEBUG
                Console.WriteLine($"[缓存服务] 缓存刷新完成 - 分类: {categoriesCount}, 物品: {itemsCount}, 机器: {machinesCount}, 监控物品: {monitoredItemsCount}");
                #endif

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[缓存服务] 缓存刷新失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 清空所有缓存
        /// </summary>
        public void ClearAll()
        {
            _categories.Clear();
            _items.Clear();
            _machines.Clear();
            _monitoredItems.Clear();
            _categoriesLastUpdated = null;
            _itemsLastUpdated = null;
            _machinesLastUpdated = null;
            _monitoredItemsLastUpdated = null;

            #if DEBUG
            Console.WriteLine("[缓存服务] 缓存已清空");
            #endif
        }

        /// <summary>
        /// 获取缓存统计信息
        /// </summary>
        /// <returns>统计信息字符串</returns>
        public string GetCacheStats()
        {
            return $"分类: {_categories.Count} 个 (更新于: {_categoriesLastUpdated?.ToString("yyyy-MM-dd HH:mm:ss") ?? "未加载"}), " +
                   $"物品: {_items.Count} 个 (更新于: {_itemsLastUpdated?.ToString("yyyy-MM-dd HH:mm:ss") ?? "未加载"}), " +
                   $"机器: {_machines.Count} 个 (更新于: {_machinesLastUpdated?.ToString("yyyy-MM-dd HH:mm:ss") ?? "未加载"}), " +
                   $"监控物品: {_monitoredItems.Count} 个 (更新于: {_monitoredItemsLastUpdated?.ToString("yyyy-MM-dd HH:mm:ss") ?? "未加载"})";
        }

        #endregion
    }
}

