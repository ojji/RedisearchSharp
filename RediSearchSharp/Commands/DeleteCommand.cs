using System.Threading.Tasks;
using RediSearchSharp.Internal;
using RediSearchSharp.Serialization;
using StackExchange.Redis;

namespace RediSearchSharp.Commands
{
    public abstract class DeleteCommand
    {
        private string DocumentKey { get; }
        private RedisValue IndexName { get; }

        private DeleteCommand(RedisValue indexName, string documentKey)
        {
            IndexName = indexName;
            DocumentKey = documentKey;
        }

        public static DeleteCommand Create<TEntity>(TEntity entity, bool deleteFromDatabase)
            where TEntity : RedisearchSerializable<TEntity>, new()
        {
            var schemaMetadata = SchemaMetadata<TEntity>.GetSchemaMetadata();
            var documentKey = string.Concat(schemaMetadata.DocumentIdPrefix,
                schemaMetadata.PrimaryKey.GetPrimaryKeyFromEntity(entity));

            if (deleteFromDatabase)
            {
                return new DeleteFromIndexAndDatabaseCommand(schemaMetadata.IndexName, documentKey);
            }
            
            return new DeleteFromIndexCommand(schemaMetadata.IndexName, documentKey);
        }

        public abstract bool Execute(IDatabase db);
        public abstract Task<bool> ExecuteAsync(IDatabase db);

        private class DeleteFromIndexCommand : DeleteCommand
        {
            public DeleteFromIndexCommand(RedisValue indexName, string documentKey) : base(indexName, documentKey)
            {
            }

            public override bool Execute(IDatabase db)
            {
                return (int) db.Execute("FT.DEL", 
                           IndexName,
                           DocumentKey) == 1;
            }

            public override async Task<bool> ExecuteAsync(IDatabase db)
            {
                return (int) await db.ExecuteAsync("FT.DEL",
                           IndexName,
                           DocumentKey).ConfigureAwait(false) == 1;
            }
        }

        private class DeleteFromIndexAndDatabaseCommand : DeleteCommand
        {
            private Task<RedisResult> DeleteFromIndexTask { get; set; }
            private Task<bool> DeleteFromStoreTask { get; set; }
            private ITransaction DeleteTransaction { get; set; }

            public DeleteFromIndexAndDatabaseCommand(RedisValue indexName, string documentKey) : base(indexName, documentKey)
            {   
            }

            public override bool Execute(IDatabase db)
            {
                CreateDeleteTasks(db);

                // if the transaction executes it returns true,
                // but we have to check the individual results of the commands too
                // since the transaction only guarantees that the commands were executed
                if (DeleteTransaction.Execute())
                {
                    return (int)DeleteFromIndexTask.Result == 1 &&
                           DeleteFromStoreTask.Result;
                }

                return false;
            }

            public override async Task<bool> ExecuteAsync(IDatabase db)
            {
                CreateDeleteTasks(db);
                // if the transaction executes it returns true,
                // but we have to check the individual results of the commands too
                // since the transaction only guarantees that the commands were executed
                if (await DeleteTransaction.ExecuteAsync().ConfigureAwait(false))
                {
                    return (int)DeleteFromIndexTask.Result == 1 &&
                           DeleteFromStoreTask.Result;
                }
                return false;
            }

            private void CreateDeleteTasks(IDatabase db)
            {
                var transaction = db.CreateTransaction();
                transaction.AddCondition(Condition.KeyExists(DocumentKey));
                DeleteFromIndexTask = transaction.ExecuteAsync(
                    "FT.DEL",
                    IndexName,
                    DocumentKey);
                DeleteFromStoreTask = transaction.KeyDeleteAsync(DocumentKey);
                DeleteTransaction = transaction;
            }
        }
    }
}