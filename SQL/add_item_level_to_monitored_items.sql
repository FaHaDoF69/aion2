-- 为 monitored_items 表添加物品等级字段
-- 执行此脚本来更新现有的监控物品表结构

-- 1. 添加物品等级字段
ALTER TABLE `monitored_items` 
ADD COLUMN `item_level` TINYINT NULL COMMENT '物品等级 (1, 2, 3)' 
AFTER `category`;

-- 2. 为物品等级字段添加索引
ALTER TABLE `monitored_items` 
ADD INDEX `idx_item_level` (`item_level`);

-- 3. 添加复合索引（机器码 + 物品等级）
ALTER TABLE `monitored_items` 
ADD INDEX `idx_machine_item_level` (`machine_code`, `item_level`);

-- 4. 更新现有数据的物品等级（根据物品名称推测等级）
UPDATE `monitored_items` SET `item_level` = 3 
WHERE `item_name` LIKE '%传说%' OR `item_name` LIKE '%神话%' OR `item_name` LIKE '%legendary%';

UPDATE `monitored_items` SET `item_level` = 2 
WHERE `item_name` LIKE '%史诗%' OR `item_name` LIKE '%稀有%' OR `item_name` LIKE '%epic%' OR `item_name` LIKE '%rare%';

UPDATE `monitored_items` SET `item_level` = 2 
WHERE `item_name` LIKE '%高级%' OR `item_name` LIKE '%superior%' OR `item_name` LIKE '%精良%';

UPDATE `monitored_items` SET `item_level` = 1 
WHERE `item_level` IS NULL AND (`item_name` LIKE '%普通%' OR `item_name` LIKE '%common%' OR `item_name` LIKE '%基础%');

-- 5. 为未匹配的物品设置默认等级
UPDATE `monitored_items` SET `item_level` = 1 
WHERE `item_level` IS NULL;

-- 6. 验证更新结果
SELECT 
    `item_level`,
    CASE `item_level`
        WHEN 1 THEN '等级1'
        WHEN 2 THEN '等级2'
        WHEN 3 THEN '等级3'
        ELSE '未知'
    END as level_name,
    COUNT(*) as count,
    GROUP_CONCAT(DISTINCT `item_name` SEPARATOR ', ') as items
FROM `monitored_items` 
GROUP BY `item_level`
ORDER BY `item_level` DESC;

-- 7. 查看表结构
DESCRIBE `monitored_items`;

-- 8. 查看索引
SHOW INDEX FROM `monitored_items` WHERE Key_name LIKE '%level%';
