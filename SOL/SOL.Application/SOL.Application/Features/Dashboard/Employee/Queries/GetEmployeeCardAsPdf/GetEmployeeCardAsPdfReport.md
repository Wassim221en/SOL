# GetEmployeeCardAsPdf - Frontend Report

## Endpoint

```http
GET api/Employee/GetEmployeeCardAsPdf
```

هذا الروت يرجع ملف PDF مباشرة لبطاقة الموظف.

ملف الـ PDF يحتوي على صفحتين:

1. البطاقة العربية.
2. البطاقة الإنكليزية.

## Query Params

أرسل القيم التالية كـ query string:

| Name | Type | Required | Notes |
|---|---|---|---|
| `EmployeeId` | `Guid` | Yes | معرف الموظف |
| `EmployeeNameEn` | `string` | Yes | اسم الموظف باللغة الإنكليزية للبطاقة الإنكليزية |
| `StartDate` | `date` | Yes | تاريخ بداية البطاقة بصيغة `yyyy-MM-dd` |
| `EndDate` | `date` | Yes | تاريخ نهاية البطاقة بصيغة `yyyy-MM-dd` |

مثال:

```http
GET api/Employee/GetEmployeeCardAsPdf?EmployeeId=00000000-0000-0000-0000-000000000000&EmployeeNameEn=Mohammad Ahmad&StartDate=2024-05-01&EndDate=2026-05-01
```

## Success Response

عند النجاح، الاستجابة ليست JSON.

الـ API يرجع ملف PDF:

```http
Content-Type: application/pdf
```

اسم الملف يرجع من السيرفر بالشكل التالي تقريبا:

```text
EmployeeCard-{EmployeeFullName}-{yyyyMMddHHmmss}.pdf
```

## Frontend Handling

يجب التعامل مع الاستجابة كـ `blob`.

مثال باستخدام `fetch`:

```js
const params = new URLSearchParams({
  EmployeeId: employeeId,
  EmployeeNameEn: employeeNameEn,
  StartDate: startDate, // yyyy-MM-dd
  EndDate: endDate,     // yyyy-MM-dd
});

const response = await fetch(`/api/Employee/GetEmployeeCardAsPdf?${params}`);

if (!response.ok) {
  const error = await response.json();
  throw error;
}

const blob = await response.blob();
const url = window.URL.createObjectURL(blob);

const link = document.createElement("a");
link.href = url;
link.download = "EmployeeCard.pdf";
link.click();

window.URL.revokeObjectURL(url);
```

## Error Responses

عند الخطأ، الاستجابة تكون JSON من نوع `OperationResponse`.

الحالات المهمة للفرونت:

| Status | سبب الخطأ | الرسالة |
|---|---|---|
| `400` | لم يتم إرسال تاريخ البداية أو النهاية | `تاريخ البداية وتاريخ النهاية مطلوبان.` |
| `400` | لم يتم إرسال الاسم الإنكليزي | `اسم الموظف باللغة الإنكليزية مطلوب.` |
| `400` | تاريخ النهاية قبل تاريخ البداية | `تاريخ النهاية يجب أن يكون بعد تاريخ البداية.` |
| `404` | الموظف غير موجود | `الموظف غير موجود.` |

## Notes

- `EmployeeNameEn` لا يتم جلبه من قاعدة البيانات، يجب إرساله من الفرونت.
- الاسم العربي، الرقم التسلسلي، والمنصب يتم جلبهم من السيرفر.
- المنصب الإنكليزي يتم جلبه من السيرفر إذا كان مخزنا في قاعدة البيانات.
- يجب إرسال التاريخ بصيغة `yyyy-MM-dd`.
