using System.Collections;
using Lidgren.Network;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
        public Image ColorPickerImage;
        public InputField NameInputField;

        public Slider RSlider;
        public Slider GSlider;
        public Slider BSlider;

        public string GameSceneName = "Game";
        public string HostAdress = "127.0.0.1";
        public int HostPort = 9888;
        public float SimulatedPing = 50;
        public float ServerUpdatesPerSecond = 25;
        public float ServerTimeout = 5;

        public string PlayerName;
        public Color PlayerColor;

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

            var cfg = Configurator.LoadConfig();

            NameInputField.text = cfg.name;
            RSlider.value = cfg.color.r;
            GSlider.value = cfg.color.g;
            BSlider.value = cfg.color.b;
        }

        void OnApplicationQuit()
        {
            Configurator.SaveConfig(PlayerName, PlayerColor);
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

            if (Client.Current == null)
            {
                var conf = new NetPeerConfiguration(ApplicationIdentification);

                /*if (SimulatedPing > 0)
                    conf.SimulatedMinimumLatency = SimulatedPing / 2000.0f;*/

                Client.Current = new Client(conf);
            }

            Client.Current.Connect(HostAdress, HostPort);

            IsServer = false;
        }

        public void Host()
        {
            /* Make Client */

            if (Client.Current != null)
                KillClient();

            if (Client.Current == null)
            {
                var conf = new NetPeerConfiguration(ApplicationIdentification);

                /*if (SimulatedPing > 0)
                    conf.SimulatedMinimumLatency = SimulatedPing / 2000.0f;*/

                Client.Current = new Client(conf);
            }

            /* Make Server */ 

            if (Server.Current != null)
                KillServer();

            if (Server.Current == null)
            {
                var conf = new NetPeerConfiguration(ApplicationIdentification);

                conf.ConnectionTimeout = 5.0f;
                conf.Port = HostPort;

                /*if (SimulatedPing > 0)
                    conf.SimulatedMinimumLatency = SimulatedPing / 2000.0f;*/

                Server.Current = new Server(conf, Client.Current);
            }

            Client.Current.Connect("127.0.0.1", HostPort);

            IsServer = true;
        }

        public PlayerController SpawnPlayer(byte playerId, Vector2 spawnPosition, string playerName, Color color)
        {
            var newPlayer = (GameObject) Instantiate(PlayerPrefab, spawnPosition, Quaternion.identity);

            var controller = newPlayer.GetComponent<PlayerController>();

            controller.NetPlayer = new NetPlayer(playerId, controller)
            {
                Name = playerName,
                Color = color
            };

            /* Visual Stuff */
            foreach (var spr in newPlayer.GetComponentsInChildren<SpriteRenderer>())
            {
                spr.color = color;
            }

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

        public float SetR
        {
            set
            {
                PlayerColor.r = value;
                ColorPickerImage.color = PlayerColor;
            }
        }

        public float SetG
        {
            set
            {
                PlayerColor.g = value;
                ColorPickerImage.color = PlayerColor;
            }
        }

        public float SetB
        {
            set
            {
                PlayerColor.b = value;
                ColorPickerImage.color = PlayerColor;
            }
        }

        public string SetName { set { PlayerName = value; } }
        public string SetHostAdress { set { HostAdress = value; } }
    }
}
