using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace itunes_aspx.Models
{
    // このファイルにカスタム コードを追加できます。変更は上書きされません。
    // 
    // モデル スキーマを変更する際に必ず、自動的に Entity Framework でドロップし、
    // データベースを生成し直す必要がある場合は、以下のコードを、
    // Global.asax ファイルの Application_Start メソッドに追加します。
    // 注: これにより、モデルが変更されるたびにデータベースが破棄され、再作成されます。
    // 
    // System.Data.Entity.Database.SetInitializer(new System.Data.Entity.DropCreateDatabaseIfModelChanges<itunes_aspx.Models.TodoItemContext>());
    public class TodoItemContext : DbContext
    {
        public TodoItemContext()
            : base("name=DefaultConnection")
        {
        }

        public DbSet<TodoItem> TodoItems { get; set; }
        public DbSet<TodoList> TodoLists { get; set; }
    }
}