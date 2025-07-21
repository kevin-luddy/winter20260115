// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

using GenBOE.DataBridge.Core.Common;
using GenBOE.DataBridge.Core.Common.Calculations;
using GenBOE.DataBridge.Core.IO.Export;
using GenBOE.DataBridge.Core.Loaders;
using GenBOE.Reports.Backend.Services;
using IES.Common.Core;
using IES.Common.Core.Configuration;
using IES.Common.Core.Enums;
using IES.Common.Core.Interfaces;
using IES.Common.Core.Loaders;
using IES.Common.Core.Security;
using IES.Common.Core.Services;
using IES.Common.Core.Utilities;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
ApplicationConfigurationBase config = new();
config.ConfigureBasics<Program>(builder, "BOE_DATABASE");
config.AddWindowsAuthentication(builder.Services, builder.Configuration);

CommonUtilities.SetLicense();

// Add Custom Services
builder.Services.AddScoped<ISecurityInformation, SecurityInformation>();
builder.Services.AddSingleton<ICacheService, CacheService>();
builder.Services.AddScoped<IActiveDirectoryService, ActiveDirectoryService>();
builder.Services.AddSingleton<ICacheDataLoader, CacheDataLoader>();
builder.Services.AddScoped<IBoeExportService, BoeExportService>();
builder.Services.AddSingleton<ICommonDataLoader, CommonDataLoader>();
builder.Services.AddSingleton<ICommonDataMapper, CommonDataMapper>();

builder.Services.AddSingleton<TravelTripCostCalculation>();

// builder.Services.AddSingleton<IWorkspaceExportFormatDTODataLoader, WorkspaceExportFormatDTODataLoader>();

SystemConfiguration.BuildConfiguration = builder.Configuration;
CompanyConfiguration companyMode = SystemConfiguration.Instance().CompanyMode;
if (companyMode == CompanyConfiguration.SpaceSystems)
{
	builder.Services.AddScoped<IVariableSelectBOEtoSumCalculation, VariableSelectBOEtoSumCalculationSpaceSystems>();
	builder.Services.AddScoped<IBOEExporter, BOEExporter>();
	builder.Services.AddSingleton<BOEExportConverter>();
	builder.Services.AddSingleton<IVariableSelectBOEtoSumCalculation, VariableSelectBOEtoSumCalculationSpaceSystems>();
	builder.Services.AddSingleton<IBOECustomExporter, BOECustomExporterSSC>();
}
else if (companyMode == CompanyConfiguration.MST)
{
	builder.Services.AddSingleton<IVariableSelectBOEtoSumCalculation, VariableSelectBOEtoSumCalculationMST>();
	builder.Services.AddSingleton<IEscalationRatesDTOLoader, EscalationRatesDTOLoader>();
	builder.Services.AddSingleton<IMSTTravelNonzoneFeesAndCostsDTODataLoader, MSTTravelNonzoneFeesAndCostsDTODataLoader>();
	builder.Services.AddSingleton<IMSTZoneTravelResourceDTODataLoader, MSTZoneTravelResourceDTODataLoader>();
	builder.Services.AddSingleton<MSTZoneTravelResourceDTODataLoader>();
	builder.Services.AddSingleton<RMSZoneTravelRatesFeesDataLoader>();
	builder.Services.AddSingleton<IVariableSelectBOEtoSumCalculation, VariableSelectBOEtoSumCalculationMST>();
	builder.Services.AddScoped<BOEExporter>();
	builder.Services.AddScoped<BOEExporterMST>();
	builder.Services.AddScoped<BOEExportConverter, BOEExportConverterRMS>();
	builder.Services.AddScoped<IBOEExporter, BOEExporterMSTDecorator>();
	builder.Services.AddSingleton<IBOECustomExporter, BOECustomExporterMST>();
}

WebApplication app = config.ConfigureAppBuilder(builder);
IES.Common.Core.Utilities.CommonUtilities.LogEnvironmentSettings(app, app.Configuration, app.Environment);
app.Run();