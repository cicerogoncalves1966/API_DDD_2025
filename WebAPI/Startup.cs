using Aplicacao.Aplicacoes;
using Aplicacao.Interfaces;
using Dominio.Interfaces;
using Dominio.Interfaces.Genericos;
using Dominio.Interfaces.InterfaceServicos;
using Dominio.Servicos;
using Entidades.Entidades;
using Infraestrutura.Configuracoes;
using Infraestrutura.Repositorio;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
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
            services.AddControllers();
            services.AddProblemDetails();
            services.AddEndpointsApiExplorer();
            // Configurações de CORS
            services.AddCors(options =>
            {
                // Política restrita para Produção
                options.AddPolicy("ProductionCorsPolicy", policy =>
                {
                    policy.WithOrigins("https://meusiteoficial.com")
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });

                // Política totalmente aberta para Desenvolvimento
                options.AddPolicy("DevelopmentCorsPolicy", policy =>
                {
                    policy.WithOrigins("http://localhost:4200",
                                       "http://www.contoso.com",
                                       "http://localhost:9425",
                                       "https://tools.ietf.org") // URL padrão do Angular
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

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

            // *** CONFIGURAÇÃO .NET 8+ PARA ADDIDENTITY ***********************
            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                // Impede o Identity de criar os cookies padrão de redirecionamento MVC
                options.SignIn.RequireConfirmedAccount = false;
            })
            .AddEntityFrameworkStores<Contexto>()
            .AddDefaultTokenProviders();

            // INTERFACE E REPOSITORIO
            services.AddScoped(typeof(IGenericos<>), typeof(Infraestrutura.Repositorio.Genericos.RepositorioGenerico<>));
            services.AddScoped<INoticia, RepositorioNoticia>();
            services.AddScoped<IUsuario, RepositorioUsuario>();

            // SERVIÇO DOMINIO
            services.AddScoped<IServicoNoticia, ServicoNoticia>();

            // INTERFACE APLICAÇÃO
            services.AddScoped<IAplicacaoNoticia, AplicacaoNoticia>();
            services.AddScoped<IAplicacaoUsuario, AplicacaoUsuario>();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidateActor = true,
                    ValidIssuer = "Teste.Securiry.Bearer",
                    ValidAudience = "Teste.Securiry.Bearer",
                    IssuerSigningKey = JwtSecurityKey.Create("Secret_Key-123456789012345678901")
                };

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        Console.WriteLine("OnAuthenticationFailed: " + context.Exception.Message);
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        Console.WriteLine("OnTokenValidated: " + context.SecurityToken);
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        // Evita o comportamento padrão do Identity de tentar redirecionar para /Account/Login
                        context.HandleResponse();
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json";
                        return Task.CompletedTask;
                    }
                };
            });

            //services.AddControllers();
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new() { Title = "API Notícias .NET 8", Version = "v1" });
                // Define o esquema de segurança (Ex: JWT Bearer) para a interface do Swagger
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Insira o token JWT desta forma: Bearer seu_token_aqui"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseExceptionHandler();
            app.UseRouting();

            if (env.IsDevelopment())
            {
                // Ativa a política permissiva em modo Development
                app.UseCors("DevelopmentCorsPolicy");

                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "API Notícias .NET 8 v1"));
            }
            else
            {
                // Ativa a política segura em modo Production
                app.UseCors("ProductionCorsPolicy");
            }

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
