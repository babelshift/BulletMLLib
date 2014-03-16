using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BulletMLLib
{
	public class BulletMLVanish : BulletMLTask
	{
		public override BLRunStatus Run(BulletMLBullet bullet)
		{
			bullet.Vanish();
			end = true;
			//if(bullet.index == DISP_BULLET_INDEX) Debug.WriteLine("Vanish");
			return BLRunStatus.End;
		}
	}
}
