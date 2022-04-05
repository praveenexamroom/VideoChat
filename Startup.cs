using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System;
using VideoChat.Abstractions;
using VideoChat.Hubs;
using VideoChat.Options;
using VideoChat.Services;

namespace VideoChat
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.Configure<TwilioSettings>(Configuration.GetSection("TwilioSettings"));
            services.Configure<TokboxSettings>(Configuration.GetSection("TokboxSettings"));
            services.AddTransient<IVideoService, VideoService>();

            services.AddSwaggerGen(s =>
            {
                s.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "ExamRoom VideoChat API",
                    Contact = new OpenApiContact
                    {
                        Name = "ExamRoom.AI",
                        Url = new Uri("https://examroom.ai/")
                    }
                });
            });

            services.AddCors(c =>
            {
                c.AddPolicy("MyPolicy", options => options
                   .SetIsOriginAllowed(origin => true)
                   .AllowAnyMethod()
                   .AllowAnyHeader()
                   .AllowCredentials());
            });

            services.AddSignalR();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "StandardSetting API V1");
                c.RoutePrefix = string.Empty;
            });

            app.UseRouting();

            app.UseCors("MyPolicy");

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<NotificationHub>("notificationHub");
            });
        }
    }
}
