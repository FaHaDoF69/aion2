-- 移除监控物品表中不合理的字段
-- 这些字段应该在交易时计算，而不是在监控配置中存储

-- 1. 移除 expected_profit_rate 字段（期望利润率）
-- 理由：利润率应该在发现交易机会时动态计算，而不是预设
ALTER TABLE `monitored_items` 
DROP COLUMN IF EXISTS `expected_profit_rate`;

-- 2. 移除 max_quantity 字段（最大购买数量）
-- 理由：购买数量应该在具体交易时决定，而不是在监控配置中限制
ALTER TABLE `monitored_items` 
DROP COLUMN IF EXISTS `max_quantity`;

-- 3. 验证字段移除结果
DESCRIBE `monitored_items`;

-- 4. 查看当前表结构
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT,
    COLUMN_COMMENT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_SCHEMA = DATABASE() 
  AND TABLE_NAME = 'monitored_items'
ORDER BY ORDINAL_POSITION;

-- 5. 验证现有数据完整性
SELECT 
    COUNT(*) as total_items,
    COUNT(CASE WHEN target_min_price IS NOT NULL THEN 1 END) as has_min_price,
    COUNT(CASE WHEN target_max_price IS NOT NULL THEN 1 END) as has_max_price,
    COUNT(CASE WHEN item_level IS NOT NULL THEN 1 END) as has_level
FROM `monitored_items`;

-- 6. 查看移除字段后的示例数据
SELECT 
    item_name,
    category,
    item_level,
    target_min_price,
    target_max_price,
    priority,
    is_enabled,
    auto_purchase_enabled,
    monitor_strategy
FROM `monitored_items` 
ORDER BY priority DESC 
LIMIT 5;
