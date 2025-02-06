using static HtmlAsCode.Renderer;
using static JsonMasher.Web.Utils;

namespace JsonMasher.Web;

public static class Components
{
    public static Element Form(
        string program,
        string input,
        string output,
        string debugging,
        string exampleProgram,
        bool slurp
    )
    {
        return H(
            "form",
            A("method", "POST"),
            A("action", ""),
            A("hx-target", "this"),
            A("hx-swap", "outerHTML"),
            Class("grid grid-cols-2 grid-rows-2 gap-2 h-full w-full"),
            QuadrantWithAction(
                "Program:",
                H(
                    "div",
                    Class("flex gap-2 items-baseline"),
                    H(
                        "select",
                        A("id", "exampleProgram"),
                        A("hx-trigger", "change"),
                        A("hx-post", "/exampleProgramChanged"),
                        A("hx-params", "exampleProgram"),
                        Name("exampleProgram"),
                        Class("p-2 rounded-sm"),
                        Examples.ExampleNames.Select(name =>
                            H(
                                "option",
                                A("value", name),
                                name == exampleProgram ? A("selected") : null,
                                name
                            )
                        )
                    ),
                    H(
                        "label",
                        H(
                            "input",
                            Class("mr-2"),
                            Name("slurp"),
                            A("type", "checkbox"),
                            slurp ? A("checked") : null
                        ),
                        "Slurp"
                    ),
                    H("button", Class("border border-gray-200 py-2 px-4 rounded-sm"), "Run")
                ),
                TextArea("program", program)
            ),
            Quadrant("Output:", TextArea("", output)),
            Quadrant("Input:", TextArea("input", input)),
            Quadrant("Debugging:", TextArea("", debugging))
        );
    }

    public static Element Head()
    {
        return H(
            "head",
            H("title", "Title"),
            H(
                "link",
                A("rel", "stylesheet"),
                A("type", "text/css"),
                A("href", "/static/styles.css")
            ),
            H("script", A("src", "https://unpkg.com/htmx.org@2.0.4"))
        );
    }

    public static Element TextArea(string name, string content)
    {
        return H(
            "textarea",
            name != "" ? Name(name) : A("readonly"),
            Class("w-full h-full border border-gray-400 rounded-sm resize-none p-2"),
            content
        );
    }

    public static Element Quadrant(string title, Element content)
    {
        return H(
            "div",
            Class("bg-yellow-100 flex flex-col gap-2 rounded-sm"),
            H("h2", Class("text-xl p-2"), title),
            content
        );
    }

    public static Element QuadrantWithAction(string title, Element ui, Element content)
    {
        return H(
            "div",
            Class("bg-yellow-100 flex flex-col gap-2 rounded-sm"),
            H(
                "div",
                Class("flex gap-2 justify-between p-2 items-baseline"),
                H("div", Class("text-xl"), title),
                ui
            ),
            content
        );
    }

    public static Element Header()
    {
        return H(
            "header",
            Class("bg-blue-100 flex flex-col gap-2 p-2 rounded-sm"),
            H(
                "a",
                Href("https://jsonmasher.mbrezu.me"),
                H("h1", Class("text-3xl link"), "JSON Masher")
            ),
            H(
                "p",
                Link("https://github.com/mbrezu/JsonMasher"),
                " a .NET implementation of ",
                Link("https://stedolan.github.io/jq/", "jq")
            )
        );
    }

    public static Element Link(string href, string? text = null)
    {
        return H("a", Class("link"), Href(href), text ?? href);
    }
}
