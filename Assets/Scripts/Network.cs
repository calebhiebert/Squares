using Assets.Scripts;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets
{
    public class Network : MonoBehaviour
    {
        private NetworkingClient client;
        private NetworkingServer server;

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (client.Client != null)
                {
                    client.Client.Disconnect("menu");
                }

                if (server.Server != null)
                {
                    server.Server.Shutdown("menu");
                }

                Destroy(gameObject);

                SceneManager.LoadScene("Menu");
            }
        }

        public void Start()
        {
            DontDestroyOnLoad(gameObject);
            client = GetComponent<NetworkingClient>();
            server = GetComponent<NetworkingServer>();
        }

        public void Join()
        {
            client.Connect();
        }

        public void Host()
        {
            server.Host();

            client.Host = "127.0.0.1";

            Join();
        }
    }
}
