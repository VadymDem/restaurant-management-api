using Microsoft.Extensions.DependencyInjection;

namespace RRMS.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services)
        => throw new NotImplementedException(
            "TODO: AddControllers, AddEndpointsApiExplorer, AddSwaggerGen (with Bearer security scheme), " +
            "CORS policy for the frontend origin, and JWT bearer authentication options (issuer, audience, signing key from the Jwt section).");
}