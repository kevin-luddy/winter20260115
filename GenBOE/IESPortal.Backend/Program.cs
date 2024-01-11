using System.Diagnostics;
using GenBOE.DataBridge.DTO;
using GenTRAC.DataBridge.DTO;
using IES.ActionLogic.Common;
using IES.ActionLogic.ControllerLogic;
using IES.Core;
using IES.Core.Exceptions;
using IES.DataBridge.Loaders;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);
var config = new ApplicationConfigurationBase();

config.ConfigureBasics(builder.Host, builder.Services, builder.Configuration, builder.Configuration.GetConnectionString("IESEntities"));
config.AddWindowsAuthentication(builder.Services);

// Add Custom Services
builder.Services.AddScoped<ISecurityInformation, SecurityInformation>();
builder.Services.AddSingleton<ICache, Cache>();
builder.Services.AddScoped<IActiveDirectoryUtilities, ActiveDirectoryUtilities>();
builder.Services.AddTransient<IBannerLoader, BannerLoader>();
builder.Services.AddSingleton<BannerMediator>();
builder.Services.AddTransient<IESPortalAdminControllerLogic>();
builder.Services.AddScoped<PtmPickListMapper>();
builder.Services.AddScoped<BoePickListMapper>();
builder.Services.AddTransient<IOfflineApplicationLoader, OfflineApplicationLoader>();
builder.Services.AddTransient<ProposalTypeLULoader>();
builder.Services.AddTransient<ProposalClassLULoader>();
builder.Services.AddTransient<TypeOfRequestLULoader>();
builder.Services.AddTransient<GenTRAC.DataBridge.DTO.LineOfBusinessDataLoader>();
builder.Services.AddTransient<ProgramAreaDataLoader>();
builder.Services.AddTransient<ContractTypeLULoader>();
builder.Services.AddTransient<ContractTypeGroupLULoader>();
builder.Services.AddTransient<GenBOE.DataBridge.DTO.LineOfBusinessDataLoader>();
builder.Services.AddTransient<ProposalClassLoader>();
builder.Services.AddTransient<ContractTypeLoader>();
builder.Services.AddScoped<ITokenService, TokenService>();

var app = builder.Build();

config.ConfigureAppBuilder(app, app.Environment);

Serilog.Debugging.SelfLog.Enable(msg =>
{
	Debug.Print(msg);
	// Debugger.Break();  // used for debugging issues with serilog
});

IHostApplicationLifetime lifetime = app.Lifetime;

lifetime.ApplicationStopped.Register(() => Serilog.Log.CloseAndFlush());

app.Run();
