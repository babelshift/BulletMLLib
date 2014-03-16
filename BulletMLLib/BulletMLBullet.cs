//C# BulletMLライブラリ使用法　簡易説明

//準備
//　1. BulletMLParser.ParseXML関数を用いてXMLをロード。(メモリ上にXMLを展開する）
//　   このクラスは同じXMLを用いる弾に対して使い回す。ひとつのXMLを何度もロードする必要はない。
//　2. IBulletMLBulletInterfaceを実装したクラスを作成する。
//　3. そのクラスに、BulletMLBulletクラスを変数として持たせておく。

//使用法
//　4. 弾発射時などに、BulletMLBullet.InitTop関数を呼び出し、1, 2で準備したクラスを引数として渡す。
//  5. 毎フレーム、BulletMLBullet.Runを呼び出す。

//詳細はサンプルコードを参照。2009.6 Bandle
//#define ExpandedBulletML

using System;
using System.Collections.Generic;

namespace BulletMLLib
{
	/// <summary>
	/// BulletMLを使用する場合は、このインタフェースを継承した弾クラスを作成すること
	/// </summary>
	public interface IBulletMLBulletInterface
	{
		//仮想プロパティ・メソッド
		float X { get; set; }

		float Y { get; set; }

		float Speed { get; set; }

		float Dir { get; set; }

		void Vanish();

#if ExpandedBulletML
        bool Visible { get; set; }
        BulletMLBullet GetNewBullet(string name);
#else

		BulletMLBullet GetNewBullet();

#endif
	}

	public class FireData
	{
		public float srcSpeed = 0;
		public float srcDir = 0;
		public bool speedInit = false;
	}

	/// <summary>
	/// BulletMLライブラリ内部で使用する弾を表す。
	/// IBulletMLBulletInterfaceを継承したクラスに、変数として一つ持たせておくこと。
	/// </summary>
	public class BulletMLBullet
	{
		private IBulletMLBulletInterface ibullet;

		//private float dir;
		public float spdX; //<accel>で使用。移動処理でspeedと加算する必要がある

		public float spdY; //<accel>で使用。移動処理でspeedと加算する必要がある

		//public BulletMLTask task;
		public List<BulletMLTask> tasks;

		public List<FireData> fireData;
		public BulletMLTree tree;
		public int index = 1;//デバッグ用
		public int activeTaskNum = 0; // 現在処理中のtasksのインデクス
		//public BulletMLBullet(IBulletMLBulletInterface ibullet, BulletMLTree tree)
		//{
		//    this.ibullet = ibullet;
		//    task = new BulletMLTask();
		//    this.tree = tree;

		//    //トップノードからの初期化
		//    task.Parse(tree.GetLabelNode("top", BLName.Action));
		//    ////Debug.WriteLine("-------------Parse-----------------");
		//    task.Init();
		//}

		public BulletMLBullet(IBulletMLBulletInterface ibullet)
		{
			this.ibullet = ibullet;
			tasks = new List<BulletMLTask>();
			tasks.Add(new BulletMLTask());
			fireData = new List<FireData>();
			fireData.Add(new FireData());
			foreach (BulletMLTask t in tasks)
				t.Init();

			//task = new BulletMLTask();
			//task.Init();
		}

		/// <summary>
		/// BulletMLを動作させる
		/// </summary>
		/// <returns>処理が終了していたらtrue</returns>
		public bool Run()
		{
			int endNum = 0;
			for (int i = 0; i < tasks.Count; i++)
			{
				activeTaskNum = i;
				BulletMLAction.BLRunStatus result = tasks[i].Run(this);
				if (result == BulletMLTask.BLRunStatus.End)
					endNum++;
			}

			X = X + spdX + (float)(Math.Sin(ibullet.Dir) * Speed);
			Y = Y + spdY + (float)(-Math.Cos(ibullet.Dir) * Speed);

			if (endNum == tasks.Count)
				return true;
			else
				return false;
		}

		//木構造のトップからの初期化
		public void InitTop(BulletMLTree node)
		{
			//トップノードからの初期化
			this.tree = node;
			//task.taskList.Clear();
			//task.Parse(tree);
			//task.Init();

			BulletMLTree tree = node.GetLabelNode("top", BLName.Action);
			if (tree != null)
			{
				BulletMLTask task = tasks[0];
				task.taskList.Clear();
				task.Parse(tree, this);
				task.Init();
			}
			else
			{
				for (int i = 1; i < 10; i++)
				{
					BulletMLTree tree2 = node.GetLabelNode("top" + i, BLName.Action);
					if (tree2 != null)
					{
						if (i > 1)
						{
							tasks.Add(new BulletMLTask());
							fireData.Add(new FireData());
						}

						BulletMLTask task = tasks[i - 1];
						task.taskList.Clear();
						task.Parse(tree2, this);
						task.Init();
					}
				}
			}
		}

#if ExpandedBulletML

        //Parse後に呼ぶ。<action name="top">直下で使えるパラメータを、プログラム側から追加する。
        public void AddParam(float param)
        {
            //PARAMノード追加
            //if (task.paramNode == null)
            //    task.paramNode = new BulletMLTree();

            //PARAMノード追加
            task.paramList.Add(param);
            //BulletMLTree b = new BulletMLTree();
            //b.values.Add(new BulletValue(BLValueType.Number, param));
            //task.paramNode.children.Add(b);
        }
#endif

		//枝の途中からの初期化
		public void Init(BulletMLTree node)
		{
			BulletMLTask task = tasks[0];
			task.taskList.Clear();
			task.Parse(node, this);
			task.Init();
			this.tree = node;
		}

		public FireData GetFireData()
		{
			return fireData[activeTaskNum];
		}

		public float Direction
		{
			get
			{
				return ibullet.Dir;
			}
			set
			{
				float dir = value;

				if (dir > 2 * Math.PI)
					dir -= (float)(2 * Math.PI);
				else if (dir < 0)
					dir += (float)(2 * Math.PI);

				ibullet.Dir = dir;
			}
		}

		public float GetAimDir()
		{
			float val = (float)Math.Atan2((BulletMLManager.GetPlayerPositionX() - X), -(BulletMLManager.GetPlayerPositionY() - Y));
			////Debug.WriteLine("SetAimDir : {0}" , val / Math.PI * 180);
			return val;
		}

		public float X { get { return ibullet.X; } set { ibullet.X = value; } }

		public float Y { get { return ibullet.Y; } set { ibullet.Y = value; } }

		public float Speed { get { return ibullet.Speed; } set { ibullet.Speed = value; } }

		public void Vanish()
		{
			ibullet.Vanish();
		}

#if ExpandedBulletML
        public bool Visible { get { return ibullet.Visible; } set { ibullet.Visible = value; } }
        internal BulletMLBullet GetNewBullet(string name)
        {
            return ibullet.GetNewBullet(name);
        }
#else

		internal BulletMLBullet GetNewBullet()
		{
			return ibullet.GetNewBullet();
		}

#endif

		//internal BulletMLBullet GetNewBullet(BulletMLTree bulletMLTree)
		//{
		//    return ibullet.GetNewBullet(bulletMLTree);
		//}
	}

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

	/// <summary>
	/// 方向設定
	/// </summary>
	public class BulletMLSetDirection : BulletMLTask
	{
		public BulletMLTree node;

		public BulletMLSetDirection(BulletMLTree node)
		{
			this.node = node;
		}

		public override BLRunStatus Run(BulletMLBullet bullet)
		{
			BLType blType = node.type;
			float value = (float)(node.GetValue(this) * Math.PI / 180);

			if (blType == BLType.Sequence)
			{
				bullet.Direction = bullet.GetFireData().srcDir + value;
			}
			else if (blType == BLType.Absolute)
			{
				bullet.Direction = value;
			}
			else if (blType == BLType.Relative)
			{
				bullet.Direction = bullet.Direction + value;
			}
			else //if (blType == BLType.Aim || blType == BLType.None)
			{
				bullet.Direction = bullet.GetAimDir() + value;
			}

			//Debug.WriteLine(String.Format("SetDirecton:{0},  (type:{1} val:{2})", bullet.Direction / Math.PI * 180, node.type, value / Math.PI * 180));
			end = true;

			return BLRunStatus.End;
		}
	}

	/// <summary>
	/// スピード変更
	/// </summary>
	public class BulletMLChangeSpeed : BulletMLTask
	{
		private float changeSpeed;
		private int term;
		private BulletMLTree node;
		private bool first = true;

		public BulletMLChangeSpeed(BulletMLTree node)
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
				if (node.GetChild(BLName.Speed).type == BLType.Sequence)
				{
					changeSpeed = node.GetChildValue(BLName.Speed, this);
				}
				else if (node.GetChild(BLName.Speed).type == BLType.Relative)
				{
					changeSpeed = node.GetChildValue(BLName.Speed, this) / term;
				}
				else
				{
					changeSpeed = (node.GetChildValue(BLName.Speed, this) - bullet.Speed) / term;
				}
			}

			term--;

			bullet.Speed += changeSpeed;

			// if (bullet.index == DISP_BULLET_INDEX)  Debug.WriteLine(String.Format("ChangeSpeed:{0} (type:{1} val:{2})", bullet.Speed, node.GetChild(BLName.Speed).type, node.GetChildValue(BLName.Speed, this)));

			if (term <= 0)
			{
				end = true;
				return BLRunStatus.End;
			}
			else
			{
				return BLRunStatus.Continue;
			}
		}
	}

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