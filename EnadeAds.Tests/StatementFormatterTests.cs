using EnadeAds.Questions;

namespace EnadeAds.Tests;

public class StatementFormatterTests
{
    [Fact]
    public void SplitsParagraphsAndCodeBlocks()
    {
        var blocks = StatementFormatter.Split("""
            Considere o código:

            for (int i = 0; i < 3; i++) {
                print(i);
            }

            O que é impresso?
            """);

        Assert.Equal(3, blocks.Count);
        Assert.False(blocks[0].IsCode);
        Assert.True(blocks[1].IsCode);
        Assert.Contains("    print(i);", blocks[1].Text);
        Assert.False(blocks[2].IsCode);
    }

    [Fact]
    public void AlignedTableAndSqlAreCode()
    {
        var blocks = StatementFormatter.Split("id | nome\n 1 | ADS\n\nSELECT nome\nFROM Curso;");

        Assert.All(blocks, block => Assert.True(block.IsCode));
    }

    [Fact]
    public void ListWithItemsEndingInSemicolonIsStillAList()
    {
        // Provas reais listam itens assim; antes o ";" fazia a lista inteira sair como código.
        var html = StatementFormatter.ToHtml("- Percorrer 99 km em 6 dias consecutivos;\n- Reduzir a distância em 5 km a cada dia.");

        Assert.Equal("<ul><li>Percorrer 99 km em 6 dias consecutivos;</li><li>Reduzir a distância em 5 km a cada dia.</li></ul>", html);
    }

    [Fact]
    public void BarInsideASentenceIsNotATable()
    {
        // " | " separando versos numa frase não é tabela; tabela tem mais de uma linha.
        var blocks = StatementFormatter.Split("III. No texto II, “Poesia | Uma mulher que grita”, predomina a função emotiva.");

        Assert.False(Assert.Single(blocks).IsCode);
    }

    [Fact]
    public void PlainSentenceIsParagraph()
    {
        var blocks = StatementFormatter.Split("Qual dos itens abaixo é um requisito não funcional?");

        Assert.False(Assert.Single(blocks).IsCode);
    }

    [Fact]
    public void FencedBlockKeepsBlankLinesAndIsCode()
    {
        var blocks = StatementFormatter.Split("Veja:\n\n```\nlinha 1\n\nlinha 3\n```\n\nFim.");

        Assert.Equal(3, blocks.Count);
        Assert.True(blocks[1].IsCode);
        Assert.Equal("linha 1\n\nlinha 3", blocks[1].Text);
    }

    [Fact]
    public void UnclosedFenceKeepsTheRestAsCode()
    {
        var blocks = StatementFormatter.Split("Texto\n\n```\nsem fechar");

        Assert.True(blocks[^1].IsCode);
        Assert.Equal("sem fechar", blocks[^1].Text);
    }

    [Fact]
    public void RendersBoldInlineCodeAndHttpsImage()
    {
        var html = StatementFormatter.ToHtml("Use **pilha** e `push(x)`. ![diagrama](https://exemplo.com/a.png)");

        Assert.Contains("<strong>pilha</strong>", html);
        Assert.Contains("<code>push(x)</code>", html);
        Assert.Contains("<img src=\"https://exemplo.com/a.png\" alt=\"diagrama\"", html);
    }

    [Theory]
    [InlineData("<script>alert(1)</script>")]
    [InlineData("**<img src=x onerror=alert(1)>**")]
    [InlineData("`<iframe src=//mal.example>`")]
    [InlineData("```\n</code></pre><script>alert(1)</script>\n```")]
    public void NeverEmitsHtmlTypedByTheAuthor(string statement)
    {
        var html = StatementFormatter.ToHtml(statement);

        Assert.DoesNotContain("<script", html);
        Assert.DoesNotContain("<img src=x", html);
        Assert.DoesNotContain("<iframe", html);
        Assert.Contains("&lt;", html);
    }

    [Theory]
    [InlineData("![x](http://exemplo.com/a.png)")]
    [InlineData("![x](javascript:alert(1))")]
    [InlineData("![x](data:image/png;base64,AAAA)")]
    public void ImagesOnlyFromHttps(string statement)
    {
        Assert.DoesNotContain("<img", StatementFormatter.ToHtml(statement));
    }

    [Fact]
    public void ImageAltCannotBreakTheAttribute()
    {
        var html = StatementFormatter.ToHtml("![\" onerror=\"alert(1)](https://exemplo.com/a.png)");

        Assert.DoesNotContain("\" onerror=\"", html);
    }
}
