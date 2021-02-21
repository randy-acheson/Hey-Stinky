using UnityEngine;
using System.Net.Sockets;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.ServiceModel;
using System.Collections.Generic;



public interface CreatureBase {
    String get_player_hash();
    Dictionary<String, String> getPositionDict();
    GameObject getGameObject();
}
