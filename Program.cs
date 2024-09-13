using chattingApp.DataAndContext;
using chattingApp.DataAndContext.Models;
using chattingApp.Helpers;
using chattingApp.middlewares;
using chattingApp.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace chattingApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            // configuration to map the data from appsetings to class JWT
            builder.Services.Configure<JWT>(builder.Configuration.GetSection("JWT"));

            // configure to map data from appsettings to class Twilio
            builder.Services.Configure<TwilioSettings>(builder.Configuration.GetSection("Twilio"));

            // assign the connection string variable with it's data
            var connectionString = builder.Configuration.GetConnectionString("defaultConnection");
            // use this connection string to map between my class db context and my db in the sql server
            builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));

            // configure our identity(tells our system that which context is for the identity)
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // configure services 
            builder.Services.AddScoped<IAuthService, AuthService>();
            //builder.Services.AddScoped<JwtMiddleware>(); // Register the middleware service
            builder.Services.AddScoped<ITransferPhotosToPathWithStoreService, TransferPhotosToPathWithStoreService>();
            builder.Services.AddTransient<ISMSService, SMSService>();

            // configure JWT
            builder.Services.AddAuthentication(options => {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
               .AddJwtBearer(o =>
               {
                   o.RequireHttpsMetadata = false;
                   o.SaveToken = false;
                   o.TokenValidationParameters = new TokenValidationParameters
                   {
                       ValidateIssuerSigningKey = true,
                       ValidateIssuer = true,
                       ValidateAudience = true,
                       ValidateLifetime = true,
                       ValidIssuer = builder.Configuration["JWT:Issuer"],
                       ValidAudience = builder.Configuration["JWT:Audience"],
                       IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"]))
                   };
               });

            // to determine the time of lifespan of the OTP which we use for reset password or 2 factor authentication for ex
            builder.Services.Configure<DataProtectionTokenProviderOptions>(op =>
            {
                // otp valid for 1 hours
                op.TokenLifespan = TimeSpan.FromHours(1);
            });

            builder.Services.AddControllers();
            builder.Services.AddMemoryCache();

            // Add HttpContextAccessor service this is for make func getUserId...
            // AddHttpContextAccessor => and this to could us get the token from the header of the API
            builder.Services.AddHttpContextAccessor();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            // enable token bearer functionality in Swagger
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

                // Define the Bearer Authentication scheme
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "JWT Authorization header using the Bearer scheme."
                });

                // Make sure the bearer token is applied to all requests
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
                            new string[] {}
                        }
                    });
            });

            ///
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseMiddleware<JwtMiddleware>();


            app.MapControllers();

            app.Run();
        }
    }
}