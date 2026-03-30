using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvakuacioSzimulacio.Core.Statistics
{
	internal class HeatMap : IStatistics
	{
		private int[,] _grid;
		private int _mapW, _mapH, _tileSize;
		private int _maxHeat = 1;

		public void Initialize(int mapW, int mapH, int tileSize)
		{
			_mapW = mapW;
			_mapH = mapH;
			_tileSize = tileSize;
			_grid = new int[_mapW, _mapH];
		}

		public void ProcessSnapshot(float time, int agentCount)
		{

		}

		public void ProcessAgent(int id, float x, float y)
		{
			int gx = (int)(x / _tileSize);
			int gy = (int)(y / _tileSize);

			if (gx >= 0 && gx < _mapW && gy >= 0 && gy < _mapH)
			{
				_grid[gx, gy]++;

				if (_grid[gx, gy] > _maxHeat)
				{
					_maxHeat = _grid[gx, gy];
				}
			}
		}

		public void Reset()
		{
			if (_grid != null)
			{
				Array.Clear(_grid, 0, _grid.Length);
			}
			_maxHeat = 1;
		}

		public void Draw(SpriteBatch sb, Texture2D pixel)
		{
			if (_grid == null) return;

			for (int x = 0; x < _mapW; x++)
			{
				for (int y = 0; y < _mapH; y++)
				{
					int value = _grid[x, y];
					if (value > 0)
					{
						float intensity = (float)value / _maxHeat;

						float alpha = intensity * 0.9f;

						sb.Draw(pixel,
							   new Rectangle(x * _tileSize, y * _tileSize, _tileSize, _tileSize),
							   Color.Blue * alpha);
					}
				}
			}
		}

		private Color GetColorFromIntensity(float intensity)
		{
			return new Color(intensity, intensity * 0.2f, 1f - intensity);
		}
	}
}
