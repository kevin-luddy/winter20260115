using GenBOE.DataBridge.Core.Picklists;
using GenTRAC.DataBridge.Core.DTO.OrgData;
using GenTRAC.DataBridge.Core.DTO.PickLists;
using GenTRAC.DataBridge.Core.DTO.PickLists.Common;
using IES.ActionLogic.Core.Common;
using IES.ActionLogic.Core.ControllerLogic;
using IES.Common.Core;
using IES.Common.Core.Interfaces;
using IES.Common.Core.Security;
using IES.Common.Core.Services;
using IES.DataBridge.Loaders;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
ApplicationConfigurationBase config = new();
config.ConfigureBasics<Program>(builder, "IESEntities");
config.AddWindowsAuthentication(builder.Services, builder.Configuration);

// Add Custom Services
builder.Services.AddScoped<ISecurityInformation, SecurityInformation>();
builder.Services.AddSingleton<ICacheService, CacheService>();
builder.Services.AddScoped<IActiveDirectoryService, ActiveDirectoryService>();
builder.Services.AddTransient<IBannerLoader, BannerLoader>();
builder.Services.AddSingleton<BannerMediator>();
builder.Services.AddTransient<IESPortalAdminControllerLogic>();
builder.Services.AddScoped<PtmPickListMapper>();
builder.Services.AddScoped<BoePickListMapper>();
builder.Services.AddTransient<IOfflineApplicationLoader, OfflineApplicationLoader>();
builder.Services.AddTransient<ProposalTypeLULoader>();
builder.Services.AddTransient<ProposalClassLULoader>();
builder.Services.AddTransient<TypeOfRequestLULoader>();
builder.Services.AddTransient<GenTRAC.DataBridge.Core.DTO.OrgData.LineOfBusinessDataLoader>();
builder.Services.AddTransient<ProgramAreaDataLoader>();
builder.Services.AddTransient<ContractTypeLULoader>();
builder.Services.AddTransient<ContractTypeGroupLULoader>();
builder.Services.AddTransient<GenBOE.DataBridge.Core.Picklists.LineOfBusinessDataLoader>();
builder.Services.AddTransient<ProposalClassLoader>();
builder.Services.AddTransient<ContractTypeLoader>();
builder.Services.AddScoped<ITokenService, TokenService>();

WebApplication app = config.ConfigureAppBuilder(builder);
app.Run();