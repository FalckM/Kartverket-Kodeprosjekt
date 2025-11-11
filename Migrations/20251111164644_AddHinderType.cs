using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NRLWebApp.Migrations
{
    /// <inheritdoc />
    public partial class AddHinderType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Navn",
                table: "Hindre");

            migrationBuilder.AddColumn<int>(
                name: "HinderTypeID",
                table: "Hindre",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "HinderTyper",
                columns: table => new
                {
                    HinderTypeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Navn = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Beskrivelse = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HinderTyper", x => x.HinderTypeID);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Hindre_HinderTypeID",
                table: "Hindre",
                column: "HinderTypeID");

            migrationBuilder.AddForeignKey(
                name: "FK_Hindre_HinderTyper_HinderTypeID",
                table: "Hindre",
                column: "HinderTypeID",
                principalTable: "HinderTyper",
                principalColumn: "HinderTypeID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Hindre_HinderTyper_HinderTypeID",
                table: "Hindre");

            migrationBuilder.DropTable(
                name: "HinderTyper");

            migrationBuilder.DropIndex(
                name: "IX_Hindre_HinderTypeID",
                table: "Hindre");

            migrationBuilder.DropColumn(
                name: "HinderTypeID",
                table: "Hindre");

            migrationBuilder.AddColumn<string>(
                name: "Navn",
                table: "Hindre",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
