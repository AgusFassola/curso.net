using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BibliotecaAPI.Migrations
{
    /// <inheritdoc />
    public partial class AutorFoto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FechaDNacimiento",
                table: "AspNetUsers",
                newName: "FechaNacimiento");

            migrationBuilder.AddColumn<string>(
                name: "Foto",
                table: "Autores",
                type: "varchar(max)",
                unicode: false,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Foto",
                table: "Autores");

            migrationBuilder.RenameColumn(
                name: "FechaNacimiento",
                table: "AspNetUsers",
                newName: "FechaDNacimiento");
        }
    }
}
