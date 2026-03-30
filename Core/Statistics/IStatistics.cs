using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvakuacioSzimulacio.Core.Statistics
{
	public interface IStatistics
	{
		void Initialize(int mapW, int mapH, int tileSize);

		void ProcessSnapshot(float time, int agentCount);

		void ProcessAgent(int id, float x, float y);

		void Draw(SpriteBatch sb, Texture2D pixel);

		void Reset();
	}
}
