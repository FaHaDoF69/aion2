-- ============================================
-- 物品分类示例数据
-- ============================================
-- 说明：这个脚本向 item_categories 表中插入示例数据
-- 用途：用于测试监控物品编辑对话框的分类下拉列表功能

USE aion2_auction_db;

-- 清空现有分类数据（可选）
-- TRUNCATE TABLE item_categories;

-- 插入物品分类数据
INSERT INTO item_categories (name, description, sort_order, is_enabled, created_at)
VALUES 
    ('武器', '各种武器类物品', 1, TRUE, NOW()),
    ('防具', '各种防具类物品', 2, TRUE, NOW()),
    ('饰品', '戒指、项链、耳环等饰品', 3, TRUE, NOW()),
    ('材料', '各种制造材料', 4, TRUE, NOW()),
    ('强化道具', '强化石、精炼石等强化相关道具', 5, TRUE, NOW()),
    ('消耗品', '药水、食物等消耗品', 6, TRUE, NOW()),
    ('宝石', '镶嵌用宝石', 7, TRUE, NOW()),
    ('特殊物品', '任务物品、特殊道具等', 8, TRUE, NOW()),
    ('时装', '装饰性时装', 9, TRUE, NOW()),
    ('坐骑', '坐骑相关物品', 10, TRUE, NOW())
ON DUPLICATE KEY UPDATE
    description = VALUES(description),
    sort_order = VALUES(sort_order),
    is_enabled = VALUES(is_enabled);

-- 查询插入的数据
SELECT * FROM item_categories ORDER BY sort_order;

-- 显示统计信息
SELECT 
    COUNT(*) AS total_categories,
    SUM(CASE WHEN is_enabled = 1 THEN 1 ELSE 0 END) AS enabled_categories,
    SUM(CASE WHEN is_enabled = 0 THEN 1 ELSE 0 END) AS disabled_categories
FROM item_categories;

-- 提示信息
SELECT '✓ 物品分类数据插入完成！' AS message;
SELECT '提示：打开监控物品编辑对话框，分类下拉列表将显示这些分类。' AS info;

