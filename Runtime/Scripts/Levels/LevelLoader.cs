using UnityEngine;
using UnityEngine.Assertions;
using Utilities;

namespace Levels
{
    public class LevelLoader : HiddenSingleton<LevelLoader>
    {
        public static LevelDataDefinition CurrentLevelDataDefinition { get; private set; }
        private int _currentLevelIndex = -1;
        private GameObject _currentLevelGameObject;

        [SerializeField]
        private LevelDataDefinition[] levels;

        private void LoadLevel(int indexToLoad)
        {
            var count = levels.Length;
            Assert.IsTrue(indexToLoad >= 0);
            Assert.IsTrue(indexToLoad < count);

            TryCleanCurrentLevel();

            var levelInstance = Instantiate(levels[indexToLoad], transform);

            _currentLevelGameObject = levelInstance.gameObject;
            _currentLevelIndex = indexToLoad;
            CurrentLevelDataDefinition = levelInstance;
        }

        private bool TryLoadNextLevel()
        {
            if (_currentLevelIndex + 1 >= levels.Length)
                return false;

            LoadLevel(_currentLevelIndex + 1);
            return true;
        }

        private void RestartLevel()
        {
            TryCleanCurrentLevel();

            LoadLevel(_currentLevelIndex);
        }
        //============================================================================================================//

        private void TryCleanCurrentLevel()
        {
            if (_currentLevelGameObject == null)
                return;
            
            Destroy(_currentLevelGameObject);
        }
        //============================================================================================================//
        public static bool OnLastLevel()
        {
            return Instance._currentLevelIndex == Instance.levels.Length - 1;
        }

        public static bool LoadNextLevel() => Instance.TryLoadNextLevel();
        
        public static void Restart() => Instance.RestartLevel();

        public static void LoadFirstLevel() => Instance.LoadLevel(0);
        
        //============================================================================================================//
        
    }
}