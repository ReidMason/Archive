using System.Collections.Generic;
using System;

using UnityEngine;

using NoxCore.Buffs;
using NoxCore.Data;
using NoxCore.Placeables.Ships;

using Davin.Buffs;

namespace NoxCore.Placeables
{
    public class Nebula : NoxObject2D
    {
        
        [Header("Nebula Buff")]

        public BuffData __buffData;
        [NonSerialized]
        protected BuffData _buffData;
        public BuffData BuffData { get { return _buffData; } set { _buffData = value; } }
        
        // TODO - check if this is still required
        // note: we have to manage the triggering in case the same object hits the trigger more than once per tick due to multiple contact points hitting the trigger at the same time adding/removing the buff multiple times
        Dictionary<int, NebulaMaxSpeedBuff> shipsInsideBuffs = new Dictionary<int, NebulaMaxSpeedBuff>();
        
        void Start()
        {
            BuffData = Instantiate(__buffData);
        }
        
        void OnTriggerEnter2D(Collider2D collision)
        {
            //Debug.Log(collision.name + "has hit nebula");

            if (collision.tag == "Ship")
            {
                int id = collision.GetInstanceID();

                if (!shipsInsideBuffs.ContainsKey(id))
                {
                    Ship ship = collision.GetComponent<Structure>() as Ship;

                    NebulaMaxSpeedBuff nebulaMaxSpeedBuff = new NebulaMaxSpeedBuff(ship.engines, BuffType.PASSIVE, 1, 50, true, 0);

                    //NebulaMaxSpeedBuff nebulaMaxSpeedBuff = new NebulaMaxSpeedBuff(ship.engines, BuffData);

                    ship.BuffManager.addBuff(nebulaMaxSpeedBuff);

                    shipsInsideBuffs.Add(id, nebulaMaxSpeedBuff);
                }
            }
        }

        void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.tag == "Ship")
            {
                int id = collision.GetInstanceID();

                if (shipsInsideBuffs.ContainsKey(id))
                {
                    Ship ship = collision.GetComponent<Structure>() as Ship;

                    NebulaMaxSpeedBuff nebulaMaxSpeedBuff;

                    shipsInsideBuffs.TryGetValue(id, out nebulaMaxSpeedBuff);

                    ship.BuffManager.removeBuff(nebulaMaxSpeedBuff);

                    shipsInsideBuffs.Remove(id);
                }
                else
                {
                    Debug.Log("This should never happen", collision.gameObject);
                }
            }
        }
    }
}
