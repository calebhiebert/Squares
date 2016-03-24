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
        public GameObject GameUiPrefab;
        public Image ColorPickerImage;
        public InputField NameInputField;
        public AudioClip Ding;

        public Slider RSlider;
        public Slider GSlider;
        public Slider BSlider;

        public string GameSceneName = "Game";
        public string HostAdress = "127.0.0.1";
        public int HostPort = 9888;
        public float SimulatedPing = 50;
        public float ServerUpdatesPerSecond = 25;
        public float Timeout = 5;
        

        public string PlayerName;
        public Color PlayerColor;
        public float ColorLightnessFactor = 0.1f;

        private float GameTimeScale = 1;

        private bool _isLoading;

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ToMenu();
            }

            if(Client.Current != null)
                Client.Current.Update();

            if(Server.Current != null)
                Server.Current.Update();
        }

        public void ToMenu()
        {
            KillNetwork();

            Destroy(gameObject);

            SceneManager.LoadScene("Menu");
        }

        void OnGUI()
        {
            if (Client.Current != null && Client.Current.ServerConnection != null)
            {
                GUI.Label(new Rect(10, 10, 100, 20), "Ping: " + (int)(Client.Current.ServerConnection.AverageRoundtripTime * 1000));
            }

            if (Client.Current != null && Client.Current.LocalPlayer != null)
            {
                GUI.Label(new Rect(10, 25, 100, 20), "Dmg: " + Client.Current.LocalPlayer.ExplosionForceModifier + "%");
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

                conf.ConnectionTimeout = Timeout;

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

                conf.ConnectionTimeout = Timeout;

                Client.Current = new Client(conf);
            }

            /* Make Server */ 

            if (Server.Current != null)
                KillServer();

            if (Server.Current == null)
            {
                var conf = new NetPeerConfiguration(ApplicationIdentification);

                conf.ConnectionTimeout = Timeout;
                conf.Port = HostPort;

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

            Debug.Log("Spawned New Player");

            return controller;
        }

        public void LoadMap(string mapName)
        {
            if(mapName == SceneManager.GetActiveScene().name || _isLoading)
                return;

            StartCoroutine(LoadScene(mapName));
        }

        void OnLevelWasLoaded(int lvl)
        {
            Debug.Log("Level " + lvl + " was loaded.");

            if (lvl == 1)
            {
                Client.Current.Register(Current.PlayerName, Current.PlayerColor);

                Instantiate(GameUiPrefab);
            }
        }

        private IEnumerator LoadScene(string mapName)
        {
            _isLoading = true;

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

        public void ChangeTimeScale(float scale, float duration)
        {
            Time.timeScale = scale;

            Invoke("ResetTimeScale", duration);
        }

        void ResetTimeScale()
        {
            Time.timeScale = 1;
        }
    }
}
