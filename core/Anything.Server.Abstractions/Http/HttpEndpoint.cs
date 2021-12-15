using System;

namespace Anything.Server.Abstractions.Http;

public record HttpEndpoint(string Pattern, string Method, Delegate RequestDelegate);
