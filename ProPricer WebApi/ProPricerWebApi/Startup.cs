// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2021 - 2022 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace APTSPropricerApi
{
	using ACV.Shared;
	using ACV.Shared.Models;
	using APTSPropricerApi.Connection;
	using Microsoft.AspNetCore.Builder;
	using Microsoft.AspNetCore.Hosting;
	using Microsoft.AspNetCore.Http;
	using Microsoft.Extensions.DependencyInjection;
	using Microsoft.Extensions.Hosting;
	using Microsoft.OpenApi.Models;
	using System;
	using System.Security.Principal;


	/// <summary>
	/// Startup class
	/// </summary>
	public class Startup
	{
		/// <summary>
		/// Configuration Service
		/// </summary>
		private readonly ConfigurationServiceProPricer configurationService;

		/// <summary>
		/// Ctor
		/// </summary>
		public Startup(IWebHostEnvironment env)
		{
			this.configurationService = new();
			ConfigurationServiceBase.BuildConfiguration<Startup>(env);
		}

		/// <summary>
		/// This method gets called by the runtime.
		/// Use this method to add services to the container.
		/// </summary>
		/// <param name="services">Collection of services</param>
		public void ConfigureServices(IServiceCollection services)
		{
			configurationService.ConfigureBasics(services);

			services.AddHttpContextAccessor();
			services.AddTransient<IPrincipal>(
				provider => provider.GetService<IHttpContextAccessor>().HttpContext.User);

			services.AddSingleton<PoolManagerList>();

			configurationService.AddMultiAuthentication(services);

			services.AddControllers(options =>
			{
				options.Filters.Add<HttpResponseExceptionFilter>();
			}).AddNewtonsoftJson(x => x.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

			services.AddSwaggerGen(opt =>
			{
				opt.SwaggerDoc("v1", new OpenApiInfo { Title = "ProPricer API", Version = $"v{ConfigurationServiceBase.Configuration["APPLICATION_VERSION"]}" });

				// Add using bearer token with Swagger
				OpenApiSecurityScheme securityScheme = new()
				{
					Name = "IES_Authorization",
					Description = "Enter Bearer before JWT Bearer token",
					In = ParameterLocation.Header,
					Type = SecuritySchemeType.ApiKey,
					Scheme = Constants.IES_TOKEN_SCHEME,
					BearerFormat = "JWT",
					Reference = new OpenApiReference
					{
						Id = Constants.IES_TOKEN_SCHEME,
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
		/// This method gets called by the runtime.
		/// Use this method to configure the HTTP request pipeline.
		/// </summary>
		/// <param name="app">Application builder</param>
		/// <param name="env">Web host environment</param>
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (!env.IsProduction())
			{
				app.UseDeveloperExceptionPage();
				app.UseSwagger();
				app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", $"ProPricer API v{ConfigurationServiceBase.Configuration["AppVersion"]}"));
			}

			this.configurationService.ConfigureAppBuilder(app);

			ClearOutOldFiles();
		}

		/// <summary>
		/// Clears out old xlsx files
		/// </summary>
		private void ClearOutOldFiles()
		{
			// Clear out old temporary files
			try
			{
				if (Directory.Exists(Constants.TEMP_DIRECTORY))
				{
					string[] files = Directory.GetFiles(Constants.TEMP_DIRECTORY, "*.xlsx");
					if (files != null && files.Length > 0)
					{
						foreach (string file in files)
						{
							File.Delete(file);
						}
					}
				}
				else
				{
					Directory.CreateDirectory(Constants.TEMP_DIRECTORY);
				}
			}
			catch (Exception ex)
			{
				// Let's not exit out of the application if we can't delete old temp files
				System.Diagnostics.Debug.WriteLine(ex.ToString());
			}
		}
	}
}
