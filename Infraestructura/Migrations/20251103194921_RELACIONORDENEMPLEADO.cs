using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infraestructura.Migrations
{
    /// <inheritdoc />
    public partial class RELACIONORDENEMPLEADO : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmpleadoMail",
                table: "OrdenesDeInspeccion",
                type: "nvarchar(200)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesDeInspeccion_EmpleadoMail",
                table: "OrdenesDeInspeccion",
                column: "EmpleadoMail");

            migrationBuilder.AddForeignKey(
                name: "FK_OrdenesDeInspeccion_Empleados_EmpleadoMail",
                table: "OrdenesDeInspeccion",
                column: "EmpleadoMail",
                principalTable: "Empleados",
                principalColumn: "Mail",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrdenesDeInspeccion_Empleados_EmpleadoMail",
                table: "OrdenesDeInspeccion");

            migrationBuilder.DropIndex(
                name: "IX_OrdenesDeInspeccion_EmpleadoMail",
                table: "OrdenesDeInspeccion");

            migrationBuilder.DropColumn(
                name: "EmpleadoMail",
                table: "OrdenesDeInspeccion");
        }
    }
}
