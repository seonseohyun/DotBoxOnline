// Program.cs
using System.Collections.Concurrent; // ConcurrentDictionary 사용 위해 필요 - 여러 스레드 동시 접근에 안전한 딕셔너리
using System.Security.Cryptography;  // 랜덤 초대코드 생성을 위한 암호학적 난수 생성
using System.Text;                   // StringBuilder 사용 위해 필요 - 문자열 붙이기

// ⚝WebApplication: ASP.NET Core에서 제공하는 HTTP 서버 본체
var builder = WebApplication.CreateBuilder(args);  //서버 설정/환경구성
var app = builder.Build();                         //서버 본체 생성

// ====================================================================
// 1) 헬스 체크 (/health)
// ====================================================================

app.MapGet("/health", () => new { status = "ok" });
/*
MapGet ("경로",핸들러함수) /health 경로로 Get 요청이 오면 핸들러함수를 실행
() => 람다식(익명함수) 사용 ()는 빈 함수 리턴 객체가 자동으로 JSON 변환되어 응답됨
*/

// ====================================================================
// 2) 플레이어 접속 (/connect)
// 클라에서 JSON body로 { "playerName": "seonseo" } 보냄
//    - playerName을 받으면 playerId 만들어서 SessionStore에 저장
// ====================================================================

app.MapPost("/connect", (ConnectRequest req) =>
{
    // ⚝Model Binding : 
    // 클라이언트의 JSON body의 "playerName" 필드를 ConnectRequest.PlayerName에 자동 매핑

    // playerName 유효성 검사
    if (string.IsNullOrWhiteSpace(req.PlayerName))
        return Results.BadRequest(new { error = "playerName is required" });

    // 고유한 플레이어 ID 생성
    /*
    Guid.NewGuid() : 랜덤한 고유 ID 생성 - 전역 고유 식별자 (UUID) 생성
    SessionStore.Players : 서버 메모리에 접속중인 모든 플레이어 저장
     - key: playerId, value: PlayerSession 객체
    Results.Ok(...) : HTTP 200 + JSON 응답 반환 -> 200은 어떻게 전달되는가? ^-^? json 안에는 없는데. . . 의문@@@
    */
    var playerId = Guid.NewGuid().ToString("N"); //    ToString("N")  : 중간의 '-'를 제거한 32자리 문자열로 변환

    var session = new PlayerSession
    {
        PlayerId = playerId,
        PlayerName = req.PlayerName,
        ConnectedAt = DateTime.UtcNow
    };

    //currentDictionary에 플레이어 세션 등록, 왜? -> 여러 스레드에서 동시에 접근할 수 있기 때문에
    SessionStore.Players[playerId] = session;

    // 클라이언트가 앞으로 사용할 playerId를 응답으로 넘겨준다.
    return Results.Ok(new
    {
        session.PlayerId,
        session.PlayerName,
        session.ConnectedAt
    });
});

// ====================================================================
// 3) 현재 접속 중인 플레이어 목록 (/players)
// ====================================================================

app.MapGet("/players", () =>
{
    //SessionStore.Players.Values : Dictionary의 value들(PlayerSession 객체들)만 반환
    //Json 배열로 응답 -> 어케함? 어찌알고 함? 의문@@@
    return Results.Ok(SessionStore.Players.Values);
});

// ====================================================================
// 4) 방 생성, 참가(/room/create, /room/join)
// ====================================================================

app.MapPost("/room/create", (CreateRoomRequest req) =>
{
    // 1) playerId 존재하는지 확인
    if (!SessionStore.Players.ContainsKey(req.PlayerId))
        return Results.BadRequest(new { error = "Invalid playerId" });
        
    // 2) 새 방 객체 생성
    var room = new GameRoom //var는 뭐냐? @@@
    {
        RoomId = Guid.NewGuid().ToString("N"),      // 고유한 방 ID 생성
        InviteCode = InviteCodeGenerator.Generate(6)// 6자리 초대코드 생성
    };

    // 3) 방 만든 사람은 자동으로 입장 (players 리스트에 추가)
    room.Players.Add(req.PlayerId);

    // 4) 방 생성 시점에서는 턴을 정하지 않고 게임 시작시 결정 -> /game/start에서 결정
    room.CurrentTurn = null;

    // 5) RoomStore에 방 등록 (roomID -> GameRoom 객체 매핑)
    RoomStore.Rooms[room.RoomId] = room;

    // 6) 클라이언트에게 방 정보와 초대코드 알려주기
    return Results.Ok(new
    {
        room.RoomId,            //방 ID
        room.InviteCode,        //초대 코드
        players = room.Players, //방에 있는 플레이어 ID 리스트
        room.MaxPlayers,        //최대 플레이어 수 이거 줘야함? 어차피 세명 아님?@@@
        room.IsFull,            //방이 가득 찼는지 여부
        room.CurrentTurn        //null 반환
    });
});

// 방 입장 (/room/join)
app.MapPost("/room/join", (JoinRoomByCodeRequest req) =>
{
    // 1) 초대코드로 방 찾기 (대소문자 무시)
    var room = RoomStore.Rooms.Values
        .FirstOrDefault(r =>
            string.Equals(r.InviteCode, req.InviteCode, StringComparison.OrdinalIgnoreCase));

    if (room == null)
        return Results.NotFound(new { error = "Room not found" });

    // 2) playerId가 방에 존재하는지 확인
    if (room.Players.Contains(req.PlayerId)) // 있으면 그냥 ok 반환
    {
        return Results.Ok(new
        {
            status = "ok",
            room.RoomId,
            room.InviteCode,
        });
    }     
    // 여기까지 왔다는 건, 아직 방에 안 들어간 새 플레이어라는 뜻
    // 3) 방이 가득 찼으면 결과 리턴
    if (room.IsFull)
        return Results.BadRequest(new { error = "Room is full" });

    // 4) 아니면 플레이어를 방에 추가
    room.Players.Add(req.PlayerId);

    if (room.CurrentTurn == null && room.Players.Count > 0)
        room.CurrentTurn = room.Players[0];

    return Results.Ok(new
    {
        status = "ok",
        room.RoomId,
        room.InviteCode
    });
});

// ====================================================================
// 5) 방 상태 조회 (/room/state/{roomId})
// ====================================================================

app.MapGet("/room/state/{roomId}", (string roomId) =>
{
    // roomId로 방 찾기 (TryGetValue: 있으면 true, 없으면 false)
    if (!RoomStore.Rooms.TryGetValue(roomId, out var room))
        return Results.NotFound(new { error = "Room not found" });

    // GameRoom 전체를 그대로 돌려주는 대신 필요한 필드만 선택해서 익명 객체로 반환
    return Results.Ok(new
    {
        room.RoomId,
        room.InviteCode,
        players = room.Players,
        // room.MaxPlayers,
        // room.CurrentTurn,
        // room.CreatedAt,
        room.IsFull
    });
});

// ====================================================================
// 5-1) 게임 시작 (/game/start)
// ====================================================================

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
    // ⚝RandomNumberGenerator: 암호학적으로 안전한 난수 생성기
    // ⚝OrderBy(_ => RandomNumberGenerator.GetInt32(int.MaxValue)) 로 랜덤 셔플 효과

    var shuffled = room.Players
        .OrderBy(_ => RandomNumberGenerator.GetInt32(int.MaxValue))
        .ToList();

    room.TurnOrder = shuffled;
    room.CurrentTurn = room.TurnOrder[0];    // 첫 번째 플레이어부터 시작    
    room.GameStarted = true;

    // 클라이언트들에게 턴 순서와 첫 플레이어 정보를 알려준다
    return Results.Ok(new
    {
        room.RoomId,
        room.InviteCode,
        players = room.Players,
        turnOrder = room.TurnOrder,
        firstPlayer = room.TurnOrder[0],
        CurrentTurn = room.CurrentTurn
    });
});

// ====================================================================
// 6) 한 턴 진행 (/game/move)
//    - 지금은 "턴만 바꾸는 로직"만 존재
//    - 나중에 여기에서 실제 Dot&Box 보드 판정 로직 호출 가능
// ====================================================================

// app.MapPost("/game/move", (MoveRequest req) =>
// {
//     if (!RoomStore.Rooms.TryGetValue(req.RoomId, out var room))
//         return Results.BadRequest(new { error = "Room not found" });

//     if (!room.GameStarted)
//         return Results.BadRequest(new { error = "Game not started" });
    
//     // 현재 턴이 아닌 사람이 호출하면 거절
//     if (room.CurrentTurn != req.PlayerId)
//         return Results.BadRequest(new { error = "Not your turn" });

//     // TurnOrder에서 현재 플레이어의 인덱스 찾기
//     int currentIndex = room.TurnOrder.IndexOf(room.CurrentTurn);
//     // 다음 플레이어 인덱스 계산 (+1 하다가 마지막이면 처음으로 돌아감) -> 순환 구조로 턴 부여
//     int nextIndex = (currentIndex + 1) % room.TurnOrder.Count;
//     string nextTurn = room.TurnOrder[nextIndex];
//     room.CurrentTurn = nextTurn;

//     return Results.Ok(new
//     {
//         status = "ok",
//         room.RoomId,
//         room.CurrentTurn
//     });
// });

// 7) 게임 좌표 찍기 (/game/point)
// app.MapPost("/game/point", (GamePointRequest req) =>
// {
//     if (!RoomStore.Rooms.TryGetValue(req.RoomId, out var room))
//         return Results.BadRequest(new { error = "Room not found" });

//     if (!room.Players.Contains(req.PlayerId))
//         return Results.BadRequest(new { error = "Player not in room" });

//     if (!room.GameStarted)
//         return Results.BadRequest(new { error = "Game not started" });

//     var ev = new GamePointEvent
//     {
//         Seq = room.NextSeq++,
//         PlayerId = req.PlayerId,
//         X = req.X,
//         Y = req.Y,
//         CreatedAt = DateTime.UtcNow
//     };

//     room.Events.Add(ev);

//     return Results.Ok(new
//     {
//         status = "ok",
//         roomId = room.RoomId,
//         point = new
//         {
//             ev.Seq,
//             ev.PlayerId,
//             ev.X,
//             ev.Y,
//             ev.CreatedAt
//         }
//     });
// });

// // 8) 게임 좌표 이벤트 조회 (/game/points?roomId=...&afterSeq=...)
// app.MapGet("/game/points", (string roomId, long? afterSeq) =>
// {
//     if (!RoomStore.Rooms.TryGetValue(roomId, out var room))
//         return Results.BadRequest(new { error = "Room not found" });

//     long seq = afterSeq ?? 0;

//     var events = room.Events
//         .Where(e => e.Seq > seq)
//         .OrderBy(e => e.Seq)
//         .Take(100) // 너무 많이 한 번에 안 보내려고 제한
//         .Select(e => new
//         {
//             e.Seq,
//             e.PlayerId,
//             e.X,
//             e.Y,
//             e.CreatedAt
//         })
//         .ToList();

//     return Results.Ok(new
//     {
//         roomId = room.RoomId,
//         events
//     });
// });

app.Run();


// ====================================================================
// ---------------- 밑에는 타입 정의 구역 ----------------
// ====================================================================

// 플레이어 세션 정보
public class PlayerSession
{
    public string PlayerId { get; set; } = default!;
    public string PlayerName { get; set; } = default!;
    public DateTime ConnectedAt { get; set; }
}

// 접속 중인 플레이어 목록을 관리하는 전역 저장소
public static class SessionStore
{
    // key: playerId
    public static ConcurrentDictionary<string, PlayerSession> Players { get; }
        = new();
}
// 방 목록 관리하는 전역 저장소
public static class RoomStore
{
    // key: roomId
    public static ConcurrentDictionary<string, GameRoom> Rooms { get; }
        = new();
}

// 하나의 게임 방 상태
public class GameRoom
{
    public string RoomId { get; set; } = default!;
    public string InviteCode { get; set; } = default!;

    // 방 안 플레이어 ID들 (최대 3명)
    public List<string> Players { get; set; } = new();

    public int MaxPlayers { get; set; } = 3;  //?@@ 왜 public으로 해놨지? 바꿀일 없을텐데 지짜 궁금

    // 게임이 시작되었는지 여부
    public bool GameStarted { get; set; } = false;

    // 랜덤으로 섞인 턴 순서 (playerId 리스트)
    public List<string> TurnOrder { get; set; } = null!; //null!이면 나중에 할당할 테니까 경고 무시 해줘 라는 뜻

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // 좌표 이벤트 로그
    // public List<GamePointEvent> Events { get; set; } = new();

    public long NextSeq { get; set; } = 1;  // 이벤트 시퀀스 번호

    // 현재 인원수가 MaxPlayer에 도달했으면 true 반환
    public bool IsFull => Players.Count >= MaxPlayers;

    public string? CurrentTurn { get; set;  } //현재 턴인 플레이어 ID (게임 시작 전에는 null)
}

// 좌표 이벤트 하나 발생~!!
// public class GamePointEvent
// {
//     public long Seq { get; set; }
//     public string PlayerId { get; set; } = default!;
//     public int X { get; set; }
//     public int Y { get; set; }
//     public DateTime CreatedAt { get; set; }
// }

// 게임 좌표 전송 요청
// public class GamePointRequest
// {
//     public string RoomId { get; set; } = default!;
//     public string PlayerId { get; set; } = default!;
//     public int X { get; set; }
//     public int Y { get; set; }
// }

// (/connect 요청 Body 모델)
public class ConnectRequest
{
    public string PlayerName { get; set; } = default!;
}

// (/room/create 요청 Body 모델)
public class CreateRoomRequest
{
    public string PlayerId { get; set; } = default!;
}

// (/room/join 요청 Body 모델)
public class JoinRoomByCodeRequest
{
    public string PlayerId { get; set; } = default!;
    public string InviteCode { get; set; } = default!;
}

// // (/game/move 요청 Body 모델)
// public class MoveRequest
// {
//     public string RoomId { get; set; } = default!;
//     public string PlayerId { get; set; } = default!;

//     // 선 시작점
//     public int X1 { get; set; }
//     public int Y1 { get; set; }

//     // 선 끝점
//     public int X2 { get; set; }
//     public int Y2 { get; set; }
// }

// 초대 코드 생성 유틸
public static class InviteCodeGenerator
{
    // 헷갈리는 문자 뺀 문자셋 (O/0, I/1 등) 뺐어요
    private const string Chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    public static string Generate(int length = 6)
    {
        var bytes = new byte[length];
        RandomNumberGenerator.Fill(bytes);  // 암호학적 난수 채우기

        var sb = new StringBuilder(length);
        for (int i = 0; i < length; i++)
        {
            sb.Append(Chars[bytes[i] % Chars.Length]);
        }
        return sb.ToString();
    }
}

// (/game/start 요청 Body 모델)
public class GameStartRequest
{
    public string RoomId { get; set; } = default!;
    public string PlayerId { get; set; } = default!;
}

