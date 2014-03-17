using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BulletMLLib
{
	public static class BulletMLManager
	{
		private static IBulletMLManager ib;

		static public void Init(IBulletMLManager ib1)
		{
			ib = ib1;
		}

		static public float GetRandom()
		{
			return ib.GetRandom();
		}

		static public float GetRank()
		{
			return ib.GetRank();
		}

		static public float GetPlayerPositionX()
		{
			return ib.GetPlayerPositionX();
		}

		static public float GetPlayerPositionY()
		{
			return ib.GetPlayerPositionY();
		}
	}
}
