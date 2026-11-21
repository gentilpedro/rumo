using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EnadeAds.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIbgeAndUfsmTracks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Tracks",
                columns: new[] { "Id", "Basis", "ExamDate", "ExamName", "Label", "Level", "MockExamStyle", "Name", "Order", "SecondExamDate", "Slug" },
                values: new object[,]
                {
                    { 7, "O conteúdo do Edital IBGE nº 03/2026 para Recenseador do 12º Censo Agro: Português, Matemática e a apostila oficial de Conhecimentos Técnicos.", new DateOnly(2027, 1, 31), "IBGE", "IBGE Recenseador", 3, 3, "IBGE 2027 · Recenseador", 7, null, "ibge-recenseador" },
                    { 8, "As habilidades do 1º ano do ensino médio no Caderno de Habilidades da UFSM, que valem para a Prova 1 do vestibular e para o PSS1.", new DateOnly(2027, 1, 9), "UFSM", "UFSM Prova 1", 1, 3, "UFSM 2027 · Prova 1 (PSS1)", 8, null, "ufsm-1" },
                    { 9, "As habilidades do 2º ano do ensino médio no Caderno de Habilidades da UFSM, que valem para a Prova 2 do vestibular e para o PSS2.", new DateOnly(2027, 1, 10), "UFSM", "UFSM Prova 2", 1, 3, "UFSM 2027 · Prova 2 (PSS2)", 9, null, "ufsm-2" },
                    { 10, "As habilidades do 3º ano do ensino médio e a redação no Caderno de Habilidades da UFSM, que valem para a Prova 3 do vestibular e para o PSS3.", new DateOnly(2027, 1, 10), "UFSM", "UFSM Prova 3", 1, 3, "UFSM 2027 · Prova 3 (PSS3)", 10, null, "ufsm-3" }
                });

            migrationBuilder.InsertData(
                table: "RoadmapNodes",
                columns: new[] { "Id", "Block", "Kind", "Order", "ParentId", "Slug", "Summary", "Title", "TrackId" },
                values: new object[,]
                {
                    { 600100, 6, 0, 1, null, "filosofia-sociologia", "São 2 questões: em geral uma de Filosofia e uma de Sociologia, ligadas ao tema da prova.", "Filosofia e Sociologia", 8 },
                    { 600200, 6, 0, 2, null, "geografia", "São 4 questões, quase sempre sobre ambiente e natureza do Brasil e do Rio Grande do Sul.", "Geografia", 8 },
                    { 600300, 6, 0, 3, null, "historia", "São 4 questões, da Pré-História ao mundo colonial, sempre com um texto ou fonte para ler.", "História", 8 },
                    { 600400, 7, 0, 5, null, "quimica", "São 4 questões, quase sempre de verdadeiro ou falso: átomo, tabela periódica, ligações e reações.", "Química", 8 },
                    { 600500, 7, 0, 6, null, "fisica", "São 4 questões de mecânica e gravitação, em contextos de esporte e cotidiano.", "Física", 8 },
                    { 600600, 7, 0, 7, null, "biologia", "São 4 questões, da célula à saúde, muitas a partir de uma notícia ou gráfico.", "Biologia", 8 },
                    { 600700, 8, 0, 9, null, "matematica", "São 5 questões: funções, porcentagem e juros, e leitura de gráficos.", "Matemática", 8 },
                    { 600800, 5, 0, 11, null, "artes-educacao-fisica", "São 3 questões de Artes Integradas e Educação Física: linguagens artísticas e corpo e saúde.", "Artes e Educação Física", 8 },
                    { 600900, 5, 0, 12, null, "lingua-portuguesa", "São 5 questões: leitura de texto, coesão e o sentido das palavras.", "Língua Portuguesa", 8 },
                    { 601000, 5, 0, 13, null, "literatura", "São 2 questões. Não há lista de leituras: a Prova 1 cobra os movimentos até o Arcadismo.", "Literatura", 8 },
                    { 601100, 5, 0, 14, null, "espanhol", "As três últimas questões são de língua estrangeira, sobre um texto ligado ao tema da prova. É leitura, não gramática decorada.", "Língua espanhola", 8 },
                    { 601200, 5, 0, 15, null, "ingles", "As três últimas questões são de língua estrangeira, sobre um texto ligado ao tema da prova. É leitura, não gramática decorada.", "Língua inglesa", 8 },
                    { 610001, 6, 2, 4, null, "checkpoint-humanas-p1", "Resolva as questões de Ciências Humanas de uma prova antiga da Prova 1, com o relógio ligado. São 40 questões em 3 horas: cerca de 4 minutos e meio por questão.", "Checkpoint: questões de Ciências Humanas", 8 },
                    { 610002, 7, 2, 8, null, "checkpoint-natureza-p1", "Resolva as questões de Ciências da Natureza de uma prova antiga da Prova 1, com o relógio ligado. São 40 questões em 3 horas: cerca de 4 minutos e meio por questão.", "Checkpoint: questões de Ciências da Natureza", 8 },
                    { 610003, 8, 2, 10, null, "checkpoint-matematica-p1", "Resolva as questões de Matemática de uma prova antiga da Prova 1, com o relógio ligado. São 40 questões em 3 horas: cerca de 4 minutos e meio por questão.", "Checkpoint: questões de Matemática", 8 },
                    { 610004, 5, 2, 16, null, "checkpoint-linguagens-p1", "Resolva as questões de Linguagens de uma prova antiga da Prova 1, com o relógio ligado. São 40 questões em 3 horas: cerca de 4 minutos e meio por questão.", "Checkpoint: questões de Linguagens", 8 },
                    { 610005, 5, 2, 17, null, "checkpoint-simulado-p1", "Faça um simulado de 40 questões em 3 horas, no horário da prova: 09/01, às 13h30. Depois, revise o caderno de erros até o dia da prova.", "Checkpoint final: simulado completo", 8 },
                    { 700100, 6, 0, 1, null, "filosofia-sociologia", "São 2 questões: em geral uma de Filosofia e uma de Sociologia, ligadas ao tema da prova.", "Filosofia e Sociologia", 9 },
                    { 700200, 6, 0, 2, null, "geografia", "São 4 questões sobre energia, clima, migrações e globalização.", "Geografia", 9 },
                    { 700300, 6, 0, 3, null, "historia", "São 4 questões, da Idade Moderna ao Brasil do século XX.", "História", 9 },
                    { 700400, 7, 0, 5, null, "quimica", "São 4 questões de físico-química, e a estequiometria aparece todo ano.", "Química", 9 },
                    { 700500, 7, 0, 6, null, "fisica", "São 4 questões de fluidos, calor e ondas.", "Física", 9 },
                    { 700600, 7, 0, 7, null, "biologia", "São 4 questões sobre seres vivos, plantas e ecologia.", "Biologia", 9 },
                    { 700700, 8, 0, 9, null, "matematica", "São 5 questões: trigonometria, contagem, probabilidade, progressões e estatística.", "Matemática", 9 },
                    { 700800, 5, 0, 11, null, "artes-educacao-fisica", "São 3 questões de Artes Integradas e Educação Física: arte moderna e contemporânea, teatro e corpo.", "Artes e Educação Física", 9 },
                    { 700900, 5, 0, 12, null, "lingua-portuguesa", "São 5 questões: gramática dentro do texto e leitura de textos multimodais.", "Língua Portuguesa", 9 },
                    { 701000, 5, 0, 13, null, "literatura", "São 2 questões. Não há lista de leituras: a Prova 2 cobra Romantismo e Realismo.", "Literatura", 9 },
                    { 701100, 5, 0, 14, null, "espanhol", "As três últimas questões são de língua estrangeira, sobre um texto ligado ao tema da prova. É leitura, não gramática decorada.", "Língua espanhola", 9 },
                    { 701200, 5, 0, 15, null, "ingles", "As três últimas questões são de língua estrangeira, sobre um texto ligado ao tema da prova. É leitura, não gramática decorada.", "Língua inglesa", 9 },
                    { 710001, 6, 2, 4, null, "checkpoint-humanas-p2", "Resolva as questões de Ciências Humanas de uma prova antiga da Prova 2, com o relógio ligado. São 40 questões em 3 horas: cerca de 4 minutos e meio por questão.", "Checkpoint: questões de Ciências Humanas", 9 },
                    { 710002, 7, 2, 8, null, "checkpoint-natureza-p2", "Resolva as questões de Ciências da Natureza de uma prova antiga da Prova 2, com o relógio ligado. São 40 questões em 3 horas: cerca de 4 minutos e meio por questão.", "Checkpoint: questões de Ciências da Natureza", 9 },
                    { 710003, 8, 2, 10, null, "checkpoint-matematica-p2", "Resolva as questões de Matemática de uma prova antiga da Prova 2, com o relógio ligado. São 40 questões em 3 horas: cerca de 4 minutos e meio por questão.", "Checkpoint: questões de Matemática", 9 },
                    { 710004, 5, 2, 16, null, "checkpoint-linguagens-p2", "Resolva as questões de Linguagens de uma prova antiga da Prova 2, com o relógio ligado. São 40 questões em 3 horas: cerca de 4 minutos e meio por questão.", "Checkpoint: questões de Linguagens", 9 },
                    { 710005, 5, 2, 17, null, "checkpoint-simulado-p2", "Faça um simulado de 40 questões em 3 horas, no horário da prova: 10/01, às 8h. Depois, revise o caderno de erros até o dia da prova.", "Checkpoint final: simulado completo", 9 },
                    { 800100, 6, 0, 1, null, "filosofia-sociologia", "São 2 questões, muitas vezes sobre trabalho, tecnologia e justiça.", "Filosofia e Sociologia", 10 },
                    { 800200, 6, 0, 2, null, "geografia", "São 4 questões sobre indústria, trabalho, população e redes.", "Geografia", 10 },
                    { 800300, 6, 0, 3, null, "historia", "São 4 questões, do século XX ao Brasil recente, com o trabalho em destaque.", "História", 10 },
                    { 800400, 7, 0, 5, null, "quimica", "São 4 questões, quase todas de Química Orgânica a partir de moléculas reais (remédios, hormônios, alimentos).", "Química", 10 },
                    { 800500, 7, 0, 6, null, "fisica", "São 4 questões, com eletricidade e magnetismo em destaque.", "Física", 10 },
                    { 800600, 7, 0, 7, null, "biologia", "São 4 questões de genética, evolução e ecologia.", "Biologia", 10 },
                    { 800700, 8, 0, 9, null, "matematica", "São 5 questões: geometria, estatística, porcentagem e probabilidade.", "Matemática", 10 },
                    { 800800, 5, 0, 11, null, "artes-educacao-fisica", "São 3 questões de Artes Integradas e Educação Física: movimentos artísticos, arte e tecnologia e o corpo.", "Artes e Educação Física", 10 },
                    { 800900, 5, 0, 12, null, "lingua-portuguesa", "São 5 questões: sintaxe, norma-padrão e leitura. É também a base da redação.", "Língua Portuguesa", 10 },
                    { 801000, 5, 0, 13, null, "literatura", "São 2 questões. Não há lista de leituras: a Prova 3 cobra o Modernismo e a literatura contemporânea.", "Literatura", 10 },
                    { 801100, 5, 0, 14, null, "espanhol", "As três últimas questões são de língua estrangeira, sobre um texto ligado ao tema da prova. É leitura, não gramática decorada.", "Língua espanhola", 10 },
                    { 801200, 5, 0, 15, null, "ingles", "As três últimas questões são de língua estrangeira, sobre um texto ligado ao tema da prova. É leitura, não gramática decorada.", "Língua inglesa", 10 },
                    { 801300, 9, 0, 17, null, "redacao", "Só a Prova 3 tem redação: artigo de opinião ou carta aberta (a prova indica qual), de 15 a 30 linhas, valendo até 10 pontos. Nota abaixo de 3,0 elimina.", "Redação", 10 },
                    { 810001, 6, 2, 4, null, "checkpoint-humanas-p3", "Resolva as questões de Ciências Humanas de uma prova antiga da Prova 3, com o relógio ligado. São 40 questões em 3 horas: cerca de 4 minutos e meio por questão.", "Checkpoint: questões de Ciências Humanas", 10 },
                    { 810002, 7, 2, 8, null, "checkpoint-natureza-p3", "Resolva as questões de Ciências da Natureza de uma prova antiga da Prova 3, com o relógio ligado. São 40 questões em 3 horas: cerca de 4 minutos e meio por questão.", "Checkpoint: questões de Ciências da Natureza", 10 },
                    { 810003, 8, 2, 10, null, "checkpoint-matematica-p3", "Resolva as questões de Matemática de uma prova antiga da Prova 3, com o relógio ligado. São 40 questões em 3 horas: cerca de 4 minutos e meio por questão.", "Checkpoint: questões de Matemática", 10 },
                    { 810004, 5, 2, 16, null, "checkpoint-linguagens-p3", "Resolva as questões de Linguagens de uma prova antiga da Prova 3, com o relógio ligado. São 40 questões em 3 horas: cerca de 4 minutos e meio por questão.", "Checkpoint: questões de Linguagens", 10 },
                    { 810005, 9, 2, 18, null, "checkpoint-simulado-p3", "Faça um simulado de 40 questões e escreva uma redação, tudo em 5 horas, no horário da prova: 10/01, às 13h30. Depois, revise o caderno de erros até o dia da prova.", "Checkpoint final: simulado e redação", 10 },
                    { 900100, 3, 0, 1, null, "conhecimentos-tecnicos", "Metade da prova: 30 questões sobre a apostila oficial do 12º Censo Agro, publicada pela Cesgranrio. É a parte que mais pesa e a única que não existe em outros concursos: leia a apostila inteira.", "Conhecimentos Técnicos", 7 },
                    { 900200, 5, 0, 3, null, "lingua-portuguesa", "Quinze questões, quase sempre a partir de um texto. Comece pela compreensão e depois revise a gramática do edital.", "Língua Portuguesa", 7 },
                    { 900300, 8, 0, 5, null, "matematica", "Quinze questões de matemática do dia a dia: contas, proporção, porcentagem, medidas e leitura de tabelas, gráficos e mapas.", "Matemática", 7 },
                    { 910001, 3, 2, 2, null, "checkpoint-conhecimentos-tecnicos", "Resolva as questões de Conhecimentos Técnicos do banco sem consultar a apostila. As que errar, procure na página citada na fonte da questão.", "Checkpoint: questões de Conhecimentos Técnicos", 7 },
                    { 910002, 5, 2, 4, null, "checkpoint-lingua-portuguesa-ibge", "Resolva questões de Português das provas antigas de Recenseador com o relógio ligado: cerca de 4 minutos por questão.", "Checkpoint: questões de Língua Portuguesa", 7 },
                    { 910003, 8, 2, 6, null, "checkpoint-simulado-ibge", "Faça um simulado de 60 questões em 4 horas, no horário da prova (31/01/2027, das 13h às 17h). Lembre que é preciso 18 pontos no total e ao menos 1 em cada disciplina.", "Checkpoint final: simulado completo", 7 },
                    { 600101, 6, 1, 1, 600100, "descartes-corpo-mente", "O dualismo cartesiano e como ele se compara à ciência de hoje.", "Descartes: corpo e mente", 8 },
                    { 600102, 6, 1, 2, 600100, "etica-ambiental-p1", "Responsabilidade com as próximas gerações, utilitarismo e decisões coletivas sobre conservação.", "Ética e meio ambiente", 8 },
                    { 600103, 6, 1, 3, 600100, "classicos-sociologia-p1", "Durkheim, Bourdieu e a saúde e a doença como construções sociais.", "Clássicos da Sociologia", 8 },
                    { 600104, 6, 1, 4, 600100, "saberes-tradicionais", "Saberes indígenas e afro-brasileiros de cura comparados ao conhecimento científico.", "Saberes tradicionais e ciência", 8 },
                    { 600201, 6, 1, 1, 600200, "cartografia-tematica", "Ler mapas temáticos: escala, legenda, variáveis visuais e implantação pontual.", "Cartografia temática", 8 },
                    { 600202, 6, 1, 2, 600200, "rochas-relevo-solos", "Rochas ígneas, sedimentares e metamórficas, fósseis, erosão, intemperismo e perfil topográfico.", "Rochas, relevo e solos", 8 },
                    { 600203, 6, 1, 3, 600200, "clima-biomas-p1", "Zonas climáticas, Pampa, Caatinga, Cerrado e Mata Atlântica.", "Clima e biomas brasileiros", 8 },
                    { 600204, 6, 1, 4, 600200, "conflitos-ambientais", "Enchentes de 2024 no RS, garimpo em terras indígenas, queimadas e agrotóxicos.", "Conflitos ambientais e racismo ambiental", 8 },
                    { 600205, 6, 1, 5, 600200, "geoparques-sustentabilidade", "Geoparques da UNESCO, como Quarta Colônia e Caçapava, e unidades de conservação.", "Geoparques e desenvolvimento sustentável", 8 },
                    { 600301, 6, 1, 1, 600300, "pre-historia-evolucao-humana", "Hominídeos africanos, bipedalismo e a dispersão humana.", "Pré-História e origem humana", 8 },
                    { 600302, 6, 1, 2, 600300, "antiguidade-egito", "Egito Antigo: religião, mumificação e medicina; fontes como Heródoto.", "Antiguidade", 8 },
                    { 600303, 6, 1, 3, 600300, "idade-media", "Peste negra, Reconquista ibérica, medicina árabe e arte românica e gótica.", "Idade Média", 8 },
                    { 600304, 6, 1, 4, 600300, "america-colonial-p1", "Colonização espanhola, Inquisição e práticas de cura.", "América colonial", 8 },
                    { 600305, 6, 1, 5, 600300, "diaspora-quilombos", "Palmares, comunidades quilombolas, irmandades negras e o mito da democracia racial.", "Diáspora africana e quilombos", 8 },
                    { 600401, 7, 1, 1, 600400, "atomistica-ions", "Distribuição eletrônica de átomos e íons, isoeletrônicos e isótopos.", "Átomo, íons e isótopos", 8 },
                    { 600402, 7, 1, 2, 600400, "tabela-periodica", "Famílias, elementos de transição e propriedades periódicas: raio atômico e energia de ionização.", "Tabela periódica", 8 },
                    { 600403, 7, 1, 3, 600400, "ligacoes-polaridade", "Ligação iônica e covalente, geometria molecular, ligações de hidrogênio e propriedades da água.", "Ligações, geometria e polaridade", 8 },
                    { 600404, 7, 1, 4, 600400, "solucoes-condutividade", "Por que algumas soluções conduzem corrente e outras não; solubilidade e densidade.", "Dissociação, ionização e condutividade", 8 },
                    { 600405, 7, 1, 5, 600400, "oxirreducao-p1", "Tipos de reação, balanceamento, número de oxidação e agente oxidante e redutor.", "Reações e oxirredução", 8 },
                    { 600406, 7, 1, 6, 600400, "funcoes-inorganicas-p1", "Ácidos, bases, sais e óxidos: nomenclatura e hidrólise de sais.", "Funções inorgânicas", 8 },
                    { 600501, 7, 1, 1, 600500, "leis-newton-atrito", "Ação e reação, peso e normal, plano inclinado e atrito estático.", "Leis de Newton e atrito", 8 },
                    { 600502, 7, 1, 2, 600500, "movimento-circular", "MCU, transmissão por correia (bicicleta) e força centrípeta numa curva.", "Movimento circular", 8 },
                    { 600503, 7, 1, 3, 600500, "energia-potencia", "Energia cinética e potencial, conservação da energia mecânica, potência e rendimento.", "Energia, trabalho e potência", 8 },
                    { 600504, 7, 1, 4, 600500, "impulso-quantidade-movimento-p1", "Força média num impacto e conservação da quantidade de movimento.", "Impulso e quantidade de movimento", 8 },
                    { 600505, 7, 1, 5, 600500, "estatica-torque", "Momento de uma força, alavancas e equilíbrio do corpo rígido.", "Equilíbrio e torque", 8 },
                    { 600506, 7, 1, 6, 600500, "gravitacao-kepler", "Órbitas elípticas, lei das áreas, terceira lei e gravitação universal.", "Gravitação e Leis de Kepler", 8 },
                    { 600601, 7, 1, 1, 600600, "citologia", "Procariontes e eucariontes, organelas, parede celular e glicocálix.", "Célula", 8 },
                    { 600602, 7, 1, 2, 600600, "acidos-nucleicos-proteinas", "Códons, ribossomos, aminoácidos e transgenia.", "DNA, RNA e síntese de proteínas", 8 },
                    { 600603, 7, 1, 3, 600600, "divisao-celular-p1", "Mitose, meiose e crossing-over como fonte de variabilidade.", "Divisão celular", 8 },
                    { 600604, 7, 1, 4, 600600, "metabolismo-energetico-p1", "Etapas, produtos e onde ocorre cada processo.", "Fotossíntese e respiração", 8 },
                    { 600605, 7, 1, 5, 600600, "histologia-p1", "Sangue (hemácias, leucócitos e plaquetas) e tecidos de sustentação das plantas.", "Tecidos animais e vegetais", 8 },
                    { 600606, 7, 1, 6, 600600, "saude-epidemiologia", "Doenças como a leptospirose, saneamento e leitura de gráficos epidemiológicos.", "Saúde e doenças", 8 },
                    { 600701, 8, 1, 1, 600700, "porcentagem-graficos", "Porcentagem, proporção, densidade e leitura de tabelas e gráficos de pesquisas.", "Porcentagem, razão e gráficos", 8 },
                    { 600702, 8, 1, 2, 600700, "funcao-afim-sistemas", "Gráfico da reta, proporcionalidade, funções por partes e sistemas de equações.", "Função afim e sistemas lineares", 8 },
                    { 600703, 8, 1, 3, 600700, "funcao-quadratica", "Raízes, vértice, máximo e mínimo, como lucro = receita − custo.", "Função quadrática", 8 },
                    { 600704, 8, 1, 4, 600700, "exponencial-logaritmo", "Crescimento exponencial, meia-vida, propriedades dos logaritmos e escalas como o pH.", "Exponencial e logaritmo", 8 },
                    { 600705, 8, 1, 5, 600700, "juros-simples-compostos-p1", "Comparar crescimento linear e exponencial do dinheiro.", "Juros simples e compostos", 8 },
                    { 600706, 8, 1, 6, 600700, "medidas-notacao-cientifica", "Conversão de unidades, grandezas compostas e algarismos significativos.", "Medidas, unidades e notação científica", 8 },
                    { 600801, 5, 1, 1, 600800, "arte-rupestre-expedicoes", "Pinturas rupestres e as expedições artísticas do século XIX, como Debret.", "Arte rupestre e o olhar do viajante", 8 },
                    { 600802, 5, 1, 2, 600800, "musica-nacionalista", "O nacionalismo musical de Villa-Lobos e Camargo Guarnieri.", "Música brasileira de concerto", 8 },
                    { 600803, 5, 1, 3, 600800, "danca-ballet", "Ballet clássico, dança moderna e contemporânea: como reconhecer cada uma.", "Dança", 8 },
                    { 600804, 5, 1, 4, 600800, "atividade-fisica-saude-p1", "Exercício aeróbico, frequência cardíaca, caminhada e bem-estar.", "Atividade física e saúde", 8 },
                    { 600901, 5, 1, 1, 600900, "leitura-generos-p1", "Objetivo do texto em notícias e textos de divulgação científica.", "Leitura e gêneros", 8 },
                    { 600902, 5, 1, 2, 600900, "coesao-conectivos-p1", "Conectivos de causa, finalidade e adição, retomadas e verbos que introduzem vozes.", "Coesão e conectivos", 8 },
                    { 600903, 5, 1, 3, 600900, "pressuposto-modalizacao", "O que o texto dá como certo e como ele marca opinião, certeza ou dúvida.", "Pressupostos e modalização", 8 },
                    { 600904, 5, 1, 4, 600900, "semantica-formacao-palavras", "Polissemia, prefixos e sufixos, e pares como \"há\" e \"a\".", "Sentido e formação das palavras", 8 },
                    { 600905, 5, 1, 5, 600900, "multimodais-p1", "Relacionar texto verbal e imagem em tirinhas, infográficos e ilustrações.", "Tirinhas e infográficos", 8 },
                    { 601001, 5, 1, 1, 601000, "generos-literarios-p1", "Lírico, narrativo e dramático; métrica, redondilha e rima.", "Gêneros e versificação", 8 },
                    { 601002, 5, 1, 2, 601000, "humanismo-gil-vicente", "O teatro de Gil Vicente e o auto como crítica de costumes.", "Humanismo", 8 },
                    { 601003, 5, 1, 3, 601000, "barroco", "Gregório de Matos: poesia satírica, lírica e religiosa.", "Barroco", 8 },
                    { 601004, 5, 1, 4, 601000, "arcadismo", "Tomás Antônio Gonzaga (Marília de Dirceu) e Cláudio Manuel da Costa: bucolismo e Minas Gerais.", "Arcadismo", 8 },
                    { 601101, 5, 1, 1, 601100, "espanhol-compreensao-p1", "Ideia principal, dados e números do texto e afirmações de verdadeiro ou falso. Exemplo de fonte: reportagens do El País e da UNESCO.", "Compreensão de texto jornalístico", 8 },
                    { 601102, 5, 1, 2, 601100, "espanhol-vocabulario-p1", "Sinônimos no texto, cognatos e falsos cognatos, e o sentido de expressões.", "Vocabulário no contexto", 8 },
                    { 601103, 5, 1, 3, 601100, "espanhol-conectivos-referencia-p1", "Palavras que ligam as ideias (causa, contraste, comparação) e pronomes que retomam termos.", "Conectivos e referência", 8 },
                    { 601104, 5, 1, 4, 601100, "espanhol-formacao-palavras-p1", "Prefixos e sufixos que mudam o sentido ou a classe da palavra.", "Formação de palavras", 8 },
                    { 601105, 5, 1, 5, 601100, "espanhol-multimodal-p1", "Infográficos, gráficos, ilustrações e posts de redes sociais: relacionar imagem e texto.", "Textos multimodais", 8 },
                    { 601201, 5, 1, 1, 601200, "ingles-compreensao-p1", "Ideia principal, dados e números do texto e afirmações de verdadeiro ou falso. Exemplo de fonte: infográficos de saúde e reportagens de turismo.", "Compreensão de texto jornalístico", 8 },
                    { 601202, 5, 1, 2, 601200, "ingles-vocabulario-p1", "Sinônimos no texto, cognatos e falsos cognatos, e o sentido de expressões.", "Vocabulário no contexto", 8 },
                    { 601203, 5, 1, 3, 601200, "ingles-conectivos-referencia-p1", "Palavras que ligam as ideias (causa, contraste, comparação) e pronomes que retomam termos.", "Conectivos e referência", 8 },
                    { 601204, 5, 1, 4, 601200, "ingles-formacao-palavras-p1", "Prefixos e sufixos que mudam o sentido ou a classe da palavra.", "Formação de palavras", 8 },
                    { 601205, 5, 1, 5, 601200, "ingles-multimodal-p1", "Infográficos, gráficos, ilustrações e posts de redes sociais: relacionar imagem e texto.", "Textos multimodais", 8 },
                    { 700101, 6, 1, 1, 700100, "platao-caverna", "Mundo sensível e mundo inteligível, comparados ao mundo das redes sociais.", "Platão e a alegoria da caverna", 9 },
                    { 700102, 6, 1, 2, 700100, "etica-ambiental-p2", "Responsabilidade moral com o planeta e com as próximas gerações.", "Ética ambiental", 9 },
                    { 700103, 6, 1, 3, 700100, "trabalho-associativismo", "Catadores, cooperativas, trabalho formal e informal, Estado e mercado.", "Trabalho e associativismo", 9 },
                    { 700104, 6, 1, 4, 700100, "leitura-sociedade", "Formação de leitores e o acesso desigual ao livro.", "Leitura e sociedade", 9 },
                    { 700201, 6, 1, 1, 700200, "fontes-energia-p2", "Energias renováveis e não renováveis, eólica e solar.", "Fontes de energia", 9 },
                    { 700202, 6, 1, 2, 700200, "efeito-estufa-clima", "Efeito estufa, chuva ácida e camada de ozônio: diferenças e causas.", "Efeito estufa e mudanças climáticas", 9 },
                    { 700203, 6, 1, 3, 700200, "correntes-ambientalistas", "Preservacionismo, conservacionismo e socioambientalismo.", "Correntes ambientalistas", 9 },
                    { 700204, 6, 1, 4, 700200, "migracoes-refugiados", "Migrações internas no Brasil (Censo 2022) e refugiados climáticos.", "Migrações e refugiados", 9 },
                    { 700205, 6, 1, 5, 700200, "globalizacao-cultura", "Cadeias produtivas globais e hibridismo cultural.", "Globalização e cultura", 9 },
                    { 700301, 6, 1, 1, 700300, "imprensa-renascimento", "A prensa de Gutenberg e a circulação de livros e ideias.", "Imprensa e Idade Moderna", 9 },
                    { 700302, 6, 1, 2, 700300, "iluminismo", "A Enciclopédia, a razão e a crítica ao Antigo Regime.", "Iluminismo", 9 },
                    { 700303, 6, 1, 3, 700300, "america-colonial-p2", "Plantation, tráfico de africanos e o impacto da colonização sobre os indígenas.", "América colonial e escravidão", 9 },
                    { 700304, 6, 1, 4, 700300, "revolucao-industrial-p2", "Carvão, máquina a vapor, urbanização e impacto ambiental.", "Revolução Industrial", 9 },
                    { 700305, 6, 1, 5, 700300, "brasil-seculo-xix", "Urbanização e o crescimento do público leitor.", "Brasil no século XIX", 9 },
                    { 700306, 6, 1, 6, 700300, "republica-1945-1964", "O período democrático e o populismo.", "República de 1945 a 1964", 9 },
                    { 700401, 7, 1, 1, 700400, "estequiometria", "Balanceamento, massa, volume nas CNTP, rendimento e reagente em excesso.", "Estequiometria", 9 },
                    { 700402, 7, 1, 2, 700400, "dispersoes-solucoes", "Solução, coloide e suspensão, concentração e titulação.", "Soluções e dispersões", 9 },
                    { 700403, 7, 1, 3, 700400, "propriedades-coligativas", "Ebulioscopia e tonoscopia: o que um soluto faz com a água.", "Propriedades coligativas", 9 },
                    { 700404, 7, 1, 4, 700400, "equilibrio-ph", "Deslocamento de equilíbrio, pH e o sistema do gás carbônico na água.", "Equilíbrio químico e pH", 9 },
                    { 700405, 7, 1, 5, 700400, "eletroquimica-p2", "Oxirredução, pilhas, ânodo, cátodo e potencial de redução.", "Eletroquímica", 9 },
                    { 700501, 7, 1, 1, 700500, "hidrostatica", "Pressão como força por área e o Teorema de Stevin.", "Pressão e hidrostática", 9 },
                    { 700502, 7, 1, 2, 700500, "calor-equilibrio-termico", "Diferença entre calor e temperatura, trocas de calor e mudanças de estado com a pressão.", "Calor e equilíbrio térmico", 9 },
                    { 700503, 7, 1, 3, 700500, "termodinamica", "Lei zero, primeira e segunda leis, máquinas térmicas e rendimento de Carnot.", "Termodinâmica", 9 },
                    { 700504, 7, 1, 4, 700500, "propriedades-liquidos", "Viscosidade, tensão superficial e capilaridade.", "Propriedades dos líquidos", 9 },
                    { 700505, 7, 1, 5, 700500, "ondas-eletromagneticas", "Frequência e comprimento de onda, luz visível, infravermelho e efeito estufa.", "Ondas e espectro eletromagnético", 9 },
                    { 700601, 7, 1, 1, 700600, "classificacao-taxonomia", "Lineu, nomenclatura binomial e categorias taxonômicas.", "Classificação dos seres vivos", 9 },
                    { 700602, 7, 1, 2, 700600, "bacterias", "Estrutura, nutrição e reprodução, e o uso em biorremediação.", "Bactérias", 9 },
                    { 700603, 7, 1, 3, 700600, "botanica", "Tecidos vegetais e nutrição mineral das plantas.", "Botânica", 9 },
                    { 700604, 7, 1, 4, 700600, "artropodes", "Exoesqueleto, metamorfose e tipos de desenvolvimento.", "Insetos e artrópodes", 9 },
                    { 700605, 7, 1, 5, 700600, "fisiologia-respiratoria", "Anatomia, pleura, diafragma e a mecânica da respiração.", "Sistema respiratório", 9 },
                    { 700606, 7, 1, 6, 700600, "ecossistemas-p2", "Manguezal e outros ambientes, adaptações das plantas e ciclos da matéria.", "Ecossistemas", 9 },
                    { 700701, 8, 1, 1, 700700, "triangulos-semelhanca", "Semelhança, relações métricas, áreas e lei dos senos e dos cossenos.", "Triângulos e semelhança", 9 },
                    { 700702, 8, 1, 2, 700700, "funcoes-trigonometricas", "Fenômenos periódicos, período, máximo e mínimo.", "Funções seno e cosseno", 9 },
                    { 700703, 8, 1, 3, 700700, "progressoes", "Termo geral e soma dos termos de PA e PG.", "Progressões aritmética e geométrica", 9 },
                    { 700704, 8, 1, 4, 700700, "analise-combinatoria-p2", "Princípio fundamental da contagem, arranjos e combinações.", "Análise combinatória", 9 },
                    { 700705, 8, 1, 5, 700700, "probabilidade-p2", "Espaço amostral, união, complementar e probabilidade condicional.", "Probabilidade", 9 },
                    { 700706, 8, 1, 6, 700700, "estatistica-p2", "Média, moda, mediana, desvio padrão, histograma e box-plot.", "Estatística", 9 },
                    { 700801, 5, 1, 1, 700800, "arte-contemporanea-ativismo", "Protestos em museus, Guerrilla Girls e o lugar da mulher na história da arte.", "Arte contemporânea e ativismo", 9 },
                    { 700802, 5, 1, 2, 700800, "concretismo-neoconcretismo", "Grupo Ruptura, Lygia Pape e o livro de artista.", "Concretismo e neoconcretismo", 9 },
                    { 700803, 5, 1, 3, 700800, "teatro-musical", "O que se exige do intérprete em canto, dança e atuação.", "Teatro e teatro musical", 9 },
                    { 700804, 5, 1, 4, 700800, "corpo-saude-p2", "Atividade física ao ar livre, aptidão física, postura e uso de telas.", "Corpo, postura e saúde", 9 },
                    { 700901, 5, 1, 1, 700900, "interpretacao-p2", "Reportagens e textos de opinião: tese, argumentos e inferências.", "Interpretação de texto", 9 },
                    { 700902, 5, 1, 2, 700900, "classes-palavras", "Advérbio, adjetivo e palavras que mudam de classe conforme o uso.", "Classes de palavras", 9 },
                    { 700903, 5, 1, 3, 700900, "termos-oracao-aposto", "Aposto, vozes verbais, agente da passiva e o \"se\".", "Termos da oração", 9 },
                    { 700904, 5, 1, 4, 700900, "linguagem-figurada", "Metáfora, metonímia e o sentido das palavras no contexto.", "Linguagem figurada", 9 },
                    { 700905, 5, 1, 5, 700900, "multimodais-p2", "Relação entre linguagem verbal e não verbal e o humor.", "Charges, tiras e cartazes", 9 },
                    { 701001, 5, 1, 1, 701000, "romantismo-poesia", "Gonçalves Dias e o nacionalismo; Castro Alves e a poesia condoreira; versificação.", "Romantismo: poesia", 9 },
                    { 701002, 5, 1, 2, 701000, "romantismo-prosa", "O romance romântico e a busca de uma identidade nacional.", "Romantismo: prosa", 9 },
                    { 701003, 5, 1, 3, 701000, "realismo-machado", "Memórias Póstumas de Brás Cubas: ironia, narrador defunto e crítica à vaidade.", "Realismo e Machado de Assis", 9 },
                    { 701004, 5, 1, 4, 701000, "realismo-europeu", "Balzac e o romance da ascensão da burguesia.", "Realismo europeu", 9 },
                    { 701101, 5, 1, 1, 701100, "espanhol-compreensao-p2", "Ideia principal, dados e números do texto e afirmações de verdadeiro ou falso. Exemplo de fonte: reportagens do El País e posts sobre leitura.", "Compreensão de texto jornalístico", 9 },
                    { 701102, 5, 1, 2, 701100, "espanhol-vocabulario-p2", "Sinônimos no texto, cognatos e falsos cognatos, e o sentido de expressões.", "Vocabulário no contexto", 9 },
                    { 701103, 5, 1, 3, 701100, "espanhol-conectivos-referencia-p2", "Palavras que ligam as ideias (causa, contraste, comparação) e pronomes que retomam termos.", "Conectivos e referência", 9 },
                    { 701104, 5, 1, 4, 701100, "espanhol-formacao-palavras-p2", "Prefixos e sufixos que mudam o sentido ou a classe da palavra.", "Formação de palavras", 9 },
                    { 701105, 5, 1, 5, 701100, "espanhol-multimodal-p2", "Infográficos, gráficos, ilustrações e posts de redes sociais: relacionar imagem e texto.", "Textos multimodais", 9 },
                    { 701201, 5, 1, 1, 701200, "ingles-compreensao-p2", "Ideia principal, dados e números do texto e afirmações de verdadeiro ou falso. Exemplo de fonte: textos da ONU sobre sustentabilidade e sobre os benefícios da leitura.", "Compreensão de texto jornalístico", 9 },
                    { 701202, 5, 1, 2, 701200, "ingles-vocabulario-p2", "Sinônimos no texto, cognatos e falsos cognatos, e o sentido de expressões.", "Vocabulário no contexto", 9 },
                    { 701203, 5, 1, 3, 701200, "ingles-conectivos-referencia-p2", "Palavras que ligam as ideias (causa, contraste, comparação) e pronomes que retomam termos.", "Conectivos e referência", 9 },
                    { 701204, 5, 1, 4, 701200, "ingles-formacao-palavras-p2", "Prefixos e sufixos que mudam o sentido ou a classe da palavra.", "Formação de palavras", 9 },
                    { 701205, 5, 1, 5, 701200, "ingles-multimodal-p2", "Infográficos, gráficos, ilustrações e posts de redes sociais: relacionar imagem e texto.", "Textos multimodais", 9 },
                    { 800101, 6, 1, 1, 800100, "classicos-sociologia-p3", "Alienação e exploração, anomia, e estratificação por classe e status.", "Marx, Durkheim e Weber", 10 },
                    { 800102, 6, 1, 2, 800100, "trabalho-plataformas", "Uberização, precarização, flexibilização e desemprego tecnológico.", "Trabalho e plataformas digitais", 10 },
                    { 800103, 6, 1, 3, 800100, "rawls-justica", "Rawls (véu de ignorância e princípio da diferença), meritocracia e ética dos algoritmos.", "Justiça e ética", 10 },
                    { 800104, 6, 1, 4, 800100, "racismo-estrutural", "Desigualdade racial no trabalho e na renda.", "Racismo estrutural", 10 },
                    { 800201, 6, 1, 1, 800200, "fordismo-toyotismo", "Produção em massa, just in time, automação e trabalho flexível.", "Fordismo e toyotismo", 10 },
                    { 800202, 6, 1, 2, 800200, "globalizacao-p3", "Desemprego estrutural e a produção espalhada pelo mundo.", "Globalização e neoliberalismo", 10 },
                    { 800203, 6, 1, 3, 800200, "redes-tecnica-geotecnologias", "Meio técnico-científico-informacional, fluxos, SIG e satélites.", "Redes, técnica e geotecnologias", 10 },
                    { 800204, 6, 1, 4, 800200, "populacao-p3", "Pirâmides etárias, transição demográfica e envelhecimento.", "População", 10 },
                    { 800205, 6, 1, 5, 800200, "rede-urbana", "Metrópoles, distribuição de serviços e desigualdades regionais.", "Rede e hierarquia urbana", 10 },
                    { 800301, 6, 1, 1, 800300, "industrializacao-trabalho", "Revolução Industrial, proletarização e a mulher no trabalho na Primeira República.", "Industrialização e trabalho", 10 },
                    { 800302, 6, 1, 2, 800300, "segunda-guerra", "Fases da guerra, tecnologia e criptografia (Turing e a Enigma).", "Segunda Guerra Mundial", 10 },
                    { 800303, 6, 1, 3, 800300, "guerra-fria", "Os dois blocos, perestroika, glasnost e o fim da URSS.", "Guerra Fria", 10 },
                    { 800304, 6, 1, 4, 800300, "brasil-1950-1970", "Modernização urbana e o Milagre Econômico da ditadura.", "Brasil de 1950 a 1970", 10 },
                    { 800305, 6, 1, 5, 800300, "ditadura-redemocratizacao", "Distensão, crises do petróleo, inflação e o Plano Collor.", "Ditadura e redemocratização", 10 },
                    { 800401, 7, 1, 1, 800400, "cadeias-hibridizacao", "Hibridização sp, sp² e sp³ e ligações sigma e pi.", "Cadeias carbônicas e hibridização", 10 },
                    { 800402, 7, 1, 2, 800400, "funcoes-organicas", "Álcool, fenol, ácido carboxílico, éster, amina e amida: reconhecer pela estrutura.", "Funções orgânicas", 10 },
                    { 800403, 7, 1, 3, 800400, "isomeria", "Isomeria geométrica (E/Z), carbono assimétrico e isomeria óptica.", "Isomeria", 10 },
                    { 800404, 7, 1, 4, 800400, "reacoes-organicas", "Adição, substituição, esterificação e hidrogenação de óleos.", "Reações orgânicas", 10 },
                    { 800405, 7, 1, 5, 800400, "bioquimica-p3", "Aminoácidos, ligação peptídica, gorduras e celulose.", "Moléculas da vida", 10 },
                    { 800501, 7, 1, 1, 800500, "eletrostatica-campo", "Carga, Lei de Coulomb e como o campo cai com a distância.", "Campo e força elétrica", 10 },
                    { 800502, 7, 1, 2, 800500, "eletrodinamica", "Lei de Ohm, associação de resistores, Leis de Kirchhoff e potência no chuveiro.", "Circuitos elétricos", 10 },
                    { 800503, 7, 1, 3, 800500, "eletromagnetismo", "Campo de um fio, força magnética sobre cargas, motores e geradores.", "Eletromagnetismo", 10 },
                    { 800504, 7, 1, 4, 800500, "radiacoes-radioatividade", "Tipos de radiação, meia-vida e usos na saúde e na indústria.", "Radiações e radioatividade", 10 },
                    { 800505, 7, 1, 5, 800500, "optica-lentes", "Refração e formação de imagens em lentes.", "Lentes", 10 },
                    { 800601, 7, 1, 1, 800600, "cromossomos-genoma", "Homólogos, centrômero, telômero e o genoma humano.", "Cromossomos e genoma", 10 },
                    { 800602, 7, 1, 2, 800600, "heranca-ligada-x", "Leis de Mendel, heredogramas, hemofilia e daltonismo.", "Genética e herança ligada ao sexo", 10 },
                    { 800603, 7, 1, 3, 800600, "evolucao", "Seleção natural e sexual e a teoria sintética: mutação e recombinação.", "Evolução", 10 },
                    { 800604, 7, 1, 4, 800600, "biodiversidade-conservacao", "Agrobiodiversidade, polinizadores e conservação in situ.", "Biodiversidade e conservação", 10 },
                    { 800605, 7, 1, 5, 800600, "ciclos-biogeoquimicos", "Ciclos do carbono e da água, desmatamento e combustíveis fósseis.", "Ciclos biogeoquímicos", 10 },
                    { 800701, 8, 1, 1, 800700, "geometria-plana-areas", "Áreas de polígonos e círculos e a razão entre áreas de figuras semelhantes.", "Áreas e semelhança", 10 },
                    { 800702, 8, 1, 2, 800700, "geometria-espacial", "Áreas e volumes de prismas, pirâmides, cilindros, cones e esferas.", "Geometria espacial", 10 },
                    { 800703, 8, 1, 3, 800700, "transformacoes-geometricas", "Translação, reflexão, rotação, homotetia e ladrilhamento.", "Transformações geométricas", 10 },
                    { 800704, 8, 1, 4, 800700, "estatistica-p3", "Média simples e ponderada, mediana e amplitude a partir de gráficos.", "Estatística", 10 },
                    { 800705, 8, 1, 5, 800700, "matematica-financeira-p3", "Descontos, impostos, juros simples e compostos.", "Porcentagem e juros", 10 },
                    { 800706, 8, 1, 6, 800700, "probabilidade-p3", "Espaço amostral e probabilidade da união de eventos.", "Probabilidade", 10 },
                    { 800801, 5, 1, 1, 800800, "movimentos-artisticos", "Romantismo, Surrealismo e Cubismo; o impacto da fotografia na pintura.", "Movimentos artísticos", 10 },
                    { 800802, 5, 1, 2, 800800, "arte-tecnologia-ia", "Inteligência artificial, originalidade e direito de autor.", "Arte, tecnologia e autoria", 10 },
                    { 800803, 5, 1, 3, 800800, "mercado-ensino-arte", "Valor simbólico e econômico da obra e o ensino de arte no Brasil.", "Mercado e ensino de arte", 10 },
                    { 800804, 5, 1, 4, 800800, "educacao-fisica-profissao", "O papel do profissional, prescrição de treino e a cobrança por desempenho.", "Educação Física e sociedade do desempenho", 10 },
                    { 800901, 5, 1, 1, 800900, "periodo-composto", "Orações subordinadas substantivas e adjetivas, reduzidas e conectivos.", "Período composto", 10 },
                    { 800902, 5, 1, 2, 800900, "funcoes-sintaticas", "Complemento nominal, adjunto adnominal e aposto.", "Funções sintáticas", 10 },
                    { 800903, 5, 1, 3, 800900, "concordancia-regencia", "Concordância com o \"se\", regência verbal e colocação pronominal.", "Concordância, regência e colocação", 10 },
                    { 800904, 5, 1, 4, 800900, "verbos-modos", "Futuro do pretérito, imperativo afirmativo e negativo.", "Tempos e modos verbais", 10 },
                    { 800905, 5, 1, 5, 800900, "pontuacao", "Vírgula com aposto, orações intercaladas e antes do \"e\".", "Pontuação", 10 },
                    { 800906, 5, 1, 6, 800900, "interpretacao-graficos-p3", "Textos informativos e gráficos: inferir sem extrapolar.", "Interpretação de textos e gráficos", 10 },
                    { 801001, 5, 1, 1, 801000, "modernismo-regionalismo", "Graciliano Ramos e Vidas Secas; o regionalismo nordestino.", "Romance de 30", 10 },
                    { 801002, 5, 1, 2, 801000, "joao-cabral", "Morte e Vida Severina: auto, retirante e linguagem seca.", "João Cabral de Melo Neto", 10 },
                    { 801003, 5, 1, 3, 801000, "concretismo-tropicalia", "Poesia visual e as canções tropicalistas.", "Concretismo e Tropicália", 10 },
                    { 801004, 5, 1, 4, 801000, "poesia-contemporanea", "Paulo Leminski, poesia marginal e intertextualidade.", "Poesia contemporânea", 10 },
                    { 801101, 5, 1, 1, 801100, "espanhol-compreensao-p3", "Ideia principal, dados e números do texto e afirmações de verdadeiro ou falso. Exemplo de fonte: El País e BBC Mundo sobre inteligência artificial e trabalho.", "Compreensão de texto jornalístico", 10 },
                    { 801102, 5, 1, 2, 801100, "espanhol-vocabulario-p3", "Sinônimos no texto, cognatos e falsos cognatos, e o sentido de expressões.", "Vocabulário no contexto", 10 },
                    { 801103, 5, 1, 3, 801100, "espanhol-conectivos-referencia-p3", "Palavras que ligam as ideias (causa, contraste, comparação) e pronomes que retomam termos.", "Conectivos e referência", 10 },
                    { 801104, 5, 1, 4, 801100, "espanhol-formacao-palavras-p3", "Prefixos e sufixos que mudam o sentido ou a classe da palavra.", "Formação de palavras", 10 },
                    { 801105, 5, 1, 5, 801100, "espanhol-multimodal-p3", "Infográficos, gráficos, ilustrações e posts de redes sociais: relacionar imagem e texto.", "Textos multimodais", 10 },
                    { 801201, 5, 1, 1, 801200, "ingles-compreensao-p3", "Ideia principal, dados e números do texto e afirmações de verdadeiro ou falso. Exemplo de fonte: relatórios do Fórum Econômico Mundial sobre empregos e clima.", "Compreensão de texto jornalístico", 10 },
                    { 801202, 5, 1, 2, 801200, "ingles-vocabulario-p3", "Sinônimos no texto, cognatos e falsos cognatos, e o sentido de expressões.", "Vocabulário no contexto", 10 },
                    { 801203, 5, 1, 3, 801200, "ingles-conectivos-referencia-p3", "Palavras que ligam as ideias (causa, contraste, comparação) e pronomes que retomam termos.", "Conectivos e referência", 10 },
                    { 801204, 5, 1, 4, 801200, "ingles-formacao-palavras-p3", "Prefixos e sufixos que mudam o sentido ou a classe da palavra.", "Formação de palavras", 10 },
                    { 801205, 5, 1, 5, 801200, "ingles-multimodal-p3", "Infográficos, gráficos, ilustrações e posts de redes sociais: relacionar imagem e texto.", "Textos multimodais", 10 },
                    { 801301, 9, 1, 1, 801300, "proposta-tema", "Ler o comando: situação comunicativa, tema, palavras-chave e para quem se escreve. Tangenciar o tema limita a nota a 3,5.", "Tema, situação e coletânea", 10 },
                    { 801302, 9, 1, 2, 801300, "artigo-opiniao", "Título, contextualização, tese, argumentos e conclusão; separar a sua voz das outras vozes citadas.", "Artigo de opinião", 10 },
                    { 801303, 9, 1, 3, 801300, "carta-aberta", "Vocativo, interlocutor e remetente, motivo, argumentos e proposta. O remetente é um papel, nunca o seu nome.", "Carta aberta", 10 },
                    { 801304, 9, 1, 4, 801300, "argumentacao", "Sustentar a tese com dados e fontes, prever e refutar argumentos contrários.", "Argumentação", 10 },
                    { 801305, 9, 1, 5, 801300, "coesao-norma", "Conectivos, retomadas, parágrafos, pontuação e registro formal.", "Coesão e norma-padrão", 10 },
                    { 801306, 9, 1, 6, 801300, "motivos-de-zero", "Fuga do tema, gênero errado, letra ilegível, identificação, linhas fora do limite ou plágio.", "O que zera a redação", 10 },
                    { 900101, 3, 1, 1, 900100, "ibge-censo-agro", "O que o IBGE faz, para que serve o Censo Agro e o que muda no 12º Censo (apostila, p. 4 a 10).", "O IBGE e o Censo Agropecuário", 7 },
                    { 900102, 3, 1, 2, 900100, "estrutura-censitaria", "Quem é quem no Censo, do recenseador ao coordenador, e a quem cada um responde (p. 11 a 19).", "Estrutura organizacional censitária", 7 },
                    { 900103, 3, 1, 3, 900100, "instrumentos-trabalho", "O que o recenseador usa em campo e para quê (p. 20 a 22).", "Instrumentos de trabalho", 7 },
                    { 900104, 3, 1, 4, 900100, "navegacao-campo", "Como se orientar no setor e percorrer a área de trabalho (p. 23 a 31).", "Navegação no trabalho de campo", 7 },
                    { 900105, 3, 1, 5, 900100, "divisao-territorial", "Estados, municípios, distritos, setores censitários e as outras estruturas que organizam a coleta (p. 32 a 38).", "Divisão político-administrativa e estruturas territoriais", 7 },
                    { 900106, 3, 1, 6, 900100, "povos-comunidades-tradicionais", "Terras indígenas, territórios quilombolas e outras estruturas de PCTs (p. 39 a 42).", "Povos e comunidades tradicionais", 7 },
                    { 900107, 3, 1, 7, 900100, "endereco-coordenadas", "Como registrar o endereço e a localização de cada estabelecimento (p. 43 a 50).", "Endereço e coordenadas geográficas", 7 },
                    { 900108, 3, 1, 8, 900100, "estabelecimento-agropecuario", "O que conta como estabelecimento, quem é o produtor e quais atividades entram (p. 51 a 56).", "Estabelecimento agropecuário", 7 },
                    { 900109, 3, 1, 9, 900100, "produtor-sem-area-situacoes", "Os casos que fogem da regra e como o recenseador deve tratá-los (p. 57 a 66).", "Produtores sem área e situações especiais", 7 },
                    { 900201, 5, 1, 1, 900200, "compreensao-texto", "Ideia principal, informações explícitas e implícitas e o sentido de trechos no texto.", "Compreensão de texto", 7 },
                    { 900202, 5, 1, 2, 900200, "significacao-palavras", "Sinônimos, antônimos e homônimos, e o sentido da palavra no contexto.", "Significação das palavras", 7 },
                    { 900203, 5, 1, 3, 900200, "pontuacao-frases-paragrafos", "Vírgula e outros sinais, e a sequência lógica de frases e parágrafos.", "Pontuação e organização do texto", 7 },
                    { 900204, 5, 1, 4, 900200, "ortografia-acentuacao", "Grafia oficial das palavras e as regras de acento gráfico.", "Ortografia e acentuação", 7 },
                    { 900205, 5, 1, 5, 900200, "classes-palavras-ibge", "Substantivo, adjetivo, verbo, advérbio, pronome, preposição e conjunção, e o papel de cada um na frase.", "Classes de palavras", 7 },
                    { 900206, 5, 1, 6, 900200, "concordancia-regencia", "Concordância nominal e verbal; regência nominal e verbal.", "Concordância e regência", 7 },
                    { 900207, 5, 1, 7, 900200, "verbos-pronomes", "Verbos regulares e irregulares, vozes do verbo e emprego dos pronomes.", "Verbos e pronomes", 7 },
                    { 900301, 8, 1, 1, 900300, "numeros-naturais", "Operações, propriedades, múltiplos e divisores, e problemas.", "Números naturais", 7 },
                    { 900302, 8, 1, 2, 900300, "inteiros-racionais", "Operações com negativos, frações e decimais, e problemas.", "Números inteiros e racionais", 7 },
                    { 900303, 8, 1, 3, 900300, "razao-proporcao-porcentagem", "Grandezas proporcionais, divisão proporcional, regra de três simples e porcentagem.", "Razão, proporção e porcentagem", 7 },
                    { 900304, 8, 1, 4, 900300, "equacoes-primeiro-grau", "Montar e resolver a equação a partir do problema.", "Equações do 1º grau", 7 },
                    { 900305, 8, 1, 5, 900300, "medidas", "Comprimento, superfície, volume, massa e tempo, e o sistema métrico decimal.", "Medidas e conversões", 7 },
                    { 900306, 8, 1, 6, 900300, "areas-perimetros-volumes", "Cálculo com as figuras e sólidos mais comuns.", "Áreas, perímetros e volumes", 7 },
                    { 900307, 8, 1, 7, 900300, "tabelas-graficos-mapas", "Ler e interpretar informações em tabelas, gráficos, quadros, mapas, croquis e esquemas.", "Tabelas, gráficos, mapas e croquis", 7 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 600101);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 600102);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 600103);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 600104);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 600201);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 600202);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 600203);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 600204);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 600205);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 600301);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 600302);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 600303);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 600304);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 600305);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 600401);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 600402);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 600403);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 600404);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 600405);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 600406);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 600501);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 600502);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 600503);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 600504);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 600505);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 600506);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 600601);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 600602);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 600603);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 600604);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 600605);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 600606);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 600701);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 600702);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 600703);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 600704);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 600705);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 600706);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 600801);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 600802);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 600803);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 600804);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 600901);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 600902);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 600903);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 600904);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 600905);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 601001);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 601002);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 601003);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 601004);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 601101);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 601102);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 601103);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 601104);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 601105);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 601201);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 601202);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 601203);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 601204);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 601205);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 610001);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 610002);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 610003);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 610004);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 610005);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 700101);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 700102);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 700103);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 700104);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 700201);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 700202);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 700203);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 700204);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 700205);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 700301);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 700302);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 700303);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 700304);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 700305);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 700306);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 700401);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 700402);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 700403);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 700404);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 700405);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 700501);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 700502);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 700503);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 700504);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 700505);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 700601);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 700602);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 700603);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 700604);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 700605);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 700606);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 700701);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 700702);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 700703);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 700704);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 700705);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 700706);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 700801);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 700802);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 700803);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 700804);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 700901);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 700902);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 700903);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 700904);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 700905);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 701001);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 701002);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 701003);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 701004);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 701101);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 701102);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 701103);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 701104);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 701105);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 701201);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 701202);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 701203);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 701204);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 701205);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 710001);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 710002);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 710003);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 710004);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 710005);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 800101);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 800102);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 800103);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 800104);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 800201);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 800202);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 800203);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 800204);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 800205);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 800301);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 800302);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 800303);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 800304);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 800305);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 800401);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 800402);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 800403);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 800404);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 800405);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 800501);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 800502);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 800503);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 800504);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 800505);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 800601);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 800602);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 800603);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 800604);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 800605);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 800701);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 800702);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 800703);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 800704);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 800705);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 800706);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 800801);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 800802);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 800803);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 800804);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 800901);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 800902);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 800903);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 800904);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 800905);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 800906);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 801001);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 801002);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 801003);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 801004);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 801101);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 801102);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 801103);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 801104);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 801105);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 801201);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 801202);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 801203);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 801204);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 801205);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 801301);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 801302);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 801303);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 801304);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 801305);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 801306);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 810001);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 810002);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 810003);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 810004);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 810005);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 900101);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 900102);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 900103);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 900104);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 900105);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 900106);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 900107);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 900108);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 900109);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 900201);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 900202);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 900203);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 900204);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 900205);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 900206);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 900207);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 900301);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 900302);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 900303);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 900304);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 900305);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 900306);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 900307);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 910001);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 910002);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 910003);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 600100);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 600200);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 600300);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 600400);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 600500);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 600600);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 600700);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 600800);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 600900);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 601000);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 601100);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 601200);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 700100);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 700200);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 700300);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 700400);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 700500);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 700600);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 700700);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 700800);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 700900);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 701000);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 701100);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 701200);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 800100);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 800200);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 800300);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 800400);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 800500);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 800600);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 800700);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 800800);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 800900);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 801000);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 801100);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 801200);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 801300);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 900100);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 900200);

            migrationBuilder.DeleteData(
                table: "RoadmapNodes",
                keyColumn: "Id",
                keyValue: 900300);

            migrationBuilder.DeleteData(
                table: "Tracks",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Tracks",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Tracks",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Tracks",
                keyColumn: "Id",
                keyValue: 10);
        }
    }
}
