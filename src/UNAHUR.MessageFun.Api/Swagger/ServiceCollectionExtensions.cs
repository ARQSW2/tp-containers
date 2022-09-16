using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System;
using System.IO;
using System.Reflection;

namespace UNAHUR.MessageFun.Api.Swagger
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Configura swagger gen 
        /// </summary>
        /// <param name="services"></param>
        /// <param name="appName"></param>
        /// <returns></returns>

        public static IServiceCollection AddSwaggerGenExtended(this IServiceCollection services, string appName)
        {
            services.AddSwaggerGen(c =>
            {
                //TODO: ESTO DEBERIA SER PARAMETRIZADO/ VERSIONAMOS LAS APIS?
                c.SwaggerDoc("v1", new OpenApiInfo { Version = "v1", Title = appName });

                // using System.Reflection;
                var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));

                // Predicado de inclusion a swagger
                c.DocInclusionPredicate((name, api) =>
                {
                    // excluye todo lo que empieza con odata
                    var include = !api.RelativePath.Contains("odata");
                    return include;

                });


                // definie el operation id default
                /*c.CustomOperationIds(c =>
                {

                    if (c.TryGetMethodInfo(out MethodInfo methodInfo))
                    {
                        return methodInfo.Name;
                    }
                    else
                    {
                        return null;
                    }


                });*/

                c.OperationFilter<StreamJsonContentFilter>();

                c.OperationFilter<XOperationNameOperationFilter>();


                // Assign scope requirements to operations based on AuthorizeAttribute
                c.OperationFilter<SecurityRequirementsOperationFilter>();

                // filtro para que el swagger genere bien los uploads de archivo
                c.OperationFilter<SingleFileOperationFilter>();

                // filtro para que el swagger genere bien los uploads de multiples archivos
                c.OperationFilter<MultiFileOperationFilter>();

                c.DescribeAllParametersInCamelCase();
            });

            return services;

        }
    }
}