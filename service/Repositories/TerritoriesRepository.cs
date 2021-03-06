using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Soli.Models;

namespace Soli.Repositories
{
    public interface TerritoriesRepository
    {
        Task<IEnumerable<Territory>> GetTerritories();
        Task<TerritoryDetail> GetTerritory(Guid territoryId);
        Task<TerritoryDetail> SaveTerritory(TerritoryDetail detail);

        Task<AssignedUnits> GetAssignedInState(string state);
    }
}