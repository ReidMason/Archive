using UnityEngine;

using NoxCore.Effects;
using NoxCore.Placeables;
using NoxCore.Rules;

public class CircularArena : MonoBehaviour
{
	public static float radius = 0.0f;

	public int roundDuration = 600;
	public float startRadius = 2000;
	public float endRadius = 2000;
	public Vector2 centre;
	public int resizeDelay = 0;
	public int resizeDuration = 0;
	public bool bounce = false;
	public int numSatellites = 32;
	public GameObject barrierSatellite;

	protected float angle;
	protected GameObject[] barrierSatellites;
	protected Vector2[] satPoint;

	protected CircleCollider2D arenaTrigger;

	public void init()
	{
		GameObject go = Instantiate(Resources.Load<GameObject>("Placeables/Empty"));

		if (go != null)
		{
			arenaTrigger = gameObject.AddComponent<CircleCollider2D>();
			gameObject.layer = LayerMask.NameToLayer("Arena");

			GameObject hierarchy = GameObject.Find("Placeables");

			go.name = "Arena Barrier";

			D.log("Content", "Building " + go.name);

			go.transform.position = centre;

			createBarrierSatellites();

			go.transform.parent = hierarchy.transform;

			ArenaRules rules = go.AddComponent<ArenaRules>();
			rules.Init(roundDuration, startRadius, endRadius, resizeDelay, resizeDuration, bounce);

			D.log("Content", "Finished building " + go.name);
		}
	}

	protected void createBarrierSatellites()
    {
		D.log("Initialising arena barrier satellites");

		GameObject barrierParent = GameObject.Find("Arena Barrier");

		if (barrierParent == null)
		{
			barrierParent = new GameObject();
			barrierParent.name = "Arena Barrier";

			GameObject placeables = GameObject.Find("Placeables");

			if (placeables == true)
			{
				barrierParent.transform.SetParent(placeables.transform);
			}
		}

		if (barrierParent != null && barrierSatellite != null)
		{
			arenaTrigger.isTrigger = true;
			arenaTrigger.radius = radius;

			centre = Vector2.zero;

			satPoint = new Vector2[numSatellites];
			barrierSatellites = new GameObject[numSatellites];

			angle = (Mathf.PI * 2.0f) / numSatellites;

			GameObject prev = null;
			GameObject first = null;
			GameObject cur = null;

			for (int i = 0; i < numSatellites; i++)
			{
				satPoint[i] = centre + new Vector2(radius * Mathf.Sin(i * angle), radius * Mathf.Cos(i * angle));
				cur = Instantiate(barrierSatellite, satPoint[i], Quaternion.identity) as GameObject;
				cur.name = "Arena Satellite " + i;
				cur.transform.parent = barrierParent.transform;

				barrierSatellites[i] = cur;

				if (prev != null)
				{
					cur.transform.Find("Beam Laser Effect").GetComponent<BarrierBeamEffect>().setTarget(prev.transform);
				}
				else
				{
					first = cur;
				}

				prev = cur;
			}

			first.transform.Find("Beam Laser Effect").GetComponent<BarrierBeamEffect>().setTarget(prev.transform);
		}
	}

	public float getRadius()
	{
		return radius;
	}

	public float getArea()
	{
		return Mathf.PI * Mathf.Pow(ArenaBarrier.radius, 2);
	}

	public float getCircumference()
	{
		return 2 * Mathf.PI * radius;
	}

	public void setInitialRadius(float radius)
	{
		ArenaBarrier.radius = radius;
	}

	public void setRadius(float newRadius)
	{
		if (ArenaBarrier.radius != newRadius)
		{
			ArenaBarrier.radius = newRadius;
			arenaTrigger.radius = newRadius;

			int i = 0;

			foreach (GameObject sat in barrierSatellites)
			{
				sat.transform.position = new Vector2(ArenaBarrier.radius * Mathf.Sin(i * angle), ArenaBarrier.radius * Mathf.Cos(i * angle));
				i++;
			}
		}
	}

	void OnTriggerEnter2D(Collider2D other)
	{
		Structure structure = other.GetComponent<Structure>();

		if (structure != null)
		{
			if (structure.Controller != null && structure.Controller.invulnerableToArenaBoundary == false)
			{
				structure.Call_InBounds(this, new BoundaryEventArgs(structure));
			}
		}
	}

	void OnTriggerExit2D(Collider2D other)
	{
		Structure structure = other.GetComponent<Structure>();

		if (structure != null)
		{
			if (structure.Controller != null && structure.Controller.invulnerableToArenaBoundary == false)
			{
				structure.Call_OutOfBounds(this, new BoundaryEventArgs(structure));
			}
		}
	}
}
