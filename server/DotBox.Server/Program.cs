using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// 1) 헬스 체크 (/health)
app.MapGet("/health", () => new { status = "ok" });
/*
MapGet ("경로",핸들러함수) /health 경로로 Get 요청이 오면 핸들러함수를 실행
() => 람다식(익명함수) 사용 ()는 빈 함수 리턴 객체가 자동으로 JSON 변환되어 응답됨
*/

// 2) 플레이어 접속 (/connect)
// 클라에서 JSON body로 { "playerName": "seonseo" } 보냄
//    - playerName을 받으면 playerId 만들어서 저장
app.MapPost("/connect", (ConnectRequest req) =>
{
    // playerName 유효성 검사
    if (string.IsNullOrWhiteSpace(req.PlayerName))
        return Results.BadRequest(new { error = "playerName is required" });

    // 고유한 플레이어 ID 생성
    /*
    Guid.NewGuid() : 랜덤한 고유 ID 생성
    SessionStore.Players : 서버 메모리에 접속중인 모든 플레이어 저장
    Results.Ok(...) : HTTP 200 + JSON 응답 반환
    */
    var playerId = Guid.NewGuid().ToString("N");

    var session = new PlayerSession
    {
        PlayerId = playerId,
        PlayerName = req.PlayerName,
        ConnectedAt = DateTime.UtcNow
    };

    SessionStore.Players[playerId] = session;

    return Results.Ok(new
    {
        session.PlayerId,
        session.PlayerName,
        session.ConnectedAt
    });
});

//
// 3) 현재 접속 중인 플레이어 목록 (/players)
app.MapGet("/players", () =>
{
    return Results.Ok(SessionStore.Players.Values);
});

//
// 4) 방 (/room/join)
// 방 생성: 방장(플레이어)이 방 하나 만들고 초대코드 받기
app.MapPost("/room/create", (CreateRoomRequest req) =>
{
    if (!SessionStore.Players.ContainsKey(req.PlayerId))
        return Results.BadRequest(new { error = "Invalid playerId" });

    var room = new GameRoom
    {
        RoomId = Guid.NewGuid().ToString("N"),
        InviteCode = InviteCodeGenerator.Generate(6)
    };

    // 방 만든 사람은 자동으로 입장 + 첫 턴
    room.Players.Add(req.PlayerId);
    room.CurrentTurn = req.PlayerId;

    RoomStore.Rooms[room.RoomId] = room;

    return Results.Ok(new
    {
        room.RoomId,
        room.InviteCode,
        players = room.Players,
        room.MaxPlayers,
        room.IsFull,
        room.CurrentTurn
    });
});

// 방 입장: 초대코드로 특정 방에 들어가기
app.MapPost("/room/join", (JoinRoomByCodeRequest req) =>
{
    if (!SessionStore.Players.ContainsKey(req.PlayerId))
        return Results.BadRequest(new { error = "Invalid playerId" });

    // 초대코드로 방 찾기 (대소문자 무시)
    var room = RoomStore.Rooms.Values
        .FirstOrDefault(r =>
            string.Equals(r.InviteCode, req.InviteCode, StringComparison.OrdinalIgnoreCase));

    if (room == null)
        return Results.NotFound(new { error = "Room not found" });

    if (room.IsFull)
        return Results.BadRequest(new { error = "Room is full" });

    if (!room.Players.Contains(req.PlayerId))
        room.Players.Add(req.PlayerId);

    if (room.CurrentTurn == null && room.Players.Count > 0)
        room.CurrentTurn = room.Players[0];

    return Results.Ok(new
    {
        room.RoomId,
        room.InviteCode,
        players = room.Players,
        room.MaxPlayers,
        room.IsFull,
        room.CurrentTurn
    });
});

//
// 5) 방 상태 조회 (/room/state/{roomId})
//
app.MapGet("/room/state/{roomId}", (string roomId) =>
{
    if (!RoomStore.Rooms.TryGetValue(roomId, out var room))
        return Results.NotFound(new { error = "Room not found" });

    return Results.Ok(new
    {
        room.RoomId,
        room.InviteCode,
        players = room.Players,
        room.MaxPlayers,
        room.CurrentTurn,
        room.CreatedAt,
        room.IsFull
    });
});

// 5-1) 게임 시작 (/game/start)
app.MapPost("/game/start", (GameStartRequest req) =>
{
    if (!RoomStore.Rooms.TryGetValue(req.RoomId, out var room))
        return Results.BadRequest(new { error = "Room not found" });

    if (!room.Players.Contains(req.PlayerId))
        return Results.BadRequest(new { error = "Player not in room" });

    if (room.Players.Count < 2)
        return Results.BadRequest(new { error = "Need at least 2 players to start" });

    if (room.GameStarted)
        return Results.BadRequest(new { error = "Game already started" });

    // 턴 순서를 랜덤으로 섞기
    var shuffled = room.Players
        .OrderBy(_ => RandomNumberGenerator.GetInt32(int.MaxValue))
        .ToList();

    room.TurnOrder = shuffled;
    room.GameStarted = true;

    return Results.Ok(new
    {
        room.RoomId,
        room.InviteCode,
        players = room.Players,
        turnOrder = room.TurnOrder,
        firstPlayer = room.TurnOrder[0]
    });
});

//
// 6) 한 턴 진행 (/game/move)
//    - 지금은 "턴만 바꾸는" 골격만 있음
//      나중에 여기다가 Dot&Box 보드 로직 추가
//
app.MapPost("/game/move", (MoveRequest req) =>
{
    if (!RoomStore.Rooms.TryGetValue(req.RoomId, out var room))
        return Results.BadRequest(new { error = "Room not found" });

    if (room.CurrentTurn != req.PlayerId)
        return Results.BadRequest(new { error = "Not your turn" });

    // 턴 교대 (A ↔ B)
    string nextTurn =
        (room.PlayerA == req.PlayerId) ? room.PlayerB! : room.PlayerA!;

    room.CurrentTurn = nextTurn;

    return Results.Ok(new
    {
        status = "ok",
        room.RoomId,
        room.CurrentTurn
    });
});

// 7) 게임 좌표 찍기 (/game/point)
app.MapPost("/game/point", (GamePointRequest req) =>
{
    if (!RoomStore.Rooms.TryGetValue(req.RoomId, out var room))
        return Results.BadRequest(new { error = "Room not found" });

    if (!room.Players.Contains(req.PlayerId))
        return Results.BadRequest(new { error = "Player not in room" });

    if (!room.GameStarted)
        return Results.BadRequest(new { error = "Game not started" });

    var ev = new GamePointEvent
    {
        Seq = room.NextSeq++,
        PlayerId = req.PlayerId,
        X = req.X,
        Y = req.Y,
        CreatedAt = DateTime.UtcNow
    };

    room.Events.Add(ev);

    return Results.Ok(new
    {
        status = "ok",
        roomId = room.RoomId,
        point = new
        {
            ev.Seq,
            ev.PlayerId,
            ev.X,
            ev.Y,
            ev.CreatedAt
        }
    });
});

// 8) 게임 좌표 이벤트 조회 (/game/points?roomId=...&afterSeq=...)
app.MapGet("/game/points", (string roomId, long? afterSeq) =>
{
    if (!RoomStore.Rooms.TryGetValue(roomId, out var room))
        return Results.BadRequest(new { error = "Room not found" });

    long seq = afterSeq ?? 0;

    var events = room.Events
        .Where(e => e.Seq > seq)
        .OrderBy(e => e.Seq)
        .Take(100) // 너무 많이 한 번에 안 보내려고 제한
        .Select(e => new
        {
            e.Seq,
            e.PlayerId,
            e.X,
            e.Y,
            e.CreatedAt
        })
        .ToList();

    return Results.Ok(new
    {
        roomId = room.RoomId,
        events
    });
});

app.Run();


//
// ---------------- 밑에는 타입 정의 구역 ----------------
//   (C#에서는 같은 파일 맨 아래에 class들 써도 됨)
//

public class PlayerSession
{
    public string PlayerId { get; set; } = default!;
    public string PlayerName { get; set; } = default!;
    public DateTime ConnectedAt { get; set; }
}

public static class SessionStore
{
    // key: playerId
    public static ConcurrentDictionary<string, PlayerSession> Players { get; }
        = new();
}

public class GameRoom
{
    public string RoomId { get; set; } = default!;
    public string InviteCode { get; set; } = default!;

    // 방 안 플레이어 ID들 (최대 3명)
    public List<string> Players { get; set; } = new();

    public int MaxPlayers { get; set; } = 3;

    // 게임이 시작되었는지 여부
    public bool GameStarted { get; set; } = false;

    // 랜덤으로 섞인 턴 순서 (playerId 리스트)
    public List<string> TurnOrder { get; set; } = new();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // 좌표 이벤트 로그
    public List<GamePointEvent> Events { get; set; } = new();

    public long NextSeq { get; set; } = 1;  // 이벤트 시퀀스 번호

    public bool IsFull => Players.Count >= MaxPlayers;
}

public class GamePointEvent
{
    public long Seq { get; set; }
    public string PlayerId { get; set; } = default!;
    public int X { get; set; }
    public int Y { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class GamePointRequest
{
    public string RoomId { get; set; } = default!;
    public string PlayerId { get; set; } = default!;
    public int X { get; set; }
    public int Y { get; set; }
}

public static class RoomStore
{
    // key: roomId
    public static ConcurrentDictionary<string, GameRoom> Rooms { get; }
        = new();
}

public class ConnectRequest
{
    public string PlayerName { get; set; } = default!;
}

public class CreateRoomRequest
{
    public string PlayerId { get; set; } = default!;
}

public class JoinRoomByCodeRequest
{
    public string PlayerId { get; set; } = default!;
    public string InviteCode { get; set; } = default!;
}

public class MoveRequest
{
    public string RoomId { get; set; } = default!;
    public string PlayerId { get; set; } = default!;

    // 선 시작점
    public int X1 { get; set; }
    public int Y1 { get; set; }

    // 선 끝점
    public int X2 { get; set; }
    public int Y2 { get; set; }
}

public static class InviteCodeGenerator
{
    // 헷갈리는 문자 뺀 문자셋 (O/0, I/1 등)
    private const string Chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    public static string Generate(int length = 6)
    {
        var bytes = new byte[length];
        RandomNumberGenerator.Fill(bytes);

        var sb = new StringBuilder(length);
        for (int i = 0; i < length; i++)
        {
            sb.Append(Chars[bytes[i] % Chars.Length]);
        }
        return sb.ToString();
    }
}

public class GameStartRequest
{
    public string RoomId { get; set; } = default!;
    public string PlayerId { get; set; } = default!;
}

