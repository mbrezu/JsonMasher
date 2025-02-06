using static HtmlAsCode.Renderer;
using HtmlAttribute = HtmlAsCode.Renderer.Attribute;

namespace JsonMasher.Web;

public static class Utils
{
    public static HtmlAttribute Href(string href)
    {
        return A("href", href);
    }

    public static HtmlAttribute Class(string classNames)
    {
        return A("class", classNames);
    }

    public static HtmlAttribute Name(string name)
    {
        return A("name", name);
    }

    public static IResult CreateHtmlResult(HttpContext context, Element html)
    {
        var htmlString = html.Render(); // Configuration.IsDev ? html.RenderPretty() : html.Render();
        var result = Results.Content(htmlString, "text/html");
        context.Response.Headers.CacheControl = "no-cache, no-store, must-revalidate";
        context.Response.Headers.Pragma = "no-cache";
        context.Response.Headers.Expires = "0";
        return result;
    }
}
