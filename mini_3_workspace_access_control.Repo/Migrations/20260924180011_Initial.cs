using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mini_3_workspace_access_control.Repo.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "people",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    display_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_people", x => x.id);
                    table.CheckConstraint("ck_people_display_name_not_blank", "length(btrim(display_name)) > 0");
                    table.CheckConstraint("ck_people_email_normalized", "email = lower(btrim(email))");
                    table.CheckConstraint("ck_people_email_not_blank", "length(btrim(email)) > 0");
                });

            migrationBuilder.CreateTable(
                name: "workspaces",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_by_person_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_workspaces", x => x.id);
                    table.CheckConstraint("ck_workspaces_code_format", "code ~ '^[a-z0-9]+(-[a-z0-9]+)*$'");
                    table.CheckConstraint("ck_workspaces_code_normalized", "code = lower(btrim(code))");
                    table.CheckConstraint("ck_workspaces_name_not_blank", "length(btrim(name)) > 0");
                    table.ForeignKey(
                        name: "fk_workspaces_people_created_by_person_id",
                        column: x => x.created_by_person_id,
                        principalTable: "people",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "workspace_invitations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    workspace_id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Viewer"),
                    token_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    accepted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_by_person_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_workspace_invitations", x => x.id);
                    table.CheckConstraint("ck_workspace_invitations_accepted_after_create", "accepted_at IS NULL OR accepted_at >= created_at");
                    table.CheckConstraint("ck_workspace_invitations_email_normalized", "email = lower(btrim(email))");
                    table.CheckConstraint("ck_workspace_invitations_email_not_blank", "length(btrim(email)) > 0");
                    table.CheckConstraint("ck_workspace_invitations_expiry", "expires_at > created_at");
                    table.CheckConstraint("ck_workspace_invitations_role_valid", "role IN ('Owner', 'Manager', 'Editor', 'Viewer')");
                    table.CheckConstraint("ck_workspace_invitations_token_hash_hex", "token_hash ~ '^[0-9a-f]{64}$'");
                    table.ForeignKey(
                        name: "fk_workspace_invitations_people_created_by_person_id",
                        column: x => x.created_by_person_id,
                        principalTable: "people",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_workspace_invitations_workspaces_workspace_id",
                        column: x => x.workspace_id,
                        principalTable: "workspaces",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "workspace_members",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    workspace_id = table.Column<Guid>(type: "uuid", nullable: false),
                    person_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Viewer"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_workspace_members", x => x.id);
                    table.CheckConstraint("ck_workspace_members_role_valid", "role IN ('Owner', 'Manager', 'Editor', 'Viewer')");
                    table.ForeignKey(
                        name: "fk_workspace_members_people_person_id",
                        column: x => x.person_id,
                        principalTable: "people",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_workspace_members_workspaces_workspace_id",
                        column: x => x.workspace_id,
                        principalTable: "workspaces",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ux_people_email",
                table: "people",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_workspace_invitations_created_by_person_id",
                table: "workspace_invitations",
                column: "created_by_person_id");

            migrationBuilder.CreateIndex(
                name: "ux_workspace_invitations_active_workspace_email",
                table: "workspace_invitations",
                columns: new[] { "workspace_id", "email" },
                unique: true,
                filter: "accepted_at IS NULL AND is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ux_workspace_invitations_token_hash",
                table: "workspace_invitations",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_workspace_members_person_id",
                table: "workspace_members",
                column: "person_id");

            migrationBuilder.CreateIndex(
                name: "ix_workspace_members_workspace_role_active",
                table: "workspace_members",
                columns: new[] { "workspace_id", "role" },
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ux_workspace_members_workspace_person_active",
                table: "workspace_members",
                columns: new[] { "workspace_id", "person_id" },
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_workspaces_created_by_person_id",
                table: "workspaces",
                column: "created_by_person_id");

            migrationBuilder.CreateIndex(
                name: "ux_workspaces_code",
                table: "workspaces",
                column: "code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "workspace_invitations");

            migrationBuilder.DropTable(
                name: "workspace_members");

            migrationBuilder.DropTable(
                name: "workspaces");

            migrationBuilder.DropTable(
                name: "people");
        }
    }
}
