using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wisdom.Media.Infrastructure.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueLessonIdIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Media_LessonId",
                table: "Media",
                column: "LessonId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Media_LessonId",
                table: "Media");
        }
    }
}
