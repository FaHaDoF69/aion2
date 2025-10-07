-- Aion2 拍卖行智能辅助系统 - 完整数据库架构
-- 创建数据库
CREATE DATABASE IF NOT EXISTS `aion2` 
DEFAULT CHARACTER SET utf8mb4 
COLLATE utf8mb4_unicode_ci;

USE `aion2`;

-- 1. 拍卖行物品表
CREATE TABLE `auction_items` (
    `id` BIGINT NOT NULL AUTO_INCREMENT,
    `machine_code` VARCHAR(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL COMMENT '机器码',
    `name` VARCHAR(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL COMMENT '物品名称',
    `price` DECIMAL(18,2) NOT NULL COMMENT '价格',
    `quantity` INT NOT NULL COMMENT '数量',
    `seller_name` VARCHAR(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT '' COMMENT '卖家名称',
    `remaining_hours` INT DEFAULT 0 COMMENT '剩余时间(小时)',
    `discovered_at` TIMESTAMP DEFAULT CURRENT_TIMESTAMP COMMENT '发现时间',
    `position_x` INT DEFAULT 0 COMMENT '屏幕位置X坐标',
    `position_y` INT DEFAULT 0 COMMENT '屏幕位置Y坐标',
    `is_abnormal_price` TINYINT(1) DEFAULT 0 COMMENT '是否为异常低价',
    `price_deviation` DOUBLE DEFAULT 0.0 COMMENT '价格偏差百分比',
    PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='拍卖行物品表';

-- 2. 购买记录表
CREATE TABLE `purchase_records` (
    `id` BIGINT NOT NULL AUTO_INCREMENT,
    `machine_code` VARCHAR(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL COMMENT '机器码',
    `item_name` VARCHAR(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL COMMENT '物品名称',
    `price` DECIMAL(18,2) NOT NULL COMMENT '购买价格',
    `quantity` INT NOT NULL COMMENT '购买数量',
    `total_amount` DECIMAL(18,2) NOT NULL COMMENT '总金额',
    `purchase_time` TIMESTAMP DEFAULT CURRENT_TIMESTAMP COMMENT '购买时间',
    `seller_name` VARCHAR(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT '卖家名称',
    `expected_profit` DECIMAL(18,2) DEFAULT 0.00 COMMENT '预期利润',
    `actual_profit` DECIMAL(18,2) DEFAULT NULL COMMENT '实际利润',
    `strategy` VARCHAR(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT '' COMMENT '购买策略',
    `status` INT DEFAULT 0 COMMENT '执行状态(0-待处理,1-执行中,2-已完成,3-失败,4-已取消)',
    `execution_time_ms` BIGINT DEFAULT 0 COMMENT '执行耗时(毫秒)',
    `notes` VARCHAR(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT '备注',
    PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='购买记录表';

-- 3. 被监控物品表
CREATE TABLE `monitored_items` (
    `id` BIGINT NOT NULL AUTO_INCREMENT,
    `machine_code` VARCHAR(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL COMMENT '机器码',
    `item_name` VARCHAR(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL COMMENT '物品名称',
    `category` VARCHAR(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT '' COMMENT '物品类别',
    `target_min_price` DECIMAL(18,2) DEFAULT NULL COMMENT '目标最低价格',
    `target_max_price` DECIMAL(18,2) DEFAULT NULL COMMENT '目标最高价格',
    `expected_profit_rate` DECIMAL(5,4) DEFAULT 0.2000 COMMENT '期望利润率',
    `max_quantity` INT DEFAULT 1 COMMENT '最大购买数量',
    `priority` INT DEFAULT 5 COMMENT '优先级(1-10)',
    `is_enabled` TINYINT(1) DEFAULT 1 COMMENT '是否启用监控',
    `auto_purchase_enabled` TINYINT(1) DEFAULT 0 COMMENT '是否启用自动购买',
    `monitor_strategy` VARCHAR(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT '价格监控' COMMENT '监控策略',
    `risk_level` INT DEFAULT 2 COMMENT '风险等级(1-低,2-中,3-高,4-极高)',
    `created_at` TIMESTAMP DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
    `updated_at` TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT '更新时间',
    `last_monitored_at` TIMESTAMP NULL DEFAULT NULL COMMENT '最后监控时间',
    `last_found_at` TIMESTAMP NULL DEFAULT NULL COMMENT '最后发现时间',
    `last_found_price` DECIMAL(18,2) DEFAULT NULL COMMENT '最后发现的价格',
    `total_found_count` INT DEFAULT 0 COMMENT '总发现次数',
    `total_purchase_count` INT DEFAULT 0 COMMENT '总购买次数',
    `notes` VARCHAR(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT '备注',
    PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='被监控物品表';

-- 创建索引
-- auction_items 表索引
CREATE INDEX `idx_auction_machine_code` ON `auction_items` (`machine_code`);
CREATE INDEX `idx_auction_name` ON `auction_items` (`name`);
CREATE INDEX `idx_auction_price` ON `auction_items` (`price`);
CREATE INDEX `idx_auction_discovered_at` ON `auction_items` (`discovered_at`);
CREATE INDEX `idx_auction_is_abnormal_price` ON `auction_items` (`is_abnormal_price`);
CREATE INDEX `idx_auction_machine_code_discovered_at` ON `auction_items` (`machine_code`, `discovered_at`);
CREATE INDEX `idx_auction_machine_code_name` ON `auction_items` (`machine_code`, `name`);

-- purchase_records 表索引
CREATE INDEX `idx_purchase_machine_code` ON `purchase_records` (`machine_code`);
CREATE INDEX `idx_purchase_item_name` ON `purchase_records` (`item_name`);
CREATE INDEX `idx_purchase_time` ON `purchase_records` (`purchase_time`);
CREATE INDEX `idx_purchase_status` ON `purchase_records` (`status`);
CREATE INDEX `idx_purchase_strategy` ON `purchase_records` (`strategy`);
CREATE INDEX `idx_purchase_machine_code_time` ON `purchase_records` (`machine_code`, `purchase_time`);
CREATE INDEX `idx_purchase_machine_code_status` ON `purchase_records` (`machine_code`, `status`);

-- monitored_items 表索引
CREATE INDEX `idx_monitored_machine_code` ON `monitored_items` (`machine_code`);
CREATE INDEX `idx_monitored_item_name` ON `monitored_items` (`item_name`);
CREATE INDEX `idx_monitored_is_enabled` ON `monitored_items` (`is_enabled`);
CREATE INDEX `idx_monitored_priority` ON `monitored_items` (`priority`);
CREATE INDEX `idx_monitored_category` ON `monitored_items` (`category`);
CREATE INDEX `idx_monitored_risk_level` ON `monitored_items` (`risk_level`);
CREATE INDEX `idx_monitored_machine_code_enabled` ON `monitored_items` (`machine_code`, `is_enabled`);
CREATE INDEX `idx_monitored_machine_code_item_name` ON `monitored_items` (`machine_code`, `item_name`);
CREATE INDEX `idx_monitored_machine_code_priority_enabled` ON `monitored_items` (`machine_code`, `priority`, `is_enabled`);
CREATE INDEX `idx_monitored_item_name_category` ON `monitored_items` (`item_name`, `category`);
