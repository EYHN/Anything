using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Anything.Utils;
using GraphQL;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;

namespace Anything.Server.Abstractions.Graphql.Endpoint;

public abstract class GraphQlEndpoint
{
    public string Name { get; }

    public string? Description { get; }

    public Type GraphType { get; }

    public Func<object?, IResolveFieldContext, IServiceProvider, object?> Resolve { get; }

    public bool IsAsync { get; }

    public QueryArguments? Arguments { get; }

    internal GraphQlEndpoint(Type? hostType, string name, string? description, Type graphType, Delegate resolve, QueryArguments? arguments)
    {
        Name = name;
        Description = description;
        GraphType = graphType;
        Arguments = arguments;

        var targetParameter = Expression.Parameter(typeof(object), "target");
        var hostParameter = Expression.Parameter(typeof(object), "host");
        var contextParameter = Expression.Parameter(typeof(IResolveFieldContext), "context");
        var serviceProviderParameter = Expression.Parameter(typeof(IServiceProvider), "serviceProvider");

        var targetExpression = resolve.Target switch
        {
            not null => Expression.Convert(targetParameter, resolve.Target.GetType()),
            null => null,
        };

        var argumentsExpressions = new List<Expression>();
        var argumentsInfos = resolve.Method.GetParameters();

        // setup first argument as host expression
        if (hostType != null && argumentsInfos.Length >= 1 && argumentsInfos[0].ParameterType.IsAssignableFrom(hostType))
        {
            argumentsExpressions.Add(Expression.Convert(hostParameter, hostType));
        }

        // map graphql argument to arguments expressions
        var getArgumentMethodInfo = typeof(GraphQL.ResolveFieldContextExtensions).GetMethods().First(m =>
            m.Name == nameof(GraphQL.ResolveFieldContextExtensions.GetArgument) && m.GetGenericArguments().Length == 1);
        foreach (var (parameter, index) in argumentsInfos.Skip(argumentsExpressions.Count).Take(arguments?.Count ?? 0)
                     .Select((value, index) => (value, index)))
        {
            argumentsExpressions.Add(Expression.Call(
                null,
                getArgumentMethodInfo.MakeGenericMethod(parameter.ParameterType),
                contextParameter,
                Expression.Constant(arguments![index].Name, typeof(string)),
                Expression.Default(parameter.ParameterType)));
        }

        // setup other arguments as request service
        var getRequiredServiceInfo = typeof(ServiceProviderServiceExtensions).GetMethods().First(m =>
            m.Name == nameof(ServiceProviderServiceExtensions.GetRequiredService) &&
            m.GetGenericArguments().Length == 1 &&
            m.GetParameters().Length == 1);
        foreach (var parameter in argumentsInfos.Skip(argumentsExpressions.Count))
        {
            argumentsExpressions.Add(Expression.Call(
                null,
                getRequiredServiceInfo.MakeGenericMethod(parameter.ParameterType),
                serviceProviderParameter));
        }

        var sourceCall = Expression.Call(targetExpression, resolve.Method, argumentsExpressions);

        var distributionExpression = NormalizeReturnType(sourceCall);

        IsAsync = distributionExpression.Type == typeof(Task<object>);

        var distributionParameters = new[] { targetParameter, hostParameter, contextParameter, serviceProviderParameter };

        var distribution = Expression
            .Lambda<Func<object?, object?, IResolveFieldContext, IServiceProvider, object?>>(
                distributionExpression,
                distributionParameters)
            .Compile();

        Resolve = (host, context, serviceProvider) => distribution(resolve.Target, host, context, serviceProvider);
    }

    private Expression NormalizeReturnType(Expression expression)
    {
        if (expression.Type == typeof(void))
        {
            return Expression.Block(expression, Expression.Constant(null));
        }
        else if (AwaitableInfo.IsTypeAwaitable(expression.Type, out _))
        {
            // convect async type to Task<object?>
            if (expression.Type == typeof(ValueTask<object>))
            {
                return Expression.Call(expression, typeof(ValueTask<object>).GetMethod(nameof(ValueTask<object>.AsTask))!);
            }
            else if (expression.Type == typeof(Task<object>))
            {
                return expression;
            }
            else if (expression.Type == typeof(Task))
            {
                return Expression.Call(
                    typeof(GraphQlEndpoint).GetMethod(nameof(ExecuteVoidTask), BindingFlags.NonPublic | BindingFlags.Static)!,
                    expression);
            }
            else if (expression.Type == typeof(ValueTask))
            {
                return Expression.Call(
                    typeof(GraphQlEndpoint).GetMethod(nameof(ExecuteVoidValueTask), BindingFlags.NonPublic | BindingFlags.Static)!,
                    expression);
            }
            else if (expression.Type.IsGenericType &&
                     expression.Type.GetGenericTypeDefinition() == typeof(Task<>))
            {
                var typeArg = expression.Type.GetGenericArguments()[0];
                return Expression.Call(
                    typeof(GraphQlEndpoint).GetMethod(nameof(ExecuteTask), BindingFlags.NonPublic | BindingFlags.Static)!
                        .MakeGenericMethod(typeArg),
                    expression);
            }
            else if (expression.Type.IsGenericType &&
                     expression.Type.GetGenericTypeDefinition() == typeof(ValueTask))
            {
                var typeArg = expression.Type.GetGenericArguments()[0];
                return Expression.Call(
                    typeof(GraphQlEndpoint).GetMethod(nameof(ExecuteValueTask), BindingFlags.NonPublic | BindingFlags.Static)!
                        .MakeGenericMethod(typeArg),
                    expression);
            }
            else
            {
                throw new NotSupportedException($"Unsupported expression type: {expression.Type}");
            }
        }
        else if (expression.Type.IsValueType)
        {
            return Expression.TypeAs(expression, typeof(object));
        }
        else
        {
            return expression;
        }
    }

    private static async Task<object?> ExecuteVoidTask(Task task)
    {
        await task;
        return null;
    }

    private static async Task<object?> ExecuteTask<T>(Task<T> task)
    {
        return await task;
    }

    private static async Task<object?> ExecuteVoidValueTask(ValueTask task)
    {
        await task;
        return null;
    }

    private static async Task<object?> ExecuteValueTask<T>(ValueTask<T> task)
    {
        return await task;
    }
}
