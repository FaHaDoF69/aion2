-- 创建被监控物品表
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
CREATE INDEX `idx_machine_code` ON `monitored_items` (`machine_code`);
CREATE INDEX `idx_item_name` ON `monitored_items` (`item_name`);
CREATE INDEX `idx_is_enabled` ON `monitored_items` (`is_enabled`);
CREATE INDEX `idx_priority` ON `monitored_items` (`priority`);
CREATE INDEX `idx_category` ON `monitored_items` (`category`);
CREATE INDEX `idx_risk_level` ON `monitored_items` (`risk_level`);

-- 创建复合索引
CREATE INDEX `idx_machine_code_enabled` ON `monitored_items` (`machine_code`, `is_enabled`);
CREATE INDEX `idx_machine_code_item_name` ON `monitored_items` (`machine_code`, `item_name`);
CREATE INDEX `idx_machine_code_priority_enabled` ON `monitored_items` (`machine_code`, `priority`, `is_enabled`);
CREATE INDEX `idx_item_name_category` ON `monitored_items` (`item_name`, `category`);
