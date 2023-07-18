using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Winkeldief.Pathfinding
{
    [BurstCompile]
    public struct AstarJob : IJob
    {
        [ReadOnly]
        public int2 start, end;
        [ReadOnly]
        public Astar astar;

        public AstarJob(Astar astar, int2 start, int2 end)
        {
            this.astar = astar;
            this.start = start;
            this.end = end;
        }

        public void Execute()
        {
            //astar.FindPath(start, end);
        }
    }
}
