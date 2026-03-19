using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManager.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddModelJoinToTaskRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Tasks_TaskItemId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_AspNetUsers_OwnerId",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_TaskItemId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "TaskItemId",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<bool>(
                name: "CanAnyoneJoin",
                table: "Tasks",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "OwnerUsername",
                table: "Tasks",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PerfomersId",
                table: "Tasks",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "JoinToTaskRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TaskId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    ReviwedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JoinToTaskRequests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TaskPerformers",
                columns: table => new
                {
                    PerformerTasksId = table.Column<int>(type: "INTEGER", nullable: false),
                    PerformersId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskPerformers", x => new { x.PerformerTasksId, x.PerformersId });
                    table.ForeignKey(
                        name: "FK_TaskPerformers_AspNetUsers_PerformersId",
                        column: x => x.PerformersId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TaskPerformers_Tasks_PerformerTasksId",
                        column: x => x.PerformerTasksId,
                        principalTable: "Tasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TaskPerformers_PerformersId",
                table: "TaskPerformers",
                column: "PerformersId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_AspNetUsers_OwnerId",
                table: "Tasks",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_AspNetUsers_OwnerId",
                table: "Tasks");

            migrationBuilder.DropTable(
                name: "JoinToTaskRequests");

            migrationBuilder.DropTable(
                name: "TaskPerformers");

            migrationBuilder.DropColumn(
                name: "CanAnyoneJoin",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "OwnerUsername",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "PerfomersId",
                table: "Tasks");

            migrationBuilder.AddColumn<int>(
                name: "TaskItemId",
                table: "AspNetUsers",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_TaskItemId",
                table: "AspNetUsers",
                column: "TaskItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Tasks_TaskItemId",
                table: "AspNetUsers",
                column: "TaskItemId",
                principalTable: "Tasks",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_AspNetUsers_OwnerId",
                table: "Tasks",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
