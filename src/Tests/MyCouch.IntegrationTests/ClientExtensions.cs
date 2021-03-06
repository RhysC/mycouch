﻿using MyCouch.Commands;

namespace MyCouch.IntegrationTests
{
    internal static class ClientExtensions
    {
        internal static void ClearAllDocuments(this IClient client)
        {
            var query = new SystemViewQuery("_all_docs");
            var response = client.Views.RunQueryAsync<dynamic>(query).Result;

            if (!response.IsEmpty)
            {
                var bulkCmd = new BulkCommand();

                foreach (var row in response.Rows)
                    bulkCmd.Delete(row.Id, row.Value.rev.ToString());
                
                client.Documents.BulkAsync(bulkCmd).Wait();
            }
        }
    }
}