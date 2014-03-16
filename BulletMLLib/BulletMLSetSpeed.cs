using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BulletMLLib
{
	/// <summary>
	/// Speed 処理
	/// </summary>
	public class BulletMLSetSpeed : BulletMLTask
	{
		private BulletMLTree node;

		public BulletMLSetSpeed(BulletMLTree node)
		{
			this.node = node;
		}

		public override BLRunStatus Run(BulletMLBullet bullet)
		{
			bullet.Speed = node.GetValue(this);
			//if(bullet.index == DISP_BULLET_INDEX) Debug.WriteLine("SetSpeed:" + bullet.Speed);
			end = true;
			return BLRunStatus.End;
		}
	}
}
