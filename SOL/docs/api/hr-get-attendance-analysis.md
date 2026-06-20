# GetAttendanceAnalysis

## Endpoint

- Method: `GET`
- URL: `/api/HR/GetAttendanceAnalysis`

## Query Parameters

| Name | Type | Required | Description |
| --- | --- | --- | --- |
| `year` | `int` | Yes | السنة المطلوبة للتحليل. |
| `month` | `int` | Yes | الشهر المطلوب للتحليل ضمن السنة، من `1` إلى `12`. |

### Example Request

```http
GET /api/HR/GetAttendanceAnalysis?year=2026&month=5
```

## Important Notes

- هذا الراوت أصبح **breaking change** على مستوى الـ response.
- المدخلات بقيت نفسها: `year` و `month`.
- القسم `Summary.Monthly` يحسب بيانات الشهر المطلوب.
- القسم `Summary.Yearly` يحسب بيانات السنة كاملة لنفس قيمة `year`.
- نسب الحضور والغياب تعتمد على السجلات التي حالتها `Present` أو `Absent`.
- `NotTaken` يدخل ضمن الإحصائيات كعدد سجلات، لكنه لا يدخل في حساب نسب الحضور/الغياب.
- `NetIncentives = TotalRewards - TotalDiscounts`.
- كل الـ charts جاهزة للفرونت مباشرة من دون إعادة تجميع.

## Response Shape

```json
{
  "success": true,
  "data": {
    "metadata": {
      "year": 2026,
      "month": 5,
      "monthName": "أيار",
      "timeZone": "Asia/Damascus",
      "monthStartDate": "2026-05-01",
      "monthEndDate": "2026-05-31",
      "yearStartDate": "2026-01-01",
      "yearEndDate": "2026-12-31",
      "generatedAt": "2026-05-12T10:35:00+03:00"
    },
    "summary": {
      "monthly": {},
      "yearly": {}
    },
    "highlights": {
      "bestMonthlyCommitment": {},
      "mostMonthlyDelayed": {},
      "mostMonthlyAbsent": {},
      "topYearlyRewarded": {}
    },
    "rankings": {
      "topMonthlyCommittedEmployees": [],
      "mostMonthlyDelayedEmployees": [],
      "mostMonthlyAbsentEmployees": [],
      "topYearlyRewardedEmployees": []
    },
    "charts": {
      "monthlyAttendanceStatus": {},
      "monthlyDailyAttendanceTrend": {},
      "monthlyWeekdayPerformance": {},
      "monthlyAbsenceTypes": {},
      "yearlyAttendanceVolume": {},
      "yearlyCommitmentTrend": {},
      "yearlyIncentivesTrend": {}
    }
  }
}
```

## Metadata

| Field | Type | Description |
| --- | --- | --- |
| `year` | `int` | السنة المطلوبة. |
| `month` | `int` | الشهر المطلوب. |
| `monthName` | `string` | اسم الشهر بالعربية. |
| `timeZone` | `string` | المنطقة الزمنية المعتمدة في التحليل. |
| `monthStartDate` | `string` | بداية الشهر بصيغة `yyyy-MM-dd`. |
| `monthEndDate` | `string` | نهاية الشهر بصيغة `yyyy-MM-dd`. |
| `yearStartDate` | `string` | بداية السنة بصيغة `yyyy-MM-dd`. |
| `yearEndDate` | `string` | نهاية السنة بصيغة `yyyy-MM-dd`. |
| `generatedAt` | `datetimeoffset` | وقت إنشاء التقرير. |

## Summary

القسمان `monthly` و `yearly` لهما نفس البنية:

| Field | Type | Description |
| --- | --- | --- |
| `totalEmployees` | `int` | عدد الموظفين الفعّالين. |
| `employeesWithRecords` | `int` | عدد الموظفين الذين لديهم سجلات دوام ضمن الفترة. |
| `employeesWithAbsences` | `int` | عدد الموظفين الذين لديهم غياب ضمن الفترة. |
| `employeesWithDelay` | `int` | عدد الموظفين الذين لديهم تأخير ضمن الفترة. |
| `totalWorkDayRecords` | `int` | مجموع سجلات الدوام ضمن الفترة. |
| `presentDays` | `int` | عدد السجلات بحالة `Present`. |
| `absentDays` | `int` | عدد السجلات بحالة `Absent`. |
| `notTakenDays` | `int` | عدد السجلات بحالة `NotTaken` أو `null`. |
| `lateDays` | `int` | عدد السجلات التي تحتوي تأخير. |
| `totalDelayMinutes` | `int` | مجموع دقائق التأخير. |
| `averageDelayMinutes` | `int` | متوسط دقائق التأخير للسجلات المتأخرة فقط. |
| `attendanceRate` | `double` | نسبة الحضور من مجموع `Present + Absent`. |
| `absenceRate` | `double` | نسبة الغياب من مجموع `Present + Absent`. |
| `lateRate` | `double` | نسبة التأخير من أيام الحضور فقط. |
| `paidAbsences` | `int` | عدد الغيابات المدفوعة. |
| `unpaidOneDayAbsences` | `int` | عدد الغيابات التي سببت خصم يوم. |
| `unpaidTwoDayAbsences` | `int` | عدد الغيابات التي سببت خصم يومين. |
| `totalRewards` | `long` | مجموع قيم المكافآت في الفترة. |
| `totalDiscounts` | `long` | مجموع قيم الخصومات في الفترة. |
| `netIncentives` | `long` | ناتج `totalRewards - totalDiscounts`. |
| `totalWarnings` | `int` | عدد التنبيهات ضمن الفترة. |

## Highlights

كل عنصر ضمن `highlights`:

| Field | Type | Description |
| --- | --- | --- |
| `title` | `string` | عنوان الكرت. |
| `metricKey` | `string` | مفتاح ثابت للقياس. |
| `metricLabel` | `string` | اسم القياس للعرض. |
| `metricValue` | `double` | قيمة القياس. |
| `employee` | `EmployeeAnalyticsDto` | بيانات الموظف المرتبط بالكرت. |

### Current Highlight Keys

- `bestMonthlyCommitment`
- `mostMonthlyDelayed`
- `mostMonthlyAbsent`
- `topYearlyRewarded`

## Rankings

كل قائمة ضمن `rankings` هي `List<EmployeeAnalyticsDto>`:

- `topMonthlyCommittedEmployees`
- `mostMonthlyDelayedEmployees`
- `mostMonthlyAbsentEmployees`
- `topYearlyRewardedEmployees`

### EmployeeAnalyticsDto

| Field | Type | Description |
| --- | --- | --- |
| `employeeId` | `guid` | رقم الموظف. |
| `fullName` | `string` | الاسم الكامل. |
| `imageUrl` | `string?` | صورة الموظف الأصلية. |
| `commitmentRate` | `double` | نسبة الالتزام ضمن الفترة الخاصة بهذه القائمة. |
| `presentDays` | `int` | عدد أيام الحضور. |
| `absentDays` | `int` | عدد أيام الغياب. |
| `notTakenDays` | `int` | عدد الأيام غير المسجلة. |
| `lateDays` | `int` | عدد أيام التأخير. |
| `totalDelayMinutes` | `int` | مجموع دقائق التأخير. |
| `averageDelayMinutes` | `int` | متوسط التأخير. |
| `rewards` | `long` | مجموع المكافآت. |
| `discounts` | `long` | مجموع الخصومات. |
| `netIncentives` | `long` | صافي الحوافز. |
| `warnings` | `int` | عدد التنبيهات. |

## Charts Contract

كل عنصر ضمن `charts` له نفس البنية العامة:

```json
{
  "id": "chart-id",
  "title": "عنوان الرسم",
  "type": "line | bar | donut",
  "unit": "days | percent | value",
  "labels": ["..."],
  "palette": ["#hex", "#hex"],
  "series": [
    {
      "key": "present",
      "label": "حاضر",
      "color": "#16a34a",
      "data": [10, 12, 9]
    }
  ]
}
```

### Available Charts

| Field | `id` | Type | Purpose |
| --- | --- | --- | --- |
| `monthlyAttendanceStatus` | `monthly-attendance-status` | `donut` | توزيع الحضور/الغياب/غير المسجل في الشهر. |
| `monthlyDailyAttendanceTrend` | `monthly-daily-attendance-trend` | `line` | الحضور والغياب والتأخير لكل يوم ضمن الشهر. |
| `monthlyWeekdayPerformance` | `monthly-weekday-performance` | `bar` | نسب الحضور والغياب والتأخير حسب يوم الأسبوع. |
| `monthlyAbsenceTypes` | `monthly-absence-types` | `donut` | توزيع أنواع الغياب في الشهر. |
| `yearlyAttendanceVolume` | `yearly-attendance-volume` | `bar` | المقارنة الشهرية للحضور والغياب والتأخير على مدار السنة. |
| `yearlyCommitmentTrend` | `yearly-commitment-trend` | `line` | اتجاه نسب الحضور والغياب على مدار السنة. |
| `yearlyIncentivesTrend` | `yearly-incentives-trend` | `bar` | المكافآت والخصومات وصافي الحوافز لكل شهر. |

## Frontend Notes

- استخدم `metadata.monthName` و `metadata.year` في عنوان الصفحة.
- استخدم `summary.monthly` لبطاقات KPI الرئيسية.
- استخدم `highlights` للكروت السريعة أعلى الصفحة.
- استخدم `rankings` للجداول أو قوائم Top 5.
- استخدم `charts.*.type` لتحديد نوع الرسم مباشرة.
- استخدم `charts.*.labels` لمحور X أو التصنيفات.
- استخدم `charts.*.series[].data` مباشرة بدون أي تحويل إضافي.
