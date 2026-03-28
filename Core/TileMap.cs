using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Transactions;

namespace EvakuacioSzimulacio.Core
{
	public enum TileType
	{
		Empty, Wall, Exit, Chair, Spawn
	}
	internal class TileMap
	{
		public Tile[,] tileMap;
		private Texture2D tileTexture;
		public int tileSize = 32;
		private int mapSize = 15;
		private Random rnd = new Random();

		public TileMap(GraphicsDevice graphicsDevice, int width, int height)
		{

			tileTexture = new Texture2D(graphicsDevice, 1, 1);
			tileTexture.SetData(new[] {Color.White});

			CreateEmptyMap(width, height);
		}
		public void CreateEmptyMap(int width, int height)
		{
			tileMap = new Tile[width, height];
			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					Vector2 center = new Vector2(x * tileSize + tileSize / 2, y * tileSize + tileSize / 2);
					tileMap[x, y] = new Tile(center, tileSize, tileSize, TileType.Empty);
				}
			}
		}

		public void Draw(SpriteBatch spriteBatch)
		{
			for (int x = 0; x < tileMap.GetLength(0); x++)
			{
				for (int y = 0; y < tileMap.GetLength(1); y++)
				{
					Tile tile = tileMap[x, y];
					Color color = GetColorForTile(tile.Type);
					spriteBatch.Draw(tileTexture, tile.Hitbox.ToRectangle(), color);
				}
			}
		}
		private Color GetColorForTile(TileType type)
		{
			return type switch
			{
				TileType.Wall => Color.Black,
				TileType.Exit => Color.Green,
				TileType.Chair => Color.LightGray,
				TileType.Spawn => Color.Brown,
				_ => Color.Gray
			};
		}
		public void SaveToFile(string filename)
		{
			StringBuilder sb = new StringBuilder();
			int width = tileMap.GetLength(0);
			int height = tileMap.GetLength(1);

			// Első sorban vannak a méretek, a többi sorban pedig a data
			sb.AppendLine($"{width},{height}");

			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					sb.Append((int)tileMap[x, y].Type);
					if (x < width - 1) sb.Append(",");
				}
				sb.AppendLine();
			}
			File.WriteAllText(filename, sb.ToString());
		}
		public void LoadFromFile(string filename)
		{
			if (!File.Exists(filename)) return;

			string[] lines = File.ReadAllLines(filename);
			if (lines.Length < 2) return;

			string[] dims = lines[0].Split(',');
			int width = int.Parse(dims[0]);
			int height = int.Parse(dims[1]);

			tileMap = new Tile[width, height];

			for (int y = 0; y < height; y++)
			{
				string[] row = lines[y + 1].Split(',');
				for (int x = 0; x < width; x++)
				{
					if (x >= row.Length) continue;

					TileType type = (TileType)int.Parse(row[x]);
					Vector2 center = new Vector2(x * tileSize + tileSize / 2, y * tileSize + tileSize / 2);
					tileMap[x, y] = new Tile(center, tileSize, tileSize, type);
				}
			}
		}
		public List<Vector2> GetExitPoints()
		{
			List<Vector2> exits = new List<Vector2>();
			for (int x = 0; x < tileMap.GetLength(0); x++)
			{
				for (int y = 0; y < tileMap.GetLength(1); y++)
				{
					if (tileMap[x, y].Type == TileType.Exit)
					{
						exits.Add(tileMap[x, y].Center);
					}
				}
			}
			return exits;
		}

	}
	public class Tile
	{
		public Vector2 Center;
		public float Width;
		public float Height;
		public TileType Type;
		public RectangleHitbox Hitbox { get; private set; }
		public float MovementCost
		{
			get
			{
				switch (Type)
				{
					case TileType.Wall: return 999f;
					case TileType.Empty: return 1f;
					case TileType.Exit: return 1f;
					case TileType.Chair: return 5f;
					case TileType.Spawn: return 1f;
					default: return 1f;
				}
				
			}
		}


        public Tile(Vector2 center, float widht, float height, TileType type)
        {
            Center = center;
			Width = widht;
			Height = height;
			Type = type;
			Hitbox = new RectangleHitbox(center, widht,height);
        }

    }
    public class  RectangleHitbox
    {
		public Vector2 Center;
		public float Width;
		public float Height;

		public Vector2 TopLeft;
		public Vector2 BottomRight;

		public RectangleHitbox(Vector2 center, float width, float height)
		{
			Center = center;
			Width = width;
			Height = height;
			TopLeft = new Vector2(Center.X - width/2, Center.Y - height/2);
			BottomRight = new Vector2(Center.X + width/2, Center.Y + height/2);
		}
		public Rectangle ToRectangle()
		{
			return new Rectangle((int)Center.X - (int)Width/2,(int)Center.Y - (int)Height/2,(int)Width,(int)Height);
		}
	}
}
