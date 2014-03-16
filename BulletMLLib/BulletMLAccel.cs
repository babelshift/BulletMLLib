using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BulletMLLib
{
	/// <summary>
	/// 加速処理
	/// </summary>
	public class BulletMLAccel : BulletMLTask
	{
		private BulletMLTree node;
		private int term;
		private float verticalAccel;
		private float horizontalAccel;
		private bool first;

		public BulletMLAccel(BulletMLTree node)
		{
			this.node = node;
		}

		public override void Init()
		{
			base.Init();
			first = true;
		}

		public override BLRunStatus Run(BulletMLBullet bullet)
		{
			if (first)
			{
				first = false;
				term = (int)node.GetChildValue(BLName.Term, this);
				if (node.type == BLType.Sequence)
				{
					horizontalAccel = node.GetChildValue(BLName.Horizontal, this);
					verticalAccel = node.GetChildValue(BLName.Vertical, this);
				}
				else if (node.type == BLType.Relative)
				{
					horizontalAccel = node.GetChildValue(BLName.Horizontal, this) / term;
					verticalAccel = node.GetChildValue(BLName.Vertical, this) / term;
				}
				else
				{
					// spdX = (float)(bullet.speed * Math.Sin(bullet.Direction));
					// spdY = (float)(bullet.speed * Math.Cos(bullet.Direction));
					horizontalAccel = (node.GetChildValue(BLName.Horizontal, this) - bullet.spdX) / term;
					verticalAccel = (node.GetChildValue(BLName.Vertical, this) - bullet.spdY) / term;
				}
			}

			term--;
			if (term < 0)
			{
				end = true;
				return BLRunStatus.End;
			}

			bullet.spdX += horizontalAccel;
			bullet.spdY += verticalAccel;

			//Debug.WriteLine(String.Format("accel spdX={0} spdY={1} verAcl={2} horAcl={3} term={4}", bullet.spdX, bullet.spdY, verticalAccel, horizontalAccel, term));

			/*
			double speedX = bullet.speed * Math.Sin(bullet.Direction);
			double speedY = bullet.speed * Math.Cos(bullet.Direction);
			speedX += horizontalAccel;
			speedY += verticalAccel;

			bullet.Direction = (float)Math.Atan2(speedX ,speedY);
			bullet.speed = (float)Math.Sqrt(speedX * speedX + speedY * speedY);
			//Debug.WriteLine("accel speed={0} dir={1} verAcl={2} horAcl={3} term={4}", bullet.speed, bullet.Direction / Math.PI * 180, verticalAccel, horizontalAccel, term); */
			return BLRunStatus.Continue;
		}
	}
}
