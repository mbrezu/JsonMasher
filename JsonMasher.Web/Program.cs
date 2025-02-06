using System.Text;
using JsonMasher.Compiler;
using JsonMasher.JsonRepresentation;
using JsonMasher.Mashers;
using Microsoft.AspNetCore.Mvc;
using static HtmlAsCode.Renderer;
using static JsonMasher.Web.Components;
using static JsonMasher.Web.Utils;

namespace JsonMasher.Web;

public class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var app = builder.Build();

        Configuration.IsDev = app.Environment.IsDevelopment();

        _ = app.UseStaticFiles(
            new StaticFileOptions
            {
                OnPrepareResponse = ctx =>
                {
                    if (Configuration.IsDev)
                    {
                        // Disable caching for all static files
                        ctx.Context.Response.Headers.CacheControl =
                            "no-cache, no-store, must-revalidate";
                        ctx.Context.Response.Headers.Pragma = "no-cache";
                        ctx.Context.Response.Headers.Expires = "0";
                    }
                },
            }
        );

        app.MapGet(
            "/",
            (HttpContext context) =>
            {
                var html = H(
                    "html",
                    Class("h-full w-full"),
                    Head(),
                    H(
                        "body",
                        A("hx-boost", "true"),
                        Class("w-full h-full"),
                        H(
                            "main",
                            Class("flex flex-col gap-2 w-full h-full p-2"),
                            Header(),
                            Form(".", "[1, 2, 3]", "", "", "", false)
                        )
                    )
                );

                return CreateHtmlResult(context, html);
            }
        );

        app.MapPost(
                "/",
                (HttpContext httpContext, [FromForm] Payload payload) =>
                {
                    var output = "";
                    var debugging = "";
                    try
                    {
                        var (filter, sourceInformation) = new Parser().Parse(payload.Program);
                        IMashStack stack = new DebugMashStack();
                        var inputs = payload.Input.AsMultipleJson();
                        if (payload.IsSlurping == true)
                        {
                            inputs = Json.Array(inputs).AsEnumerable();
                        }
                        var (results, context) = new JsonMasher.Mashers.JsonMasher().Mash(
                            inputs,
                            filter,
                            stack,
                            sourceInformation,
                            100000
                        );
                        output = CollectStdOut(results);
                        debugging = CollectStdErr(context);
                    }
                    catch (Exception ex)
                    {
                        output = ex.ToString();
                    }
                    return CreateHtmlResult(
                        httpContext,
                        Form(
                            payload.Program,
                            payload.Input,
                            output,
                            debugging,
                            payload.ExampleProgram,
                            payload.IsSlurping
                        )
                    );
                }
            )
            .DisableAntiforgery();

        app.MapPost(
                "/exampleProgramChanged",
                (HttpContext httpContext, [FromForm] Payload payload) =>
                {
                    var example = Examples.GetExample(payload.ExampleProgram);
                    return CreateHtmlResult(
                        httpContext,
                        Form(
                            example?.Program ?? payload.Program,
                            example?.Input ?? payload.Input,
                            "",
                            "",
                            payload.ExampleProgram,
                            payload.IsSlurping
                        )
                    );
                }
            )
            .DisableAntiforgery();

        app.Run();

        string CollectStdOut(IEnumerable<Json> results)
        {
            var sb = new StringBuilder();
            foreach (var result in results)
            {
                sb.AppendLine(result.ToString());
            }
            return sb.ToString();
        }

        string CollectStdErr(IMashContext context)
        {
            var sb = new StringBuilder();
            foreach (var log in context.Log)
            {
                sb.AppendLine(log.ToString());
            }
            return sb.ToString();
        }
    }
}
