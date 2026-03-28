using System.Collections.Generic;
using Microsoft.VisualBasic;
using Microsoft.Xna.Framework;
//using System.Numerics;

namespace EvakuacioSzimulacio.Core.Simulation
{
	internal class SpatialGrid
	{
		//Mekkora a cellaméret
		//Mennyi cella vízszintesen
		//Mennyi cella függőlegesen
		public int CellSize;
		int WidthAmount;
		int HeightAmount;
		List<Person>[,] PeopleListArray;
		List<Tile>[,] TileListArray;

		public SpatialGrid(int cellSize, int width, int height)
		{
			CellSize = cellSize;
			WidthAmount = width/cellSize;
			HeightAmount = height/cellSize;

			PeopleListArray = new List<Person>[WidthAmount,HeightAmount];
			TileListArray = new List<Tile>[WidthAmount,HeightAmount];
			for(int i = 0; i < WidthAmount; i++)
			{
				for (int j = 0; j < HeightAmount; j++)
				{
					PeopleListArray[i, j] = new List<Person>();
					TileListArray[i,j] = new List<Tile>();
				}
			}
		}
		public void Clear()
		{
			for (int i = 0; i < WidthAmount; i++)
			{
				for (int j = 0; j < HeightAmount; j++)
				{
					PeopleListArray[i, j].Clear();
				}
			}
		}
		public void AddPerson(Person p)
		{
			var (cellX, cellY) = GetCellNumber(p.Position);
			if(IsInBounds(cellX, cellY))
			{
				PeopleListArray[cellX, cellY].Add(p);
			}
		}

		public List<Person> GetNearbyPeople(Person p)
		{
			List<Person> nearbyPeople = new List<Person>();
			var (cellX, cellY) = GetCellNumber(p.Position);

			for(int i = -2; i <= 2; i++)
			{
				for(int j = -2; j <= 2; j++)
				{
					int idxX = cellX + i;
					int idxY = cellY + j;
					if (IsInBounds(idxX,idxY))
					{
						
						nearbyPeople.AddRange(PeopleListArray[idxX,idxY]);
					}
				}
			}
			nearbyPeople.Remove(p);
			return nearbyPeople;
		}


		public (int,int) GetCellNumber(Vector2 position)
		{
			return((int)position.X/CellSize,((int)position.Y/CellSize));
		}
		public bool IsInBounds(int x, int y) //nem pozíció, hanem cella indexelő elemek
		{
			return(x>=0 && x < WidthAmount && y>=0 && y < HeightAmount);
		}


		//Minden Tile a Listába bekerül
		public void ClearTiles()
		{
			for (int i = 0; i < WidthAmount; i++)
			{
				for (int j = 0; j < HeightAmount; j++)
				{
					TileListArray[i, j].Clear();
				}
			}
		}
		public void AddTiles(Tile t)
		{
			var (cellX, cellY) = GetCellNumber(t.Center);
			if (IsInBounds(cellX, cellY))
			{
				TileListArray[cellX, cellY].Add(t);
			}
		}

		public List<Tile> GetNearbyTiles(Person p)
		{
			List<Tile> nearbyTiles = new List<Tile>();
			var (cellX, cellY) = GetCellNumber(p.Position);

			for (int i = -1; i <= 1; i++)
			{
				for (int j = -1; j <= 1; j++)
				{
					int idxX = cellX + i;
					int idxY = cellY + j;
					if (IsInBounds(idxX,idxY))
					{
						
						foreach(var t in TileListArray[idxX, idxY])
						{
							if(t.Type == TileType.Wall)
							{
								nearbyTiles.Add(t);
							}
						}
					}
				}
			}
			return nearbyTiles;
		}
	}
}
