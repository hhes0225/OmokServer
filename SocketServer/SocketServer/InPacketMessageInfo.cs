using MemoryPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketLibrary;


[MemoryPackable]
public partial class PKTNtfInUserCheck
{
}

[MemoryPackable]
public partial class PKTNtfInRoomCheck
{
}

[MemoryPackable]
public partial class PKTNtfInnerTurnCheck
{
}

[MemoryPackable]
public partial class PKTNtfInGameResultUpdate
{
    public bool IsDraw;
    public string Winner;
    public string Loser;
}

[MemoryPackable]
public partial class PKTReqInInsertTestUser
{
    public string Id;
    public int WinCount;
    public int DrawCount;
    public int LoseCount;
}

[MemoryPackable]
public partial class PKTResInInsertTestUser
{
    public short Result;
}