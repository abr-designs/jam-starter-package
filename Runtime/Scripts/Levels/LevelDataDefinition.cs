using UnityEngine;

namespace Levels
{
    public class LevelDataDefinition : MonoBehaviour
    {
        public string levelName;
        [TextArea]
        public string levelDescription;
        
        [Min(0), Space(10f)]
        public int levelTime;
        [Min(0)]
        public int minScoreToPass;
    }
}