

namespace IES.Core
{
	using System;
	using System.Security.Principal;
	using HealthChecks.UI.Client;
	using IES.Core.Exceptions;
	using IES.Core.Logging;
	using Microsoft.AspNetCore.Authentication.Negotiate;
	using Microsoft.AspNetCore.Authorization;
	using Microsoft.AspNetCore.Builder;
	using Microsoft.AspNetCore.Diagnostics.HealthChecks;
	using Microsoft.AspNetCore.Hosting;
	using Microsoft.AspNetCore.Http;
	using Microsoft.AspNetCore.HttpOverrides;
	using Microsoft.Extensions.Configuration;
	using Microsoft.Extensions.DependencyInjection;
	using Microsoft.Extensions.DependencyModel;
	using Microsoft.Extensions.Hosting;
	using Newtonsoft.Json.Serialization;
	using Serilog;
	using Serilog.Settings.Configuration;
	using Serilog.Sinks.MSSqlServer;
	using Serilog.Ui.MsSqlServerProvider;
	using Serilog.Ui.Web;

	/// <summary>
	/// Application Configuration Base class
	/// </summary>
	public class ApplicationConfigurationBase
	{
		/// <summary>
		/// Adds Authentication to the site
		/// </summary>
		public void AddWindowsAuthentication(IServiceCollection services)
		{
			services.AddAuthentication(options =>
			{
				options.DefaultScheme = NegotiateDefaults.AuthenticationScheme;
				options.DefaultChallengeScheme = NegotiateDefaults.AuthenticationScheme;
			})
				.AddNegotiate();
			// TODO:  This is future work for genBOE integration
			//.AddScheme<TokenAuthenticationOptions, TokenAuthenticationSchemeHandler>(
			//				//	Constants.IES_TOKEN_SCHEME,
			//				//	opts => { }
			//				//);

			services.AddAuthorization(options =>
			{
				// TODO:  This is future work for genBOE integration
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
		}

		/// <summary>
		/// Configure Basics for an application
		/// </summary>
		/// <param name="host">Host Builder</param>
		/// <param name="services">Services Collection</param>
		/// <param name="configuration">Configuration</param>
		/// <param name="connectionString">Connection string used for Serilog logging to DB if turned on</param>
		public void ConfigureBasics(IHostBuilder host, IServiceCollection services, IConfiguration configuration, string connectionString)
		{
			services.AddMemoryCache();
			services.AddHttpContextAccessor();
			services.AddHttpClient();
			services.AddTransient<IPrincipal>(
				provider => provider.GetService<IHttpContextAccessor>()?.HttpContext?.User);

			this.ConfigureSerilog(host, services, configuration, connectionString);
			services.AddSingleton(Log.Logger);
			this.ConfigureHealthChecks(services, connectionString);
			this.ConfigureCors(services);

			services.AddControllers().AddJsonOptions(options =>
			{
				options.JsonSerializerOptions.PropertyNamingPolicy = null;
			}).AddNewtonsoftJson(x =>
			{
				x.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
				x.SerializerSettings.ContractResolver = new DefaultContractResolver();
			});

			// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
			services.AddEndpointsApiExplorer();
			services.AddSwaggerGen();
		}

		/// <summary>
		/// Configure Serilog
		/// </summary>
		/// <param name="host">Host Builder</param>
		/// <param name="services">Services Collection</param>
		/// <param name="configuration">Configuration</param>
		/// <param name="connectionString">Connection string used for Serilog logging to DB if turned on</param>
		protected virtual void ConfigureSerilog(IHostBuilder host, IServiceCollection services, IConfiguration configuration, string connectionString)
		{
			bool logToDb = configuration.GetValue<bool>("LoggerConfig:LogIntoDb");
			bool logToSplunk = configuration.GetValue<bool>("LoggerConfig:LogIntoSplunk");
			bool logToFile = configuration.GetValue<bool>("LoggerConfig:LogIntoFile");
			LoggerConfiguration config = new LoggerConfiguration()
				.ReadFrom.Configuration(configuration, new ConfigurationReaderOptions(DependencyContext.Default) { SectionName = "LoggerConfig", FormatProvider = null })
				.Enrich.FromLogContext()
				.Enrich.WithMachineName()
				.Enrich.WithProcessId()
				.Enrich.WithThreadId()
				.Enrich.With<ApplicationNameEnricher>()
				.WriteTo.Console(
					outputTemplate: "{Level} {Timestamp:HH:mm:ss.fff} {CorrelationId} {UserNTID} {Message}{NewLine}"
				);

			if (logToFile)
			{
				config
					.WriteTo.Logger(l => l
						.Filter.ByIncludingOnly(l => l.Level == Serilog.Events.LogEventLevel.Verbose)
						.WriteTo.File(
							path: "logs/log-.csv",
							outputTemplate: "{Timestamp:HH:mm:ss.fff},{SourceContext},{CorrelationId},{UserNTID},{Message}{NewLine}",
							fileSizeLimitBytes: 20000000,
							//buffered: true,
							//flushToDiskInterval: TimeSpan.FromSeconds(60),
							rollingInterval: RollingInterval.Day,
							rollOnFileSizeLimit: true,
							retainedFileCountLimit: 20,
							retainedFileTimeLimit: TimeSpan.FromDays(31),
							hooks: new HeaderWriter("Timestamp,Source Context,Correlation Id,User NtId,Message")
						)
					);
			}

			if (logToDb)
			{
				ColumnOptions columnOptions = new()
				{
					DisableTriggers = true,
					ClusteredColumnstoreIndex = false,
				};
				columnOptions.Store.Remove(StandardColumn.MessageTemplate);
				columnOptions.Store.Add(StandardColumn.LogEvent);
				columnOptions.AdditionalColumns = new List<SqlColumn>{
					new SqlColumn("CorrelationId", System.Data.SqlDbType.VarChar, true, 40),
					new SqlColumn("UserNTID", System.Data.SqlDbType.VarChar, true, 50),
					new SqlColumn("MachineName", System.Data.SqlDbType.VarChar, true, 50),
					new SqlColumn("ApplicationName", System.Data.SqlDbType.VarChar, true, 50)
				};

				columnOptions.Properties.OmitElementIfEmpty = true;
				columnOptions.TimeStamp.ConvertToUtc = true;

				config.WriteTo.MSSqlServer(
					restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information,
					connectionString: connectionString,
					sinkOptions: new MSSqlServerSinkOptions() { SchemaName = "dbo", TableName = "_Logs", AutoCreateSqlTable = true },
					columnOptions: columnOptions);

				services.AddSerilogUi(options =>
				  options.UseSqlServer(connectionString, "_Logs"));
			}

			if (logToSplunk)
			{
				config.WriteTo.EventCollector(
					restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information,
					splunkHost: configuration["Splunk:Host"],
					uriPath: configuration["Splunk:Uri"],
					eventCollectorToken: configuration["Splunk:Token"],
					sourceType: configuration["Splunk:SourceType"],
					index: configuration["Splunk:Index"]);
			}

			Log.Logger = config.CreateLogger();
			host.UseSerilog(Log.Logger);
		}

		/// <summary>
		/// Configure Health Checks for the application
		/// </summary>
		/// <param name="services">Services which we are configuring</param>
		/// <param name="connectionString">Connection string used for health checks</param>
		protected virtual void ConfigureHealthChecks(IServiceCollection services, string connectionString)
		{
			IHealthChecksBuilder healthChecksBuilder = services.AddHealthChecks()
					.AddProcessAllocatedMemoryHealthCheck(1024, name: "Memory Allocation");

			if (connectionString is not null)
			{
				healthChecksBuilder.AddSqlServer(connectionString, name: "DB Connection");
			}
		}

		/// <summary>
		/// Configures CORS
		/// 
		/// Should be called from Startup.ConfigureServices
		/// </summary>
		/// <param name="services">Services which we are configuring</param>
		protected virtual void ConfigureCors(IServiceCollection services)
		{
			services.AddCors(options => options.AddPolicy("CorsPolicy", builder =>
					 builder.WithOrigins("https://*.lmco.com")
							 .AllowAnyMethod()
							 .AllowAnyHeader()
							 .AllowCredentials()));
		}

		/// <summary>
		/// Configures the App Builder
		/// </summary>
		/// <param name="app">App Builder</param>
		/// <param name="env">Web Host Environment</param>
		public void ConfigureAppBuilder(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (!env.IsProduction())
			{
				app.UseDeveloperExceptionPage();
			}

			app.ConfigureExceptionHandler();

			app.UseSwagger();
			app.UseSwaggerUI();

			app.UseForwardedHeaders(GetForwardedHeadersOptions());
			app.UseAuthentication();
			app.UseHttpsRedirection();
			app.UseStaticFiles();
			app.UseSerilogRequestLogging();
			app.UseRouting();
			app.UseCors("CorsPolicy");
			app.UseAuthorization();
			app.UseMiddleware<UserLoggingMiddleware>();
			app.UseMiddleware<CorrelationMiddleware>();

			if (env.IsDevelopment())
			{
				app.UseSerilogUi(options =>
				{
					options.Authorization.AuthenticationType = AuthenticationType.Windows;
				});
			}

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

		/// <summary>
		/// Helps w/ load balancers
		/// </summary>
		protected virtual ForwardedHeadersOptions GetForwardedHeadersOptions()
		{
			ForwardedHeadersOptions forwardedHeadersOptions = new()
			{
				ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
			};

			forwardedHeadersOptions.KnownNetworks.Clear();
			forwardedHeadersOptions.KnownProxies.Clear();

			return forwardedHeadersOptions;
		}
	}
}
