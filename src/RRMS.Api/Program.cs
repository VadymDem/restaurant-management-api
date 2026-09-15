// ASP.NET Core Web API entry point.
//
// The pipeline is intentionally left as a minimal shell. TODO (in order):
//   1. services.AddControllers();
//   2. services.AddEndpointsApiExplorer() + services.AddSwaggerGen(...);  // Swagger with a Bearer security scheme
//   3. services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//               .AddJwtBearer(options => { ... });                        // JWT settings from the Jwt section
//   4. services.AddAuthorization();
//   5. services.AddApplication().AddInfrastructure(Configuration);
//   6. app.UseMiddleware<ExceptionHandlingMiddleware>();
//   7. app.UseSwagger(); app.UseSwaggerUI();
//   8. app.UseAuthentication(); app.UseAuthorization();
//   9. app.MapControllers();

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.Run();