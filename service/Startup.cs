using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Microsoft.Identity.Web;
using Soli.Repositories.Impl;
using Soli.Repositories;
using System.Data;
using System.Data.SqlClient;

namespace Soli
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApi(Configuration, "AzureAD");

            services.AddAuthorization(opts => {
                opts.AddPolicy("TerritoryAdmin", policy => policy.RequireClaim("soli_territoryadmin"));
                opts.AddPolicy("InviteUser", policy => policy.RequireClaim("soli_inviteuser"));
            });

            services.AddScoped<IDbConnection>(db =>
           {
               return new SqlConnection(Configuration.GetConnectionString("soli_db"));
           });

            services.AddTransient<UserClaimRepository, UserClaimRepositoryImpl>();
            services.AddTransient<TerritoriesRepository, TerritoriesRepositoryImpl>();
            services.AddTransient<InvitationRepository, InvitationRepositoryImpl>();

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Soli", Version = "v1" });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Soli v1"));
            }

            // if (!env.IsDevelopment())
            // {
            //     app.UseHttpsRedirection();
            // }

            app.UseCors(builder =>
            {
                builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
            });

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
