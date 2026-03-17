using System.Collections;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Levels;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests.Levels
{
    public class LevelLoaderTests
    {
        private const int LEVEL_COUNT = 10;
        
        private LevelDataDefinition[] m_levels;
        private GameObject m_levelLoaderGameObject; 
        
        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            m_levels = new LevelDataDefinition[LEVEL_COUNT];
            for (var i = 0; i < LEVEL_COUNT; i++)
            {
                var name = $"LevelLoaderTest_Definition[{i}]";
                var tempObject = new GameObject(name);
                m_levels[i] = tempObject.AddComponent<LevelDataDefinition>();
                m_levels[i].levelName = name;
                
                Object.DontDestroyOnLoad(tempObject);
            }

            
            var field = typeof(LevelLoader).GetField(
                "levels",
                BindingFlags.NonPublic | BindingFlags.Instance
            );
            
            m_levelLoaderGameObject = new GameObject("TestLevelLoader");
            var levelLoader = m_levelLoaderGameObject.AddComponent<LevelLoader>();
            
            field.SetValue(levelLoader, m_levels);
        }

        [SetUp]
        public void TestSetup()
        {
            LevelLoader.LoadFirstLevel();
        }
        
        [UnityTest]
        public IEnumerator DuplicateLevelLoaderTest()
        {
            LogAssert.Expect(LogType.Error, new Regex(".*Attempted to create Multiple instances of*"));
            
           var tempLevelLoaderGameObject = new GameObject("OtherLoader", typeof(LevelLoader));

            yield return null;
            
            Assert.NotNull(LevelLoader.Levels);
            
            if(tempLevelLoaderGameObject)
                Object.Destroy(tempLevelLoaderGameObject);
        }
        
        [Test]
        public void LevelCountTest()
        {
            Assert.AreEqual(LEVEL_COUNT, LevelLoader.Levels.Count);
        }

        [Test]
        public void LoadFirstLevelTest()
        {
            Assert.AreEqual(m_levels[0].name + "(Clone)", LevelLoader.CurrentLevelDataDefinition.name);
        }
        
        [UnityTest]
        public IEnumerator LoadAllLevelsTest()
        {
            for (int i = 0; i < LEVEL_COUNT; i++)
            {
                Assert.AreEqual(m_levels[i].name + "(Clone)", LevelLoader.CurrentLevelDataDefinition.name);
                LevelLoader.LoadNextLevel();
                yield return null;
            }
        }

        [Test]
        public void IsLastLevelTest()
        {
            LevelLoader.LoadLevelAtIndex(LEVEL_COUNT - 1);
            Assert.IsTrue(LevelLoader.OnLastLevel());
        }

        [UnityTest]
        public IEnumerator RestartLevelGameObjectTest([Values(0, 4, 7, 9)] int index)
        {
            var currentGameObject = LevelLoader.CurrentLevelDataDefinition.gameObject;
            
            LevelLoader.LoadLevelAtIndex(index);
            
            yield return null;
            
            LevelLoader.Restart();

            yield return null;
            
            var newGameObject = LevelLoader.CurrentLevelDataDefinition.gameObject;
            
            //We want different Objects
            Assert.AreNotEqual(currentGameObject, newGameObject);
        }
        
        [UnityTest]
        public IEnumerator RestartLevelNameTest([Values(2, 5, 6, 8)] int index)
        {
            var targetLevelName = m_levels[index].levelName;
            
            LevelLoader.LoadLevelAtIndex(index);
            
            yield return null;
            
            LevelLoader.Restart();

            yield return null;
            
            var newLevelName = LevelLoader.CurrentLevelDataDefinition.levelName;
            
            Assert.AreEqual(targetLevelName, newLevelName);
        }

        [UnityTest]
        public IEnumerator DuplicateLevelTest([Values(0, 4, 7, 9)] int index)
        {
            var levelName = m_levels[index].levelName;
            
            LevelLoader.LoadLevelAtIndex(index);
            
            yield return null;
            
            LevelLoader.Restart();

            yield return null;

            var count = Object
                .FindObjectsByType<LevelDataDefinition>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Count(x => x.levelName == levelName);
            
            //We're also counting the prefab that was created at the setup time
            Assert.AreEqual(2, count);
        }
        
        [OneTimeTearDown]
        public void TearDown()
        {
            if (m_levels != null && m_levels.Length > 0)
            {
                for (int i = 0; i < LEVEL_COUNT; i++)
                {
                    if (m_levels[i] == null)
                        continue;
                    
                    Object.Destroy(m_levels[i].gameObject);
                }
            }

            if (m_levelLoaderGameObject != null)
                Object.Destroy(m_levelLoaderGameObject);
        }
    }
}