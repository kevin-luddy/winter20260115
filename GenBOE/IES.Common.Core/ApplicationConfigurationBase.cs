namespace IES.Common.Core
{
	using System;
	using System.Diagnostics;
	using System.Security.Principal;
	using HealthChecks.UI.Client;
	using IES.Common.Core.Authorization;
	using IES.Common.Core.Constants;
	using IES.Common.Core.Exceptions;
	using IES.Common.Core.Logging;
	using IES.Common.Core.Security;
	using IES.Common.Core.Utilities;
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
	using Microsoft.Extensions.Logging;
	using Microsoft.OpenApi.Models;
	using Newtonsoft.Json.Serialization;
	using Serilog;
	using Serilog.Settings.Configuration;
	using Serilog.Sinks.MSSqlServer;
	using Serilog.Ui.Core.Extensions;
	using Serilog.Ui.MsSqlServerProvider;
	using Serilog.Ui.MsSqlServerProvider.Extensions;
	using Serilog.Ui.Web;
	using Serilog.Ui.Web.Extensions;

	/// <summary>
	/// Application Configuration Base class
	/// </summary>
	public class ApplicationConfigurationBase
	{
		/// <summary>
		/// Configuration
		/// </summary>
		public static IConfiguration Configuration { get; set; }

		/// <summary>
		/// Adds Authentication to the site
		/// </summary>
		public void AddWindowsAuthentication(IServiceCollection services, IConfiguration configuration)
		{
			services.AddAuthentication(options =>
			{
				options.DefaultScheme = NegotiateDefaults.AuthenticationScheme;
				options.DefaultChallengeScheme = NegotiateDefaults.AuthenticationScheme;
			})
				.AddNegotiate();
			
			services.AddAuthorization(options =>
			{
				options.AddPolicy("OnlyNegotiate", policy =>
				{
					policy.AuthenticationSchemes.Add(NegotiateDefaults.AuthenticationScheme);
					policy.RequireAuthenticatedUser();
					string allowedRoles = configuration["AllowedRoles"] ?? string.Empty;
					policy.Requirements.Add(new GroupsCheckRequirement(allowedRoles));
				});

			});

			services.AddScoped<IAuthorizationHandler, GroupsCheckHandler>();
		}

		/// <summary>
		/// Adds Authentication to the site
		/// </summary>
		public void AddWindowsAndTokenAuthentication(IServiceCollection services, IConfiguration configuration)
		{
			services.AddAuthentication(options =>
			{
				options.DefaultScheme = NegotiateDefaults.AuthenticationScheme;
				options.DefaultChallengeScheme = NegotiateDefaults.AuthenticationScheme;
			})
				.AddNegotiate()
				.AddScheme<TokenAuthenticationOptions, TokenAuthenticationSchemeHandler>(
					CommonConstants.IES_TOKEN_SCHEME,
					opts => { }
				);

			services.AddAuthorization(options =>
			{
				// this lets us put [Authorize(Policy = "OnlyIesToken")] onto Controller
				// or just [Authorize(AuthenticationSchemes = CommonConstants.IES_TOKEN_SCHEME)]
				AuthorizationPolicyBuilder onlyIesTokenSchemePolicyBuilder = new(CommonConstants.IES_TOKEN_SCHEME);
				options.AddPolicy("OnlyIesToken", onlyIesTokenSchemePolicyBuilder
					.RequireAuthenticatedUser()
					.Build());

				// this lets us put [Authorize(Policy = "OnlyNegotiate")] onto Controller
				// or just [Authorize]
				options.AddPolicy("OnlyNegotiate", policy =>
				{
					policy.AuthenticationSchemes.Add(NegotiateDefaults.AuthenticationScheme);
					policy.RequireAuthenticatedUser();
					string allowedRoles = configuration["AllowedRoles"] ?? string.Empty;
					policy.Requirements.Add(new GroupsCheckRequirement(allowedRoles));
				});
			});

			services.AddScoped<IAuthorizationHandler, GroupsCheckHandler>();
		}

		/// <summary>
		/// Configure Basics for an application
		/// </summary>
		/// <param name="host">Host Builder</param>
		/// <param name="services">Services Collection</param>
		/// <param name="configuration">Configuration</param>
		/// <param name="connectionString">Connection string used for Serilog logging to DB if turned on</param>
		public void ConfigureBasics<T>(WebApplicationBuilder builder, string connectionString) where T : class
		{
			builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

			if (Directory.Exists("/etc/config-volume"))
			{
				builder.Configuration.AddJsonFile("/etc/config-volume/settings", false, true);
			}
			else
			{
				builder.Configuration.AddUserSecrets<T>();
			}
			
			builder.Services.AddMemoryCache();
			builder.Services.AddHttpContextAccessor();
			builder.Services.AddHttpClient();
			builder.Services.AddTransient<IPrincipal>(
				provider => provider.GetService<IHttpContextAccessor>()?.HttpContext?.User);

			ConfigureSerilog(builder.Host, builder.Services, builder.Configuration, builder.Configuration.GetConnectionString(connectionString));
			builder.Services.AddSingleton(Log.Logger);
			ConfigureHealthChecks(builder.Services, connectionString);
			ConfigureCors(builder.Services, builder.Configuration);

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
			builder.Services.AddSwaggerGen(opt =>
			{
				// Add using bearer token with Swagger
				OpenApiSecurityScheme securityScheme = new()
				{
					Name = "JWT Authentication",
					Description = "Enter JWT Bearer token **_only_**",
					In = ParameterLocation.Header,
					Type = SecuritySchemeType.Http,
					Scheme = "bearer", // must be lower case
					BearerFormat = "JWT",
					Reference = new OpenApiReference
					{
						Id = "Bearer", // JwtBearerDefaults.AuthenticationScheme
						Type = ReferenceType.SecurityScheme
					}
				};

				opt.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);
				opt.AddSecurityRequirement(new OpenApiSecurityRequirement
				{
					{ securityScheme, Array.Empty<string>() }
				});
			});
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
				  options.UseSqlServer(opts => opts
					  .WithConnectionString(connectionString)
					  .WithTable("_Logs"))
					.AddScopedAsyncAuthFilter<CustomAuthorizeFilter>()
				  ); 
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
		protected virtual void ConfigureCors(IServiceCollection services, IConfiguration configuration)
		{
			string allowedOrigins = configuration["AllowedOrigins"];
			if (string.IsNullOrWhiteSpace(allowedOrigins))
			{
				allowedOrigins = "https://*.lmco.com";
			}

			string[] origins = allowedOrigins.Split(",");
			services.AddCors(options => options.AddPolicy("CorsPolicy", builder =>
					 builder.WithOrigins(origins)
							 .AllowAnyMethod()
							 .AllowAnyHeader()
							 .AllowCredentials().WithExposedHeaders("content-disposition")));
		}

		/// <summary>
		/// Configures the App Builder
		/// </summary>
		/// <param name="builder">Web app builder</param>
		public WebApplication ConfigureAppBuilder(WebApplicationBuilder builder, bool isIesPortal = false)
		{
			WebApplication app = builder.Build();
			Configuration = app.Configuration;

			if (!app.Environment.IsProduction())
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

			Microsoft.Extensions.Logging.ILogger logger = app.Services.GetService<ILogger<ApplicationConfigurationBase>>(); 
			SafeUriUtility.Initialize(logger);

			if (isIesPortal)
			{
				app.UseSerilogUi(options =>
				{
					options.WithAuthenticationType(Serilog.Ui.Web.Models.AuthenticationType.Jwt);
					options.HideSerilogUiBrand();					
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

			Serilog.Debugging.SelfLog.Enable(msg =>
			{
				Debug.Print(msg);
				// Debugger.Break();  // used for debugging issues with serilog
			});

			IHostApplicationLifetime lifetime = app.Lifetime;
			lifetime.ApplicationStopped.Register(() => Log.CloseAndFlush());

			return app;
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
