# Mini 3 — Giải thích database Workspace Access Control

## Phạm vi cố ý của mini project

Database chỉ phục vụ quản lý workspace và quyền truy cập trong workspace. `people` là identity demo; project này **không** có password, JWT, refresh token hay gọi Mini 2. API sẽ xác định người đang thao tác từ một header demo, ví dụ `X-Demo-Person-Id`.

Mọi bảng đều tương ứng một entity kế thừa `BaseEntity<Guid>` và implement `IAuditableEntity`, nên có chung:

- `id`: khoá chính GUID.
- `is_deleted`: soft delete.
- `created_at`: thời điểm tạo do `DbContext` gán tập trung.
- `updated_at`: thời điểm sửa gần nhất do `DbContext` gán tập trung.

## Giai đoạn 1 — RBAC role cố định

### `people`

Người dùng demo trong hệ thống.

- `email`: định danh duy nhất, lưu lowercase.
- `display_name`: tên để hiển thị trong danh sách thành viên.
- `is_active`: false thì không thể tham gia/thao tác workspace.

Không có password hash vì xác thực không phải bài học của Mini 3.

### `workspaces`

Tenant boundary của dự án. Mọi member, role, invitation và authorization đều phải thuộc cùng một workspace.

- `code`: mã URL-friendly, unique toàn hệ thống, ví dụ `acme-platform`.
- `name`, `description`: thông tin hiển thị.
- `created_by_person_id`: người tạo; đồng thời phải được tạo thành `Owner` member trong cùng transaction.

### `workspace_members`

Đây là bảng trung tâm của RBAC: quyền không thuộc về `people`, mà thuộc về cặp **person + workspace**.

- `workspace_id`, `person_id`: người nào là thành viên của workspace nào.
- `role`: enum giai đoạn 1: `Owner`, `Manager`, `Editor`, `Viewer`.
- unique `(workspace_id, person_id)`: một người chỉ có đúng một membership trong một workspace.

Role cố định gợi ý:

- `Viewer`: xem workspace và member.
- `Editor`: quyền Viewer + cập nhật nội dung workspace.
- `Manager`: quyền Editor + mời/xóa member và đổi các role không phải Owner.
- `Owner`: toàn quyền, gồm xóa workspace và chuyển ownership.

Invariant quan trọng: không được xóa hoặc hạ cấp Owner cuối cùng.

### `workspace_invitations`

Lời mời cho một email vào một workspace.

- `email`: email đích, chuẩn hóa lowercase.
- `role`: role sẽ cấp khi invitation được chấp nhận.
- `token_hash`: SHA-256 của token random gửi cho người nhận; database tuyệt đối không lưu raw token.
- `expires_at`: invitation hết hạn.
- `accepted_at`: khác `null` nghĩa là đã dùng; token chỉ dùng một lần.
- `created_by_person_id`: người đã gửi lời mời để audit/authorization.

Khi accept, service phải kiểm tra token, expiry và `accepted_at`, tạo `workspace_member`, đặt `accepted_at`, rồi lưu tất cả trong một transaction.

## Giai đoạn 2 — custom role và permission override

Giai đoạn này mở rộng RBAC, không thay thế ranh giới `workspace` hay `workspace_member`.

### `permissions`

Danh mục permission toàn hệ thống, được seed và không cho client tự tạo. Ví dụ:

- `workspace.view`
- `workspace.update`
- `member.view`
- `member.manage`
- `role.manage`
- `workspace.delete`
- `ownership.transfer`

`code` unique, ổn định và được dùng trong policy/service; `display_name`/`description` chỉ phục vụ UI.

### `workspace_roles`

Role thuộc một workspace cụ thể.

- `workspace_id`: cùng tên role ở hai workspace vẫn là hai role khác nhau.
- `name`: ví dụ `Owner`, `Manager`, `Intern`, `HR`.
- `is_system`: bảo vệ các role nền (`Owner`, `Manager`, `Editor`, `Viewer`) khỏi việc xóa/sửa tùy tiện.
- unique `(workspace_id, name)`: tránh hai role cùng tên trong một workspace.

Khi nâng cấp từ giai đoạn 1, mỗi workspace seed bốn system role tương ứng với bốn enum cũ.

### `workspace_role_permissions`

Bảng many-to-many giữa role và permission.

- `workspace_role_id`: role trong workspace.
- `permission_id`: permission được role đó grant.
- unique `(workspace_role_id, permission_id)`: không grant trùng.

Ở tầng này, service hỏi `HasPermission(member, "member.manage")`, thay vì rải check `role == Manager` khắp controller.

### `member_permission_overrides`

Ngoại lệ theo từng thành viên, chỉ thêm khi MVP đã hoàn chỉnh.

- `workspace_member_id`: ngoại lệ chỉ có hiệu lực trong membership này.
- `permission_id`: permission bị điều chỉnh.
- `effect`: `Allow` cấp thêm quyền, `Deny` thu hồi một quyền từ role.
- unique `(workspace_member_id, permission_id)`: chỉ một quyết định cuối cùng cho mỗi permission.

Thứ tự resolve đề xuất: membership phải tồn tại và active → `Deny` override thắng → `Allow` override → quyền từ role → deny mặc định.

## Chuyển từ giai đoạn 1 sang giai đoạn 2

Không tạo database khác. Migration theo thứ tự:

1. Thêm `permissions`, `workspace_roles`, `workspace_role_permissions`, `member_permission_overrides`.
2. Seed permission catalogue và bốn system roles cho từng workspace; seed mapping role–permission.
3. Thêm nullable `workspace_role_id` vào `workspace_members` và `workspace_invitations`; backfill từ enum `role`.
4. Kiểm tra không còn `workspace_role_id` null, đổi cột thành required.
5. Xóa enum field `role` khỏi hai bảng và chuyển toàn bộ authorization sang permission check.

DBML hiện tại hiển thị đồng thời `role` và `workspace_role_id` để nhìn được cả hai tầng. Trong database thực tế, chúng chỉ cùng tồn tại trong migration chuyển đổi ngắn hạn.
