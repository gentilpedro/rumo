using EnadeAds.Data;
using EnadeAds.Questions;
using Microsoft.Extensions.DependencyInjection;

namespace EnadeAds.Tests;

[Collection(AppCollection.Name)]
public class QuestionColumnLimitTests(EnadeAdsFactory factory)
{
    [Fact]
    public void DatabaseColumns_AcceptWhatTheValidationAccepts()
    {
        // O SQLite dos testes não aplica tamanho de coluna; o PostgreSQL aplica. Em 21/11/2026 o deploy da v1.12.0
        // caiu porque o enunciado passou a aceitar 12.000 caracteres e a coluna continuava com 8.000.
        using var scope = factory.Services.CreateScope();
        var entity = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Model.FindEntityType(typeof(Question))!;
        var option = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Model.FindEntityType(typeof(QuestionOption))!;

        Assert.True(entity.FindProperty(nameof(Question.Statement))!.GetMaxLength() is null or >= QuestionEditor.StatementMaxLength);
        Assert.True(entity.FindProperty(nameof(Question.Source))!.GetMaxLength() is null or >= QuestionEditor.SourceMaxLength);
        Assert.True(option.FindProperty(nameof(QuestionOption.Text))!.GetMaxLength() is null or >= QuestionEditor.OptionMaxLength);
        Assert.True(option.FindProperty(nameof(QuestionOption.Explanation))!.GetMaxLength() is null or >= QuestionEditor.OptionMaxLength);
    }
}
