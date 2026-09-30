using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mini_3_workspace_access_control.Repo.Migrations
{
    /// <inheritdoc />
    public partial class config_job_background_service : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ux_workspace_invitations_active_workspace_email",
                table: "workspace_invitations");

            migrationBuilder.AddColumn<string>(
                name: "status",
                table: "workspace_invitations",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Pending");

            migrationBuilder.CreateIndex(
                name: "ix_workspace_invitations_pending_expires_at",
                table: "workspace_invitations",
                column: "expires_at",
                filter: "status = 'Expired' AND is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ux_workspace_invitations_active_workspace_email",
                table: "workspace_invitations",
                columns: new[] { "workspace_id", "email" },
                unique: true,
                filter: "status = 'Pending' AND is_deleted = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_workspace_invitations_pending_expires_at",
                table: "workspace_invitations");

            migrationBuilder.DropIndex(
                name: "ux_workspace_invitations_active_workspace_email",
                table: "workspace_invitations");

            migrationBuilder.DropColumn(
                name: "status",
                table: "workspace_invitations");

            migrationBuilder.CreateIndex(
                name: "ux_workspace_invitations_active_workspace_email",
                table: "workspace_invitations",
                columns: new[] { "workspace_id", "email" },
                unique: true,
                filter: "accepted_at IS NULL AND is_deleted = false");
        }
    }
}
