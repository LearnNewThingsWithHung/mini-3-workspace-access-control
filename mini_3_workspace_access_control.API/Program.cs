using System.Reflection.Metadata;
using FluentValidation;
using mini_3_workspace_access_control.API.Extensions;
using mini_3_workspace_access_control.API.Middleware;
using mini_3_workspace_access_control.Repo;
using Microsoft.EntityFrameworkCore;
using mini_3_workspace_access_control.Service.BackgroundJobService;
using Quartz;
using MailService = mini_3_workspace_access_control.Service.MailService;
using JwtService = mini_3_workspace_access_control.Service.JwtService;
using PersonAccess = mini_3_workspace_access_control.Service.PersonAccess;
using WorkspaceService = mini_3_workspace_access_control.Service.WorkSpace;
using WorkspaceMemberService = mini_3_workspace_access_control.Service.WorkSpaceMember;
using InvitationService = mini_3_workspace_access_control.Service.Invitation;


    var builder = WebApplication.CreateBuilder(args);
    
    builder.Services.AddControllers();
    // Add services to the container.
    builder.Services.AddEndpointsApiExplorer();

    builder.Services.AddDbContext<AppDbContext>(options =>
        options
            .UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
            .UseSnakeCaseNamingConvention()
    );

    builder.Services.ConfigureRateLimiter();
    builder.Services.AddJwtServices(builder.Configuration);
    builder.Services.AddSwaggerServices();
    builder.Services.AddHttpContextAccessor();

    builder.Services.AddScoped<MailService.IService, MailService.Service>();
    builder.Services.AddScoped<JwtService.IService, JwtService.Service>();
    builder.Services.AddScoped<PersonAccess.IService, PersonAccess.Service>();
    builder.Services.AddScoped<WorkspaceService.IService, WorkspaceService.Service>();
    builder.Services.AddScoped<WorkspaceMemberService.IService, WorkspaceMemberService.Service>();
    builder.Services.AddScoped<InvitationService.IService, InvitationService.Service>();
    builder.Services.AddTransient<GlobalExceptionHandlerMiddleware>();

    //builder.Services.AddValidatorsFromAssembly(AssemblyReference.Assembly);

    var expireInvitationJobKey = new JobKey(nameof(ExpireWorkspaceInvitationsJob));

    builder.Services.AddQuartz(options =>
    {
        options.AddJob<ExpireWorkspaceInvitationsJob>(x => x.WithIdentity(expireInvitationJobKey));
        options.AddTrigger(x => x
            .ForJob(expireInvitationJobKey)
            .WithIdentity(
                $"{nameof(ExpireWorkspaceInvitationsJob)}-trigger")
            .StartNow()
            .WithSimpleSchedule(x => x
                .WithIntervalInMinutes(1)
                .RepeatForever()));
    });

    builder.Services.AddQuartzHostedService(x =>
    {
        x.WaitForJobsToComplete = true;
    });
    
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowFrontend", policy =>
        {
            policy
                .WithOrigins("http://localhost:4200")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
    });


    var app = builder.Build();
    app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwaggerAPI();
    }

    app.UseCors("AllowFrontend");

    app.UseRateLimiter();

    app.UseAuthentication();

    app.UseAuthorization();

    app.MapControllers();

    app.Run();
