﻿using System;
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
    class Character : AbstractScene
    {
        protected Dictionary<CharacterActions, Attack> _attacks;
        public Dictionary<CharacterActions, Attack> Attacks
        {
            get { return _attacks; }
            private set { _attacks = value; }
        }
        protected Rectangle _windowSize;
        protected GraphicalBounds<CharacterActions> _graphicalBounds;
        private CharacterActions _action;

        public CharacterActions Action
        {
            get { return _action; }
            set { _action = value; }
        }
        protected AnimatedSprite _sprite;
        protected Vector2 _position;
        public Vector2 Position
        {
            get { return _position; }
            set { _position = value; }
        }
        public Rectangle DrawingRectangle { get { return _sprite != null ? _sprite.Position : new Rectangle(); } }
        protected Physics _physics;
        public bool _canMove { get; protected set; }
        public bool _isOnGround { get; protected set; }
        public bool _jumping { get; protected set; }
        public int _jumpHeight { get; protected set; }
        protected bool _direction;
        protected int _timer;
        private Weapon _weapon;
        public Weapon Weapon
        {
            get { return _weapon; }
            set { _weapon = value; }
        }
        public float Life { get; set; }

        public Character(Rectangle winSize, Vector2 position, int width, int height,
            string assetName, int textureColumns, int textureLines)
        {
            _windowSize = winSize;
            _position = position;
            _graphicalBounds = new GraphicalBounds<CharacterActions>(new Dictionary<CharacterActions, Rectangle>());
            _sprite = new AnimatedSprite(new Rectangle((int)position.X - width / 2, (int)position.Y - height, width, height), winSize, assetName, textureColumns, textureLines, 30, 1, -1, -1, true);
            _canMove = true;
            _jumping = false;
            _jumpHeight = 0;
            _direction = true; // = right
            _timer = 0;
            _physics = new Physics();
            Life = 1;
            _action = CharacterActions.StandRight;
            _attacks = new Dictionary<CharacterActions, Attack>();
        }

        public override void LoadContent(ContentManager content)
        {
            _sprite.LoadContent(content);
            if (_weapon != null)
                _weapon.LoadContent(content);
            foreach (Attack a in _attacks.Values)
                a.LoadContent(content);
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!_jumping)
            {
                _sprite.Draw(spriteBatch);
                if (_weapon != null)
                    _weapon.Draw(spriteBatch, _sprite.Position);
            }
            else
            {
                _sprite.Draw(spriteBatch, new Vector2(_sprite.Position.X, _sprite.Position.Y - _jumpHeight));
                if (_weapon != null)
                    _weapon.Draw(spriteBatch, new Rectangle(_sprite.Position.X, _sprite.Position.Y - _jumpHeight, _sprite.Position.Width, _sprite.Position.Height));
            }
            foreach (Attack a in _attacks.Values)
                a.Draw(spriteBatch);
        }
        public override void Update(float elapsedTime)
        {
            _timer -= (int)elapsedTime;
            _sprite.Update(elapsedTime);
            if (_weapon != null)
                _weapon.Update(elapsedTime);
            if (!_sprite._repeating && _sprite.IsEnd())
                Stand(_direction);
            if (_jumping)
                _jumpHeight += _physics.Update(elapsedTime);
            testOnGround();
            if (_timer < 0 && _canMove == false)
            {
                _timer = 0;
                _canMove = true;
                Stand(_direction);
            }
            foreach (Attack a in _attacks.Values)
                a.Update(elapsedTime);

            if (Life < 0)
                Life = 0;
            else if (Life > 1)
                Life = 1;
        }
        public override void WindowResized(Rectangle rect)
        {
            _sprite.windowResized(rect);
            if (_weapon != null)
                _weapon.WindowResized(rect);

            float x = (float)rect.Width / (float)_windowSize.Width, y = (float)rect.Height / (float)_windowSize.Height;

            _physics.WindowResized(y);

            _position.X *= x;
            _position.Y *= y;

            _jumpHeight = (int)((float)_jumpHeight * y);

            foreach (Attack a in _attacks.Values)
                a.WindowResized(rect);

            _windowSize = rect;
        }

        public virtual void AddAttack(CharacterActions action, Attack attac)
        {
            _attacks.Add(action, attac);
        }
        public virtual void Attack(CharacterActions attack)
        {
            if (_attacks.ContainsKey(attack))
            {
                switch (attack)
                {
                    case CharacterActions.Attack1Right:
                        _attacks[attack].Launch(new Rectangle((int)Position.X, _sprite.Position.Y, _sprite.Position.Width / 2, _sprite.Position.Height));
                        break;
                    case CharacterActions.Attack1Left:
                        _attacks[attack].Launch(new Rectangle((int)Position.X, (int)Position.Y, _sprite.Position.Width / 2, _sprite.Position.Height));
                        break;
                    case CharacterActions.AttackStunLeft:
                    case CharacterActions.AttackStunRight:
                        Rectangle r = _sprite.Position;
                        _attacks[attack].Launch(new Rectangle(r.X - 2 * r.Width, r.Y - r.Height, 5 * r.Width, 3 * r.Height));
                        break;
                }
                _canMove = false;
                _timer = _attacks[attack].AttackTime;
            }
        }
        public virtual void ReceiveAttack(float damage = 0, int blockTime = 100)
        {
            _action = _direction ? CharacterActions.ReceiveAttackRight : CharacterActions.ReceiveAttackLeft;
            _canMove = false;
            _timer = blockTime;
            actualizeSpriteGraphicalBounds();
            Life -= damage;
        }
        public virtual void Stand(bool right)
        {
            if (!(right == _direction && (_action == CharacterActions.StandRight || _action == CharacterActions.StandLeft)))
            {
                _direction = right;
                if (right)
                    _action = CharacterActions.StandRight;
                else
                    _action = CharacterActions.StandLeft;
                actualizeSpriteGraphicalBounds();
            }
        }
        public virtual void Jump()
        {
            if (_canMove && !_jumping)
            {
                if (_direction)
                    _action = CharacterActions.JumpRight;
                else
                    _action = CharacterActions.JumpLeft;
                actualizeSpriteGraphicalBounds();
                _jumpHeight = 0;
                _jumping = true;
                _isOnGround = false;
                _physics.Jump();
            }
        }
        public virtual void Move(bool right)
        {
            if (_canMove && !(right == _direction && (_action == CharacterActions.WalkRight || _action == CharacterActions.WalkLeft)))
            {
                _direction = right;
                if (right)
                    _action = CharacterActions.WalkRight;
                else
                    _action = CharacterActions.WalkLeft;
                actualizeSpriteGraphicalBounds();
            }
        }
        public virtual void Paralize(int time)
        {
            _canMove = false;
            _timer = time;
            _action = CharacterActions.Paralized;
        }
        public virtual void Free()
        {
            _canMove = true;
        }
        public virtual void DoubleDash()
        {
        }

        protected virtual bool testOnGround()
        {
            if (_jumpHeight < 0)
            {
                _jumping = false;
                _jumpHeight = 0;
                _isOnGround = true;
                Stand(_direction);
                return true;
            }
            else
                return false;
        }
        protected virtual void actualizeSpritePosition()
        {
            _sprite.setRelatvePos(
                new Rectangle(
                    (int)_position.X - _sprite.Position.Width / 2,
                    (int)_position.Y - _sprite.Position.Height,
                    _sprite.Position.Width,
                    _sprite.Position.Height),
                    _windowSize.Width, _windowSize.Height);
        }
        protected virtual void actualizeSpriteGraphicalBounds()
        {
            Rectangle r = _graphicalBounds.get(_action);
            _sprite.SetPictureBounds(r.Y, r.Width, r.X, true);
            _sprite.Speed = r.Height;
            if (_weapon != null)
                _weapon.actualizeSpriteGraphicalBounds(r);
        }
    }
}
