# Authentication API - Login & Refresh Token

## Login Endpoint

### Endpoint Details

| Method | URL | Auth Required |
| --- | --- | --- |
| `POST` | `/api/AppUser/Login` | No |

### Request Body Parameters

| Name | Type | Required | Description |
| --- | --- | --- | --- |
| `Email` | `string` | **One of Email/PhoneNumber/UserName required** | البريد الإلكتروني للمستخدم |
| `PhoneNumber` | `string` | **One of Email/PhoneNumber/UserName required** | رقم الهاتف للمستخدم |
| `UserName` | `string` | **One of Email/PhoneNumber/UserName required** | اسم المستخدم |
| `Password` | `string` | Yes | كلمة المرور |
| `DeviceToken` | `string` | No | توكن الجهاز للإشعارات (اختياري) |
| `ConfirmPassword` | `string` | No | تأكيد كلمة المرور (يبدوا أنه لا يُستخدم في الـ login) |

### Validation Rules

- يجب توفير **واحد فقط** من المعرفات (Email أو PhoneNumber أو UserName)
- لا يمكن تركيبة أكثر من معرف في طلب واحد

### Example Request

```json
{
  "email": "admin@template.com",
  "password": "Admin@123",
  "deviceToken": "device-push-token-here"
}
```

### Response Shape

```json
{
  "success": true,
  "data": {
    "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "firstName": "John",
    "lastName": "Doe",
    "userName": "johndoe",
    "email": "admin@template.com",
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "ABC123XYZ789...",
    "imageUrl": "https://example.com/images/original.jpg",
    "thumbnailUrl": "https://example.com/images/thumbnail.jpg",
    "roles": [
      {
        "roleId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "roleName": "Admin",
        "permissionsPages": [
          {
            "page": "Employees",
            "permissions": ["View", "Create", "Update", "Delete"]
          },
          {
            "page": "HR",
            "permissions": ["View", "Create"]
          }
        ]
      }
    ]
  }
}
```

### Response Fields Description

| Field | Type | Description |
| --- | --- | --- |
| `userId` | `guid` | رقم المستخدم الفريد |
| `firstName` | `string` | الاسم الأول |
| `lastName` | `string` | اسم العائلة |
| `userName` | `string` | اسم المستخدم |
| `email` | `string` | البريد الإلكتروني |
| `token` | `string` | JWT Token للاستخدام في الطلبات المحمية (تنتهي صلاحيته بعد 7 أيام) |
| `refreshToken` | `string` | توكن التحديث (refresh token) لتمديد صلاحية الـ token |
| `imageUrl` | `string?` | رابط الصورة الأصلية للمستخدم |
| `thumbnailUrl` | `string?` | رابط الصورة المصغرة للمستخدم |
| `roles` | `array` | قائمة الأدوار والصلاحيات |
| `roles[].roleId` | `guid` | رقم الدور |
| `roles[].roleName` | `string` | اسم الدور |
| `roles[].permissionsPages` | `array` | قائمة الصفحات والصلاحيات |
| `roles[].permissionsPages[].page` | `string` | اسم الصفحة |
| `roles[].permissionsPages[].permissions` | `array<string>` | قائمة الصلاحيات للصفحة |

---

## Refresh Token Endpoint

### Endpoint Details

| Method | URL | Auth Required |
| --- | --- | --- |
| `POST` | `/api/AppUser/RefreshToken` | No |

### Request Body Parameters

| Name | Type | Required | Description |
| --- | --- | --- | --- |
| `RefreshToken` | `string` | Yes | توكن التحديث (refresh token) الحالي |

### Example Request

```json
{
  "refreshToken": "ABC123XYZ789..."
}
```

### Response Shape

```json
{
  "success": true,
  "data": {
    "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "firstName": "John",
    "lastName": "Doe",
    "userName": "johndoe",
    "email": "admin@template.com",
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "NEW_REFRESH_TOKEN_HERE...",
    "roles": [
      {
        "roleId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "roleName": "Admin",
        "permissionsPages": [
          {
            "page": "Employees",
            "permissions": ["View", "Create", "Update", "Delete"]
          }
        ]
      }
    ]
  }
}
```

### Notes for Frontend

- **Refresh Token Validity**: صلاحية التوكن الجديد هو 7 أيام، والـ refresh token جديد صالح لمدة 15 يوم
- **Token Storage**: يُفترض حفظ الـ `token` في الـ `localStorage` أو `sessionStorage` واستخدامه في كل طلب بـ Header `Authorization: Bearer {token}`
- **When to Use Refresh**: عندما يرفض الـ API طلب بسبب انتهاء صلاحية الـ token (401 Unauthorized)، استخدم الـ refresh token للحصول على توكن جديد
- **Invalid Refresh Token**: إذا تم إبطال أو انتهاء صلاحية الـ refresh token، سيُرجع الخطأ `Invalid RefreshToken` - في هذا الحالة يجب توجيه المستخدم لتسجيل الدخول مرة أخرى
- **New Refresh Token**: كل مرة تُعاد فيها التوكن، يتم إنشاء refresh token جديد - احفظه بدلاً القديم

### Error Responses

#### Invalid Credentials (Login)

```json
{
  "success": false,
  "errorMessage": "Invalid password or email"
}
```

#### Multiple Identifiers Provided

```json
{
  "success": false,
  "errorMessage": "Only one login identifier is allowed"
}
```

#### No Identifier Provided

```json
{
  "success": false,
  "errorMessage": "Login identifier is required"
}
```

#### Invalid Refresh Token

```json
{
  "success": false,
  "errorMessage": "Invalid RefreshToken"
}
```