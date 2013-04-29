using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace TRODS
{
    class Attack : AbstractScene
    {
        private AnimatedSprite _sprite;
        public bool Active { get; set; }
        public Rectangle Position
        {
            get
            {
                return _sprite != null ? _sprite.Position : new Rectangle();
            }
            private set { }
        }
        private int _lifetime;
        public int Duration { get; set; }
        public float Damage { get; set; }
        public int BlockTime { get; set; }
        public int AttackTime { get; set; }
        public float Consumption { get; set; }

        public Attack(Rectangle winSize, AnimatedSprite sprite, int duration = 50, float damage = 0.2f, int blockTime = 300, int attackTime = 50,float consumption=0.1f)
        {
            _sprite = sprite;
            Active = false;
            _lifetime = 0;
            Duration = duration;
            Damage = damage;
            BlockTime = blockTime;
            AttackTime = attackTime;
            Consumption = consumption;
        }

        public void Launch(Rectangle position)
        {
            Active = true;
            _lifetime = Duration;
            _sprite.Position = position;
        }

        public override void LoadContent(ContentManager content)
        {
            if (_sprite != null && _sprite.AssetName != "")
                _sprite.LoadContent(content);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Active && _sprite != null)
                _sprite.Draw(spriteBatch);
        }

        public override void Update(float elapsedTime)
        {
            _lifetime -= (int)elapsedTime;
            if (Active && _sprite != null)
                _sprite.Update(elapsedTime);
            if (_lifetime < 0)
                Active = false;
        }

        public override void WindowResized(Rectangle rect)
        {
            if (_sprite != null)
                _sprite.windowResized(rect);
        }
    }
}
