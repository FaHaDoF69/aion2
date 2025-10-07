-- 示例监控物品数据
-- 注意：请将 'YOUR_MACHINE_CODE' 替换为实际的机器码

-- 插入示例监控物品数据
INSERT INTO `monitored_items` (
    `machine_code`, `item_name`, `category`, `item_level`, `target_min_price`, `target_max_price`, 
    `priority`, `is_enabled`, `auto_purchase_enabled`, `monitor_strategy`, `risk_level`, `notes`
) VALUES 
-- 高价值装备强化材料
('YOUR_MACHINE_CODE', '传说武器强化石', '强化材料', 3, 800000.00, 1200000.00, 9, 1, 1, '价格监控', 2, '高价值强化材料，利润丰厚'),
('YOUR_MACHINE_CODE', '史诗防具强化石', '强化材料', 2, 500000.00, 800000.00, 8, 1, 1, '价格监控', 2, '防具强化必需品'),
('YOUR_MACHINE_CODE', '神话饰品强化石', '强化材料', 3, 1000000.00, 1500000.00, 10, 1, 0, '价格监控', 3, '极高价值，手动购买'),

-- 稀有材料
('YOUR_MACHINE_CODE', '龙鳞碎片', '稀有材料', 2, 200000.00, 350000.00, 7, 1, 1, '价格监控', 2, '制作高级装备材料'),
('YOUR_MACHINE_CODE', '魔法水晶', '稀有材料', 2, 150000.00, 250000.00, 6, 1, 1, '价格监控', 1, '常用魔法材料'),
('YOUR_MACHINE_CODE', '精灵之泪', '稀有材料', 2, 300000.00, 500000.00, 7, 1, 1, '价格监控', 2, '炼金术高级材料'),

-- 装备碎片
('YOUR_MACHINE_CODE', '传说武器碎片', '装备碎片', 3, 400000.00, 600000.00, 8, 1, 1, '价格监控', 2, '合成传说武器'),
('YOUR_MACHINE_CODE', '史诗防具碎片', '装备碎片', 2, 250000.00, 400000.00, 7, 1, 1, '价格监控', 2, '合成史诗防具'),
('YOUR_MACHINE_CODE', '神话饰品碎片', '装备碎片', 3, 600000.00, 900000.00, 9, 1, 0, '价格监控', 3, '极稀有，谨慎购买'),

-- 宝石类
('YOUR_MACHINE_CODE', '力量红宝石', '宝石', 2, 100000.00, 180000.00, 5, 1, 1, '价格监控', 1, '常用属性宝石'),
('YOUR_MACHINE_CODE', '敏捷蓝宝石', '宝石', 2, 120000.00, 200000.00, 5, 1, 1, '价格监控', 1, '敏捷职业必需'),
('YOUR_MACHINE_CODE', '智力紫宝石', '宝石', 2, 110000.00, 190000.00, 5, 1, 1, '价格监控', 1, '法师职业必需'),
('YOUR_MACHINE_CODE', '完美钻石', '宝石', 3, 500000.00, 800000.00, 9, 1, 0, '价格监控', 3, '顶级宝石，手动确认'),

-- 药水类
('YOUR_MACHINE_CODE', '高级生命药水', '药水', 2, 50000.00, 80000.00, 4, 1, 1, '价格监控', 1, '日常消耗品'),
('YOUR_MACHINE_CODE', '高级魔法药水', '药水', 2, 45000.00, 75000.00, 4, 1, 1, '价格监控', 1, '法师职业消耗'),
('YOUR_MACHINE_CODE', '经验增益药水', '药水', 2, 80000.00, 120000.00, 6, 1, 1, '价格监控', 1, '练级必备'),

-- 特殊物品
('YOUR_MACHINE_CODE', '时空裂缝钥匙', '特殊物品', 2, 300000.00, 500000.00, 8, 1, 1, '价格监控', 2, '副本入场券'),
('YOUR_MACHINE_CODE', '神秘宝箱', '特殊物品', 2, 200000.00, 350000.00, 6, 1, 1, '价格监控', 2, '随机奖励宝箱'),
('YOUR_MACHINE_CODE', '重置卷轴', '特殊物品', 2, 150000.00, 250000.00, 5, 1, 1, '价格监控', 1, '技能重置道具'),

-- 禁用的监控物品示例
('YOUR_MACHINE_CODE', '普通铁矿石', '基础材料', 1, 10000.00, 20000.00, 2, 0, 0, '价格监控', 1, '暂时禁用监控'),
('YOUR_MACHINE_CODE', '废弃的装备', '垃圾物品', 1, 5000.00, 15000.00, 1, 0, 0, '价格监控', 1, '低价值物品，已禁用');

-- 更新一些物品的发现信息（模拟已经监控过的数据）
UPDATE `monitored_items` 
SET 
    `last_monitored_at` = DATE_SUB(NOW(), INTERVAL FLOOR(RAND() * 24) HOUR),
    `last_found_at` = DATE_SUB(NOW(), INTERVAL FLOOR(RAND() * 48) HOUR),
    `last_found_price` = `target_min_price` + ((`target_max_price` - `target_min_price`) * RAND()),
    `total_found_count` = FLOOR(RAND() * 20) + 1,
    `total_purchase_count` = FLOOR(RAND() * 5)
WHERE `machine_code` = 'YOUR_MACHINE_CODE' 
AND `item_name` IN ('传说武器强化石', '史诗防具强化石', '龙鳞碎片', '魔法水晶', '力量红宝石');
