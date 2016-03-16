using System.Collections;
using Lidgren.Network;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.Networking
{
    class NetworkMain : MonoBehaviour
    {
        public static readonly string ApplicationIdentification = "Squares";
        public static event SceneLoadedEvent OnSceneLoadComplete;
        public static NetworkMain Current;
        public static bool IsServer;

        public delegate void SceneLoadedEvent(string sceneName);
        public GameObject PlayerPrefab;

        public string GameSceneName = "Game";
        public string HostAdress = "127.0.0.1";
        public int HostPort = 9888;
        public float SimulatedPing = 50;
        public float ServerUpdatesPerSecond = 25;
        public float ServerTimeout = 5;

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                KillNetwork();

                Destroy(gameObject);

                SceneManager.LoadScene("Menu");
            }

            if(Client.Current != null)
                Client.Current.Update();

            if(Server.Current != null)
                Server.Current.Update();
        }

        void OnGUI()
        {
            if (Client.Current != null && Client.Current.ServerConnection != null)
            {
                GUI.Label(new Rect(10, 10, 100, 20), "Ping: " + (int)(Client.Current.ServerConnection.AverageRoundtripTime * 1000));
            }
        }

        public void Awake()
        {
            DontDestroyOnLoad(gameObject);
            Current = this;
        }

        private void KillNetwork()
        {
            KillClient();
            KillServer();
        }

        private void KillClient()
        {
            if (Client.Current != null)
            {
                Client.Current.Disconnect("menu");
                Client.Current = null;
            }
        }

        private void KillServer()
        {
            if (Server.Current != null)
            {
                Server.Current.Shutdown("menu");
                Server.Current = null;
            }
        }

        public void Join()
        {
            if (Client.Current != null)
                KillClient();

            if(Client.Current == null)
                Client.Current = new Client(new NetPeerConfiguration(ApplicationIdentification)
                {
                    SimulatedMinimumLatency = SimulatedPing / 2.0f
                });

            Client.Current.Connect(HostAdress, HostPort);

            IsServer = false;
        }

        public void Host()
        {
            if (Client.Current != null)
                KillClient();

            if (Client.Current == null)
                Client.Current = new Client(new NetPeerConfiguration(ApplicationIdentification)
                {
                    SimulatedMinimumLatency = SimulatedPing / 2.0f,
                    ConnectionTimeout = ServerTimeout
                });

            if (Server.Current != null)
                KillServer();

            if(Server.Current == null)
                Server.Current = new Server(new NetPeerConfiguration(ApplicationIdentification)
                {
                    Port = HostPort,
                    SimulatedMinimumLatency = SimulatedPing / 2.0f
                }, Client.Current);

            Client.Current.Connect("127.0.0.1", HostPort);

            IsServer = true;
        }

        public PlayerController SpawnPlayer(byte playerId, Vector2 spawnPosition, string playerName)
        {
            var newPlayer = (GameObject) Instantiate(PlayerPrefab, spawnPosition, Quaternion.identity);

            var controller = newPlayer.GetComponent<PlayerController>();

            controller.NetPlayer = new NetPlayer(playerId, controller)
            {
                Name = playerName,
            };

            return controller;
        }

        public void LoadMap(string mapName)
        {
            StartCoroutine(LoadScene(mapName));
        }

        private IEnumerator LoadScene(string mapName)
        {
            var operation = SceneManager.LoadSceneAsync(mapName);

            while (!operation.isDone)
            {
                yield return null;
            }

            if (OnSceneLoadComplete != null)
                OnSceneLoadComplete(mapName);
        }
    }
}
