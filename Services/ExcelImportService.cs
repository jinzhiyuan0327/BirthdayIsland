using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using BirthdayIsland.Models;
using ClosedXML.Excel;

namespace BirthdayIsland.Services;

public class ExcelImportService
{
    private const int DefaultYear = 2000;

    private static readonly string[] NameHeaders =
        { "姓名", "名字", "name" };

    private static readonly string[] BirthdayHeaders =
        { "生日", "出生日期", "birthday", "birthdate" };

    private static readonly string[] ClassHeaders =
        { "班级", "class", "classname" };

    private static readonly string[] RemarkHeaders =
        { "备注", "remark", "note" };

    public BirthdayImportResult Import(string filePath)
    {
        var result = new BirthdayImportResult();

        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
        {
            result.Fatal = true;
            result.Message = "导入失败：文件不存在，请重新选择。";
            return result;
        }

        XLWorkbook workbook;
        try
        {
            workbook = new XLWorkbook(filePath);
        }
        catch (IOException)
        {
            result.Fatal = true;
            result.Message = "导入失败：文件可能正在被其它程序占用，请关闭后重试。";
            return result;
        }
        catch (Exception)
        {
            result.Fatal = true;
            result.Message = "导入失败：文件格式错误或已损坏，请确认是有效的 .xlsx 文件。";
            return result;
        }

        using (workbook)
        {
            var sheet = workbook.Worksheets.FirstOrDefault();
            if (sheet == null)
            {
                result.Fatal = true;
                result.Message = "导入失败：表格中没有工作表。";
                return result;
            }

            var range = sheet.RangeUsed();
            if (range == null)
            {
                result.Fatal = true;
                result.Message = "导入失败：表格为空。";
                return result;
            }

            // 第一行作为表头
            var headerRow = range.FirstRow();
            int nameCol = FindColumn(headerRow, NameHeaders);
            int birthdayCol = FindColumn(headerRow, BirthdayHeaders);
            int classCol = FindColumn(headerRow, ClassHeaders);
            int remarkCol = FindColumn(headerRow, RemarkHeaders);

            if (nameCol == 0)
            {
                result.Fatal = true;
                result.Message = "导入失败：未找到“姓名”列，请确认 Excel 第一行包含“姓名”或“Name”。";
                return result;
            }

            if (birthdayCol == 0)
            {
                result.Fatal = true;
                result.Message = "导入失败：未找到“生日”列，请确认 Excel 第一行包含“生日”或“Birthday”。";
                return result;
            }

            // 从第二行开始解析数据
            foreach (var row in range.Rows().Skip(1))
            {
                int rowNumber = row.RowNumber();
                var nameCell = row.Cell(nameCol);
                string name = nameCell.GetString().Trim();

                // 姓名为空：跳过空行（不计为错误）
                if (string.IsNullOrWhiteSpace(name))
                    continue;

                var birthdayCell = row.Cell(birthdayCol);
                string rawBirthday = birthdayCell.GetString().Trim();

                if (!TryParseBirthday(birthdayCell, out var birthday))
                {
                    result.Errors.Add(new BirthdayImportError
                    {
                        RowNumber = rowNumber,
                        RawName = name,
                        RawBirthday = rawBirthday,
                        Reason = string.IsNullOrWhiteSpace(rawBirthday)
                            ? "生日为空"
                            : $"无法识别生日格式：{rawBirthday}"
                    });
                    continue;
                }

                result.People.Add(new BirthdayPerson
                {
                    Name = name,
                    RawBirthday = rawBirthday,
                    Birthday = birthday,
                    ClassName = classCol > 0 ? row.Cell(classCol).GetString().Trim() : null,
                    Remark = remarkCol > 0 ? row.Cell(remarkCol).GetString().Trim() : null
                });
            }
        }

        if (result.SuccessCount == 0 && result.ErrorCount == 0)
        {
            result.Message = "导入完成：表格中没有有效数据。";
        }
        else
        {
            result.Message = $"导入完成：成功 {result.SuccessCount} 条，失败 {result.ErrorCount} 条。";
        }

        return result;
    }

    /// <summary>在表头行中查找匹配任一别名的列号（1 基）；找不到返回 0。</summary>
    private static int FindColumn(IXLRangeRow headerRow, string[] aliases)
    {
        foreach (var cell in headerRow.Cells())
        {
            string header = cell.GetString().Trim();
            if (string.IsNullOrEmpty(header))
                continue;

            foreach (var alias in aliases)
            {
                if (string.Equals(header, alias, StringComparison.OrdinalIgnoreCase))
                    return cell.Address.ColumnNumber;
            }
        }
        return 0;
    }

    private static bool TryParseBirthday(IXLCell cell, out DateTime birthday)
    {
        birthday = default;

        // 1) Excel 原生日期单元格
        if (cell.DataType == XLDataType.DateTime)
        {
            try
            {
                birthday = cell.GetDateTime();
                return true;
            }
            catch
            {
                // 继续按文本解析
            }
        }

        string raw = cell.GetString().Trim();
        return TryParseBirthdayText(raw, out birthday);
    }

    /// <summary>
    /// 解析多种生日文本格式：
    /// 2008-03-27 / 2008/03/27 / 2008.03.27 / 03-27 / 03/27 / 3月27日 / 2008年3月27日。
    /// 仅有月日时使用默认年份 2000。
    /// </summary>
    public static bool TryParseBirthdayText(string raw, out DateTime birthday)
    {
        birthday = default;
        if (string.IsNullOrWhiteSpace(raw))
            return false;

        raw = raw.Trim();

        // 中文“X年X月X日 / X月X日”
        var cn = Regex.Match(raw, @"^(?:(\d{1,4})\s*年)?\s*(\d{1,2})\s*月\s*(\d{1,2})\s*日?$");
        if (cn.Success)
        {
            int year = cn.Groups[1].Success ? int.Parse(cn.Groups[1].Value) : DefaultYear;
            int month = int.Parse(cn.Groups[2].Value);
            int day = int.Parse(cn.Groups[3].Value);
            return TryBuildDate(year, month, day, out birthday);
        }

        // 用 - / . 分隔的数字
        var parts = raw.Split(new[] { '-', '/', '.' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 3 &&
            int.TryParse(parts[0], out int y3) &&
            int.TryParse(parts[1], out int m3) &&
            int.TryParse(parts[2], out int d3))
        {
            return TryBuildDate(y3, m3, d3, out birthday);
        }

        if (parts.Length == 2 &&
            int.TryParse(parts[0], out int m2) &&
            int.TryParse(parts[1], out int d2))
        {
            return TryBuildDate(DefaultYear, m2, d2, out birthday);
        }

        // 兜底：交给 .NET 通用解析
        if (DateTime.TryParse(raw, CultureInfo.CurrentCulture,
                DateTimeStyles.None, out var parsed) ||
            DateTime.TryParse(raw, CultureInfo.InvariantCulture,
                DateTimeStyles.None, out parsed))
        {
            birthday = parsed;
            return true;
        }

        return false;
    }

    private static bool TryBuildDate(int year, int month, int day, out DateTime date)
    {
        date = default;
        if (year < 1 || year > 9999 || month < 1 || month > 12 || day < 1 || day > 31)
            return false;

        // 闰日：若给定年份不是闰年但写了 2/29，则归一化到闰年 2000，便于按月日比较。
        if (month == 2 && day == 29 && !DateTime.IsLeapYear(year))
            year = DefaultYear;

        if (day > DateTime.DaysInMonth(year, month))
            return false;

        date = new DateTime(year, month, day);
        return true;
    }
}