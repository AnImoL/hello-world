using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI; /* This requires TShockAPI.dll as reference */
using Terraria; /* This requires TerrariaServer.exe as reference  */

namespace AnITestPlugin3
{
    [TerrariaApi.Server.ApiVersion(1, 17)] /* The version of TerrariaServer.exe (or the API, currently 1.17.0.0) */
    public class AnITestPlugin3 : TerrariaApi.Server.TerrariaPlugin
    {
        public AnITestPlugin3(Main game)
            : base(game)
        {
        }
        public override void Initialize()
        {
        }
        public override Version Version
        {
            get { return new Version("1.1"); } /* YOu can change this as your version improves */
        }
        public override string Name
        {
            get { return "AnITestPlugin3"; } /* The name of your plugin */
        }
        public override string Author
        {
            get { return "AnImoL"; } /* Your name */
        }
        public override string Description
        {
            get { return "Testing 6 6 6!"; } /* Description... gotta get more info */
        }
    }
}
