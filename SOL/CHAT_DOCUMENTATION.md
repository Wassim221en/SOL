# وثائق الدردشة لفريق الفرونت إند

## نظرة عامة

يوفر هذا المشروع نظام دردشة متكامل باستخدام SignalR للاتصال الفوري وREST API للعمليات الثابتة. تم تصميم النظام لدعم الدردشة الثنائية بين المستخدمين مع ميزات متقدمة مثل إرسال الصور، إظهار حالة القراءة، ومؤشر الكتابة.

## الهيكل العام

### 1. SignalR Hub (الاتصال الفوري)
- **المسار**: `SignalR/Hubs/ChatHub/ChatHub.cs`
- **الواجهة**: `SignalR/Hubs/ChatHub/IChatHub.cs`
- **نقطة النهاية**: `/chat-hub`

### 2. REST API (العمليات الثابتة)
- **المسار**: `SOL.API/Controllers/ChatController.cs`
- **نقطة النهاية**: `/api/chat`

### 3. نماذج البيانات
- **الدردشة**: `SOL.Domain/Entities/Chates/Chat.cs`
- **الرسالة**: `SOL.Domain/Entities/Chates/Message.cs`

## SignalR Hub Methods

### الاتصال والانفصال

#### `OnConnectedAsync()`
- **الوصف**: يتم استدعاؤه تلقائيًا عند اتصال المستخدم بالـ Hub
- **الوظيفة**: 
  - يسجل اتصال المستخدم
  - يرسل إشعار "المستخدم متصل" لجميع المستخدمين
- **الاستخدام**: لا يحتاج إلى استدعاء من الفرونت

#### `OnDisconnectedAsync()`
- **الوصف**: يتم استدعاؤه تلقائيًا عند انفصال المستخدم عن الـ Hub
- **الوظيفة**:
  - يزيل المستخدم من قائمة المتصلين
  - يرسل إشعار "المستخدم غير متصل" لجميع المستخدمين
- **الاستخدام**: لا يحتاج إلى استدعاء من الفرونت

### إرسال واستقبال الرسائل

#### `SendMessage(chatId, receiverUserId, message)`
- **الوصف**: إرسال رسالة إلى مستخدم معين
- **المعلمات**:
  - `chatId`: GUID معرف الدردشة
  - `receiverUserId`: GUID معرف المستخدم المستقبل
  - `message`: نص الرسالة
- **الاستخدام**:
```javascript
connection.invoke("SendMessage", chatId, receiverUserId, messageText);
```

#### `MessageReceived` (Event)
- **الوصف**: حدث يتم إرساله عند استلام رسالة جديدة
- **البيانات المستلمة**:
```javascript
{
  ChatId: "string",
  SenderId: "string", 
  ReceiverId: "string",
  Text: "string",
  Timestamp: "DateTime"
}
```
- **الاستخدام**:
```javascript
connection.on("MessageReceived", function(message) {
  // معالجة الرسالة الجديدة
  displayMessage(message);
});
```

### إدارة قراءة الرسائل

#### `ReadMessage(chatId, messageId)`
- **الوصف**: تمييز رسالة معينة كمقروءة
- **المعلمات**:
  - `chatId`: GUID معرف الدردشة
  - `messageId`: GUID معرف الرسالة
- **الاستخدام**:
```javascript
connection.invoke("ReadMessage", chatId, messageId);
```

#### `ReadAllMessages(chatId, senderUserId)`
- **الوصف**: تمييز جميع الرسائل من مستخدم معين كمقروءة
- **المعلمات**:
  - `chatId`: GUID معرف الدردشة
  - `senderUserId`: GUID معرف المستخدم المرسل
- **الاستخدام**:
```javascript
connection.invoke("ReadAllMessages", chatId, senderUserId);
```

#### `ReadMessage` و `ReadMessages` (Events)
- **الوصف**: أحداث يتم إرسالها عند قراءة الرسائل
- **الاستخدام**:
```javascript
connection.on("ReadMessage", function(messageId) {
  // تحديث واجهة المستخدم لإظهار أن الرسالة مقروءة
  markMessageAsRead(messageId);
});

connection.on("ReadMessages", function(chatId) {
  // تحديث جميع الرسائل في الدردشة كمقروءة
  markAllMessagesAsRead(chatId);
});
```

### مؤشر الكتابة

#### `StartTyping(chatId, receiverUserId)`
- **الوصف**: إرسال إشعار ببدء الكتابة
- **المعلمات**:
  - `chatId`: GUID معرف الدردشة
  - `receiverUserId`: GUID معرف المستخدم المستقبل
- **الاستخدام**:
```javascript
connection.invoke("StartTyping", chatId, receiverUserId);
```

#### `EndTyping(chatId, receiverUserId)`
- **الوصف**: إرسال إشعار بإنهاء الكتابة
- **المعلمات**:
  - `chatId`: GUID معرف الدردشة
  - `receiverUserId`: GUID معرف المستخدم المستقبل
- **الاستخدام**:
```javascript
connection.invoke("EndTyping", chatId, receiverUserId);
```

#### `UserTyping` (Event)
- **الوصف**: حدث يتم إرساله عند بدء أو إنهاء الكتابة
- **البيانات المستلمة**:
```javascript
{
  chatId: "string",
  receiverUserId: "string", 
  isTyping: boolean
}
```
- **الاستخدام**:
```javascript
connection.on("UserTyping", function(data) {
  if (data.isTyping) {
    showTypingIndicator(data.chatId);
  } else {
    hideTypingIndicator(data.chatId);
  }
});
```

### إدارة الدردشات

#### `CreateChat(receiverUserId)`
- **الوصف**: إنشاء دردشة جديدة مع مستخدم
- **المعلمات**:
  - `receiverUserId`: GUID معرف المستخدم الذي تريد الدردشة معه
- **الاستخدام**:
```javascript
connection.invoke("CreateChat", receiverUserId);
```

#### `UserStatusChanged` (Event)
- **الوصف**: حدث يتم إرساله عند تغير حالة اتصال المستخدم
- **البيانات المستلمة**:
```javascript
{
  userId: "string",
  isOnline: boolean
}
```
- **الاستخدام**:
```javascript
connection.on("UserStatusChanged", function(data) {
  updateUserStatus(data.userId, data.isOnline);
});
```

## REST API Endpoints

### الحصول على جميع الدردشات

#### `GET /api/chat/GetAllChats`
- **الوصف**: الحصول على جميع دردشات المستخدم مع إمكانية التصفح
- **المعلمات**:
  - `UserId`: GUID معرف المستخدم
  - `PageSize`: حجم الصفحة (اختياري)
  - `PageIndex`: رقم الصفحة (اختياري)
- **الاستجابة**:
```json
{
  "success": true,
  "data": {
    "Count": 5,
    "Chates": [
      {
        "ChatId": "guid",
        "UserId": "guid", 
        "UserFullName": "string",
        "DateUpdated": "datetime",
        "Messages": [
          {
            "MessageId": "guid",
            "Content": "string",
            "SenderId": "guid",
            "SenderName": "string", 
            "IsRead": boolean,
            "DateCreated": "string",
            "FromMe": boolean
          }
        ]
      }
    ]
  }
}
```

### الحصول على جميع الرسائل

#### `GET /api/chat/GetAllMessages`
- **الوصف**: الحصول على جميع رسائل الدردشة مع إمكانية التصفح
- **المعلمات**:
  - `ChatId`: GUID معرف الدردشة
  - `PageSize`: حجم الصفحة (اختياري)
  - `PageIndex`: رقم الصفحة (اختياري)
- **الاستجابة**:
```json
{
  "success": true,
  "data": {
    "Count": 25,
    "Messages": [
      {
        "MessageId": "guid",
        "Content": "string",
        "ImageUrl": "string",
        "SenderId": "guid",
        "SenderName": "string",
        "IsRead": boolean, 
        "DateCreated": "datetime"
      }
    ]
  }
}
```

### رفع صورة

#### `POST /api/chat/UploadImage`
- **الوصف**: رفع صورة وإرسالها كرسالة
- **المعلمات** (FormData):
  - `ChatId`: GUID معرف الدردشة
  - `Content`: نص الرسالة
  - `Image`: ملف الصورة (JPG, JPEG, PNG, GIF - max 5MB)
- **الاستجابة**:
```json
{
  "success": true,
  "data": null
}
```

## نماذج البيانات

### Chat (الدردشة)
```csharp
public class Chat
{
    public Guid Id { get; private set; }
    public Guid UserAId { get; private set; }
    public User UserA { get; private set; }
    public Guid UserBId { get; private set; }
    public User UserB { get; private set; }
    public IReadOnlyCollection<Message> Messages { get; private set; }
}
```

### Message (الرسالة)
```csharp
public class Message
{
    public Guid Id { get; private set; }
    public Guid ChatId { get; private set; }
    public Chat Chat { get; private set; }
    public Guid SenderId { get; private set; }
    public User Sender { get; private set; }
    public Guid ReceiverId { get; private set; }
    public User Receiver { get; private set; }
    public string Content { get; private set; }
    public string? ImageUrl { get; private set; }
    public bool IsRead { get; private set; }
    public DateTimeOffset DateCreated { get; private set; }
}
```

## مثال على استخدام SignalR في JavaScript

```javascript
// إعداد الاتصال
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/chat-hub", {
        accessTokenFactory: () => getAccessToken()
    })
    .build();

// بدء الاتصال
connection.start().then(function () {
    console.log("Connected to chat hub");
}).catch(function (err) {
    console.error(err.toString());
});

// الاستماع للأحداث
connection.on("MessageReceived", function(message) {
    displayMessage(message);
});

connection.on("UserTyping", function(data) {
    if (data.isTyping) {
        showTypingIndicator(data.chatId);
    } else {
        hideTypingIndicator(data.chatId);
    }
});

connection.on("UserStatusChanged", function(data) {
    updateUserStatus(data.userId, data.isOnline);
});

connection.on("ReadMessage", function(messageId) {
    markMessageAsRead(messageId);
});

connection.on("ReadMessages", function(chatId) {
    markAllMessagesAsRead(chatId);
});

// إرسال الرسائل
function sendMessage(chatId, receiverUserId, message) {
    connection.invoke("SendMessage", chatId, receiverUserId, message)
        .catch(function(err) {
            console.error(err.toString());
        });
}

// مؤشر الكتابة
let typingTimer;
function startTyping(chatId, receiverUserId) {
    connection.invoke("StartTyping", chatId, receiverUserId);
    clearTimeout(typingTimer);
    typingTimer = setTimeout(() => {
        endTyping(chatId, receiverUserId);
    }, 3000);
}

function endTyping(chatId, receiverUserId) {
    connection.invoke("EndTyping", chatId, receiverUserId);
}

// قراءة الرسائل
function readMessage(chatId, messageId) {
    connection.invoke("ReadMessage", chatId, messageId);
}

function readAllMessages(chatId, senderUserId) {
    connection.invoke("ReadAllMessages", chatId, senderUserId);
}
```

## ملاحظات هامة

1. **المصادقة**: يجب أن يكون المستخدم مصادقًا لاستخدام SignalR Hub
2. **معرفات GUID**: جميع المعرفات يجب أن تكون بتنسيق GUID
3. **الاتصال**: يجب الحفاظ على اتصال SignalR نشطًا للحصول على الرسائل الفورية
4. **الصور**: حجم الصور محدود بـ 5MB وتنسيقات محددة فقط
5. **التوقيت**: يتم تحويل التوقيت تلقائيًا إلى التوقيت المحلي (Asia/Damascus)

## استكشاف الأخطاء وإصلاحها

### مشاكل الاتصال
- تأكد من أن المستخدم مصادق
- تحقق من صحة رمز الوصول (Access Token)
- تأكد من أن SignalR Hub يعمل بشكل صحيح

### مشاكل استلام الرسائل
- تحقق من أن المستخدم متصل بالـ Hub
- تأكد من صحة معرفات الدردشة والمستخدمين
- تحقق من أن الأحداث مسجلة بشكل صحيح

### مشاكل رفع الصور
- تحقق من تنسيق الصورة (JPG, JPEG, PNG, GIF)
- تأكد من أن حجم الصورة أقل من 5MB
- تحقق من صحة معرف الدردشة