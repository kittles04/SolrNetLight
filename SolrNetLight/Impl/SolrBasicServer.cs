﻿#region license
// Copyright (c) 2007-2010 Mauricio Scheffer
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//      http://www.apache.org/licenses/LICENSE-2.0
//  
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Linq;
using Newtonsoft.Json;
using SolrNetLight.Commands;
using SolrNetLight.Commands.Parameters;

namespace SolrNetLight.Impl
{
    /// <summary>
    /// Implements the basic Solr operations
    /// </summary>
    /// <typeparam name="T">Document type</typeparam>
    public class SolrBasicServer<T> : ISolrBasicOperations<T>
    {
        private readonly ISolrConnection connection;
        private readonly ISolrQueryExecuter<T> queryExecuter;
        //private readonly ISolrHeaderResponseParser headerParser;
        private readonly ISolrQuerySerializer querySerializer;

        public SolrBasicServer(ISolrConnection connection, ISolrQueryExecuter<T> queryExecuter, ISolrQuerySerializer querySerializer, ISolrExtractResponseParser extractResponseParser = null)
        {
            this.connection = connection;
            //this.extractResponseParser = extractResponseParser;
            this.queryExecuter = queryExecuter;
            //this.documentSerializer = documentSerializer;
            //this.schemaParser = schemaParser;
            //this.headerParser = headerParser;
            this.querySerializer = querySerializer;
            //this.dihStatusParser = dihStatusParser;
        }

        public async Task<ResponseHeader> Commit(CommitOptions options)
        {
            options = options ?? new CommitOptions();
            var cmd = new CommitCommand
            {
                WaitFlush = options.WaitFlush,
                WaitSearcher = options.WaitSearcher,
                ExpungeDeletes = options.ExpungeDeletes,
                MaxSegments = options.MaxSegments,
            };
            return await SendAndParseHeader(cmd);
        }

        public async Task<ResponseHeader> Rollback()
        {
            return await SendAndParseHeader(new RollbackCommand());
        }

        public async Task<ResponseHeader> AddWithBoost(IEnumerable<KeyValuePair<T, double?>> docs, AddParameters parameters)
        {
            var cmd = new AddCommand<T>(docs, parameters);
            return await SendAndParseHeader(cmd);
        }

        public async Task<SolrQueryResults<T>> Query(ISolrQuery query, QueryOptions options)
        {
            return await queryExecuter.Execute(query, options);
        }

        public async Task<string> Send(ISolrCommand cmd)
        {
            return await cmd.Execute(connection);
        }


        public async Task<ResponseHeader> SendAndParseHeader(ISolrCommand cmd)
        {
            var jsonResponse = await Send(cmd);

            return JsonConvert.DeserializeObject<ResponseHeader>(jsonResponse);

        }




        
    }
}
