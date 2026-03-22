using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManager.Api.Migrations
{
    /// <inheritdoc />
    public partial class DeletePerformersIdInTaskAndAddUsernameAndTaskInJoinRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PerfomersId",
                table: "Tasks");

            migrationBuilder.RenameColumn(
                name: "ReviwedAt",
                table: "JoinToTaskRequests",
                newName: "ReviewedAt");

            migrationBuilder.AddColumn<string>(
                name: "UserName",
                table: "JoinToTaskRequests",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_JoinToTaskRequests_TaskId",
                table: "JoinToTaskRequests",
                column: "TaskId");

            migrationBuilder.AddForeignKey(
                name: "FK_JoinToTaskRequests_Tasks_TaskId",
                table: "JoinToTaskRequests",
                column: "TaskId",
                principalTable: "Tasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JoinToTaskRequests_Tasks_TaskId",
                table: "JoinToTaskRequests");

            migrationBuilder.DropIndex(
                name: "IX_JoinToTaskRequests_TaskId",
                table: "JoinToTaskRequests");

            migrationBuilder.DropColumn(
                name: "UserName",
                table: "JoinToTaskRequests");

            migrationBuilder.RenameColumn(
                name: "ReviewedAt",
                table: "JoinToTaskRequests",
                newName: "ReviwedAt");

            migrationBuilder.AddColumn<string>(
                name: "PerfomersId",
                table: "Tasks",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
