# تقرير روتات الموظفين والصلاحيات للفرونت اند

هذا التقرير مبني على الكود الحالي في:

- `SOL.API/SOL.API/Controllers/AppUserController.cs`
- `SOL.API/SOL.API/Controllers/RoleController.cs`
- `SOL.API/SOL.API/Authorization/Permissions.cs`
- أوامر واستعلامات `SOL.Application/Features/Dashboard/Employee` و `Role`

## ملاحظات عامة

الـ base route للموظفين هو:

```http
/api/AppUser/{Action}
```

الـ base route للأدوار هو:

```http
/api/Role/{Action}
```

معظم الردود ترجع داخل envelope اسمه `OperationResponse`:

```json
{
  "statusCode": null,
  "success": true,
  "errorMessage": null,
  "data": {}
}
```

في حال الخطأ:

```json
{
  "statusCode": "BadRequest",
  "success": false,
  "errorMessage": {
    "message": "نص الخطأ",
    "statusCode": "BadRequest"
  }
}
```

المشروع يستخدم `JsonStringEnumConverter`، لذلك الـ enums بالـ JSON يمكن إرسالها واستقبالها كنصوص:

- `Gender`: `Male`, `Female`
- `ActiveStatus`: `Active`, `Inactive`
- `RoleStatus`: `Active`, `Inactive`
- `SortType`: `Ascending`, `Descending`

الروتات التي تستقبل صورة تستخدم `multipart/form-data` وليس JSON.

## الصلاحيات المعرفة في النظام

الصلاحيات المعرفة حالياً:

```text
Employees.View
Employees.Create
Employees.Update
Employees.Delete

HR.View
HR.Create
HR.Update
HR.Delete

Roles.View
Roles.Create
Roles.Update
Roles.Delete
```

آلية التحقق:

- إذا كان الـ JWT يحتوي claim باسم `IsOwner` وقيمته `true`، المستخدم يتجاوز فحص الصلاحيات.
- غير ذلك، النظام يقرأ أدوار المستخدم من claims ثم يجلب صلاحيات هذه الأدوار من `RoleClaims`.
- `HasPermissions` يدعم:
  - `Or`: يكفي امتلاك صلاحية واحدة.
  - `And`: يجب امتلاك كل الصلاحيات.

ملاحظة مهمة: واجهات الأدوار الحالية لا تستقبل ولا ترجع permissions داخل الدور. الصلاحيات موجودة كـ role claims وتظهر في نتيجة `Login` و `RefreshToken`، لكن لا يوجد حالياً endpoint واضح لإضافة/تعديل permissions للدور من الكنترولرز الموجودة.

## روتات المصادقة والحساب

### POST `/api/AppUser/Login`

تسجيل دخول. لا يحتاج authorization.

Body JSON:

```json
{
  "email": "admin@template.com",
  "phoneNumber": null,
  "userName": null,
  "password": "Admin@123",
  "deviceToken": "optional-device-token"
}
```

يجب إرسال واحد فقط من: `email`, `phoneNumber`, `userName`.

يرجع:

```json
{
  "success": true,
  "data": {
    "userId": "guid",
    "firstName": "string",
    "lastName": "string",
    "userName": "string",
    "email": "string",
    "token": "jwt-token",
    "refreshToken": "refresh-token",
    "imageUrl": "string|null",
    "thumbnailUrl": "string|null",
    "roles": [
      {
        "roleId": "guid",
        "roleName": "ADMIN",
        "permissionsPages": [
          {
            "page": "Employees",
            "permissions": ["View", "Create", "Update"]
          }
        ]
      }
    ]
  }
}
```

### POST `/api/AppUser/RefreshToken`

تجديد access token.

Body JSON:

```json
{
  "refreshToken": "refresh-token"
}
```

يرجع نفس شكل `Login`.

### POST `/api/AppUser/ForgetPassword`

طلب رمز إعادة تعيين كلمة المرور. لا يحتاج authorization.

Body JSON:

```json
{
  "email": "user@example.com",
  "phoneNumber": null
}
```

يرجع `OperationResponse` بدون `data`.

### POST `/api/AppUser/ResetTokenVerification`

التحقق من رمز إعادة التعيين.

Body JSON:

```json
{
  "email": "user@example.com",
  "phoneNumber": null,
  "resetToken": "123456"
}
```

يرجع `OperationResponse` بدون `data`.

### POST `/api/AppUser/ResetPassword`

إعادة تعيين كلمة المرور ثم تسجيل الدخول تلقائياً.

Body JSON:

```json
{
  "email": "user@example.com",
  "phoneNumber": null,
  "resetToken": "123456",
  "newPassword": "NewPassword@123",
  "confirmPassword": "NewPassword@123"
}
```

يرجع نفس شكل `Login`.

### POST `/api/AppUser/EmailConfirmed`

تأكيد البريد. في الكود الحالي يستقبل القيم من query رغم أنه POST.

Query:

```http
?email=user@example.com&code=123456
```

يرجع `OperationResponse` بدون `data`.

### POST `/api/AppUser/ModifyMyPassword`

تعديل كلمة مرور المستخدم الحالي. يحتاج JWT عملياً لأن الهاندلر يعتمد على المستخدم الحالي.

Body JSON:

```json
{
  "oldPassword": "OldPassword@123",
  "newPassword": "NewPassword@123",
  "confirmNewPassword": "NewPassword@123"
}
```

يرجع `OperationResponse` بدون `data`.

### GET `/api/AppUser/GetMyProfile`

جلب بروفايل المستخدم الحالي. يحتاج JWT عملياً.

Query: لا يوجد بارامترات.

يرجع:

```json
{
  "success": true,
  "data": {
    "employeeId": "guid",
    "number": 1,
    "firstName": "string",
    "lastName": "string",
    "userName": "string",
    "email": "string",
    "phoneNumber": "string",
    "imageUrl": "string|null",
    "thumbnailUrl": "string|null"
  }
}
```

### POST `/api/AppUser/ModifyMyProfile`

تعديل بروفايل المستخدم الحالي. يحتاج `multipart/form-data`.

Form data:

```text
userName: string
email: string|null
image: File|null
deleteImage: true|false
```

يرجع:

```json
{
  "success": true,
  "data": {
    "id": "guid",
    "userName": "string",
    "email": "string|null",
    "imageUrl": "string|null"
  }
}
```

## روتات الموظفين

### GET `/api/AppUser/GetAll`

جلب قائمة الموظفين.

الصلاحية المطلوبة:

```text
Employees.View OR HR.View
```

Query:

```http
?search=ali&status=Active&pageSize=10&pageIndex=1&column=firstName&sortType=Ascending
```

البارامترات:

```text
search: string|null
status: Active|Inactive|null
pageSize: number, default 10
pageIndex: number, default 1
column: string|null
sortType: Ascending|Descending
```

يرجع:

```json
{
  "success": true,
  "data": {
    "count": 25,
    "employees": [
      {
        "employeeId": "guid",
        "number": 1,
        "firstName": "string",
        "lastName": "string",
        "fullName": "string",
        "userName": "string",
        "email": "string",
        "phoneNumber": "string",
        "imageUrl": "string|null",
        "thumbnailUrl": "string|null",
        "status": "Active"
      }
    ]
  }
}
```

ملاحظة: `count` هو عدد النتائج بعد الفلترة وقبل pagination.

### GET `/api/AppUser/GetAllNames`

جلب أسماء الموظفين للاستخدام في dropdowns. لا يوجد `HasPermissions` على الأكشن حالياً.

Query: لا يوجد بارامترات.

يرجع:

```json
{
  "success": true,
  "data": [
    {
      "employeeId": "guid",
      "number": 1,
      "name": "First Last",
      "phoneNumber": "string|null",
      "imageUrl": "string|null",
      "thumbnailUrl": "string|null"
    }
  ]
}
```

### GET `/api/AppUser/GetById`

جلب موظف بالتفصيل.

الصلاحية المطلوبة:

```text
Employees.View
```

Query:

```http
?id=employee-guid
```

يرجع:

```json
{
  "success": true,
  "data": {
    "employeeId": "guid",
    "number": 1,
    "firstName": "string",
    "lastName": "string",
    "fullName": "string",
    "email": "string|null",
    "userName": "string",
    "phoneNumber": "string",
    "emailConfirmed": true,
    "phoneNumberConfirmed": false,
    "status": "Active",
    "dateCreated": "2026-06-26T10:00:00+03:00",
    "roles": [
      {
        "roleId": "guid",
        "name": "ADMIN"
      }
    ],
    "gender": "Male",
    "dateOfBirth": "1995-01-01",
    "imageUrl": "string|null"
  }
}
```

### POST `/api/AppUser/Add`

إضافة موظف. يحتاج `multipart/form-data`.

الصلاحية المطلوبة:

```text
Employees.Create
```

Form data:

```text
firstName: string
lastName: string
email: string|null
userName: string
phoneNumber: string
password: string
roleIds: guid[]
gender: Male|Female
dateOfBirth: yyyy-MM-dd|null
activeStatus: Active|Inactive
image: File|null
```

يرجع:

```json
{
  "success": true,
  "data": {
    "employeeId": "guid",
    "number": 0,
    "firstName": "string",
    "lastName": "string",
    "fullName": "string",
    "userName": "string",
    "email": "string",
    "phoneNumber": "string",
    "imageUrl": null,
    "thumbnailUrl": null,
    "status": "Active"
  }
}
```

ملاحظات:

- الصورة المسموحة: `.jpg`, `.jpeg`, `.png`, `.gif`, `.webp`, `.bmp`
- الحد الأقصى للصورة: 5MB
- يتم تحويل `userName` و `email` إلى lowercase.
- يفشل الطلب إذا كان `userName` أو `email` أو `phoneNumber` مستخدماً مسبقاً.

### POST `/api/AppUser/Modify`

تعديل موظف. يحتاج `multipart/form-data`.

الصلاحية المطلوبة:

```text
Employees.Update OR Employees.Create
```

Form data:

```text
employeeId: guid
firstName: string
lastName: string
userName: string
email: string|null
phoneNumber: string
roleIds: guid[]
gender: Male|Female
dateOfBirth: yyyy-MM-dd|null
activeStatus: Active|Inactive
image: File|null
deleteImage: true|false
newPassword: string|null
```

يرجع نفس شكل `GetById`.

### DELETE `/api/AppUser/Delete`

حذف موظف أو عدة موظفين soft delete.

الصلاحية المطلوبة:

```text
Employees.Delete
```

Query:

```http
?id=employee-guid
```

أو:

```http
?ids=guid1&ids=guid2&ids=guid3
```

يرجع:

```json
{
  "statusCode": null,
  "success": true,
  "errorMessage": null
}
```

ملاحظة: المستخدم `SUPERADMIN` لا يتم حذفه من هذا الهاندلر.

### POST `/api/AppUser/ModifyPersonalImage`

تعديل صورة موظف محدد. يحتاج `multipart/form-data`.

الصلاحية المطلوبة:

```text
Employees.Update OR Employees.Create
```

Form data:

```text
employeeId: guid
image: File|null
deleteImage: true|false
```

يرجع:

```json
{
  "success": true,
  "data": {
    "imageUrl": "string|null",
    "thumbnailUrl": "string|null"
  }
}
```

ملاحظة: إذا `image` كانت null أو `deleteImage=true` يتم حذف الصورة القديمة.

### POST `/api/AppUser/GetEmployeesAsPdf`

تحميل PDF لقائمة موظفين.

الصلاحية المطلوبة:

```text
Employees.View
```

Body JSON:

```json
{
  "employeeIds": ["guid1", "guid2"]
}
```

يرجع ملف PDF مباشرة:

```http
Content-Type: application/pdf
Content-Disposition: attachment; filename="..."
```

في حال الخطأ يرجع `OperationResponse`.

### GET `/api/AppUser/GetEmployeeCardAsPdf`

تحميل بطاقة موظف PDF.

في الكود الحالي صلاحية `Employees.View` معلقة بتعليق وليست مفعلة.

Query:

```http
?employeeId=guid&employeeNameEn=John%20Doe&startDate=2026-01-01&endDate=2026-12-31
```

يرجع ملف PDF مباشرة:

```http
Content-Type: application/pdf
Content-Disposition: attachment; filename="..."
```

في حال الخطأ يرجع `OperationResponse`.

## روتات الأدوار

ملاحظة مهمة: كل روتات `RoleController` عليها `AllowAnonymous` حالياً، لذلك لا يوجد تطبيق فعلي لصلاحيات `Roles.*` على هذه الروتات في الكود الحالي.

### POST `/api/Role/Add`

إضافة دور.

Body JSON:

```json
{
  "name": "ADMIN",
  "description": "Administrator",
  "status": "Active"
}
```

يرجع:

```json
{
  "success": true,
  "data": {
    "roleId": "guid",
    "roleName": "ADMIN",
    "description": "Administrator",
    "status": "Active"
  }
}
```

### POST `/api/Role/Modify`

تعديل دور.

Body JSON:

```json
{
  "roleId": "guid",
  "roleName": "ADMIN",
  "description": "Administrator",
  "status": "Active"
}
```

يرجع:

```json
{
  "success": true,
  "data": {
    "roleId": "guid",
    "roleName": "ADMIN",
    "description": "Administrator",
    "status": "Active"
  }
}
```

### DELETE `/api/Role/Delete`

حذف دور.

Query:

```http
?id=role-guid
```

يرجع `OperationResponse` بدون `data`.

### GET `/api/Role/GetAll`

جلب كل الأدوار.

Query:

```http
?status=Active
```

ملاحظة: `status` موجود في الـ request، لكن الـ specification الحالية لا تطبق فلترة عليه.

يرجع:

```json
{
  "success": true,
  "data": {
    "count": 2,
    "roles": [
      {
        "roleId": "guid",
        "roleName": "ADMIN",
        "description": "Administrator",
        "status": "Active"
      }
    ]
  }
}
```

### GET `/api/Role/GetById`

جلب دور بالتفصيل.

Query:

```http
?id=role-guid
```

يرجع:

```json
{
  "success": true,
  "data": {
    "roleId": "guid",
    "roleName": "ADMIN",
    "description": "Administrator",
    "status": "Active"
  }
}
```

## روتات موجودة في Application لكن غير مكشوفة في Controller

هذه الأوامر/الاستعلامات موجودة في طبقة Application، لكن لا يوجد لها endpoint في `RoleController` الحالي:

- `GetAllRoleNamesQuery`
- `GetRolesAsPdfQuery`

## توصيات للفرونت اند

- خزّن `token` و `refreshToken` من `Login`.
- أرسل `Authorization: Bearer {token}` مع روتات الموظفين المحمية.
- اعتمد على `roles[].permissionsPages` القادمة من `Login` أو `RefreshToken` لبناء إظهار/إخفاء صفحات وأزرار الواجهة.
- عند رفع صورة استخدم `FormData`.
- عند تحميل PDF، تعامل مع الرد كـ `blob` وليس JSON.
- لا تعتمد حالياً على Role API لإدارة صلاحيات الدور؛ لا يوجد request field باسم `permissions` في إضافة/تعديل الدور.
