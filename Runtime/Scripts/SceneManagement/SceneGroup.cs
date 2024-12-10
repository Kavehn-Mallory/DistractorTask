using System;
using System.Linq;
using Eflatun.SceneReference;

namespace DistractorTask.SceneManagement
{
    [Serializable]
    public class SceneGroup
    {
        public string groupName = "New Scene Group";
        public SceneData[] scenes;

        public string FindSceneNameByType(SceneType sceneType)
        {
            return scenes.FirstOrDefault(scene => scene.sceneType == sceneType)?.reference.Name;
        }

        public bool ContainsScenesWithType(SceneType sceneType)
        {
            return scenes.FirstOrDefault(scene => scene.sceneType == sceneType) != null;
        }
    }

    
    
    public enum SceneType {ActiveScene, MainMenu, UserInterface, HUD, Cinematic, Environment, Tooling, UserStudy}
    [Serializable]
    public class SceneData
    {
        public SceneReference reference;
        public string Name => reference.Name;
        public SceneType sceneType;
    }
}