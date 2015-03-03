using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TShockAPI; /* This requires TShockAPI.dll as reference */
using Terraria; /* This requires TerrariaServer.exe as reference  */
 
namespace TestPlugin
{
    [ApiVersion(1, 16)] /* The version of TerrariaServer.exe (or the API, currently 1.16.0.0) */
    public class TestPlugin : TerrariaPlugin
    {
        public TestPlugin(Main game) : base(game)
        {
        }
        public override void Initialize()
        {
        }
        public override Version Version
        {
            get { return new Version("1.0"); } /* YOu can change this as your version improves */
        }
        public override string Name
        {
            get { return "TestPlugin"; } /* The name of your plugin */
        }
        public override string Author
        {
            get { return "AnImoL"; } /* Your name */
        }
        public override string Description
        {
            get { return "Testing!"; } /* Description... gotta get more info */
        }
    }
}
