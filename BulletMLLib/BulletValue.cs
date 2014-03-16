using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BulletMLLib
{
	public class BulletValue
	{
		public BLValueType valueType;
		public float value;

		public BulletValue(BLValueType type, float value)
		{
			this.valueType = type;
			this.value = value;
		}
	}
}
