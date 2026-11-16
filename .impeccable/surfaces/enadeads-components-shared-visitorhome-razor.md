---
version: 1
slug: "enadeads-components-shared-visitorhome-razor"
primary_target: "EnadeAds/Components/Shared/VisitorHome.razor"
related_targets: ["EnadeAds/Components/Pages/Home.razor","EnadeAds/Components/Shared/VisitorHome.razor.css"]
---

# Landing do visitante (Início sem prova escolhida)

Escopo: o que a rota "/" mostra para quem ainda não escolheu a prova; substitui o antigo TrackChooser. Quem já escolheu continua no feed (brief da casca e do Início). Modo: Persuade dentro do mundo Grade do Semestre. Visitante: estudante de ENEM ou ENADE chegando por link, no celular ou no computador. Ação: escolher a prova (trilha/{slug}). Crença: dá para praticar no formato real. Provas na página: uma questão real do Inep respondível sem conta, dias até cada prova, trecho do roadmap, últimos avisos oficiais. Restrições: SSR sem JS (responder é formulário), claro e escuro, nada inventado (avisos e números vêm do banco).

## Direction contract

THESIS: A landing é o quadro do curso no corredor: questão da semana, dias até cada prova, avisos dos professores e o roadmap afixados lado a lado, desenhados como a grade. Recusa o hero de título sobre subtítulo e botão com fileira de cartões iguais embaixo.

OWN-WORLD: O mundo Grade do Semestre sem mudança: casca azul-tinta, chão branco ou azul-noite, módulos com fio de 2px e aba de título em Archivo estreita caixa alta, linhas separadas por fio de 1px, cor de bloco só em matéria, coral só para "você" (a prova que é hoje, a alternativa marcada). Alternativas como bolhas de cartão-resposta. Numerais tabulares grandes para os dias.

STORY: O visitante primeiro entende o que é o Rumo (plataforma aberta do curso, uma trilha por prova) e já vê as provas com os dias que faltam; desce para Como funciona, cinco etapas do roadmap ao dia da prova, cada uma com uma amostra real; então responde uma questão real sem conta e vê o gabarito comentado; por fim vê a turma (avisos oficiais, roadmap) e que é aberto e gratuito.

FIRST VIEWPORT: Casca de 56px. Apresentação em duas colunas (1.35fr / 1fr): à esquerda o título "Prepare-se para o ENEM e o ENADE no formato da prova.", um parágrafo explicando o Rumo, uma lista com fios (Roadmap, Questões, Simulados, Turma) e links para Como funciona e para a questão; à direita o módulo Para qual prova você vai estudar? com uma linha por prova (nome, data, dias em 36px, link Estudar para X). Abaixo da dobra: Como funciona em cinco colunas de grade, a demo com a questão, a turma e o fechamento. No celular: apresentação, provas, etapas empilhadas, questão.

FORM: Mural do curso desenhado como a grade, candidato 6 de 7 na lista ordenada; seed:7b95e1400d603f90

FINISH: unreviewed and undocumented is unfinished; this build ends with the finish review, the verdict, DESIGN.md, and every shipping raster carrying its provenance

## Momento memorável

Depois de entender o produto, responder a questão real sem conta e ver o gabarito comentado abrir ao lado de cada alternativa, na própria landing.
