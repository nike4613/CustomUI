using IPA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using IPALogger = IPA.Logging.Logger;

namespace CustomUI
{
    internal static class Logger
    {
        public static IPALogger log { get; set; }
    }

    internal class Plugin : IBeatSaberPlugin
    {

        public void Init(IPALogger logger)
        {
            Logger.log = logger;
            Logger.log.Debug("Init called");
        }

        public void OnActiveSceneChanged(Scene prevScene, Scene nextScene)
        {
        }

        public void OnApplicationQuit()
        {
        }

        public void OnApplicationStart()
        {
        }

        public void OnFixedUpdate()
        {
        }

        public void OnSceneLoaded(Scene scene, LoadSceneMode sceneMode)
        {
        }

        public void OnSceneUnloaded(Scene scene)
        {
        }

        public void OnUpdate()
        {
        }
    }
}
