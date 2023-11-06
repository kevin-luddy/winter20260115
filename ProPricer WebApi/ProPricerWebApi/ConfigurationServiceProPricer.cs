namespace APTSPropricerApi
{
	using ACV.Shared;
	using APTSPropricerApi.Common;
	using HealthChecks.UI.Client;
	using Microsoft.AspNetCore.Authentication.Negotiate;
	using Microsoft.AspNetCore.Authorization;
	using Microsoft.AspNetCore.Diagnostics.HealthChecks;
	using Serilog;

	/// <summary>
	/// Configuration Service Class that adds Negotiate and IES Auth Token
	/// </summary>
	public class ConfigurationServiceProPricer : ConfigurationServiceWeb
	{
		/// <summary>
		/// Add multiple authentication methods to Service
		/// </summary>
		/// <param name="services"></param>
		public void AddMultiAuthentication(IServiceCollection services)
		{
			services.AddAuthentication(options =>
			{
				options.DefaultScheme = NegotiateDefaults.AuthenticationScheme;
				options.DefaultChallengeScheme = NegotiateDefaults.AuthenticationScheme;
			})
				.AddNegotiate()
				.AddScheme<TokenAuthenticationOptions, TokenAuthenticationSchemeHandler>(
					Constants.IES_TOKEN_SCHEME,
					opts => { }
				);
		}

		/// <summary>
		/// Adds Authorization Policies to the Service Collection
		/// </summary>
		/// <param name="services">Service Collection</param>
		protected override void AddAuthorizationPolicies(IServiceCollection services)
		{
			services.AddAuthorization(options =>
			{
				// this lets us put [Authorize(Policy = "OnlyIesToken")] onto Controller
				var onlyIesTokenSchemePolicyBuilder = new AuthorizationPolicyBuilder(Constants.IES_TOKEN_SCHEME);
				options.AddPolicy("OnlyIesToken", onlyIesTokenSchemePolicyBuilder
					.RequireAuthenticatedUser()
					.Build());

				// this lets us put [Authorize(Policy = "OnlyNegotiate")] onto Controller
				var negotiatePolicyBuilder = new AuthorizationPolicyBuilder(NegotiateDefaults.AuthenticationScheme);
				options.AddPolicy("OnlyNegotiate", negotiatePolicyBuilder
					.RequireAuthenticatedUser()
					.Build());
			});
		}

		/// <summary>
		/// Configure Health Checks for the application (removing Database dependency)
		/// 
		/// Should be called from Startup.ConfigureServices
		/// </summary>
		/// <param name="services">Services which we are configuring</param>
		protected override void ConfigureHealthChecks(IServiceCollection services)
		{
			services.AddHealthChecks()
					.AddProcessAllocatedMemoryHealthCheck(512, name: "Memory Allocation");
		}

		/// <summary>
		/// Configure AppBuilder w/ Serilog and setup a health endpoint
		/// Add custom Middleware for User
		/// </summary>
		/// <param name="app">App to configure</param>
		public new void ConfigureAppBuilder(IApplicationBuilder app)
		{
			app.UseForwardedHeaders(this.GetForwardedHeadersOptions());
			app.UseAuthentication();
			app.UseHttpsRedirection();
			app.UseStaticFiles();
			app.UseSerilogRequestLogging();
			app.UseRouting();
			app.UseCors("CorsPolicy");
			app.UseAuthorization();
			app.UseMiddleware<ProPricerUserLoggingMiddleware>();
			app.UseMiddleware<CorrelationMiddleware>();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
				endpoints.MapHealthChecks("/Health", new HealthCheckOptions() { ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse });
			});

			app.Use(async (context, next) =>
			{
				context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
				context.Response.Headers.Add("X-Frame-Options", "DENY");
				context.Response.Headers.Add("X-Xss-Protection", "1; mode=block");

				await next();
			});
		}
	}
}
