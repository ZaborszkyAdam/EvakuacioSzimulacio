//using System.Numerics;
using Microsoft.Xna.Framework;
using System.Collections.Generic;


namespace EvakuacioSzimulacio.Core.Pathfinding
{
	internal class AStar
	{
		private int TileSize;
		public List<Node> AStarPathFinding(TileMap map, Vector2 start, Vector2 target)
		{
			int width = map.tileMap.GetLength(0);
			int height = map.tileMap.GetLength(1);
			TileSize = map.tileSize;
			Node[,] nodes = new Node[width, height];

			for (int i = 0; i < width; i++)
			{
				for(int j = 0; j < height; j++)
				{
					nodes[i, j] = new Node(i, j, map.tileMap[i,j].Type == TileType.Empty || map.tileMap[i,j].Type == TileType.Chair || map.tileMap[i,j].Type == TileType.Exit || map.tileMap[i,j].Type == TileType.Spawn);
				}
			}
			;

			int tilesize = map.tileSize;
			//if(start.X !< 0 || start.Y !< 0 || start.X !> width || start.Y !> height)
			//{

			//}
			//if (target.X! < 0 || target.Y! < 0 || target.X! > width || target.Y! > height)
			//{

			//}
			Node startNode = nodes[(int)start.X, (int)start.Y];
			Node targetNode = nodes[(int)target.X, (int)target.Y];
			

			List<Node> openList = new List<Node>();
			List<Node> closedList = new List<Node>();

			openList.Add(startNode);

			while(openList.Count > 0)
			{
				Node current = openList[0];
				foreach(Node n in openList)
				{
					if(n.SumDistance < current.SumDistance || (n.SumDistance == current.SumDistance && n.DistanceFromEnd < current.DistanceFromEnd))
					{
						current = n;
					}
				}

				openList.Remove(current);
				closedList.Add(current);

				if(current == targetNode)
				{
					List<Node> path = new List<Node>();
					Node temp = current;
					while(temp != null)
					{
						path.Add(temp);
						temp = temp.Parent;
					}
					path.Reverse();
					return path;
				}




				foreach(var neighbour in Neighbours(nodes, current, width, height))
				{
					if (!neighbour.Walkable || closedList.Contains(neighbour)) continue;

					float stepCost = Vector2.Distance(new Vector2(current.X, current.Y),new Vector2(neighbour.X, neighbour.Y)) * map.tileMap[neighbour.X, neighbour.Y].MovementCost;
					float commulativeLength = current.DistanceFromBeginning + stepCost;

					if(!openList.Contains(neighbour) || commulativeLength < neighbour.DistanceFromBeginning)
					{
						neighbour.DistanceFromBeginning = commulativeLength;
						neighbour.DistanceFromEnd = Vector2.Distance(new Vector2(neighbour.X, neighbour.Y), new Vector2(targetNode.X, targetNode.Y));
						neighbour.Parent = current;

						openList.Add(neighbour);
					}
				}

			}
			return null;





		}
		private List<Node> Neighbours(Node[,] nodes, Node node, int width, int height)
		{
			List<Node> neighbours = new List<Node>();
			int tilesize = TileSize;

			int[] idxX = { -1, 0, 1, 0 };
			int[] idxY = { 0, -1, 0, 1 };

			for(int i = 0; i < 4; i++)
			{
				int x = node.X + idxX[i];
				int y = node.Y + idxY[i];
				if(x >= 0 && y >= 0 && x < width &&  y < height)
				{
					neighbours.Add(nodes[x, y]);
				}
			}
			return neighbours;

		}
	}
	class Node
	{
		public int X;
		public int Y;
		public bool Walkable;
		public Node Parent;
		public float DistanceFromBeginning;
		public float DistanceFromEnd;
		public float SumDistance => DistanceFromBeginning + DistanceFromEnd;

        public Node(int x, int y, bool walkable)
        {
            X = x;
			Y = y;
			Walkable = walkable;
        }
    }
}
