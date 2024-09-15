using UnityEngine;

namespace NoxCore.GameModes
{
	public class RandomRaceMode : RaceMode 
	{
		public int m_AmountOfPointOnCircleBoundry;
		public int m_AmountOfRaceGatesPerBoundry;
		public int m_AmountOfAsteroids;
		public int m_Radius;
		public GameObject m_NavPointPrefab;
		public GameObject[] m_AsteroidPrefabs;
		public Transform m_NavPointParent;
		public Transform m_AsteroidParent;
		public Vector3[] m_Boundaries;
		public float m_SpaceBetweenObjects;

		protected override bool ReadyToStartMatch()
		{
			if (Application.isEditor == true)
			{
				return true;
			}
			else if (numAIs + numPlayers > 0)
			{
				return true;
			}			
			
			return false;
		}
		
		protected override bool ReadyToEndMatch()
		{
			return base.ReadyToEndMatch();
		}		

        public override void init()
		{
			m_Boundaries = new Vector3[m_AmountOfPointOnCircleBoundry];

			for(int i = 0; i < m_AmountOfPointOnCircleBoundry; i++)
            {
				float ang = (360 / m_AmountOfPointOnCircleBoundry) * i;
				Vector2 pos = new Vector2();
				pos.x = (m_Radius * Mathf.Sin(ang * Mathf.Deg2Rad));
				pos.y = m_Radius * Mathf.Cos(ang * Mathf.Deg2Rad);
				pos *= Random.Range(0.1f, 1.9f);
				m_Boundaries[i] = pos;
				new GameObject("Circle Point" + i).transform.position = pos;
			}

			for (int i = 0; i < m_AmountOfRaceGatesPerBoundry * m_AmountOfPointOnCircleBoundry; i++)
			{
				GameObject go = Instantiate(m_NavPointPrefab, m_NavPointParent);

				if(i < 9)
					go.name = "NavPoint0" + (i + 1);
				else
					go.name = "NavPoint" + (i + 1);
			}

			for (int i = 0; i < m_AmountOfAsteroids; i++)
            {
				GameObject go = Instantiate(m_AsteroidPrefabs[Random.Range(0, m_AsteroidPrefabs.Length)], m_AsteroidParent);
			}

			base.init();

			raceGatesAndIDs[0].navPoint.transform.position = m_Boundaries[0];
			float pointsBetweenBoundry = (float)m_Boundaries.Length / raceGatesAndIDs.Count;
			for (int i = 1; i < raceGatesAndIDs.Count; i++)
            {
				int currentBoundry = Mathf.FloorToInt(i * pointsBetweenBoundry);
				if (currentBoundry >= m_Boundaries.Length) currentBoundry = 0;
				int nextBoundry = currentBoundry + 1;
				if (nextBoundry >= m_Boundaries.Length) nextBoundry = 0;

				float distanceBetweenBoundries = (m_Boundaries[nextBoundry] - m_Boundaries[currentBoundry]).magnitude;


				Vector3 direction = m_Boundaries[nextBoundry] - raceGatesAndIDs[i - 1].navPoint.transform.position;
				direction.Normalize();
				Vector3 newDirection = Quaternion.AngleAxis(Random.Range(-45.0f, 45.0f), Vector3.forward) * direction;
				Vector3 startPos = raceGatesAndIDs[i - 1].navPoint.transform.position;
				raceGatesAndIDs[i].navPoint.transform.position = startPos + (newDirection * (distanceBetweenBoundries / (raceGatesAndIDs.Count / m_Boundaries.Length)));
			}

			for (int i = 0; i < raceGatesAndIDs.Count - 1; i++)
			{
				raceGatesAndIDs[i].navPoint.transform.up = raceGatesAndIDs[i + 1].navPoint.transform.position - raceGatesAndIDs[i].navPoint.transform.position;
			}
			raceGatesAndIDs[raceGatesAndIDs.Count - 1].navPoint.transform.up = raceGatesAndIDs[0].navPoint.transform.position - raceGatesAndIDs[raceGatesAndIDs.Count - 1].navPoint.transform.position;


			GameObject[] asteroids = GameObject.FindGameObjectsWithTag("Asteroid");

			for (int i = 0; i < asteroids.Length - 1; i++)
			{
				int rnd = Random.Range(i, asteroids.Length);
				GameObject tempGO = asteroids[rnd];
				asteroids[rnd] = asteroids[i];
				asteroids[i] = tempGO;
			}

			pointsBetweenBoundry = (float)m_Boundaries.Length / asteroids.Length;
			
			for (int i = 0; i < asteroids.Length; i++)
			{
				int currentBoundry = Mathf.FloorToInt(i * pointsBetweenBoundry);
				if (currentBoundry >= m_Boundaries.Length) currentBoundry = 0;
				int nextBoundry = currentBoundry + 1;
				if (nextBoundry >= m_Boundaries.Length) nextBoundry = 0;

				float distanceBetweenBoundries = (m_Boundaries[nextBoundry] - m_Boundaries[currentBoundry]).magnitude;

				Vector2 pointOnLine = Vector3.Lerp(m_Boundaries[currentBoundry], m_Boundaries[nextBoundry], (i % (asteroids.Length / m_Boundaries.Length))  * pointsBetweenBoundry);
				pointOnLine += (Random.insideUnitCircle.normalized * Random.Range(0, 1000));
				asteroids[i].transform.position = pointOnLine;
			}

			for (int i = 0; i < asteroids.Length - 1; i++)
            {
				if (asteroids[i] != null)
				{
					for (int j = i+1; j < asteroids.Length; j++)
					{
						if (Vector2.Distance(asteroids[i].transform.position, asteroids[j].transform.position) < asteroids[i].transform.localScale.magnitude * m_SpaceBetweenObjects)
						{
							Destroy(asteroids[i]);
						}
					}
				}
			}

			for (int i = 0; i < asteroids.Length; i++)
			{
				if (asteroids[i] != null)
				{
					for (int j = 0; j < raceGatesAndIDs.Count; j++)
					{
						//Debug.Log((Vector2.Distance(asteroids[i].transform.position, raceGatesAndIDs[j].navPoint.transform.position);
						if (Vector2.Distance(asteroids[i].transform.position, raceGatesAndIDs[j].navPoint.transform.position) < asteroids[i].transform.localScale.magnitude * m_SpaceBetweenObjects)
                        {
							Destroy(asteroids[i]);
						}
					}
				}
			}
		}
	}
}