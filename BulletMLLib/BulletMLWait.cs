using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BulletMLLib
{
	/// <summary>
	/// Wait処理
	/// </summary>
	public class BulletMLWait : BulletMLTask
	{
		private int term;
		private BulletMLTree node;

		public BulletMLWait(BulletMLTree node)
		{
			this.node = node;
		}

		public override void Init()
		{
			base.Init();
			term = (int)node.GetValue(this) + 1; //初回実行時に一回処理されるため、そのぶん加算しておく
		}

		public override BLRunStatus Run(BulletMLBullet bullet)
		{
			if (term >= 0)
				term--;

			//if (term >= 0) if (bullet.index == DISP_BULLET_INDEX)  Debug.WriteLine("Wait " + term);

			if (term >= 0)
				return BLRunStatus.Stop;
			else
			{
				end = true;
				return BLRunStatus.End;
			}
		}
	}
}
