using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace BulletMLSample
{
    class Myship
    {
        public Vector2 pos;
        float speed = 3;

        public void Init()
        {
            pos.X = Game1.graphics.PreferredBackBufferWidth / 2;
            pos.Y = Game1.graphics.PreferredBackBufferHeight / 2; ;
        }

        public void Update()
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Left))
                pos.X -= speed;
            if (Keyboard.GetState().IsKeyDown(Keys.Right))
                pos.X += speed;
            if (Keyboard.GetState().IsKeyDown(Keys.Up))
                pos.Y -= speed;
            if (Keyboard.GetState().IsKeyDown(Keys.Down))
                pos.Y += speed;

        }
    }


}
