using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
//using WebApi.Authorization;
//using WebApi.Entities;
using WebApi.Helpers;
//using WebApi.Services;
using WebApplication1.Authorization;
using WebApplication1.Entities;
using WebApplication1.Services;

var builder = WebApplication.CreateBuilder(args);

// add services to DI container
{

    builder.Services.AddDbContext<UserDbContext>(opt => opt.UseSqlServer(builder.Configuration.GetConnectionString("Db")));
    var services = builder.Services;
    var env = builder.Environment;

    //services.AddDbContext<DataContext>();
    services.AddCors();
    services.AddControllers()
        .AddJsonOptions(x => x.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull);

    // configure strongly typed settings object
    services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));

    // configure DI for application services
    services.AddScoped<IJwtUtils, JwtUtils>();
    services.AddScoped<IUserService, UserService>();
}


var app = builder.Build();

// Configure the HTTP request pipeline.

{
    // global cors policy
    app.UseCors(x => x
        .SetIsOriginAllowed(origin => true)
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials());

    // global error handler
    app.UseMiddleware<ErrorHandlerMiddleware>();

    // custom jwt auth middleware
    app.UseMiddleware<JwtMiddleware>();

    app.MapControllers();
}

app.Run();
