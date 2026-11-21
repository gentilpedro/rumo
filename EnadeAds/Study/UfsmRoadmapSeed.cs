namespace EnadeAds.Study;

/// <summary>
/// Roadmap inicial das três trilhas do Vestibular/PSS da UFSM, a partir do Manual do Candidato do Vestibular UFSM
/// 2027 e dos três Cadernos de Habilidades de Referência (habilidades da BNCC adaptadas, um caderno por prova).
/// Cada prova cobra um ano do ensino médio com as mesmas disciplinas: 40 questões em 3 horas, na ordem das áreas
/// do manual: Ciências Humanas, Ciências da Natureza, Matemática e Linguagens. Só a Prova 3 tem redação.
///
/// Os cadernos não listam conteúdos, só habilidades genéricas; os tópicos abaixo traduzem as habilidades de cada
/// ano no que as provas de 2025 e 2026 daquele ano cobraram. As provas não agrupam as questões por disciplina:
/// cada uma gira em torno de um tema do ano, e a disciplina de cada questão é inferida pelo conteúdo.
///
/// As disciplinas têm o mesmo slug nas três trilhas (fisica, literatura...): é por ele que as questões das provas
/// antigas acham a matéria. Ids a partir de 600.000, 700.000 e 800.000, um bloco por prova.
///
/// Como na <see cref="RoadmapSeed"/>: depois de publicado, o conteúdo muda pela tela, não por este arquivo.
/// </summary>
public static class UfsmRoadmapSeed
{
    public const int Prova1IdBase = 600_000;
    public const int Prova2IdBase = 700_000;
    public const int Prova3IdBase = 800_000;

    public const string EssaySlug = "redacao";
    public const string EnglishSlug = "ingles";
    public const string SpanishSlug = "espanhol";

    /// <summary>Disciplinas de cada área da prova objetiva, com os slugs usados nas três provas.</summary>
    public static IReadOnlyList<string> ObjectiveSubjectSlugs { get; } =
    [
        "filosofia-sociologia", "geografia", "historia",
        "quimica", "fisica", "biologia",
        "matematica",
        "artes-educacao-fisica", "lingua-portuguesa", "literatura", SpanishSlug, EnglishSlug,
    ];

    // Espanhol e inglês são matérias separadas: na prova o candidato escolhe o idioma (questões 38 a 40), e no
    // Rumo cada um estuda no seu. O que se cobra é leitura, com os mesmos tipos de questão nos dois idiomas.
    private static RoadmapSeed.Subject ForeignLanguage(string slug, string title, int prova, string example) =>
        new(slug, title, StudyBlock.Languages,
            "As três últimas questões são de língua estrangeira, sobre um texto ligado ao tema da prova. É leitura, não gramática decorada.",
            [
                ($"{slug}-compreensao-p{prova}", "Compreensão de texto jornalístico", $"Ideia principal, dados e números do texto e afirmações de verdadeiro ou falso. Exemplo de fonte: {example}."),
                ($"{slug}-vocabulario-p{prova}", "Vocabulário no contexto", "Sinônimos no texto, cognatos e falsos cognatos, e o sentido de expressões."),
                ($"{slug}-conectivos-referencia-p{prova}", "Conectivos e referência", "Palavras que ligam as ideias (causa, contraste, comparação) e pronomes que retomam termos."),
                ($"{slug}-formacao-palavras-p{prova}", "Formação de palavras", "Prefixos e sufixos que mudam o sentido ou a classe da palavra."),
                ($"{slug}-multimodal-p{prova}", "Textos multimodais", "Infográficos, gráficos, ilustrações e posts de redes sociais: relacionar imagem e texto."),
            ]);

    private static RoadmapSeed.Subject Spanish(int prova, string example) => ForeignLanguage(SpanishSlug, "Língua espanhola", prova, example);

    private static RoadmapSeed.Subject English(int prova, string example) => ForeignLanguage(EnglishSlug, "Língua inglesa", prova, example);

    private static RoadmapSeed.Checkpoint AreaCheckpoint(string area, string slug, StudyBlock block, int prova) =>
        new($"checkpoint-{slug}-p{prova}", $"Checkpoint: questões de {area}", block,
            $"Resolva as questões de {area} de uma prova antiga da Prova {prova}, com o relógio ligado. São 40 questões em 3 horas: cerca de 4 minutos e meio por questão.");

    private static readonly object[] Prova1 =
    [
        new RoadmapSeed.Subject("filosofia-sociologia", "Filosofia e Sociologia", StudyBlock.Humanities,
            "São 2 questões: em geral uma de Filosofia e uma de Sociologia, ligadas ao tema da prova.",
            [
                ("descartes-corpo-mente", "Descartes: corpo e mente", "O dualismo cartesiano e como ele se compara à ciência de hoje."),
                ("etica-ambiental-p1", "Ética e meio ambiente", "Responsabilidade com as próximas gerações, utilitarismo e decisões coletivas sobre conservação."),
                ("classicos-sociologia-p1", "Clássicos da Sociologia", "Durkheim, Bourdieu e a saúde e a doença como construções sociais."),
                ("saberes-tradicionais", "Saberes tradicionais e ciência", "Saberes indígenas e afro-brasileiros de cura comparados ao conhecimento científico."),
            ]),
        new RoadmapSeed.Subject("geografia", "Geografia", StudyBlock.Humanities,
            "São 4 questões, quase sempre sobre ambiente e natureza do Brasil e do Rio Grande do Sul.",
            [
                ("cartografia-tematica", "Cartografia temática", "Ler mapas temáticos: escala, legenda, variáveis visuais e implantação pontual."),
                ("rochas-relevo-solos", "Rochas, relevo e solos", "Rochas ígneas, sedimentares e metamórficas, fósseis, erosão, intemperismo e perfil topográfico."),
                ("clima-biomas-p1", "Clima e biomas brasileiros", "Zonas climáticas, Pampa, Caatinga, Cerrado e Mata Atlântica."),
                ("conflitos-ambientais", "Conflitos ambientais e racismo ambiental", "Enchentes de 2024 no RS, garimpo em terras indígenas, queimadas e agrotóxicos."),
                ("geoparques-sustentabilidade", "Geoparques e desenvolvimento sustentável", "Geoparques da UNESCO, como Quarta Colônia e Caçapava, e unidades de conservação."),
            ]),
        new RoadmapSeed.Subject("historia", "História", StudyBlock.Humanities,
            "São 4 questões, da Pré-História ao mundo colonial, sempre com um texto ou fonte para ler.",
            [
                ("pre-historia-evolucao-humana", "Pré-História e origem humana", "Hominídeos africanos, bipedalismo e a dispersão humana."),
                ("antiguidade-egito", "Antiguidade", "Egito Antigo: religião, mumificação e medicina; fontes como Heródoto."),
                ("idade-media", "Idade Média", "Peste negra, Reconquista ibérica, medicina árabe e arte românica e gótica."),
                ("america-colonial-p1", "América colonial", "Colonização espanhola, Inquisição e práticas de cura."),
                ("diaspora-quilombos", "Diáspora africana e quilombos", "Palmares, comunidades quilombolas, irmandades negras e o mito da democracia racial."),
            ]),
        AreaCheckpoint("Ciências Humanas", "humanas", StudyBlock.Humanities, 1),

        new RoadmapSeed.Subject("quimica", "Química", StudyBlock.NaturalSciences,
            "São 4 questões, quase sempre de verdadeiro ou falso: átomo, tabela periódica, ligações e reações.",
            [
                ("atomistica-ions", "Átomo, íons e isótopos", "Distribuição eletrônica de átomos e íons, isoeletrônicos e isótopos."),
                ("tabela-periodica", "Tabela periódica", "Famílias, elementos de transição e propriedades periódicas: raio atômico e energia de ionização."),
                ("ligacoes-polaridade", "Ligações, geometria e polaridade", "Ligação iônica e covalente, geometria molecular, ligações de hidrogênio e propriedades da água."),
                ("solucoes-condutividade", "Dissociação, ionização e condutividade", "Por que algumas soluções conduzem corrente e outras não; solubilidade e densidade."),
                ("oxirreducao-p1", "Reações e oxirredução", "Tipos de reação, balanceamento, número de oxidação e agente oxidante e redutor."),
                ("funcoes-inorganicas-p1", "Funções inorgânicas", "Ácidos, bases, sais e óxidos: nomenclatura e hidrólise de sais."),
            ]),
        new RoadmapSeed.Subject("fisica", "Física", StudyBlock.NaturalSciences,
            "São 4 questões de mecânica e gravitação, em contextos de esporte e cotidiano.",
            [
                ("leis-newton-atrito", "Leis de Newton e atrito", "Ação e reação, peso e normal, plano inclinado e atrito estático."),
                ("movimento-circular", "Movimento circular", "MCU, transmissão por correia (bicicleta) e força centrípeta numa curva."),
                ("energia-potencia", "Energia, trabalho e potência", "Energia cinética e potencial, conservação da energia mecânica, potência e rendimento."),
                ("impulso-quantidade-movimento-p1", "Impulso e quantidade de movimento", "Força média num impacto e conservação da quantidade de movimento."),
                ("estatica-torque", "Equilíbrio e torque", "Momento de uma força, alavancas e equilíbrio do corpo rígido."),
                ("gravitacao-kepler", "Gravitação e Leis de Kepler", "Órbitas elípticas, lei das áreas, terceira lei e gravitação universal."),
            ]),
        new RoadmapSeed.Subject("biologia", "Biologia", StudyBlock.NaturalSciences,
            "São 4 questões, da célula à saúde, muitas a partir de uma notícia ou gráfico.",
            [
                ("citologia", "Célula", "Procariontes e eucariontes, organelas, parede celular e glicocálix."),
                ("acidos-nucleicos-proteinas", "DNA, RNA e síntese de proteínas", "Códons, ribossomos, aminoácidos e transgenia."),
                ("divisao-celular-p1", "Divisão celular", "Mitose, meiose e crossing-over como fonte de variabilidade."),
                ("metabolismo-energetico-p1", "Fotossíntese e respiração", "Etapas, produtos e onde ocorre cada processo."),
                ("histologia-p1", "Tecidos animais e vegetais", "Sangue (hemácias, leucócitos e plaquetas) e tecidos de sustentação das plantas."),
                ("saude-epidemiologia", "Saúde e doenças", "Doenças como a leptospirose, saneamento e leitura de gráficos epidemiológicos."),
            ]),
        AreaCheckpoint("Ciências da Natureza", "natureza", StudyBlock.NaturalSciences, 1),

        new RoadmapSeed.Subject("matematica", "Matemática", StudyBlock.Mathematics,
            "São 5 questões: funções, porcentagem e juros, e leitura de gráficos.",
            [
                ("porcentagem-graficos", "Porcentagem, razão e gráficos", "Porcentagem, proporção, densidade e leitura de tabelas e gráficos de pesquisas."),
                ("funcao-afim-sistemas", "Função afim e sistemas lineares", "Gráfico da reta, proporcionalidade, funções por partes e sistemas de equações."),
                ("funcao-quadratica", "Função quadrática", "Raízes, vértice, máximo e mínimo, como lucro = receita − custo."),
                ("exponencial-logaritmo", "Exponencial e logaritmo", "Crescimento exponencial, meia-vida, propriedades dos logaritmos e escalas como o pH."),
                ("juros-simples-compostos-p1", "Juros simples e compostos", "Comparar crescimento linear e exponencial do dinheiro."),
                ("medidas-notacao-cientifica", "Medidas, unidades e notação científica", "Conversão de unidades, grandezas compostas e algarismos significativos."),
            ]),
        AreaCheckpoint("Matemática", "matematica", StudyBlock.Mathematics, 1),

        new RoadmapSeed.Subject("artes-educacao-fisica", "Artes e Educação Física", StudyBlock.Languages,
            "São 3 questões de Artes Integradas e Educação Física: linguagens artísticas e corpo e saúde.",
            [
                ("arte-rupestre-expedicoes", "Arte rupestre e o olhar do viajante", "Pinturas rupestres e as expedições artísticas do século XIX, como Debret."),
                ("musica-nacionalista", "Música brasileira de concerto", "O nacionalismo musical de Villa-Lobos e Camargo Guarnieri."),
                ("danca-ballet", "Dança", "Ballet clássico, dança moderna e contemporânea: como reconhecer cada uma."),
                ("atividade-fisica-saude-p1", "Atividade física e saúde", "Exercício aeróbico, frequência cardíaca, caminhada e bem-estar."),
            ]),
        new RoadmapSeed.Subject("lingua-portuguesa", "Língua Portuguesa", StudyBlock.Languages,
            "São 5 questões: leitura de texto, coesão e o sentido das palavras.",
            [
                ("leitura-generos-p1", "Leitura e gêneros", "Objetivo do texto em notícias e textos de divulgação científica."),
                ("coesao-conectivos-p1", "Coesão e conectivos", "Conectivos de causa, finalidade e adição, retomadas e verbos que introduzem vozes."),
                ("pressuposto-modalizacao", "Pressupostos e modalização", "O que o texto dá como certo e como ele marca opinião, certeza ou dúvida."),
                ("semantica-formacao-palavras", "Sentido e formação das palavras", "Polissemia, prefixos e sufixos, e pares como \"há\" e \"a\"."),
                ("multimodais-p1", "Tirinhas e infográficos", "Relacionar texto verbal e imagem em tirinhas, infográficos e ilustrações."),
            ]),
        new RoadmapSeed.Subject("literatura", "Literatura", StudyBlock.Languages,
            "São 2 questões. Não há lista de leituras: a Prova 1 cobra os movimentos até o Arcadismo.",
            [
                ("generos-literarios-p1", "Gêneros e versificação", "Lírico, narrativo e dramático; métrica, redondilha e rima."),
                ("humanismo-gil-vicente", "Humanismo", "O teatro de Gil Vicente e o auto como crítica de costumes."),
                ("barroco", "Barroco", "Gregório de Matos: poesia satírica, lírica e religiosa."),
                ("arcadismo", "Arcadismo", "Tomás Antônio Gonzaga (Marília de Dirceu) e Cláudio Manuel da Costa: bucolismo e Minas Gerais."),
            ]),
        Spanish(1, "reportagens do El País e da UNESCO"),
        English(1, "infográficos de saúde e reportagens de turismo"),
        AreaCheckpoint("Linguagens", "linguagens", StudyBlock.Languages, 1),
        new RoadmapSeed.Checkpoint("checkpoint-simulado-p1", "Checkpoint final: simulado completo", StudyBlock.Languages,
            "Faça um simulado de 40 questões em 3 horas, no horário da prova: 09/01, às 13h30. Depois, revise o caderno de erros até o dia da prova."),
    ];

    private static readonly object[] Prova2 =
    [
        new RoadmapSeed.Subject("filosofia-sociologia", "Filosofia e Sociologia", StudyBlock.Humanities,
            "São 2 questões: em geral uma de Filosofia e uma de Sociologia, ligadas ao tema da prova.",
            [
                ("platao-caverna", "Platão e a alegoria da caverna", "Mundo sensível e mundo inteligível, comparados ao mundo das redes sociais."),
                ("etica-ambiental-p2", "Ética ambiental", "Responsabilidade moral com o planeta e com as próximas gerações."),
                ("trabalho-associativismo", "Trabalho e associativismo", "Catadores, cooperativas, trabalho formal e informal, Estado e mercado."),
                ("leitura-sociedade", "Leitura e sociedade", "Formação de leitores e o acesso desigual ao livro."),
            ]),
        new RoadmapSeed.Subject("geografia", "Geografia", StudyBlock.Humanities,
            "São 4 questões sobre energia, clima, migrações e globalização.",
            [
                ("fontes-energia-p2", "Fontes de energia", "Energias renováveis e não renováveis, eólica e solar."),
                ("efeito-estufa-clima", "Efeito estufa e mudanças climáticas", "Efeito estufa, chuva ácida e camada de ozônio: diferenças e causas."),
                ("correntes-ambientalistas", "Correntes ambientalistas", "Preservacionismo, conservacionismo e socioambientalismo."),
                ("migracoes-refugiados", "Migrações e refugiados", "Migrações internas no Brasil (Censo 2022) e refugiados climáticos."),
                ("globalizacao-cultura", "Globalização e cultura", "Cadeias produtivas globais e hibridismo cultural."),
            ]),
        new RoadmapSeed.Subject("historia", "História", StudyBlock.Humanities,
            "São 4 questões, da Idade Moderna ao Brasil do século XX.",
            [
                ("imprensa-renascimento", "Imprensa e Idade Moderna", "A prensa de Gutenberg e a circulação de livros e ideias."),
                ("iluminismo", "Iluminismo", "A Enciclopédia, a razão e a crítica ao Antigo Regime."),
                ("america-colonial-p2", "América colonial e escravidão", "Plantation, tráfico de africanos e o impacto da colonização sobre os indígenas."),
                ("revolucao-industrial-p2", "Revolução Industrial", "Carvão, máquina a vapor, urbanização e impacto ambiental."),
                ("brasil-seculo-xix", "Brasil no século XIX", "Urbanização e o crescimento do público leitor."),
                ("republica-1945-1964", "República de 1945 a 1964", "O período democrático e o populismo."),
            ]),
        AreaCheckpoint("Ciências Humanas", "humanas", StudyBlock.Humanities, 2),

        new RoadmapSeed.Subject("quimica", "Química", StudyBlock.NaturalSciences,
            "São 4 questões de físico-química, e a estequiometria aparece todo ano.",
            [
                ("estequiometria", "Estequiometria", "Balanceamento, massa, volume nas CNTP, rendimento e reagente em excesso."),
                ("dispersoes-solucoes", "Soluções e dispersões", "Solução, coloide e suspensão, concentração e titulação."),
                ("propriedades-coligativas", "Propriedades coligativas", "Ebulioscopia e tonoscopia: o que um soluto faz com a água."),
                ("equilibrio-ph", "Equilíbrio químico e pH", "Deslocamento de equilíbrio, pH e o sistema do gás carbônico na água."),
                ("eletroquimica-p2", "Eletroquímica", "Oxirredução, pilhas, ânodo, cátodo e potencial de redução."),
            ]),
        new RoadmapSeed.Subject("fisica", "Física", StudyBlock.NaturalSciences,
            "São 4 questões de fluidos, calor e ondas.",
            [
                ("hidrostatica", "Pressão e hidrostática", "Pressão como força por área e o Teorema de Stevin."),
                ("calor-equilibrio-termico", "Calor e equilíbrio térmico", "Diferença entre calor e temperatura, trocas de calor e mudanças de estado com a pressão."),
                ("termodinamica", "Termodinâmica", "Lei zero, primeira e segunda leis, máquinas térmicas e rendimento de Carnot."),
                ("propriedades-liquidos", "Propriedades dos líquidos", "Viscosidade, tensão superficial e capilaridade."),
                ("ondas-eletromagneticas", "Ondas e espectro eletromagnético", "Frequência e comprimento de onda, luz visível, infravermelho e efeito estufa."),
            ]),
        new RoadmapSeed.Subject("biologia", "Biologia", StudyBlock.NaturalSciences,
            "São 4 questões sobre seres vivos, plantas e ecologia.",
            [
                ("classificacao-taxonomia", "Classificação dos seres vivos", "Lineu, nomenclatura binomial e categorias taxonômicas."),
                ("bacterias", "Bactérias", "Estrutura, nutrição e reprodução, e o uso em biorremediação."),
                ("botanica", "Botânica", "Tecidos vegetais e nutrição mineral das plantas."),
                ("artropodes", "Insetos e artrópodes", "Exoesqueleto, metamorfose e tipos de desenvolvimento."),
                ("fisiologia-respiratoria", "Sistema respiratório", "Anatomia, pleura, diafragma e a mecânica da respiração."),
                ("ecossistemas-p2", "Ecossistemas", "Manguezal e outros ambientes, adaptações das plantas e ciclos da matéria."),
            ]),
        AreaCheckpoint("Ciências da Natureza", "natureza", StudyBlock.NaturalSciences, 2),

        new RoadmapSeed.Subject("matematica", "Matemática", StudyBlock.Mathematics,
            "São 5 questões: trigonometria, contagem, probabilidade, progressões e estatística.",
            [
                ("triangulos-semelhanca", "Triângulos e semelhança", "Semelhança, relações métricas, áreas e lei dos senos e dos cossenos."),
                ("funcoes-trigonometricas", "Funções seno e cosseno", "Fenômenos periódicos, período, máximo e mínimo."),
                ("progressoes", "Progressões aritmética e geométrica", "Termo geral e soma dos termos de PA e PG."),
                ("analise-combinatoria-p2", "Análise combinatória", "Princípio fundamental da contagem, arranjos e combinações."),
                ("probabilidade-p2", "Probabilidade", "Espaço amostral, união, complementar e probabilidade condicional."),
                ("estatistica-p2", "Estatística", "Média, moda, mediana, desvio padrão, histograma e box-plot."),
            ]),
        AreaCheckpoint("Matemática", "matematica", StudyBlock.Mathematics, 2),

        new RoadmapSeed.Subject("artes-educacao-fisica", "Artes e Educação Física", StudyBlock.Languages,
            "São 3 questões de Artes Integradas e Educação Física: arte moderna e contemporânea, teatro e corpo.",
            [
                ("arte-contemporanea-ativismo", "Arte contemporânea e ativismo", "Protestos em museus, Guerrilla Girls e o lugar da mulher na história da arte."),
                ("concretismo-neoconcretismo", "Concretismo e neoconcretismo", "Grupo Ruptura, Lygia Pape e o livro de artista."),
                ("teatro-musical", "Teatro e teatro musical", "O que se exige do intérprete em canto, dança e atuação."),
                ("corpo-saude-p2", "Corpo, postura e saúde", "Atividade física ao ar livre, aptidão física, postura e uso de telas."),
            ]),
        new RoadmapSeed.Subject("lingua-portuguesa", "Língua Portuguesa", StudyBlock.Languages,
            "São 5 questões: gramática dentro do texto e leitura de textos multimodais.",
            [
                ("interpretacao-p2", "Interpretação de texto", "Reportagens e textos de opinião: tese, argumentos e inferências."),
                ("classes-palavras", "Classes de palavras", "Advérbio, adjetivo e palavras que mudam de classe conforme o uso."),
                ("termos-oracao-aposto", "Termos da oração", "Aposto, vozes verbais, agente da passiva e o \"se\"."),
                ("linguagem-figurada", "Linguagem figurada", "Metáfora, metonímia e o sentido das palavras no contexto."),
                ("multimodais-p2", "Charges, tiras e cartazes", "Relação entre linguagem verbal e não verbal e o humor."),
            ]),
        new RoadmapSeed.Subject("literatura", "Literatura", StudyBlock.Languages,
            "São 2 questões. Não há lista de leituras: a Prova 2 cobra Romantismo e Realismo.",
            [
                ("romantismo-poesia", "Romantismo: poesia", "Gonçalves Dias e o nacionalismo; Castro Alves e a poesia condoreira; versificação."),
                ("romantismo-prosa", "Romantismo: prosa", "O romance romântico e a busca de uma identidade nacional."),
                ("realismo-machado", "Realismo e Machado de Assis", "Memórias Póstumas de Brás Cubas: ironia, narrador defunto e crítica à vaidade."),
                ("realismo-europeu", "Realismo europeu", "Balzac e o romance da ascensão da burguesia."),
            ]),
        Spanish(2, "reportagens do El País e posts sobre leitura"),
        English(2, "textos da ONU sobre sustentabilidade e sobre os benefícios da leitura"),
        AreaCheckpoint("Linguagens", "linguagens", StudyBlock.Languages, 2),
        new RoadmapSeed.Checkpoint("checkpoint-simulado-p2", "Checkpoint final: simulado completo", StudyBlock.Languages,
            "Faça um simulado de 40 questões em 3 horas, no horário da prova: 10/01, às 8h. Depois, revise o caderno de erros até o dia da prova."),
    ];

    private static readonly object[] Prova3 =
    [
        new RoadmapSeed.Subject("filosofia-sociologia", "Filosofia e Sociologia", StudyBlock.Humanities,
            "São 2 questões, muitas vezes sobre trabalho, tecnologia e justiça.",
            [
                ("classicos-sociologia-p3", "Marx, Durkheim e Weber", "Alienação e exploração, anomia, e estratificação por classe e status."),
                ("trabalho-plataformas", "Trabalho e plataformas digitais", "Uberização, precarização, flexibilização e desemprego tecnológico."),
                ("rawls-justica", "Justiça e ética", "Rawls (véu de ignorância e princípio da diferença), meritocracia e ética dos algoritmos."),
                ("racismo-estrutural", "Racismo estrutural", "Desigualdade racial no trabalho e na renda."),
            ]),
        new RoadmapSeed.Subject("geografia", "Geografia", StudyBlock.Humanities,
            "São 4 questões sobre indústria, trabalho, população e redes.",
            [
                ("fordismo-toyotismo", "Fordismo e toyotismo", "Produção em massa, just in time, automação e trabalho flexível."),
                ("globalizacao-p3", "Globalização e neoliberalismo", "Desemprego estrutural e a produção espalhada pelo mundo."),
                ("redes-tecnica-geotecnologias", "Redes, técnica e geotecnologias", "Meio técnico-científico-informacional, fluxos, SIG e satélites."),
                ("populacao-p3", "População", "Pirâmides etárias, transição demográfica e envelhecimento."),
                ("rede-urbana", "Rede e hierarquia urbana", "Metrópoles, distribuição de serviços e desigualdades regionais."),
            ]),
        new RoadmapSeed.Subject("historia", "História", StudyBlock.Humanities,
            "São 4 questões, do século XX ao Brasil recente, com o trabalho em destaque.",
            [
                ("industrializacao-trabalho", "Industrialização e trabalho", "Revolução Industrial, proletarização e a mulher no trabalho na Primeira República."),
                ("segunda-guerra", "Segunda Guerra Mundial", "Fases da guerra, tecnologia e criptografia (Turing e a Enigma)."),
                ("guerra-fria", "Guerra Fria", "Os dois blocos, perestroika, glasnost e o fim da URSS."),
                ("brasil-1950-1970", "Brasil de 1950 a 1970", "Modernização urbana e o Milagre Econômico da ditadura."),
                ("ditadura-redemocratizacao", "Ditadura e redemocratização", "Distensão, crises do petróleo, inflação e o Plano Collor."),
            ]),
        AreaCheckpoint("Ciências Humanas", "humanas", StudyBlock.Humanities, 3),

        new RoadmapSeed.Subject("quimica", "Química", StudyBlock.NaturalSciences,
            "São 4 questões, quase todas de Química Orgânica a partir de moléculas reais (remédios, hormônios, alimentos).",
            [
                ("cadeias-hibridizacao", "Cadeias carbônicas e hibridização", "Hibridização sp, sp² e sp³ e ligações sigma e pi."),
                ("funcoes-organicas", "Funções orgânicas", "Álcool, fenol, ácido carboxílico, éster, amina e amida: reconhecer pela estrutura."),
                ("isomeria", "Isomeria", "Isomeria geométrica (E/Z), carbono assimétrico e isomeria óptica."),
                ("reacoes-organicas", "Reações orgânicas", "Adição, substituição, esterificação e hidrogenação de óleos."),
                ("bioquimica-p3", "Moléculas da vida", "Aminoácidos, ligação peptídica, gorduras e celulose."),
            ]),
        new RoadmapSeed.Subject("fisica", "Física", StudyBlock.NaturalSciences,
            "São 4 questões, com eletricidade e magnetismo em destaque.",
            [
                ("eletrostatica-campo", "Campo e força elétrica", "Carga, Lei de Coulomb e como o campo cai com a distância."),
                ("eletrodinamica", "Circuitos elétricos", "Lei de Ohm, associação de resistores, Leis de Kirchhoff e potência no chuveiro."),
                ("eletromagnetismo", "Eletromagnetismo", "Campo de um fio, força magnética sobre cargas, motores e geradores."),
                ("radiacoes-radioatividade", "Radiações e radioatividade", "Tipos de radiação, meia-vida e usos na saúde e na indústria."),
                ("optica-lentes", "Lentes", "Refração e formação de imagens em lentes."),
            ]),
        new RoadmapSeed.Subject("biologia", "Biologia", StudyBlock.NaturalSciences,
            "São 4 questões de genética, evolução e ecologia.",
            [
                ("cromossomos-genoma", "Cromossomos e genoma", "Homólogos, centrômero, telômero e o genoma humano."),
                ("heranca-ligada-x", "Genética e herança ligada ao sexo", "Leis de Mendel, heredogramas, hemofilia e daltonismo."),
                ("evolucao", "Evolução", "Seleção natural e sexual e a teoria sintética: mutação e recombinação."),
                ("biodiversidade-conservacao", "Biodiversidade e conservação", "Agrobiodiversidade, polinizadores e conservação in situ."),
                ("ciclos-biogeoquimicos", "Ciclos biogeoquímicos", "Ciclos do carbono e da água, desmatamento e combustíveis fósseis."),
            ]),
        AreaCheckpoint("Ciências da Natureza", "natureza", StudyBlock.NaturalSciences, 3),

        new RoadmapSeed.Subject("matematica", "Matemática", StudyBlock.Mathematics,
            "São 5 questões: geometria, estatística, porcentagem e probabilidade.",
            [
                ("geometria-plana-areas", "Áreas e semelhança", "Áreas de polígonos e círculos e a razão entre áreas de figuras semelhantes."),
                ("geometria-espacial", "Geometria espacial", "Áreas e volumes de prismas, pirâmides, cilindros, cones e esferas."),
                ("transformacoes-geometricas", "Transformações geométricas", "Translação, reflexão, rotação, homotetia e ladrilhamento."),
                ("estatistica-p3", "Estatística", "Média simples e ponderada, mediana e amplitude a partir de gráficos."),
                ("matematica-financeira-p3", "Porcentagem e juros", "Descontos, impostos, juros simples e compostos."),
                ("probabilidade-p3", "Probabilidade", "Espaço amostral e probabilidade da união de eventos."),
            ]),
        AreaCheckpoint("Matemática", "matematica", StudyBlock.Mathematics, 3),

        new RoadmapSeed.Subject("artes-educacao-fisica", "Artes e Educação Física", StudyBlock.Languages,
            "São 3 questões de Artes Integradas e Educação Física: movimentos artísticos, arte e tecnologia e o corpo.",
            [
                ("movimentos-artisticos", "Movimentos artísticos", "Romantismo, Surrealismo e Cubismo; o impacto da fotografia na pintura."),
                ("arte-tecnologia-ia", "Arte, tecnologia e autoria", "Inteligência artificial, originalidade e direito de autor."),
                ("mercado-ensino-arte", "Mercado e ensino de arte", "Valor simbólico e econômico da obra e o ensino de arte no Brasil."),
                ("educacao-fisica-profissao", "Educação Física e sociedade do desempenho", "O papel do profissional, prescrição de treino e a cobrança por desempenho."),
            ]),
        new RoadmapSeed.Subject("lingua-portuguesa", "Língua Portuguesa", StudyBlock.Languages,
            "São 5 questões: sintaxe, norma-padrão e leitura. É também a base da redação.",
            [
                ("periodo-composto", "Período composto", "Orações subordinadas substantivas e adjetivas, reduzidas e conectivos."),
                ("funcoes-sintaticas", "Funções sintáticas", "Complemento nominal, adjunto adnominal e aposto."),
                ("concordancia-regencia", "Concordância, regência e colocação", "Concordância com o \"se\", regência verbal e colocação pronominal."),
                ("verbos-modos", "Tempos e modos verbais", "Futuro do pretérito, imperativo afirmativo e negativo."),
                ("pontuacao", "Pontuação", "Vírgula com aposto, orações intercaladas e antes do \"e\"."),
                ("interpretacao-graficos-p3", "Interpretação de textos e gráficos", "Textos informativos e gráficos: inferir sem extrapolar."),
            ]),
        new RoadmapSeed.Subject("literatura", "Literatura", StudyBlock.Languages,
            "São 2 questões. Não há lista de leituras: a Prova 3 cobra o Modernismo e a literatura contemporânea.",
            [
                ("modernismo-regionalismo", "Romance de 30", "Graciliano Ramos e Vidas Secas; o regionalismo nordestino."),
                ("joao-cabral", "João Cabral de Melo Neto", "Morte e Vida Severina: auto, retirante e linguagem seca."),
                ("concretismo-tropicalia", "Concretismo e Tropicália", "Poesia visual e as canções tropicalistas."),
                ("poesia-contemporanea", "Poesia contemporânea", "Paulo Leminski, poesia marginal e intertextualidade."),
            ]),
        Spanish(3, "El País e BBC Mundo sobre inteligência artificial e trabalho"),
        English(3, "relatórios do Fórum Econômico Mundial sobre empregos e clima"),
        AreaCheckpoint("Linguagens", "linguagens", StudyBlock.Languages, 3),

        new RoadmapSeed.Subject(EssaySlug, "Redação", StudyBlock.Essay,
            "Só a Prova 3 tem redação: artigo de opinião ou carta aberta (a prova indica qual), de 15 a 30 linhas, valendo até 10 pontos. Nota abaixo de 3,0 elimina.",
            [
                ("proposta-tema", "Tema, situação e coletânea", "Ler o comando: situação comunicativa, tema, palavras-chave e para quem se escreve. Tangenciar o tema limita a nota a 3,5."),
                ("artigo-opiniao", "Artigo de opinião", "Título, contextualização, tese, argumentos e conclusão; separar a sua voz das outras vozes citadas."),
                ("carta-aberta", "Carta aberta", "Vocativo, interlocutor e remetente, motivo, argumentos e proposta. O remetente é um papel, nunca o seu nome."),
                ("argumentacao", "Argumentação", "Sustentar a tese com dados e fontes, prever e refutar argumentos contrários."),
                ("coesao-norma", "Coesão e norma-padrão", "Conectivos, retomadas, parágrafos, pontuação e registro formal."),
                ("motivos-de-zero", "O que zera a redação", "Fuga do tema, gênero errado, letra ilegível, identificação, linhas fora do limite ou plágio."),
            ]),
        new RoadmapSeed.Checkpoint("checkpoint-simulado-p3", "Checkpoint final: simulado e redação", StudyBlock.Essay,
            "Faça um simulado de 40 questões e escreva uma redação, tudo em 5 horas, no horário da prova: 10/01, às 13h30. Depois, revise o caderno de erros até o dia da prova."),
    ];

    public static IReadOnlyList<RoadmapNode> Prova1Nodes { get; } = RoadmapSeed.Build(Prova1, Tracks.Ufsm1, Prova1IdBase);
    public static IReadOnlyList<RoadmapNode> Prova2Nodes { get; } = RoadmapSeed.Build(Prova2, Tracks.Ufsm2, Prova2IdBase);
    public static IReadOnlyList<RoadmapNode> Prova3Nodes { get; } = RoadmapSeed.Build(Prova3, Tracks.Ufsm3, Prova3IdBase);

    public static IReadOnlyList<RoadmapNode> Nodes { get; } = [.. Prova1Nodes, .. Prova2Nodes, .. Prova3Nodes];
}
