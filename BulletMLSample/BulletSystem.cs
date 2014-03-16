using BulletMLLib;

namespace BulletMLSample
{
    /// <summary>
    /// BulletMLLibから呼ばれる関数群を実装
    /// </summary>
    class MyBulletFunctions: IBulletMLManager
    {
        public float GetRandom() { return (float)Game1.rand.NextDouble(); }

        public float GetRank() { return 0; }

        public float GetShipPosX() { return Game1.myship.pos.X; } //自機の座標を返す

        public float GetShipPosY() { return Game1.myship.pos.Y; } //自機の座標を返す
    }

}
