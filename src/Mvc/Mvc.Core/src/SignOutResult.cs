// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Mvc;

/// <summary>
/// An <see cref="ActionResult"/> that on execution invokes <see cref="M:HttpContext.SignOutAsync"/>.
/// </summary>
public class SignOutResult : ActionResult, IResult
{
    /// <summary>
    /// Initializes a new instance of <see cref="SignOutResult"/> with the default sign out scheme.
    /// </summary>
    public SignOutResult()
        : this(Array.Empty<string>())
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="SignOutResult"/> with the default sign out scheme.
    /// specified authentication scheme and <paramref name="properties"/>.
    /// </summary>
    /// <param name="properties"><see cref="AuthenticationProperties"/> used to perform the sign-out operation.</param>
    public SignOutResult(AuthenticationProperties properties)
        : this(Array.Empty<string>(), properties)
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="SignOutResult"/> with the
    /// specified authentication scheme.
    /// </summary>
    /// <param name="authenticationScheme">The authentication scheme to use when signing out the user.</param>
    public SignOutResult(string authenticationScheme)
        : this(new[] { authenticationScheme })
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="SignOutResult"/> with the
    /// specified authentication schemes.
    /// </summary>
    /// <param name="authenticationSchemes">The authentication schemes to use when signing out the user.</param>
    public SignOutResult(IList<string> authenticationSchemes)
        : this(authenticationSchemes, properties: null)
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="SignOutResult"/> with the
    /// specified authentication scheme and <paramref name="properties"/>.
    /// </summary>
    /// <param name="authenticationScheme">The authentication schemes to use when signing out the user.</param>
    /// <param name="properties"><see cref="AuthenticationProperties"/> used to perform the sign-out operation.</param>
    public SignOutResult(string authenticationScheme, AuthenticationProperties? properties)
        : this(new[] { authenticationScheme }, properties)
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="SignOutResult"/> with the
    /// specified authentication schemes and <paramref name="properties"/>.
    /// </summary>
    /// <param name="authenticationSchemes">The authentication scheme to use when signing out the user.</param>
    /// <param name="properties"><see cref="AuthenticationProperties"/> used to perform the sign-out operation.</param>
    public SignOutResult(IList<string> authenticationSchemes, AuthenticationProperties? properties)
    {
        AuthenticationSchemes = authenticationSchemes ?? throw new ArgumentNullException(nameof(authenticationSchemes));
        Properties = properties;
    }

    /// <summary>
    /// Gets or sets the authentication schemes that are challenged.
    /// </summary>
    public IList<string> AuthenticationSchemes { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="AuthenticationProperties"/> used to perform the sign-out operation.
    /// </summary>
    public AuthenticationProperties? Properties { get; set; }

    /// <inheritdoc />
    public override Task ExecuteResultAsync(ActionContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        return ExecuteAsync(context.HttpContext);
    }

    /// <inheritdoc />
    Task IResult.ExecuteAsync(HttpContext httpContext)
    {
        return ExecuteAsync(httpContext);
    }

    private async Task ExecuteAsync(HttpContext httpContext)
    {
        if (AuthenticationSchemes == null)
        {
            throw new InvalidOperationException(
                Resources.FormatPropertyOfTypeCannotBeNull(
                    /* property: */ nameof(AuthenticationSchemes),
                    /* type: */ nameof(SignOutResult)));
        }

        var loggerFactory = httpContext.RequestServices.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger<SignOutResult>();

        logger.SignOutResultExecuting(AuthenticationSchemes);

        if (AuthenticationSchemes.Count == 0)
        {
            await httpContext.SignOutAsync(Properties);
        }
        else
        {
            for (var i = 0; i < AuthenticationSchemes.Count; i++)
            {
                await httpContext.SignOutAsync(AuthenticationSchemes[i], Properties);
            }
        }
    }
}
