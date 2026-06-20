namespace Template.Application.Common;

public static class Translation
{
    private static readonly Dictionary<string, string> Dictionary = new()
    {
        { "Id", "المعرف" },
        { "Name", "الاسم" },
        { "CreatedDate", "تاريخ الإنشاء" },
        { "UpdatedDate", "تاريخ التعديل" },
        { "Status", "الحالة" },
        { "Count", "العدد" },
        { "Description", "الوصف" },
        { "Search", "بحث" },
        { "StartDate", "تاريخ البدء" },
        { "EndDate", "تاريخ الانتهاء" },
        { "Date", "التاريخ" },
        { "DayName", "اسم اليوم" },
        { "Available", "متاح" },
        { "InUse", "قيد الاستخدام" },
        { "Maintenance", "صيانة" },
        { "Damaged", "تالف" },
        { "Low", "منخفض" },
        { "Medium", "متوسط" },
        { "High", "عالية" },
        { "Critical", "عالية جداً" },
        {"Sunday", "الأحد"},
        {"Monday", "الاثنين"},
        {"Tuesday", "الثلاثاء"},
        {"Wednesday", "الأربعاء"},
        {"Thursday", "الخميس"},
        {"Friday", "الجمعة"},
        {"Saturday", "السبت"},
     };
    public static string Translate(this string key) => Dictionary.GetValueOrDefault(key, key);
}