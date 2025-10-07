-- 为监控物品表添加物品分类字段

-- 1. 添加 category_id 字段
ALTER TABLE `monitored_items` 
ADD COLUMN `category_id` INT NULL COMMENT '物品分类ID' AFTER `category`;

-- 2. 创建外键约束
ALTER TABLE `monitored_items`
ADD CONSTRAINT `fk_monitored_items_category` 
FOREIGN KEY (`category_id`) REFERENCES `item_categories` (`id`) 
ON DELETE SET NULL;

-- 3. 创建索引
ALTER TABLE `monitored_items`
ADD INDEX `idx_category_id` (`category_id`);

-- 4. 根据现有的 category 字符串字段自动关联到 item_categories 表
UPDATE `monitored_items` m
INNER JOIN `item_categories` c ON m.category = c.name
SET m.category_id = c.id
WHERE m.category IS NOT NULL AND m.category != '';

