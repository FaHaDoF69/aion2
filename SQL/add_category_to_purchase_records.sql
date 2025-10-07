-- 为购买记录表添加物品分类字段
-- 用于统计分析不同分类物品的购买情况

USE aion2_helper;

-- 添加分类字段
ALTER TABLE purchase_records 
ADD COLUMN category_id INT NULL COMMENT '物品分类ID' AFTER item_name;

-- 添加外键约束
ALTER TABLE purchase_records 
ADD CONSTRAINT fk_purchase_category 
FOREIGN KEY (category_id) REFERENCES item_categories(id) 
ON DELETE SET NULL;

-- 添加索引
CREATE INDEX idx_purchase_category_id ON purchase_records(category_id);
CREATE INDEX idx_purchase_category_time ON purchase_records(category_id, purchase_time);

-- 验证字段是否添加成功
SELECT 'purchase_records表分类字段添加成功！' AS message;

-- 显示表结构
DESCRIBE purchase_records;

-- 统计各分类的购买记录数量
SELECT 
    ic.name AS category_name,
    COUNT(pr.id) AS purchase_count,
    SUM(pr.total_amount) AS total_amount,
    SUM(pr.expected_profit) AS total_expected_profit
FROM purchase_records pr
LEFT JOIN item_categories ic ON pr.category_id = ic.id
GROUP BY pr.category_id, ic.name
ORDER BY purchase_count DESC;

