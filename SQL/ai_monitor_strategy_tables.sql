-- ====================================================
-- AI智能监控策略 - 数据库表结构
-- 创建时间：2025-10-07
-- 说明：支持AI智能分析和监控策略功能
-- ====================================================

-- 1. AI训练数据表（用于机器学习）
-- 记录所有购买和售出的历史数据，用于AI模型训练
DROP TABLE IF EXISTS ai_training_data;
CREATE TABLE ai_training_data (
    id INT AUTO_INCREMENT PRIMARY KEY COMMENT 'ID',
    item_id INT NOT NULL COMMENT '物品ID（关联items表）',
    item_name VARCHAR(200) NOT NULL COMMENT '物品名称',
    category_id INT COMMENT '物品分类ID',
    
    -- 购买信息
    purchase_price DECIMAL(15,2) NOT NULL COMMENT '购买价格',
    purchase_time DATETIME NOT NULL COMMENT '购买时间',
    purchase_quantity INT DEFAULT 1 COMMENT '购买数量',
    
    -- 市场信息（购买时）
    market_supply INT COMMENT '市场供应量（购买时）',
    market_avg_price DECIMAL(15,2) COMMENT '市场平均价（购买时）',
    price_trend VARCHAR(20) COMMENT '价格趋势（上涨/下跌/稳定）',
    
    -- 售出信息
    sale_price DECIMAL(15,2) COMMENT '售出价格',
    sale_time DATETIME COMMENT '售出时间',
    sale_quantity INT COMMENT '售出数量',
    
    -- 盈利分析
    gross_profit DECIMAL(15,2) COMMENT '毛利润',
    tax_fee DECIMAL(15,2) COMMENT '税费',
    net_profit DECIMAL(15,2) COMMENT '净利润',
    profit_rate DECIMAL(5,2) COMMENT '利润率（%）',
    hold_days INT COMMENT '持有天数',
    
    -- 策略信息
    strategy_used VARCHAR(50) COMMENT '使用的监控策略',
    ai_score INT COMMENT 'AI评分（购买时）',
    ai_recommendation VARCHAR(50) COMMENT 'AI推荐操作',
    
    -- 结果
    success BOOLEAN DEFAULT NULL COMMENT '是否成功（盈利=true，亏损=false，未售出=null）',
    machine_code VARCHAR(100) COMMENT '机器码',
    
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
    updated_at DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT '更新时间',
    
    INDEX idx_item_id (item_id),
    INDEX idx_purchase_time (purchase_time),
    INDEX idx_strategy (strategy_used),
    INDEX idx_success (success),
    INDEX idx_machine_code (machine_code)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='AI训练数据表';

-- 2. AI分析日志表
-- 记录每次AI分析的详细过程和结果
DROP TABLE IF EXISTS ai_analysis_logs;
CREATE TABLE ai_analysis_logs (
    id INT AUTO_INCREMENT PRIMARY KEY COMMENT 'ID',
    item_id INT NOT NULL COMMENT '物品ID',
    item_name VARCHAR(200) NOT NULL COMMENT '物品名称',
    monitored_item_id INT COMMENT '监控物品ID（关联monitored_items表）',
    
    -- 当前价格信息
    current_price DECIMAL(15,2) NOT NULL COMMENT '当前价格',
    target_min_price DECIMAL(15,2) COMMENT '目标最低价',
    target_max_price DECIMAL(15,2) COMMENT '目标最高价',
    
    -- AI分析得分（各维度）
    price_score INT COMMENT '价格维度得分（0-100）',
    market_score INT COMMENT '市场维度得分（0-100）',
    profit_score INT COMMENT '盈利维度得分（0-100）',
    timing_score INT COMMENT '时机维度得分（0-100）',
    history_score INT COMMENT '历史维度得分（0-100）',
    
    -- AI综合评分
    final_score INT NOT NULL COMMENT 'AI最终得分（0-100）',
    score_level VARCHAR(20) COMMENT '评分等级（强烈推荐/建议购买/可以考虑/谨慎观望/不建议）',
    
    -- AI分析详情（JSON格式）
    analysis_details TEXT COMMENT 'AI分析详情（JSON格式，包含各维度详细数据）',
    
    -- AI推荐
    recommendation VARCHAR(50) NOT NULL COMMENT 'AI推荐操作（立即购买/继续观望/忽略）',
    recommendation_reason TEXT COMMENT 'AI推荐理由',
    
    -- 用户操作
    user_action VARCHAR(20) COMMENT '用户实际操作（购买/观望/忽略/未操作）',
    action_time DATETIME COMMENT '操作时间',
    
    -- 实际结果
    actual_result VARCHAR(50) COMMENT '实际结果（盈利/亏损/未售出/未购买）',
    actual_profit DECIMAL(15,2) COMMENT '实际利润',
    
    -- 策略信息
    strategy_used VARCHAR(50) COMMENT '使用的监控策略',
    ai_mode VARCHAR(50) COMMENT 'AI分析模式（AI建议/AI辅助决策/AI自动决策等）',
    
    machine_code VARCHAR(100) COMMENT '机器码',
    analysis_time DATETIME DEFAULT CURRENT_TIMESTAMP COMMENT '分析时间',
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
    
    INDEX idx_item_id (item_id),
    INDEX idx_monitored_item_id (monitored_item_id),
    INDEX idx_final_score (final_score),
    INDEX idx_recommendation (recommendation),
    INDEX idx_analysis_time (analysis_time),
    INDEX idx_machine_code (machine_code)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='AI分析日志表';

-- 3. AI配置表
-- 存储用户的AI设置和权重配置
DROP TABLE IF EXISTS ai_configurations;
CREATE TABLE ai_configurations (
    id INT AUTO_INCREMENT PRIMARY KEY COMMENT 'ID',
    machine_code VARCHAR(100) NOT NULL COMMENT '机器码',
    
    -- AI模式设置
    ai_mode VARCHAR(50) DEFAULT 'AI辅助决策' COMMENT 'AI分析模式',
    auto_buy_enabled BOOLEAN DEFAULT FALSE COMMENT '是否启用自动购买',
    auto_buy_min_score INT DEFAULT 75 COMMENT '自动购买最低AI评分',
    auto_ignore_max_score INT DEFAULT 60 COMMENT '自动忽略最高AI评分',
    
    -- 权重配置（总和应为100）
    price_weight INT DEFAULT 30 COMMENT '价格维度权重（%）',
    market_weight INT DEFAULT 20 COMMENT '市场维度权重（%）',
    profit_weight INT DEFAULT 30 COMMENT '盈利维度权重（%）',
    timing_weight INT DEFAULT 10 COMMENT '时机维度权重（%）',
    history_weight INT DEFAULT 10 COMMENT '历史维度权重（%）',
    
    -- 安全限制
    max_single_investment DECIMAL(15,2) DEFAULT 5000000 COMMENT '单次最大投入',
    require_manual_confirm_amount DECIMAL(15,2) DEFAULT 1000000 COMMENT '需要人工确认的金额',
    enable_ai_blacklist BOOLEAN DEFAULT TRUE COMMENT '启用AI黑名单',
    
    -- 触发条件
    trigger_on_strategy BOOLEAN DEFAULT TRUE COMMENT '策略触发后启动AI',
    trigger_on_every_check BOOLEAN DEFAULT FALSE COMMENT '每次检查都进行AI分析',
    manual_trigger_only BOOLEAN DEFAULT FALSE COMMENT '仅手动触发',
    
    is_active BOOLEAN DEFAULT TRUE COMMENT '是否启用',
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
    updated_at DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT '更新时间',
    
    UNIQUE KEY uk_machine_code (machine_code)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='AI配置表';

-- 4. 监控策略定义表（枚举表）
-- 存储所有可用的监控策略
DROP TABLE IF EXISTS monitor_strategies;
CREATE TABLE monitor_strategies (
    id INT PRIMARY KEY COMMENT '策略ID',
    name VARCHAR(50) NOT NULL COMMENT '策略名称',
    display_name VARCHAR(100) NOT NULL COMMENT '显示名称',
    description VARCHAR(500) COMMENT '策略描述',
    risk_level INT DEFAULT 1 COMMENT '风险等级（1-5）',
    icon VARCHAR(50) COMMENT '图标',
    is_builtin BOOLEAN DEFAULT TRUE COMMENT '是否内置策略',
    is_active BOOLEAN DEFAULT TRUE COMMENT '是否启用',
    sort_order INT DEFAULT 0 COMMENT '排序',
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='监控策略定义表';

-- 5. 插入默认监控策略数据
INSERT INTO monitor_strategies (id, name, display_name, description, risk_level, sort_order) VALUES
(1, 'PriceMonitor', '价格监控', '监控价格是否在目标范围内（推荐）', 1, 1),
(2, 'LowPriceBuy', '低价抢购', '发现低价立即购买（激进）', 3, 2),
(3, 'StableBuy', '稳健收购', '确保利润后购买（保守）', 1, 3),
(4, 'BulkPurchase', '批量收购', '大量购买囤货（长期）', 2, 4),
(5, 'TrendWatch', '趋势观察', '观察价格走势后决策', 2, 5),
(6, 'TimedCheck', '定时检查', '定期检查不紧急物品', 1, 6),
(99, 'Custom', '自定义', '用户自定义策略', 0, 99);

-- 6. 修改monitored_items表，添加新字段（如果不存在）
-- 注意：这些是ALTER语句，如果字段已存在会报错，使用时请根据实际情况调整

-- 添加策略ID字段（关联monitor_strategies表）
ALTER TABLE monitored_items 
ADD COLUMN strategy_id INT DEFAULT 1 COMMENT '监控策略ID' AFTER monitor_strategy,
ADD INDEX idx_strategy_id (strategy_id);

-- 添加AI相关字段
ALTER TABLE monitored_items
ADD COLUMN ai_enabled BOOLEAN DEFAULT FALSE COMMENT '是否启用AI分析' AFTER strategy_id,
ADD COLUMN ai_mode VARCHAR(50) DEFAULT 'AI辅助决策' COMMENT 'AI分析模式' AFTER ai_enabled,
ADD COLUMN ai_min_score INT DEFAULT 60 COMMENT 'AI最低评分要求' AFTER ai_mode;

-- 7. 创建AI学习模型表（可选，用于存储训练好的模型参数）
DROP TABLE IF EXISTS ai_models;
CREATE TABLE ai_models (
    id INT AUTO_INCREMENT PRIMARY KEY COMMENT 'ID',
    model_name VARCHAR(100) NOT NULL COMMENT '模型名称',
    model_version VARCHAR(50) NOT NULL COMMENT '模型版本',
    model_type VARCHAR(50) COMMENT '模型类型（规则引擎/机器学习/深度学习）',
    category_id INT COMMENT '适用的物品分类（NULL表示通用）',
    
    -- 模型参数（JSON格式存储）
    model_parameters TEXT COMMENT '模型参数（JSON格式）',
    
    -- 模型性能指标
    accuracy DECIMAL(5,2) COMMENT '准确率（%）',
    precision_rate DECIMAL(5,2) COMMENT '精确率（%）',
    recall_rate DECIMAL(5,2) COMMENT '召回率（%）',
    f1_score DECIMAL(5,2) COMMENT 'F1分数',
    
    -- 训练信息
    training_samples INT COMMENT '训练样本数',
    training_date DATETIME COMMENT '训练日期',
    
    is_active BOOLEAN DEFAULT FALSE COMMENT '是否启用',
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
    updated_at DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT '更新时间',
    
    UNIQUE KEY uk_model_version (model_name, model_version)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='AI模型表';

-- 8. 创建视图：AI分析统计
CREATE OR REPLACE VIEW v_ai_analysis_stats AS
SELECT 
    DATE(analysis_time) as analysis_date,
    strategy_used,
    ai_mode,
    COUNT(*) as total_analysis,
    COUNT(CASE WHEN user_action = '购买' THEN 1 END) as purchase_count,
    COUNT(CASE WHEN actual_result = '盈利' THEN 1 END) as profit_count,
    AVG(final_score) as avg_score,
    AVG(CASE WHEN actual_profit IS NOT NULL THEN actual_profit END) as avg_profit
FROM ai_analysis_logs
GROUP BY DATE(analysis_time), strategy_used, ai_mode;

-- 9. 创建视图：训练数据统计
CREATE OR REPLACE VIEW v_training_data_stats AS
SELECT 
    item_name,
    category_id,
    strategy_used,
    COUNT(*) as total_trades,
    COUNT(CASE WHEN success = TRUE THEN 1 END) as success_count,
    COUNT(CASE WHEN success = FALSE THEN 1 END) as failure_count,
    AVG(CASE WHEN success = TRUE THEN profit_rate END) as avg_success_profit_rate,
    AVG(hold_days) as avg_hold_days,
    AVG(ai_score) as avg_ai_score
FROM ai_training_data
WHERE sale_time IS NOT NULL
GROUP BY item_name, category_id, strategy_used;

-- 10. 插入默认AI配置（示例）
-- 方式1：从machine_authorizations表获取所有已授权的机器
INSERT IGNORE INTO ai_configurations (machine_code, ai_mode, auto_buy_enabled, auto_buy_min_score) 
SELECT 
    machine_code, 
    'AI辅助决策' as ai_mode,
    FALSE as auto_buy_enabled,
    75 as auto_buy_min_score
FROM machine_authorizations 
WHERE is_enabled = TRUE;

-- 方式2：如果machine_authorizations表也没有数据，可以手动插入示例配置
-- INSERT INTO ai_configurations (machine_code, ai_mode, auto_buy_enabled, auto_buy_min_score) 
-- VALUES ('YOUR_MACHINE_CODE', 'AI辅助决策', FALSE, 75);

-- 方式3：从monitored_items表获取（如果monitored_items表有数据）
-- INSERT IGNORE INTO ai_configurations (machine_code, ai_mode, auto_buy_enabled, auto_buy_min_score) 
-- SELECT DISTINCT 
--     machine_code, 
--     'AI辅助决策',
--     FALSE,
--     75
-- FROM monitored_items;

-- ====================================================
-- 索引优化建议
-- ====================================================
-- 如果数据量大，可以考虑添加以下复合索引：

-- ai_analysis_logs表的复合索引
-- CREATE INDEX idx_analysis_item_time ON ai_analysis_logs(item_id, analysis_time);
-- CREATE INDEX idx_machine_strategy ON ai_analysis_logs(machine_code, strategy_used);

-- ai_training_data表的复合索引
-- CREATE INDEX idx_item_success ON ai_training_data(item_id, success);
-- CREATE INDEX idx_strategy_success ON ai_training_data(strategy_used, success);

-- ====================================================
-- 使用说明
-- ====================================================
-- 1. 依次执行上述SQL语句创建表结构
-- 2. 根据实际情况调整ALTER TABLE语句（如果字段已存在则跳过）
-- 3. 确保已有item_categories、items、monitored_items等基础表
-- 4. 建议在测试环境先执行，验证无误后再应用到生产环境

-- ====================================================
-- 查询示例
-- ====================================================

-- 查询AI分析成功率
-- SELECT 
--     strategy_used,
--     COUNT(*) as total,
--     COUNT(CASE WHEN actual_result = '盈利' THEN 1 END) as success,
--     ROUND(COUNT(CASE WHEN actual_result = '盈利' THEN 1 END) * 100.0 / COUNT(*), 2) as success_rate
-- FROM ai_analysis_logs
-- WHERE user_action = '购买'
-- GROUP BY strategy_used;

-- 查询各策略的平均利润
-- SELECT 
--     strategy_used,
--     COUNT(*) as trades,
--     AVG(net_profit) as avg_profit,
--     AVG(profit_rate) as avg_profit_rate
-- FROM ai_training_data
-- WHERE success = TRUE
-- GROUP BY strategy_used
-- ORDER BY avg_profit DESC;


