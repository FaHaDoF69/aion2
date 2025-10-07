using System;
using System.Management;
using System.Security.Cryptography;
using System.Text;

namespace Aion2Helper.Utils;

/// <summary>
/// 机器码生成和管理辅助类
/// </summary>
public static class MachineCodeHelper
{
    private static string? _machineCode;
    private static readonly object _lock = new object();

    /// <summary>
    /// 获取当前机器的唯一标识码
    /// </summary>
    /// <returns>机器码</returns>
    public static string GetMachineCode()
    {
        if (_machineCode != null)
            return _machineCode;

        lock (_lock)
        {
            if (_machineCode != null)
                return _machineCode;

            _machineCode = GenerateMachineCode();
            return _machineCode;
        }
    }

    /// <summary>
    /// 生成机器码
    /// </summary>
    /// <returns>机器码</returns>
    private static string GenerateMachineCode()
    {
        try
        {
            var components = new StringBuilder();

            // 获取CPU序列号
            var cpuId = GetCpuId();
            if (!string.IsNullOrEmpty(cpuId))
                components.Append(cpuId);

            // 获取主板序列号
            var motherboardId = GetMotherboardId();
            if (!string.IsNullOrEmpty(motherboardId))
                components.Append(motherboardId);

            // 获取硬盘序列号
            var diskId = GetDiskId();
            if (!string.IsNullOrEmpty(diskId))
                components.Append(diskId);

            // 获取MAC地址
            var macAddress = GetMacAddress();
            if (!string.IsNullOrEmpty(macAddress))
                components.Append(macAddress);

            // 如果无法获取硬件信息，使用机器名和用户名作为备选
            if (components.Length == 0)
            {
                components.Append(Environment.MachineName);
                components.Append(Environment.UserName);
                components.Append(Environment.OSVersion.ToString());
            }

            // 生成MD5哈希
            using var md5 = MD5.Create();
            var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(components.ToString()));
            return Convert.ToHexString(hash);
        }
        catch (Exception ex)
        {
            // 如果生成失败，使用备用方案
            var fallback = $"{Environment.MachineName}_{Environment.UserName}_{DateTime.Now.Ticks}";
            using var md5 = MD5.Create();
            var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(fallback));
            return Convert.ToHexString(hash);
        }
    }

    /// <summary>
    /// 获取CPU ID
    /// </summary>
    /// <returns>CPU ID</returns>
    private static string GetCpuId()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT ProcessorId FROM Win32_Processor");
            foreach (ManagementObject obj in searcher.Get())
            {
                var processorId = obj["ProcessorId"]?.ToString();
                if (!string.IsNullOrEmpty(processorId))
                    return processorId;
            }
        }
        catch
        {
            // 忽略异常
        }
        return string.Empty;
    }

    /// <summary>
    /// 获取主板ID
    /// </summary>
    /// <returns>主板ID</returns>
    private static string GetMotherboardId()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BaseBoard");
            foreach (ManagementObject obj in searcher.Get())
            {
                var serialNumber = obj["SerialNumber"]?.ToString();
                if (!string.IsNullOrEmpty(serialNumber) && serialNumber != "None")
                    return serialNumber;
            }
        }
        catch
        {
            // 忽略异常
        }
        return string.Empty;
    }

    /// <summary>
    /// 获取硬盘ID
    /// </summary>
    /// <returns>硬盘ID</returns>
    private static string GetDiskId()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_DiskDrive WHERE MediaType='Fixed hard disk media'");
            foreach (ManagementObject obj in searcher.Get())
            {
                var serialNumber = obj["SerialNumber"]?.ToString()?.Trim();
                if (!string.IsNullOrEmpty(serialNumber))
                    return serialNumber;
            }
        }
        catch
        {
            // 忽略异常
        }
        return string.Empty;
    }

    /// <summary>
    /// 获取MAC地址
    /// </summary>
    /// <returns>MAC地址</returns>
    private static string GetMacAddress()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT MACAddress FROM Win32_NetworkAdapter WHERE NetConnectionStatus=2");
            foreach (ManagementObject obj in searcher.Get())
            {
                var macAddress = obj["MACAddress"]?.ToString();
                if (!string.IsNullOrEmpty(macAddress))
                    return macAddress.Replace(":", "");
            }
        }
        catch
        {
            // 忽略异常
        }
        return string.Empty;
    }

    /// <summary>
    /// 获取机器码的显示名称（前8位）
    /// </summary>
    /// <returns>机器码显示名称</returns>
    public static string GetMachineCodeDisplay()
    {
        var code = GetMachineCode();
        return code.Length >= 8 ? code.Substring(0, 8) : code;
    }

    /// <summary>
    /// 验证机器码格式
    /// </summary>
    /// <param name="machineCode">机器码</param>
    /// <returns>是否有效</returns>
    public static bool IsValidMachineCode(string machineCode)
    {
        if (string.IsNullOrEmpty(machineCode))
            return false;

        return machineCode.Length == 32 && 
               System.Text.RegularExpressions.Regex.IsMatch(machineCode, "^[A-F0-9]{32}$");
    }
}
