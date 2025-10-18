using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infraestructura.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Empleados",
                columns: table => new
                {
                    Mail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Apellido = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Empleados", x => x.Mail);
                });

            migrationBuilder.CreateTable(
                name: "EstacionesSismologica",
                columns: table => new
                {
                    CodigoEstacion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DocumentoCertificacionAdquirida = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FechaSolicitudCertificacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Latitud = table.Column<double>(type: "float", nullable: false),
                    Longitud = table.Column<double>(type: "float", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NroCertificacionAdquisicion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstacionesSismologica", x => x.CodigoEstacion);
                });

            migrationBuilder.CreateTable(
                name: "Estados",
                columns: table => new
                {
                    Ambito = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NombreEstado = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Estados", x => new { x.Ambito, x.NombreEstado });
                });

            migrationBuilder.CreateTable(
                name: "MotivosTipo",
                columns: table => new
                {
                    TipoMotivo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MotivosTipo", x => x.TipoMotivo);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    EmpleadoMail = table.Column<string>(type: "nvarchar(200)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Nombre);
                    table.ForeignKey(
                        name: "FK_Roles_Empleados_EmpleadoMail",
                        column: x => x.EmpleadoMail,
                        principalTable: "Empleados",
                        principalColumn: "Mail",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    NombreUsuario = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Contraseña = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EmpleadoMail = table.Column<string>(type: "nvarchar(200)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.NombreUsuario);
                    table.ForeignKey(
                        name: "FK_Usuarios_Empleados_EmpleadoMail",
                        column: x => x.EmpleadoMail,
                        principalTable: "Empleados",
                        principalColumn: "Mail",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrdenesDeInspeccion",
                columns: table => new
                {
                    NroOrden = table.Column<int>(type: "int", nullable: false),
                    FechaHoraInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaHoraFinalizacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaHoraCierre = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ObservacionCierre = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Ambito = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    NombreEstado = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    CodigoEstacion = table.Column<string>(type: "nvarchar(50)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrdenesDeInspeccion", x => x.NroOrden);
                    table.ForeignKey(
                        name: "FK_OrdenesDeInspeccion_EstacionesSismologica_CodigoEstacion",
                        column: x => x.CodigoEstacion,
                        principalTable: "EstacionesSismologica",
                        principalColumn: "CodigoEstacion",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrdenesDeInspeccion_Estados_Ambito_NombreEstado",
                        columns: x => new { x.Ambito, x.NombreEstado },
                        principalTable: "Estados",
                        principalColumns: new[] { "Ambito", "NombreEstado" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Sismografos",
                columns: table => new
                {
                    IdentificacionSismografo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FechaAdquisicion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NroSerie = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Ambito = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    NombreEstado = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    EstacionSismologicaCodigoEstacion = table.Column<string>(type: "nvarchar(50)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sismografos", x => x.IdentificacionSismografo);
                    table.ForeignKey(
                        name: "FK_Sismografos_EstacionesSismologica_EstacionSismologicaCodigoEstacion",
                        column: x => x.EstacionSismologicaCodigoEstacion,
                        principalTable: "EstacionesSismologica",
                        principalColumn: "CodigoEstacion",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Sismografos_Estados_Ambito_NombreEstado",
                        columns: x => new { x.Ambito, x.NombreEstado },
                        principalTable: "Estados",
                        principalColumns: new[] { "Ambito", "NombreEstado" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Sesiones",
                columns: table => new
                {
                    FechaHoraDesde = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaHoraHasta = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioNombreUsuario = table.Column<string>(type: "nvarchar(100)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sesiones", x => x.FechaHoraDesde);
                    table.ForeignKey(
                        name: "FK_Sesiones_Usuarios_UsuarioNombreUsuario",
                        column: x => x.UsuarioNombreUsuario,
                        principalTable: "Usuarios",
                        principalColumn: "NombreUsuario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CambioEstados",
                columns: table => new
                {
                    FechaHoraInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Ambito = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NombreEstado = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FechaHoraFin = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SismografoIdentificacionSismografo = table.Column<string>(type: "nvarchar(100)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CambioEstados", x => new { x.FechaHoraInicio, x.Ambito, x.NombreEstado });
                    table.ForeignKey(
                        name: "FK_CambioEstados_Estados_Ambito_NombreEstado",
                        columns: x => new { x.Ambito, x.NombreEstado },
                        principalTable: "Estados",
                        principalColumns: new[] { "Ambito", "NombreEstado" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CambioEstados_Sismografos_SismografoIdentificacionSismografo",
                        column: x => x.SismografoIdentificacionSismografo,
                        principalTable: "Sismografos",
                        principalColumn: "IdentificacionSismografo",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MotivosFueraServicio",
                columns: table => new
                {
                    Comentario = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CambioEstadoAmbito = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    CambioEstadoFechaHoraInicio = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CambioEstadoNombreEstado = table.Column<string>(type: "nvarchar(50)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MotivosFueraServicio", x => x.Comentario);
                    table.ForeignKey(
                        name: "FK_MotivosFueraServicio_CambioEstados_CambioEstadoFechaHoraInicio_CambioEstadoAmbito_CambioEstadoNombreEstado",
                        columns: x => new { x.CambioEstadoFechaHoraInicio, x.CambioEstadoAmbito, x.CambioEstadoNombreEstado },
                        principalTable: "CambioEstados",
                        principalColumns: new[] { "FechaHoraInicio", "Ambito", "NombreEstado" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CambioEstados_Ambito_NombreEstado",
                table: "CambioEstados",
                columns: new[] { "Ambito", "NombreEstado" });

            migrationBuilder.CreateIndex(
                name: "IX_CambioEstados_SismografoIdentificacionSismografo",
                table: "CambioEstados",
                column: "SismografoIdentificacionSismografo");

            migrationBuilder.CreateIndex(
                name: "IX_MotivosFueraServicio_CambioEstadoFechaHoraInicio_CambioEstadoAmbito_CambioEstadoNombreEstado",
                table: "MotivosFueraServicio",
                columns: new[] { "CambioEstadoFechaHoraInicio", "CambioEstadoAmbito", "CambioEstadoNombreEstado" });

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesDeInspeccion_Ambito_NombreEstado",
                table: "OrdenesDeInspeccion",
                columns: new[] { "Ambito", "NombreEstado" });

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesDeInspeccion_CodigoEstacion",
                table: "OrdenesDeInspeccion",
                column: "CodigoEstacion");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_EmpleadoMail",
                table: "Roles",
                column: "EmpleadoMail");

            migrationBuilder.CreateIndex(
                name: "IX_Sesiones_UsuarioNombreUsuario",
                table: "Sesiones",
                column: "UsuarioNombreUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_Sismografos_Ambito_NombreEstado",
                table: "Sismografos",
                columns: new[] { "Ambito", "NombreEstado" });

            migrationBuilder.CreateIndex(
                name: "IX_Sismografos_EstacionSismologicaCodigoEstacion",
                table: "Sismografos",
                column: "EstacionSismologicaCodigoEstacion");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_EmpleadoMail",
                table: "Usuarios",
                column: "EmpleadoMail");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MotivosFueraServicio");

            migrationBuilder.DropTable(
                name: "MotivosTipo");

            migrationBuilder.DropTable(
                name: "OrdenesDeInspeccion");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Sesiones");

            migrationBuilder.DropTable(
                name: "CambioEstados");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Sismografos");

            migrationBuilder.DropTable(
                name: "Empleados");

            migrationBuilder.DropTable(
                name: "EstacionesSismologica");

            migrationBuilder.DropTable(
                name: "Estados");
        }
    }
}
