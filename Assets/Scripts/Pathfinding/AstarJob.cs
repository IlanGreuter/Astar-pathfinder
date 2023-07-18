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

        public AstarJob(int2 start, int2 end)
        {
            this.start = start;
            this.end = end;
        }

        [BurstCompile]
        public void Execute()
        {
            //AstarTile sT = new AstarTile(start.x, start.y, 0, 4);
            //AstarTile eT = new AstarTile(end.x, end.y, 0, 4);
            //for (int i = 0; i < 100000; i++)
            //    sT.GetDistanceTo(end.x, end.y);
        }
    }
}
