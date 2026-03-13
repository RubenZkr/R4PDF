using System.Text.Json;
using R4PDF.Parsing;

namespace R4PDF.Tests;

public class TemplateParserTests
{
    [Fact]
    public void Parse_ValidTemplate_ReturnsPdfTemplate()
    {
        var json = """
        {
            "settings": { "pageSize": "A4", "orientation": "Portrait" },
            "pages": [{
                "body": {
                    "elements": [
                        { "type": "text", "text": "Hello World" }
                    ]
                }
            }]
        }
        """;

        var template = TemplateParser.Parse(json);

        Assert.NotNull(template);
        Assert.Single(template.Pages);
        Assert.Equal("A4", template.Settings.PageSize);
        Assert.Single(template.Pages[0].Body.Elements);
    }

    [Fact]
    public void Parse_EmptyPages_ThrowsJsonException()
    {
        var json = """
        {
            "settings": { "pageSize": "A4" },
            "pages": []
        }
        """;

        Assert.Throws<JsonException>(() => TemplateParser.Parse(json));
    }

    [Fact]
    public void Parse_NullOrEmpty_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => TemplateParser.Parse(""));
        Assert.Throws<ArgumentNullException>(() => TemplateParser.Parse(null!));
    }

    [Fact]
    public void Parse_WithStyles_ParsesStylesDictionary()
    {
        var json = """
        {
            "styles": {
                "title": { "fontSize": 20, "fontWeight": "bold", "color": "#003366" }
            },
            "pages": [{
                "body": {
                    "elements": [
                        { "type": "text", "text": "Styled", "style": "title" }
                    ]
                }
            }]
        }
        """;

        var template = TemplateParser.Parse(json);

        Assert.Single(template.Styles);
        Assert.True(template.Styles.ContainsKey("title"));
        Assert.Equal(20, template.Styles["title"].FontSize);
    }

    [Fact]
    public void Parse_WithMetadata_ParsesMetadata()
    {
        var json = """
        {
            "metadata": {
                "title": "My Document",
                "author": "Test Author"
            },
            "pages": [{
                "body": {
                    "elements": [
                        { "type": "text", "text": "Test" }
                    ]
                }
            }]
        }
        """;

        var template = TemplateParser.Parse(json);

        Assert.NotNull(template.Metadata);
        Assert.Equal("My Document", template.Metadata!.Title);
        Assert.Equal("Test Author", template.Metadata.Author);
    }

    [Fact]
    public void Parse_MultipleElementTypes_ParsesPolymorphically()
    {
        var json = """
        {
            "pages": [{
                "body": {
                    "elements": [
                        { "type": "text", "text": "Hello" },
                        { "type": "paragraph", "content": "A paragraph." },
                        { "type": "line" },
                        { "type": "rectangle", "fillColor": "#EEEEEE", "width": "100mm", "height": "50mm" }
                    ]
                }
            }]
        }
        """;

        var template = TemplateParser.Parse(json);
        var elements = template.Pages[0].Body.Elements;

        Assert.Equal(4, elements.Count);
        Assert.IsType<Models.Elements.TextElement>(elements[0]);
        Assert.IsType<Models.Elements.ParagraphElement>(elements[1]);
        Assert.IsType<Models.Elements.LineElement>(elements[2]);
        Assert.IsType<Models.Elements.RectangleElement>(elements[3]);
    }
}
