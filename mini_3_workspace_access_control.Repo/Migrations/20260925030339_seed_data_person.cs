using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace mini_3_workspace_access_control.Repo.Migrations
{
    /// <inheritdoc />
    public partial class seed_data_person : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "people",
                columns: new[] { "id", "created_at", "display_name", "email", "is_active", "updated_at" },
                values: new object[,]
                {
                    { new Guid("1010a001-1001-4001-8001-000000000001"), new DateTimeOffset(new DateTime(2026, 9, 1, 8, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Linh Nguyễn", "linh.nguyen@workspace-demo.test", true, null },
                    { new Guid("1010a002-1002-4002-8002-000000000002"), new DateTimeOffset(new DateTime(2026, 9, 1, 8, 5, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Minh Trần", "minh.tran@workspace-demo.test", true, null },
                    { new Guid("1010a003-1003-4003-8003-000000000003"), new DateTimeOffset(new DateTime(2026, 9, 1, 8, 10, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Anh Phạm", "anh.pham@workspace-demo.test", true, null },
                    { new Guid("1010a004-1004-4004-8004-000000000004"), new DateTimeOffset(new DateTime(2026, 9, 1, 8, 15, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Khánh Lê", "khanh.le@workspace-demo.test", true, null },
                    { new Guid("1010a005-1005-4005-8005-000000000005"), new DateTimeOffset(new DateTime(2026, 9, 1, 8, 20, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Quang Võ", "quang.vo@workspace-demo.test", true, null },
                    { new Guid("1010a006-1006-4006-8006-000000000006"), new DateTimeOffset(new DateTime(2026, 9, 1, 8, 25, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Thư Hoàng", "thu.hoang@workspace-demo.test", true, null },
                    { new Guid("1010a007-1007-4007-8007-000000000007"), new DateTimeOffset(new DateTime(2026, 9, 1, 8, 30, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Nam Đặng", "nam.dang@workspace-demo.test", true, null },
                    { new Guid("1010a008-1008-4008-8008-000000000008"), new DateTimeOffset(new DateTime(2026, 9, 1, 8, 35, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Mai Bùi", "mai.bui@workspace-demo.test", true, null },
                    { new Guid("1010a009-1009-4009-8009-000000000009"), new DateTimeOffset(new DateTime(2026, 9, 1, 8, 40, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Huy Đỗ", "huy.do@workspace-demo.test", true, null },
                    { new Guid("1010a010-1010-4010-8010-000000000010"), new DateTimeOffset(new DateTime(2026, 9, 1, 8, 45, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Hà Ngô", "ha.ngo@workspace-demo.test", true, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "people",
                keyColumn: "id",
                keyValue: new Guid("1010a001-1001-4001-8001-000000000001"));

            migrationBuilder.DeleteData(
                table: "people",
                keyColumn: "id",
                keyValue: new Guid("1010a002-1002-4002-8002-000000000002"));

            migrationBuilder.DeleteData(
                table: "people",
                keyColumn: "id",
                keyValue: new Guid("1010a003-1003-4003-8003-000000000003"));

            migrationBuilder.DeleteData(
                table: "people",
                keyColumn: "id",
                keyValue: new Guid("1010a004-1004-4004-8004-000000000004"));

            migrationBuilder.DeleteData(
                table: "people",
                keyColumn: "id",
                keyValue: new Guid("1010a005-1005-4005-8005-000000000005"));

            migrationBuilder.DeleteData(
                table: "people",
                keyColumn: "id",
                keyValue: new Guid("1010a006-1006-4006-8006-000000000006"));

            migrationBuilder.DeleteData(
                table: "people",
                keyColumn: "id",
                keyValue: new Guid("1010a007-1007-4007-8007-000000000007"));

            migrationBuilder.DeleteData(
                table: "people",
                keyColumn: "id",
                keyValue: new Guid("1010a008-1008-4008-8008-000000000008"));

            migrationBuilder.DeleteData(
                table: "people",
                keyColumn: "id",
                keyValue: new Guid("1010a009-1009-4009-8009-000000000009"));

            migrationBuilder.DeleteData(
                table: "people",
                keyColumn: "id",
                keyValue: new Guid("1010a010-1010-4010-8010-000000000010"));
        }
    }
}
