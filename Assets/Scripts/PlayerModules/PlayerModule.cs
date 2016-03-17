using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Scripts.Networking;
using UnityEngine;

namespace Assets.Scripts.PlayerModules
{
    public class PlayerModule : MonoBehaviour
    {
        protected NetPlayer Owner;

        public virtual void Start()
        {
            Owner = GetComponentInParent<PlayerController>().NetPlayer;
        }

        public virtual void Update()
        {
            if(Owner.IsLocal)
                OnOwnerUpdate();

            if (NetworkMain.IsServer)
                OnServerUpdate();
            
            OnEveryoneUpdate();
        }

        // called only if this player is local
        public virtual void OnOwnerUpdate()
        {
            
        }

        // called only if this player is the server
        public virtual void OnServerUpdate()
        {
            
        }

        // called on everyones computer
        public virtual void OnEveryoneUpdate()
        {
            
        }
    }
}
