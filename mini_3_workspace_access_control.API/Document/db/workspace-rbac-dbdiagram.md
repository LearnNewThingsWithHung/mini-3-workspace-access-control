# Mini 3 — Workspace Access Control: DBML cho dbdiagram

Sao chép **phần code DBML bên dưới** vào [dbdiagram.io](https://dbdiagram.io). Phần này dùng cú pháp DBML hợp lệ; các ghi chú `Phase 1`/`Phase 2` chỉ mô tả lộ trình migration, không phải hai database khác nhau.

```dbml
Enum workspace_role_phase1 {
  Owner
  Manager
  Editor
  Viewer
}

Enum permission_effect {
  Allow
  Deny
}

Table people {
  id uuid [pk, not null]
  email varchar(320) [not null, unique]
  display_name varchar(200) [not null]
  is_active boolean [not null, default: true]
  is_deleted boolean [not null, default: false]
  created_at timestamptz [not null]
  updated_at timestamptz

  Note: 'Phase 1. Demo identity only; no password or JWT belongs in this mini project.'
}

Table workspaces {
  id uuid [pk, not null]
  code varchar(50) [not null, unique]
  name varchar(200) [not null]
  description varchar(1000)
  created_by_person_id uuid [not null]
  is_deleted boolean [not null, default: false]
  created_at timestamptz [not null]
  updated_at timestamptz

  Note: 'Tenant boundary. All authorization is evaluated inside one workspace.'
}

Table workspace_members {
  id uuid [pk, not null]
  workspace_id uuid [not null]
  person_id uuid [not null]
  role workspace_role_phase1 [not null, default: 'Viewer']
  workspace_role_id uuid
  is_deleted boolean [not null, default: false]
  created_at timestamptz [not null]
  updated_at timestamptz

  indexes {
    (workspace_id, person_id) [unique]
    (workspace_id, role)
    (workspace_id, workspace_role_id)
  }

  Note: 'Phase 1 uses role. Phase 2 backfills workspace_role_id, then makes it required and removes role.'
}

Table workspace_invitations {
  id uuid [pk, not null]
  workspace_id uuid [not null]
  email varchar(320) [not null]
  role workspace_role_phase1 [not null, default: 'Viewer']
  workspace_role_id uuid
  token_hash varchar(64) [not null, unique]
  expires_at timestamptz [not null]
  accepted_at timestamptz
  created_by_person_id uuid [not null]
  is_deleted boolean [not null, default: false]
  created_at timestamptz [not null]
  updated_at timestamptz

  indexes {
    (workspace_id, email)
    (workspace_id, email, accepted_at)
  }

  Note: 'Phase 1 uses role. Phase 2 transitions invitations to workspace_role_id. Store only SHA-256 token_hash, never the raw invitation token.'
}

Table permissions {
  id uuid [pk, not null]
  code varchar(100) [not null, unique]
  display_name varchar(150) [not null]
  description varchar(500)
  is_deleted boolean [not null, default: false]
  created_at timestamptz [not null]
  updated_at timestamptz

  Note: 'Phase 2 global catalogue. Seed values such as workspace.view, workspace.update, member.manage, role.manage, workspace.delete and ownership.transfer.'
}

Table workspace_roles {
  id uuid [pk, not null]
  workspace_id uuid [not null]
  name varchar(100) [not null]
  description varchar(500)
  is_system boolean [not null, default: false]
  is_deleted boolean [not null, default: false]
  created_at timestamptz [not null]
  updated_at timestamptz

  indexes {
    (workspace_id, name) [unique]
  }

  Note: 'Phase 2. Every workspace receives system roles Owner, Manager, Editor and Viewer; an Owner may create additional roles such as Intern or HR.'
}

Table workspace_role_permissions {
  id uuid [pk, not null]
  workspace_role_id uuid [not null]
  permission_id uuid [not null]
  is_deleted boolean [not null, default: false]
  created_at timestamptz [not null]
  updated_at timestamptz

  indexes {
    (workspace_role_id, permission_id) [unique]
  }

  Note: 'Phase 2 role-to-permission mapping. Presence of a row means the role grants the permission.'
}

Table member_permission_overrides {
  id uuid [pk, not null]
  workspace_member_id uuid [not null]
  permission_id uuid [not null]
  effect permission_effect [not null]
  is_deleted boolean [not null, default: false]
  created_at timestamptz [not null]
  updated_at timestamptz

  indexes {
    (workspace_member_id, permission_id) [unique]
  }

  Note: 'Phase 2 optional exception. Deny overrides role grants; Allow grants an extra permission. Keep it out of Phase 1.'
}

Ref: workspaces.created_by_person_id > people.id

Ref: workspace_members.workspace_id > workspaces.id
Ref: workspace_members.person_id > people.id
Ref: workspace_members.workspace_role_id > workspace_roles.id

Ref: workspace_invitations.workspace_id > workspaces.id
Ref: workspace_invitations.workspace_role_id > workspace_roles.id
Ref: workspace_invitations.created_by_person_id > people.id

Ref: workspace_roles.workspace_id > workspaces.id
Ref: workspace_role_permissions.workspace_role_id > workspace_roles.id
Ref: workspace_role_permissions.permission_id > permissions.id

Ref: member_permission_overrides.workspace_member_id > workspace_members.id
Ref: member_permission_overrides.permission_id > permissions.id
```
