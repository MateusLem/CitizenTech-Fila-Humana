using FilaHumana.Domain.Abstractions;
using FilaHumana.Domain.Services;
using FilaHumana.Infrastructure.Dados;
using FilaHumana.Infrastructure.Repositorios;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// --- Camada de apresentação -------------------------------------------------
// Enums (StatusLotacao, OrigemLeitura) como texto: "Movimentado", "Sensor".
builder.Services
    .AddControllers()
    .AddJsonOptions(opcoes =>
        opcoes.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter()));

builder.Services.AddEndpointsApiExplorer();

// Documentação automática dos endpoints com comentários XML do código.
builder.Services.AddSwaggerGen(opcoes =>
{
    opcoes.SwaggerDoc("v1", new()
    {
        Title = "API Fila Humana",
        Version = "v1",
        Description = "CitizenTech Fila Humana — lotação atual e melhor horário de UBS e Poupatempo."
    });

    var arquivoXml = Path.Combine(AppContext.BaseDirectory, "FilaHumana.Api.xml");
    if (File.Exists(arquivoXml))
    {
        opcoes.IncludeXmlComments(arquivoXml);
    }
});

// --- Camada de infraestrutura (EF Core + SQLite) ----------------------------
var connectionString = builder.Configuration.GetConnectionString("FilaHumana")
    ?? "Data Source=fila-humana.db";

builder.Services.AddDbContext<FilaHumanaContext>(opcoes => opcoes.UseSqlite(connectionString));
builder.Services.AddScoped<IRepositorioLocais, RepositorioLocais>();
builder.Services.AddScoped<IRepositorioLeituras, RepositorioLeituras>();

// --- Regras de negócio (Domain) --------------------------------------------
builder.Services.AddScoped<IServicoLotacao, ServicoLotacao>();

var app = builder.Build();

// Cria/atualiza o banco a partir das migrações e popula o demo na 1ª execução.
using (var escopo = app.Services.CreateScope())
{
    var db = escopo.ServiceProvider.GetRequiredService<FilaHumanaContext>();
    db.Database.Migrate();
    await DadosDeDemonstracao.GarantirLeiturasAsync(db);
}

// Swagger sempre ativo para facilitar as evidências da entrega.
app.UseSwagger();
app.UseSwaggerUI(opcoes =>
{
    opcoes.SwaggerEndpoint("/swagger/v1/swagger.json", "Fila Humana API v1");
    opcoes.DocumentTitle = "Fila Humana — Documentação da API";
});

app.MapControllers();

// Health check simples (útil para o monitoramento da disciplina de SO).
app.MapGet("/health", async (FilaHumanaContext db, CancellationToken cancellationToken) =>
    await db.Database.CanConnectAsync(cancellationToken)
        ? Results.Ok(new { status = "ok", horarioUtc = DateTime.UtcNow })
        : Results.Json(new { status = "sem banco de dados" }, statusCode: StatusCodes.Status503ServiceUnavailable));

app.Run();
