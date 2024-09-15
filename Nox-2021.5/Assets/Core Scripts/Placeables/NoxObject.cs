using UnityEngine;

using NoxCore.Data;
using NoxCore.GUIs;
using NoxCore.Managers;
using NoxCore.Data.Placeables;

namespace NoxCore.Placeables
{
	public abstract class NoxObject : MonoBehaviour, INoxObject
	{
        protected NoxObjectData _noxObjectData;
        public NoxObjectData NoxObjectData { get { return _noxObjectData; } set { _noxObjectData = value; } }

        [Header("NoxObject", order = 0)]

        [Tooltip("ScriptableObject asset containing faction data")]
        [SerializeField] protected FactionData _faction;
        public FactionData Faction { get { return _faction; } set { _faction = value; } }

        [Header("Setup Overrides")]
        [Tooltip("Override name for the object in the scene")]
        [SerializeField] protected string _name;
        public string Name { get { return _name; } set { _name = value; } }

        [Header("NoxObject UI", order = 6)]

        protected GameObject _factionLabelGO;
		public GameObject FactionLabelGO { get { return _factionLabelGO; } set { _factionLabelGO = value; } }

        protected FactionLabel _factionLabel;
        public FactionLabel FactionLabel { get { return _factionLabel; } set { _factionLabel = value; } }

        protected GameObject _nameLabelGO;
        public GameObject NameLabelGO { get { return _nameLabelGO; } set { _nameLabelGO = value; } }

        protected NameLabel _nameLabel;
        public NameLabel NameLabel { get { return _nameLabel; } set { _nameLabel = value; } }

        protected bool _objectInitialised;
        public bool ObjectInitialised { get { return _objectInitialised; } private set { _objectInitialised = value; } }

        protected Quaternion factionLabelRotation, nameLabelRotation;

        protected bool eventsSubscribedInto;

        public virtual void init(NoxObjectData noxObjectData = null)
        {
            NoxObjectData = noxObjectData;

            if (Name == "")
            {
                Name = gameObject.name;
            }
            else
            {
                gameObject.name = Name;
            }

            Transform uiParent = transform.Find("UI");

            if (uiParent == null)
            {
                GameObject uiParentGO = new GameObject();
                uiParentGO.name = "UI";
                uiParentGO.transform.SetParent(transform);
                uiParent = uiParentGO.transform;
            }

            GameObject factionLabelPrefab = Resources.Load("UI/Faction Label") as GameObject;

            if (factionLabelPrefab != null)
            {
                factionLabelRotation = factionLabelPrefab.transform.rotation;
                FactionLabelGO = Instantiate(factionLabelPrefab, transform.position + factionLabelPrefab.GetComponent<FactionLabel>().labelOffset, factionLabelRotation) as GameObject;
                FactionLabelGO.transform.SetParent(uiParent);
                FactionLabel = FactionLabelGO.GetComponent<FactionLabel>();
            }
            else
            {
                D.warn("SYSTEM", "Missing prefab: UI/Faction Label");
            }

            GameObject nameLabelPrefab = Resources.Load("UI/Name Label") as GameObject;

            if (nameLabelPrefab != null)
            {
                nameLabelRotation = nameLabelPrefab.transform.rotation;
                NameLabelGO = Instantiate(nameLabelPrefab, transform.position + nameLabelPrefab.GetComponent<NameLabel>().labelOffset, nameLabelRotation) as GameObject;
                NameLabelGO.transform.SetParent(uiParent);
                NameLabel = NameLabelGO.GetComponent<NameLabel>();
            }
            else
            {
                D.warn("SYSTEM", "Missing prefab: UI/Name Label");
            }

            ObjectInitialised = true;
        }

        public virtual void OnEnable() { eventsSubscribedInto = true; }
        public virtual void OnDisable() { eventsSubscribedInto = false; }

        public virtual void spawn(bool spawnEnabled = false)
        {
            if (Faction != null)
            {
                FactionManager.Instance.addFaction(Faction);
            }
        }
	}
}