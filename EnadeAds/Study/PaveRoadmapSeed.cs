namespace EnadeAds.Study;

/// <summary>
/// Roadmap inicial das três trilhas do PAVE (UFPel), a partir da Matriz de Referência PAVE 2026 (Anexo II do
/// Edital CRA nº 16/2026). Cada etapa cobra um ano do ensino médio, com as mesmas disciplinas, na ordem das
/// áreas da prova: Ciências da Natureza, Ciências Humanas, Matemática e Linguagens. A Etapa 3 tem a redação.
///
/// As disciplinas têm o mesmo slug nas três trilhas (biologia, fisica...): é por ele que as questões das provas
/// antigas acham a matéria. Ids a partir de 300.000, 400.000 e 500.000, um bloco por etapa.
///
/// Como na <see cref="RoadmapSeed"/>: depois de publicado, o conteúdo muda pela tela, não por este arquivo.
/// </summary>
public static class PaveRoadmapSeed
{
    public const int Stage1IdBase = 300_000;
    public const int Stage2IdBase = 400_000;
    public const int Stage3IdBase = 500_000;

    public const string EssaySlug = "redacao";
    public const string EnglishSlug = "ingles";
    public const string SpanishSlug = "espanhol";

    /// <summary>Idiomas entre os quais o candidato escolhe nas questões 30 a 32.</summary>
    public static IReadOnlyList<string> ForeignLanguageSlugs { get; } = [EnglishSlug, SpanishSlug];

    /// <summary>Disciplinas de cada área da prova objetiva, com os slugs usados nas três etapas.</summary>
    public static IReadOnlyList<string> ObjectiveSubjectSlugs { get; } =
    [
        "biologia", "fisica", "quimica",
        "historia", "geografia", "filosofia", "sociologia",
        "matematica",
        "lingua-portuguesa", "literatura", "arte", EnglishSlug, SpanishSlug,
    ];

    // A Biologia tem dois temas comuns às três etapas na matriz; cada etapa repete os dois, com slug próprio.
    private static (string Slug, string Title, string Summary) Ecosystems(int stage) =>
        ($"ecossistemas-impactos-e{stage}", "Ecossistemas e impactos ambientais",
            "Fluxo de energia, ciclos da água, do carbono e do nitrogênio, relações ecológicas, biomas brasileiros, poluição e mudanças climáticas. Tema comum às três etapas.");

    private static (string Slug, string Title, string Summary) Classification(int stage) =>
        ($"classificacao-biologica-e{stage}", "Diversidade e classificação biológica",
            "Categorias taxonômicas, os três domínios da vida, árvores filogenéticas e evolução humana; darwinismo social, eugenia e racismo \"científico\". Tema comum às três etapas.");

    // Espanhol e inglês são matérias separadas: na prova o candidato escolhe o idioma, e no Rumo cada um estuda
    // (e faz o simulado) no seu. Os assuntos da matriz são os mesmos para os dois.
    private static RoadmapSeed.Subject ForeignLanguage(string slug, string title, int stage) =>
        new(slug, title, StudyBlock.Languages,
            "As três últimas questões da prova são de língua estrangeira, e você escolhe entre espanhol e inglês. O que se cobra é leitura, não gramática decorada.",
            [
                ($"{slug}-generos-estrutura-e{stage}", "Gêneros e estrutura do texto", "Reconhecer o gênero (notícia, tirinha, anúncio, artigo), as partes do texto e como elas se ligam."),
                ($"{slug}-vocabulario-contexto-e{stage}", "Palavras no texto e no contexto", "Ideia principal e secundária, o que está explícito e implícito, cognatos e falsos cognatos."),
                ($"{slug}-objetivos-texto-e{stage}", "Objetivos do texto", "Quem escreve, para quem e para quê; pronomes e conectivos que retomam e ligam as ideias."),
            ]);

    private static RoadmapSeed.Subject English(int stage) => ForeignLanguage(EnglishSlug, "Língua inglesa", stage);

    private static RoadmapSeed.Subject Spanish(int stage) => ForeignLanguage(SpanishSlug, "Língua espanhola", stage);

    private static readonly object[] Stage1 =
    [
        new RoadmapSeed.Subject("biologia", "Biologia", StudyBlock.NaturalSciences,
            "Da origem da vida à célula: a base de toda a Biologia do ensino médio. As questões costumam partir de uma notícia de saúde ou de meio ambiente.",
            [
                ("origem-da-vida", "Teorias de origem da vida", "Abiogênese e biogênese, e o contexto histórico de cada explicação."),
                ("teorias-evolucao", "Teorias da evolução", "Lamarck e Darwin, seleção natural e a evolução humana."),
                ("celula-organelas", "Célula, organelas e membrana", "Procariontes e eucariontes, função de cada organela, osmose, difusão e transporte ativo."),
                ("metabolismo-energetico", "Metabolismo energético", "Fotossíntese, respiração, fermentação e quimiossíntese: etapas, produtos e quem faz cada uma."),
                ("divisao-celular", "Divisão celular", "Mitose e meiose: etapas e o papel de cada uma no crescimento e na formação de gametas."),
                ("histologia", "Histologia", "Tecidos epitelial, conjuntivo, muscular e nervoso: características e funções."),
                ("embriologia-reproducao", "Embriologia e reprodução", "Do zigoto à gástrula; reprodução sexuada e assexuada, com vantagens e desvantagens."),
                Ecosystems(1),
                Classification(1),
            ]),
        new RoadmapSeed.Subject("fisica", "Física", StudyBlock.NaturalSciences,
            "Mecânica: como descrever o movimento e o que o causa. Vale montar o desenho das forças antes de fazer conta.",
            [
                ("grandezas-unidades", "Grandezas e unidades", "Ordem de grandeza, notação científica, grandezas escalares e vetoriais e o Sistema Internacional."),
                ("cinematica", "Cinemática", "Velocidade e aceleração, movimento uniforme e uniformemente variado, movimento circular e queda livre."),
                ("leis-de-newton", "Leis de Newton e forças", "Força resultante, peso, normal, tração, elástica, atrito e centrípeta."),
                ("trabalho-energia", "Trabalho, energia e potência", "Energia cinética e potencial, conservação da energia mecânica e forças não conservativas."),
                ("impulso-quantidade-movimento", "Impulso e quantidade de movimento", "Colisões e o princípio da conservação da quantidade de movimento."),
                ("estatica", "Estática", "Equilíbrio do ponto material e do corpo extenso, momento de uma força e centro de gravidade."),
            ]),
        new RoadmapSeed.Subject("quimica", "Química", StudyBlock.NaturalSciences,
            "Do átomo à reação: a linguagem da Química e as contas com massa e quantidade de matéria.",
            [
                ("materia-misturas", "Matéria, substâncias e misturas", "Propriedades da matéria, substâncias simples e compostas, misturas e métodos de separação."),
                ("atomistica-tabela-periodica", "Atomística e tabela periódica", "Modelos atômicos, número atômico e de massa, íons, isótopos e propriedades periódicas."),
                ("ligacoes-quimicas", "Ligações e interações", "Ligação iônica, covalente e metálica, geometria, polaridade e forças intermoleculares."),
                ("funcoes-inorganicas", "Funções inorgânicas e reações", "Ácidos, bases, sais e óxidos: nomenclatura, propriedades e reações no ar, no solo e na água."),
                ("calculos-quimicos", "Cálculos químicos", "Conservação da massa, estequiometria, rendimento, pureza e reagente limitante."),
            ]),
        new RoadmapSeed.Checkpoint("checkpoint-natureza-e1", "Checkpoint: questões de Ciências da Natureza", StudyBlock.NaturalSciences,
            "Resolva as questões 1 a 9 de uma prova antiga da Etapa 1 com o relógio ligado: cerca de 6 minutos por questão."),

        new RoadmapSeed.Subject("historia", "História", StudyBlock.Humanities,
            "Da origem humana na África à formação do mundo moderno, sempre com fonte histórica para ler.",
            [
                ("introducao-historia", "Introdução à História", "Fontes, tempo, periodizações e disputas pela memória."),
                ("pre-historia", "Pré-História", "A África e a origem humana, Paleolítico e Neolítico, primeiras cidades e metalurgia."),
                ("mundo-antigo", "Mundo Antigo", "Egito, Mesopotâmia, Grécia e Roma, e a crise do mundo antigo."),
                ("mundo-medieval", "Mundo Medieval", "Impérios Bizantino e Islâmico, Idade Média ocidental e feudalismo."),
                ("mundo-moderno-capitalismo", "Mundo Moderno e formação do capitalismo", "Renascimento, Grandes Navegações, reformas religiosas, absolutismo, mercantilismo e Iluminismo."),
            ]),
        new RoadmapSeed.Subject("geografia", "Geografia", StudyBlock.Humanities,
            "Ler o espaço: mapas, a dinâmica da Terra e os problemas ambientais.",
            [
                ("orientacao-cartografia", "Orientação e cartografia", "Movimentos da Terra, coordenadas, fusos horários, escalas, projeções e geotecnologias."),
                ("estrutura-relevo-solos", "Estrutura da Terra, relevo e solos", "Tectônica de placas, rochas, formação e degradação dos solos, agentes do relevo."),
                ("clima-biomas", "Clima, biomas e domínios morfoclimáticos", "Dinâmica atmosférica, tipos de clima, eventos extremos, biomas e os domínios brasileiros."),
                ("agua-problemas-ambientais", "Recursos hídricos e problemas ambientais", "A água no Brasil e no mundo e os impactos da ação humana."),
            ]),
        new RoadmapSeed.Subject("filosofia", "Filosofia", StudyBlock.Humanities,
            "O que é filosofar e as ideias dos autores que abrem a tradição ocidental.",
            [
                ("introducao-filosofia", "Introdução à Filosofia", "Origens da filosofia, do que ela trata e o que é a reflexão filosófica."),
                ("filosofia-antiga", "Antiguidade clássica", "Pré-socráticos, a maiêutica de Sócrates, a alegoria da caverna de Platão e a ética e a lógica de Aristóteles."),
                ("filosofia-medieval", "Filosofia medieval", "Fé e razão na Europa cristã e as Confissões de Agostinho."),
                ("filosofia-moderna", "Filosofia moderna", "O cogito de Descartes e os limites do conhecimento em Kant."),
            ]),
        new RoadmapSeed.Subject("sociologia", "Sociologia", StudyBlock.Humanities,
            "Os conceitos que explicam a vida em sociedade, com Marx, Weber e Durkheim.",
            [
                ("classicos-sociologia", "Surgimento da Sociologia e os clássicos", "Iluminismo e revoluções; os conceitos-chave de Marx, Weber e Durkheim."),
                ("status-papeis-sociais", "Status, papéis e interação social", "Interação, representação social, cooperação e conflito."),
                ("estratificacao-mobilidade", "Estratificação e mobilidade social", "Classes sociais, desigualdades econômicas e políticas e mobilidade."),
                ("cultura-identidade", "Cultura e identidade", "Conceito de cultura, etnocentrismo e relativismo, cultura de massa e indústria cultural."),
            ]),
        new RoadmapSeed.Checkpoint("checkpoint-humanas-e1", "Checkpoint: questões de Ciências Humanas", StudyBlock.Humanities,
            "Resolva as questões 10 a 18 de uma prova antiga da Etapa 1. Leia a pergunta antes do texto-base."),

        new RoadmapSeed.Subject("matematica", "Matemática", StudyBlock.Mathematics,
            "São cinco questões: funções e progressões aparecem em quase toda prova.",
            [
                ("conjuntos-intervalos", "Conjuntos numéricos e intervalos", "Operações com números reais, diagramas de Venn e operações com intervalos."),
                ("funcoes-conceitos", "Funções: conceitos", "Domínio, imagem, raízes, crescimento, função composta e inversa, leitura de gráficos e tabelas."),
                ("funcoes-elementares", "Funções afim, quadrática, exponencial e logarítmica", "Gráfico, raízes, vértice, crescimento exponencial e propriedades dos logaritmos."),
                ("progressao-aritmetica", "Progressão aritmética", "Termo geral e soma dos termos."),
                ("progressao-geometrica", "Progressão geométrica", "Termo geral e soma dos termos da PG finita e da infinita."),
                ("estatistica", "Estatística", "Gráficos, média, mediana, moda, variância e desvio padrão."),
            ]),
        new RoadmapSeed.Checkpoint("checkpoint-matematica-e1", "Checkpoint: questões de Matemática", StudyBlock.Mathematics,
            "Resolva as questões 19 a 23 de uma prova antiga da Etapa 1. Na dúvida entre chutar e marcar \"Ignoro a resposta\", lembre que o erro desconta 10% de um acerto."),

        new RoadmapSeed.Subject("lingua-portuguesa", "Língua Portuguesa", StudyBlock.Languages,
            "A língua em uso: variação, sentido das palavras e as classes gramaticais dentro do texto.",
            [
                ("linguagem-figuras", "Linguagem, sentido e figuras", "Discurso formal e informal, denotação e conotação, figuras e vícios de linguagem."),
                ("variacao-linguistica-pave", "Variação linguística", "Sotaque, dialeto e idioleto, sem preconceito linguístico."),
                ("formacao-palavras", "Estrutura e formação das palavras", "Radical, desinências, vogal temática e afixos."),
                ("classes-variaveis", "Classes variáveis", "Substantivo, verbo, pronome, artigo e adjetivo: flexão, concordância, regência e colocação pronominal."),
                ("classes-invariaveis", "Classes invariáveis", "Advérbio, preposição e conjunção, e os operadores argumentativos."),
            ]),
        new RoadmapSeed.Subject("literatura", "Literatura", StudyBlock.Languages,
            "O que é literatura e os primeiros movimentos, de Portugal medieval ao Brasil do século XVI.",
            [
                ("introducao-literatura", "Introdução aos estudos literários", "Funções da literatura, gêneros lírico, narrativo e dramático, intertextualidade."),
                ("trovadorismo-classicismo", "Trovadorismo, Humanismo e Classicismo", "As cantigas medievais, o teatro de Gil Vicente e Camões (\"Os Lusíadas\")."),
                ("literatura-informacao", "Literatura de Informação", "Os textos sobre o Brasil do século XVI, como a carta de Caminha."),
                ("leitura-literaria-e1", "Leitura literária", "Interpretar textos clássicos e contemporâneos e compará-los com outras linguagens."),
            ]),
        new RoadmapSeed.Subject("arte", "Arte", StudyBlock.Languages,
            "Fundamentos das artes visuais e a relação da arte com a cultura.",
            [
                ("linguagem-visual", "Elementos e organização da linguagem visual", "Ponto, linha, forma, cor, textura e volume; equilíbrio, ritmo, proporção e contraste."),
                ("contextos-praticas-artisticas", "Contextos e práticas artísticas", "Função social da arte, modalidades das artes visuais e manifestações locais e regionais."),
            ]),
        English(1),
        Spanish(1),
        new RoadmapSeed.Checkpoint("checkpoint-simulado-pave-1", "Checkpoint final: simulado completo", StudyBlock.Languages,
            "Faça um simulado de 32 questões em 3h30, no horário da prova (início às 13h15). Depois, revise o caderno de erros até o dia do PAVE."),
    ];

    private static readonly object[] Stage2 =
    [
        new RoadmapSeed.Subject("biologia", "Biologia", StudyBlock.NaturalSciences,
            "Saúde e ambiente lidos por dados e indicadores, mais os dois temas comuns a todas as etapas.",
            [
                ("saude-idh", "Saúde e Índice de Desenvolvimento Humano", "Doenças e transtornos que afetam a população, vacinas, saúde mental e a relação entre desigualdade e vulnerabilidade."),
                Ecosystems(2),
                Classification(2),
            ]),
        new RoadmapSeed.Subject("fisica", "Física", StudyBlock.NaturalSciences,
            "Calor, luz e ondas: fenômenos do dia a dia explicados com poucas fórmulas.",
            [
                ("termometria-dilatacao", "Termometria e dilatação térmica", "Temperatura, escalas Celsius, Fahrenheit e Kelvin, dilatação de sólidos e líquidos e a água."),
                ("calorimetria", "Calorimetria", "Calor sensível e latente, calor específico, mudanças de estado, condução, convecção e irradiação."),
                ("optica-geometrica", "Óptica geométrica", "Reflexão e refração, espelhos planos e esféricos, lentes e a óptica da visão."),
                ("ondulatoria", "Ondulatória", "Amplitude, frequência e comprimento de onda, fenômenos ondulatórios, som e efeito Doppler."),
            ]),
        new RoadmapSeed.Subject("quimica", "Química", StudyBlock.NaturalSciences,
            "A físico-química: quanto de energia, com que rapidez e até onde uma reação vai.",
            [
                ("solucoes", "Soluções", "Soluções e coloides, solubilidade, unidades de concentração e diluição."),
                ("termoquimica", "Termoquímica", "Calor de reação, entalpia e Lei de Hess."),
                ("cinetica-quimica", "Cinética química", "Rapidez de reação, fatores que a alteram e catálise."),
                ("equilibrio-quimico", "Equilíbrio químico", "Constante de equilíbrio, deslocamento (Le Chatelier), pH, pOH e hidrólise de sais."),
                ("eletroquimica", "Eletroquímica", "Oxirredução, pilhas e baterias, eletrólise ígnea e em meio aquoso."),
                ("materiais-sustentabilidade", "Materiais, processos e ambiente", "Viabilidade dos processos químicos e a escolha de materiais pensando no impacto ambiental."),
            ]),
        new RoadmapSeed.Checkpoint("checkpoint-natureza-e2", "Checkpoint: questões de Ciências da Natureza", StudyBlock.NaturalSciences,
            "Resolva as questões 1 a 9 de uma prova antiga da Etapa 2 com o relógio ligado: cerca de 6 minutos por questão."),

        new RoadmapSeed.Subject("historia", "História", StudyBlock.Humanities,
            "Da colonização da América ao imperialismo do século XIX, com o Brasil e o Rio Grande do Sul em destaque.",
            [
                ("america-africa-colonizacao", "África Atlântica, povos indígenas e colonização da América", "Os mundos indígenas e africanos, as invasões europeias e os sistemas coloniais."),
                ("brasil-colonial", "Brasil colonial", "Economia, escravidão, resistência africana e indígena e a crise do sistema colonial."),
                ("rio-grande-do-sul-colonial-imperial", "Rio Grande do Sul colonial e imperial", "A formação do Rio Grande do Sul na colônia e no Império."),
                ("era-das-revolucoes", "A era das revoluções", "Revolução Industrial, Revolução Americana e Revolução Francesa."),
                ("brasil-imperio-pave", "Brasil Império", "Independência, os dois reinados, a crise do escravismo e o movimento republicano."),
                ("nacionalismos-imperialismos", "Nacionalismos e imperialismos", "Nacionalismo e unificações na Europa e os imperialismos do século XIX."),
            ]),
        new RoadmapSeed.Subject("geografia", "Geografia", StudyBlock.Humanities,
            "O mundo organizado em Estados e redes, e o campo brasileiro.",
            [
                ("espaco-mundial-geopolitica", "Espaço mundial e geopolítica", "Estado-nação, fronteiras, ONU, blocos econômicos e conflitos territoriais."),
                ("globalizacao", "Globalização", "Empresas globais, fluxos de dinheiro, mercadorias e informação, redes de transporte e diversidade cultural."),
                ("espaco-rural", "Produção do espaço rural", "Sistemas agrícolas, agricultura familiar, estrutura fundiária, reforma agrária e territórios indígenas e quilombolas."),
            ]),
        new RoadmapSeed.Subject("filosofia", "Filosofia", StudyBlock.Humanities,
            "Argumentar bem, agir bem e entender o papel da linguagem.",
            [
                ("logica", "Lógica", "O que é um argumento, lógica dedutiva e lógica indutiva."),
                ("etica", "Ética", "Ética e moral, e os dilemas éticos de hoje nas relações e nas profissões."),
                ("filosofia-linguagem", "Filosofia da linguagem", "O que a linguagem faz, do debate contemporâneo às redes sociais."),
            ]),
        new RoadmapSeed.Subject("sociologia", "Sociologia", StudyBlock.Humanities,
            "Sociedade e natureza, povos originários e a vida nas redes.",
            [
                ("sociologia-ambiental", "Meio ambiente e sociologia ambiental", "Sociedade de risco, produção e consumo, justiça ambiental, sociologia rural e urbana."),
                ("etnicidade-povos-originarios", "Etnicidade e povos originários", "Povos indígenas e quilombolas hoje, direitos territoriais e políticas afirmativas."),
                ("cultura-digital", "Cultura digital", "Redes, vigilância e dados, capitalismo de plataforma, algoritmos e inteligência artificial generativa."),
            ]),
        new RoadmapSeed.Checkpoint("checkpoint-humanas-e2", "Checkpoint: questões de Ciências Humanas", StudyBlock.Humanities,
            "Resolva as questões 10 a 18 de uma prova antiga da Etapa 2. Leia a pergunta antes do texto-base."),

        new RoadmapSeed.Subject("matematica", "Matemática", StudyBlock.Mathematics,
            "Trigonometria e geometria dominam a etapa. Desenhe a figura antes de calcular.",
            [
                ("triangulo-retangulo", "Triângulo retângulo", "Relações métricas, Teorema de Pitágoras, seno, cosseno e tangente."),
                ("trigonometria-pave", "Trigonometria", "Arcos e ângulos, círculo trigonométrico, identidades, lei dos senos e lei dos cossenos."),
                ("numeros-complexos", "Números complexos", "Forma algébrica e trigonométrica, operações e o plano de Argand-Gauss."),
                ("equacoes-algebricas", "Equações algébricas", "Raízes, multiplicidade e relações entre coeficientes e raízes."),
                ("geometria-plana-pave", "Geometria plana", "Unidades de medida, semelhança, polígonos regulares, círculo, perímetros e áreas."),
                ("geometria-espacial-pave", "Geometria espacial", "Poliedros e o teorema de Euler, prismas, pirâmides, cilindros, cones e esferas."),
            ]),
        new RoadmapSeed.Checkpoint("checkpoint-matematica-e2", "Checkpoint: questões de Matemática", StudyBlock.Mathematics,
            "Resolva as questões 19 a 23 de uma prova antiga da Etapa 2. Na dúvida entre chutar e marcar \"Ignoro a resposta\", lembre que o erro desconta 10% de um acerto."),

        new RoadmapSeed.Subject("lingua-portuguesa", "Língua Portuguesa", StudyBlock.Languages,
            "Sintaxe: como as orações se organizam e o que isso muda no sentido e na pontuação.",
            [
                ("termos-da-oracao", "Termos da oração", "Termos essenciais, integrantes e acessórios; tipos de sujeito e de predicado."),
                ("periodo-simples", "Período simples", "Sujeito, predicado, complementos, deslocamentos e pontuação."),
                ("periodo-coordenacao", "Período composto por coordenação", "As orações coordenadas e as relações de sentido entre elas."),
                ("periodo-subordinacao", "Período composto por subordinação", "Orações substantivas, adjetivas (explicativas e restritivas) e adverbiais."),
                ("oracoes-reduzidas", "Orações reduzidas", "De infinitivo, de gerúndio e de particípio."),
            ]),
        new RoadmapSeed.Subject("literatura", "Literatura", StudyBlock.Languages,
            "Do Barroco ao Realismo: a literatura brasileira dos séculos XVII a XIX.",
            [
                ("barroco-arcadismo", "Barroco e Arcadismo", "Gregório de Matos e os sermões de Vieira; Tomás Antônio Gonzaga e Cláudio Manuel da Costa."),
                ("romantismo", "Romantismo", "Independência e identidade nacional; poesia lírica, indianista e ultrarromântica; Gonçalves Dias, Álvares de Azevedo e José de Alencar."),
                ("realismo", "Realismo", "Contexto e características do movimento, em oposição ao idealismo romântico."),
            ]),
        new RoadmapSeed.Subject("arte", "Arte", StudyBlock.Languages,
            "História da arte e leitura crítica de obras.",
            [
                ("historia-da-arte", "História da arte e movimentos artísticos", "Da arte pré-histórica ao Renascimento e aos movimentos modernos e contemporâneos."),
                ("critica-estetica", "Análise crítica e estética", "Como analisar uma obra e a relação entre arte, sociedade e política."),
            ]),
        English(2),
        Spanish(2),
        new RoadmapSeed.Checkpoint("checkpoint-simulado-pave-2", "Checkpoint final: simulado completo", StudyBlock.Languages,
            "Faça um simulado de 32 questões em 3h30, no horário da prova (início às 13h15). Depois, revise o caderno de erros até o dia do PAVE."),
    ];

    private static readonly object[] Stage3 =
    [
        new RoadmapSeed.Subject("biologia", "Biologia", StudyBlock.NaturalSciences,
            "Genética e biotecnologia, mais os dois temas comuns a todas as etapas.",
            [
                ("genetica", "Genética", "Leis de Mendel, cruzamentos e probabilidade, padrões de herança e variabilidade genética."),
                ("biotecnologia", "Biotecnologia e bioética", "Clonagem, melhoramento genético, tecnologias do DNA, células-tronco, biorremediação e biossegurança."),
                Ecosystems(3),
                Classification(3),
            ]),
        new RoadmapSeed.Subject("fisica", "Física", StudyBlock.NaturalSciences,
            "Eletricidade e magnetismo, do choque estático ao gerador de energia.",
            [
                ("eletrostatica", "Eletrostática", "Carga elétrica, eletrização, Lei de Coulomb, campo e potencial elétrico."),
                ("eletrodinamica", "Eletrodinâmica", "Corrente, resistência, Leis de Ohm, potência, associação de resistores e circuitos simples."),
                ("eletromagnetismo", "Eletromagnetismo", "Ímãs, campo magnético de fios e solenoides, força magnética, indução (Faraday-Lenz) e espectro eletromagnético."),
            ]),
        new RoadmapSeed.Subject("quimica", "Química", StudyBlock.NaturalSciences,
            "Química orgânica: reconhecer a função pela estrutura e ligar a molécula ao uso dela.",
            [
                ("compostos-organicos", "Compostos orgânicos e cadeias carbônicas", "Hibridização do carbono, ligações entre carbonos e classificação das cadeias."),
                ("funcoes-organicas", "Funções orgânicas", "Hidrocarbonetos, funções oxigenadas e nitrogenadas, haletos: nomenclatura e propriedades."),
                ("isomeria", "Isomeria", "Isomeria plana e espacial, e por que ela importa em medicamentos."),
                ("bioquimica", "Noções de bioquímica", "Glicídios, lipídios, aminoácidos, proteínas e polímeros."),
                ("quimica-organica-contextos", "Química orgânica em contexto", "Agricultura, saúde, medicamentos e as questões éticas e ambientais envolvidas."),
                ("radioatividade", "Radioatividade", "Emissões radioativas e tempo de meia-vida."),
            ]),
        new RoadmapSeed.Checkpoint("checkpoint-natureza-e3", "Checkpoint: questões de Ciências da Natureza", StudyBlock.NaturalSciences,
            "Resolva as questões 1 a 9 de uma prova antiga da Etapa 3 com o relógio ligado: cerca de 6 minutos por questão."),

        new RoadmapSeed.Subject("historia", "História", StudyBlock.Humanities,
            "O século XX e o tempo presente, no mundo e no Brasil.",
            [
                ("guerras-mundiais-entreguerras", "Guerras mundiais e entreguerras", "Primeira Guerra, Revolução Russa, crise de 1929, fascismo, nazismo e Segunda Guerra."),
                ("guerra-fria-descolonizacao", "Guerra Fria e descolonização", "A disputa entre os blocos, a independência de países da África e da Ásia, o Oriente Médio e a crise do Leste Europeu."),
                ("america-latina-mundo-contemporaneo", "América Latina e mundo contemporâneo", "Da América Latina do pós-guerra aos temas políticos e sociais de hoje."),
                ("primeira-republica", "Primeira República", "Café e industrialização, movimentos sociais, tenentismo, modernismo e o Rio Grande do Sul."),
                ("era-vargas-populismo", "Era Vargas e período democrático", "Revolução de 1930, Estado Novo, movimento operário e os governos de 1945 a 1964."),
                ("ditadura-redemocratizacao-pave", "Ditadura civil-militar e redemocratização", "Golpe de 1964, repressão e resistência, e a Constituição de 1988."),
                ("brasil-contemporaneo", "Brasil contemporâneo", "Democracia e desigualdades, problemas ambientais, racismo e antirracismo, culturas indígenas e afro-brasileiras."),
            ]),
        new RoadmapSeed.Subject("geografia", "Geografia", StudyBlock.Humanities,
            "Indústria, cidade e população: onde as pessoas vivem e trabalham.",
            [
                ("industria-energia", "Produção industrial e energia", "Revoluções industriais, distribuição da indústria, fontes de energia e a matriz energética brasileira."),
                ("espaco-urbano", "Produção do espaço urbano", "Redes urbanas, metropolização, urbanização brasileira, segregação e direito à cidade."),
                ("populacao", "População e espaço geográfico", "Indicadores e teorias demográficas, pirâmides etárias, migrações, trabalho e renda."),
            ]),
        new RoadmapSeed.Subject("filosofia", "Filosofia", StudyBlock.Humanities,
            "Filosofia política e estética.",
            [
                ("filosofia-politica", "Política", "A formação do Estado moderno: Maquiavel, Hobbes, Locke, o liberalismo, os socialismos e o anarquismo."),
                ("estetica", "Estética", "O belo e o feio, a questão do gosto, a atitude estética, indústria cultural e as funções da arte."),
            ]),
        new RoadmapSeed.Subject("sociologia", "Sociologia", StudyBlock.Humanities,
            "Poder, raça, trabalho e gênero na sociedade brasileira.",
            [
                ("poder-estado-democracia", "Poder, Estado e democracia", "Teorias do poder, Estado e governo, cidadania, direitos humanos e movimentos sociais."),
                ("relacoes-etnico-raciais", "Relações étnico-raciais", "Política de branqueamento, racismo e injúria racial, e a desvalorização da cultura afro-brasileira e indígena."),
                ("mundo-do-trabalho", "Mundo do trabalho", "Terceirização, precarização e capitalismo de plataforma."),
                ("interseccionalidade", "Interseccionalidade: gênero, raça e classe", "Papéis sociais de gênero, desigualdade econômica e violência de gênero."),
            ]),
        new RoadmapSeed.Checkpoint("checkpoint-humanas-e3", "Checkpoint: questões de Ciências Humanas", StudyBlock.Humanities,
            "Resolva as questões 10 a 18 de uma prova antiga da Etapa 3. Leia a pergunta antes do texto-base."),

        new RoadmapSeed.Subject("matematica", "Matemática", StudyBlock.Mathematics,
            "Contagem, probabilidade, matrizes e geometria analítica, além da matemática financeira do dia a dia.",
            [
                ("analise-combinatoria-pave", "Análise combinatória", "Princípio fundamental da contagem, arranjos, permutações e combinações."),
                ("probabilidade-pave", "Probabilidade", "Eventos independentes, probabilidade condicional e da união de eventos."),
                ("matrizes-determinantes", "Matrizes e determinantes", "Tipos e operações com matrizes, matriz inversa e determinantes até ordem 3."),
                ("sistemas-lineares", "Sistemas lineares", "Classificação e resolução, com as matrizes associadas."),
                ("geometria-analitica", "Geometria analítica", "Ponto, distância, equações da reta e da circunferência e posições relativas."),
                ("matematica-financeira", "Matemática financeira", "Regra de três, porcentagem, juros simples e compostos, descontos e educação financeira."),
            ]),
        new RoadmapSeed.Checkpoint("checkpoint-matematica-e3", "Checkpoint: questões de Matemática", StudyBlock.Mathematics,
            "Resolva as questões 19 a 23 de uma prova antiga da Etapa 3. Na dúvida entre chutar e marcar \"Ignoro a resposta\", lembre que o erro desconta 10% de um acerto."),

        new RoadmapSeed.Subject("lingua-portuguesa", "Língua Portuguesa", StudyBlock.Languages,
            "O texto como um todo: gêneros, construção de sentido e norma culta. É também a base da redação.",
            [
                ("tipos-generos-textuais", "Tipos e gêneros textuais", "Tipos de texto, gêneros da fala, da escrita e das mídias digitais."),
                ("textualidade", "Textualidade: coesão e coerência", "Referenciação, organização de tópicos e parágrafos, discurso direto e indireto, pontuação e sentido."),
                ("compreensao-leitura", "Compreensão da leitura", "Inferência, dito e subentendido, ambiguidade e ironia."),
                ("norma-culta", "Adequação à norma culta", "Ler e escrever de acordo com a norma culta da língua."),
            ]),
        new RoadmapSeed.Subject("literatura", "Literatura", StudyBlock.Languages,
            "Do Parnasianismo aos autores de hoje: a literatura brasileira do século XX e XXI.",
            [
                ("parnasianismo-simbolismo", "Parnasianismo e Simbolismo", "Olavo Bilac, Alberto de Oliveira e Raimundo Correia; Cruz e Sousa e Alphonsus de Guimaraens."),
                ("pre-modernismo", "Pré-Modernismo", "Lima Barreto, Euclides da Cunha e Monteiro Lobato."),
                ("modernismo-primeira-fase", "Modernismo: 1ª fase (1922-1930)", "A Semana de Arte Moderna de 1922, Mário de Andrade, Oswald de Andrade e Manuel Bandeira."),
                ("modernismo-segunda-fase", "Modernismo: 2ª fase (1930-1945)", "A prosa de Graciliano Ramos, Jorge Amado e José Lins do Rego; a poesia de Drummond, Cecília Meireles e Vinícius de Moraes."),
                ("modernismo-terceira-fase", "Modernismo: 3ª fase (pós-1945)", "João Cabral de Melo Neto, Clarice Lispector, Guimarães Rosa e Lygia Fagundes Telles."),
                ("literatura-contemporanea-pave", "Literatura contemporânea", "Pluralidade de vozes e linguagens híbridas: Conceição Evaristo, Itamar Vieira Junior, Sérgio Vaz, Eliane Potiguara e Djamila Ribeiro."),
            ]),
        new RoadmapSeed.Subject("arte", "Arte", StudyBlock.Languages,
            "Arte brasileira e a relação entre arte e tecnologia.",
            [
                ("arte-brasileira", "Arte brasileira e identidades culturais", "Arte indígena e africana no Brasil, arte colonial e imperial, o Modernismo e a arte contemporânea."),
                ("arte-tecnologia", "Arte e tecnologia", "Fotografia, cinema, arte digital e as linguagens híbridas."),
            ]),
        English(3),
        Spanish(3),
        new RoadmapSeed.Checkpoint("checkpoint-linguagens-e3", "Checkpoint: questões de Linguagens", StudyBlock.Languages,
            "Resolva as questões 24 a 32 de uma prova antiga da Etapa 3 e deixe pelo menos 1 hora da prova para a redação."),

        new RoadmapSeed.Subject(EssaySlug, "Redação", StudyBlock.Essay,
            "Só a Etapa 3 tem redação: um texto dissertativo-argumentativo de 16 a 30 linhas, com título, que vale até 14 pontos.",
            [
                ("proposta-coletanea", "Tema e coletânea", "Ler a proposta e os textos de apoio, sem fugir do tema e sem copiar trechos da coletânea."),
                ("estrutura-dissertativa", "Tipo de texto: a estrutura dissertativo-argumentativa", "Título, tese, argumentos e conclusão. Menos de 16 linhas ou outro tipo de texto zera a redação."),
                ("coerencia-coesao-redacao", "Coerência e coesão", "Ideias que progridem sem contradição, ligadas por conectivos e retomadas."),
                ("modalidade-escrita-formal", "Modalidade escrita formal", "Domínio da modalidade escrita formal da língua portuguesa."),
            ]),
        new RoadmapSeed.Checkpoint("checkpoint-simulado-pave-3", "Checkpoint final: simulado e redação", StudyBlock.Essay,
            "Faça um simulado de 32 questões e escreva uma redação, tudo em 4 horas, no horário da prova (início às 13h15). Depois, revise o caderno de erros até o dia do PAVE."),
    ];

    public static IReadOnlyList<RoadmapNode> Stage1Nodes { get; } = RoadmapSeed.Build(Stage1, Tracks.Pave1, Stage1IdBase);
    public static IReadOnlyList<RoadmapNode> Stage2Nodes { get; } = RoadmapSeed.Build(Stage2, Tracks.Pave2, Stage2IdBase);
    public static IReadOnlyList<RoadmapNode> Stage3Nodes { get; } = RoadmapSeed.Build(Stage3, Tracks.Pave3, Stage3IdBase);

    public static IReadOnlyList<RoadmapNode> Nodes { get; } = [.. Stage1Nodes, .. Stage2Nodes, .. Stage3Nodes];
}
