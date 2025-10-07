-- 示例交易记录数据
-- 注意：请将 'YOUR_MACHINE_CODE' 替换为实际的机器码
-- 
-- 获取当前机器码的方法：
-- 1. 运行程序，在日志中查看 "当前机器码: XXXXXXXX"
-- 2. 或者在程序中执行: MachineCodeHelper.GetMachineCode()
--
-- 使用方法：
-- 1. 复制下面的机器码查询语句到数据库管理工具中执行
-- 2. 将查询结果中的机器码替换下面SQL中的 'YOUR_MACHINE_CODE'
-- 3. 执行插入语句

-- 查询当前数据库中已有的机器码（如果有数据的话）
-- SELECT DISTINCT machine_code FROM purchase_records LIMIT 5;
-- SELECT DISTINCT machine_code FROM monitored_items LIMIT 5;

-- 插入示例交易记录
INSERT INTO purchase_records (
    machine_code, item_name, price, quantity, total_amount, 
    seller_name, expected_profit, actual_profit, strategy, 
    status, execution_time_ms, notes, purchase_time
) VALUES 
-- 最近的成功交易
('YOUR_MACHINE_CODE', '传说武器强化石', 850000, 1, 850000, '玩家A', 350000, 380000, '捡漏', 2, 1250, '价格非常好', NOW() - INTERVAL 1 HOUR),
('YOUR_MACHINE_CODE', '史诗防具碎片', 450000, 2, 900000, '玩家B', 150000, 140000, '套利', 2, 980, '利润略低于预期', NOW() - INTERVAL 2 HOUR),
('YOUR_MACHINE_CODE', '稀有材料包', 120000, 5, 600000, '玩家C', 80000, 95000, '趋势', 2, 750, '市场价格上涨', NOW() - INTERVAL 3 HOUR),

-- 昨天的交易
('YOUR_MACHINE_CODE', '魔法水晶', 75000, 10, 750000, '玩家D', 25000, 30000, '批量', 2, 1100, '批量购买成功', NOW() - INTERVAL 1 DAY),
('YOUR_MACHINE_CODE', '装备碎片', 200000, 3, 600000, '玩家E', 60000, 55000, '捡漏', 2, 890, '小幅盈利', NOW() - INTERVAL 1 DAY - INTERVAL 2 HOUR),
('YOUR_MACHINE_CODE', '强化石', 300000, 2, 600000, '玩家F', 100000, 0, '套利', 3, 2300, '交易失败，网络问题', NOW() - INTERVAL 1 DAY - INTERVAL 4 HOUR),

-- 前天的交易
('YOUR_MACHINE_CODE', '宝石', 500000, 1, 500000, '玩家G', 200000, 220000, '捡漏', 2, 650, '超预期收益', NOW() - INTERVAL 2 DAY),
('YOUR_MACHINE_CODE', '药水', 15000, 20, 300000, '玩家H', 5000, 8000, '批量', 2, 1800, '消耗品投资', NOW() - INTERVAL 2 DAY - INTERVAL 1 HOUR),
('YOUR_MACHINE_CODE', '装备', 800000, 1, 800000, '玩家I', 300000, NULL, '趋势', 1, 0, '正在执行中', NOW() - INTERVAL 2 DAY - INTERVAL 3 HOUR),

-- 一周前的交易
('YOUR_MACHINE_CODE', '传说装备', 1500000, 1, 1500000, '玩家J', 500000, 480000, '捡漏', 2, 2100, '高价值交易', NOW() - INTERVAL 7 DAY),
('YOUR_MACHINE_CODE', '材料', 80000, 8, 640000, '玩家K', 20000, 25000, '套利', 2, 950, '材料价格稳定', NOW() - INTERVAL 7 DAY - INTERVAL 2 HOUR),
('YOUR_MACHINE_CODE', '强化材料', 180000, 4, 720000, '玩家L', 40000, 0, '趋势', 4, 0, '用户取消交易', NOW() - INTERVAL 7 DAY - INTERVAL 5 HOUR),

-- 更早的交易
('YOUR_MACHINE_CODE', '稀有武器', 1200000, 1, 1200000, '玩家M', 400000, 450000, '捡漏', 2, 1650, '武器市场火热', NOW() - INTERVAL 10 DAY),
('YOUR_MACHINE_CODE', '防具套装', 900000, 1, 900000, '玩家N', 200000, 180000, '套利', 2, 1400, '防具需求稳定', NOW() - INTERVAL 12 DAY),
('YOUR_MACHINE_CODE', '消耗品', 25000, 15, 375000, '玩家O', 8000, 12000, '批量', 2, 2200, '消耗品补货', NOW() - INTERVAL 15 DAY),

-- 失败的交易示例
('YOUR_MACHINE_CODE', '高级材料', 600000, 2, 1200000, '玩家P', 150000, 0, '套利', 3, 3500, '服务器维护导致失败', NOW() - INTERVAL 20 DAY),
('YOUR_MACHINE_CODE', '装备强化石', 400000, 3, 1200000, '玩家Q', 120000, 0, '趋势', 3, 1800, '价格突然下跌', NOW() - INTERVAL 25 DAY),

-- 待处理的交易
('YOUR_MACHINE_CODE', '新物品', 250000, 2, 500000, '玩家R', 80000, NULL, '测试', 0, 0, '等待处理', NOW() - INTERVAL 30 MINUTE),
('YOUR_MACHINE_CODE', '测试物品', 100000, 1, 100000, '玩家S', 30000, NULL, '测试', 0, 0, '测试交易', NOW() - INTERVAL 10 MINUTE);

-- 查询验证数据
SELECT 
    COUNT(*) as total_records,
    SUM(CASE WHEN status = 2 THEN 1 ELSE 0 END) as completed,
    SUM(CASE WHEN status = 3 THEN 1 ELSE 0 END) as failed,
    SUM(CASE WHEN status = 0 THEN 1 ELSE 0 END) as pending,
    SUM(total_amount) as total_amount,
    SUM(actual_profit) as total_profit
FROM purchase_records 
WHERE machine_code = 'YOUR_MACHINE_CODE';

-- 按日期统计
SELECT 
    DATE(purchase_time) as purchase_date,
    COUNT(*) as daily_count,
    SUM(total_amount) as daily_amount,
    SUM(actual_profit) as daily_profit
FROM purchase_records 
WHERE machine_code = 'YOUR_MACHINE_CODE'
GROUP BY DATE(purchase_time)
ORDER BY purchase_date DESC;
