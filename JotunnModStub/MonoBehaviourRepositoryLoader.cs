using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace Valharvest {
    public static class MonoBehaviourRepositoryLoader {
        private static Assembly _monoBehaviourRepositoryAssembly;

        public static Assembly MonoBehaviourRepositoryAssembly {
            get {
                if (_monoBehaviourRepositoryAssembly != null) return _monoBehaviourRepositoryAssembly;
                var path = Path.Combine(AssemblyDirectory.FullName, "MonoBehaviourRepository.dll");
                _monoBehaviourRepositoryAssembly = Assembly.LoadFile(path);
                return _monoBehaviourRepositoryAssembly;
            }
        }

        private static DirectoryInfo AssemblyDirectory {
            get {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                var fileInfo = new FileInfo(Uri.UnescapeDataString(uri.Path));
                return fileInfo.Directory;
            }
        }

        #region GameObject Extensions

        public static GameObject AddMonoBehaviour(this GameObject gameObject, string name) {
            Type type = Assembly.GetExecutingAssembly().GetType(name) ?? MonoBehaviourRepositoryAssembly.GetType(name);

            if (type == null) {
                throw new ArgumentException($"Unable to find MonoBehaviour: {name}", nameof(name));
            }

            gameObject.AddComponent(type);
            return gameObject;
        }

        public static GameObject AddMonoBehaviour<T>(this GameObject gameObject) where T : MonoBehaviour {
            gameObject.AddComponent<T>();
            return gameObject;
        }

        public static MonoBehaviour GetOrAddMonoBehaviour(this GameObject gameObject, string name) {
            return (gameObject.GetComponent(name) ?? gameObject.AddMonoBehaviour(name).GetComponent(name)) as
                MonoBehaviour;
        }

        /// <summary>
        /// Returns the component of Type type. If one doesn't already exist on the GameObject it will be added.
        /// Source: Jotunn JVL
        /// </summary>
        /// <remarks>Source: https://wiki.unity3d.com/index.php/GetOrAddComponent</remarks>
        /// <typeparam name="T">The type of Component to return.</typeparam>
        /// <param name="gameObject">The GameObject this Component is attached to.</param>
        /// <returns>Component</returns>
        public static T GetOrAddMonoBehaviour<T>(this GameObject gameObject) where T : MonoBehaviour {
            return gameObject.GetComponent<T>() ?? gameObject.AddComponent<T>();
        }

        #endregion
    }
}