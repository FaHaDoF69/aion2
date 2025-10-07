-- 创建监控历史表
-- 用于存储监控到的物品机会记录

CREATE TABLE IF NOT EXISTS `monitoring_history` (
    `id` BIGINT AUTO_INCREMENT PRIMARY KEY COMMENT '记录ID',
    `machine_code` VARCHAR(32) NOT NULL COMMENT '机器码',
    `item_name` VARCHAR(200) NOT NULL COMMENT '物品名称',
    `item_level` TINYINT NULL COMMENT '物品等级 (1, 2, 3)',
    `current_price` DECIMAL(18,2) NOT NULL COMMENT '当前价格',
    `expected_price` DECIMAL(18,2) NOT NULL COMMENT '预期价格',
    `expected_profit` DECIMAL(18,2) NOT NULL COMMENT '预期利润',
    `profit_rate` DECIMAL(5,4) NOT NULL COMMENT '利润率',
    `risk_level` TINYINT NOT NULL DEFAULT 1 COMMENT '风险等级 (0=低, 1=中, 2=高, 3=极高)',
    `strategy` VARCHAR(50) NOT NULL DEFAULT '' COMMENT '监控策略',
    `seller_name` VARCHAR(100) NULL COMMENT '卖家名称',
    `quantity` INT NOT NULL DEFAULT 1 COMMENT '物品数量',
    `discovered_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '发现时间',
    `is_processed` BOOLEAN NOT NULL DEFAULT FALSE COMMENT '是否已处理',
    `process_status` TINYINT NOT NULL DEFAULT 0 COMMENT '处理状态 (0=待处理, 1=已购买, 2=已忽略, 3=价格已变化, 4=已售出, 5=自动处理失败)',
    `processed_at` TIMESTAMP NULL COMMENT '处理时间',
    `purchase_record_id` BIGINT NULL COMMENT '关联的购买记录ID',
    `monitored_item_id` BIGINT NULL COMMENT '关联的监控物品ID',
    `price_deviation` DECIMAL(10,4) NOT NULL DEFAULT 0 COMMENT '价格偏差',
    `is_abnormal_price` BOOLEAN NOT NULL DEFAULT FALSE COMMENT '是否为异常价格',
    `notes` VARCHAR(500) NULL COMMENT '备注',
    
    -- 创建索引
    INDEX `idx_machine_code` (`machine_code`),
    INDEX `idx_item_name` (`item_name`),
    INDEX `idx_discovered_at` (`discovered_at`),
    INDEX `idx_is_processed` (`is_processed`),
    INDEX `idx_process_status` (`process_status`),
    INDEX `idx_monitored_item_id` (`monitored_item_id`),
    INDEX `idx_purchase_record_id` (`purchase_record_id`),
    INDEX `idx_is_abnormal_price` (`is_abnormal_price`),
    
    -- 复合索引
    INDEX `idx_machine_discovered` (`machine_code`, `discovered_at`),
    INDEX `idx_machine_processed` (`machine_code`, `is_processed`),
    INDEX `idx_machine_status` (`machine_code`, `process_status`),
    INDEX `idx_machine_item` (`machine_code`, `item_name`),
    INDEX `idx_monitored_discovered` (`monitored_item_id`, `discovered_at`),
    
    -- 外键约束
    CONSTRAINT `fk_monitoring_history_purchase_record` 
        FOREIGN KEY (`purchase_record_id`) 
        REFERENCES `purchase_records`(`id`) 
        ON DELETE SET NULL ON UPDATE CASCADE,
        
    CONSTRAINT `fk_monitoring_history_monitored_item` 
        FOREIGN KEY (`monitored_item_id`) 
        REFERENCES `monitored_items`(`id`) 
        ON DELETE SET NULL ON UPDATE CASCADE
        
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='监控历史记录表';

-- 创建视图：监控历史统计
CREATE OR REPLACE VIEW `v_monitoring_history_stats` AS
SELECT 
    `machine_code`,
    COUNT(*) as `total_count`,
    SUM(CASE WHEN `process_status` = 1 THEN 1 ELSE 0 END) as `purchased_count`,
    SUM(CASE WHEN `process_status` = 2 THEN 1 ELSE 0 END) as `ignored_count`,
    SUM(CASE WHEN `process_status` = 0 THEN 1 ELSE 0 END) as `pending_count`,
    SUM(CASE WHEN `is_abnormal_price` = 1 THEN 1 ELSE 0 END) as `abnormal_price_count`,
    SUM(`expected_profit`) as `total_expected_profit`,
    AVG(`profit_rate`) as `average_profit_rate`,
    ROUND((SUM(CASE WHEN `is_processed` = 1 THEN 1 ELSE 0 END) / COUNT(*)) * 100, 2) as `process_rate_percent`,
    MIN(`discovered_at`) as `first_discovered`,
    MAX(`discovered_at`) as `last_discovered`
FROM `monitoring_history`
GROUP BY `machine_code`;

-- 创建视图：每日监控统计
CREATE OR REPLACE VIEW `v_daily_monitoring_stats` AS
SELECT 
    `machine_code`,
    DATE(`discovered_at`) as `discovery_date`,
    COUNT(*) as `daily_count`,
    SUM(`expected_profit`) as `daily_expected_profit`,
    AVG(`profit_rate`) as `daily_avg_profit_rate`,
    SUM(CASE WHEN `process_status` = 1 THEN 1 ELSE 0 END) as `daily_purchased`,
    SUM(CASE WHEN `is_abnormal_price` = 1 THEN 1 ELSE 0 END) as `daily_abnormal_price`
FROM `monitoring_history`
GROUP BY `machine_code`, DATE(`discovered_at`)
ORDER BY `discovery_date` DESC;

-- 插入示例数据（可选）
-- INSERT INTO `monitoring_history` (
--     `machine_code`, `item_name`, `current_price`, `expected_price`, 
--     `expected_profit`, `profit_rate`, `strategy`, `risk_level`
-- ) VALUES 
-- ('YOUR_MACHINE_CODE', '传说武器强化石', 850000, 1200000, 350000, 0.4118, '捡漏', 0),
-- ('YOUR_MACHINE_CODE', '史诗防具碎片', 450000, 600000, 150000, 0.3333, '套利', 1);

-- 查询验证
SELECT 
    COUNT(*) as total_records,
    COUNT(DISTINCT machine_code) as unique_machines,
    MIN(discovered_at) as earliest_record,
    MAX(discovered_at) as latest_record
FROM monitoring_history;
