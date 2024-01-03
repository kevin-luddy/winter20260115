using System.Security.Principal;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<IPrincipal>(
				provider => provider.GetService<IHttpContextAccessor>()?.HttpContext?.User);


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
app.UseRouting();
// TODO TIW app.UseCors
app.UseAuthorization();

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

app.Run();
