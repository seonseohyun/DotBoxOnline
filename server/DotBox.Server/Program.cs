// Program.cs
using System.Collections.Concurrent; // ConcurrentDictionary 사용 위해 필요 - 여러 스레드 동시 접근에 안전한 딕셔너리
using System.Security.Cryptography;  // 랜덤 초대코드 생성을 위한 암호학적 난수 생성
using System.Text;                   // StringBuilder 사용 위해 필요 - 문자열 붙이기

// ⚝WebApplication: ASP.NET Core에서 제공하는 HTTP 서버 본체
var builder = WebApplication.CreateBuilder(args);  //서버 설정/환경구성

// 콘솔 로깅 강제 활성화 + 로그 레벨 설정
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

var app = builder.Build();                         //서버 본체 생성

// ====================================================================
// 공통 요청/응답 로그 미들웨어
// ====================================================================

app.Use(async (context, next) =>
{
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

    //[Debug] 요청 메서드/경로/쿼리 로그
    logger.LogInformation("[REQ] {Method} {Path}{QueryString}",
        context.Request.Method,
        context.Request.Path,
        context.Request.QueryString.HasValue ? context.Request.QueryString.Value : "");

    await next();

    //[Debug] 응답 상태 코드 로그
    logger.LogInformation("[RES] {StatusCode} {Method} {Path}",
        context.Response.StatusCode,
        context.Request.Method,
        context.Request.Path);
});

// ====================================================================
// 1) 헬스 체크 (/health)
// ====================================================================

app.MapGet("/health", (ILogger<Program> logger) =>
{
    //[Debug] 헬스체크 호출 로그
    logger.LogInformation("[Health] /health called");

    return Results.Ok(new { status = "ok" });
});
/*
MapGet ("경로",핸들러함수) /health 경로로 Get 요청이 오면 핸들러함수를 실행
() => 람다식(익명함수) 사용 ()는 빈 함수 리턴 객체가 자동으로 JSON 변환되어 응답됨
*/

// ====================================================================
// 2) 플레이어 접속 (/connect)
// 클라에서 JSON body로 { "playerName": "seonseo" } 보냄
//    - playerName을 받으면 playerId 만들어서 SessionStore에 저장
// ====================================================================

app.MapPost("/connect", (ConnectRequest req, ILogger<Program> logger) =>
{
    // ⚝Model Binding : 
    // 클라이언트의 JSON body의 "playerName" 필드를 ConnectRequest.PlayerName에 자동 매핑

    //[Debug] 접속 요청 바디 로그
    logger.LogInformation("[Connect] request playerName={PlayerName}", req.PlayerName);

    // playerName 유효성 검사
    if (string.IsNullOrWhiteSpace(req.PlayerName))
    {
        //[Debug] 잘못된 playerName 로그
        logger.LogWarning("[Connect] invalid playerName (empty or whitespace)");
        return Results.BadRequest(new { error = "playerName is required" });
    }

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

    //[Debug] 생성된 세션 정보 로그
    logger.LogInformation("[Connect] created session playerId={PlayerId}, playerName={PlayerName}, connectedAt={ConnectedAt}",
        session.PlayerId, session.PlayerName, session.ConnectedAt);

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

app.MapGet("/players", (ILogger<Program> logger) =>
{
    var players = SessionStore.Players.Values.ToList();

    //[Debug] 현재 플레이어 수 + 간단 목록 로그
    logger.LogInformation("[Players] count={Count}, players={Players}",
        players.Count,
        players.Select(p => new { p.PlayerId, p.PlayerName }));

    //SessionStore.Players.Values : Dictionary의 value들(PlayerSession 객체들)만 반환
    //Json 배열로 응답 -> 어케함? 어찌알고 함? 의문@@@
    return Results.Ok(players);
});

// ====================================================================
// 4) 방 생성, 참가(/room/create, /room/join)
// ====================================================================

app.MapPost("/room/create", (CreateRoomRequest req, ILogger<Program> logger) =>
{
    //[Debug] 방 생성 요청 로그
    logger.LogInformation("[RoomCreate] request playerId={PlayerId}", req.PlayerId);

    // 1) playerId 존재하는지 확인
    if (!SessionStore.Players.ContainsKey(req.PlayerId))
    {
        //[Debug] 잘못된 playerId 로그
        logger.LogWarning("[RoomCreate] invalid playerId={PlayerId}", req.PlayerId);
        return Results.BadRequest(new { error = "Invalid playerId" });
    }

    // 2) 새 방 객체 생성
    var room = new GameRoom
    {
        RoomId = Guid.NewGuid().ToString("N"),      // 고유한 방 ID 생성
        InviteCode = InviteCodeGenerator.Generate(6),// 6자리 초대코드 생성
        HostId = req.PlayerId             // [NEW] 방장 = 방 만든 사람
    };

    // 3) 방 만든 사람은 자동으로 입장 (players 리스트에 추가)
    room.Players.Add(req.PlayerId);

    // 4) 방 생성 시점에서는 턴을 정하지 않고 게임 시작시 결정 -> /game/start에서 결정
    room.CurrentTurn = null;

    // 5) RoomStore에 방 등록 (roomID -> GameRoom 객체 매핑)
    RoomStore.Rooms[room.RoomId] = room;

    //[Debug] 생성된 방 정보 로그
    logger.LogInformation("[RoomCreate] roomId={RoomId}, hostId={HostId}, inviteCode={InviteCode}, players={Players}, maxPlayers={MaxPlayers}",
        room.RoomId, room.HostId, room.InviteCode, string.Join(",", room.Players), room.MaxPlayers);

    var playerInfos = PlayerMapper.ToPlayerInfos(room.Players);

    // 6) 클라이언트에게 방 정보와 초대코드 알려주기
    return Results.Ok(new
    {
        room.RoomId,            //방 ID
        room.InviteCode,        //초대 코드
        players = room.Players, //방에 있는 플레이어 ID 리스트
        playerInfos,             // 이름 포함 리스트
        room.MaxPlayers,        //최대 플레이어 수 (3)
        room.IsFull,            //방이 가득 찼는지 여부
        room.CurrentTurn        //null 반환
    });
});


// 방 입장 (/room/join)
app.MapPost("/room/join", (JoinRoomByCodeRequest req, ILogger<Program> logger) =>
{
    //[Debug] 방 입장 요청 로그
    logger.LogInformation("[RoomJoin] request inviteCode={InviteCode}, playerId={PlayerId}",
        req.InviteCode, req.PlayerId);

    // 1) 초대코드로 방 찾기 (대소문자 무시)
    var room = RoomStore.Rooms.Values
        .FirstOrDefault(r =>
            string.Equals(r.InviteCode, req.InviteCode, StringComparison.OrdinalIgnoreCase));

    if (room == null)
    {
        //[Debug] 방 없음 로그
        logger.LogWarning("[RoomJoin] room not found for inviteCode={InviteCode}", req.InviteCode);
        return Results.NotFound(new { error = "Room not found" });
    }

    var playerInfos = PlayerMapper.ToPlayerInfos(room.Players);

    // 2) playerId가 방에 존재하는지 확인
    if (room.Players.Contains(req.PlayerId)) // 있으면 그냥 ok 반환
    {
        //[Debug] 이미 방에 있는 플레이어 로그
        logger.LogInformation("[RoomJoin] player already in room roomId={RoomId}, playerId={PlayerId}",
            room.RoomId, req.PlayerId);

        return Results.Ok(new
        {
            status = "ok",
            room.RoomId,
            room.InviteCode,
            players = room.Players,
            playerInfos
        });
    }

    // 여기까지 왔다는 건, 아직 방에 안 들어간 새 플레이어라는 뜻
    // 3) 방이 가득 찼으면 결과 리턴
    if (room.IsFull)
    {
        //[Debug] 방 가득 참 로그
        logger.LogWarning("[RoomJoin] room full roomId={RoomId}, players={Players}",
            room.RoomId, string.Join(",", room.Players));
        return Results.BadRequest(new { error = "Room is full" });
    }

    // 4) 아니면 플레이어를 방에 추가
    room.Players.Add(req.PlayerId);

    //[Debug] 방 입장 성공 로그
    logger.LogInformation("[RoomJoin] joined roomId={RoomId}, inviteCode={InviteCode}, players={Players}, currentTurn={CurrentTurn}",
        room.RoomId, room.InviteCode, string.Join(",", room.Players), room.CurrentTurn);

    return Results.Ok(new
    {
        status = "ok",
        room.RoomId,
        room.InviteCode,
        Players = room.Players
    });
});

// ====================================================================
// 4-1) 방 나가기 (/room/leave)
// ====================================================================

app.MapPost("/room/leave", (LeaveRoomRequest req, ILogger<Program> logger) =>
{
    logger.LogInformation("[RoomLeave] request roomId={RoomId}, playerId={PlayerId}",
        req.RoomId, req.PlayerId);

    if (!RoomStore.Rooms.TryGetValue(req.RoomId, out var room))
    {
        logger.LogWarning("[RoomLeave] room not found roomId={RoomId}", req.RoomId);
        return Results.NotFound(new { error = "Room not found" });
    }

    if (!room.Players.Contains(req.PlayerId))
    {
        logger.LogWarning("[RoomLeave] player not in room roomId={RoomId}, playerId={PlayerId}",
            req.RoomId, req.PlayerId);
        return Results.BadRequest(new { error = "Player not in room" });
    }

    // 플레이어 제거
    room.Players.Remove(req.PlayerId);
    room.TurnOrder?.Remove(req.PlayerId);

    bool isOwnerChanged = false;
    string? newOwnerId = null;

    // 방장이 나갔으면 -> 남은 사람 중 첫 번째를 새 방장으로
    if (room.HostId == req.PlayerId && room.Players.Count > 0)
    {
        room.HostId = room.Players[0];
        isOwnerChanged = true;
        newOwnerId = room.HostId;
    }

    // 아무도 안 남으면 방 삭제
    if (room.Players.Count == 0)
    {
        RoomStore.Rooms.TryRemove(req.RoomId, out _);

        logger.LogInformation("[RoomLeave] room empty, removed roomId={RoomId}", req.RoomId);

        return Results.Ok(new
        {
            roomId = req.RoomId,
            playerId = req.PlayerId,
            players = Array.Empty<string>(),
            currentTurn = (string?)null,
            isOwnerChanged = false,
            newOwnerId = (string?)null
        });
    }

    // 현재 턴인 애가 나갔으면 턴 다시 설정
    if (room.CurrentTurn == req.PlayerId)
    {
        if (room.TurnOrder != null && room.TurnOrder.Count > 0)
        {
            // 그냥 턴 순서 리스트 첫 번째로 돌려버리기
            room.CurrentTurn = room.TurnOrder[0];
        }
        else if (room.Players.Count > 0)
        {
            // TurnOrder가 아직 안 세팅돼있으면 Players[0] 사용
            room.CurrentTurn = room.Players[0];
        }
        else
        {
            room.CurrentTurn = null;
        }
    }

    logger.LogInformation("[RoomLeave] success roomId={RoomId}, players={Players}, currentTurn={CurrentTurn}, isOwnerChanged={IsOwnerChanged}, newOwnerId={NewOwnerId}",
        room.RoomId, string.Join(",", room.Players), room.CurrentTurn, isOwnerChanged, newOwnerId);

    return Results.Ok(new
    {
        roomId = room.RoomId,
        playerId = req.PlayerId,
        players = room.Players,
        currentTurn = room.CurrentTurn,
        isOwnerChanged,
        newOwnerId
    });
});


// ====================================================================
// 5) 방 상태 조회 (/room/state/{roomId})
// ====================================================================

app.MapGet("/room/state/{roomId}", (string roomId, ILogger<Program> logger) =>
{
    //[Debug] 방 상태 조회 요청 로그
    logger.LogInformation("[RoomState] request roomId={RoomId}", roomId);

    // roomId로 방 찾기 (TryGetValue: 있으면 true, 없으면 false)
    if (!RoomStore.Rooms.TryGetValue(roomId, out var room))
    {
        //[Debug] 방 없음 로그
        logger.LogWarning("[RoomState] room not found roomId={RoomId}", roomId);
        return Results.NotFound(new { error = "Room not found" });
    }

    //[Debug] 방 상태 응답 로그
    logger.LogInformation(
        "[RoomState] roomId={RoomId}, inviteCode={InviteCode}, players={Players}, isFull={IsFull}, currentTurn={CurrentTurn}",
        room.RoomId,
        room.InviteCode,
        string.Join(",", room.Players),
        room.IsFull,
        room.CurrentTurn
    );

    var playersInfos = PlayerMapper.ToPlayerInfos(room.Players);

    // GameRoom 전체를 그대로 돌려주는 대신 필요한 필드만 선택해서 익명 객체로 반환
    return Results.Ok(new
    {
        room.RoomId,
        room.InviteCode,
        players = room.Players,
        playersInfos,
        room.MaxPlayers,
        CurrentTurn = room.CurrentTurn,
        room.IsFull
    });
});

// ====================================================================
// 6) 게임 시작 (/game/start)
// ====================================================================

app.MapPost("/game/start", (GameStartRequest req, ILogger<Program> logger) =>
{
    //[Debug] 게임 시작 요청 로그
    logger.LogInformation("[GameStart] request roomId={RoomId}, playerId={PlayerId}",
        req.RoomId, req.PlayerId);

    if (!RoomStore.Rooms.TryGetValue(req.RoomId, out var room))
    {
        //[Debug] 방 없음 로그
        logger.LogWarning("[GameStart] room not found roomId={RoomId}", req.RoomId);
        return Results.BadRequest(new { error = "Room not found" });
    }

    if (!room.Players.Contains(req.PlayerId))
    {
        //[Debug] 방에 없는 플레이어 로그
        logger.LogWarning("[GameStart] player not in room roomId={RoomId}, playerId={PlayerId}",
            req.RoomId, req.PlayerId);
        return Results.BadRequest(new { error = "Player not in room" });
    }

    if (room.Players.Count < 2)
    {
        //[Debug] 인원 부족 로그
        logger.LogWarning("[GameStart] not enough players roomId={RoomId}, count={Count}",
            req.RoomId, room.Players.Count);
        return Results.BadRequest(new { error = "Need at least 2 players to start" });
    }

    if (room.GameStarted)
    {
        //[Debug] 이미 시작된 게임 로그
        logger.LogInformation("[GameStart] already started roomId={RoomId}", req.RoomId);
        return Results.BadRequest(new { error = "Game already started" });
    }

    // 방장 정보가 HostId에 있고, Players 리스트는 [방장, 2번, 3번] 순으로 유지된다고 가정
    var ordered = room.Players
        .OrderBy(p => p == room.HostId ? 0 : 1)            // 방장 먼저
        .ThenBy(p => room.Players.IndexOf(p))              // 그 다음 입장 순서
        .ToList();

    room.TurnOrder = ordered;
    room.CurrentTurn = room.TurnOrder[0];    // 항상 방장부터
    room.GameStarted = true;

    //[Debug] 턴 순서/첫 플레이어 로그
    logger.LogInformation("[GameStart] started roomId={RoomId}, hostId={HostId}, players={Players}, turnOrder={TurnOrder}, firstPlayer={FirstPlayer}",
        room.RoomId,
        room.HostId,
        string.Join(",", room.Players),
        string.Join(",", room.TurnOrder),
        room.CurrentTurn);

    var playerInfos     = PlayerMapper.ToPlayerInfos(room.Players);
    var turnOrderInfos  = PlayerMapper.ToPlayerInfos(room.TurnOrder);
    
    // 클라이언트들에게 턴 순서와 첫 플레이어 정보를 알려준다
    return Results.Ok(new
    {
        room.RoomId,
        room.InviteCode,

        // ID 기반 기존 필드
        players = room.Players,
        turnOrder = room.TurnOrder,

        // 이름 포함 DTO 리스트 버전~!!
        playerInfos,
        turnOrderInfos,

        firstPlayer = room.TurnOrder[0],
        CurrentTurn = room.CurrentTurn
    });
});

// ===============================================
//  /choice : 한 플레이어가 선 하나 그리기
// ===============================================
app.MapPost("/choice", (ChoiceRequest req, ILogger<Program> logger) =>
{
    logger.LogInformation(
        "[Choice] roomId={RoomId}, playerId={PlayerId}, isHorizontal={IsHorizontal}, row={Row}, col={Col}",
        req.RoomId, req.PlayerId, req.IsHorizontal, req.Row, req.Col);

    // 1) 방 찾기
    if (!RoomStore.Rooms.TryGetValue(req.RoomId, out var room))
    {
        logger.LogWarning("[Choice] room not found roomId={RoomId}", req.RoomId);
        return Results.BadRequest(new { status = "error", errorCode = "Room not found" });
    }

    // 2) 플레이어 체크
    if (!room.Players.Contains(req.PlayerId))
    {
        logger.LogWarning("[Choice] player not in room roomId={RoomId}, playerId={PlayerId}",
            req.RoomId, req.PlayerId);
        return Results.BadRequest(new { status = "error", errorCode = "Player not in room" });
    }

    // 3) 게임 시작 여부
    if (!room.GameStarted)
    {
        logger.LogWarning("[Choice] game not started roomId={RoomId}", req.RoomId);
        return Results.BadRequest(new { status = "error", errorCode = "Game not started" });
    }

    // 4) 턴 체크
    if (room.CurrentTurn != req.PlayerId)
    {
        logger.LogWarning("[Choice] not your turn roomId={RoomId}, currentTurn={CurrentTurn}, playerId={PlayerId}",
            req.RoomId, room.CurrentTurn, req.PlayerId);

        return Results.BadRequest(new
        {
            status = "error",
            errorCode = "Not your turn",
            currentTurnPlayerId = room.CurrentTurn
        });
    }

    // 5) 이벤트 시퀀스 할당
    var seq = room.NextSeq++;

    var ev = new GameMoveEvent
    {
        Seq = seq,
        RoomId = room.RoomId,
        PlayerId = req.PlayerId,
        IsHorizontal = req.IsHorizontal,
        Row = req.Row,
        Col = req.Col
    };

    room.Events.Add(ev);

    // 6) 턴 넘기기 (단순 라운드 로빈)
    if (room.TurnOrder is not null && room.TurnOrder.Count > 0)
    {
        var idx = room.TurnOrder.IndexOf(req.PlayerId);
        if (idx < 0) idx = 0;
        var nextIdx = (idx + 1) % room.TurnOrder.Count;
        room.CurrentTurn = room.TurnOrder[nextIdx];
    }

    logger.LogInformation(
        "[Choice] success roomId={RoomId}, seq={Seq}, nextTurn={NextTurn}",
        room.RoomId, seq, room.CurrentTurn);

    // 지금은 madeBoxes/boardCompleted는 클라에서 계산하도록 비워둠
    return Results.Ok(new
    {
        status = "ok",
        roomId = room.RoomId,
        moveSeq = seq,
        move = new
        {
            playerId = ev.PlayerId,
            isHorizontal = ev.IsHorizontal,
            row = ev.Row,
            col = ev.Col
        },
        madeBoxes = Array.Empty<object>(),
        nextTurnPlayerId = room.CurrentTurn,
        boardCompleted = false
    });
});


// ===============================================
//  /draw : afterSeq 이후의 모든 선 이벤트 조회
// ===============================================
app.MapGet("/draw", (string roomId, long? afterSeq, ILogger<Program> logger) =>
{
    var startSeq = afterSeq ?? 0L;

    logger.LogInformation(
        "[Draw] request roomId={RoomId}, afterSeq={AfterSeq}",
        roomId, startSeq);

    if (!RoomStore.Rooms.TryGetValue(roomId, out var room))
    {
        logger.LogWarning("[Draw] room not found roomId={RoomId}", roomId);
        return Results.BadRequest(new { status = "error", errorCode = "ROOM_NOT_FOUND" });
    }

    // 해당 시퀀스 이후 이벤트만
    var events = room.Events
        .Where(e => e.Seq > startSeq)
        .OrderBy(e => e.Seq)
        .Select(e => new
        {
            seq = e.Seq,
            playerId = e.PlayerId,
            isHorizontal = e.IsHorizontal,
            row = e.Row,
            col = e.Col,
            madeBoxes = Array.Empty<object>()
        })
        .ToList();

    var lastSeq = events.Count > 0 ? events[^1].seq : startSeq;

    logger.LogInformation(
        "[Draw] response roomId={RoomId}, count={Count}, lastSeq={LastSeq}",
        roomId, events.Count, lastSeq);

    return Results.Ok(new
    {
        roomId,
        events,
        lastSeq
    });
});



app.Run();

// ====================================================================
//                              타입 정의 구역
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

// 클라이언트에 내려줄 플레이어 정보 DTO
public class PlayerInfoDto
{
    public string PlayerId { get; set; } = default!;
    public string PlayerName { get; set; } = default!;
}

// SessionStore에 있는 세션 -> DTO로 매핑
public static class PlayerMapper
{
    public static List<PlayerInfoDto> ToPlayerInfos(IEnumerable<string> playerIds)
    {
        var result = new List<PlayerInfoDto>();

        foreach (var id in playerIds)
        {
            if (SessionStore.Players.TryGetValue(id, out var session))
            {
                result.Add(new PlayerInfoDto
                {
                    PlayerId = session.PlayerId,
                    PlayerName = session.PlayerName
                });
            }
            else
            {
                // 세션에서 못 찾는 경우 대비 (이름 모르면 id만 전달)
                result.Add(new PlayerInfoDto
                {
                    PlayerId = id,
                    PlayerName = "(unknown)"
                });
            }
        }

        return result;
    }
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

    // [NEW] 방장 ID
    public string HostId { get; set; } = default!;

    // 방 안 플레이어 ID들 (최대 3명)
    public List<string> Players { get; set; } = new();

    public int MaxPlayers { get; set; } = 3;  

    // 게임이 시작되었는지 여부
    public bool GameStarted { get; set; } = false;

    // 방장 + 입장 순서 기준 턴 순서 (playerId 리스트)
    public List<string> TurnOrder { get; set; } = null!; //null!이면 나중에 할당할 테니까 경고 무시 해줘 라는 뜻

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // 좌표 이벤트 로그
    public List<GameMoveEvent> Events { get; set; } = new();

    public long NextSeq { get; set; } = 1;  // 이벤트 시퀀스 번호

    // 현재 인원수가 MaxPlayer에 도달했으면 true 반환
    public bool IsFull => Players.Count >= MaxPlayers;

    public string? CurrentTurn { get; set;  } //현재 턴인 플레이어 ID (게임 시작 전에는 null)
}

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

// (/room/leave 요청 Body 모델)
public class LeaveRoomRequest
{
    public string RoomId { get; set; } = default!;
    public string PlayerId { get; set; } = default!;
}

// (/choice 요청 Body 모델)
public class ChoiceRequest
{
    public string RoomId { get; set; } = default!;
    public string PlayerId { get; set; } = default!;
    public bool IsHorizontal { get; set; }
    public int Row { get; set; }
    public int Col { get; set; }
}

// 게임 진행 중 하나의 "선 그리기" 이벤트
public class GameMoveEvent
{
    public long Seq { get; set; }
    public string RoomId { get; set; } = default!;
    public string PlayerId { get; set; } = default!;
    public bool IsHorizontal { get; set; }
    public int Row { get; set; }
    public int Col { get; set; }
}
