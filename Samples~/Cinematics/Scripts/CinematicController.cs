using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using Utilities;

namespace Cinematics
{
    public class CinematicController : HiddenSingleton<CinematicController>
    {
        [SerializeField]
        private List<CinematicBase> cinematicPrefabs;

        private CinematicBase _activeCinematic;

        //============================================================================================================//
        // Start is called before the first frame update
        void Start()
        {
        
        }

        //============================================================================================================//

        private Coroutine TryPlayCinematic(string cinematicName)
        {
            if (_activeCinematic != null && _activeCinematic.IsPlaying)
                throw new Exception($"Trying to play <b>{cinematicName}</b> while <b>{_activeCinematic.name}</b> is still playing!");
            
            var cinematicPrefab = cinematicPrefabs.FirstOrDefault(x => x.cinematicName.Equals(cinematicName));
            
            Assert.IsNotNull(cinematicPrefab, $"No Cinematic found with name {cinematicName}");

            _activeCinematic = Instantiate(cinematicPrefab, transform);

            return _activeCinematic.StartCinematic();
        }
        
        //============================================================================================================//

        //TODO Maybe want to delay it?
        public static Coroutine PlayCinematic(string cinematicName)
        {
            return Instance.TryPlayCinematic(cinematicName);
        }

        //============================================================================================================//
    }
}
