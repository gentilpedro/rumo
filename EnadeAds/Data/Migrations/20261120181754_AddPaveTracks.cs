using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EnadeAds.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPaveTracks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IgnoredCount",
                table: "MockExams",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IgnoredAnswer",
                table: "MockExamQuestions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.InsertData(
                table: "Tracks",
                columns: new[] { "Id", "Basis", "ExamDate", "ExamName", "Label", "Level", "MockExamStyle", "Name", "Order", "SecondExamDate", "Slug" },
                values: new object[,]
                {
                    { 4, "Os conteúdos do 1º ano do ensino médio na Matriz de Referência PAVE 2026 da UFPel, na ordem das áreas da prova.", new DateOnly(2026, 12, 5), "PAVE", "PAVE 1", 1, 2, "PAVE 2026 · Etapa 1", 4, null, "pave-1" },
                    { 5, "Os conteúdos do 2º ano do ensino médio na Matriz de Referência PAVE 2026 da UFPel, na ordem das áreas da prova.", new DateOnly(2026, 12, 6), "PAVE", "PAVE 2", 1, 2, "PAVE 2026 · Etapa 2", 5, null, "pave-2" },
                    { 6, "Os conteúdos do 3º ano do ensino médio e a redação na Matriz de Referência PAVE 2026 da UFPel, na ordem das áreas da prova.", new DateOnly(2026, 12, 6), "PAVE", "PAVE 3", 1, 2, "PAVE 2026 · Etapa 3", 6, null, "pave-3" }
                });

            migrationBuilder.InsertData(
                table: "RoadmapNodes",
                columns: new[] { "Id", "Block", "Kind", "Order", "ParentId", "Slug", "Summary", "Title", "TrackId" },
                values: new object[,]
                {
                    { 300100, 7, 0, 1, null, "biologia", "Da origem da vida à célula: a base de toda a Biologia do ensino médio. As questões costumam partir de uma notícia de saúde ou de meio ambiente.", "Biologia", 4 },
                    { 300200, 7, 0, 2, null, "fisica", "Mecânica: como descrever o movimento e o que o causa. Vale montar o desenho das forças antes de fazer conta.", "Física", 4 },
                    { 300300, 7, 0, 3, null, "quimica", "Do átomo à reação: a linguagem da Química e as contas com massa e quantidade de matéria.", "Química", 4 },
                    { 300400, 6, 0, 5, null, "historia", "Da origem humana na África à formação do mundo moderno, sempre com fonte histórica para ler.", "História", 4 },
                    { 300500, 6, 0, 6, null, "geografia", "Ler o espaço: mapas, a dinâmica da Terra e os problemas ambientais.", "Geografia", 4 },
                    { 300600, 6, 0, 7, null, "filosofia", "O que é filosofar e as ideias dos autores que abrem a tradição ocidental.", "Filosofia", 4 },
                    { 300700, 6, 0, 8, null, "sociologia", "Os conceitos que explicam a vida em sociedade, com Marx, Weber e Durkheim.", "Sociologia", 4 },
                    { 300800, 8, 0, 10, null, "matematica", "São cinco questões: funções e progressões aparecem em quase toda prova.", "Matemática", 4 },
                    { 300900, 5, 0, 12, null, "lingua-portuguesa", "A língua em uso: variação, sentido das palavras e as classes gramaticais dentro do texto.", "Língua Portuguesa", 4 },
                    { 301000, 5, 0, 13, null, "literatura", "O que é literatura e os primeiros movimentos, de Portugal medieval ao Brasil do século XVI.", "Literatura", 4 },
                    { 301100, 5, 0, 14, null, "arte", "Fundamentos das artes visuais e a relação da arte com a cultura.", "Arte", 4 },
                    { 301200, 5, 0, 15, null, "ingles", "As três últimas questões da prova são de língua estrangeira, e você escolhe entre espanhol e inglês. O que se cobra é leitura, não gramática decorada.", "Língua inglesa", 4 },
                    { 301300, 5, 0, 16, null, "espanhol", "As três últimas questões da prova são de língua estrangeira, e você escolhe entre espanhol e inglês. O que se cobra é leitura, não gramática decorada.", "Língua espanhola", 4 },
                    { 310001, 7, 2, 4, null, "checkpoint-natureza-e1", "Resolva as questões 1 a 9 de uma prova antiga da Etapa 1 com o relógio ligado: cerca de 6 minutos por questão.", "Checkpoint: questões de Ciências da Natureza", 4 },
                    { 310002, 6, 2, 9, null, "checkpoint-humanas-e1", "Resolva as questões 10 a 18 de uma prova antiga da Etapa 1. Leia a pergunta antes do texto-base.", "Checkpoint: questões de Ciências Humanas", 4 },
                    { 310003, 8, 2, 11, null, "checkpoint-matematica-e1", "Resolva as questões 19 a 23 de uma prova antiga da Etapa 1. Na dúvida entre chutar e marcar \"Ignoro a resposta\", lembre que o erro desconta 10% de um acerto.", "Checkpoint: questões de Matemática", 4 },
                    { 310004, 5, 2, 17, null, "checkpoint-simulado-pave-1", "Faça um simulado de 32 questões em 3h30, no horário da prova (início às 13h15). Depois, revise o caderno de erros até o dia do PAVE.", "Checkpoint final: simulado completo", 4 },
                    { 400100, 7, 0, 1, null, "biologia", "Saúde e ambiente lidos por dados e indicadores, mais os dois temas comuns a todas as etapas.", "Biologia", 5 },
                    { 400200, 7, 0, 2, null, "fisica", "Calor, luz e ondas: fenômenos do dia a dia explicados com poucas fórmulas.", "Física", 5 },
                    { 400300, 7, 0, 3, null, "quimica", "A físico-química: quanto de energia, com que rapidez e até onde uma reação vai.", "Química", 5 },
                    { 400400, 6, 0, 5, null, "historia", "Da colonização da América ao imperialismo do século XIX, com o Brasil e o Rio Grande do Sul em destaque.", "História", 5 },
                    { 400500, 6, 0, 6, null, "geografia", "O mundo organizado em Estados e redes, e o campo brasileiro.", "Geografia", 5 },
                    { 400600, 6, 0, 7, null, "filosofia", "Argumentar bem, agir bem e entender o papel da linguagem.", "Filosofia", 5 },
                    { 400700, 6, 0, 8, null, "sociologia", "Sociedade e natureza, povos originários e a vida nas redes.", "Sociologia", 5 },
                    { 400800, 8, 0, 10, null, "matematica", "Trigonometria e geometria dominam a etapa. Desenhe a figura antes de calcular.", "Matemática", 5 },
                    { 400900, 5, 0, 12, null, "lingua-portuguesa", "Sintaxe: como as orações se organizam e o que isso muda no sentido e na pontuação.", "Língua Portuguesa", 5 },
                    { 401000, 5, 0, 13, null, "literatura", "Do Barroco ao Realismo: a literatura brasileira dos séculos XVII a XIX.", "Literatura", 5 },
                    { 401100, 5, 0, 14, null, "arte", "História da arte e leitura crítica de obras.", "Arte", 5 },
                    { 401200, 5, 0, 15, null, "ingles", "As três últimas questões da prova são de língua estrangeira, e você escolhe entre espanhol e inglês. O que se cobra é leitura, não gramática decorada.", "Língua inglesa", 5 },
                    { 401300, 5, 0, 16, null, "espanhol", "As três últimas questões da prova são de língua estrangeira, e você escolhe entre espanhol e inglês. O que se cobra é leitura, não gramática decorada.", "Língua espanhola", 5 },
                    { 410001, 7, 2, 4, null, "checkpoint-natureza-e2", "Resolva as questões 1 a 9 de uma prova antiga da Etapa 2 com o relógio ligado: cerca de 6 minutos por questão.", "Checkpoint: questões de Ciências da Natureza", 5 },
                    { 410002, 6, 2, 9, null, "checkpoint-humanas-e2", "Resolva as questões 10 a 18 de uma prova antiga da Etapa 2. Leia a pergunta antes do texto-base.", "Checkpoint: questões de Ciências Humanas", 5 },
                    { 410003, 8, 2, 11, null, "checkpoint-matematica-e2", "Resolva as questões 19 a 23 de uma prova antiga da Etapa 2. Na dúvida entre chutar e marcar \"Ignoro a resposta\", lembre que o erro desconta 10% de um acerto.", "Checkpoint: questões de Matemática", 5 },
                    { 410004, 5, 2, 17, null, "checkpoint-simulado-pave-2", "Faça um simulado de 32 questões em 3h30, no horário da prova (início às 13h15). Depois, revise o caderno de erros até o dia do PAVE.", "Checkpoint final: simulado completo", 5 },
                    { 500100, 7, 0, 1, null, "biologia", "Genética e biotecnologia, mais os dois temas comuns a todas as etapas.", "Biologia", 6 },
                    { 500200, 7, 0, 2, null, "fisica", "Eletricidade e magnetismo, do choque estático ao gerador de energia.", "Física", 6 },
                    { 500300, 7, 0, 3, null, "quimica", "Química orgânica: reconhecer a função pela estrutura e ligar a molécula ao uso dela.", "Química", 6 },
                    { 500400, 6, 0, 5, null, "historia", "O século XX e o tempo presente, no mundo e no Brasil.", "História", 6 },
                    { 500500, 6, 0, 6, null, "geografia", "Indústria, cidade e população: onde as pessoas vivem e trabalham.", "Geografia", 6 },
                    { 500600, 6, 0, 7, null, "filosofia", "Filosofia política e estética.", "Filosofia", 6 },
                    { 500700, 6, 0, 8, null, "sociologia", "Poder, raça, trabalho e gênero na sociedade brasileira.", "Sociologia", 6 },
                    { 500800, 8, 0, 10, null, "matematica", "Contagem, probabilidade, matrizes e geometria analítica, além da matemática financeira do dia a dia.", "Matemática", 6 },
                    { 500900, 5, 0, 12, null, "lingua-portuguesa", "O texto como um todo: gêneros, construção de sentido e norma culta. É também a base da redação.", "Língua Portuguesa", 6 },
                    { 501000, 5, 0, 13, null, "literatura", "Do Parnasianismo aos autores de hoje: a literatura brasileira do século XX e XXI.", "Literatura", 6 },
                    { 501100, 5, 0, 14, null, "arte", "Arte brasileira e a relação entre arte e tecnologia.", "Arte", 6 },
                    { 501200, 5, 0, 15, null, "ingles", "As três últimas questões da prova são de língua estrangeira, e você escolhe entre espanhol e inglês. O que se cobra é leitura, não gramática decorada.", "Língua inglesa", 6 },
                    { 501300, 5, 0, 16, null, "espanhol", "As três últimas questões da prova são de língua estrangeira, e você escolhe entre espanhol e inglês. O que se cobra é leitura, não gramática decorada.", "Língua espanhola", 6 },
                    { 501400, 9, 0, 18, null, "redacao", "Só a Etapa 3 tem redação: um texto dissertativo-argumentativo de 16 a 30 linhas, com título, que vale até 14 pontos.", "Redação", 6 },
                    { 510001, 7, 2, 4, null, "checkpoint-natureza-e3", "Resolva as questões 1 a 9 de uma prova antiga da Etapa 3 com o relógio ligado: cerca de 6 minutos por questão.", "Checkpoint: questões de Ciências da Natureza", 6 },
                    { 510002, 6, 2, 9, null, "checkpoint-humanas-e3", "Resolva as questões 10 a 18 de uma prova antiga da Etapa 3. Leia a pergunta antes do texto-base.", "Checkpoint: questões de Ciências Humanas", 6 },
                    { 510003, 8, 2, 11, null, "checkpoint-matematica-e3", "Resolva as questões 19 a 23 de uma prova antiga da Etapa 3. Na dúvida entre chutar e marcar \"Ignoro a resposta\", lembre que o erro desconta 10% de um acerto.", "Checkpoint: questões de Matemática", 6 },
                    { 510004, 5, 2, 17, null, "checkpoint-linguagens-e3", "Resolva as questões 24 a 32 de uma prova antiga da Etapa 3 e deixe pelo menos 1 hora da prova para a redação.", "Checkpoint: questões de Linguagens", 6 },
                    { 510005, 9, 2, 19, null, "checkpoint-simulado-pave-3", "Faça um simulado de 32 questões e escreva uma redação, tudo em 4 horas, no horário da prova (início às 13h15). Depois, revise o caderno de erros até o dia do PAVE.", "Checkpoint final: simulado e redação", 6 },
                    { 300101, 7, 1, 1, 300100, "origem-da-vida", "Abiogênese e biogênese, e o contexto histórico de cada explicação.", "Teorias de origem da vida", 4 },
                    { 300102, 7, 1, 2, 300100, "teorias-evolucao", "Lamarck e Darwin, seleção natural e a evolução humana.", "Teorias da evolução", 4 },
                    { 300103, 7, 1, 3, 300100, "celula-organelas", "Procariontes e eucariontes, função de cada organela, osmose, difusão e transporte ativo.", "Célula, organelas e membrana", 4 },
                    { 300104, 7, 1, 4, 300100, "metabolismo-energetico", "Fotossíntese, respiração, fermentação e quimiossíntese: etapas, produtos e quem faz cada uma.", "Metabolismo energético", 4 },
                    { 300105, 7, 1, 5, 300100, "divisao-celular", "Mitose e meiose: etapas e o papel de cada uma no crescimento e na formação de gametas.", "Divisão celular", 4 },
                    { 300106, 7, 1, 6, 300100, "histologia", "Tecidos epitelial, conjuntivo, muscular e nervoso: características e funções.", "Histologia", 4 },
                    { 300107, 7, 1, 7, 300100, "embriologia-reproducao", "Do zigoto à gástrula; reprodução sexuada e assexuada, com vantagens e desvantagens.", "Embriologia e reprodução", 4 },
                    { 300108, 7, 1, 8, 300100, "ecossistemas-impactos-e1", "Fluxo de energia, ciclos da água, do carbono e do nitrogênio, relações ecológicas, biomas brasileiros, poluição e mudanças climáticas. Tema comum às três etapas.", "Ecossistemas e impactos ambientais", 4 },
                    { 300109, 7, 1, 9, 300100, "classificacao-biologica-e1", "Categorias taxonômicas, os três domínios da vida, árvores filogenéticas e evolução humana; darwinismo social, eugenia e racismo \"científico\". Tema comum às três etapas.", "Diversidade e classificação biológica", 4 },
                    { 300201, 7, 1, 1, 300200, "grandezas-unidades", "Ordem de grandeza, notação científica, grandezas escalares e vetoriais e o Sistema Internacional.", "Grandezas e unidades", 4 },
                    { 300202, 7, 1, 2, 300200, "cinematica", "Velocidade e aceleração, movimento uniforme e uniformemente variado, movimento circular e queda livre.", "Cinemática", 4 },
                    { 300203, 7, 1, 3, 300200, "leis-de-newton", "Força resultante, peso, normal, tração, elástica, atrito e centrípeta.", "Leis de Newton e forças", 4 },
                    { 300204, 7, 1, 4, 300200, "trabalho-energia", "Energia cinética e potencial, conservação da energia mecânica e forças não conservativas.", "Trabalho, energia e potência", 4 },
                    { 300205, 7, 1, 5, 300200, "impulso-quantidade-movimento", "Colisões e o princípio da conservação da quantidade de movimento.", "Impulso e quantidade de movimento", 4 },
                    { 300206, 7, 1, 6, 300200, "estatica", "Equilíbrio do ponto material e do corpo extenso, momento de uma força e centro de gravidade.", "Estática", 4 },
                    { 300301, 7, 1, 1, 300300, "materia-misturas", "Propriedades da matéria, substâncias simples e compostas, misturas e métodos de separação.", "Matéria, substâncias e misturas", 4 },
                    { 300302, 7, 1, 2, 300300, "atomistica-tabela-periodica", "Modelos atômicos, número atômico e de massa, íons, isótopos e propriedades periódicas.", "Atomística e tabela periódica", 4 },
                    { 300303, 7, 1, 3, 300300, "ligacoes-quimicas", "Ligação iônica, covalente e metálica, geometria, polaridade e forças intermoleculares.", "Ligações e interações", 4 },
                    { 300304, 7, 1, 4, 300300, "funcoes-inorganicas", "Ácidos, bases, sais e óxidos: nomenclatura, propriedades e reações no ar, no solo e na água.", "Funções inorgânicas e reações", 4 },
                    { 300305, 7, 1, 5, 300300, "calculos-quimicos", "Conservação da massa, estequiometria, rendimento, pureza e reagente limitante.", "Cálculos químicos", 4 },
                    { 300401, 6, 1, 1, 300400, "introducao-historia", "Fontes, tempo, periodizações e disputas pela memória.", "Introdução à História", 4 },
                    { 300402, 6, 1, 2, 300400, "pre-historia", "A África e a origem humana, Paleolítico e Neolítico, primeiras cidades e metalurgia.", "Pré-História", 4 },
                    { 300403, 6, 1, 3, 300400, "mundo-antigo", "Egito, Mesopotâmia, Grécia e Roma, e a crise do mundo antigo.", "Mundo Antigo", 4 },
                    { 300404, 6, 1, 4, 300400, "mundo-medieval", "Impérios Bizantino e Islâmico, Idade Média ocidental e feudalismo.", "Mundo Medieval", 4 },
                    { 300405, 6, 1, 5, 300400, "mundo-moderno-capitalismo", "Renascimento, Grandes Navegações, reformas religiosas, absolutismo, mercantilismo e Iluminismo.", "Mundo Moderno e formação do capitalismo", 4 },
                    { 300501, 6, 1, 1, 300500, "orientacao-cartografia", "Movimentos da Terra, coordenadas, fusos horários, escalas, projeções e geotecnologias.", "Orientação e cartografia", 4 },
                    { 300502, 6, 1, 2, 300500, "estrutura-relevo-solos", "Tectônica de placas, rochas, formação e degradação dos solos, agentes do relevo.", "Estrutura da Terra, relevo e solos", 4 },
                    { 300503, 6, 1, 3, 300500, "clima-biomas", "Dinâmica atmosférica, tipos de clima, eventos extremos, biomas e os domínios brasileiros.", "Clima, biomas e domínios morfoclimáticos", 4 },
                    { 300504, 6, 1, 4, 300500, "agua-problemas-ambientais", "A água no Brasil e no mundo e os impactos da ação humana.", "Recursos hídricos e problemas ambientais", 4 },
                    { 300601, 6, 1, 1, 300600, "introducao-filosofia", "Origens da filosofia, do que ela trata e o que é a reflexão filosófica.", "Introdução à Filosofia", 4 },
                    { 300602, 6, 1, 2, 300600, "filosofia-antiga", "Pré-socráticos, a maiêutica de Sócrates, a alegoria da caverna de Platão e a ética e a lógica de Aristóteles.", "Antiguidade clássica", 4 },
                    { 300603, 6, 1, 3, 300600, "filosofia-medieval", "Fé e razão na Europa cristã e as Confissões de Agostinho.", "Filosofia medieval", 4 },
                    { 300604, 6, 1, 4, 300600, "filosofia-moderna", "O cogito de Descartes e os limites do conhecimento em Kant.", "Filosofia moderna", 4 },
                    { 300701, 6, 1, 1, 300700, "classicos-sociologia", "Iluminismo e revoluções; os conceitos-chave de Marx, Weber e Durkheim.", "Surgimento da Sociologia e os clássicos", 4 },
                    { 300702, 6, 1, 2, 300700, "status-papeis-sociais", "Interação, representação social, cooperação e conflito.", "Status, papéis e interação social", 4 },
                    { 300703, 6, 1, 3, 300700, "estratificacao-mobilidade", "Classes sociais, desigualdades econômicas e políticas e mobilidade.", "Estratificação e mobilidade social", 4 },
                    { 300704, 6, 1, 4, 300700, "cultura-identidade", "Conceito de cultura, etnocentrismo e relativismo, cultura de massa e indústria cultural.", "Cultura e identidade", 4 },
                    { 300801, 8, 1, 1, 300800, "conjuntos-intervalos", "Operações com números reais, diagramas de Venn e operações com intervalos.", "Conjuntos numéricos e intervalos", 4 },
                    { 300802, 8, 1, 2, 300800, "funcoes-conceitos", "Domínio, imagem, raízes, crescimento, função composta e inversa, leitura de gráficos e tabelas.", "Funções: conceitos", 4 },
                    { 300803, 8, 1, 3, 300800, "funcoes-elementares", "Gráfico, raízes, vértice, crescimento exponencial e propriedades dos logaritmos.", "Funções afim, quadrática, exponencial e logarítmica", 4 },
                    { 300804, 8, 1, 4, 300800, "progressao-aritmetica", "Termo geral e soma dos termos.", "Progressão aritmética", 4 },
                    { 300805, 8, 1, 5, 300800, "progressao-geometrica", "Termo geral e soma dos termos da PG finita e da infinita.", "Progressão geométrica", 4 },
                    { 300806, 8, 1, 6, 300800, "estatistica", "Gráficos, média, mediana, moda, variância e desvio padrão.", "Estatística", 4 },
                    { 300901, 5, 1, 1, 300900, "linguagem-figuras", "Discurso formal e informal, denotação e conotação, figuras e vícios de linguagem.", "Linguagem, sentido e figuras", 4 },
                    { 300902, 5, 1, 2, 300900, "variacao-linguistica-pave", "Sotaque, dialeto e idioleto, sem preconceito linguístico.", "Variação linguística", 4 },
                    { 300903, 5, 1, 3, 300900, "formacao-palavras", "Radical, desinências, vogal temática e afixos.", "Estrutura e formação das palavras", 4 },
                    { 300904, 5, 1, 4, 300900, "classes-variaveis", "Substantivo, verbo, pronome, artigo e adjetivo: flexão, concordância, regência e colocação pronominal.", "Classes variáveis", 4 },
                    { 300905, 5, 1, 5, 300900, "classes-invariaveis", "Advérbio, preposição e conjunção, e os operadores argumentativos.", "Classes invariáveis", 4 },
                    { 301001, 5, 1, 1, 301000, "introducao-literatura", "Funções da literatura, gêneros lírico, narrativo e dramático, intertextualidade.", "Introdução aos estudos literários", 4 },
                    { 301002, 5, 1, 2, 301000, "trovadorismo-classicismo", "As cantigas medievais, o teatro de Gil Vicente e Camões (\"Os Lusíadas\").", "Trovadorismo, Humanismo e Classicismo", 4 },
                    { 301003, 5, 1, 3, 301000, "literatura-informacao", "Os textos sobre o Brasil do século XVI, como a carta de Caminha.", "Literatura de Informação", 4 },
                    { 301004, 5, 1, 4, 301000, "leitura-literaria-e1", "Interpretar textos clássicos e contemporâneos e compará-los com outras linguagens.", "Leitura literária", 4 },
                    { 301101, 5, 1, 1, 301100, "linguagem-visual", "Ponto, linha, forma, cor, textura e volume; equilíbrio, ritmo, proporção e contraste.", "Elementos e organização da linguagem visual", 4 },
                    { 301102, 5, 1, 2, 301100, "contextos-praticas-artisticas", "Função social da arte, modalidades das artes visuais e manifestações locais e regionais.", "Contextos e práticas artísticas", 4 },
                    { 301201, 5, 1, 1, 301200, "ingles-generos-estrutura-e1", "Reconhecer o gênero (notícia, tirinha, anúncio, artigo), as partes do texto e como elas se ligam.", "Gêneros e estrutura do texto", 4 },
                    { 301202, 5, 1, 2, 301200, "ingles-vocabulario-contexto-e1", "Ideia principal e secundária, o que está explícito e implícito, cognatos e falsos cognatos.", "Palavras no texto e no contexto", 4 },
                    { 301203, 5, 1, 3, 301200, "ingles-objetivos-texto-e1", "Quem escreve, para quem e para quê; pronomes e conectivos que retomam e ligam as ideias.", "Objetivos do texto", 4 },
                    { 301301, 5, 1, 1, 301300, "espanhol-generos-estrutura-e1", "Reconhecer o gênero (notícia, tirinha, anúncio, artigo), as partes do texto e como elas se ligam.", "Gêneros e estrutura do texto", 4 },
                    { 301302, 5, 1, 2, 301300, "espanhol-vocabulario-contexto-e1", "Ideia principal e secundária, o que está explícito e implícito, cognatos e falsos cognatos.", "Palavras no texto e no contexto", 4 },
                    { 301303, 5, 1, 3, 301300, "espanhol-objetivos-texto-e1", "Quem escreve, para quem e para quê; pronomes e conectivos que retomam e ligam as ideias.", "Objetivos do texto", 4 },
                    { 400101, 7, 1, 1, 400100, "saude-idh", "Doenças e transtornos que afetam a população, vacinas, saúde mental e a relação entre desigualdade e vulnerabilidade.", "Saúde e Índice de Desenvolvimento Humano", 5 },
                    { 400102, 7, 1, 2, 400100, "ecossistemas-impactos-e2", "Fluxo de energia, ciclos da água, do carbono e do nitrogênio, relações ecológicas, biomas brasileiros, poluição e mudanças climáticas. Tema comum às três etapas.", "Ecossistemas e impactos ambientais", 5 },
                    { 400103, 7, 1, 3, 400100, "classificacao-biologica-e2", "Categorias taxonômicas, os três domínios da vida, árvores filogenéticas e evolução humana; darwinismo social, eugenia e racismo \"científico\". Tema comum às três etapas.", "Diversidade e classificação biológica", 5 },
                    { 400201, 7, 1, 1, 400200, "termometria-dilatacao", "Temperatura, escalas Celsius, Fahrenheit e Kelvin, dilatação de sólidos e líquidos e a água.", "Termometria e dilatação térmica", 5 },
                    { 400202, 7, 1, 2, 400200, "calorimetria", "Calor sensível e latente, calor específico, mudanças de estado, condução, convecção e irradiação.", "Calorimetria", 5 },
                    { 400203, 7, 1, 3, 400200, "optica-geometrica", "Reflexão e refração, espelhos planos e esféricos, lentes e a óptica da visão.", "Óptica geométrica", 5 },
                    { 400204, 7, 1, 4, 400200, "ondulatoria", "Amplitude, frequência e comprimento de onda, fenômenos ondulatórios, som e efeito Doppler.", "Ondulatória", 5 },
                    { 400301, 7, 1, 1, 400300, "solucoes", "Soluções e coloides, solubilidade, unidades de concentração e diluição.", "Soluções", 5 },
                    { 400302, 7, 1, 2, 400300, "termoquimica", "Calor de reação, entalpia e Lei de Hess.", "Termoquímica", 5 },
                    { 400303, 7, 1, 3, 400300, "cinetica-quimica", "Rapidez de reação, fatores que a alteram e catálise.", "Cinética química", 5 },
                    { 400304, 7, 1, 4, 400300, "equilibrio-quimico", "Constante de equilíbrio, deslocamento (Le Chatelier), pH, pOH e hidrólise de sais.", "Equilíbrio químico", 5 },
                    { 400305, 7, 1, 5, 400300, "eletroquimica", "Oxirredução, pilhas e baterias, eletrólise ígnea e em meio aquoso.", "Eletroquímica", 5 },
                    { 400306, 7, 1, 6, 400300, "materiais-sustentabilidade", "Viabilidade dos processos químicos e a escolha de materiais pensando no impacto ambiental.", "Materiais, processos e ambiente", 5 },
                    { 400401, 6, 1, 1, 400400, "america-africa-colonizacao", "Os mundos indígenas e africanos, as invasões europeias e os sistemas coloniais.", "África Atlântica, povos indígenas e colonização da América", 5 },
                    { 400402, 6, 1, 2, 400400, "brasil-colonial", "Economia, escravidão, resistência africana e indígena e a crise do sistema colonial.", "Brasil colonial", 5 },
                    { 400403, 6, 1, 3, 400400, "rio-grande-do-sul-colonial-imperial", "A formação do Rio Grande do Sul na colônia e no Império.", "Rio Grande do Sul colonial e imperial", 5 },
                    { 400404, 6, 1, 4, 400400, "era-das-revolucoes", "Revolução Industrial, Revolução Americana e Revolução Francesa.", "A era das revoluções", 5 },
                    { 400405, 6, 1, 5, 400400, "brasil-imperio-pave", "Independência, os dois reinados, a crise do escravismo e o movimento republicano.", "Brasil Império", 5 },
                    { 400406, 6, 1, 6, 400400, "nacionalismos-imperialismos", "Nacionalismo e unificações na Europa e os imperialismos do século XIX.", "Nacionalismos e imperialismos", 5 },
                    { 400501, 6, 1, 1, 400500, "espaco-mundial-geopolitica", "Estado-nação, fronteiras, ONU, blocos econômicos e conflitos territoriais.", "Espaço mundial e geopolítica", 5 },
                    { 400502, 6, 1, 2, 400500, "globalizacao", "Empresas globais, fluxos de dinheiro, mercadorias e informação, redes de transporte e diversidade cultural.", "Globalização", 5 },
                    { 400503, 6, 1, 3, 400500, "espaco-rural", "Sistemas agrícolas, agricultura familiar, estrutura fundiária, reforma agrária e territórios indígenas e quilombolas.", "Produção do espaço rural", 5 },
                    { 400601, 6, 1, 1, 400600, "logica", "O que é um argumento, lógica dedutiva e lógica indutiva.", "Lógica", 5 },
                    { 400602, 6, 1, 2, 400600, "etica", "Ética e moral, e os dilemas éticos de hoje nas relações e nas profissões.", "Ética", 5 },
                    { 400603, 6, 1, 3, 400600, "filosofia-linguagem", "O que a linguagem faz, do debate contemporâneo às redes sociais.", "Filosofia da linguagem", 5 },
                    { 400701, 6, 1, 1, 400700, "sociologia-ambiental", "Sociedade de risco, produção e consumo, justiça ambiental, sociologia rural e urbana.", "Meio ambiente e sociologia ambiental", 5 },
                    { 400702, 6, 1, 2, 400700, "etnicidade-povos-originarios", "Povos indígenas e quilombolas hoje, direitos territoriais e políticas afirmativas.", "Etnicidade e povos originários", 5 },
                    { 400703, 6, 1, 3, 400700, "cultura-digital", "Redes, vigilância e dados, capitalismo de plataforma, algoritmos e inteligência artificial generativa.", "Cultura digital", 5 },
                    { 400801, 8, 1, 1, 400800, "triangulo-retangulo", "Relações métricas, Teorema de Pitágoras, seno, cosseno e tangente.", "Triângulo retângulo", 5 },
                    { 400802, 8, 1, 2, 400800, "trigonometria-pave", "Arcos e ângulos, círculo trigonométrico, identidades, lei dos senos e lei dos cossenos.", "Trigonometria", 5 },
                    { 400803, 8, 1, 3, 400800, "numeros-complexos", "Forma algébrica e trigonométrica, operações e o plano de Argand-Gauss.", "Números complexos", 5 },
                    { 400804, 8, 1, 4, 400800, "equacoes-algebricas", "Raízes, multiplicidade e relações entre coeficientes e raízes.", "Equações algébricas", 5 },
                    { 400805, 8, 1, 5, 400800, "geometria-plana-pave", "Unidades de medida, semelhança, polígonos regulares, círculo, perímetros e áreas.", "Geometria plana", 5 },
                    { 400806, 8, 1, 6, 400800, "geometria-espacial-pave", "Poliedros e o teorema de Euler, prismas, pirâmides, cilindros, cones e esferas.", "Geometria espacial", 5 },
                    { 400901, 5, 1, 1, 400900, "termos-da-oracao", "Termos essenciais, integrantes e acessórios; tipos de sujeito e de predicado.", "Termos da oração", 5 },
                    { 400902, 5, 1, 2, 400900, "periodo-simples", "Sujeito, predicado, complementos, deslocamentos e pontuação.", "Período simples", 5 },
                    { 400903, 5, 1, 3, 400900, "periodo-coordenacao", "As orações coordenadas e as relações de sentido entre elas.", "Período composto por coordenação", 5 },
                    { 400904, 5, 1, 4, 400900, "periodo-subordinacao", "Orações substantivas, adjetivas (explicativas e restritivas) e adverbiais.", "Período composto por subordinação", 5 },
                    { 400905, 5, 1, 5, 400900, "oracoes-reduzidas", "De infinitivo, de gerúndio e de particípio.", "Orações reduzidas", 5 },
                    { 401001, 5, 1, 1, 401000, "barroco-arcadismo", "Gregório de Matos e os sermões de Vieira; Tomás Antônio Gonzaga e Cláudio Manuel da Costa.", "Barroco e Arcadismo", 5 },
                    { 401002, 5, 1, 2, 401000, "romantismo", "Independência e identidade nacional; poesia lírica, indianista e ultrarromântica; Gonçalves Dias, Álvares de Azevedo e José de Alencar.", "Romantismo", 5 },
                    { 401003, 5, 1, 3, 401000, "realismo", "Contexto e características do movimento, em oposição ao idealismo romântico.", "Realismo", 5 },
                    { 401101, 5, 1, 1, 401100, "historia-da-arte", "Da arte pré-histórica ao Renascimento e aos movimentos modernos e contemporâneos.", "História da arte e movimentos artísticos", 5 },
                    { 401102, 5, 1, 2, 401100, "critica-estetica", "Como analisar uma obra e a relação entre arte, sociedade e política.", "Análise crítica e estética", 5 },
                    { 401201, 5, 1, 1, 401200, "ingles-generos-estrutura-e2", "Reconhecer o gênero (notícia, tirinha, anúncio, artigo), as partes do texto e como elas se ligam.", "Gêneros e estrutura do texto", 5 },
                    { 401202, 5, 1, 2, 401200, "ingles-vocabulario-contexto-e2", "Ideia principal e secundária, o que está explícito e implícito, cognatos e falsos cognatos.", "Palavras no texto e no contexto", 5 },
                    { 401203, 5, 1, 3, 401200, "ingles-objetivos-texto-e2", "Quem escreve, para quem e para quê; pronomes e conectivos que retomam e ligam as ideias.", "Objetivos do texto", 5 },
                    { 401301, 5, 1, 1, 401300, "espanhol-generos-estrutura-e2", "Reconhecer o gênero (notícia, tirinha, anúncio, artigo), as partes do texto e como elas se ligam.", "Gêneros e estrutura do texto", 5 },
                    { 401302, 5, 1, 2, 401300, "espanhol-vocabulario-contexto-e2", "Ideia principal e secundária, o que está explícito e implícito, cognatos e falsos cognatos.", "Palavras no texto e no contexto", 5 },
                    { 401303, 5, 1, 3, 401300, "espanhol-objetivos-texto-e2", "Quem escreve, para quem e para quê; pronomes e conectivos que retomam e ligam as ideias.", "Objetivos do texto", 5 },
                    { 500101, 7, 1, 1, 500100, "genetica", "Leis de Mendel, cruzamentos e probabilidade, padrões de herança e variabilidade genética.", "Genética", 6 },
                    { 500102, 7, 1, 2, 500100, "biotecnologia", "Clonagem, melhoramento genético, tecnologias do DNA, células-tronco, biorremediação e biossegurança.", "Biotecnologia e bioética", 6 },
                    { 500103, 7, 1, 3, 500100, "ecossistemas-impactos-e3", "Fluxo de energia, ciclos da água, do carbono e do nitrogênio, relações ecológicas, biomas brasileiros, poluição e mudanças climáticas. Tema comum às três etapas.", "Ecossistemas e impactos ambientais", 6 },
                    { 500104, 7, 1, 4, 500100, "classificacao-biologica-e3", "Categorias taxonômicas, os três domínios da vida, árvores filogenéticas e evolução humana; darwinismo social, eugenia e racismo \"científico\". Tema comum às três etapas.", "Diversidade e classificação biológica", 6 },
                    { 500201, 7, 1, 1, 500200, "eletrostatica", "Carga elétrica, eletrização, Lei de Coulomb, campo e potencial elétrico.", "Eletrostática", 6 },
                    { 500202, 7, 1, 2, 500200, "eletrodinamica", "Corrente, resistência, Leis de Ohm, potência, associação de resistores e circuitos simples.", "Eletrodinâmica", 6 },
                    { 500203, 7, 1, 3, 500200, "eletromagnetismo", "Ímãs, campo magnético de fios e solenoides, força magnética, indução (Faraday-Lenz) e espectro eletromagnético.", "Eletromagnetismo", 6 },
                    { 500301, 7, 1, 1, 500300, "compostos-organicos", "Hibridização do carbono, ligações entre carbonos e classificação das cadeias.", "Compostos orgânicos e cadeias carbônicas", 6 },
                    { 500302, 7, 1, 2, 500300, "funcoes-organicas", "Hidrocarbonetos, funções oxigenadas e nitrogenadas, haletos: nomenclatura e propriedades.", "Funções orgânicas", 6 },
                    { 500303, 7, 1, 3, 500300, "isomeria", "Isomeria plana e espacial, e por que ela importa em medicamentos.", "Isomeria", 6 },
                    { 500304, 7, 1, 4, 500300, "bioquimica", "Glicídios, lipídios, aminoácidos, proteínas e polímeros.", "Noções de bioquímica", 6 },
                    { 500305, 7, 1, 5, 500300, "quimica-organica-contextos", "Agricultura, saúde, medicamentos e as questões éticas e ambientais envolvidas.", "Química orgânica em contexto", 6 },
                    { 500306, 7, 1, 6, 500300, "radioatividade", "Emissões radioativas e tempo de meia-vida.", "Radioatividade", 6 },
                    { 500401, 6, 1, 1, 500400, "guerras-mundiais-entreguerras", "Primeira Guerra, Revolução Russa, crise de 1929, fascismo, nazismo e Segunda Guerra.", "Guerras mundiais e entreguerras", 6 },
                    { 500402, 6, 1, 2, 500400, "guerra-fria-descolonizacao", "A disputa entre os blocos, a independência de países da África e da Ásia, o Oriente Médio e a crise do Leste Europeu.", "Guerra Fria e descolonização", 6 },
                    { 500403, 6, 1, 3, 500400, "america-latina-mundo-contemporaneo", "Da América Latina do pós-guerra aos temas políticos e sociais de hoje.", "América Latina e mundo contemporâneo", 6 },
                    { 500404, 6, 1, 4, 500400, "primeira-republica", "Café e industrialização, movimentos sociais, tenentismo, modernismo e o Rio Grande do Sul.", "Primeira República", 6 },
                    { 500405, 6, 1, 5, 500400, "era-vargas-populismo", "Revolução de 1930, Estado Novo, movimento operário e os governos de 1945 a 1964.", "Era Vargas e período democrático", 6 },
                    { 500406, 6, 1, 6, 500400, "ditadura-redemocratizacao-pave", "Golpe de 1964, repressão e resistência, e a Constituição de 1988.", "Ditadura civil-militar e redemocratização", 6 },
                    { 500407, 6, 1, 7, 500400, "brasil-contemporaneo", "Democracia e desigualdades, problemas ambientais, racismo e antirracismo, culturas indígenas e afro-brasileiras.", "Brasil contemporâneo", 6 },
                    { 500501, 6, 1, 1, 500500, "industria-energia", "Revoluções industriais, distribuição da indústria, fontes de energia e a matriz energética brasileira.", "Produção industrial e energia", 6 },
                    { 500502, 6, 1, 2, 500500, "espaco-urbano", "Redes urbanas, metropolização, urbanização brasileira, segregação e direito à cidade.", "Produção do espaço urbano", 6 },
                    { 500503, 6, 1, 3, 500500, "populacao", "Indicadores e teorias demográficas, pirâmides etárias, migrações, trabalho e renda.", "População e espaço geográfico", 6 },
                    { 500601, 6, 1, 1, 500600, "filosofia-politica", "A formação do Estado moderno: Maquiavel, Hobbes, Locke, o liberalismo, os socialismos e o anarquismo.", "Política", 6 },
                    { 500602, 6, 1, 2, 500600, "estetica", "O belo e o feio, a questão do gosto, a atitude estética, indústria cultural e as funções da arte.", "Estética", 6 },
                    { 500701, 6, 1, 1, 500700, "poder-estado-democracia", "Teorias do poder, Estado e governo, cidadania, direitos humanos e movimentos sociais.", "Poder, Estado e democracia", 6 },
                    { 500702, 6, 1, 2, 500700, "relacoes-etnico-raciais", "Política de branqueamento, racismo e injúria racial, e a desvalorização da cultura afro-brasileira e indígena.", "Relações étnico-raciais", 6 },
                    { 500703, 6, 1, 3, 500700, "mundo-do-trabalho", "Terceirização, precarização e capitalismo de plataforma.", "Mundo do trabalho", 6 },
                    { 500704, 6, 1, 4, 500700, "interseccionalidade", "Papéis sociais de gênero, desigualdade econômica e violência de gênero.", "Interseccionalidade: gênero, raça e classe", 6 },
                    { 500801, 8, 1, 1, 500800, "analise-combinatoria-pave", "Princípio fundamental da contagem, arranjos, permutações e combinações.", "Análise combinatória", 6 },
                    { 500802, 8, 1, 2, 500800, "probabilidade-pave", "Eventos independentes, probabilidade condicional e da união de eventos.", "Probabilidade", 6 },
                    { 500803, 8, 1, 3, 500800, "matrizes-determinantes", "Tipos e operações com matrizes, matriz inversa e determinantes até ordem 3.", "Matrizes e determinantes", 6 },
                    { 500804, 8, 1, 4, 500800, "sistemas-lineares", "Classificação e resolução, com as matrizes associadas.", "Sistemas lineares", 6 },
                    { 500805, 8, 1, 5, 500800, "geometria-analitica", "Ponto, distância, equações da reta e da circunferência e posições relativas.", "Geometria analítica", 6 },
                    { 500806, 8, 1, 6, 500800, "matematica-financeira", "Regra de três, porcentagem, juros simples e compostos, descontos e educação financeira.", "Matemática financeira", 6 },
                    { 500901, 5, 1, 1, 500900, "tipos-generos-textuais", "Tipos de texto, gêneros da fala, da escrita e das mídias digitais.", "Tipos e gêneros textuais", 6 },
                    { 500902, 5, 1, 2, 500900, "textualidade", "Referenciação, organização de tópicos e parágrafos, discurso direto e indireto, pontuação e sentido.", "Textualidade: coesão e coerência", 6 },
                    { 500903, 5, 1, 3, 500900, "compreensao-leitura", "Inferência, dito e subentendido, ambiguidade e ironia.", "Compreensão da leitura", 6 },
                    { 500904, 5, 1, 4, 500900, "norma-culta", "Ler e escrever de acordo com a norma culta da língua.", "Adequação à norma culta", 6 },
                    { 501001, 5, 1, 1, 501000, "parnasianismo-simbolismo", "Olavo Bilac, Alberto de Oliveira e Raimundo Correia; Cruz e Sousa e Alphonsus de Guimaraens.", "Parnasianismo e Simbolismo", 6 },
                    { 501002, 5, 1, 2, 501000, "pre-modernismo", "Lima Barreto, Euclides da Cunha e Monteiro Lobato.", "Pré-Modernismo", 6 },
                    { 501003, 5, 1, 3, 501000, "modernismo-primeira-fase", "A Semana de Arte Moderna de 1922, Mário de Andrade, Oswald de Andrade e Manuel Bandeira.", "Modernismo: 1ª fase (1922-1930)", 6 },
                    { 501004, 5, 1, 4, 501000, "modernismo-segunda-fase", "A prosa de Graciliano Ramos, Jorge Amado e José Lins do Rego; a poesia de Drummond, Cecília Meireles e Vinícius de Moraes.", "Modernismo: 2ª fase (1930-1945)", 6 },
                    { 501005, 5, 1, 5, 501000, "modernismo-terceira-fase", "João Cabral de Melo Neto, Clarice Lispector, Guimarães Rosa e Lygia Fagundes Telles.", "Modernismo: 3ª fase (pós-1945)", 6 },
                    { 501006, 5, 1, 6, 501000, "literatura-contemporanea-pave", "Pluralidade de vozes e linguagens híbridas: Conceição Evaristo, Itamar Vieira Junior, Sérgio Vaz, Eliane Potiguara e Djamila Ribeiro.", "Literatura contemporânea", 6 },
                    { 501101, 5, 1, 1, 501100, "arte-brasileira", "Arte indígena e africana no Brasil, arte colonial e imperial, o Modernismo e a arte contemporânea.", "Arte brasileira e identidades culturais", 6 },
                    { 501102, 5, 1, 2, 501100, "arte-tecnologia", "Fotografia, cinema, arte digital e as linguagens híbridas.", "Arte e tecnologia", 6 },
                    { 501201, 5, 1, 1, 501200, "ingles-generos-estrutura-e3", "Reconhecer o gênero (notícia, tirinha, anúncio, artigo), as partes do texto e como elas se ligam.", "Gêneros e estrutura do texto", 6 },
                    { 501202, 5, 1, 2, 501200, "ingles-vocabulario-contexto-e3", "Ideia principal e secundária, o que está explícito e implícito, cognatos e falsos cognatos.", "Palavras no texto e no contexto", 6 },
                    { 501203, 5, 1, 3, 501200, "ingles-objetivos-texto-e3", "Quem escreve, para quem e para quê; pronomes e conectivos que retomam e ligam as ideias.", "Objetivos do texto", 6 },
                    { 501301, 5, 1, 1, 501300, "espanhol-generos-estrutura-e3", "Reconhecer o gênero (notícia, tirinha, anúncio, artigo), as partes do texto e como elas se ligam.", "Gêneros e estrutura do texto", 6 },
                    { 501302, 5, 1, 2, 501300, "espanhol-vocabulario-contexto-e3", "Ideia principal e secundária, o que está explícito e implícito, cognatos e falsos cognatos.", "Palavras no texto e no contexto", 6 },
                    { 501303, 5, 1, 3, 501300, "espanhol-objetivos-texto-e3", "Quem escreve, para quem e para quê; pronomes e conectivos que retomam e ligam as ideias.", "Objetivos do texto", 6 },
                    { 501401, 9, 1, 1, 501400, "proposta-coletanea", "Ler a proposta e os textos de apoio, sem fugir do tema e sem copiar trechos da coletânea.", "Tema e coletânea", 6 },
                    { 501402, 9, 1, 2, 501400, "estrutura-dissertativa", "Título, tese, argumentos e conclusão. Menos de 16 linhas ou outro tipo de texto zera a redação.", "Tipo de texto: a estrutura dissertativo-argumentativa", 6 },
                    { 501403, 9, 1, 3, 501400, "coerencia-coesao-redacao", "Ideias que progridem sem contradição, ligadas por conectivos e retomadas.", "Coerência e coesão", 6 },
                    { 501404, 9, 1, 4, 501400, "modalidade-escrita-formal", "Domínio da modalidade escrita formal da língua portuguesa.", "Modalidade escrita formal", 6 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 300101);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 300102);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 300103);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 300104);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 300105);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 300106);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 300107);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 300108);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 300109);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 300201);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 300202);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 300203);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 300204);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 300205);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 300206);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 300301);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 300302);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 300303);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 300304);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 300305);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 300401);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 300402);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 300403);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 300404);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 300405);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 300501);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 300502);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 300503);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 300504);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 300601);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 300602);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 300603);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 300604);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 300701);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 300702);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 300703);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 300704);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 300801);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 300802);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 300803);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 300804);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 300805);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 300806);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 300901);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 300902);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 300903);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 300904);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 300905);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 301001);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 301002);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 301003);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 301004);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 301101);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 301102);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 301201);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 301202);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 301203);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 301301);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 301302);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 301303);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 310001);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 310002);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 310003);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 310004);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 400101);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 400102);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 400103);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 400201);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 400202);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 400203);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 400204);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 400301);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 400302);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 400303);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 400304);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 400305);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 400306);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 400401);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 400402);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 400403);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 400404);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 400405);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 400406);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 400501);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 400502);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 400503);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 400601);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 400602);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 400603);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 400701);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 400702);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 400703);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 400801);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 400802);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 400803);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 400804);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 400805);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 400806);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 400901);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 400902);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 400903);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 400904);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 400905);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 401001);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 401002);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 401003);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 401101);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 401102);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 401201);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 401202);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 401203);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 401301);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 401302);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 401303);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 410001);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 410002);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 410003);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 410004);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 500101);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 500102);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 500103);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 500104);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 500201);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 500202);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 500203);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 500301);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 500302);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 500303);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 500304);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 500305);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 500306);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 500401);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 500402);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 500403);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 500404);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 500405);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 500406);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 500407);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 500501);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 500502);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 500503);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 500601);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 500602);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 500701);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 500702);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 500703);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 500704);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 500801);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 500802);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 500803);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 500804);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 500805);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 500806);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 500901);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 500902);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 500903);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 500904);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 501001);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 501002);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 501003);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 501004);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 501005);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 501006);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 501101);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 501102);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 501201);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 501202);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 501203);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 501301);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 501302);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 501303);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 501401);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 501402);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 501403);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 501404);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 510001);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 510002);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 510003);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 510004);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 510005);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 300100);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 300200);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 300300);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 300400);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 300500);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 300600);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 300700);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 300800);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 300900);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 301000);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 301100);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 301200);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 301300);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 400100);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 400200);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 400300);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 400400);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 400500);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 400600);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 400700);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 400800);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 400900);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 401000);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 401100);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 401200);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 401300);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 500100);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 500200);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 500300);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 500400);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 500500);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 500600);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 500700);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 500800);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 500900);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 501000);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 501100);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 501200);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 501300);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 501400);

            migrationBuilder.DeleteData(
                table: "Tracks",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Tracks",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Tracks",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DropColumn(
                name: "IgnoredCount",
                table: "MockExams");

            migrationBuilder.DropColumn(
                name: "IgnoredAnswer",
                table: "MockExamQuestions");
        }
    }
}
