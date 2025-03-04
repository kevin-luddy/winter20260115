// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

using GenBOE.Reports.Backend.Services;
using IES.Common.Core;
using IES.Common.Core.Interfaces;
using IES.Common.Core.Loaders;
using IES.Common.Core.Security;
using IES.Common.Core.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
ApplicationConfigurationBase config = new();
config.ConfigureBasics<Program>(builder, "BOE_DATABASE");
config.AddWindowsAuthentication(builder.Services, builder.Configuration);

// Add Custom Services
builder.Services.AddScoped<ISecurityInformation, SecurityInformation>();
builder.Services.AddSingleton<ICacheService, CacheService>();
builder.Services.AddScoped<IActiveDirectoryService, ActiveDirectoryService>();
builder.Services.AddScoped<ICacheDataLoader, CacheDataLoader>();
builder.Services.AddSingleton<IBoeExportService, BoeExportService>();

WebApplication app = config.ConfigureAppBuilder(builder);
IES.Common.Core.Utilities.CommonUtilities.LogEnvironmentSettings(app, app.Configuration, app.Environment);
app.Run();