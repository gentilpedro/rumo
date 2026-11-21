using EnadeAds.Study;

namespace EnadeAds.Questions;

public enum QuestionKind
{
    /// <summary>Quatro ou cinco alternativas, uma correta.</summary>
    Objective = 0,

    /// <summary>Resposta escrita, comparada com um padrão de resposta.</summary>
    Discursive = 1,
}

/// <summary>
/// Questão objetiva (cinco alternativas, como no ENADE e no ENEM, ou quatro, como na OAB) ou discursiva (padrão de resposta). A trilha é a da matéria.
/// No PAVE, a sexta alternativa ("Ignoro a resposta") não é guardada: é uma opção do simulado.
/// </summary>
public class Question
{
    public int Id { get; set; }
    public QuestionKind Kind { get; set; }

    /// <summary>Identificador estável de questões importadas (ex.: enade-2021-ads-q12); nulo para as criadas na tela.</summary>
    public string? ExternalKey { get; set; }
    public int SubjectId { get; set; }
    public RoadmapNode? Subject { get; set; }

    /// <summary>Texto puro; quebras de linha e indentação são preservadas (código e tabelas).</summary>
    public required string Statement { get; set; }

    /// <summary>De onde vem a questão, por exemplo "ENADE 2021 – questão 12" ou "Autoria própria".</summary>
    public required string Source { get; set; }

    /// <summary>Autor na plataforma; nulo para o conteúdo inicial.</summary>
    public string? AuthorId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>Discursiva: o que uma boa resposta deve conter. Só aparece depois que o aluno envia a dele.</summary>
    public string? ExpectedAnswer { get; set; }

    /// <summary>Discursiva: tamanho sugerido da resposta, em linhas (a prova costuma pedir cerca de 15).</summary>
    public int? SuggestedLines { get; set; }

    /// <summary>Questão arquivada some das listas e da página pública; as tentativas dos alunos ficam.</summary>
    public DateTimeOffset? ArchivedAt { get; set; }

    public List<QuestionOption> Options { get; set; } = [];
}

public class QuestionOption
{
    public int Id { get; set; }
    public int QuestionId { get; set; }
    public char Letter { get; set; }
    public required string Text { get; set; }
    public bool IsCorrect { get; set; }

    /// <summary>Por que a alternativa está certa ou errada; só aparece depois de responder.</summary>
    public required string Explanation { get; set; }
}

public class QuestionAttempt
{
    public long Id { get; set; }
    public int QuestionId { get; set; }
    public required string UserId { get; set; }
    public int OptionId { get; set; }
    public bool IsCorrect { get; set; }
    public DateTimeOffset AnsweredAt { get; set; }

    /// <summary>Preenchido quando a resposta foi dada dentro de um desafio semanal.</summary>
    public int? ChallengeId { get; set; }
}

/// <summary>Resposta de um aluno a uma questão discursiva. Só o próprio aluno vê.</summary>
public class DiscursiveAnswer
{
    public long Id { get; set; }
    public int QuestionId { get; set; }
    public required string UserId { get; set; }
    public required string Text { get; set; }
    public DateTimeOffset AnsweredAt { get; set; }
}
