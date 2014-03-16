using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BulletMLLib
{
	/// <summary>
	/// BulletMLタスク
	/// 実際に弾を動かすクラス
	/// </summary>
	public class BulletMLTask
	{
		public const int DISP_BULLET_INDEX = 1;

		public enum BLRunStatus { Continue, End, Stop };

		public List<BulletMLTask> taskList;
		public bool end = false;

		//public BulletMLTree paramNode = null;
		public List<float> paramList = new List<float>();

		public BulletMLTask owner = null;

		public BulletMLTask()
		{
			taskList = new List<BulletMLTask>();
		}

		public virtual void Init()
		{
			end = false;

			foreach (BulletMLTask task in taskList)
			{
				task.Init();
			}
		}

		public virtual BLRunStatus Run(BulletMLBullet bullet)
		{
			end = true;
			for (int i = 0; i < taskList.Count; i++)
			{
				if (!taskList[i].end)
				{
					BLRunStatus sts = taskList[i].Run(bullet);
					if (sts == BLRunStatus.Stop)
					{
						end = false;
						return BLRunStatus.Stop;
					}
					else if (sts == BLRunStatus.Continue)
						end = false;
				}
			}

			if (end)
				return BLRunStatus.End;
			else
				return BLRunStatus.Continue;//継続して実行
		}

		//BulletMLTreeの内容を元に、実行のための各種クラスを生成し、自身を初期化する
		public void Parse(BulletMLTree tree, BulletMLBullet bullet)
		{
			foreach (BulletMLTree node in tree.children)
			{
				// Action
				if (node.name == BLName.Repeat)
				{
					Parse(node, bullet);
				}
				else if (node.name == BLName.Action)
				{
					////Debug.WriteLine("Action");
					int repeatNum = 1;
					if (node.parent.name == BLName.Repeat)
						repeatNum = (int)node.parent.GetChildValue(BLName.Times, this);
					BulletMLAction task = new BulletMLAction(node, repeatNum);
					task.owner = this;
					taskList.Add(task);
					task.Parse(node, bullet);
				}
				else if (node.name == BLName.ActionRef)
				{
					BulletMLTree refNode = tree.GetLabelNode(node.label, BLName.Action);
					int repeatNum = 1;
					if (node.parent.name == BLName.Repeat)
						repeatNum = (int)node.parent.GetChildValue(BLName.Times, this);
					BulletMLAction task = new BulletMLAction(refNode, repeatNum);
					task.owner = this;
					taskList.Add(task);

					// パラメータを取得
					for (int i = 0; i < node.children.Count; i++)
					{
						task.paramList.Add(node.children[i].GetValue(this));
					}
					//if (node.children.Count > 0)
					//{
					//    task.paramNode = node;
					//}

					task.Parse(refNode, bullet);
				}
				else if (node.name == BLName.ChangeSpeed)
				{
					BulletMLChangeSpeed blChangeSpeed = new BulletMLChangeSpeed(node);
					blChangeSpeed.owner = this;
					taskList.Add(blChangeSpeed);
					////Debug.WriteLine("ChangeSpeed");
				}
				else if (node.name == BLName.ChangeDirection)
				{
					BulletMLChangeDirection blChangeDir = new BulletMLChangeDirection(node);
					blChangeDir.owner = this;
					taskList.Add(blChangeDir);
					////Debug.WriteLine("ChangeDirection");
				}
				else if (node.name == BLName.Fire)
				{
					if (taskList == null) taskList = new List<BulletMLTask>();
					BulletMLFire fire = new BulletMLFire(node);
					fire.owner = this;
					taskList.Add(fire);
				}
				else if (node.name == BLName.FireRef)
				{
					if (taskList == null) taskList = new List<BulletMLTask>();
					BulletMLTree refNode = tree.GetLabelNode(node.label, BLName.Fire);
					BulletMLFire fire = new BulletMLFire(refNode);
					fire.owner = this;
					taskList.Add(fire);
					// パラメータを取得
					//if (node.children.Count > 0)
					//{
					//    fire.paramNode = node;
					//}
					for (int i = 0; i < node.children.Count; i++)
					{
						fire.paramList.Add(node.children[i].GetValue(this));
					}
				}
				else if (node.name == BLName.Wait)
				{
					BulletMLWait wait = new BulletMLWait(node);
					wait.owner = this;
					taskList.Add(wait);
				}
				else if (node.name == BLName.Speed)
				{
					//BulletMLSetSpeed task = new BulletMLSetSpeed(node);
					//task.owner = this;
					//taskList.Add(task);
					bullet.GetFireData().speedInit = true; // 値を明示的にセットしたことを示す
					bullet.Speed = node.GetValue(this);
				}
				else if (node.name == BLName.Direction)
				{
					BulletMLSetDirection task = new BulletMLSetDirection(node);
					task.owner = this;
					taskList.Add(task);
				}
				else if (node.name == BLName.Vanish)
				{
					BulletMLVanish task = new BulletMLVanish();
					task.owner = this;
					taskList.Add(task);
				}
				else if (node.name == BLName.Accel)
				{
					BulletMLAccel task = new BulletMLAccel(node);
					task.owner = this;
					taskList.Add(task);
				}
				else
				{
					////Debug.WriteLine("node.name:{0}", node.name);
				}
			}
		}
	}
}
