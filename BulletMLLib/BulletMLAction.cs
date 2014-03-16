using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BulletMLLib
{
	/// <summary>
	/// Actionタグの処理。
	/// Repeatタグは常についていると仮定し、1～Timesの回数繰り返す。
	/// </summary>
	public class BulletMLAction : BulletMLTask
	{
		public int repeatNumMax;
		public int repeatNum;
		private BulletMLTree node;

		public BulletMLAction(BulletMLTree node, int repeatNumMax)
		{
			this.node = node;
			this.repeatNumMax = repeatNumMax;
		}

		public override void Init()
		{
			base.Init();
			repeatNum = 0;
		}

		public override BLRunStatus Run(BulletMLBullet bullet)
		{
			while (repeatNum < repeatNumMax)
			{
				BLRunStatus runStatus = base.Run(bullet);

				if (runStatus == BLRunStatus.End)
				{
					repeatNum++;
					base.Init();
				}
				else if (runStatus == BLRunStatus.Stop)
					return BLRunStatus.Stop;// BLRunStatus.Stop;
				else
					return BLRunStatus.Continue;// BLRunStatus.Stop;
			}

			end = true;
			return BLRunStatus.End;

			//if (repeatNum < repeatNumMax)
			//{
			//    BLRunStatus runStatus = base.Run(bullet);
			//    if (runStatus == BLRunStatus.End)
			//    {
			//        repeatNum++;
			//        //if (bullet.index == DISP_BULLET_INDEX) Debug.WriteLine(String.Format("Repeat: {0} / {1}", repeatNum, repeatNumMax));
			//        base.Init();
			//        if (repeatNum == repeatNumMax)
			//        {
			//            end = true;
			//            return BLRunStatus.End;
			//        }
			//        else
			//            return BLRunStatus.Stop;// Continue;
			//    }
			//    else if (runStatus == BLRunStatus.Stop)
			//        return BLRunStatus.Stop;
			//    else
			//        return BLRunStatus.Stop;// BLRunStatus.Continue;
			//}
			//else
			//{
			//    end = true;
			//    return BLRunStatus.End;
			//}
		}
	}
}
