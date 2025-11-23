using EvakuacioSzimulacio.Core.Pathfinding;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
//using System.Numerics;

namespace EvakuacioSzimulacio.Core.Simulation
{
	internal class MovementManager
	{
		AStar AStarPathfinding;
		CollisionManager collisionManager;
		SpatialGrid spatialGrid;
		List<Person> People;
		Tile[,] Tiles;
		TileMap tileMap;
		double maxDistance;
		Vector2 acceleration = new Vector2(3, 3);
		public Vector2 target = Vector2.Zero;
		public List<RVO> personRVO;
		public List<ORCAConstraint> personORCAConstraints;
		Random rnd = new Random();



		public List<Debug> debuglist;
		Vector2 debugpos;
		float debugradius;
		Vector2 debugveloc;
		Vector2 debugdesired;
		Vector2 debugtarget;
		Vector2 debugleftaxis;
		Vector2 debugrightaxis;
		Vector2 debugtaucirclemiddle;
		float debugtaucircleradius;
		Vector2 debugcirclemiddle;
		float debugcircleradius;
		Vector2 debugprojectedpoint;
		Vector2 debugorcapoint;
		Vector2 debugorcadirection;
		Vector2 debugnewveloc;

		public MovementManager(List<Person> people, TileMap tileMap)
		{
			People = people;
			int width = tileMap.tileSize;
			spatialGrid = new SpatialGrid(32, 10000, 10000);
			maxDistance = Math.Sqrt(spatialGrid.CellSize * spatialGrid.CellSize + spatialGrid.CellSize * spatialGrid.CellSize) * 2;
			Tiles = tileMap.tileMap;
			spatialGrid.ClearTiles();
			foreach (var t in Tiles)
			{
				spatialGrid.AddTiles(t);
			}
			collisionManager = new CollisionManager();
			AStarPathfinding = new AStar();
			this.tileMap = tileMap;
			personRVO = new List<RVO>();
			personORCAConstraints = new List<ORCAConstraint>();
			debuglist = new List<Debug>();
		}
		public void WhereToMove(GameTime gameTime)
		{
			float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
			debuglist.Clear();
			spatialGrid.Clear();
			foreach (var p in People)
			{
				spatialGrid.AddPerson(p);
			}
			foreach (Person p in People)
			{
				List<Person> nearbyPeople = spatialGrid.GetNearbyPeople(p);
				List<Tile> nearbyTiles = spatialGrid.GetNearbyTiles(p);
				if (float.IsNaN(p.Direction.X))
				{
					p.Direction = new Vector2(0, p.Direction.Y);
				}
				if (float.IsNaN(p.Direction.Y))
				{
					p.Direction = new Vector2(p.Direction.X, 0);
				}

				if (nearbyTiles.Count > 0)
				{
					;//debug line
				}

				//---------------------------------------------------------------------Path kezdődik------------------------------------------------------------------
				//Először számolja ki a path-ot, azután lépjen
				if (p.Target != Vector2.Zero && p.Target != p.lastTarget)
				{

					var startCell = new Vector2((int)(p.Position.X / tileMap.tileSize), (int)(p.Position.Y / tileMap.tileSize));
					var targetCell = new Vector2((int)(p.Target.X / tileMap.tileSize), (int)(p.Target.Y / tileMap.tileSize));

					List<Node> path = AStarPathfinding.AStarPathFinding(tileMap, startCell, targetCell);
					if (path != null)
					{
						p.Path = new List<Node>(path);
						p.lastTarget = p.Target;
						debugtarget = p.Target;
					}


				}
				if (p.Path != null && p.Path.Count > 0)
				{
					p.DesiredVelocity = p.Position - new Vector2(p.Path[0].X * 32, p.Path[0].Y * 32);
					Vector2 nextCellCenter = new Vector2(p.Path[0].X * tileMap.tileSize + tileMap.tileSize / 2, p.Path[0].Y * tileMap.tileSize + tileMap.tileSize / 2);
					Vector2 pathDirection = nextCellCenter - p.Position;
					float pullStrength = 4f;

					if (pathDirection != Vector2.Zero)
					{
						pathDirection = Vector2.Normalize(pathDirection);
					}
					p.Direction += pathDirection * pullStrength;
					int xPos = (int)p.Position.X;
					int yPos = (int)p.Position.Y;
					int xTarget = (int)nextCellCenter.X;
					int yTarget = (int)nextCellCenter.Y;

					//Debug.WriteLine($"{xTarget / 32} == {xPos / 32} és {yTarget / 32} == {yPos / 32}");

					if (xTarget / tileMap.tileSize == xPos / tileMap.tileSize && yTarget / tileMap.tileSize == yPos / tileMap.tileSize)
					{
						p.Path.RemoveAt(0);
						//Debug.WriteLine("removed " + p.Path.Count);
					}
				}

				//------------------------------------------------------------------------Path vége-----------------------------------------------------------
				//-----------------------------------------------------------------------Desired velocity kiszámolása------------------------------------------
				Vector2 v_pref = Vector2.Zero;
				if (p.Path != null && p.Path.Count > 0)
				{
					Vector2 nextCellCenter = new Vector2(p.Path[0].X * tileMap.tileSize + tileMap.tileSize / 2, p.Path[0].Y * tileMap.tileSize + tileMap.tileSize / 2);
					Vector2 vectorToTarget = nextCellCenter - p.Position;
					float distanceToTarget = vectorToTarget.Length();

					// Feltételezzük, hogy a p.Speed a MaxSpeed
					float maxSpeed = p.Speed;

					if (distanceToTarget < 10)
					{
						// Target közel van: lassíts, hogy ne lőj túl rajta
						v_pref = vectorToTarget;
					}
					else if (distanceToTarget >= 10)
					{
						// Target messze van: menj max. sebességgel
						v_pref = Vector2.Normalize(vectorToTarget) * maxSpeed;
					}
					// Ha distanceToTarget == 0, v_pref marad Vector2.Zero
				}
				p.DesiredVelocity = v_pref;
				debugdesired = v_pref;
				debugradius = p.Hitbox.Radius;

				//----------------------------------------------------------------------Desired velocity kiszámolásának a vége------------------------------------------------




				//--------------------------------------------------------------------------------------ORCA implementáció------------------------------------------------------------------------------
				personRVO.Clear();
				personORCAConstraints.Clear();
				foreach (var nearppl in nearbyPeople)
				{
					float sumRadius = p.Hitbox.Radius + nearppl.Hitbox.Radius;

					Vector2 otherPosition = nearppl.Position;
					Vector2 betweenVectorTwoCenter = otherPosition - p.Position;					debugcirclemiddle = betweenVectorTwoCenter;debugcircleradius = sumRadius;
					float distanceBetweenTwoCenter = betweenVectorTwoCenter.Length();
					
					Vector2 betweenDirection = Vector2.Normalize(betweenVectorTwoCenter);
					Vector2 perpendicularVector = new Vector2(-betweenDirection.Y, betweenDirection.X);
					Vector2 sumVelocityVectorPerTwo = (p.Direction + nearppl.Direction) / 2;

					float theta = MathF.Asin(sumRadius / distanceBetweenTwoCenter);
					float cos = MathF.Cos(theta);
					float sin = MathF.Sin(theta);

					Vector2 left = betweenDirection * cos + perpendicularVector * sin;
					Vector2 right = betweenDirection * cos - perpendicularVector * sin;

					//left += sumVelocityVectorPerTwo;
					//right += sumVelocityVectorPerTwo;
					//Vector2 apex = sumVelocityVectorPerTwo;
					RVO rvoToAdd = new RVO(left, right, new Vector2(0, 0));
					personRVO.Add(rvoToAdd);

					debugleftaxis = left;
					debugrightaxis = right;

					Vector2 relativeVelocity = p.Direction - nearppl.Direction;
					float tau = 1.2f; //Mennyi időváltozás legyen az, ami alatt javítanak a helyzetükön
					Vector2 tauCircleMiddlePoint = betweenVectorTwoCenter / tau;
					debugtaucirclemiddle = tauCircleMiddlePoint;
					debugtaucircleradius = sumRadius / tau;

					Vector2 closestPointOnRVO = rvoToAdd.ClosestPointProjectionOnRVO(relativeVelocity, tauCircleMiddlePoint, sumRadius / tau);			debugprojectedpoint = closestPointOnRVO;

					//if (rvoToAdd.isinside)
					//{
					//	p.Direction = closestPointOnRVO;
					//}
					//else
					//{
					//	p.Direction = p.DesiredVelocity;
					//}
					//p.Direction = closestPointOnRVO;

					Vector2 HalfPanePoint = p.DesiredVelocity + (closestPointOnRVO * 2f);																/*debugprojectedpoint = HalfPanePoint;*/

					Vector2 N = closestPointOnRVO;
					if (rvoToAdd.directionIsInTheForbiddenWay)
					{
						N = -N;
						//closestPointOnRVO = closestPointOnRVO * -1;
					}
					Vector2 HalfPaneDirectionVector = new Vector2(-N.Y, N.X);
					HalfPaneDirectionVector = Vector2.Normalize(HalfPaneDirectionVector);
					ORCAConstraint orcaToAdd = new ORCAConstraint(HalfPanePoint, N, HalfPaneDirectionVector);							debugorcadirection = HalfPaneDirectionVector; debugorcapoint = HalfPanePoint;
					personORCAConstraints.Add(orcaToAdd);

				}
				
				bool anyViolated = true;
				int maxIteration = 20;
				int iteration = 0;
				Vector2 newVelocity = p.DesiredVelocity;
				while (anyViolated && iteration < maxIteration)
				{
					int orcabadamount = 0;
					foreach (var orca in personORCAConstraints)
					{
						if (orca.IsViolated(newVelocity))
						{
							orcabadamount++;
							newVelocity = orca.Project(newVelocity);
						}
					}
					if (orcabadamount == 0) anyViolated = false;
					iteration++;
				}
				if (!anyViolated)
				{
					p.Direction = newVelocity; debugveloc = newVelocity;

				}
				else if(anyViolated && iteration == maxIteration)
				{
					p.Direction = Vector2.Zero; debugveloc = newVelocity;
				}
				//p.Direction = newVelocity; debugnewveloc = newVelocity;

				//-------------------------------------------------------------------------------ORCA implementáció vége--------------------------------------------------------------------------------

				//-------------------------------------------------------------------------------Egyszerű ütközésgátló logika---------------------------------------------------------------------------
				foreach (var nearppl in nearbyPeople)
				{
					if (collisionManager.Intersects(p.Hitbox, nearppl.Hitbox))
					{
						Vector2 betweenVector = nearppl.Position - p.Position;
						float lenght = betweenVector.Length();
						float correctionLenght = p.Hitbox.Radius + nearppl.Hitbox.Radius - lenght;
						betweenVector = Vector2.Normalize(betweenVector);
						betweenVector *= correctionLenght;

						betweenVector = betweenVector / 2;
						p.Position -= betweenVector;
					}
				}
				//-------------------------------------------------------------------------------Ütközésgátló logika vége------------------------------------------------------------------------------

				//-------------------------------------------------------------------Régi implementáció, ha ütköznek akkor módosítanak------------------------------------------------------------
				//foreach(var nearppl in nearbyPeople)
				//{
				//var distance = Vector2.Distance(p.Position, nearppl.Position);
				////var direction = p.Position - nearppl.Position;
				////var force = (maxDistance - distance)/4000;
				//var force = 1f;

				//float avoidStrength = 0.05f;
				//if (collisionManager.Intersects(p.Hitbox, nearppl.Hitbox))
				//{
				//	//Debug.WriteLine("IGEN");

				//	Vector2 betweenVector = nearppl.Position - p.Position;
				//	Vector2 collisionSteeringVector = RotateVector(p.Direction, GetSignedAngle(p.Direction, betweenVector) * avoidStrength);

				//	p.Direction += collisionSteeringVector;
				//	p.Hitbox.Coloring = Color.Yellow;


				//	float distanceBetween = betweenVector.Length();
				//	Vector2 normalizedBetweenVector = Vector2.Normalize(betweenVector);
				//	if (betweenVector.Length() == 0)
				//	{
				//		;//debug line vector NaN NaN
				//	}
				//	float minDistance = p.Hitbox.Radius + nearppl.Hitbox.Radius;
				//	if (distanceBetween < minDistance)
				//	{
				//		float distCorrection = (minDistance - distanceBetween) / 2;
				//		p.Position -= normalizedBetweenVector * distCorrection;
				//		p.Hitbox.Center = p.Position;
				//	}


				//}
				//else
				//{
				//	//Debug.WriteLine("NEM");
				//	//p.Direction += (float)force * direction;

				//}
				//-------------------------------------------------------------------------------Régi implementáció vége----------------------------------------------------------------------------

				//-------------------------------------------------------------------------------Tile számolás-----------------------------------------------------------------------------
				foreach (var neartiles in nearbyTiles)
				{
					var distance = Vector2.Distance(p.Position, neartiles.Center);
					var direction = p.Position - neartiles.Center;
					var force = (maxDistance - distance) / 8000;
					if (collisionManager.Intersects(p.Hitbox, neartiles.Hitbox))
					{
						float xAxis = Math.Clamp(p.Position.X, neartiles.Hitbox.TopLeft.X, neartiles.Hitbox.TopLeft.X + neartiles.Hitbox.Width);
						float yAxis = Math.Clamp(p.Position.Y, neartiles.Hitbox.TopLeft.Y, neartiles.Hitbox.TopLeft.Y + neartiles.Hitbox.Height);
						Vector2 collisionPoint = new Vector2(xAxis, yAxis);



						Vector2 centerToCollisionPoint = collisionPoint - p.Position;
						float distanceToCollision = centerToCollisionPoint.Length();
						float overlap = p.Hitbox.Radius - distanceToCollision;

						if (overlap > 0)
						{
							Vector2 collisionNormal = Vector2.Normalize(centerToCollisionPoint);
							if (centerToCollisionPoint.Length() == 0)
							{
								;//debug line vector NaN NaN
							}

							p.Position -= collisionNormal * overlap;

							Vector2 v = p.Direction;
							Vector2 normal = collisionNormal;

							Vector2 tangent = v - Vector2.Dot(v, normal) * normal;

							p.Direction = tangent;
						}


					}



					//p.Direction += (float)force * direction;


				}


				//----------------------------------------------------------------------------------Tile számolás vége---------------------------------------------------------------------


				float length = p.Direction.Length();

				float currentSpeed = p.Direction.Length();
				float moveSpeed = Math.Max(length, p.Speed);
				float speedTolerance = 2f;


				if (p.Direction.Length() < p.Speed - speedTolerance)
				{
					float accel = (float)acceleration.Length() * dt;
					Vector2 dirNorm = Vector2.Zero;
					if (p.Direction != Vector2.Zero)
					{
						dirNorm = Vector2.Normalize(p.Direction);
						if (p.Direction.Length() == 0)
						{
							;//debug line vector NaN NaN
						}
					}
					p.Direction += dirNorm * accel;

					if (p.Direction.Length() > p.Speed)
						p.Direction = dirNorm * p.Speed;
				}
				if (p.Direction.Length() > p.Speed)
				{
					if (p.Direction != Vector2.Zero)
					{
						p.Direction = Vector2.Normalize(p.Direction) * p.Speed;
						if (p.Direction.Length() == 0)
						{
							;//debug line vector NaN NaN
						}
					}
				}
				if (p.Direction.Length() >= 0)
				{
					int tileX = (int)(p.Position.X / tileMap.tileSize);
					int tileY = (int)(p.Position.Y / tileMap.tileSize);

					p.CurrentTile = tileMap.tileMap[tileX, tileY];
					
				}



				float stationaryThreshold = 0.1f;
				Vector2 deltaPos = p.Position - p.LastPosition;

				if (deltaPos.LengthSquared() < 0.01f) // szinte mozdulatlan
				{
					p.StationaryTime += dt;
					if (p.StationaryTime > stationaryThreshold)
					{
						// Adj neki kis véletlenszerű lökést a DesiredVelocity irányához képest
						Vector2 pertubation = Vector2.Zero;
						if (p.DesiredVelocity.LengthSquared() > 0.0001f)
						{
							Vector2 dirNorm = Vector2.Normalize(p.DesiredVelocity);

							// Véletlenszög -30 és +30 fok között
							float maxAngleDeg = 90f;
							float angleRad = MathHelper.ToRadians((float)(rnd.NextDouble() * 2 * maxAngleDeg - maxAngleDeg));

							// 2D vektor elforgatása
							pertubation = new Vector2(
								dirNorm.X * (float)Math.Cos(angleRad) - dirNorm.Y * (float)Math.Sin(angleRad),
								dirNorm.X * (float)Math.Sin(angleRad) + dirNorm.Y * (float)Math.Cos(angleRad)
							);

							pertubation *= 1f; // erősség skálázása
						}

						p.Direction += pertubation;
						p.StationaryTime = 0;
					}
				}
				else
				{
					p.StationaryTime = 0;
				}

				p.LastPosition = p.Position;



				p.Position += p.Direction * dt / p.CurrentTile.MovementCost;




				//Debug.WriteLine("position :" + p.Position + " speed: " + p.Direction.Length() + " direction: " + p.Direction);
				p.Hitbox.Center = p.Position;
				debugpos = p.Position;



				


				//Ha rálép az exit tile-ra, akkor kiszedjük a listából, így a következő frameben már kirajzolni se fogjuk
				if (p.CurrentTile.Type == TileType.Exit)
				{
					p.IsExited = true;
				}


				debuglist.Add(new Debug(debugpos, debugradius, debugveloc, debugdesired, debugtarget, debugleftaxis, debugrightaxis, 
					debugtaucirclemiddle, debugtaucircleradius, debugcirclemiddle, debugcircleradius, debugprojectedpoint,debugorcapoint, debugorcadirection,debugnewveloc));


			}
		}
		float GetSignedAngle(Vector2 a, Vector2 b)
		{
			a = Vector2.Normalize(a);
			b = Vector2.Normalize(b);
			if (a.Length() == 0 || b.Length() == 0)
			{
				;//debug line vector NaN NaN
			}

			float angle = MathF.Atan2(b.Y, b.X) - MathF.Atan2(a.Y, a.X);

			if (angle > MathF.PI)
				angle -= MathF.Tau;
			else if (angle < -MathF.PI)
				angle += MathF.Tau;

			return angle;
		}
		Vector2 RotateVector(Vector2 v, float angleRadians)
		{
			float cos = MathF.Cos(angleRadians);
			float sin = MathF.Sin(angleRadians);

			return new Vector2(
				v.X * cos - v.Y * sin,
				v.X * sin + v.Y * cos
			);
		}
		private static Vector2 ProjectOntoRay(Vector2 v, Vector2 rayDir)
		{
			// 1. Skaláris vetítés kiszámítása
			// Képlet: (v · rayDir) / |rayDir|^2
			float scalar_proj = Vector2.Dot(v, rayDir) / rayDir.LengthSquared();

			// 2. Csúcs (origó) ellenőrzése
			if (scalar_proj <= 0)
			{
				// Ha a skaláris projektálás nulla vagy negatív, 
				// a vetület a sugár kezdőpontjához (origóhoz) van a legközelebb.
				return Vector2.Zero;
			}
			else
			{
				// 3. Vetület a sugáron
				return rayDir * scalar_proj;
			}
		}
		public static Vector2 ClosestPointOnLine(Vector2 P, Vector2 D, Vector2 point)
		{
			// Normalizáljuk az irányvektort
			Vector2 dirNormalized = Vector2.Normalize(D);

			// Vetítési paraméter t
			float t = Vector2.Dot(point - P, dirNormalized);

			// Vetített pont
			Vector2 closestPoint = P + dirNormalized * t;
			return closestPoint;
		}
	}
	public class RVO
	{
		public RVO(Vector2 leftaxis, Vector2 rightaxis, Vector2 apex)
		{
			this.leftaxis = leftaxis;
			this.rightaxis = rightaxis;
			this.apex = apex;
		}

		Vector2 leftaxis { get; set; }
		Vector2 rightaxis { get; set; }
		Vector2 apex { get; set; }
		public bool directionIsInTheForbiddenWay { get; set; }
		public bool isinside {  get; set; }
		public Vector2 ClosestPointProjectionOnRVO(Vector2 vector, Vector2 circleMiddlePoint, float circleRadius)
		{
			Vector2 result = Vector2.Zero;
			Vector2 leftProjected = Vector2.Zero;
			float leftDistance = 0f;
			Vector2 rightProjected = Vector2.Zero;
			float rightDistance = 0f;
			Vector2 circleProjected = Vector2.Zero;
			float circleDistance = 0f;

			Vector2 leftAxisNormalized = Vector2.Normalize(leftaxis);
			Vector2 rightAxisNormalized = Vector2.Normalize(rightaxis);
			float leftproj = Vector2.Dot(circleMiddlePoint,leftAxisNormalized);
			float rightproj = Vector2.Dot(circleMiddlePoint,rightAxisNormalized);
			float leftlengthsquared = leftaxis.LengthSquared();
			float rightlengthsquared = rightaxis.LengthSquared();
			leftProjected = leftproj * leftAxisNormalized;
			rightProjected = rightproj * rightAxisNormalized;


			//félegyenes kezdőpontja, iránya, vetíteni kívánt pont
			Vector2 leftstartPoint = leftProjected;
			Vector2 leftdirection = leftaxis;
			Vector2 rightstartPoint = rightProjected;
			Vector2 rightdirection = rightaxis;
			Vector2 Q = vector;

			// paraméter a vetítéshez
			float tleft = Vector2.Dot(Q - leftstartPoint, leftdirection) / leftdirection.LengthSquared();
			float tright = Vector2.Dot(Q - rightstartPoint, rightdirection) / rightdirection.LengthSquared();
			isinside = false;
			if(tleft < 0 && tright < 0) //nev vetíthető egyik félegyenesre sem, a körív részre kell vetíteni
			{
				Vector2 directionformcircle = Q - circleMiddlePoint;
				directionformcircle = Vector2.Normalize(directionformcircle) * circleRadius;
				result = circleMiddlePoint + directionformcircle;
				
			}
			else if(tleft >= 0 && tright < 0) //left félegyenesre vetíthető másikra nem, így left félegyenesre kell vetíteni
			{
				result = leftstartPoint + leftdirection * tleft;
			}
			else if(tleft < 0 && tright >= 0) //right félegyenesre vetíthető a másikra nem, így right félegyenesre kell vetíteni
			{
				result = rightstartPoint + rightdirection * tright;
			}
			else if(tleft >= 0 && tright >= 0) //left és right félegyenesre is vetíthető, így el kell dönteni, hogy melyik pont lesz a legközelebb a vetíteni kívánt ponthoz
			{
				isinside = true;
				float distleft = Vector2.Distance(leftstartPoint + leftdirection * tleft, Q);
				float distright = Vector2.Distance(rightstartPoint + rightdirection * tright, Q);
				if(distleft < distright)
				{
					result = leftstartPoint + leftdirection * tleft;
				}
				else
				{
					result = rightstartPoint + rightdirection * tright;
				}
			}
			else //valami hiba történt
			{
				//result = new Vector2(-100,-100);
			}


			result = result - vector;

			Vector2 leftNormal = new Vector2(-leftaxis.Y, leftaxis.X);
			Vector2 rightNormal = new Vector2(rightaxis.Y, -rightaxis.X);

			bool leftViolated = Vector2.Dot(vector - apex, leftNormal) < 0;
			bool rightViolated = Vector2.Dot(vector - apex, rightNormal) < 0;

			bool insideEdge = leftViolated && rightViolated;
			bool insideCircle = (vector - circleMiddlePoint).Length() < circleRadius;

			directionIsInTheForbiddenWay = insideEdge || insideCircle;
			if (insideEdge || insideCircle) isinside = true;

			return result;
		}
		
	}
	public class ORCAConstraint
	{
		// P: A referencia pont a fél-síkon (a v_pref + u/2 pont).
		public Vector2 P { get; private set; }

		// N: A normálvektor, amely a tiltott oldal felé mutat
		public Vector2 N { get; private set; }
		// P: irányvektor
		public Vector2 D { get; private set; }

		public ORCAConstraint(Vector2 referencePoint, Vector2 normalVector, Vector2 directionVector)
		{
			P = referencePoint;
			N = Vector2.Normalize(normalVector);
			D = Vector2.Normalize(directionVector);
		}

		
		/// Ellenőrzi, hogy a megadott sebesség (v) megsérti-e a megszorítást.
		public bool IsViolated(Vector2 v)
		{
			// Ha (v - P) · N < 0, akkor a sebesség a tiltott oldalon van.
			return Vector2.Dot(v - P, N) > 0;
		}
		public Vector2 Project(Vector2 v)
		{
			float dist = Vector2.Dot(v - P, N);
			return v - dist * N * 1.5f;
		}
	}
	
}
