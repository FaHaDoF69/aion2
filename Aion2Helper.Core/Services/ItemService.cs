using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Aion2Helper.Data;
using Aion2Helper.Models;

namespace Aion2Helper.Services
{
    /// <summary>
    /// 物品服务类
    /// </summary>
    public class ItemService : IDisposable
    {
        private readonly Aion2DbContext _context;
        private bool _disposed = false;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="context">数据库上下文</param>
        public ItemService(Aion2DbContext context)
        {
            _context = context;
        }

        #region 物品分类相关

        /// <summary>
        /// 获取所有物品分类
        /// </summary>
        /// <returns>物品分类列表</returns>
        public async Task<List<ItemCategory>> GetAllCategoriesAsync()
        {
            return await _context.ItemCategories
                .Where(c => c.IsEnabled)
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.Name)
                .ToListAsync();
        }

        /// <summary>
        /// 根据ID获取分类
        /// </summary>
        /// <param name="id">分类ID</param>
        /// <returns>物品分类</returns>
        public async Task<ItemCategory?> GetCategoryByIdAsync(int id)
        {
            return await _context.ItemCategories.FindAsync(id);
        }

        #endregion

        #region 物品CRUD操作

        /// <summary>
        /// 获取所有物品（包含分类信息）
        /// </summary>
        /// <returns>物品列表</returns>
        public async Task<List<Item>> GetAllItemsAsync()
        {
            return await _context.Items
                .Include(i => i.Category)
                .OrderBy(i => i.SortOrder)
                .ThenBy(i => i.Name)
                .ToListAsync();
        }

        /// <summary>
        /// 根据分类ID获取物品
        /// </summary>
        /// <param name="categoryId">分类ID</param>
        /// <returns>物品列表</returns>
        public async Task<List<Item>> GetItemsByCategoryAsync(int categoryId)
        {
            return await _context.Items
                .Include(i => i.Category)
                .Where(i => i.CategoryId == categoryId)
                .OrderBy(i => i.SortOrder)
                .ThenBy(i => i.Name)
                .ToListAsync();
        }

        /// <summary>
        /// 根据名称搜索物品
        /// </summary>
        /// <param name="keyword">搜索关键词</param>
        /// <returns>物品列表</returns>
        public async Task<List<Item>> SearchItemsAsync(string keyword)
        {
            return await _context.Items
                .Include(i => i.Category)
                .Where(i => i.Name.Contains(keyword))
                .OrderBy(i => i.SortOrder)
                .ThenBy(i => i.Name)
                .ToListAsync();
        }

        /// <summary>
        /// 根据ID获取物品
        /// </summary>
        /// <param name="id">物品ID</param>
        /// <returns>物品信息</returns>
        public async Task<Item?> GetItemByIdAsync(int id)
        {
            return await _context.Items
                .Include(i => i.Category)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        /// <summary>
        /// 添加物品
        /// </summary>
        /// <param name="item">物品信息</param>
        /// <returns>是否成功</returns>
        public async Task<bool> AddItemAsync(Item item)
        {
            try
            {
                _context.Items.Add(item);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"添加物品失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 更新物品
        /// </summary>
        /// <param name="item">物品信息</param>
        /// <returns>是否成功</returns>
        public async Task<bool> UpdateItemAsync(Item item)
        {
            try
            {
                _context.Items.Update(item);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"更新物品失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 删除物品
        /// </summary>
        /// <param name="id">物品ID</param>
        /// <returns>是否成功</returns>
        public async Task<bool> DeleteItemAsync(int id)
        {
            try
            {
                var item = await _context.Items.FindAsync(id);
                if (item == null)
                {
                    return false;
                }

                _context.Items.Remove(item);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"删除物品失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 检查物品名称是否存在
        /// </summary>
        /// <param name="name">物品名称</param>
        /// <param name="excludeId">排除的物品ID（用于更新时检查）</param>
        /// <returns>是否存在</returns>
        public async Task<bool> IsItemNameExistsAsync(string name, int? excludeId = null)
        {
            var query = _context.Items.Where(i => i.Name == name);
            
            if (excludeId.HasValue)
            {
                query = query.Where(i => i.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        #endregion

        #region IDisposable 实现

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="disposing">是否释放托管资源</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // 释放托管资源
                    // 注意：由于 context 是通过构造函数传入的，不应该在这里释放
                    // 释放工作由调用者负责
                }

                _disposed = true;
            }
        }

        #endregion
    }
}

