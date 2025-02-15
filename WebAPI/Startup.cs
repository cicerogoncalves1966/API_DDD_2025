﻿using Aplicacao.Aplicacoes;
using Aplicacao.Interfaces;
using Dominio.Interfaces;
using Dominio.Interfaces.Genericos;
using Dominio.Interfaces.InterfaceServicos;
using Dominio.Servicos;
using Entidades.Entidades;
using Infraestrutura.Configuracoes;
using Infraestrutura.Repositorio;
using Infraestrutura.Repositorio.Genericos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using WebAPI.Token;

namespace WebAPI
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

            // Configurações de CORS
            services.AddCors();

            // *** CONFIGURAÇÃO PARA RODAR COM SQL-SERVER *********************
            //services.AddDbContext<Contexto>(options =>
            // options.UseSqlServer(
            //     Configuration.GetConnectionString("DefaultConnection")));

            // *** CONFIGURACAO ORIGINAL ADDDEFAULTIDENTITY *******************
            //services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
            //    .AddEntityFrameworkStores<Contexto>();

            // *** CONFIGURAÇÃO PARA BANCO DE DADOS POSTGRE-SQL ***************
            services.AddDbContext<Contexto>(options =>
             options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection")));

            // *** CONFIGURAÇÃO .NET 8 PARA ADDIDENTITY ***********************
            services.AddIdentity<ApplicationUser, IdentityRole>()
                    .AddEntityFrameworkStores<Contexto>()
                    .AddDefaultTokenProviders();

            // INTERFACE E REPOSITORIO
            services.AddSingleton(typeof(IGenericos<>), typeof(RepositorioGenerico<>));
            services.AddSingleton<INoticia, RepositorioNoticia>();
            services.AddSingleton<IUsuario, RepositorioUsuario>();

            // SERVIÇO DOMINIO
            services.AddSingleton<IServicoNoticia, ServicoNoticia>();

            // INTERFACE APLICAÇÃO
            services.AddSingleton<IAplicacaoNoticia, AplicacaoNoticia>();
            services.AddSingleton<IAplicacaoUsuario, AplicacaoUsuario>();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(option =>
                    {
                        option.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuer = false,
                            ValidateAudience = false,
                            ValidateLifetime = true,
                            ValidateIssuerSigningKey = true,

                            ValidIssuer = "Teste.Securiry.Bearer",
                            ValidAudience = "Teste.Securiry.Bearer",
                            IssuerSigningKey = JwtSecurityKey.Create("Secret_Key-12345678")
                        };

                        option.Events = new JwtBearerEvents
                        {
                            OnAuthenticationFailed = context =>
                            {
                                Console.WriteLine("OnAuthenticationFailed: " + context.Exception.Message);
                                return Task.CompletedTask;
                            },
                            OnTokenValidated = context =>
                            {
                                Console.WriteLine("OnTokenValidated: " + context.SecurityToken);
                                return Task.CompletedTask;
                            }
                        };
                    });

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "WebAPI", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Configurações de CORS - Lista de URLs com permissão de acesso
            var listURL1 = "https://www.mestresdaweb.com.br/";
            var listURL2 = "https://www.youtube.com/";
            app.UseCors(c => c.WithOrigins(listURL1, listURL2));

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebAPI v1"));
            }

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
