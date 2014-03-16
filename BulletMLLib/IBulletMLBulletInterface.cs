using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
}
