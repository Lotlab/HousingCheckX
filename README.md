# HousingCheck XP

FF14查房插件

## 简介

此插件能够记录游戏内房区的房间信息，并提供了M/L房提醒与房屋信息上报功能。

本插件并非自动化插件，无法实现自动查询，仅在玩家查询时记录数据。

相对于 HousingCheck(X) 增加的功能：

- M/L房通知与语音提醒
- 整点提醒
- 房屋门牌记录
- 抽签状态记录

## 下载

点击 [Release](https://github.com/Lotlab/HousingCheckXP/releases) 下载

## 安装

直接载入即可。

若需要记录抽签人数等抽签信息，你需要先将此插件更新到 1.7.0 或以上。

在此基础之上，你需要将你的FFXIV解析插件模式改为 WinPcap 模式。

在插件设置页面中，勾上 "启用Winpcap支持" 或 "Use Winpcap-compatile library" 即可。

若你是第一次使用 WinPcap 模式，则需要安装 [Npcap](https://npcap.com/#download)

## 数据格式 

### 房屋列表信息 `/info`

```JSON
[
	{
		"time": 166666666, // 获取数据时的 Unix 时间戳
		"server": 1043, // 服务器 ID
		"area": "薰衣草苗圃", // 区域名称
		"slot": 0, // 小区ID，从0开始
		"purchase_main": 2, // 非扩展区的购买类型. 0: 不可购买, 1: 先到先得, 2: 抽选
		"purchase_sub": 2, // 扩展区的购买类型
		"region_main": 1, // 非扩展区的购买限制. 0: 无限制, 1: 仅限部队, 2: 仅限个人
		"region_sub": 1, // 扩展区的购买限制
		"houses": [
			{
				"id": 1, // 房屋编号，从1开始
				"price": 50000000, // 房屋价格
				"size": "L", // 房屋大小, S/M/L
				"isEmpty": false, // 是否为空房
				// ...
			},
			// ...
		]
	},
	// ...
]
```

### 房屋抽选信息 `/lottery`

```JSON
[
	{
		"ServerID": 1043, // 服务器 ID
		"Area": 0, // 区域ID. 0: 海雾村, 1: 薰衣草苗圃, 2: 高脚孤丘, 3: 白银乡, 4: 穹顶皓天
		"Slot": 0, // 小区ID，从0开始
		"LandID": 1, // 房屋编号，从1开始
		"Time": 166666666, // 获取数据时的 Unix 时间戳
		"State": 0, // 当前状态. 1: 可抽签, 2: 公示中, 3: 准备中
		"Participate": 0, // 参与人数
		"Winner": 0, // 中签编号
		"EndTime": 166666666, // 当前阶段结束的 Unix 时间戳
	},
	// ...
]
```
