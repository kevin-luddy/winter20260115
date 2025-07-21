using IES.ActionLogic.Core.Common;
using IES.ActionLogic.Core.ControllerLogic;
using IES.ActionLogic.Core.IO.Export;
using IES.ActionLogic.Core.Mediator;
using IES.Common.Core;
using IES.Common.Core.Email;
using IES.Common.Core.Interfaces;
using IES.Common.Core.Loaders;
using IES.Common.Core.Security;
using IES.Common.Core.Services;
using IES.Common.Core.Utilities;
using IES.DataBridge.Common;
using IES.DataBridge.Loaders;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
ApplicationConfigurationBase config = new();
config.ConfigureBasics<Program>(builder, "IES_DATABASE");
config.AddWindowsAuthentication(builder.Services, builder.Configuration);

CommonUtilities.SetLicense();

// Add Custom Services
// Register ICache, Cache and Non Cache Data Loader
builder.Services.AddSingleton<ICacheService, CacheService>();
builder.Services.AddScoped<IActiveDirectoryService, ActiveDirectoryService>();
builder.Services.AddScoped<ITokenService, TokenService>();

// Register Mapper
builder.Services.AddScoped<ICommonDataMapper, CommonDataMapper>();

// Register Loaders
builder.Services.AddScoped<IFileAttachmentLoader, FileAttachmentLoader>();
builder.Services.AddScoped<ICobraDetailLoader, CobraDetailLoader>();
builder.Services.AddScoped<ICobraYearsLoader, CobraYearsLoader>();
builder.Services.AddScoped<IBurdenPoolLoader, BurdenPoolLoader>();
builder.Services.AddScoped<IRevisionLoader, RevisionLoader>();
builder.Services.AddScoped<ICacheDataLoader, CacheDataLoader>();
builder.Services.AddScoped<IRevisionMediator, RevisionMediator>();
builder.Services.AddScoped<IAreaLockingLoader, AreaLockingLoader>();
builder.Services.AddScoped<IRateCodeYearLoader, RateCodeYearLoader>();
builder.Services.AddScoped<IProPricerRateCodeXrefLoader, ProPricerRateCodeXrefLoader>();
builder.Services.AddScoped<IRateConfigLoader, RateConfigLoader>();
builder.Services.AddScoped<IRateCodeReplicationLoader, RateCodeReplicationLoader>();
builder.Services.AddScoped<IRateDetailLoader, RateDetailLoader>();
builder.Services.AddScoped<ISectionLoader, SectionLoader>();
builder.Services.AddScoped<IWhosOnlineLoader, WhosOnlineLoader>();

// Register Misc Classes
builder.Services.AddScoped<IDataFetchingScheduler, DataFetchingScheduler>();
builder.Services.AddScoped<IIESEmailer, IESEmailer>();
builder.Services.AddScoped<ISecurityInformation, SecurityInformation>();
builder.Services.AddScoped<IPPRDExporter, PPRDExporter>();
builder.Services.AddScoped<IRdmRevisionExporter, RdmRevisionExporter>();
builder.Services.AddScoped<RateFormatter>();

// Register Action Logic
builder.Services.AddScoped<IAdminControllerLogic, AdminControllerLogic>();
builder.Services.AddScoped<IBurdenPoolControllerLogic, BurdenPoolControllerLogic>();
builder.Services.AddScoped<IHomeControllerLogic, HomeControllerLogic>();
builder.Services.AddScoped<IPPRDControllerLogic, PPRDControllerLogic>();
builder.Services.AddScoped<IRateControllerLogic, RateControllerLogic>();
builder.Services.AddScoped<IReportsControllerLogic, ReportsControllerLogic>();
builder.Services.AddScoped<IVersionControllerLogic, VersionControllerLogic>();
builder.Services.AddScoped<IFileAttachmentControllerLogic, FileAttachmentControllerLogic>();

WebApplication app = config.ConfigureAppBuilder(builder);
IES.Common.Core.Utilities.CommonUtilities.LogEnvironmentSettings(app, app.Configuration, app.Environment);
app.Run();
