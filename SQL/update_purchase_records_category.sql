-- ============================================
-- 更新购买记录表的物品分类字段
-- ============================================
-- 说明：根据物品名称自动匹配并填充category_id字段
-- 方法1：从items表关联
-- 方法2：根据关键词推断

USE aion2_helper;

-- 步骤1：从items表中关联更新（如果物品在items表中存在）
UPDATE purchase_records pr
INNER JOIN items i ON pr.item_name = i.name
SET pr.category_id = i.category_id
WHERE pr.category_id IS NULL 
  AND i.category_id IS NOT NULL;

SELECT CONCAT('✓ 通过items表关联更新了 ', ROW_COUNT(), ' 条记录') AS message;

-- 步骤2：根据物品名称关键词推断分类
-- 注意：确保item_categories表中已有对应的分类数据

-- 2.1 更新武器类
UPDATE purchase_records pr
LEFT JOIN item_categories ic ON ic.name = '武器'
SET pr.category_id = ic.id
WHERE pr.category_id IS NULL
  AND (pr.item_name LIKE '%武器%' 
       OR pr.item_name LIKE '%剑%' 
       OR pr.item_name LIKE '%刀%' 
       OR pr.item_name LIKE '%枪%' 
       OR pr.item_name LIKE '%弓%' 
       OR pr.item_name LIKE '%法杖%');

-- 2.2 更新防具类
UPDATE purchase_records pr
LEFT JOIN item_categories ic ON ic.name = '防具'
SET pr.category_id = ic.id
WHERE pr.category_id IS NULL
  AND (pr.item_name LIKE '%防具%' 
       OR pr.item_name LIKE '%盔甲%' 
       OR pr.item_name LIKE '%头盔%' 
       OR pr.item_name LIKE '%胸甲%' 
       OR pr.item_name LIKE '%护腿%' 
       OR pr.item_name LIKE '%靴子%'
       OR pr.item_name LIKE '%套装%');

-- 2.3 更新饰品类
UPDATE purchase_records pr
LEFT JOIN item_categories ic ON ic.name = '饰品'
SET pr.category_id = ic.id
WHERE pr.category_id IS NULL
  AND (pr.item_name LIKE '%戒指%' 
       OR pr.item_name LIKE '%项链%' 
       OR pr.item_name LIKE '%耳环%' 
       OR pr.item_name LIKE '%手镯%' 
       OR pr.item_name LIKE '%饰品%');

-- 2.4 更新强化道具类
UPDATE purchase_records pr
LEFT JOIN item_categories ic ON ic.name = '强化道具'
SET pr.category_id = ic.id
WHERE pr.category_id IS NULL
  AND (pr.item_name LIKE '%强化石%' 
       OR pr.item_name LIKE '%精炼石%' 
       OR pr.item_name LIKE '%强化%' 
       OR pr.item_name LIKE '%碎片%'
       OR pr.item_name LIKE '%精华%');

-- 2.5 更新材料类
UPDATE purchase_records pr
LEFT JOIN item_categories ic ON ic.name = '材料'
SET pr.category_id = ic.id
WHERE pr.category_id IS NULL
  AND (pr.item_name LIKE '%材料%' 
       OR pr.item_name LIKE '%矿石%' 
       OR pr.item_name LIKE '%皮革%' 
       OR pr.item_name LIKE '%布料%' 
       OR pr.item_name LIKE '%木材%'
       OR pr.item_name LIKE '%包%'
       OR pr.item_name LIKE '%龙鳞%');

-- 2.6 更新宝石类
UPDATE purchase_records pr
LEFT JOIN item_categories ic ON ic.name = '宝石'
SET pr.category_id = ic.id
WHERE pr.category_id IS NULL
  AND (pr.item_name LIKE '%宝石%' 
       OR pr.item_name LIKE '%水晶%' 
       OR pr.item_name LIKE '%红宝石%' 
       OR pr.item_name LIKE '%蓝宝石%' 
       OR pr.item_name LIKE '%绿宝石%'
       OR pr.item_name LIKE '%钻石%');

-- 2.7 更新消耗品类
UPDATE purchase_records pr
LEFT JOIN item_categories ic ON ic.name = '消耗品'
SET pr.category_id = ic.id
WHERE pr.category_id IS NULL
  AND (pr.item_name LIKE '%药水%' 
       OR pr.item_name LIKE '%药剂%' 
       OR pr.item_name LIKE '%食物%' 
       OR pr.item_name LIKE '%卷轴%'
       OR pr.item_name LIKE '%消耗%');

-- 2.8 更新装备类（通用）
UPDATE purchase_records pr
LEFT JOIN item_categories ic ON ic.name = '防具'
SET pr.category_id = ic.id
WHERE pr.category_id IS NULL
  AND (pr.item_name LIKE '%装备%');

-- 显示更新结果统计
SELECT '====== 更新完成统计 ======' AS title;

SELECT 
    '已分类记录' AS status,
    COUNT(*) AS count,
    CONCAT(ROUND(COUNT(*) * 100.0 / (SELECT COUNT(*) FROM purchase_records), 2), '%') AS percentage
FROM purchase_records 
WHERE category_id IS NOT NULL

UNION ALL

SELECT 
    '未分类记录' AS status,
    COUNT(*) AS count,
    CONCAT(ROUND(COUNT(*) * 100.0 / (SELECT COUNT(*) FROM purchase_records), 2), '%') AS percentage
FROM purchase_records 
WHERE category_id IS NULL;

-- 按分类统计购买记录
SELECT 
    COALESCE(ic.name, '未分类') AS category_name,
    COUNT(pr.id) AS purchase_count,
    SUM(pr.quantity) AS total_quantity,
    SUM(pr.total_amount) AS total_amount,
    AVG(pr.price) AS avg_price,
    SUM(pr.expected_profit) AS total_expected_profit
FROM purchase_records pr
LEFT JOIN item_categories ic ON pr.category_id = ic.id
GROUP BY pr.category_id, ic.name
ORDER BY purchase_count DESC;

-- 显示未分类的物品名称（用于手动处理）
SELECT '====== 未分类物品列表 ======' AS title;

SELECT DISTINCT 
    pr.item_name,
    COUNT(*) AS purchase_count
FROM purchase_records pr
WHERE pr.category_id IS NULL
GROUP BY pr.item_name
ORDER BY purchase_count DESC;

-- 完成提示
SELECT '✓ 购买记录分类字段更新完成！' AS message;
SELECT '提示：如果还有未分类的物品，可以手动更新或添加更多关键词匹配规则。' AS info;

