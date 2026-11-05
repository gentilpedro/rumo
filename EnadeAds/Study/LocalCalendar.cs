using Microsoft.Extensions.Options;

namespace EnadeAds.Study;

/// <summary>"Hoje" no fuso da plataforma (o dos alunos), não no fuso do servidor, que em produção é UTC.</summary>
public sealed class LocalCalendar(TimeProvider clock, IOptions<ExamOptions> exam)
{
    public DateOnly Today() => DateOnly.FromDateTime(ToLocal(clock.GetUtcNow()));

    /// <summary>Converte um instante para a hora local da plataforma, para exibir datas.</summary>
    public DateTime ToLocal(DateTimeOffset moment) =>
        TimeZoneInfo.ConvertTime(moment, TimeZoneInfo.FindSystemTimeZoneById(exam.Value.TimeZone)).DateTime;

    /// <summary>Início e fim (exclusivo) de um dia da plataforma, em instantes, para filtrar no banco.</summary>
    public (DateTimeOffset Start, DateTimeOffset End) DayRange(DateOnly day)
    {
        var zone = TimeZoneInfo.FindSystemTimeZoneById(exam.Value.TimeZone);
        DateTimeOffset At(DateOnly date)
        {
            var local = date.ToDateTime(TimeOnly.MinValue);
            // Em UTC, como os instantes gravados: o SQLite dos testes compara o número, não o instante.
            return new DateTimeOffset(local, zone.GetUtcOffset(local)).ToUniversalTime();
        }

        return (At(day), At(day.AddDays(1)));
    }
}
