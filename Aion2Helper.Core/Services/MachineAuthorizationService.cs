using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Aion2Helper.Data;
using Aion2Helper.Models;
using Aion2Helper.Utils;

namespace Aion2Helper.Services
{
    /// <summary>
    /// 机器授权服务
    /// </summary>
    public class MachineAuthorizationService : IDisposable
    {
        private readonly Aion2DbContext _context;
        private bool _disposed = false;

        public MachineAuthorizationService(Aion2DbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 检查当前机器是否有运行权限
        /// </summary>
        /// <returns>授权检查结果</returns>
        public async Task<AuthorizationCheckResult> CheckCurrentMachineAuthorizationAsync()
        {
            try
            {
                var currentMachineCode = MachineCodeHelper.GetMachineCode();
                
                var authorization = await _context.MachineAuthorizations
                    .FirstOrDefaultAsync(x => x.MachineCode == currentMachineCode);

                if (authorization == null)
                {
                    return new AuthorizationCheckResult
                    {
                        IsAuthorized = false,
                        Message = "该机器未授权，请联系管理员添加授权",
                        MachineCode = currentMachineCode
                    };
                }

                if (!authorization.IsEnabled)
                {
                    return new AuthorizationCheckResult
                    {
                        IsAuthorized = false,
                        Message = "该机器授权已被禁用，请联系管理员",
                        MachineCode = currentMachineCode,
                        Authorization = authorization
                    };
                }

                if (!authorization.IsInAuthorizationPeriod())
                {
                    var reason = "授权已过期";
                    if (authorization.StartTime.HasValue && DateTime.Now < authorization.StartTime.Value)
                    {
                        reason = $"授权尚未开始，开始时间: {authorization.StartTime.Value:yyyy-MM-dd HH:mm:ss}";
                    }
                    else if (authorization.EndTime.HasValue && DateTime.Now > authorization.EndTime.Value)
                    {
                        reason = $"授权已过期，过期时间: {authorization.EndTime.Value:yyyy-MM-dd HH:mm:ss}";
                    }

                    return new AuthorizationCheckResult
                    {
                        IsAuthorized = false,
                        Message = reason,
                        MachineCode = currentMachineCode,
                        Authorization = authorization
                    };
                }

                // 更新最后使用时间
                authorization.LastUsedAt = DateTime.Now;
                await _context.SaveChangesAsync();

                return new AuthorizationCheckResult
                {
                    IsAuthorized = true,
                    Message = "授权验证成功",
                    MachineCode = currentMachineCode,
                    Authorization = authorization
                };
            }
            catch (Exception ex)
            {
                return new AuthorizationCheckResult
                {
                    IsAuthorized = false,
                    Message = $"授权检查失败: {ex.Message}",
                    MachineCode = MachineCodeHelper.GetMachineCode()
                };
            }
        }

        /// <summary>
        /// 添加机器授权
        /// </summary>
        public async Task<MachineAuthorization> AddAuthorizationAsync(MachineAuthorization authorization)
        {
            _context.MachineAuthorizations.Add(authorization);
            await _context.SaveChangesAsync();
            return authorization;
        }

        /// <summary>
        /// 更新机器授权
        /// </summary>
        public async Task<MachineAuthorization> UpdateAuthorizationAsync(MachineAuthorization authorization)
        {
            _context.MachineAuthorizations.Update(authorization);
            await _context.SaveChangesAsync();
            return authorization;
        }

        /// <summary>
        /// 删除机器授权
        /// </summary>
        public async Task<bool> DeleteAuthorizationAsync(long id)
        {
            var authorization = await _context.MachineAuthorizations.FindAsync(id);
            if (authorization == null)
                return false;

            _context.MachineAuthorizations.Remove(authorization);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// 启用机器授权
        /// </summary>
        public async Task<bool> EnableAuthorizationAsync(string machineCode)
        {
            var authorization = await _context.MachineAuthorizations
                .FirstOrDefaultAsync(x => x.MachineCode == machineCode);

            if (authorization == null)
                return false;

            authorization.IsEnabled = true;
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// 禁用机器授权
        /// </summary>
        public async Task<bool> DisableAuthorizationAsync(string machineCode)
        {
            var authorization = await _context.MachineAuthorizations
                .FirstOrDefaultAsync(x => x.MachineCode == machineCode);

            if (authorization == null)
                return false;

            authorization.IsEnabled = false;
            await _context.SaveChangesAsync();
            return true;
        }

        #region IDisposable 实现

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // 由于 context 是通过构造函数传入的，不应该在这里释放
                }
                _disposed = true;
            }
        }

        #endregion
    }

    /// <summary>
    /// 授权检查结果
    /// </summary>
    public class AuthorizationCheckResult
    {
        /// <summary>
        /// 是否授权
        /// </summary>
        public bool IsAuthorized { get; set; }

        /// <summary>
        /// 消息
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 机器码
        /// </summary>
        public string MachineCode { get; set; } = string.Empty;

        /// <summary>
        /// 授权信息
        /// </summary>
        public MachineAuthorization? Authorization { get; set; }
    }
}

