# 生日显示插件

这是一个 ClassIsland 插件，用于从 Excel 导入姓名与生日，并在主界面显示今日生日人数。

## Github项目主页

https://github.com/jinzhiyuan0327/BirthdayIsland
帮忙点个Star呗

## 功能预览

![功能预览](https://raw.githubusercontent.com/jinzhiyuan0327/BirthdayIsland/github/Pictures/1.png)
![组件设置截图](https://raw.githubusercontent.com/jinzhiyuan0327/BirthdayIsland/github/Pictures/2.png)
![导入内容截图](https://raw.githubusercontent.com/jinzhiyuan0327/BirthdayIsland/github/Pictures/3.png)

## 功能

- 从 Excel 导入姓名和生日
- 自动保存生日数据到插件配置目录
- 在 ClassIsland 主界面显示今日生日人数（可选显示姓名）
- 在插件设置页查看、删除、清空生日数据
- 处理闰年 2 月 29 日生日（可配置非闰年显示规则）

## Excel 格式

第一行必须包含表头：

| 姓名 | 生日 |
|------|------|
| 张三 | 2008-03-27 |
| 李四 | 3月27日 |

可自动识别的列名：

- 姓名列：姓名 / 名字 / Name
- 生日列：生日 / 出生日期 / Birthday / BirthDate
- （可选）班级列：班级 / Class
- （可选）备注列：备注 / Remark

支持的生日格式：

- 2008-03-27
- 2008/03/27
- 03-27
- 03/27
- 3月27日
- 2008年3月27日

## 使用方法

1. 安装插件
2. 打开 ClassIsland 设置
3. 进入“生日显示插件”
4. 点击“导入 Excel”，选择 .xlsx 文件
5. 在【应用设置】->【组件】中，将“今日生日”组件添加到主界面组件栏
6. （可选）在组件设置中开启/关闭姓名显示，设置最多显示姓名数

## 数据存储

生日数据保存在插件配置目录下的 `birthdays.json`，启动时自动读取，导入/删除/清空后自动保存。

## 开发说明

使用Opus 4.8辅助开发