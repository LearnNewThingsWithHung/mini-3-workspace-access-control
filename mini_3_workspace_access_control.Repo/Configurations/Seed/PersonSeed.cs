using mini_3_workspace_access_control.Repo.Entity;

namespace mini_3_workspace_access_control.Repo.Configurations.Seed;

public static class PersonSeed
{
    public static readonly Person[] Data =
    [
        new()
        {
            Id = Guid.Parse("1010a001-1001-4001-8001-000000000001"),
            Email = "linh.nguyen@workspace-demo.test",
            DisplayName = "Linh Nguyễn",
            IsActive = true,
            IsDeleted = false,
            CreatedAt = new DateTimeOffset(2026, 9, 1, 8, 0, 0, TimeSpan.Zero)
        },
        new()
        {
            Id = Guid.Parse("1010a002-1002-4002-8002-000000000002"),
            Email = "minh.tran@workspace-demo.test",
            DisplayName = "Minh Trần",
            IsActive = true,
            IsDeleted = false,
            CreatedAt = new DateTimeOffset(2026, 9, 1, 8, 5, 0, TimeSpan.Zero)
        },
        new()
        {
            Id = Guid.Parse("1010a003-1003-4003-8003-000000000003"),
            Email = "anh.pham@workspace-demo.test",
            DisplayName = "Anh Phạm",
            IsActive = true,
            IsDeleted = false,
            CreatedAt = new DateTimeOffset(2026, 9, 1, 8, 10, 0, TimeSpan.Zero)
        },
        new()
        {
            Id = Guid.Parse("1010a004-1004-4004-8004-000000000004"),
            Email = "khanh.le@workspace-demo.test",
            DisplayName = "Khánh Lê",
            IsActive = true,
            IsDeleted = false,
            CreatedAt = new DateTimeOffset(2026, 9, 1, 8, 15, 0, TimeSpan.Zero)
        },
        new()
        {
            Id = Guid.Parse("1010a005-1005-4005-8005-000000000005"),
            Email = "quang.vo@workspace-demo.test",
            DisplayName = "Quang Võ",
            IsActive = true,
            IsDeleted = false,
            CreatedAt = new DateTimeOffset(2026, 9, 1, 8, 20, 0, TimeSpan.Zero)
        },
        new()
        {
            Id = Guid.Parse("1010a006-1006-4006-8006-000000000006"),
            Email = "thu.hoang@workspace-demo.test",
            DisplayName = "Thư Hoàng",
            IsActive = true,
            IsDeleted = false,
            CreatedAt = new DateTimeOffset(2026, 9, 1, 8, 25, 0, TimeSpan.Zero)
        },
        new()
        {
            Id = Guid.Parse("1010a007-1007-4007-8007-000000000007"),
            Email = "nam.dang@workspace-demo.test",
            DisplayName = "Nam Đặng",
            IsActive = true,
            IsDeleted = false,
            CreatedAt = new DateTimeOffset(2026, 9, 1, 8, 30, 0, TimeSpan.Zero)
        },
        new()
        {
            Id = Guid.Parse("1010a008-1008-4008-8008-000000000008"),
            Email = "mai.bui@workspace-demo.test",
            DisplayName = "Mai Bùi",
            IsActive = true,
            IsDeleted = false,
            CreatedAt = new DateTimeOffset(2026, 9, 1, 8, 35, 0, TimeSpan.Zero)
        },
        new()
        {
            Id = Guid.Parse("1010a009-1009-4009-8009-000000000009"),
            Email = "huy.do@workspace-demo.test",
            DisplayName = "Huy Đỗ",
            IsActive = true,
            IsDeleted = false,
            CreatedAt = new DateTimeOffset(2026, 9, 1, 8, 40, 0, TimeSpan.Zero)
        },
        new()
        {
            Id = Guid.Parse("1010a010-1010-4010-8010-000000000010"),
            Email = "ha.ngo@workspace-demo.test",
            DisplayName = "Hà Ngô",
            IsActive = true,
            IsDeleted = false,
            CreatedAt = new DateTimeOffset(2026, 9, 1, 8, 45, 0, TimeSpan.Zero)
        }
    ];
}
