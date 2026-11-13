# CitizenTech - Fila Humana — Backend

## Problema

Cidadãos que dependem dos serviços públicos de saúde em São Paulo enfrentam dificuldade para encontrar informações sobre a demanda e o tempo de espera nas unidades, o que gera deslocamentos desnecessários e dificulta o acesso ao atendimento.

## Proposta

Uma plataforma que monitora a demanda de serviços públicos, estima períodos de maior movimento e orienta cidadãos sobre onde e quando procurar atendimento.

O foco inicial será a rede de saúde pública de São Paulo, especialmente UBS e AMAs. Futuramente, a solução poderá ser expandida para Poupatempo e outros serviços municipais.

Nosso projeto busca informar ao cidadão informações significativas sobre o estado de lotação desses serviços, incluindo:
- Qual unidade está mais cheia.
- Quanto tempo provavelmente vai esperar.
- Qual horário costuma ter menor movimento.
- Se existe outra unidade próxima com menor demanda.
- Se vale a pena sair de casa naquele momento.

Dessa forma, propomos reduzir deslocamentos desnecessários, perda de horas, superlotação em determinados horários e dificuldade de organização para quem precisa trabalhar, cuidar de filhos ou acompanhar idosos.

**Público impactado:**
- Pessoas que dependem exclusivamente do SUS.
- Idosos e pessoas com mobilidade reduzida.
- Trabalhadores com pouco tempo disponível.
- Pais e responsáveis por crianças.
- Pessoas que precisam de atendimento espontâneo.

**ODS relacionados:**
- ODS 3 — Saúde e Bem-Estar.
- ODS 10 — Redução das Desigualdades.
- ODS 11 — Cidades e Comunidades Sustentáveis.
- ODS 9 — Indústria, Inovação e Infraestrutura.

---

## Nomes dos integrantes

| Integrante | RM |
|---|---|
| Denise Senise | 556006 |
| Larissa Rodrigues Lapa | 554517 |
| Mateus Leme | 557803 |
| David Gabriel Gomes Fernandes | 556020 |
| Vinicius Augusto Neves Prestes | 559097 |

---

## Como executar

**Pré-requisito:** .NET SDK 10.0 (`dotnet --version`).

```bash
# 1. Restaura e compila tudo
dotnet build FilaHumana.slnx

# 2. Sobe a API (cria o banco SQLite e popula os dados de demonstração na 1ª execução)
dotnet run --project src/FilaHumana.Api
#    Padrão do projeto (launchSettings.json): http://localhost:5153

# 3. Abre a documentação
#    Swagger UI : http://localhost:5153/swagger
#    Health     : http://localhost:5153/health
```

**Testes (22 testes: entidades, serviço com mocks e repositórios com SQLite em memória):**

```bash
dotnet test FilaHumana.slnx
```

**Migrações (EF Core):**

```bash
dotnet tool install --global dotnet-ef   # uma vez
dotnet ef migrations add <Nome> --project src/FilaHumana.Infrastructure --startup-project src/FilaHumana.Api
dotnet ef database update                --project src/FilaHumana.Infrastructure --startup-project src/FilaHumana.Api
```

---

## Estrutura (separação em camadas)

```
FilaHumana.slnx
├── src/
│   ├── FilaHumana.Domain/          # Regras de negócio — SEM dependência de EF
│   │   ├── Entities/               #   Local, LeituraDeFila, Usuario (POO)
│   │   ├── Enums/                  #   StatusLotacao, OrigemLeitura
│   │   ├── Abstractions/           #   IRepositorioLocais, IRepositorioLeituras
│   │   ├── Models/                 #   LotaçãoAtual, MelhorHorario (DTOs de domínio)
│   │   └── Services/               #   IServicoLotacao + ServicoLotacao (regras)
│   ├── FilaHumana.Infrastructure/  # Persistência — EF Core + SQLite
│   │   ├── Dados/                  #   FilaHumanaContext, Fluent API, migrações, seed
│   │   └── Repositorios/           #   implementações assíncronas dos repositórios
│   └── FilaHumana.Api/             # Apresentação — controllers REST + Swagger
│       ├── Controllers/            #   LocaisController, LeiturasController
│       └── Models/                 #   DTOs de entrada/saída
└── tests/
    └── FilaHumana.Tests/           # xUnit + Moq (22 testes)
```

O `Domain` não referencia o EF: os controllers dependem das **interfaces**, e o EF é injetado
por **injeção de dependência** no `Program.cs` — trocar SQLite por Postgres não toca na regra de negócio.

---

## Endpoints

| Método | Rota | Descrição |
|---|---|---|
| GET | `/api/locais` | Lista os locais monitorados |
| GET | `/api/locais/{id}` | Detalhe de um local (404 se não existir) |
| POST | `/api/locais` | Cadastra um local (400 com dados inválidos) |
| GET | `/api/locais/{id}/lotacao` | **Lotação atual** com status `Vazio`/`Confortavel`/`Movimentado`/`Lotado` |
| GET | `/api/locais/{id}/melhor-horario?dias=30` | **Melhor horário para ir**: horas mais vazias e de pico (LINQ sobre as leituras) |
| POST | `/api/leituras` | Registra uma contagem — **endpoint chamado pelo ESP32** |
| GET | `/api/leituras?localId=1&desde=&ate=` | Histórico de leituras por período |
| GET | `/health` | Health check (usado pelo monitoramento da VM) |

Exemplo de resposta de melhor horário:

```json
{
  "localId": 1,
  "nome": "UBS Vila Mariana",
  "diasAnalisados": 14,
  "melhoresHorarios": [ { "hora": 19, "mediaPessoas": 1.6, "amostras": 13 } ],
  "horariosPico":     [ { "hora": 9,  "mediaPessoas": 9.4, "amostras": 14 } ]
}
```

### Regra de negócio central

`Local.AvaliarStatus(quantidade)` compara a contagem com a `CapacidadeConfortavel`:

| Relação | Status |
|---|---|
| < 50% | `Vazio` |
| 50% – 100% | `Confortavel` |
| 100% – 150% | `Movimentado` |
| ≥ 150% | `Lotado` |

---

## Dados

O banco **SQLite** (`fila-humana.db`) é criado automaticamente a partir das **migrações**.
Na primeira execução são gravados 4 locais fictícios (2 UBS + 2 Poupatempo) e 14 dias de
leituras de demonstração com padrão realista (pico de manhã e início da tarde, menos movimento
nos fins de semana) para o app e o cálculo de melhor horário terem o que exibir.

> ⚠️ Esses registros são **fictícios, para demonstração** — nenhum dado pessoal real é usado.
