# Mini 3 — Phase 1 API endpoints: Workspace RBAC

## Quy ước chung cho frontend

Mini 3 không làm login/JWT. Mỗi request gửi header demo:

```http
X-Demo-Person-Id: {person-guid}
```

Backend dùng person này để tìm membership trong workspace đang được truy cập. Frontend không gửi `role` hiện tại của mình; backend luôn tự kiểm tra từ `workspace_members`.

Response thành công dùng một shape chung (`data`, `traceId`); lỗi trả HTTP status, `messageCode`, `detail`, `traceId`. FE nên dựa vào `messageCode`, không dựa vào câu chữ `detail`.

## Ma trận quyền Phase 1

| Khả năng | Viewer | Editor | Manager | Owner |
|---|:---:|:---:|:---:|:---:|
| Xem workspace/member | ✓ | ✓ | ✓ | ✓ |
| Cập nhật workspace | - | ✓ | ✓ | ✓ |
| Mời, xem, xóa member thường | - | - | ✓ | ✓ |
| Đổi role không phải Owner | - | - | ✓ | ✓ |
| Chuyển Owner | - | - | - | ✓ |
| Xóa workspace | - | - | - | ✓ |

`Owner` là role đặc biệt: không được cấp qua endpoint đổi role thông thường; chỉ chuyển bằng endpoint transfer ownership để backend luôn bảo vệ invariant “workspace phải còn Owner”.

## 1. Context người dùng demo

### `GET /api/people/me`

**Ai gọi:** mọi person active có header demo hợp lệ.

**FE dùng để:** lấy identity đang được chọn cho demo, hiển thị tên/email và kiểm tra header có hợp lệ trước khi gọi các màn workspace.

**Kết quả:** `200` trả `id`, `email`, `displayName`, `isActive`.

> Không cần CRUD `people` trong Phase 1. Person được seed/migration tạo sẵn để project tập trung vào RBAC.

## 2. Workspace

### `POST /api/workspaces`

**Ai gọi:** person active.

**FE dùng để:** form tạo workspace.

**Body:**

```json
{
  "code": "acme-platform",
  "name": "Acme Platform",
  "description": "Nhóm phát triển sản phẩm"
}
```

**Kết quả:** `201 Created` trả workspace. Backend tạo đồng thời `Workspace` và `WorkspaceMember` role `Owner` trong một transaction.

**Cần học:** transaction giữ invariant: không tồn tại workspace không có Owner.

### `GET /api/workspaces/mine`

**Ai gọi:** person active.

**FE dùng để:** trang chọn workspace sau khi vào ứng dụng.

**Kết quả:** `200` danh sách workspace mà person là member, mỗi item có `myRole`, số member và thông tin tóm tắt.

**Cần học:** query bắt đầu từ `workspace_members`, không trả mọi workspace trong database.

### `GET /api/workspaces/{workspaceId}`

**Ai gọi:** Viewer trở lên trong workspace.

**FE dùng để:** màn overview/settings workspace.

**Kết quả:** `200` workspace detail, `myRole` và các capability FE được phép hiển thị.

### `PUT /api/workspaces/{workspaceId}`

**Ai gọi:** Editor trở lên.

**FE dùng để:** sửa tên/description workspace.

**Body:**

```json
{
  "name": "Acme Platform",
  "description": "Mô tả mới"
}
```

**Kết quả:** `200` workspace mới nhất.

### `DELETE /api/workspaces/{workspaceId}`

**Ai gọi:** chỉ Owner.

**FE dùng để:** hành động nguy hiểm trong phần settings; FE cần dialog xác nhận.

**Kết quả:** `204 No Content`; đây là soft delete. Member/invitation không tự bị xóa vật lý.

## 3. Thành viên và role

### `GET /api/workspaces/{workspaceId}/members`

**Ai gọi:** Viewer trở lên.

**FE dùng để:** màn danh sách thành viên, hiển thị tên/email/role/joinedAt và bật/tắt nút hành động theo capability backend trả về.

**Kết quả:** `200` danh sách member.

### `PUT /api/workspaces/{workspaceId}/members/{personId}/role`

**Ai gọi:** Manager hoặc Owner.

**FE dùng để:** dropdown đổi `Viewer` / `Editor` / `Manager` cho thành viên đang có.

**Body:**

```json
{
  "role": "Editor"
}
```

**Kết quả:** `200` member đã cập nhật.

**Rule:** endpoint này không nhận `Owner`. Manager không được nâng ai thành Owner hay tự thay đổi quyền của mình theo cách vượt quyền.

**Cần học:** backend phải kiểm tra role của caller **trong `workspaceId` này**, không kiểm tra một role toàn cục.

### `DELETE /api/workspaces/{workspaceId}/members/{personId}`

**Ai gọi:** Manager hoặc Owner.

**FE dùng để:** nút remove member.

**Kết quả:** `204 No Content` (soft delete membership).

**Rule:** không được remove Owner cuối cùng. Manager không được xóa Owner.

### `POST /api/workspaces/{workspaceId}/ownership-transfer`

**Ai gọi:** chỉ Owner hiện tại.

**FE dùng để:** flow riêng “Transfer ownership”, phải yêu cầu xác nhận vì caller sẽ không còn là Owner sau đó.

**Body:**

```json
{
  "targetPersonId": "person-guid"
}
```

**Kết quả:** `200` trả hai membership đã đổi: target thành `Owner`, Owner cũ thành `Manager`.

**Cần học:** đây là transaction gồm hai update; không dùng endpoint đổi role chung để tránh một thời điểm workspace có 0 hoặc 2 Owner ngoài ý muốn.

## 4. Invitation

### `POST /api/workspaces/{workspaceId}/invitations`

**Ai gọi:** Manager hoặc Owner.

**FE dùng để:** form mời email vào workspace.

**Body:**

```json
{
  "email": "new.member@example.com",
  "role": "Viewer"
}
```

**Kết quả:** `201 Created` trả invitation metadata, không trả raw token trong production response. Ở Development có thể log token/link để Swagger test được.

**Rule:** không mời `Owner`; chỉ có một invitation đang chờ cho một email trong một workspace.

### `GET /api/workspaces/{workspaceId}/invitations`

**Ai gọi:** Manager hoặc Owner.

**FE dùng để:** tab Pending invitations; hiển thị email, role, expiresAt, người gửi và trạng thái.

**Kết quả:** `200` danh sách invitation chưa hết hạn hoặc toàn bộ lịch sử tùy filter `?status=pending|accepted|expired`.

### `POST /api/invitations/accept`

**Ai gọi:** person active có header demo. Person phải có email trùng email được mời.

**FE dùng để:** trang nhận invitation từ link/email.

**Body:**

```json
{
  "token": "raw-invitation-token"
}
```

**Kết quả:** `200` trả workspace/member mới.

**Rule:** hash token để tìm invitation; kiểm tra chưa accept, chưa hết hạn, workspace còn active và email caller khớp. Sau đó tạo membership + đặt `acceptedAt` trong một transaction.

**Cần học:** raw token không lưu DB và token không được accept hai lần, kể cả khi hai request đến đồng thời.

## Các lỗi FE cần xử lý

| HTTP | `messageCode` gợi ý | Khi nào |
|---:|---|---|
| 400 | `VALIDATION_FAILED` | Body, GUID, code hoặc role không hợp lệ |
| 401 | `DEMO_PERSON_REQUIRED` | Thiếu/sai `X-Demo-Person-Id` |
| 403 | `WORKSPACE_PERMISSION_DENIED` | Có membership nhưng role không đủ |
| 404 | `WORKSPACE_NOT_FOUND`, `MEMBER_NOT_FOUND` | Không tồn tại hoặc không được phép thấy resource |
| 409 | `WORKSPACE_CODE_TAKEN` | Code workspace trùng |
| 409 | `ALREADY_MEMBER`, `INVITATION_ALREADY_PENDING` | Vi phạm unique/invariant |
| 409 | `LAST_OWNER`, `OWNER_ROLE_REQUIRES_TRANSFER` | Vi phạm rule Owner |
| 400 | `INVITATION_INVALID_OR_EXPIRED` | Token sai, hết hạn hoặc đã accept |

## Ngoài phạm vi Phase 1

- Password, JWT, refresh token, email provider thật.
- Role do Owner tự tạo.
- Permission catalogue, role-permission mapping và member permission override.
- Audit log chi tiết, pagination, resend/cancel invitation.

Các phần này thuộc Giai đoạn 2 hoặc stretch goals sau khi RBAC role cố định và test role matrix đã hoàn chỉnh.
