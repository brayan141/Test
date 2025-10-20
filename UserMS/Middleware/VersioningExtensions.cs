using System.Diagnostics.CodeAnalysis;
using Asp.Versioning;

namespace UserMS.Middleware
{
    /// <summary>
    ///     Versioning Extensions
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class VersioningExtensions
    {
        /// <summary>
        ///     Add Versioning
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddVersioning(
            this IServiceCollection services)
        {
            services.AddApiVersioning(o =>
            {
                o.DefaultApiVersion = new ApiVersion(1, 0);
                o.AssumeDefaultVersionWhenUnspecified = true;
                o.ReportApiVersions = true;
                o.ApiVersionReader = new HeaderApiVersionReader();
            }).AddApiExplorer(options =>
            {
                //semantic versioning
                //first character is the principal or greater version
                //second character is the minor version
                //third character is the patch
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });
            return services;
        }
    }
}