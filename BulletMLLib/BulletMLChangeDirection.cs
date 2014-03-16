using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BulletMLLib
{
	/// <summary>
	/// 方向転換処理
	/// </summary>
	public class BulletMLChangeDirection : BulletMLTask
	{
		private float changeDir;
		private int term;
		private BulletMLTree node;
		private bool first = true;
		private BLType blType = BLType.None;

		public BulletMLChangeDirection(BulletMLTree node)
		{
			this.node = node;
		}

		public override void Init()
		{
			base.Init();
			first = true;
			term = (int)node.GetChildValue(BLName.Term, this);
		}

		public override BLRunStatus Run(BulletMLBullet bullet)
		{
			if (first)
			{
				first = false;
				float value = (float)(node.GetChildValue(BLName.Direction, this) * Math.PI / 180);
				blType = node.GetChild(BLName.Direction).type;
				if (blType == BLType.Sequence)
				{
					changeDir = value;
				}
				else
				{
					if (blType == BLType.Absolute)
					{
						changeDir = (float)((value - bullet.Direction));
					}
					else if (blType == BLType.Relative)
					{
						changeDir = (float)(value);
					}
					else
					{
						changeDir = (float)((bullet.GetAimDir() + value - bullet.Direction));
					}

					if (changeDir > Math.PI) changeDir -= 2 * (float)Math.PI;
					if (changeDir < -Math.PI) changeDir += 2 * (float)Math.PI;

					changeDir /= term;

					/*
										float finalDir = 0;

										if (blType == BLType.Absolute)
										{
											finalDir = value;
										}
										else if (blType == BLType.Relative)
										{
											finalDir = bullet.Direction + value;
										}
										else
										{
											finalDir = bullet.GetAimDir() + value;
										}

										// 角度の小さいほうへ回転する
										float changeDir1 = finalDir - bullet.Direction;
										float changeDir2;
										changeDir2 = changeDir1 > 0 ? changeDir2 = changeDir1 - 360: changeDir2 = changeDir1 + 360;
										changeDir = Math.Abs(changeDir1) < Math.Abs(changeDir2) ? changeDir1 : changeDir2;
										changeDir = changeDir / term;
					*/
				}
			}

			term--;

			bullet.Direction = bullet.Direction + changeDir;

			// if (bullet.index == DISP_BULLET_INDEX) Debug.WriteLine(String.Format("changeDirection:{0}度 (changeDir:{1} type:{2})", bullet.Direction / Math.PI * 180, changeDir, node.GetChild(BLName.Direction).type));

			if (term <= 0)
			{
				end = true;
				return BLRunStatus.End;
			}
			else
				return BLRunStatus.Continue;
		}
	}
}
