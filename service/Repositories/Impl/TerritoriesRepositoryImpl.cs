using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using Soli.Models;
using Soli.Repositories;

namespace Soli.Repositories.Impl
{
    public class TerritoriesRepositoryImpl : TerritoriesRepository
    {
        private readonly IDbConnection _dbConn;
        private ILogger _logger;

        public TerritoriesRepositoryImpl(IDbConnection dbConn, ILogger<TerritoriesRepositoryImpl> logger)
        {
            _dbConn = dbConn;
            if(_dbConn.State != ConnectionState.Open) {
                _dbConn.Open();
            }
            _logger = logger;
        }

        public async Task<AssignedUnits> GetAssignedInState(string state)
        {
            using(var res = await _dbConn.QueryMultipleAsync(@"select z.zip from territoryzips z 
                                    join territories t on t.id = z.territory_id where t.state = @State;
                                select c.church from territorychurches c
                                    join territories t on t.id = c.territory_id where t.state = @State;",
                            new { State = state }))
            {
                var au = new AssignedUnits {
                    Zips = await res.ReadAsync<string>(),
                    Churches = await res.ReadAsync<string>()
                };

                return au;
            }
        }

        public Task<IEnumerable<Territory>> GetTerritories()
        {
            return _dbConn.QueryAsync<Territory>(@"select id, name, state from territories");
        }

        public async Task<TerritoryDetail> GetTerritory(Guid territoryId)
        {
            using (var res = await _dbConn.QueryMultipleAsync(@"select Name,Color,State from Territories where id = @Id;
                                            select church from TerritoryChurches where territory_id = @Id;
                                            select zip from TerritoryZips where territory_id = @Id;",
                                    new { Id = territoryId}))
            {
                var dtl = await res.ReadSingleOrDefaultAsync<TerritoryDetail>();
                if(dtl != null ) {
                    dtl.Id = territoryId;
                }
                var churches = await res.ReadAsync<string>();
                var zips = await res.ReadAsync<string>();
                if(dtl != null)
                {
                    dtl.Churches = churches;
                    dtl.Zips = zips;
                }
                return dtl;
            }
        }

        public async Task<TerritoryDetail> SaveTerritory(TerritoryDetail detail)
        {
            using (var ctx = _dbConn.BeginTransaction())
            {
                try
                {
                // update or add
                var updated = await _dbConn.ExecuteAsync(@"update Territories set name = @Name,
                                        color = @Color, state = @State where id = @Id", detail, transaction: ctx);

                if (updated < 1)
                {
                    detail.Id = Guid.NewGuid(); 
                    await _dbConn.ExecuteAsync(@"insert into Territories (id, name, color, state)
                            values (@Id, @Name, @Color, @State);", detail, transaction: ctx);
                }

                await _dbConn.ExecuteAsync("delete from TerritoryChurches where territory_id = @Id", detail, transaction: ctx);
                await _dbConn.ExecuteAsync("insert into TerritoryChurches (territory_id, church) values (@Id, @Church)",
                                detail.Churches.Select(c => new { Id = detail.Id, Church = c }), ctx);

                await _dbConn.ExecuteAsync("delete from TerritoryZips where territory_id = @Id", detail, transaction: ctx);
                await _dbConn.ExecuteAsync("insert into TerritoryZips (territory_id, zip) values (@Id, @Zip)",
                                detail.Zips.Select(z => new { Id = detail.Id, Zip = z }), transaction: ctx);
                ctx.Commit();
                return detail;
                } catch(DbException ex)
                {
                    _logger.LogError(ex, "failed to save");
                    ctx.Rollback();
                    return null; // need better error handling
                }
            }
        }
    }
}