-- =====================================================
-- 监控历史表完整SQL语句集合
-- =====================================================

-- 1. 创建监控历史表
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

-- 2. 创建统计视图
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

-- 3. 创建每日统计视图
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

-- =====================================================
-- 示例数据插入 (请替换 YOUR_MACHINE_CODE 为实际机器码)
-- =====================================================

-- 获取当前机器码的方法：
-- 1. 运行程序，在日志中查看 "当前机器码: XXXXXXXX"
-- 2. 或查询现有数据：SELECT DISTINCT machine_code FROM monitored_items LIMIT 1;

-- 插入示例监控历史数据
INSERT INTO `monitoring_history` (
    `machine_code`, `item_name`, `item_level`, `current_price`, `expected_price`, 
    `expected_profit`, `profit_rate`, `strategy`, `risk_level`, 
    `seller_name`, `quantity`, `discovered_at`, `is_processed`, `process_status`
) VALUES 
-- 最近发现的机会
('YOUR_MACHINE_CODE', '传说武器强化石', 3, 850000, 1200000, 350000, 0.4118, '捡漏', 0, '玩家A', 1, NOW() - INTERVAL 1 HOUR, TRUE, 1),
('YOUR_MACHINE_CODE', '史诗防具碎片', 2, 450000, 600000, 150000, 0.3333, '套利', 1, '玩家B', 2, NOW() - INTERVAL 2 HOUR, TRUE, 1),
('YOUR_MACHINE_CODE', '稀有材料包', 2, 120000, 180000, 60000, 0.5000, '趋势', 0, '玩家C', 5, NOW() - INTERVAL 3 HOUR, FALSE, 0),

-- 昨天的机会
('YOUR_MACHINE_CODE', '魔法水晶', 1, 75000, 95000, 20000, 0.2667, '批量', 1, '玩家D', 10, NOW() - INTERVAL 1 DAY, TRUE, 1),
('YOUR_MACHINE_CODE', '装备碎片', 2, 200000, 280000, 80000, 0.4000, '捡漏', 0, '玩家E', 3, NOW() - INTERVAL 1 DAY - INTERVAL 2 HOUR, TRUE, 2),
('YOUR_MACHINE_CODE', '强化石', 2, 300000, 350000, 50000, 0.1667, '套利', 2, '玩家F', 2, NOW() - INTERVAL 1 DAY - INTERVAL 4 HOUR, FALSE, 3),

-- 前天的机会
('YOUR_MACHINE_CODE', '宝石', 2, 500000, 750000, 250000, 0.5000, '捡漏', 0, '玩家G', 1, NOW() - INTERVAL 2 DAY, TRUE, 1),
('YOUR_MACHINE_CODE', '药水', 1, 15000, 20000, 5000, 0.3333, '批量', 1, '玩家H', 20, NOW() - INTERVAL 2 DAY - INTERVAL 1 HOUR, TRUE, 2),
('YOUR_MACHINE_CODE', '装备', 3, 800000, 1000000, 200000, 0.2500, '趋势', 2, '玩家I', 1, NOW() - INTERVAL 2 DAY - INTERVAL 3 HOUR, FALSE, 0),

-- 一周前的机会
('YOUR_MACHINE_CODE', '传说装备', 3, 1500000, 2000000, 500000, 0.3333, '捡漏', 1, '玩家J', 1, NOW() - INTERVAL 7 DAY, TRUE, 1),
('YOUR_MACHINE_CODE', '材料', 2, 80000, 100000, 20000, 0.2500, '套利', 1, '玩家K', 8, NOW() - INTERVAL 7 DAY - INTERVAL 2 HOUR, TRUE, 1),
('YOUR_MACHINE_CODE', '强化材料', 2, 180000, 220000, 40000, 0.2222, '趋势', 2, '玩家L', 4, NOW() - INTERVAL 7 DAY - INTERVAL 5 HOUR, FALSE, 4);

-- =====================================================
-- 常用查询语句
-- =====================================================

-- 1. 查询基本统计信息
SELECT 
    COUNT(*) as total_records,
    COUNT(DISTINCT machine_code) as unique_machines,
    MIN(discovered_at) as earliest_record,
    MAX(discovered_at) as latest_record
FROM monitoring_history;

-- 2. 查询当前机器的监控统计
SELECT * FROM v_monitoring_history_stats 
WHERE machine_code = 'YOUR_MACHINE_CODE';

-- 3. 查询最近7天的监控记录
SELECT 
    item_name,
    item_level,
    current_price,
    expected_price,
    expected_profit,
    profit_rate,
    strategy,
    CASE process_status
        WHEN 0 THEN '待处理'
        WHEN 1 THEN '已购买'
        WHEN 2 THEN '已忽略'
        WHEN 3 THEN '价格已变化'
        WHEN 4 THEN '已售出'
        WHEN 5 THEN '自动处理失败'
    END as status_text,
    discovered_at
FROM monitoring_history 
WHERE machine_code = 'YOUR_MACHINE_CODE'
  AND discovered_at >= DATE_SUB(NOW(), INTERVAL 7 DAY)
ORDER BY discovered_at DESC;

-- 4. 查询未处理的机会
SELECT 
    item_name,
    item_level,
    current_price,
    expected_price,
    expected_profit,
    profit_rate,
    strategy,
    seller_name,
    discovered_at,
    TIMESTAMPDIFF(MINUTE, discovered_at, NOW()) as minutes_ago
FROM monitoring_history 
WHERE machine_code = 'YOUR_MACHINE_CODE'
  AND is_processed = FALSE
ORDER BY discovered_at DESC;

-- 5. 查询高利润机会（利润率>30%）
SELECT 
    item_name,
    item_level,
    current_price,
    expected_price,
    expected_profit,
    ROUND(profit_rate * 100, 2) as profit_percent,
    strategy,
    discovered_at
FROM monitoring_history 
WHERE machine_code = 'YOUR_MACHINE_CODE'
  AND profit_rate > 0.30
ORDER BY profit_rate DESC;

-- 6. 查询每日监控效果
SELECT 
    discovery_date,
    daily_count as '发现数量',
    daily_purchased as '已购买',
    daily_expected_profit as '预期利润',
    ROUND(daily_avg_profit_rate * 100, 2) as '平均利润率%'
FROM v_daily_monitoring_stats 
WHERE machine_code = 'YOUR_MACHINE_CODE'
  AND discovery_date >= DATE_SUB(CURDATE(), INTERVAL 30 DAY)
ORDER BY discovery_date DESC;

-- 7. 查询物品监控效果排行
SELECT 
    item_name,
    COUNT(*) as discovery_count,
    AVG(profit_rate) as avg_profit_rate,
    SUM(expected_profit) as total_expected_profit,
    SUM(CASE WHEN process_status = 1 THEN 1 ELSE 0 END) as purchased_count,
    ROUND((SUM(CASE WHEN process_status = 1 THEN 1 ELSE 0 END) / COUNT(*)) * 100, 2) as purchase_rate
FROM monitoring_history 
WHERE machine_code = 'YOUR_MACHINE_CODE'
GROUP BY item_name
HAVING discovery_count >= 2
ORDER BY avg_profit_rate DESC;

-- 8. 查询策略效果分析
SELECT 
    strategy,
    COUNT(*) as total_opportunities,
    AVG(profit_rate) as avg_profit_rate,
    SUM(expected_profit) as total_expected_profit,
    SUM(CASE WHEN process_status = 1 THEN 1 ELSE 0 END) as purchased_count,
    ROUND((SUM(CASE WHEN process_status = 1 THEN 1 ELSE 0 END) / COUNT(*)) * 100, 2) as success_rate
FROM monitoring_history 
WHERE machine_code = 'YOUR_MACHINE_CODE'
GROUP BY strategy
ORDER BY avg_profit_rate DESC;

-- 9. 查询异常价格记录
SELECT 
    item_name,
    item_level,
    current_price,
    expected_price,
    price_deviation,
    ABS(price_deviation) * 100 as deviation_percent,
    discovered_at,
    notes
FROM monitoring_history 
WHERE machine_code = 'YOUR_MACHINE_CODE'
  AND is_abnormal_price = TRUE
ORDER BY ABS(price_deviation) DESC;

-- 10. 分页查询（示例：第1页，每页20条）
SELECT 
    item_name,
    item_level,
    current_price,
    expected_price,
    expected_profit,
    ROUND(profit_rate * 100, 2) as profit_percent,
    strategy,
    CASE process_status
        WHEN 0 THEN '待处理'
        WHEN 1 THEN '已购买'
        WHEN 2 THEN '已忽略'
        WHEN 3 THEN '价格已变化'
        WHEN 4 THEN '已售出'
        WHEN 5 THEN '自动处理失败'
    END as status_text,
    discovered_at
FROM monitoring_history 
WHERE machine_code = 'YOUR_MACHINE_CODE'
ORDER BY discovered_at DESC
LIMIT 20 OFFSET 0;

-- =====================================================
-- 维护和优化语句
-- =====================================================

-- 清理30天前的记录（可选）
-- DELETE FROM monitoring_history 
-- WHERE discovered_at < DATE_SUB(NOW(), INTERVAL 30 DAY);

-- 重建索引（如果需要）
-- ALTER TABLE monitoring_history DROP INDEX idx_machine_discovered;
-- ALTER TABLE monitoring_history ADD INDEX idx_machine_discovered (machine_code, discovered_at);

-- 查看表状态
SHOW TABLE STATUS LIKE 'monitoring_history';

-- 查看索引使用情况
SHOW INDEX FROM monitoring_history;

-- =====================================================
-- 物品等级相关查询
-- =====================================================

-- 11. 按物品等级统计
SELECT 
    item_level,
    COUNT(*) as total_count,
    AVG(profit_rate) as avg_profit_rate,
    SUM(expected_profit) as total_expected_profit,
    SUM(CASE WHEN process_status = 1 THEN 1 ELSE 0 END) as purchased_count,
    ROUND((SUM(CASE WHEN process_status = 1 THEN 1 ELSE 0 END) / COUNT(*)) * 100, 2) as purchase_rate,
    AVG(current_price) as avg_current_price,
    MAX(current_price) as max_current_price,
    MIN(current_price) as min_current_price
FROM monitoring_history 
WHERE machine_code = 'YOUR_MACHINE_CODE'
  AND item_level IS NOT NULL
GROUP BY item_level
ORDER BY avg_profit_rate DESC;

-- 12. 查询特定等级的物品机会
SELECT 
    item_name,
    item_level,
    current_price,
    expected_price,
    expected_profit,
    ROUND(profit_rate * 100, 2) as profit_percent,
    strategy,
    seller_name,
    discovered_at
FROM monitoring_history 
WHERE machine_code = 'YOUR_MACHINE_CODE'
  AND item_level = '传说'  -- 可替换为：普通、高级、稀有、史诗、传说
ORDER BY profit_rate DESC;

-- 13. 物品等级价值分析
SELECT 
    item_level,
    COUNT(DISTINCT item_name) as unique_items,
    COUNT(*) as total_opportunities,
    AVG(current_price) as avg_price,
    AVG(profit_rate) as avg_profit_rate,
    SUM(expected_profit) as total_expected_profit
FROM monitoring_history 
WHERE machine_code = 'YOUR_MACHINE_CODE'
  AND item_level IS NOT NULL
GROUP BY item_level
ORDER BY avg_price DESC;
