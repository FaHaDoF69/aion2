-- ============================================
-- 移除监控历史表中不必要的外键字段
-- ============================================
-- 说明：删除 monitored_item_id 和 purchase_record_id 字段
-- 原因：这两个字段导致查询复杂，且关系不合理
-- 影响：删除后查询更简单，通过 item_name 匹配即可

USE aion2_auction_db;

-- 检查字段是否存在
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = 'aion2_auction_db'
    AND TABLE_NAME = 'monitoring_history'
    AND COLUMN_NAME IN ('monitored_item_id', 'purchase_record_id');

-- 删除外键约束（如果存在）
SET @schema_name = 'aion2_auction_db';
SET @table_name = 'monitoring_history';

-- 查找并删除相关的外键约束
SELECT CONCAT('ALTER TABLE ', TABLE_NAME, ' DROP FOREIGN KEY ', CONSTRAINT_NAME, ';') AS drop_fk_sql
FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE
WHERE TABLE_SCHEMA = @schema_name
    AND TABLE_NAME = @table_name
    AND REFERENCED_TABLE_NAME IS NOT NULL
    AND COLUMN_NAME IN ('monitored_item_id', 'purchase_record_id');

-- 手动执行上面查询结果中的 SQL，或者使用以下语句（如果知道约束名）
-- ALTER TABLE monitoring_history DROP FOREIGN KEY fk_monitoring_history_monitored_item;
-- ALTER TABLE monitoring_history DROP FOREIGN KEY fk_monitoring_history_purchase_record;

-- 删除字段
ALTER TABLE monitoring_history 
DROP COLUMN monitored_item_id;

ALTER TABLE monitoring_history 
DROP COLUMN purchase_record_id;

-- 验证删除结果
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = 'aion2_auction_db'
    AND TABLE_NAME = 'monitoring_history'
ORDER BY ORDINAL_POSITION;

-- 显示成功消息
SELECT '✓ 成功删除 monitored_item_id 和 purchase_record_id 字段' AS message;
SELECT '提示：这两个字段已从模型和代码中移除，查询现在通过 item_name 匹配' AS info;

