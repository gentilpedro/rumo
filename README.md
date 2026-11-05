# Rumo

**Plataforma de estudos gratuita e unificada para as provas que abrem portas.** Em vez de um site para cada exame, o Rumo reúne num lugar só o que o estudante precisa para chegar preparado: o que estudar, em que ordem, questões no formato da prova com gabarito comentado, simulados cronometrados e uma comunidade com professores de verdade.

**Acesse: [rumo.aamora.com.br](https://rumo.aamora.com.br)**. Ler é livre, sem conta. Para responder, votar e publicar, basta se cadastrar.

O código é aberto (MIT) e o conteúdo cresce com quem usa: professores propõem questões e ajustes no roadmap, e outro professor revisa antes de publicar.

## Uma plataforma, várias provas

Cada prova é uma **trilha**. Ao entrar, o estudante escolhe a dele, e a marca passa a mostrar onde ele está: "Rumo ENEM", "Rumo ENADE ADS", "Rumo ENADE Redes". Dá para trocar de trilha a qualquer momento, e cada uma tem roadmap, questões, desafios, simulados e datas próprios.

| Trilha | Para quem | Situação |
|---|---|---|
| **ENEM 2026** | Quem está terminando o ensino médio. Provas em 08/11 e 15/11/2026 | No ar: roadmap pela Matriz de Referência (4 áreas e redação), questões de autoria própria e simulado no ritmo do ENEM |
| **ENADE 2026 · ADS** | Estudantes de Tecnologia em Análise e Desenvolvimento de Sistemas. Prova em 29/11/2026 | No ar: roadmap pela Portaria Inep nº 169/2026, provas reais de 2017 e 2021 e simulado no formato do ENADE |
| **ENADE 2026 · Redes de Computadores** | Estudantes de Tecnologia em Redes de Computadores. Prova em 29/11/2026 | No ar: roadmap pela Portaria Inep nº 172/2026, provas reais de 2017 e 2021 e simulado no formato do ENADE |
| **Seletivos de IFs** (como o IFSul) | Quem vai entrar no ensino médio técnico | Próxima trilha |

## O que tem em cada trilha

- **Roadmap de estudos:** as matérias da prova em ordem, divididas em assuntos e checkpoints. O progresso fica salvo na conta.
- **Questões com gabarito comentado:** cada alternativa explica por que está certa ou errada. As discursivas trazem um padrão de resposta.
- **Simulados cronometrados** no formato da prova, com questões das várias áreas e correção ao final.
- **Questão do dia:** uma questão por dia em cada prova, a mesma para todo mundo, com quantos acertaram e um link para mandar à turma.
- **Modo sobrevivência:** questões em sequência, das mais fáceis para as mais difíceis, até errar 3. Vale bater o próprio recorde.
- **Estrelas no roadmap:** cada matéria ganha até 3 estrelas pelo seu acerto, como fases de um jogo.
- **Batalha da semana:** as provas disputam a semana; cada acerto soma um ponto para a sua.
- **Duelos:** 5 questões, 10 minutos e um link para mandar no grupo da turma. Ganha quem acertar mais; no empate, quem foi mais rápido.
- **Desafios semanais** publicados pelos professores.
- **Meu desempenho:** acertos por matéria, para saber onde focar.
- **XP, níveis e conquistas:** cada questão nova, simulado, desafio e duelo rende XP. A sequência de dias estudando fica no início.
- **Grade do semestre:** quantas semanas faltam, os desafios de cada semana e o dia da prova.
- **Comunidade:** feed com avisos oficiais, dúvidas e votos, moderado por professores.

## Quem participa

- **Estudantes:** todo cadastro começa como estudante. Só cria conta quem tem 13 anos ou mais; de 13 a 17 anos, com autorização do responsável legal.
- **Professores:** quem se cadastra com e-mail institucional (`.edu`) e marca "Sou professor" vira professor depois da aprovação de um administrador. Professores criam e corrigem questões, mexem no roadmap e publicam desafios. **Cada edição passa por revisão:** ela só vale depois que outro professor ou um administrador aprova, na página *Revisões*.
- **Administradores:** aprovam professores, cuidam das contas e definem as datas das provas.

As regras completas estão em [docs/regras-de-negocio.md](docs/regras-de-negocio.md).

## Privacidade

O Rumo coleta o mínimo: nome, e-mail e a faixa de idade, sem data de nascimento, documento ou escola. Os termos de uso e a política de privacidade seguem a LGPD, incluindo o art. 14, sobre crianças e adolescentes. Cada pessoa pode baixar todos os seus dados ou excluir a conta pela própria página da conta.

## De onde vêm as questões

- **ENADE (Inep):** as questões das provas de 2021 e 2017 estão em `EnadeAds/Data/Seed/inep/*.json` (ADS e Formação Geral) e `EnadeAds/Data/Seed/redes/*.json` (Redes de Computadores). As figuras ficam em `EnadeAds/wwwroot/img/inep/`.
  - **Formação Geral:** é a mesma prova para todos os cursos do ano. O arquivo dela lista as outras trilhas em `sharedWith`, e cada trilha recebe a sua cópia.
  - **Fonte:** as provas e os gabaritos definitivos de [download.inep.gov.br](https://download.inep.gov.br/enade/provas_e_gabaritos/).
  - **Licença:** o portal do Inep publica sob [CC BY-ND 3.0](https://creativecommons.org/licenses/by-nd/3.0/deed.pt_BR). Por isso enunciados e alternativas são transcritos sem alteração, e cada questão cita a prova, o ano e o número.
  - **Anuladas:** a questão 15 de ADS 2017 e a 21 de Redes 2017 foram anuladas pelo Inep e não entram.
- **ENEM:** as questões de `EnadeAds/Data/Seed/enem/` são de autoria própria. As provas oficiais do ENEM vão entrar numa próxima versão.
- **Comentários e padrões de resposta** são da plataforma e ficam sob [CC BY-SA 4.0](https://creativecommons.org/licenses/by-sa/4.0/deed.pt_BR).

As questões entram quando a aplicação sobe. Só são importadas as que ainda não existem, então as correções feitas pelos professores na tela não são sobrescritas.

## Como funciona por dentro

- Blazor Web App .NET 10, a partir do template [`GentilPedro.Templates.Blazor10`](https://github.com/gentilpedro/template-dotnet-blazor10). As páginas são renderizadas no servidor, com formulários, e funcionam sem JavaScript.
- PostgreSQL com EF Core. Um filtro global recorta roadmap, questões, desafios, simulados e posts pela trilha ativa.
- ASP.NET Core Identity com confirmação de e-mail, perfis e limite de tentativas por IP.
- Bootstrap restilizado, tema claro e escuro.
- Serilog e xUnit.

## Rodando localmente

Requer o [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) e o Docker (para o PostgreSQL).

```bash
docker compose up -d          # PostgreSQL local
dotnet tool restore           # dotnet-ef
dotnet run --project EnadeAds # aplica as migrations e importa as questões ao subir
dotnet test                   # usa SQLite em memória, não precisa do Docker
```

Em desenvolvimento, sem SMTP configurado, o link de confirmação de e-mail aparece na própria tela depois do cadastro.

Migrations novas: `dotnet ef migrations add <Nome> --project EnadeAds --output-dir Data/Migrations`.

O projeto e o namespace ainda se chamam `EnadeAds`, o nome de quando a plataforma era só do ENADE. A troca para `Rumo` fica para outra hora.

## Configuração em produção

Use variáveis de ambiente (o `__` separa as seções):

| Variável | Para quê |
|---|---|
| `ConnectionStrings__DefaultConnection` | String de conexão do PostgreSQL |
| `Admin__Emails__0`, `Admin__Emails__1`... | E-mails que recebem o perfil Admin ao se cadastrar |
| `Smtp__Host`, `Smtp__Port`, `Smtp__EnableSsl`, `Smtp__UserName`, `Smtp__Password`, `Smtp__FromAddress` | Envio dos e-mails de confirmação e de senha |
| `ReverseProxy__KnownNetworks__0` | Rede do proxy reverso (ex.: `172.18.0.0/16`), para ler o IP real do `X-Forwarded-For` |
| `Privacy__ControllerName`, `Privacy__ContactEmail` | Quem responde pelos dados desta instalação e o e-mail para pedidos de titulares (LGPD), mostrados na política de privacidade |
| `RateLimit__AccountPostsPerMinute` | Envios de formulários de conta por IP por minuto (padrão 10) |
| `DataProtection__KeysPath` | Pasta das chaves de login. Num container, precisa ser um volume para ninguém perder a sessão a cada deploy |
| `Analytics__Origin` | Origem do serviço de contagem de visitas (ex.: `https://stats.aamora.com.br`, um Umami). Libera essa origem na CSP e conta, na política de privacidade, que existe contagem anônima. O script é inserido pelo proxy reverso |

Sem SMTP, a aplicação sobe fora de desenvolvimento, mas não envia e-mail nem mostra o link de confirmação na tela. Mostrar o link deixaria qualquer um confirmar um endereço `.edu` que não é seu. Até o SMTP ser configurado, ninguém confirma conta nova, e o log avisa a cada e-mail não enviado.

A instalação oficial roda numa VPS com Docker e Nginx, e cada tag `vX.Y.Z` publica uma versão nova. Os detalhes estão em [deploy/vps/README.md](deploy/vps/README.md).

## Contribuindo

Questões, correções e trilhas novas são bem-vindas. Veja [CONTRIBUTING.md](CONTRIBUTING.md).

## Licença

O código está sob a [MIT](LICENSE). O conteúdo próprio da plataforma está sob a CC BY-SA 4.0, e as provas do Inep, sob a CC BY-ND 3.0.
