using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketServer;

public class RoomManager
{
    List<Room> RoomList = new List<Room>();
    MainServer ServerNetwork;

    public RoomManager(MainServer mainServer)
    {
        ServerNetwork = mainServer;
    }

    public void CreateRooms()
    {
        var maxRoomCount = ServerNetwork.ServerOption.RoomMaxCount;
        var startNumber = ServerNetwork.ServerOption.RoomStartNumber;
        var maxUserCount = ServerNetwork.ServerOption.RoomMaxUserCount;

        for(int i=0; i<maxRoomCount; i++)
        {
            var roomNubmer = startNumber + i;
            var room = new Room();
            room.Init(i, roomNubmer, maxUserCount);
            RoomList.Add(room);
        }
    }

    public List<Room> GetRoomList()
    {
        return RoomList;
    }

}
