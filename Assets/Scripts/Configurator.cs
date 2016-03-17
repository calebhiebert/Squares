using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts
{
    class Configurator
    {
        public struct ConfigData
        {
            public string name;
            public Color color;
        }

        public static void SaveConfig(string name, Color color)
        {
            TextWriter writer = new StreamWriter(Application.persistentDataPath + "/" + "config.cfg");

            writer.WriteLine(name);
            writer.WriteLine(color.r);
            writer.WriteLine(color.g);
            writer.WriteLine(color.b);

            writer.Close();
        }

        public static ConfigData LoadConfig()
        {
            try
            {
                TextReader reader = new StreamReader(Application.persistentDataPath + "/" + "config.cfg");

                var c = new ConfigData();

                c.name = reader.ReadLine();
                c.color = new Color(
                    float.Parse(reader.ReadLine()),
                    float.Parse(reader.ReadLine()),
                    float.Parse(reader.ReadLine()));

                reader.Close();

                return c;
            }
            catch (FileNotFoundException)
            {
                Debug.Log("Config file was not present, or was corrupted");

                return new ConfigData()
                {
                    color = Color.white,
                    name = "Chuck Norris"
                };
            }
        }
    }
}
