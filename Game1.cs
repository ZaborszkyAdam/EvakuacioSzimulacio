using EvakuacioSzimulacio.Core;
using EvakuacioSzimulacio.Core.Simulation;
//using Microsoft.VisualBasic.Devices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
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
		private TileType _selectedBrush = TileType.Wall;

		public enum GameState { Simulation, Editor, Menu, Load }
		private GameState _currentState = GameState.Menu;


		private string mapsFolder = "maps";
		private List<string> _mapFiles = new List<string>();
		private int _selectedMapIndex = 0;
		
		
		public Game1()
		{
			_graphics = new GraphicsDeviceManager(this);
			_graphics.PreferredBackBufferWidth = 1920;
			_graphics.PreferredBackBufferHeight = 1080;
			_graphics.ApplyChanges();
			Content.RootDirectory = "Content";
			
			IsMouseVisible = true;
			IsFixedTimeStep = false;                    // ne legyen fix timestep
			_graphics.SynchronizeWithVerticalRetrace = false; // kikapcsolja a V-Sync-et
		}

		protected override void Initialize()
		{
			base.Initialize();
			_map = new TileMap(GraphicsDevice,0,0);
			_personTexture = new Texture2D(GraphicsDevice, 1, 1);
			_personTexture.SetData(new[] { Color.White });
			_camera = new Camera();
			rnd = new Random();
			_exitedPeople = new List<Person>();

			if (!System.IO.Directory.Exists(mapsFolder))
			{
				System.IO.Directory.CreateDirectory(mapsFolder);
			}
			
			_people = new List<Person>{};
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
			{
				Thread.Sleep(100);//Kell ide egy ilyen egyenlőre, mivel ha nagyon gyors a váltás, akkor még lenyomva tartva tekinti az esc gombot pl az editorból való váltást a menübe, így ki is lép a programból
				if (_currentState != GameState.Menu)
				{
					_currentState = GameState.Menu;
				}
				else
				{
					Exit();
				}
			}
			var kState = Keyboard.GetState();
			if (kState.IsKeyDown(Keys.E))
			{
				if(_currentState == GameState.Menu)
				{
					InitializeLevel(50, 50);
				}
				_currentState = GameState.Editor;
			}
			if (kState.IsKeyDown(Keys.S))
			{
				if (_currentState == GameState.Editor)
				{
					if (_people.Count == 0)
					{
						int id = 0;
						for (int x = 0; x < _map.tileMap.GetLength(0); x++)
						{
							for (int y = 0; y < _map.tileMap.GetLength(1); y++)
							{
								if (_map.tileMap[x, y].Type == TileType.Spawn)
								{
									_people.Add(new Person(id, _circleTexture, _map.tileMap[x,y].Center, rnd.Next(30,71), 10f));
									id++;
								}
							}
						}
					}
					_movementManager.UpdateReferences(_map, _people);
					_movementManager.RefreshEnvironment();
				}
				ResetElapsedTime();
				_currentState = GameState.Simulation;
			}
			if (kState.IsKeyDown(Keys.L))
			{
				_currentState = GameState.Load;
			}

			switch( _currentState )
			{
				case GameState.Menu:MenuUpdate(gameTime); break;
				case GameState.Editor:EditorUpdate(gameTime); break;
				case GameState.Simulation:SimultaionUpdate(gameTime); break;
				case GameState.Load:LoadUpdate(gameTime); break;
			}





			base.Update(gameTime);

		}

		protected void MenuUpdate(GameTime gameTime)
		{

		}
		protected void EditorUpdate(GameTime gameTime)
		{
			var mouse = Mouse.GetState();
			var kState = Keyboard.GetState();

			if (kState.IsKeyDown(Keys.D1)) _selectedBrush = TileType.Wall;
			if (kState.IsKeyDown(Keys.D2)) _selectedBrush = TileType.Empty;
			if (kState.IsKeyDown(Keys.D3)) _selectedBrush = TileType.Chair;
			if (kState.IsKeyDown(Keys.D4)) _selectedBrush = TileType.Exit;
			if (kState.IsKeyDown(Keys.D5)) _selectedBrush = TileType.Spawn;

			Vector2 move = Vector2.Zero;
			if (kState.IsKeyDown(Keys.Up)) move.Y -= 1;
			if (kState.IsKeyDown(Keys.Down)) move.Y += 1;
			if (kState.IsKeyDown(Keys.Left)) move.X -= 1;
			if (kState.IsKeyDown(Keys.Right)) move.X += 1;

			if (move != Vector2.Zero) move.Normalize();
			_camera.Move(move);

			_camera.Update();


			// MENTÉS: 'K' billentyű
			if (kState.IsKeyDown(Keys.K))
			{
				string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
				string fileName = $"map_{timestamp}.csv";

				// Összefűzzük: "maps/map_2023...csv"
				string fullPath = System.IO.Path.Combine(mapsFolder, fileName);

				_map.SaveToFile(fullPath);
				System.Diagnostics.Debug.WriteLine($"Mentve a mappába: {fullPath}");

				System.Threading.Thread.Sleep(200);
			}

			// BETÖLTÉS: 'L' billentyű
			if (kState.IsKeyDown(Keys.L))
			{
				if (System.IO.Directory.Exists(mapsFolder))
				{
					var directory = new System.IO.DirectoryInfo(mapsFolder);
					var latestFile = directory.GetFiles("*.csv")
											  .OrderByDescending(f => f.CreationTime)
											  .FirstOrDefault();

					if (latestFile != null)
					{
						_map.LoadFromFile(latestFile.FullName); // Teljes elérési út kell
						_movementManager.UpdateReferences(_map, _people);
						_movementManager.RefreshEnvironment();
					}
				}
				System.Threading.Thread.Sleep(200);
			}



			if (mouse.LeftButton == ButtonState.Pressed)
			{
				Vector2 worldPos = Vector2.Transform(new Vector2(mouse.X, mouse.Y), Matrix.Invert(_camera.Transform));

				int idxX = (int)worldPos.X / _map.tileSize;
				int idxY = (int)worldPos.Y / _map.tileSize;

				if (idxX >= 0 && idxX < _map.tileMap.GetLength(0) && idxY >= 0 && idxY < _map.tileMap.GetLength(1))
				{
					_map.tileMap[idxX, idxY].Type = _selectedBrush;
				}
			}

		}
		protected void SimultaionUpdate(GameTime gameTime)
		{
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
		}

		protected void LoadUpdate(GameTime gametime)
		{
			var kState = Keyboard.GetState();

			// Fájllista beolvasása, ha még üres (vagy frissíteni akarod)
			if (_mapFiles.Count == 0 && Directory.Exists(mapsFolder))
			{
				_mapFiles = Directory.GetFiles(mapsFolder, "*.csv")
									 .Select(Path.GetFileName)
									 .ToList();
			}

			if (_mapFiles.Count > 0)
			{
				// Navigáció FEL
				if (kState.IsKeyDown(Keys.Up))
				{
					_selectedMapIndex--;
					if (_selectedMapIndex < 0) _selectedMapIndex = _mapFiles.Count - 1;
					Thread.Sleep(150); // Megállítjuk egy pillanatra, hogy ne pörögjön túl
				}

				// Navigáció LE
				if (kState.IsKeyDown(Keys.Down))
				{
					_selectedMapIndex++;
					if (_selectedMapIndex >= _mapFiles.Count) _selectedMapIndex = 0;
					Thread.Sleep(150);
				}

				if (kState.IsKeyDown(Keys.Back))
				{
					string fileToDelete = Path.Combine(mapsFolder, _mapFiles[_selectedMapIndex]);

					if (File.Exists(fileToDelete))
					{
						try
						{
							File.Delete(fileToDelete);

							// Frissítjük a belső listát, hogy eltűnjön a kijelzőről
							_mapFiles.RemoveAt(_selectedMapIndex);

							// Index korrigálása, hogy ne mutassunk túl a listán
							if (_selectedMapIndex >= _mapFiles.Count)
							{
								_selectedMapIndex = Math.Max(0, _mapFiles.Count - 1);
							}

							System.Diagnostics.Debug.WriteLine($"Torolve: {fileToDelete}");
							Thread.Sleep(250); // Hosszabb sleep, hogy ne töröljünk véletlenül többet
						}
						catch (Exception ex)
						{
							System.Diagnostics.Debug.WriteLine($"Hiba a torlesnel: {ex.Message}");
						}
					}
				}

				// Kiválasztás ENTER-rel
				if (kState.IsKeyDown(Keys.Enter))
				{
					string selectedPath = Path.Combine(mapsFolder, _mapFiles[_selectedMapIndex]);

					// Tényleges betöltés
					_map.LoadFromFile(selectedPath);

					// Fontos: Referenciák frissítése a betöltött adatokkal
					_people.Clear(); // Betöltésnél általában tiszta lapot akarunk az embereknek
					_movementManager.UpdateReferences(_map, _people);
					_movementManager.RefreshEnvironment();

					// Visszalépés az Editorba a betöltött pályával
					_currentState = GameState.Editor;
					_mapFiles.Clear(); // Kiürítjük a listát a következő megnyitásig
					Thread.Sleep(200);
				}
			}
		}


		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.CornflowerBlue);

			switch (_currentState)
			{
				case GameState.Menu:MenuDraw(gameTime); break;
				case GameState.Editor:EditorDraw(gameTime); break;
				case GameState.Simulation:SimultaionDraw(gameTime); break;
				case GameState.Load:LoadDraw(gameTime); break;
			}


			
			base.Draw(gameTime);
		}
		protected void MenuDraw(GameTime gameTime)
		{
			_spriteBatch.Begin();

			_spriteBatch.DrawString(font, "MENU", new Vector2(100, 100), Color.White);
			_spriteBatch.DrawString(font, "[S] Szimulacio  [E] Editor", new Vector2(100, 150), Color.White);

			_spriteBatch.End();
		}
		protected void EditorDraw(GameTime gameTime)
		{
			_spriteBatch.Begin(transformMatrix: _camera.Transform);

			_map.Draw(_spriteBatch);

			// Kiszámoljuk hol van az egér a rácson (hogy rajzolhassunk egy kijelölőt)
			var mouse = Mouse.GetState();
			Vector2 worldPos = Vector2.Transform(new Vector2(mouse.X, mouse.Y), Matrix.Invert(_camera.Transform));

			// Cellára pattintott koordináták (Snap to grid)
			int gx = (int)worldPos.X / _map.tileSize * _map.tileSize;
			int gy = (int)worldPos.Y / _map.tileSize * _map.tileSize;

			// Ha a pálya határain belül vagyunk, rajzolunk egy áttetsző keretet
			if (gx >= 0 && gx < _map.tileMap.GetLength(0) * _map.tileSize &&
				gy >= 0 && gy < _map.tileMap.GetLength(1) * _map.tileSize)
			{
				// Használd a korábban létrehozott 'pixel' textúrádat egy kis átlátszósággal
				_spriteBatch.Draw(pixel, new Rectangle(gx, gy, _map.tileSize, _map.tileSize), Color.White * 0.4f);
			}

			_spriteBatch.End();

			// 2. UI RAJZOLÁSA (Kamera nélkül, fixen a képernyő sarkába)
			_spriteBatch.Begin();

			string brushInfo = $"Ecset: {_selectedBrush} (1: Fal, 2: Ures, 3: Szek, 4: Kijarat)";
			_spriteBatch.DrawString(font, "EDITOR MOD", new Vector2(20, 20), Color.Gold);
			_spriteBatch.DrawString(font, brushInfo, new Vector2(20, 45), Color.White);
			_spriteBatch.DrawString(font, "Mozgas: Nyilak | Kilepes: Esc", new Vector2(20, 70), Color.LightGray);

			_spriteBatch.End();
		}
		protected void SimultaionDraw(GameTime gameTime)
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
		protected void LoadDraw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.DarkSlateBlue);
			_spriteBatch.Begin();

			_spriteBatch.DrawString(font, "VALASSZ PALYAT (Enter: OK, Esc: Megse)", new Vector2(100, 50), Color.Gold);

			for (int i = 0; i < _mapFiles.Count; i++)
			{
				// Kiemeljük a kiválasztottat
				Color textColor = (i == _selectedMapIndex) ? Color.Lime : Color.White;
				string pointer = (i == _selectedMapIndex) ? ">> " : "   ";

				_spriteBatch.DrawString(font, pointer + _mapFiles[i], new Vector2(120, 100 + (i * 30)), textColor);
			}

			if (_mapFiles.Count == 0)
			{
				_spriteBatch.DrawString(font, "Nincs mentett palya a 'maps' mappaban!", new Vector2(120, 100), Color.Red);
			}

			_spriteBatch.End();
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
		private void InitializeLevel(int width, int height)
		{
			_map = new TileMap(GraphicsDevice, width, height);
			_people.Clear();
			_exitedPeople.Clear();
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
