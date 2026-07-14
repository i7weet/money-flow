using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MoneyFlow.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FinalizeDomainEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_transactions_UserId",
                table: "transactions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_transaction_tags",
                table: "transaction_tags");

            migrationBuilder.DropIndex(
                name: "IX_transaction_tags_TransactionId_TagId",
                table: "transaction_tags");

            migrationBuilder.DropIndex(
                name: "IX_categories_UserId_Type_ParentCategoryId_Name",
                table: "categories");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "transaction_tags");

            migrationBuilder.AddPrimaryKey(
                name: "PK_transaction_tags",
                table: "transaction_tags",
                columns: new[] { "TransactionId", "TagId" });

            migrationBuilder.CreateIndex(
                name: "IX_transactions_CounterAccountId",
                table: "transactions",
                column: "CounterAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_categories_UserId_Type_ParentCategoryId_Name",
                table: "categories",
                columns: new[] { "UserId", "Type", "ParentCategoryId", "Name" },
                unique: true)
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.AddForeignKey(
                name: "FK_accounts_users_UserId",
                table: "accounts",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_categories_users_UserId",
                table: "categories",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_tags_users_UserId",
                table: "tags",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_transaction_tags_tags_TagId",
                table: "transaction_tags",
                column: "TagId",
                principalTable: "tags",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_transaction_tags_transactions_TransactionId",
                table: "transaction_tags",
                column: "TransactionId",
                principalTable: "transactions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_transactions_accounts_CounterAccountId",
                table: "transactions",
                column: "CounterAccountId",
                principalTable: "accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_transactions_accounts_PrimaryAccountId",
                table: "transactions",
                column: "PrimaryAccountId",
                principalTable: "accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_transactions_categories_CategoryId",
                table: "transactions",
                column: "CategoryId",
                principalTable: "categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_transactions_users_UserId",
                table: "transactions",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_accounts_users_UserId",
                table: "accounts");

            migrationBuilder.DropForeignKey(
                name: "FK_categories_users_UserId",
                table: "categories");

            migrationBuilder.DropForeignKey(
                name: "FK_tags_users_UserId",
                table: "tags");

            migrationBuilder.DropForeignKey(
                name: "FK_transaction_tags_tags_TagId",
                table: "transaction_tags");

            migrationBuilder.DropForeignKey(
                name: "FK_transaction_tags_transactions_TransactionId",
                table: "transaction_tags");

            migrationBuilder.DropForeignKey(
                name: "FK_transactions_accounts_CounterAccountId",
                table: "transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_transactions_accounts_PrimaryAccountId",
                table: "transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_transactions_categories_CategoryId",
                table: "transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_transactions_users_UserId",
                table: "transactions");

            migrationBuilder.DropIndex(
                name: "IX_transactions_CounterAccountId",
                table: "transactions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_transaction_tags",
                table: "transaction_tags");

            migrationBuilder.DropIndex(
                name: "IX_categories_UserId_Type_ParentCategoryId_Name",
                table: "categories");
            
            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "transaction_tags",
                type: "uuid",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE transaction_tags
                SET "Id" = gen_random_uuid();
                """);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "transaction_tags",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_transaction_tags",
                table: "transaction_tags",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_transactions_UserId",
                table: "transactions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_transaction_tags_TransactionId_TagId",
                table: "transaction_tags",
                columns: new[] { "TransactionId", "TagId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_categories_UserId_Type_ParentCategoryId_Name",
                table: "categories",
                columns: new[] { "UserId", "Type", "ParentCategoryId", "Name" },
                unique: true);
        }
    }
}
