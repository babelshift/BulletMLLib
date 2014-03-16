using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BulletMLLib
{
	/// <summary>
	/// BulletML Fire処理
	/// </summary>
	public class BulletMLFire : BulletMLTask
	{
		private BulletMLTree refNode, dirNode, spdNode, node, bulletNode;

		public BulletMLFire(BulletMLTree node)
		{
			this.node = node;
			this.dirNode = node.GetChild(BLName.Direction);
			this.spdNode = node.GetChild(BLName.Speed);
			this.refNode = node.GetChild(BLName.BulletRef);
			this.bulletNode = node.GetChild(BLName.Bullet);
			if (dirNode == null && refNode != null)
				dirNode = refNode.GetChild(BLName.Direction);
			if (dirNode == null && bulletNode != null)
				dirNode = bulletNode.GetChild(BLName.Direction);
			if (spdNode == null && refNode != null)
				spdNode = refNode.GetChild(BLName.Speed);
			if (spdNode == null && bulletNode != null)
				spdNode = bulletNode.GetChild(BLName.Speed);
		}

		public override BLRunStatus Run(BulletMLBullet bullet)
		{
			float changeDir = 0;
			float changeSpd = 0;

			// 方向の設定
			if (dirNode != null)
			{
				changeDir = (int)dirNode.GetValue(this) * (float)Math.PI / (float)180;
				if (dirNode.type == BLType.Sequence)
				{
					bullet.GetFireData().srcDir += changeDir;
				}
				else if (dirNode.type == BLType.Absolute)
				{
					bullet.GetFireData().srcDir = changeDir;
				}
				else if (dirNode.type == BLType.Relative)
				{
					bullet.GetFireData().srcDir = changeDir + bullet.Direction;
				}
				else
				{
					bullet.GetFireData().srcDir = changeDir + bullet.GetAimDir();
				}
			}
			else
			{
				bullet.GetFireData().srcDir = bullet.GetAimDir();
			}

			// 弾の生成
#if ExpandedBulletML
            string blName = "";
            if (bulletNode != null)
                blName = bulletNode.bulletName;
            else if (refNode != null)
                blName = refNode.bulletName;
            BulletMLBullet newBullet = bullet.GetNewBullet(blName);//bullet.tree);
#else
			BulletMLBullet newBullet = bullet.GetNewBullet();//bullet.tree);
#endif

			if (newBullet == null)
			{
				end = true;
				return BLRunStatus.End;
			}

			if (refNode != null)
			{
				// パラメータを取得
				for (int i = 0; i < refNode.children.Count; i++)
				{
					newBullet.tasks[0].paramList.Add(refNode.children[i].GetValue(this));
				}

				//if (refNode.children.Count > 0)
				//{
				//    newBullet.task.paramNode = refNode;// node;
				//}
				// refBulletで参照
				newBullet.Init(bullet.tree.GetLabelNode(refNode.label, BLName.Bullet));
#if ExpandedBulletML
                newBullet.Visible = refNode.visible;
#endif
			}
			else
			{
				newBullet.Init(bulletNode);
#if ExpandedBulletML
               newBullet.Visible = bulletNode.visible;
#endif
			}

			newBullet.X = bullet.X;
			newBullet.Y = bullet.Y;
			newBullet.tasks[0].owner = this;
			newBullet.Direction = bullet.GetFireData().srcDir;

			if (!bullet.GetFireData().speedInit && newBullet.GetFireData().speedInit)
			{
				// 自分の弾発射速度の初期化がまだのとき、子供の弾の速度を使って初期値とする
				bullet.GetFireData().srcSpeed = newBullet.Speed;
				bullet.GetFireData().speedInit = true;
			}
			else
			{
				// 自分の弾発射速度の初期化済みのとき
				// スピードの設定
				if (spdNode != null)
				{
					changeSpd = spdNode.GetValue(this);
					if (spdNode.type == BLType.Sequence || spdNode.type == BLType.Relative)
					{
						bullet.GetFireData().srcSpeed += changeSpd;
					}
					else
					{
						bullet.GetFireData().srcSpeed = changeSpd;
					}
				}
				else
				{
					// 特に弾に速度が設定されていないとき
					if (!newBullet.GetFireData().speedInit)
						bullet.GetFireData().srcSpeed = 1;
					else
						bullet.GetFireData().srcSpeed = newBullet.Speed;
				}
			}

			newBullet.GetFireData().speedInit = false;
			newBullet.Speed = bullet.GetFireData().srcSpeed;

			//if(bullet.index == DISP_BULLET_INDEX) Debug.WriteLine(String.Format("Fire dir:{0} spd:{1} label:{2}", bullet.srcDir / Math.PI * 180, bullet.srcSpeed, refNode != null ? refNode.label : ""));
			//Debug.WriteLine("index({3}) Fire dir:{0} spd:{1} label:{2}", bullet.srcDir / Math.PI * 180, bullet.srcSpeed, refNode != null ? refNode.label : "", bullet.index);

			end = true;
			return BLRunStatus.End;
		}
	}
}
