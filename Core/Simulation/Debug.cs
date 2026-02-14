using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
//using System.Drawing;
using System.Linq;
//using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EvakuacioSzimulacio.Core.Simulation
{
	internal class Debug
	{
		

		public Vector2 position {  get; set; }
		public float radius { get; set; }
		public Vector2 velocity { get; set; }
		public Vector2 desiredvelocity { get; set; }
		public Vector2 target {  get; set; }
		Texture2D pixel;
		public Vector2 leftaxis { get; set; }
		public Vector2 rightaxis { get; set; }
		public Vector2 taucirclemiddle { get; set; }
		public float taucircleradius { get; set; }
		public Vector2 circlemiddle { get; set; }
		public float circleradius { get; set; }
		public Vector2 projectedpoint { get; set; }
		public Vector2 orcapoint { get; set; }
		public Vector2 orcadirection { get; set; }
		public Vector2 newveloc { get; set; }


		public Debug(Vector2 position, float radius, Vector2 velocity, Vector2 desiredvelocity, Vector2 target,
			Vector2 leftaxis, Vector2 rightaxis, Vector2 taucirclemiddle, float taucircleradius,
			Vector2 circlemiddle, float circleradius, Vector2 projectedpoint, Vector2 orcapoint, Vector2 orcadirection, Vector2 newveloc)
		{
			this.position = position;
			this.radius = radius;
			this.velocity = velocity;
			this.desiredvelocity = desiredvelocity;
			this.target = target;
			this.leftaxis = leftaxis;
			this.rightaxis = rightaxis;
			this.taucirclemiddle = taucirclemiddle;
			this.taucircleradius = taucircleradius;
			this.circlemiddle = circlemiddle;
			this.circleradius = circleradius;
			this.projectedpoint = projectedpoint;
			this.orcapoint = orcapoint;
			this.orcadirection = orcadirection;
			this.newveloc = newveloc;
		}

		public void Draw(SpriteBatch spriteBatch)
		{
			if (pixel == null)
			{
				// Hozzunk létre 1x1 fehér pixelt
				pixel = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
				pixel.SetData(new[] { Color.White });
			}

			// Radius kirajzolása (kör)
			//DrawCircle(spriteBatch, position, radius, Color.Red);
			DrawCircle(spriteBatch, position + taucirclemiddle, taucircleradius, Color.Green);
			//DrawCircle(spriteBatch, position + circlemiddle, circleradius, Color.Red);
			//DrawCircle(spriteBatch, position + desiredvelocity + projectedpoint, 4, Color.White);



			// Velocity vektor kirajzolása
			//DrawLine(spriteBatch, position, (position + velocity), Color.Yellow, 2f);
			//DrawLine(spriteBatch, position + taucirclemiddle, (position + taucirclemiddle + new Vector2(0, 1) * taucircleradius), Color.Brown, 2f);
			//DrawLine(spriteBatch, position + new Vector2(100, 100), (position + velocity) + new Vector2(100,100), Color.Yellow, 2f);
			//DrawCircle(spriteBatch, position + new Vector2(100,100), 5, Color.Green);



			// DesiredVelocity vektor kirajzolása
			DrawLine(spriteBatch, position, (position + desiredvelocity), Color.Black, 2f);

			DrawLine(spriteBatch, position, position + leftaxis, Color.Pink, 2f, 50);
			DrawLine(spriteBatch, position, position + rightaxis, Color.Magenta, 2f, 50);
			//DrawLine(spriteBatch, position + orcapoint - orcadirection * 30, position + orcapoint + orcadirection, Color.Chocolate, 2f, 30);
			//DrawLine(spriteBatch, position, position + newveloc, Color.Aqua,1f);

			// Target pont kirajzolása
			//DrawCircle(spriteBatch, target, 5, Color.Blue); // kis kör a targethez
		}
		private void DrawCircle(SpriteBatch sb, Vector2 center, float radius, Color color, int segments = 20)
		{
			Vector2 prev = center + new Vector2(radius, 0);
			float increment = MathF.Tau / segments;

			for (int i = 1; i <= segments; i++)
			{
				float angle = i * increment;
				Vector2 next = center + new Vector2(MathF.Cos(angle) * radius, MathF.Sin(angle) * radius);
				DrawLine(sb, prev, next, color);
				prev = next;
			}
		}
		public void DrawLine(SpriteBatch sb, Vector2 start, Vector2 end, Color color, float thickness = 1f, int lengthnövelés = 1)
		{
			Vector2 edge = end - start;
			float angle = (float)Math.Atan2(edge.Y, edge.X);
			//if (edge.Length() < 20)
			//{
			//	edge.Normalize();

			//}
			sb.Draw(
				pixel,
				new Rectangle(
					(int)start.X,
					(int)start.Y,
					(int)edge.Length() * lengthnövelés,
					(int)thickness
				),
				null,
				color,
				angle,
				Vector2.Zero,
				SpriteEffects.None,
				0
			);
		}
	}
}
