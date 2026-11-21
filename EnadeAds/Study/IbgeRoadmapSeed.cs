namespace EnadeAds.Study;

/// <summary>
/// Roadmap inicial da trilha IBGE 2027 · Recenseador, a seleção do 12º Censo Agropecuário, Florestal e Aquícola
/// (Edital IBGE nº 03/2026, banca Cesgranrio). A prova tem 60 questões: 15 de Língua Portuguesa, 15 de Matemática e
/// 30 de Conhecimentos Técnicos (item 10.6). Os conteúdos de Português e Matemática vêm do Anexo III do edital; os de
/// Conhecimentos Técnicos, da apostila oficial que o edital indica, na ordem do sumário dela.
/// Ids a partir de 900.000.
///
/// Como na <see cref="RoadmapSeed"/>: depois de publicado, o conteúdo muda pela tela, não por este arquivo.
/// </summary>
public static class IbgeRoadmapSeed
{
    public const int IdBase = 900_000;

    public static IReadOnlyList<string> SubjectSlugs { get; } = ["lingua-portuguesa", "matematica", "conhecimentos-tecnicos"];

    private static readonly object[] Spine =
    [
        new RoadmapSeed.Subject("conhecimentos-tecnicos", "Conhecimentos Técnicos", StudyBlock.ProfessionalContext,
            "Metade da prova: 30 questões sobre a apostila oficial do 12º Censo Agro, publicada pela Cesgranrio. É a parte que mais pesa e a única que não existe em outros concursos: leia a apostila inteira.",
            [
                ("ibge-censo-agro", "O IBGE e o Censo Agropecuário", "O que o IBGE faz, para que serve o Censo Agro e o que muda no 12º Censo (apostila, p. 4 a 10)."),
                ("estrutura-censitaria", "Estrutura organizacional censitária", "Quem é quem no Censo, do recenseador ao coordenador, e a quem cada um responde (p. 11 a 19)."),
                ("instrumentos-trabalho", "Instrumentos de trabalho", "O que o recenseador usa em campo e para quê (p. 20 a 22)."),
                ("navegacao-campo", "Navegação no trabalho de campo", "Como se orientar no setor e percorrer a área de trabalho (p. 23 a 31)."),
                ("divisao-territorial", "Divisão político-administrativa e estruturas territoriais", "Estados, municípios, distritos, setores censitários e as outras estruturas que organizam a coleta (p. 32 a 38)."),
                ("povos-comunidades-tradicionais", "Povos e comunidades tradicionais", "Terras indígenas, territórios quilombolas e outras estruturas de PCTs (p. 39 a 42)."),
                ("endereco-coordenadas", "Endereço e coordenadas geográficas", "Como registrar o endereço e a localização de cada estabelecimento (p. 43 a 50)."),
                ("estabelecimento-agropecuario", "Estabelecimento agropecuário", "O que conta como estabelecimento, quem é o produtor e quais atividades entram (p. 51 a 56)."),
                ("produtor-sem-area-situacoes", "Produtores sem área e situações especiais", "Os casos que fogem da regra e como o recenseador deve tratá-los (p. 57 a 66)."),
            ]),
        new RoadmapSeed.Checkpoint("checkpoint-conhecimentos-tecnicos", "Checkpoint: questões de Conhecimentos Técnicos", StudyBlock.ProfessionalContext,
            "Resolva as questões de Conhecimentos Técnicos do banco sem consultar a apostila. As que errar, procure na página citada na fonte da questão."),

        new RoadmapSeed.Subject("lingua-portuguesa", "Língua Portuguesa", StudyBlock.Languages,
            "Quinze questões, quase sempre a partir de um texto. Comece pela compreensão e depois revise a gramática do edital.",
            [
                ("compreensao-texto", "Compreensão de texto", "Ideia principal, informações explícitas e implícitas e o sentido de trechos no texto."),
                ("significacao-palavras", "Significação das palavras", "Sinônimos, antônimos e homônimos, e o sentido da palavra no contexto."),
                ("pontuacao-frases-paragrafos", "Pontuação e organização do texto", "Vírgula e outros sinais, e a sequência lógica de frases e parágrafos."),
                ("ortografia-acentuacao", "Ortografia e acentuação", "Grafia oficial das palavras e as regras de acento gráfico."),
                ("classes-palavras-ibge", "Classes de palavras", "Substantivo, adjetivo, verbo, advérbio, pronome, preposição e conjunção, e o papel de cada um na frase."),
                ("concordancia-regencia", "Concordância e regência", "Concordância nominal e verbal; regência nominal e verbal."),
                ("verbos-pronomes", "Verbos e pronomes", "Verbos regulares e irregulares, vozes do verbo e emprego dos pronomes."),
            ]),
        new RoadmapSeed.Checkpoint("checkpoint-lingua-portuguesa-ibge", "Checkpoint: questões de Língua Portuguesa", StudyBlock.Languages,
            "Resolva questões de Português das provas antigas de Recenseador com o relógio ligado: cerca de 4 minutos por questão."),

        new RoadmapSeed.Subject("matematica", "Matemática", StudyBlock.Mathematics,
            "Quinze questões de matemática do dia a dia: contas, proporção, porcentagem, medidas e leitura de tabelas, gráficos e mapas.",
            [
                ("numeros-naturais", "Números naturais", "Operações, propriedades, múltiplos e divisores, e problemas."),
                ("inteiros-racionais", "Números inteiros e racionais", "Operações com negativos, frações e decimais, e problemas."),
                ("razao-proporcao-porcentagem", "Razão, proporção e porcentagem", "Grandezas proporcionais, divisão proporcional, regra de três simples e porcentagem."),
                ("equacoes-primeiro-grau", "Equações do 1º grau", "Montar e resolver a equação a partir do problema."),
                ("medidas", "Medidas e conversões", "Comprimento, superfície, volume, massa e tempo, e o sistema métrico decimal."),
                ("areas-perimetros-volumes", "Áreas, perímetros e volumes", "Cálculo com as figuras e sólidos mais comuns."),
                ("tabelas-graficos-mapas", "Tabelas, gráficos, mapas e croquis", "Ler e interpretar informações em tabelas, gráficos, quadros, mapas, croquis e esquemas."),
            ]),
        new RoadmapSeed.Checkpoint("checkpoint-simulado-ibge", "Checkpoint final: simulado completo", StudyBlock.Mathematics,
            "Faça um simulado de 60 questões em 4 horas, no horário da prova (31/01/2027, das 13h às 17h). Lembre que é preciso 18 pontos no total e ao menos 1 em cada disciplina."),
    ];

    public static IReadOnlyList<RoadmapNode> Nodes { get; } = RoadmapSeed.Build(Spine, Tracks.IbgeRecenseador, IdBase);
}
