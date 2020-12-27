using System.Threading;
using System;

namespace TheLordServer.GameThread
{
    using Table;
    using Table.Structure;
    using Util;

    public class ThreadRaidBoss
    {
        const int RaidBossIndex = 16;

        Thread runnable;

        public void Start()
        {
            runnable = new Thread ( Update );
            runnable.IsBackground = true;
            runnable.Start ( );
        }

        public void Stop()
        {
            runnable.Abort ( );
        }


        void Update()
        {
            Thread.Sleep ( 5000 );
            while(true)
            {
                Thread.Sleep ( 1000 );
                UpdateTime ( );
            }
        }

        void UpdateTime()
        {
            var bossData = TheLordServer.Instance.bossDataList[0];
            var time = GameUtility.Now ( ) - bossData.CreateTime;
            var nextTime = new TimeSpan ( 24, 0, 0 );
            if ( time >= nextTime )
            {
                var sheet = TheLordTable.Instance.CharacterTable.CharacterInfoSheet;
                var record = BaseTable.Get ( sheet, "index", RaidBossIndex );
                bossData.HP = (float)record["hp"];
                bossData.CreateTime += new TimeSpan ( 24, 0, 0 );
            }
        }

    }
}
