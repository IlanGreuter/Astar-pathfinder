using UnityEngine;

namespace Winkeldief.Pathfinding
{
    public class Pathfinder : PathfinderBase
    {
        public override int GetTileCost(Vector3Int cell)
        {
            //Simply makes a tile unwalkable if it exist
            return map.HasTile(cell) ? -1 : 1;
        }
    }
}