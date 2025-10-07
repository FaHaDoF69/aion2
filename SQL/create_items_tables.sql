-- 创建物品分类表和物品表

-- 1. 创建物品分类表
CREATE TABLE IF NOT EXISTS `item_categories` (
  `id` INT NOT NULL AUTO_INCREMENT COMMENT '分类ID',
  `name` VARCHAR(50) NOT NULL COMMENT '分类名称',
  `description` VARCHAR(200) NULL COMMENT '分类描述',
  `sort_order` INT NOT NULL DEFAULT 0 COMMENT '排序顺序',
  `is_enabled` TINYINT(1) NOT NULL DEFAULT 1 COMMENT '是否启用',
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_name` (`name`),
  KEY `idx_sort_order` (`sort_order`),
  KEY `idx_is_enabled` (`is_enabled`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='物品分类表';

-- 2. 创建物品表
CREATE TABLE IF NOT EXISTS `items` (
  `id` INT NOT NULL AUTO_INCREMENT COMMENT '物品ID',
  `name` VARCHAR(100) NOT NULL COMMENT '物品名称',
  `category_id` INT NOT NULL COMMENT '物品分类ID',
  `item_level` INT NULL COMMENT '物品等级',
  `quality` VARCHAR(20) NULL COMMENT '物品品质',
  `reference_price` DECIMAL(18,2) NULL COMMENT '参考价格',
  `description` VARCHAR(500) NULL COMMENT '物品描述',
  `icon_path` VARCHAR(255) NULL COMMENT '物品图标路径',
  `is_tradable` TINYINT(1) NOT NULL DEFAULT 1 COMMENT '是否可交易',
  `is_enabled` TINYINT(1) NOT NULL DEFAULT 1 COMMENT '是否启用',
  `sort_order` INT NOT NULL DEFAULT 0 COMMENT '排序顺序',
  `remarks` VARCHAR(255) NULL COMMENT '备注',
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  `updated_at` DATETIME NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT '更新时间',
  PRIMARY KEY (`id`),
  KEY `idx_name` (`name`),
  KEY `idx_category_id` (`category_id`),
  KEY `idx_is_enabled` (`is_enabled`),
  KEY `idx_category_name` (`category_id`, `name`),
  CONSTRAINT `fk_items_category` FOREIGN KEY (`category_id`) REFERENCES `item_categories` (`id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='物品表';

-- 3. 插入初始物品分类数据
INSERT INTO `item_categories` (`name`, `description`, `sort_order`, `is_enabled`) VALUES
('强化石', '用于装备强化的材料', 1, 1),
('碎片', '各类装备碎片', 2, 1),
('宝石', '镶嵌在装备上的宝石', 3, 1),
('药水', '回复生命、魔法的药水', 4, 1),
('卷轴', '各类魔法卷轴', 5, 1),
('材料', '制作物品的原材料', 6, 1),
('装备', '武器、防具等装备', 7, 1),
('饰品', '戒指、项链、耳环等', 8, 1),
('其他', '其他物品', 99, 1)
ON DUPLICATE KEY UPDATE
  `description` = VALUES(`description`),
  `sort_order` = VALUES(`sort_order`);

-- 4. 插入示例物品数据（可选）
INSERT INTO `items` (`name`, `category_id`, `item_level`, `quality`, `reference_price`, `description`, `is_tradable`, `sort_order`) VALUES
('神话饰品强化石', 1, 0, '传说', 10, '用于强化神话品质的饰品', 1, 1),
('传说武器强化石', 1, 0, '传说', 9, '用于强化传说品质的武器', 1, 2),
('完美钻石', 1, 0, '史诗', 9, '完美品质的钻石', 1, 3),
('神话饰品碎片', 2, 0, '传说', 9, '用于合成神话饰品', 1, 4),
('传说武器碎片', 2, 0, '传说', 8, '用于合成传说武器', 1, 5),
('史诗防具碎片', 2, 0, '史诗', 8, '用于合成史诗防具', 1, 6),
('时空裂缝钥匙', 2, 0, '稀有', 8, '开启时空裂缝副本', 1, 7),
('精灵之泪', 3, 0, '史诗', 7, '稀有的宝石材料', 1, 8),
('龙鳞碎片', 3, 0, '史诗', 7, '龙鳞制作的碎片', 1, 9),
('魔法水晶', 4, 0, '稀有', 6, '恢复魔法值', 1, 10),
('神秘宝箱', 5, 0, '传说', 6, '可能开出稀有物品', 1, 11),
('经验增益药水', 4, 0, '稀有', 6, '增加经验获取', 1, 12),
('力量红宝石', 3, 0, '稀有', 5, '增加力量属性', 1, 13),
('敏捷蓝宝石', 3, 0, '稀有', 5, '增加敏捷属性', 1, 14),
('智力紫宝石', 3, 0, '稀有', 5, '增加智力属性', 1, 15),
('雷霆卷轴', 5, 0, '稀有', 5, '施放雷霆魔法', 1, 16),
('高级生命药水', 4, 0, '高级', 4, '恢复大量生命值', 1, 17),
('高级魔法药水', 4, 0, '高级', 4, '恢复大量魔法值', 1, 18),
('普通快矿石', 6, 0, '普通', 2, '基础矿石材料', 1, 19),
('废弃的装备', 7, 0, '普通', 1, '低级装备', 1, 20)
ON DUPLICATE KEY UPDATE
  `category_id` = VALUES(`category_id`),
  `item_level` = VALUES(`item_level`),
  `quality` = VALUES(`quality`),
  `reference_price` = VALUES(`reference_price`),
  `description` = VALUES(`description`);

