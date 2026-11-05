# Regras de negócio

Este documento reúne as decisões de produto da plataforma. Toda mudança de regra passa por aqui no mesmo PR que muda o código.

## Trilhas

O Rumo atende várias provas. Cada prova numa edição é uma **trilha**: hoje, **ENADE 2026 · ADS**, **ENADE 2026 · Redes de Computadores** e **ENEM 2026**.

- **Ficam na trilha:** datas de prova, roadmap (matérias, assuntos e checkpoints), questões (pela matéria), desafios e simulados.
- **Ficam na plataforma:** contas, papéis, moderação e o feed. No feed aparecem os posts da trilha ativa e os posts gerais.
- **Trilha ativa:** fica num cookie (`trilha`) e se troca no topo, ao lado da marca ("Rumo ENEM"). Quem nunca escolheu fica no ENADE ADS, a primeira trilha. Trocar de trilha numa página de um item (uma matéria, uma questão) volta para a lista da seção, porque o item é da outra trilha.
- **Datas:** cada trilha tem um ou dois dias de prova; o ENEM tem dois domingos (8 e 15/11/2026). A contagem regressiva vai até o próximo dia de prova. O admin muda as datas da trilha ativa em *Data da prova*.
- **Posts:** aluno publica sempre na trilha ativa. Professor e admin podem marcar *Publicar para todas as trilhas*.
- **Simulados:** um aberto por aluno em cada trilha. O formato segue a prova da trilha (ver *Simulados*).
- **Dados da conta (LGPD):** a exportação traz os dados de todas as trilhas, não só da ativa.

## Perfis

| Perfil | Quem é | O que pode fazer |
|---|---|---|
| Visitante | Quem não entrou na conta | Ver o roadmap, as matérias, as questões (sem gabarito), os desafios e os avisos |
| Aluno | Toda conta cadastrada | Tudo do visitante, mais responder questões, ver o gabarito e salvar o progresso |
| ProfessorPendente | Pediu acesso de professor e aguarda aprovação | O mesmo que o aluno |
| Professor | Aprovado por um administrador | Tudo do aluno, mais propor questões, desafios e mudanças no roadmap (que outro professor ou um admin revisa), publicar avisos e revisar propostas de colegas |
| Admin | E-mail listado na configuração `Admin:Emails` | Tudo do professor, mais aprovar ou recusar professores |

Uma conta pode ter mais de um perfil ao mesmo tempo. Por exemplo, um professor aprovado também é aluno.

## Revisão das edições de professores

Toda alteração de professor em **questões** (criar, editar, arquivar), **roadmap** (editar, criar assunto ou checkpoint, reordenar, remover) e **desafios** (criar, editar, encerrar) vai para a fila de *Revisões* e só vale depois de aprovada.

- **Quem aprova:** outro professor ou um admin. Quem enviou nunca aprova a própria proposta, mas pode retirá-la.
- **Admin** publica direto, sem fila.
- **Na aprovação**, a alteração é aplicada como foi proposta, em nome do autor, na trilha em que foi proposta e com as mesmas validações da tela. Se o item mudou desde a proposta e ela não vale mais (por exemplo, um desafio cuja data de início já passou), a proposta fica como *aprovada, mas não aplicada*, com o motivo.
- **Recusa** pode vir com um motivo, que o autor vê em *Minhas propostas*.
- **Fora da fila:** posts da comunidade e moderação, que precisam valer na hora.
- Excluir a conta apaga as propostas dela.

## Cadastro

### Idade

- Só cria conta quem tem **13 anos ou mais**. O cadastro pede a faixa de idade: "18 anos ou mais" ou "de 13 a 17 anos, com autorização do responsável legal".
- Para quem tem de 13 a 17 anos, a conta guarda `IsMinor` e a data da declaração de autorização (`GuardianConsentAt`). Não pedimos data de nascimento, documento nem escola (minimização, LGPD art. 14).
- O texto dos termos e da política sobre menores é provisório e passa pela revisão de um advogado.

- Toda conta nova recebe o perfil **Aluno**.
- O formulário tem a caixa **"Sou professor"**. Ao marcá-la, aparece um texto explicando que o acesso depende de aprovação.
- **"Sou professor" marcado e e-mail institucional:** a conta recebe **Aluno + ProfessorPendente**.
- **"Sou professor" marcado e e-mail não institucional:** o cadastro é recusado e nenhuma conta é criada.
- **"Sou professor" desmarcado:** a conta é só de **Aluno**, mesmo com e-mail institucional.
- O e-mail é institucional quando tem `edu` como parte do domínio, depois do nome da instituição: `nome@faculdade.edu.br` e `nome@mit.edu` são; `nome@educacao.com.br` não é.
- A conta só entra depois de confirmar o e-mail pelo link enviado. É isso que prova que o professor é dono do endereço `.edu`.
- E-mails listados em `Admin:Emails` recebem também o perfil **Admin** ao se cadastrar.

### Por que o professor fica pendente

Alunos também têm e-mail institucional, muitas vezes no mesmo domínio dos professores. Sem aprovação, qualquer aluno marcaria a caixa e passaria a publicar questões e ver gabaritos como professor.

## Aprovação de professores

- Só **Admin** vê a lista de pedidos pendentes.
- **Aprovar:** a conta perde ProfessorPendente e ganha **Professor**.
- **Recusar:** a conta perde ProfessorPendente e continua como **Aluno**.
- Depois da decisão, a sessão da pessoa é renovada em até 1 minuto e ela precisa entrar de novo para o novo perfil valer.

## Gestão de contas

- Só **Admin** abre a tela *Contas*: busca por nome ou e-mail e filtro por perfil (alunos, professores pendentes, professores, administradores, bloqueadas).
- O admin pode **revogar o perfil de Professor** (a pessoa continua Aluno), **remover o perfil de Admin** de outra pessoa e **bloquear ou desbloquear** uma conta. Conta bloqueada não consegue entrar.
- Toda mudança vale em até 1 minuto: a sessão da pessoa é renovada e ela precisa entrar de novo.
- Um admin **não altera a própria conta** por essa tela, para não se trancar fora nem deixar a plataforma sem admin por engano.
- Novos admins continuam vindo só da configuração `Admin:Emails`.

## Privacidade (LGPD)

- O cadastro exige aceitar os **Termos de uso** e a **Política de privacidade**. A data e a versão aceitas ficam guardadas na conta.
- Os textos ficam em `/termos` e `/privacidade`, com link no rodapé de todas as páginas. O responsável pelos dados e o e-mail de contato vêm da configuração (`Privacy:ControllerName`, `Privacy:ContactEmail`), porque cada instalação tem o seu responsável.
- **Baixar meus dados** (Minha conta → Dados pessoais) entrega um arquivo com a conta, o progresso no roadmap, as respostas, os simulados, os desafios, os duelos, os posts, os votos, as denúncias e os comentários.
- **Excluir a conta** apaga a conta, o progresso, as respostas objetivas e discursivas, os simulados, as participações em duelos e os envios de desafio. Os votos saem junto. O duelo criado por quem excluiu a conta continua para quem jogou, sem o nome de quem criou. Os **posts e comentários da comunidade continuam, sem o nome** ("Participante que excluiu a conta"). Questões, desafios e avisos que um professor publicou continuam, assinados como "Equipe ENADE ADS".
- Registros técnicos de acesso ficam até 31 dias; cópias de segurança, até 30 dias.
- O conteúdo publicado por professores segue a licença Creative Commons BY-SA 4.0; o código, a licença MIT.

## Proteção contra abuso

- Os formulários de entrar, criar conta, esqueci a senha, reenviar confirmação, criar nova senha e códigos de 2FA aceitam no máximo **10 envios por minuto por IP**. Acima disso, a pessoa vê "Muitas tentativas em pouco tempo. Aguarde um minuto e tente de novo."
- **5 senhas erradas seguidas** bloqueiam a conta por **15 minutos**. O admin pode desbloquear antes pela tela *Contas*.
- Todo texto enviado tem limite de tamanho: comentário 2.000 caracteres, resposta discursiva 5.000, enunciado 8.000, post 20.000.
- O site envia cabeçalhos de segurança: política de conteúdo (CSP), proibição de ser exibido dentro de outro site (frame) e de adivinhar tipo de arquivo.

## Acesso ao conteúdo

- **Conteúdo é aberto:** roadmap, matérias, enunciados das questões, desafios e avisos podem ser vistos sem login.
- **Responder exige login:** marcar uma alternativa, enviar uma resposta discursiva e ver o gabarito ou o comentário exigem uma conta.
- O progresso (sessões do roadmap concluídas, questões respondidas e acertos) é salvo na conta de cada aluno e só ele vê o próprio progresso.

## Roadmap

- O roadmap mostra as matérias da portaria ou matriz da prova (20 em cada trilha do ENADE) e a Formação Geral, com seus assuntos e um checkpoint ao fim de cada bloco.
- Cada aluno marca matérias e assuntos como **Concluído**, **Estudando** ou **Pulei**, e pode limpar o status.
- A porcentagem conta só matérias e assuntos concluídos; checkpoints são marcos e não entram na conta.
- A contagem regressiva vai até o próximo dia de prova da trilha ativa, definido pelo **admin** na tela *Data da prova*, no fuso de São Paulo. A data não pode estar no passado.

### Edição do roadmap

- **Professor** e **Admin** editam o roadmap pela tela (link *Editar* no painel lateral e na página da matéria). Diferente de questões e posts, o roadmap é de todos os professores: qualquer professor edita qualquer tópico.
- Das **matérias** dá para editar título e explicação e reordenar dentro do bloco. Elas **não podem ser removidas**, porque vêm da matriz oficial da prova (no ENADE, as Portarias Inep nº 169/2026 para ADS e nº 172/2026 para Redes; no ENEM, a Matriz de Referência).
- **Assuntos** podem ser criados (entram no fim da matéria), editados, reordenados dentro da matéria e removidos.
- **Checkpoints** podem ser criados logo depois de uma matéria, editados, reordenados dentro do bloco e removidos.
- O endereço de um tópico (`/roadmap?topico=...`) não muda quando o título é editado, para links compartilhados continuarem valendo.
- Remover um assunto ou checkpoint apaga só o status que os alunos marcaram nele; o resto do progresso não muda.

## Simulados

**ENEM:** só questões objetivas, revezando entre as quatro áreas (Linguagens, Humanas, Natureza e Matemática) para nenhuma ficar de fora. Curto: 20 questões; médio: 45; completo: 90, como um dia de prova. O tempo segue o segundo dia do ENEM (5 horas para 90 questões). A redação não entra no simulado: ela tem assuntos e checkpoint no roadmap.

**ENADE:**

- O aluno logado monta um simulado com questões do banco em três tamanhos, todos com a proporção da prova do ENADE (Portarias Inep nº 169 e 172/2026): **Curto** (3 de formação geral + 7 específicas + 1 discursiva), **Médio** (7 + 15 + 1) e **Completo** (15 + 30 + 1).
- O tempo é proporcional ao da prova: 4 horas para 46 questões (o curto fica com cerca de 57 minutos). Mínimo de 10 minutos.
- Se o banco ainda não tem questões suficientes, o simulado usa as que existem (completando formação geral com específicas e vice-versa) e avisa antes de começar quantas questões e quanto tempo terá.
- O **prazo é gravado no servidor** quando o simulado começa. Recarregar a página ou trocar de aparelho não reinicia o relógio.
- O aluno pode salvar as respostas no meio do caminho e entregar quando quiser. Quando o tempo acaba, o simulado é **entregue automaticamente** com o que estava salvo. Há 1 minuto de tolerância para o envio feito no último segundo.
- Só existe **um simulado em andamento por aluno**. Ele precisa entregar (ou esperar o tempo acabar) para começar outro.
- O **gabarito comentado e o padrão de resposta só aparecem depois da entrega**. Questão em branco conta como errada.
- Depois da entrega, as respostas objetivas entram no painel de desempenho e no caderno de erros, e a discursiva vira uma resposta salva da questão.
- O simulado é do aluno: ninguém mais vê o que ele respondeu.

## Duelos

- Quem está logado cria um duelo com **5 questões objetivas** sorteadas da trilha ativa e recebe um link (`/duelos/<código>`). O código é aleatório, para ninguém achar duelos de outras pessoas.
- O duelo é da trilha em que foi criado: quem abre o link joga as questões dela, esteja na trilha que estiver.
- Qualquer pessoa logada com o link entra, até **7 dias** depois da criação e até **30 jogadores**. Visitante vê o convite e é chamado a entrar ou criar conta.
- Cada pessoa tem **10 minutos** a partir do "Começar". O relógio é do servidor. Depois do prazo (com 30 segundos de tolerância), o que for enviado não conta e as questões ficam em branco.
- Cada pessoa joga uma vez. **Placar:** mais acertos primeiro; no empate, menor tempo. O tempo para no limite de 10 minutos.
- O placar e o gabarito comentado aparecem **depois do envio**, só para quem jogou. O placar mostra o primeiro nome e a inicial do sobrenome.
- As respostas entram no painel de desempenho e no caderno de erros, como no simulado.
- **Revanche** cria um duelo novo, com outras questões da mesma trilha.

## Meta diária

- Cada pessoa escolhe quantas questões quer responder por dia: **5, 10 (padrão), 20 ou 30**. Ela troca a meta em Meu desempenho.
- O progresso de hoje conta as **questões diferentes** respondidas no dia (horário de Brasília), objetivas e discursivas, em qualquer prova e em qualquer lugar da plataforma.
- O cartão "Seu nível" mostra "Meta de hoje: X de N" e avisa quando ela é batida.
- A conquista **Meta batida** sai no primeiro dia em que a pessoa responde as questões da meta (pela meta atual).

## Estrelas por matéria

- Cada matéria mostra de 0 a 3 estrelas pelo acerto da pessoa nela, contando a resposta mais recente de cada questão (como em Meu desempenho): **1 estrela a partir de 40%, 2 a partir de 60% e 3 a partir de 80%**.
- As estrelas só aparecem depois de 5 questões respondidas na matéria, ou de todas, se a matéria tiver menos de 5.
- Aparecem no nó da matéria no roadmap e em Meu desempenho. O painel da matéria diz quanto falta para a próxima estrela.
- Só a própria pessoa vê as suas estrelas.

## Batalha da semana

- As trilhas disputam a semana, de segunda a domingo no horário de Brasília.
- Cada questão objetiva **acertada na semana** soma 1 ponto para a trilha da questão. Conta uma vez por pessoa e por questão, fora dos desafios.
- O início mostra a pontuação de cada prova, quantas pessoas pontuaram, a prova ativa em destaque e quem venceu a semana anterior. Em caso de empate na semana anterior, não aparece vencedor.
- **Só números por prova**, sem ranking de pessoas.

## Modo sobrevivência

- Partida solo na trilha ativa: questões objetivas uma por vez, com **3 vidas**. Errar tira uma vida; sem vidas (ou sem questões), a partida acaba.
- **A ordem é gravada no começo:** primeiro as questões que a turma mais acerta de primeira, depois as que mais erra, em faixas de 10 pontos sorteadas por dentro. As questões com menos de 3 pessoas respondendo entram como 50%.
- Depois de cada resposta aparecem a alternativa certa e a explicação dela. A próxima questão só vem no "Próxima".
- **Uma partida aberta por pessoa em cada trilha.** Começar outra encerra a anterior. Uma resposta que chega para uma questão que já passou (aba antiga) é ignorada.
- **Recorde:** o maior número de acertos numa partida, por trilha. Só a própria pessoa vê.
- As respostas valem como qualquer outra: desempenho, caderno de erros e XP. A conquista **Sobrevivente** sai com 10 acertos numa partida.

## Quantos acertaram

- Depois de responder uma questão objetiva, a página mostra a porcentagem de quem **acertou de primeira** e quantas pessoas responderam. Conta a primeira resposta de cada pessoa, fora dos desafios.
- Na questão do dia, a conta é só do dia.
- O número só aparece com **pelo menos 3 pessoas**, para não dar para saber como um colega específico foi.
- Questão com menos de 40% de acerto de primeira ganha o selo **Pegadinha**.

## Questão do dia

- Cada trilha tem **uma questão objetiva por dia**, sorteada no primeiro acesso do dia (fuso da plataforma) e gravada. É a mesma para todo mundo até a meia-noite.
- O sorteio evita questões que já foram do dia nos últimos 60 dias, enquanto o banco tiver outras.
- `/questao-do-dia` abre a da prova ativa. `/questao-do-dia/<prova>` é o link para compartilhar: abre a daquela prova, esteja quem abrir na trilha que estiver.
- A resposta vale como qualquer outra (desempenho, caderno de erros, XP). Depois de responder, aparecem o gabarito comentado e quantas pessoas responderam e acertaram hoje, contando a primeira resposta do dia de cada uma.
- Responder no próprio dia rende 5 XP a mais. A conquista **Pontual** sai com 5 questões do dia respondidas, cada uma no seu dia.

## XP, níveis e conquistas

- O XP é **calculado do que a pessoa já fez**, em todas as trilhas. Nada novo é gravado, então o histórico de antes desta funcionalidade conta.
- **Quanto vale:**

  | O quê | XP |
  |---|---|
  | Primeira resposta a uma questão objetiva | 2 |
  | Primeiro acerto de uma questão | 10 |
  | Discursiva respondida | 15 |
  | Simulado entregue (completo: +25) | 25 |
  | Desafio enviado | 20 |
  | Duelo enviado | 10 |
  | Duelo vencido, com pelo menos 2 jogadores e ao menos 1 acerto | 15 |
  | Tópico do roadmap concluído | 5 |
  | Questão do dia respondida no próprio dia (além do XP da questão) | 5 |

- **Repetir a mesma questão não dá XP de novo.** Ajuda a fixar o conteúdo, mas não soma pontos.
- **Acerto dentro de um desafio não dá XP de acerto**, só o envio do desafio. Senão o XP revelaria o gabarito antes de o desafio encerrar.
- **Níveis:** cada nível pede 50 XP a mais que o anterior (0, 50, 150, 300, 500...), de Calouro (1) a Lenda (9).
- **Sequência:** dias seguidos com alguma atividade, no fuso da plataforma. Quem estudou ontem e ainda não hoje mantém a sequência até o fim do dia.
- **Conquistas:** marcos fixos, como primeira resposta, 10 e 100 acertos, 3 e 7 dias seguidos, duelista, campeão, maratonista, gabaritou, no rumo certo, madrugador (estudar das 4h às 7h) e coruja (das 23h às 4h).
- O nível aparece no início e em Meu desempenho, só para a própria pessoa. Ao responder uma questão, a tela mostra o XP que a resposta rendeu.

## Painel de desempenho

- A página **Meu desempenho** é só do próprio aluno; ninguém mais vê os números dele.
- Em cada questão objetiva vale a **resposta mais recente**: errou e depois acertou, conta como acerto.
- A taxa de acerto aparece no total, por bloco e por matéria. **Revise primeiro** lista as 3 matérias com menor acerto entre as que o aluno já respondeu.
- O **caderno de erros** lista as questões em que a última resposta foi errada, com link para refazer. Ao acertar, a questão sai da lista. Questões arquivadas não aparecem.
- Respostas de um desafio **ainda aberto** não entram no painel, para não revelar quais questões estão erradas antes do gabarito sair. Depois que o desafio encerra, elas passam a contar.
- Discursivas não têm nota automática: o painel mostra só quantas o aluno respondeu.
- O painel também mostra o progresso no roadmap e a nota de cada desafio de que o aluno participou.

## Comunidade (página inicial)

A página inicial é um feed no estilo Reddit, onde professores publicam avisos e a turma conversa.

### Quem publica o quê

- **Qualquer pessoa lê** o feed, os posts e os comentários, sem conta.
- **Publicar, votar, comentar e denunciar exigem login.**
- **Aluno** publica direto, sem aprovação, nas categorias *Dúvidas*, *Discussão* e *Dicas de estudo*.
- **Professor** e **Admin** publicam também nas categorias de aviso oficial: *Datas e prazos*, *Portarias e editais* e *Avisos da turma*.
- Post de professor ou admin é **aviso oficial**: leva o selo *Professor*, aparece na aba *Avisos oficiais* e no feed RSS (`/blog/feed.xml`, os 20 mais recentes).
- Aluno publica no máximo **5 posts por hora** (contra spam). Professor e admin não têm limite.
- O post tem título, texto e categoria; resumo e link (sempre https) são opcionais. Sem resumo, o feed mostra o começo do texto.
- O post pode ficar como **rascunho** (só o autor vê) até ser publicado. O autor edita o próprio post; admin edita qualquer um.
- O endereço do post (`/blog/titulo-do-post`) sai do título e **não muda depois de publicado**, para links compartilhados continuarem valendo.
- **Arquivar** (autor ou admin) tira o post do feed; publicar de novo traz ele de volta.

### Feed e votos

- Abas: **Em alta** (padrão), **Novos** e **Avisos oficiais**, com filtro por categoria.
- Cada pessoa dá **um voto** por post, a favor (+1) ou contra (-1). Clicar de novo no mesmo voto desfaz; clicar no outro troca.
- *Em alta* combina pontuação e idade: `(pontos + 1) / (horas desde a publicação + 2)^1,5`. Um post bem votado sobe, mas não fica no topo para sempre.
- **Fixar** (professor nos próprios posts, admin em qualquer um) coloca o post no topo do feed, em todas as abas. Só um post fica fixado por vez.

### Moderação

- **Professor e admin removem** qualquer post. Post removido some do feed e o autor não consegue editar nem publicar de novo; ele ainda vê o post com o aviso de remoção. Professor e admin podem **restaurar**.
- O autor exclui o próprio comentário; professor e admin excluem qualquer comentário.
- Qualquer pessoa logada pode **denunciar** um post (com motivo opcional) ou um comentário. Denunciar de novo o mesmo item não duplica.
- A página **Moderação** (professor e admin) lista as denúncias abertas, agrupadas por item. *Remover* tira o conteúdo do ar; *Manter* encerra a denúncia sem mexer no conteúdo. Remover um post encerra também as denúncias dos comentários dele.
- O texto do post aceita a marcação mínima (parágrafos, listas, negrito, links e imagens por https). Comentários são texto puro.

## Desafios semanais

- **Professor** e **Admin** criam desafios: título, descrição, primeiro e último dia, e de 1 a 30 questões **objetivas** publicadas do banco.
- O desafio fica **agendado** antes do primeiro dia (só quem edita vê), **aberto** do primeiro ao último dia (no fuso de São Paulo) e **encerrado** depois, ou antes se o professor encerrar.
- Visitante vê as questões de um desafio aberto, sem gabarito. Para participar é preciso entrar.
- Cada aluno envia **uma vez** por desafio e precisa responder todas as questões. Logo depois vê a própria nota (quantas acertou), mas **não o gabarito**.
- Quando o desafio encerra, todos veem o gabarito comentado e o desempenho da turma: quantos participaram e a taxa de acerto de cada questão. Não há ranking com nomes.
- As questões de um desafio não mudam depois que algum aluno enviou respostas. Título, descrição e datas ainda podem ser ajustados.
- As respostas dadas no desafio contam só no desafio: não aparecem como tentativa na página da questão nem liberam o gabarito lá.
- **Limite conhecido:** as questões do desafio são do banco público. Quem procurar a mesma questão no banco consegue responder lá e ver o gabarito antes do desafio encerrar.

## Questões

- Visitante vê o enunciado e as alternativas, mas nunca a alternativa correta nem os comentários: eles não vão para a página.
- Ao responder, o aluno vê se acertou, qual é a correta e o comentário de **todas** as alternativas.
- Cada resposta fica registrada como uma tentativa. O aluno pode responder de novo quantas vezes quiser; a lista de questões mostra o resultado da tentativa mais recente.
- Toda questão informa a fonte: prova e ano do Inep, ou autoria própria.

### Questões das provas do Inep

- O banco já vem com as questões de ADS e de Redes de Computadores do ENADE 2021 e 2017 (formação geral e componente específico), com o gabarito definitivo do Inep. A formação geral é a mesma prova para os dois cursos, e cada trilha recebe a sua cópia das questões.
- Enunciado e alternativas são transcritos sem alterações (licença CC BY-ND do Inep). Os comentários das alternativas são da plataforma.
- As discursivas trazem um padrão de resposta escrito pela plataforma, marcado como não oficial.
- Questões anuladas pelo Inep não entram.
- A importação é feita ao subir a aplicação e nunca sobrescreve uma questão já importada: se um professor corrigir algo pela tela, a correção fica.

### Questões discursivas

- A discursiva tem enunciado, **padrão de resposta** (o que uma boa resposta deve conter) e um tamanho sugerido em linhas.
- Visitante vê só o enunciado. O aluno logado escreve a resposta e **só depois de enviar** vê o padrão de resposta, lado a lado com o que escreveu.
- A resposta não recebe nota automática: o aluno compara com o padrão. Na lista, a questão aparece como **Respondida**.
- Só o próprio aluno vê o que escreveu. Ele pode escrever de novo quantas vezes quiser.

### Publicação de questões

- **Professor** e **Admin** criam questões. Professor edita e arquiva só as que ele criou; Admin edita e arquiva qualquer uma.
- Toda questão tem matéria, enunciado e fonte. A objetiva tem 5 alternativas com texto e comentário, e exatamente uma correta; a discursiva tem padrão de resposta. A validação acontece no servidor.
- O tipo (objetiva ou discursiva) é escolhido ao criar e não muda depois.
- Antes de publicar, dá para pré-visualizar a questão como o aluno vai ver.
- **Arquivar** tira a questão das listas e da página pública. As respostas que os alunos já deram continuam no histórico deles.
- Editar uma questão não muda o resultado das respostas antigas: cada tentativa guarda se acertou no momento em que respondeu.
- O enunciado aceita só uma marcação mínima (parágrafos, negrito, código e imagens por https). Nenhum HTML digitado chega à página.
- Se o professor apagar a conta, as questões que ele publicou continuam na plataforma, sem autor.
