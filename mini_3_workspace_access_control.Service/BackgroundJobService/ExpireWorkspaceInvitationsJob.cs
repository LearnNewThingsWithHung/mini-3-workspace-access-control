using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using mini_3_workspace_access_control.Repo;
using mini_3_workspace_access_control.Repo.Enum;
using Quartz;

namespace mini_3_workspace_access_control.Service.BackgroundJobService;

public class ExpireWorkspaceInvitationsJob: IJob
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<ExpireWorkspaceInvitationsJob> _logger;

    public ExpireWorkspaceInvitationsJob(AppDbContext dbContext, ILogger<ExpireWorkspaceInvitationsJob> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
    
    public async Task Execute(IJobExecutionContext context)
    {
        var now = DateTimeOffset.UtcNow;
        
        var affectedRows = await _dbContext.WorkspaceInvitations
            .Where(x =>
                x.Status == InvitationStatus.Pending &&
                x.ExpiresAt <= now)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(
                    x => x.Status,
                    InvitationStatus.Expired)
                .SetProperty(
                    x => x.UpdatedAt,
                    now),
            context.CancellationToken);

        if (affectedRows > 0)
        {
            _logger.LogInformation("Expired {InvitationCount} workspace invitations}",  affectedRows);
        }        
    }
}