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
}