using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FilaHumana.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Locais",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    Tipo = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    Bairro = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    Endereco = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    CapacidadeConfortavel = table.Column<int>(type: "INTEGER", nullable: false),
                    Ativo = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locais", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    ReceberAlerta = table.Column<bool>(type: "INTEGER", nullable: false),
                    CadastradoEm = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LeiturasDeFila",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LocalId = table.Column<int>(type: "INTEGER", nullable: false),
                    QuantidadePessoas = table.Column<int>(type: "INTEGER", nullable: false),
                    RegistradoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Origem = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeiturasDeFila", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeiturasDeFila_Locais_LocalId",
                        column: x => x.LocalId,
                        principalTable: "Locais",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Locais",
                columns: new[] { "Id", "Ativo", "Bairro", "CapacidadeConfortavel", "Endereco", "Nome", "Tipo" },
                values: new object[,]
                {
                    { 1, true, "Vila Mariana", 12, "Rua das Palmeiras, 100", "UBS Vila Mariana", "UBS" },
                    { 2, true, "Butantã", 10, "Av. Professor Alfonso Bovero, 500", "UBS Butantã", "UBS" },
                    { 3, true, "Sé", 20, "Praça da Sé, s/n", "Poupatempo Sé", "Poupatempo" },
                    { 4, true, "Tatuapé", 16, "Av. Celso Garcia, 1500", "Poupatempo Tatuapé", "Poupatempo" }
                });

            migrationBuilder.InsertData(
                table: "Usuarios",
                columns: new[] { "Id", "CadastradoEm", "Email", "Nome", "ReceberAlerta" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 11, 1, 0, 0, 0, 0, DateTimeKind.Utc), "demo@filahumana.test", "Cidadão de Demonstração", true },
                    { 2, new DateTime(2026, 11, 1, 0, 0, 0, 0, DateTimeKind.Utc), "teste@filahumana.test", "Usuária de Teste", false }
                });

            migrationBuilder.CreateIndex(
                name: "IX_LeiturasDeFila_LocalId_RegistradoEm",
                table: "LeiturasDeFila",
                columns: new[] { "LocalId", "RegistradoEm" });

            migrationBuilder.CreateIndex(
                name: "IX_Locais_Bairro",
                table: "Locais",
                column: "Bairro");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LeiturasDeFila");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Locais");
        }
    }
}
