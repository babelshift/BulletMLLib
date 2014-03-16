//#define ExpandedBulletML

using System.Collections.Generic;

namespace BulletMLLib
{
	public enum BLType { None, Aim, Absolute, Relative, Sequence } ;

#if ExpandedBulletML
    enum BLName
    {
        Bullet, Action, Fire, ChangeDirection, ChangeSpeed, Accel,
        Wait, Repeat, BulletRef, ActionRef, FireRef, Vanish,
        Horizontal, Vertical, Term, Times, Direction, Speed, Param,
        Bulletml, None
    } ;
#else

	public enum BLName
	{
		Bullet, Action, Fire, ChangeDirection, ChangeSpeed, Accel,
		Wait, Repeat, BulletRef, ActionRef, FireRef, Vanish,
		Horizontal, Vertical, Term, Times, Direction, Speed, Param,
		Bulletml, None
	} ;

#endif

	public enum BLValueType { Number, Rand, Rank, Operator, Param } ;

	public class BulletMLTree
	{
		public BLName name;
		public BLType type;
		public string label;
		public BulletMLTree parent;
		public BulletMLTree next;
		public List<BulletValue> values;
		public List<BulletMLTree> children;
#if ExpandedBulletML
        public bool visible;
        public string bulletName;
#endif

		public BulletMLTree()
		{
			children = new List<BulletMLTree>();
			values = new List<BulletValue>();
			parent = null;
			next = null;
#if ExpandedBulletML
            visible = true;
            bulletName = "";
#endif
		}

		public BulletMLTree GetLabelNode(string label, BLName name)
		{
			BulletMLTree rootNode = this; //先頭までさかのぼる
			while (rootNode.parent != null)
				rootNode = rootNode.parent;

			foreach (BulletMLTree tree in rootNode.children)
			{
				if (tree.label == label && tree.name == name)
					return tree;
			}
			return null;
		}

		public float GetChildValue(BLName name, BulletMLTask task)
		{
			foreach (BulletMLTree tree in children)
			{
				if (tree.name == name)
					return tree.GetValue(task);
			}
			return 0;
		}

		public BulletMLTree GetChild(BLName name)
		{
			foreach (BulletMLTree node in children)
			{
				if (node.name == name)
					return node;
			}
			return null;
		}

		public float GetValue(BulletMLTask task)
		{
			int startIndex = 0;

			return GetValue(0, ref startIndex, task);
		}

		public float GetValue(float v, ref int i, BulletMLTask task)
		{
			for (; i < values.Count; i++)
			{
				if (values[i].valueType == BLValueType.Operator)
				{
					if (values[i].value == '+')
					{
						i++;
						if (IsNextNum(i))
							v += GetNumValue(values[i], task);
						else
							v += GetValue(v, ref i, task);
					}
					else if (values[i].value == '-')
					{
						i++;
						if (IsNextNum(i))
							v -= GetNumValue(values[i], task);
						else
							v -= GetValue(v, ref i, task);
					}
					else if (values[i].value == '*')
					{
						i++;
						if (IsNextNum(i))
							v *= GetNumValue(values[i], task);
						else
							v *= GetValue(v, ref i, task);
					}
					else if (values[i].value == '/')
					{
						i++;
						if (IsNextNum(i))
							v /= GetNumValue(values[i], task);
						else
							v /= GetValue(v, ref i, task);
					}
					else if (values[i].value == '(')
					{
						i++;
						float res = GetValue(v, ref i, task);
						if ((i < values.Count - 1 && values[i + 1].valueType == BLValueType.Operator)
							   && (values[i + 1].value == '*' || values[i + 1].value == '/'))
							return GetValue(res, ref i, task);
						else
							return res;
					}
					else if (values[i].value == ')')
					{
						//Debug.WriteLine(" ）の戻り値:" + v);
						return v;
					}
				}
				else if (i < values.Count - 1 && values[i + 1].valueType == BLValueType.Operator && values[i + 1].value == '*')
				{
					// 次が掛け算のとき
					float val = GetNumValue(values[i], task);
					i += 2;
					if (IsNextNum(i))
						return val * GetNumValue(values[i], task);
					else
						return val * GetValue(v, ref i, task);
				}
				else if (i < values.Count - 1 && values[i + 1].valueType == BLValueType.Operator && values[i + 1].value == '/')
				{
					// 次が割り算のとき
					float val = GetNumValue(values[i], task);
					i += 2;
					if (IsNextNum(i))
						return val / GetNumValue(values[i], task);
					else
						return val / GetValue(v, ref i, task);
				}
				else
					v = GetNumValue(values[i], task);
			}

			return v;
		}

		private bool IsNextNum(int i)
		{
			if ((i < values.Count - 1 && values[i + 1].valueType == BLValueType.Operator) && (values[i + 1].value == '*' || values[i + 1].value == '/'))
			{
				return false;
			}
			else if (values[i].value == ')' || values[i].value == '(')
			{
				return false;
			}
			else
				return true;
		}

		private float GetNumValue(BulletValue v, BulletMLTask task)
		{
			if (v.valueType == BLValueType.Number)
			{
				return v.value;
			}
			else if (v.valueType == BLValueType.Rand)
			{
				return (float)BulletMLManager.GetRandom();
			}
			else if (v.valueType == BLValueType.Rank)
			{
				return BulletMLManager.GetRank();
			}
			else if (v.valueType == BLValueType.Param)
			{
				BulletMLTask ownerTask = task;
				while (ownerTask.paramList.Count == 0)
					ownerTask = ownerTask.owner;
				float val = ownerTask.paramList[(int)v.value - 1];

				//BulletMLTask ownerTask = task;
				//while (ownerTask.paramNode == null)
				//    ownerTask = ownerTask.owner;
				//float val = ownerTask.paramNode.children[(int)v.value - 1].GetValue(ownerTask.owner);

				//Debug.WriteLine(String.Format( "{2} param{0} = {1}", (int)v.value - 1, val, ownerTask));
				return val;
			}
			else
			{
				//Debug.WriteLine("不正な値がパラメータになっています");
				return 0;
			}
		}

		internal float GetParam(int p, BulletMLTask task)
		{
			return children[p].GetValue(task); //<param>以外のタグは持っていないので
		}
	}
}