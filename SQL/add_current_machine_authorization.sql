-- ============================================
-- 添加当前机器授权
-- ============================================
-- 说明：将当前使用的机器码添加到授权表
-- 用途：快速授权当前机器运行客户端

USE aion2_auction_db;

-- 从 monitored_items 表获取当前机器码并添加授权
INSERT INTO machine_authorizations (machine_code, is_enabled, machine_name, notes, start_time)
SELECT 
    DISTINCT machine_code,
    1 as is_enabled,
    '开发机器' as machine_name,
    '自动添加的授权记录' as notes,
    NOW() as start_time
FROM monitored_items
WHERE machine_code IS NOT NULL
ON DUPLICATE KEY UPDATE 
    is_enabled = 1,
    updated_at = NOW();

-- 查询授权结果
SELECT * FROM machine_authorizations;

-- 显示统计信息
SELECT 
    COUNT(*) as total_authorizations,
    SUM(CASE WHEN is_enabled = 1 THEN 1 ELSE 0 END) as enabled_count,
    SUM(CASE WHEN is_enabled = 0 THEN 1 ELSE 0 END) as disabled_count
FROM machine_authorizations;

SELECT '✓ 当前机器授权添加完成！' AS message;

