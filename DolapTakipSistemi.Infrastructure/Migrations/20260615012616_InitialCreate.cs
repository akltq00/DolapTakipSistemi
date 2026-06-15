using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DolapTakipSistemi.Infrastructure.Migrations;

public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Dolaplar",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Numara = table.Column<int>(type: "int", nullable: false),
                OgrenciAdSoyad = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                OkulNumarasi = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                ZimmetSifreHash = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                ZimmetTarihi = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Dolaplar", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Dolaplar_Numara",
            table: "Dolaplar",
            column: "Numara",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "Dolaplar");
    }
}
