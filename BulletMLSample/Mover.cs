using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using BulletMLLib;

namespace BulletMLSample
{
    /// <summary>
    /// 弾や敵オブジェクト（自身が弾源になる場合も、弾源から呼び出される場合もあります）
    /// </summary>
    class Mover : IBulletMLBulletInterface
    {
        public BulletMLBullet mlBullet;
        public bool used;
        public bool bulletRoot;
        public Vector2 pos;

        public void Init()
        {
            used = true;
            bulletRoot = false;
            mlBullet = new BulletMLBullet(this);
        }

        public void Update()
        {
            //BulletMLで自分を動かす
            if (mlBullet.Run()) //自分が弾の発信源なら、処理終了後に自動的に消える
                if(bulletRoot)
                    used = false;

            if (X < 0 || X > Game1.graphics.PreferredBackBufferWidth || Y < 0 || Y > Game1.graphics.PreferredBackBufferHeight)
                used = false;
           
        }

        /// BulletMLの弾幕定義を自分にセット
        public void SetBullet(BulletMLTree tree)
        {
            mlBullet.InitTop(tree);
        }

        ///以下、BulletMLLibに必要なインターフェイスを実装します

        /// <summary>
        /// 新しい弾(Mover)を作成するときライブラリから呼ばれる
        /// </summary>
        public BulletMLBullet GetNewBullet()
        {
            bulletRoot = true;
            Mover mover = MoverManager.CreateMover();
            return mover.mlBullet;
        }

        /// <summary>
        /// 弾が消えたときにライブラリから呼び出される
        /// </summary>
        public void Vanish()
        {
            used = false;
        }

        // 座標、向き、速度のプロパティを実装します。
        public float X
        {
            get { return pos.X; }
            set { pos.X = value; }
        }

        public float Y
        {
            get { return pos.Y; }
            set { pos.Y = value; }
        }

        public float Dir { get; set; }
        public float Speed { get; set; }


    }

 
}
