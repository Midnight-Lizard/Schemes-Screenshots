using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MidnightLizard.Schemes.Screenshots.Configuration;
using MidnightLizard.Schemes.Screenshots.Services;
using Newtonsoft.Json;

namespace MidnightLizard.Schemes.Screenshots
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            services.Configure<BrowserConfig>(this.Configuration);
            services.Configure<ExtensionConfig>(this.Configuration);
            services.Configure<ScreenshotsConfig>(this.Configuration);
            
            services.AddSingleton<KafkaConfig>(x => new KafkaConfig
            {
                KAFKA_EVENTS_CONSUMER_CONFIG = JsonConvert
                    .DeserializeObject<Dictionary<string, object>>(
                        Configuration.GetValue<string>(
                            nameof(KafkaConfig.KAFKA_EVENTS_CONSUMER_CONFIG))),

                SCHEMES_EVENTS_TOPIC = Configuration.GetValue<string>(
                    nameof(KafkaConfig.SCHEMES_EVENTS_TOPIC))
            });

            services.AddTransient<IExtensionManager, ExtensionManager>();
            services.AddTransient<IScreenshotGenerator, ScreenshotGenerator>();
            services.AddTransient<IScreenshotUploader, ScreenshotUploader>();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IOptions<ScreenshotsConfig> options)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseStaticFiles();
            }
            app.UseMvc();
        }
    }
}
