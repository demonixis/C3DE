using C3DE;
using Newtonsoft.Json;
using System;
using System.IO;

namespace C3DE.Player
{
    /*
     * {
	        "Title": "C3DE Player Demo",
	        "Width": 1280,
	        "Height": 800,
	        "Fullscreen": true,
	        "VWidth": 1280,
	        "VHeight": 800,
	        "AssetFolder": "Content",
	        "Scenes": [{
		        "Name": "Menu Scene",
		        "Materials": [],
		        "SceneObjects": [{
			        "Name": "SceneObject 1",
			        "Id": 123456789,
			        "Transform": [
				        [0, 0, 0],
				        [0, 1.5, 0],
				        [1, 4, 2]
			        ]
		        }]
	        }, {
		        "Name": "Demo Scene",
		        ...
	        }]
        }
     */
    class Program
    {
        private const string ConfigFileName = "game.dat";

        static void Main(string[] args)
        {
            if (!File.Exists(ConfigFileName))
                throw new Exception("The Configuration file is missing.");

            var configFile = File.OpenText(ConfigFileName);
            var data = JsonConvert.DeserializeObject(configFile.ReadToEnd());

            var title = "C3DE Player";
            var width = 1280;
            var height = 800;
            var fullscreen = false;
            var vWidth = 1280;
            var vHeight = 800;
            var assetFolder = "Content";

            if (!Directory.Exists(assetFolder))
                throw new Exception("The asset folder is missing.");

            using (var game = new Engine(title, width, height, fullscreen))
            {
                Screen.SetVirtualResolution(vWidth, vHeight);
                game.Run();
            }
        }
    }
}
