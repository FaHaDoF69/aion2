-- ============================================
-- 机器授权表
-- ============================================
-- 说明：用于控制哪些机器可以运行客户端
-- 用途：权限管理、授权控制

USE aion2_auction_db;

-- 创建机器授权表
CREATE TABLE IF NOT EXISTS machine_authorizations (
    id BIGINT AUTO_INCREMENT PRIMARY KEY COMMENT '主键ID',
    machine_code VARCHAR(100) NOT NULL COMMENT '机器码',
    is_enabled TINYINT(1) DEFAULT 1 NOT NULL COMMENT '是否启用',
    start_time DATETIME NULL COMMENT '授权开始时间',
    end_time DATETIME NULL COMMENT '授权结束时间',
    machine_name VARCHAR(100) NULL COMMENT '机器名称/备注',
    max_concurrent_runs INT DEFAULT 1 NOT NULL COMMENT '最大并发运行数',
    notes VARCHAR(500) NULL COMMENT '备注说明',
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP NOT NULL COMMENT '创建时间',
    updated_at DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP NOT NULL COMMENT '更新时间',
    last_used_at DATETIME NULL COMMENT '最后使用时间',
    UNIQUE KEY uk_machine_code (machine_code),
    KEY idx_is_enabled (is_enabled),
    KEY idx_machine_code (machine_code)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='机器授权表';

-- 插入示例数据（当前机器自动授权）
INSERT INTO machine_authorizations (machine_code, is_enabled, machine_name, notes, start_time)
SELECT 
    @current_machine_code := (
        SELECT DISTINCT machine_code 
        FROM monitored_items 
        LIMIT 1
    ) as machine_code,
    1 as is_enabled,
    '开发机器' as machine_name,
    '自动添加的授权记录' as notes,
    NOW() as start_time
WHERE @current_machine_code IS NOT NULL
ON DUPLICATE KEY UPDATE 
    is_enabled = 1,
    updated_at = NOW();

-- 查询授权表
SELECT * FROM machine_authorizations;

-- 显示创建结果
SELECT '✓ 机器授权表创建完成！' AS message;

