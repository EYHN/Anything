using System;
using System.Collections.Generic;
using Anything.Server.Abstractions.Http;
using Anything.Server.Api;
using GraphQL.Server.Ui.Playground;
using GraphQL.Server.Ui.Voyager;
using GraphQL.Types;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Anything.Server;

public static class Server
{
    public static WebApplication ConfigureWebApplication(Action<WebApplicationBuilder> configureAction)
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddGraphQlService();
        configureAction(builder);

        var app = builder.Build();
        app.UseGraphQL<ISchema>("/api/graphql");
        app.UseFileServer();

        app.UseGraphQLPlayground(new PlaygroundOptions { GraphQLEndPoint = "/api/graphql" }, "/graphql");
        app.UseGraphQLVoyager(new VoyagerOptions { GraphQLEndPoint = "/api/graphql" }, "/voyager");

        var httpEndpoints = app.Services.GetService<IEnumerable<HttpEndpoint>>();
        if (httpEndpoints != null)
        {
            foreach (var endpoint in httpEndpoints)
            {
                switch (endpoint.Method)
                {
                    case "GET":
                        app.MapGet(endpoint.Pattern, endpoint.RequestDelegate);
                        break;
                    case "POST":
                        app.MapPost(endpoint.Pattern, endpoint.RequestDelegate);
                        break;
                    case "DELETE":
                        app.MapDelete(endpoint.Pattern, endpoint.RequestDelegate);
                        break;
                    case "PUT":
                        app.MapPut(endpoint.Pattern, endpoint.RequestDelegate);
                        break;
                }
            }
        }

        return app;
    }
}
