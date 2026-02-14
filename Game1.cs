using EvakuacioSzimulacio.Core;
using EvakuacioSzimulacio.Core.Simulation;
//using Microsoft.VisualBasic.Devices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
//using System.Windows.Forms;
//using System.Windows.Forms;
//using System.Drawing;
//using System.Drawing;

namespace EvakuacioSzimulacio
{
	public class Game1 : Game
	{
		private GraphicsDeviceManager _graphics;
		private SpriteBatch _spriteBatch;
		private TileMap _map;
		private List<Person> _people;
		private Texture2D _personTexture;
		private Texture2D _circleTexture;
		private MovementManager _movementManager;
		private Vector2 _target = Vector2.Zero;
		Camera _camera;
		Random rnd;
		Texture2D pixel;
		SpriteFont font;
		private double fps;
		private double elapsedTime;

		private List<Person> _exitedPeople;

		public Game1()
		{
			_graphics = new GraphicsDeviceManager(this);
			_graphics.PreferredBackBufferWidth = 1920;
			_graphics.PreferredBackBufferHeight = 900;
			_graphics.ApplyChanges();
			Content.RootDirectory = "Content";
			IsMouseVisible = true;
			IsFixedTimeStep = false;                    // ne legyen fix timestep
			_graphics.SynchronizeWithVerticalRetrace = false; // kikapcsolja a V-Sync-et
		}

		protected override void Initialize()
		{
			base.Initialize();
			_map = new TileMap(GraphicsDevice);
			_personTexture = new Texture2D(GraphicsDevice, 1, 1);
			_personTexture.SetData(new[] { Color.White });
			_camera = new Camera();
			rnd = new Random();
			_exitedPeople = new List<Person>();
			

			

			_people = new List<Person>
			{
				//new Person(_circleTexture, new Vector2(3,3)*_map.tileSize,50f,10f),
				//new Person(_circleTexture, new Vector2(3,4)*_map.tileSize,70f,10f),
				//new Person(_circleTexture, new Vector2(300,95),70f,10f),
				////new Person(_circleTexture, new Vector2(150,300),40f,10f)

			};
			//_people[0].Direction = new Vector2(10, 10);
			//_people[1].Direction = new Vector2(-40, -10);
			//_people[2].Direction = new Vector2(-30, 20);
			////_people[3].Direction = new Vector2(1, 5);



			FillFolyoso();

			//int id = 0;
			//foreach (var l in _map.tileMap)
			//{
			//	if (l.Type == TileType.Chair && rnd.Next(1, 11) < 8)
			//	{
			//		_people.Add(new Person(id, _circleTexture, l.Center, rnd.Next(50, 71), 10f));
			//		id++;
			//	}
			//}





			//_people.Add(new Person(0, _circleTexture, new Vector2(336, 64), rnd.Next(50, 71), 10f));
			//_people.Add(new Person(1, _circleTexture, new Vector2(368, 127), rnd.Next(50, 71), 10f));



			//_people.Add(new Person(0, _circleTexture, new Vector2(112, 208), 10, 10f));
			//_people.Add(new Person(1, _circleTexture, new Vector2(208, 218), 10, 10f));
			//_people[0].Target = new Vector2(208, 208);
			//_people[1].Target = new Vector2(112, 208);


			_movementManager = new MovementManager(_people,_map);

			
		}

		protected override void LoadContent()
		{
			_spriteBatch = new SpriteBatch(GraphicsDevice);
			_circleTexture = Content.Load<Texture2D>("circle");
			pixel = new Texture2D(GraphicsDevice, 1, 1);
			pixel.SetData(new[] { Color.White });
			font = Content.Load<SpriteFont>("default");

		}

		protected override void Update(GameTime gameTime)
		{
			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
				Exit();
			Vector2 WorldPosition = new Vector2();
			Vector2 CameraPosition = _camera.Position;
			//foreach(Person p in _people)
			//{
			//	Vector2 next = p.NextPositionOption(gameTime);

			//	p.Position = next;
			//	p.Hitbox.Center = next;
			//}
			var mouse = Mouse.GetState();
			int scrollDelta = mouse.ScrollWheelValue; // aktuális görgő pozíció
			int previousScrollValue = 0;
			scrollDelta = mouse.ScrollWheelValue - previousScrollValue;

			_camera.AddZoom(scrollDelta * 0.00001f); // érzékenység állítható
			previousScrollValue = mouse.ScrollWheelValue;
			if (mouse.LeftButton == ButtonState.Pressed)
			{
				Point MovedCameraPosition = new Point(mouse.Position.X + (int)CameraPosition.X, mouse.Position.Y + (int)CameraPosition.Y);
				int idxX = MovedCameraPosition.X / _map.tileSize;
				int idxY = MovedCameraPosition.Y / _map.tileSize;
				int width = _map.tileMap.GetLength(1);
				int height = _map.tileMap.GetLength(0);
				bool insidemap = idxX > 0 && idxY > 0 && idxX < height && idxY < width;
				
				if(insidemap)
				{
					if (_map.tileMap[idxX, idxY].Type == TileType.Empty || _map.tileMap[idxX, idxY].Type == TileType.Chair || _map.tileMap[idxX,idxY].Type == TileType.Exit)
					{
						_target = new Vector2(MovedCameraPosition.X, MovedCameraPosition.Y);
						//_movementManager.target = _target;
						foreach (var p in _people)
						{
							p.Target = _target;
						}
					}
				}
				
				
			}
			

			KeyboardState state = Keyboard.GetState();
			Vector2 move = Vector2.Zero;
			
			if (state.IsKeyDown(Keys.Up))
				move.Y -= 1;
			if (state.IsKeyDown(Keys.Down))
				move.Y += 1;
			if (state.IsKeyDown(Keys.Left))
				move.X -= 1;
			if (state.IsKeyDown(Keys.Right))
				move.X += 1;

			if (move != Vector2.Zero)
				move.Normalize();

			_camera.Move(move);
			_camera.Update();
			_movementManager.WhereToMove(gameTime);
			_exitedPeople.AddRange(_people.Where(p => p.IsExited));
			_people.RemoveAll(p => p.IsExited);

			elapsedTime += gameTime.ElapsedGameTime.TotalSeconds;
			if(elapsedTime >= 1.0)
			{
				fps = 1.0/gameTime.ElapsedGameTime.TotalSeconds;
				elapsedTime = 0;
			}


			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			KeyboardState state = Keyboard.GetState();
			GraphicsDevice.Clear(Color.CornflowerBlue);
			
			_spriteBatch.Begin(transformMatrix: _camera.Transform);
			_map.Draw(_spriteBatch);
			


			foreach(Person p in _people)
			{
				p.Draw(_spriteBatch,p.Hitbox.Coloring,font);
				//Debug.WriteLine(p.Position);
				DrawCircle(_spriteBatch, p.Target, 5, Color.AliceBlue);

				
			}
			int i = 0;
			foreach (var d in _movementManager.debuglist)
			{
				if (i == 1) break;
				i++;
				//d.Draw(_spriteBatch);
			}
			_spriteBatch.DrawString(font, $"FPS: {fps:F1}", new Vector2(10, 10), Color.Red);

			_spriteBatch.End();
			if (state.IsKeyDown(Keys.A))
			{
				Thread.Sleep(15000);
			}

			base.Draw(gameTime);
		}
		public void DrawLine(SpriteBatch sb, Vector2 start, Vector2 end, Color color, float thickness = 1f)
		{
			Vector2 edge = end - start;
			float angle = (float)Math.Atan2(edge.Y, edge.X);

			sb.Draw(
				pixel,
				new Rectangle(
					(int)start.X,
					(int)start.Y,
					(int)edge.Length(),
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
		public void FillFolyoso()
		{
			int id = 0;
			foreach (var l in _map.tileMap)
			{
				if ((int)l.Center.X / 32 < 10 && (int)l.Center.X > 16 && (int)l.Center.Y > 16 && (int)l.Center.Y / 32 < 20 && rnd.Next(1,11) < 11)
				{
					Person p = new Person(id, _circleTexture, l.Center, rnd.Next(30, 71), 10f);
					p.Target = new Vector2(2576, 336);
					p.lastTarget = p.Target;
					p.Path = new List<Core.Pathfinding.Node>();
					p.Path.Add(new Core.Pathfinding.Node(80,(int)l.Center.Y / 32, true));
					_people.Add(p);
					id++;
				}
				if((int)l.Center.X / 32 > 73 && (int)l.Center.X / 32 < 83 && (int)l.Center.Y > 16 && (int)l.Center.Y / 32 < 20 && rnd.Next(1,11) < 11)
				{
					Person p = new Person(id, _circleTexture, l.Center, rnd.Next(30, 71), 10f);
					p.Target = new Vector2(48, 336);
					p.lastTarget = p.Target;
					p.Path = new List<Core.Pathfinding.Node>();
					p.Path.Add(new Core.Pathfinding.Node(10,(int)l.Center.Y / 32, true));
					_people.Add(p);
					id++;
				}
			}
		}
	}
	public class Camera
	{
		public Matrix Transform { get; private set; }
		public Vector2 Position { get; private set; }
		public float MoveSpeed = 5f;
		public float Zoom { get; private set; } = 1f;  // alapértelmezett 1 = 100%

		public void Move(Vector2 direction)
		{
			Position += direction * MoveSpeed;
		}

		public void Update()
		{
			// Létrehozzuk a transform mátrixot a pozíció és zoom alapján
			Transform =
				Matrix.CreateTranslation(new Vector3(-Position.X, -Position.Y, 0)) *
				Matrix.CreateScale(Zoom, Zoom, 1f);
		}

		// Új: zoom kezelés
		public void AddZoom(float amount)
		{
			Zoom += amount;
			Zoom = MathHelper.Clamp(Zoom, 0.1f, 5f); // például min 0.1, max 5
		}
	}
}
