using GenTRAC.DataBridge.Core.Common.Security;
using GenTRAC.DataBridge.Core.DTO.Proposal;
using GenTRAC.DataBridge.Core.DTO.User;
using IES.ActionLogic.Core.Common;
using IES.ActionLogic.Core.ControllerLogic;
using IES.ActionLogic.Core.IO.Export;
using IES.Common.Core;
using IES.Common.Core.Interfaces;
using IES.Common.Core.Loaders;
using IES.Common.Core.Security;
using IES.Common.Core.Services;
using IES.Common.Core.Utilities;
using IES.DataBridge.Loaders;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
ApplicationConfigurationBase config = new();
config.ConfigureBasics<Program>(builder, "IES_DATABASE");
config.AddWindowsAuthentication(builder.Services, builder.Configuration);
builder.Services.AddCors(options =>
{
	options.AddDefaultPolicy(builder =>
		builder.SetIsOriginAllowed(_ => true)
		.AllowAnyMethod()
		.AllowAnyHeader()
		.AllowCredentials());
});

// Add Custom Services
builder.Services.AddScoped<ISecurityInformation, SecurityInformation>();
builder.Services.AddSingleton<ICacheService, CacheService>();
builder.Services.AddScoped<IActiveDirectoryService, ActiveDirectoryService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<ISecurityMapper, SecurityMapper>();
builder.Services.AddScoped<TokenHandling>();
builder.Services.AddScoped<IDocumentControllerLogic, DocumentControllerLogic>();
builder.Services.AddScoped<IWhosOnlineLoader, WhosOnlineLoader>();
builder.Services.AddScoped<IRateDetailLoader, RateDetailLoader>();
builder.Services.AddScoped<ISecurityUserAuthorizationsDataLoader, SecurityUserAuthorizationsDataLoader>();
builder.Services.AddScoped<IUserMapper, UserMapper>();
builder.Services.AddScoped<ICacheDataLoader, CacheDataLoader>();
builder.Services.AddScoped<IAreaLockingLoader, AreaLockingLoader>();
builder.Services.AddScoped<IDocumentLoader, DocumentLoader>();
builder.Services.AddScoped<IDocumentDetailLoader, DocumentDetailLoader>();
builder.Services.AddScoped<IRdsbRateCodeXrefLoader, RdsbRateCodeXrefLoader>();
builder.Services.AddScoped<IRdsbSectionXrefLoader, RdsbSectionXrefLoader>();
builder.Services.AddScoped<IProposalLoader, ProposalLoader>();
builder.Services.AddScoped<IRateCodeYearLoader, RateCodeYearLoader>();
builder.Services.AddScoped<IRateConfigLoader, RateConfigLoader>();
builder.Services.AddScoped<IRevisionLoader, RevisionLoader>();
builder.Services.AddScoped<ISectionLoader, SectionLoader>();
builder.Services.AddScoped<IFileAttachmentLoader, FileAttachmentLoader>();
builder.Services.AddScoped<IPPRDExporter, PPRDExporter>();
builder.Services.AddScoped<IUserLoader, UserLoader>();
builder.Services.AddScoped<IProPricerRateCodeXrefLoader, ProPricerRateCodeXrefLoader>();
builder.Services.AddScoped<RateFormatter>();

WebApplication app = config.ConfigureAppBuilder(builder);
app.Run();