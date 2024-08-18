using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.Migrations
{
    /// <inheritdoc />
    public partial class AddCollection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserID",
                table: "Samples",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PublicationStatus",
                table: "Collections",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UserID",
                table: "Collections",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Samples_UserID",
                table: "Samples",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Collections_UserID",
                table: "Collections",
                column: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_Collections_Users_UserID",
                table: "Collections",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Samples_Users_UserID",
                table: "Samples",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Collections_Users_UserID",
                table: "Collections");

            migrationBuilder.DropForeignKey(
                name: "FK_Samples_Users_UserID",
                table: "Samples");

            migrationBuilder.DropIndex(
                name: "IX_Samples_UserID",
                table: "Samples");

            migrationBuilder.DropIndex(
                name: "IX_Collections_UserID",
                table: "Collections");

            migrationBuilder.DropColumn(
                name: "UserID",
                table: "Samples");

            migrationBuilder.DropColumn(
                name: "PublicationStatus",
                table: "Collections");

            migrationBuilder.DropColumn(
                name: "UserID",
                table: "Collections");
        }
    }
}
