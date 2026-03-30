using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvakuacioSzimulacio.Core.Statistics
{
	internal class StatisticsManager
	{
		private List<IStatistics> _statistics = new List<IStatistics>();
		private int _mapW, _mapH, _tileSize;
		int count = 0;
		public StatisticsManager(int mapW, int mapH, int tileSize)
		{
			_mapW = mapW;
			_mapH = mapH;
			_tileSize = tileSize;
		}

		public void AddStatistic(IStatistics stat)
		{
			stat.Initialize(_mapW, _mapH, _tileSize);
			_statistics.Add(stat);
		}

		public string ProcessFile(string filePath)
		{
			if (!File.Exists(filePath)) return null;
			string mapName = "";

			foreach (var stat in _statistics) stat.Reset();

			using (BinaryReader reader = new BinaryReader(File.Open(filePath, FileMode.Open)))
			{
				try
				{
					int magic = reader.ReadInt32(); // Azonosító, hogy a saját fájlunkat olvassuk be
					if(magic != 0x53494D1)
					{
						System.Diagnostics.Debug.WriteLine("Nem a saját fájlunk, hibás azonosító!");
					}
					reader.ReadInt32(); // Version
					reader.ReadInt32(); // W
					reader.ReadInt32(); // H
					reader.ReadInt32(); // TileSize
					mapName = reader.ReadString();

					while (reader.BaseStream.Position < reader.BaseStream.Length)
					{
						float time = reader.ReadSingle();
						int agentCount = reader.ReadInt32();
						count += agentCount;
						foreach (var stat in _statistics)
							stat.ProcessSnapshot(time, agentCount);

						for (int i = 0; i < agentCount; i++)
						{
							int id = reader.ReadInt32();
							float x = reader.ReadSingle();
							float y = reader.ReadSingle();

							foreach (var stat in _statistics)
								stat.ProcessAgent(id, x, y);
						}
					}
				}
				catch (EndOfStreamException) {}
			}
			System.Diagnostics.Debug.WriteLine($"OSSZESEN BEOLVASOTT PONT: {count}");
			return mapName;
		}

		public void Draw(SpriteBatch sb, Texture2D pixel)
		{
			foreach (var stat in _statistics)
			{
				stat.Draw(sb, pixel);
			}
		}
	}
}
