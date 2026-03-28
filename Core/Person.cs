using EvakuacioSzimulacio.Core.Pathfinding;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace EvakuacioSzimulacio.Core
{
	internal class Person
	{
        private int ID { get; set; }
        private Vector2 direction;
		Texture2D texture;
		public Vector2 Position { get; set; }
		public Vector2 Direction { get { return direction; } set { direction = value; } }
        public Vector2 DesiredVelocity { get; set; }
		public float Speed { get; set; } //ez maximális érték amit felvehet az adott person gyorsaságként. Ennél gyorsabb nem lehet
        public CircleHitbox Hitbox { get; set; }
        public List<Node> Path { get; set; }
        public Vector2 lastTarget {  get; set; }
        public Tile CurrentTile { get; set; }
        public bool IsExited { get; set; }
        public Vector2 Target { get;set; }
        public Vector2 LastPosition { get; set; }
        public float StationaryTime { get; set; }
        public bool pathNeedsRefreshment = false;
        public float pathTimer = 0.0f;
        public float pathTimerInterval = 0.5f;
        public bool hasPerturbated = false;

        public Person(int id, Texture2D ctorTexture, Vector2 ctorPosition, float ctorSpeed, float circleHitboxRadius)
        {
            ID = id;
            texture = ctorTexture;
            Position = ctorPosition;
            Speed = ctorSpeed;
            IsExited = false;
			Hitbox = new CircleHitbox(ctorPosition,circleHitboxRadius);
            direction = new Vector2(0, 0);
        }

        //public Vector2 NextPositionOption(GameTime gameTime)
        //{
        //    return Position + Direction * Speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
        //}

        public void Draw(SpriteBatch spriteBatch, Color color, SpriteFont font)
        {
            spriteBatch.Draw(texture, new Rectangle((int)Position.X - (int)Hitbox.Radius, (int)Position.Y-(int)Hitbox.Radius,(int)Hitbox.Radius*2,(int)Hitbox.Radius*2),Color.Red);
			spriteBatch.Draw(texture,
				new Rectangle((int)(Position.X), (int)(Position.Y), 2, 2),
				Color.Black);  //Person középpontja
            spriteBatch.DrawString(font, $"{ID}", Position, Color.Black);
		}
	}

    class CircleHitbox
    {
        public Vector2 Center;
        public float Radius;
        public Color Coloring;


		public CircleHitbox(Vector2 center,float radius)
		{
			Center = center;
            Radius = radius;
            Coloring = Color.Red;
		}
	}
}
