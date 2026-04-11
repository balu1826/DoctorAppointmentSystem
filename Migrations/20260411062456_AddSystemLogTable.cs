using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoctorAppointmentSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddSystemLogTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
        name: "SystemLogs",
        columns: table => new
        {
            Id = table.Column<int>(nullable: false)
                .Annotation("SqlServer:Identity", "1, 1"),

            UserId = table.Column<string>(nullable: true),

            ApiEndpoint = table.Column<string>(nullable: true),

            HttpMethod = table.Column<string>(nullable: true),

            ExceptionMessage = table.Column<string>(nullable: true),

            StackTrace = table.Column<string>(nullable: true),

            CreatedAt = table.Column<DateTime>(nullable: false)
        },
        constraints: table =>
        {
            table.PrimaryKey("PK_SystemLogs", x => x.Id);
        });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
