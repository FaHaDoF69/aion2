-- Aion2 数据库常用查询语句

-- 1. 查看所有表的记录数量
SELECT 
    'auction_items' as table_name, COUNT(*) as record_count FROM auction_items
UNION ALL
SELECT 
    'purchase_records' as table_name, COUNT(*) as record_count FROM purchase_records
UNION ALL
SELECT 
    'monitored_items' as table_name, COUNT(*) as record_count FROM monitored_items;

-- 2. 按机器码查看数据分布
SELECT 
    machine_code,
    COUNT(*) as auction_items_count
FROM auction_items 
GROUP BY machine_code
ORDER BY auction_items_count DESC;

-- 3. 查看最近24小时发现的拍卖行物品
SELECT 
    machine_code,
    name,
    price,
    quantity,
    seller_name,
    discovered_at
FROM auction_items 
WHERE discovered_at >= DATE_SUB(NOW(), INTERVAL 24 HOUR)
ORDER BY discovered_at DESC;

-- 4. 查看异常低价物品
SELECT 
    machine_code,
    name,
    price,
    quantity,
    price_deviation,
    discovered_at
FROM auction_items 
WHERE is_abnormal_price = 1
ORDER BY price_deviation DESC;

-- 5. 查看购买记录统计
SELECT 
    machine_code,
    status,
    COUNT(*) as count,
    SUM(total_amount) as total_amount,
    SUM(expected_profit) as total_expected_profit
FROM purchase_records 
GROUP BY machine_code, status
ORDER BY machine_code, status;

-- 6. 查看启用的监控物品
SELECT 
    machine_code,
    item_name,
    category,
    target_min_price,
    target_max_price,
    priority,
    auto_purchase_enabled,
    total_found_count,
    total_purchase_count,
    last_found_at
FROM monitored_items 
WHERE is_enabled = 1
ORDER BY machine_code, priority DESC, item_name;

-- 7. 查看监控物品的发现统计
SELECT 
    machine_code,
    category,
    COUNT(*) as item_count,
    SUM(total_found_count) as total_found,
    SUM(total_purchase_count) as total_purchased,
    AVG(expected_profit_rate) as avg_profit_rate
FROM monitored_items 
WHERE is_enabled = 1
GROUP BY machine_code, category
ORDER BY machine_code, total_found DESC;

-- 8. 查看最近发现的监控物品
SELECT 
    m.machine_code,
    m.item_name,
    m.category,
    m.last_found_price,
    m.target_min_price,
    m.target_max_price,
    m.last_found_at,
    CASE 
        WHEN m.last_found_price <= m.target_min_price THEN '低于目标价'
        WHEN m.last_found_price >= m.target_max_price THEN '高于目标价'
        ELSE '在目标范围内'
    END as price_status
FROM monitored_items m
WHERE m.last_found_at IS NOT NULL
ORDER BY m.last_found_at DESC
LIMIT 20;

-- 9. 查看各机器的活跃度（最近7天）
SELECT 
    machine_code,
    MAX(discovered_at) as last_auction_activity,
    MAX(purchase_time) as last_purchase_activity,
    COUNT(DISTINCT DATE(discovered_at)) as active_days
FROM (
    SELECT machine_code, discovered_at, NULL as purchase_time FROM auction_items WHERE discovered_at >= DATE_SUB(NOW(), INTERVAL 7 DAY)
    UNION ALL
    SELECT machine_code, NULL as discovered_at, purchase_time FROM purchase_records WHERE purchase_time >= DATE_SUB(NOW(), INTERVAL 7 DAY)
) combined
GROUP BY machine_code
ORDER BY active_days DESC, last_auction_activity DESC;

-- 10. 查看盈利最好的物品
SELECT 
    pr.machine_code,
    pr.item_name,
    COUNT(*) as purchase_count,
    AVG(pr.price) as avg_purchase_price,
    SUM(pr.expected_profit) as total_expected_profit,
    AVG(pr.expected_profit) as avg_expected_profit
FROM purchase_records pr
WHERE pr.status = 2 -- 已完成
GROUP BY pr.machine_code, pr.item_name
HAVING purchase_count >= 2
ORDER BY total_expected_profit DESC
LIMIT 10;

-- 11. 清理过期数据（30天前的拍卖行记录）
-- 注意：这是删除操作，请谨慎使用
-- DELETE FROM auction_items WHERE discovered_at < DATE_SUB(NOW(), INTERVAL 30 DAY);

-- 12. 备份特定机器的监控配置
-- SELECT * FROM monitored_items WHERE machine_code = 'YOUR_MACHINE_CODE';

-- 13. 查看数据库表大小
SELECT 
    table_name,
    ROUND(((data_length + index_length) / 1024 / 1024), 2) AS 'Size (MB)'
FROM information_schema.tables 
WHERE table_schema = 'aion2'
ORDER BY (data_length + index_length) DESC;

-- 14. 查看索引使用情况
SELECT 
    table_name,
    index_name,
    column_name,
    cardinality
FROM information_schema.statistics 
WHERE table_schema = 'aion2'
ORDER BY table_name, index_name;

-- 15. 监控物品价格范围分析
SELECT 
    category,
    COUNT(*) as item_count,
    MIN(target_min_price) as min_target_price,
    MAX(target_max_price) as max_target_price,
    AVG(target_min_price) as avg_min_price,
    AVG(target_max_price) as avg_max_price,
    AVG(expected_profit_rate) as avg_profit_rate
FROM monitored_items 
WHERE target_min_price IS NOT NULL AND target_max_price IS NOT NULL
GROUP BY category
ORDER BY avg_profit_rate DESC;
