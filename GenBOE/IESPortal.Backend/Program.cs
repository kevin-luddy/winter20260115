using System.Configuration;
using System.Diagnostics;
using System.Net.Http;
using System.Security.Principal;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using GenBOE.DataBridge.DTO;
using GenTRAC.DataBridge.DTO;
using IES.ActionLogic.Common;
using IES.ActionLogic.ControllerLogic;
using IES.Core;
using IES.DataBridge.Loaders;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Serialization;
using Serilog;
using Serilog.Ui.MsSqlServerProvider;
using Serilog.Ui.Web;

var builder = WebApplication.CreateBuilder(args);

//Add support to logging with SERILOG
builder.Host.UseSerilog((context, configuration) =>
	configuration.ReadFrom.Configuration(context.Configuration));

// Add services to the container.
builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();
builder.Services.AddTransient<IPrincipal>(
				provider => provider.GetService<IHttpContextAccessor>()?.HttpContext?.User);

builder.Services.AddCors(options => options.AddPolicy("CorsPolicy", builder =>
					 builder.WithOrigins("https://*.lmco.com")
							 .AllowAnyMethod()
							 .AllowAnyHeader()
							 .AllowCredentials()));
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
builder.Services.AddSerilogUi(options =>
	  // each provider exposes extension methods to configure.
	  // example with MSSqlServerProvider:
	  options.UseSqlServer(builder.Configuration.GetConnectionString("IESEntities"), "Logs"));


builder.Services.AddControllers().AddJsonOptions(options =>
{
	options.JsonSerializerOptions.PropertyNamingPolicy = null;
}).AddNewtonsoftJson(x =>
{
	x.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
	x.SerializerSettings.ContractResolver = new DefaultContractResolver();
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthentication(options =>
{
	options.DefaultScheme = NegotiateDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = NegotiateDefaults.AuthenticationScheme;
})
				.AddNegotiate();
				//.AddScheme<TokenAuthenticationOptions, TokenAuthenticationSchemeHandler>(
				//	Constants.IES_TOKEN_SCHEME,
				//	opts => { }
				//);
builder.Services.AddAuthorization(options =>
{
	//// this lets us put [Authorize(Policy = "OnlyIesToken")] onto Controller
	//AuthorizationPolicyBuilder onlyIesTokenSchemePolicyBuilder = new(Constants.IES_TOKEN_SCHEME);
	//options.AddPolicy("OnlyIesToken", onlyIesTokenSchemePolicyBuilder
	//	.RequireAuthenticatedUser()
	//	.Build());

	// this lets us put [Authorize(Policy = "OnlyNegotiate")] onto Controller
	AuthorizationPolicyBuilder negotiatePolicyBuilder = new(NegotiateDefaults.AuthenticationScheme);
	options.AddPolicy("OnlyNegotiate", negotiatePolicyBuilder
		.RequireAuthenticatedUser()
		.Build());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

ForwardedHeadersOptions forwardedHeadersOptions = new ForwardedHeadersOptions
{
	ForwardedHeaders = (ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto)
};
forwardedHeadersOptions.KnownNetworks.Clear();
forwardedHeadersOptions.KnownProxies.Clear();

app.UseForwardedHeaders(forwardedHeadersOptions);
app.UseAuthentication();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSerilogRequestLogging();
app.UseRouting();
app.UseCors("CorsPolicy");
app.UseAuthorization();
app.UseMiddleware<UserLoggingMiddleware>();
app.UseMiddleware<CorrelationMiddleware>();

// Enable middleware to serve log-ui (HTML, JS, CSS, etc.).
app.UseSerilogUi();

app.UseEndpoints(endpoints =>
{
	endpoints.MapControllers();
	// TODO TIW endpoints.MapHealthChecks("/Health", new HealthCheckOptions() { ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse });
});

app.Use(async (context, next) =>
{
	context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
	context.Response.Headers.Add("X-Frame-Options", "DENY");
	context.Response.Headers.Add("X-Xss-Protection", "1; mode=block");

	await next();
});

Serilog.Debugging.SelfLog.Enable(msg =>
{
	Debug.Print(msg);
	Debugger.Break();
});

app.Run();
