using FileScannerAPI.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.ApplicationInsights.NLogTarget;
using NLog.Config;
using NLog;
using NLog.Targets.Wrappers;

namespace FileScannerAPI
{
    public class Startup
    {
        private LoggingSettings _loggingSettings;
        private ApplicationInsightsSettings _applicationInsightsSettings;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddApplicationInsightsTelemetry();

            services.AddControllers();

            ConfigureSettings(services);

            ConfigureDI(services);

            ConfigureAppInsights(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private void ConfigureSettings(IServiceCollection services)
        {
            services.Configure<FileScanSettings>(options => Configuration.GetSection("FileScanSettings").Bind(options));
            services.Configure<LoggingSettings>(options => Configuration.GetSection("LoggingSettings").Bind(options));
            services.Configure<ApplicationInsightsSettings>(options => Configuration.GetSection("ApplicationInsightsSettings").Bind(options));
        }

        private void ConfigureDI(IServiceCollection services)
        {
            services.AddScoped<IFileHandler, FileHandler>();
            services.AddScoped<ExceptionFilter>();
        }

        private void ConfigureAppInsights(IServiceCollection services)
        {
            _loggingSettings = new LoggingSettings();
            Configuration.GetSection("LoggingSettings").Bind(_loggingSettings);

            _applicationInsightsSettings = new ApplicationInsightsSettings();
            Configuration.GetSection("ApplicationInsightsSettings").Bind(_applicationInsightsSettings);

            var fileScannerTarget = new ApplicationInsightsTarget
            {
                InstrumentationKey = Configuration["ApplicationInsightsSettings:InstrumentationKey"],
                Name = "FileScannerApi",
                Layout = Configuration["ApplicationInsightsSettings:Layout"]
            };
            
            var asyncWrapper = new AsyncTargetWrapper(fileScannerTarget, _applicationInsightsSettings.MaximumQueueLimit, AsyncTargetWrapperOverflowAction.Discard);
            var fileScannerRule = new LoggingRule("*", NLog.LogLevel.FromString(_loggingSettings.LogLevel.Default.ToString()), fileScannerTarget);

            LogManager.Configuration.AddTarget("FileScannerApi", asyncWrapper);
            LogManager.Configuration.LoggingRules.Add(fileScannerRule);
            LogManager.KeepVariablesOnReload = true;
            LogManager.ReconfigExistingLoggers();
        }
    }
}