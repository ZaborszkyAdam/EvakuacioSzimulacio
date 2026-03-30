using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvakuacioSzimulacio.Core.Simulation
{
	internal class SimulationLogger
	{
		private string filePath;
		private MemoryStream backBuffer;
		private MemoryStream frontBuffer;
		private BinaryWriter writer;
		private string folderPath = "saves";

		private Task savingTask = Task.CompletedTask;
		private readonly object lockObject = new object();

		private bool isHeaderWritten = false;

		public SimulationLogger(string fileName)
		{
			folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "saves");

			if (!Directory.Exists(folderPath))
			{
				Directory.CreateDirectory(folderPath);
			}

			filePath = Path.Combine(folderPath, fileName);

			frontBuffer = new MemoryStream(5 * 1024 * 1024);
			backBuffer = new MemoryStream(5 * 1024 * 1024);

			writer = new BinaryWriter(frontBuffer);
		}

		public void WriteHeader(int mapWidth, int mapHeight, int tileSize, string mapFileName)
		{
			if (isHeaderWritten) return;

			using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
			using (BinaryWriter headerWriter = new BinaryWriter(fs))
			{
				headerWriter.Write(0x53494D1);
				headerWriter.Write(1);
				headerWriter.Write(mapWidth);
				headerWriter.Write(mapHeight);
				headerWriter.Write(tileSize);
				headerWriter.Write(mapFileName);
			}
			isHeaderWritten = true;
		}
		public void LogSnapshot(List<Person> people, float currentTime)
		{
			writer.Write(currentTime);
			writer.Write(people.Count);

			foreach (var p in people)
			{
				writer.Write(p.ID);
				writer.Write(p.Position.X);
				writer.Write(p.Position.Y);
			}

			if(frontBuffer.Position >4 * 1024 * 1024)
			{
				SwapBuffers();
			}
		}
		private void SwapBuffers()
		{
			savingTask.Wait();

			lock (lockObject)
			{
				MemoryStream temp = frontBuffer;
				frontBuffer = backBuffer;
				backBuffer = temp;

				frontBuffer.SetLength(0);
				writer = new BinaryWriter(frontBuffer);

				byte[] dataToSave = backBuffer.ToArray();
				savingTask = Task.Run(() => AppendToFile(dataToSave));
			}
		}
		private void AppendToFile(byte[] data)
		{
			try
			{
				using(FileStream fs = new FileStream(filePath, FileMode.Append, FileAccess.Write))
				{
					fs.Write(data, 0, data.Length);
				}
			}
			catch(Exception ex)
			{
				System.Diagnostics.Debug.WriteLine("Hiba mentéskor" + ex.ToString());
			}
		}

		public void Finish()
		{
			savingTask.Wait();

			if(frontBuffer.Length > 0)
			{
				AppendToFile(frontBuffer.ToArray());
			}
		}
	}
}
