-- 创建AI配置表
-- 用于存储每台机器的AI智能分析设置

USE aion2_helper;

-- 删除旧表（如果存在）
DROP TABLE IF EXISTS ai_configurations;

-- 创建AI配置表
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
    
    UNIQUE KEY uk_machine_code (machine_code),
    INDEX idx_is_active (is_active),
    INDEX idx_machine_code_active (machine_code, is_active)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='AI配置表';

-- 检查表是否创建成功
SELECT 'AI配置表创建成功！' AS message;
SELECT COUNT(*) AS table_exists 
FROM information_schema.TABLES 
WHERE TABLE_SCHEMA = 'aion2_helper' 
  AND TABLE_NAME = 'ai_configurations';

-- 显示表结构
DESCRIBE ai_configurations;

