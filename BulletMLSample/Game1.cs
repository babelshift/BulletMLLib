using System;
using BulletMLLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
namespace BulletMLSample
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    class Game1 : Microsoft.Xna.Framework.Game
    {
        static public GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Texture2D texture;
        static public Myship myship;
        static public BulletMLParser parser = new BulletMLParser(); //BulletML����͂��A��͌��ʂ�ێ�����N���X�BXML���ƂɕK�v�ł��B
        static public Random rand = new Random();
        int timer = 0;
        Mover mover;
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 320;// 640;
            graphics.PreferredBackBufferHeight = 240;// 480;
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            base.Initialize();
            //���@������
            myship = new Myship();
            myship.Init();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            texture = Content.Load<Texture2D>("Sprites\\bullet");
            parser.ParseXML(@"Content\sample.xml"); ///BulletML�����
            //parser.ParseXML(@"Content\xml\[1943]_rolling_fire.xml");
            //parser.ParseXML(@"Content\xml\[Guwange]_round_2_boss_circle_fire.xml");
            //parser.ParseXML(@"Content\xml\[Guwange]_round_3_boss_fast_3way.xml");
            //parser.ParseXML(@"Content\xml\[Guwange]_round_4_boss_eye_ball.xml");
            //parser.ParseXML(@"Content\xml\[G_DARIUS]_homing_laser.xml");
            //parser.ParseXML(@"Content\xml\[Progear]_round_1_boss_grow_bullets.xml");
            //parser.ParseXML(@"Content\xml\[Progear]_round_2_boss_struggling.xml");
            //parser.ParseXML(@"Content\xml\[Progear]_round_3_boss_back_burst.xml");
            //parser.ParseXML(@"Content\xml\[Progear]_round_3_boss_wave_bullets.xml");
            //parser.ParseXML(@"Content\xml\[Progear]_round_4_boss_fast_rocket.xml");
            //parser.ParseXML(@"Content\xml\[Progear]_round_5_boss_last_round_wave.xml");
            //parser.ParseXML(@"Content\xml\[Progear]_round_5_middle_boss_rockets.xml");
            //parser.ParseXML(@"Content\xml\[Progear]_round_6_boss_parabola_shot.xml");
            //parser.ParseXML(@"Content\xml\[Psyvariar]_X-A_boss_opening.xml");
            //parser.ParseXML(@"Content\xml\[Psyvariar]_X-A_boss_winder.xml");
            //parser.ParseXML(@"Content\xml\[Psyvariar]_X-B_colony_shape_satellite.xml");
            //parser.ParseXML(@"Content\xml\[XEVIOUS]_garu_zakato.xml");
            //BulletML�̏�����
            BulletMLManager.Init(new MyBulletFunctions());

            //�G�����ʒ����ɍ쐬���A�e��f���悤�ݒ�
            mover = MoverManager.CreateMover();
            mover.pos = new Vector2(graphics.PreferredBackBufferWidth / 2, graphics.PreferredBackBufferHeight/2);
            mover.SetBullet(parser.tree); //BulletML�œ������悤�ɐݒ�
        }


        protected override void UnloadContent()
        {
        }


        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                this.Exit();
            timer++;
            if (timer > 60)
            {
                timer = 0;
                if (mover.used == false)
                {
                    //�G�����ʒ����ɍ쐬���A�e��f���悤�ݒ�
                    mover = MoverManager.CreateMover();
                    mover.pos = new Vector2(graphics.PreferredBackBufferWidth / 4 + graphics.PreferredBackBufferWidth/2 * (float)rand.NextDouble(), graphics.PreferredBackBufferHeight/2 * (float)rand.NextDouble());
                    mover.SetBullet(parser.tree); //BulletML�œ������悤�ɐݒ�
                }
            }

            //���ׂĂ�Mover���s��������
            MoverManager.Update();
            //�g��Ȃ��Ȃ���Mover�����
            MoverManager.FreeMovers();
            // ���@���X�V
            myship.Update();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            //�G��e��`��
            spriteBatch.Begin();

            foreach(Mover mover in MoverManager.movers)
                spriteBatch.Draw(texture, mover.pos, Color.Black);

            spriteBatch.Draw(texture, myship.pos, Color.Black);

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
