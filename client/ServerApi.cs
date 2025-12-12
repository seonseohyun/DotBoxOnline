using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Diagnostics; //debug출력
using System.Windows.Forms;

namespace DotsAndBoxes
{
    // /connect 응답 DTO
    public class ConnectResponse
    {
        public string playerId { get; set; }
        public string playerName { get; set; }
        public string connectedAt { get; set; }
    }
    
    // PlayerInfo DTO 
    public class PlayerInfo
    {
        public string playerId { get; set; }
        public string playerName { get; set; }
    }
    
    // /room/create 응답 DTO
    public class CreateRoomResponse
    {
        public string roomId { get; set; }
        public string inviteCode { get; set; }
        public string[] players { get; set; }
        public int maxPlayers { get; set; }
        public bool isFull { get; set; }
        public string currentTurn { get; set; }
        public PlayerInfo[] playerInfos { get; set; } // 플레이어의 ID + 닉네임 정보
    }

    // /room/join 응답 DTO
    public class JoinRoomResponse
    {
        public string status { get; set; }    // "ok"
        public string roomId { get; set; }
        public string inviteCode { get; set; }
        public string[] players { get; set; }   // ← 이 프로퍼티가 반드시 있어야 joinRes.players가 컴파일됨
        public int maxPlayers { get; set; }
        public bool isFull { get; set; }
        public string currentTurn { get; set; }
        public PlayerInfo[] playerInfos { get; set; }
    }

    // /room/state 응답 DTO
    public class RoomStateResponse
    {
        public string roomId { get; set; }
        public string inviteCode { get; set; }
        public string[] players { get; set; }
        public bool isFull { get; set; }
        public string currentTurn { get; set; }// 게임 시작 여부 확인
        public PlayerInfo[] playerInfos { get; set; }
        public PlayerInfo[] playersInfos { get; set; }// /room/state에서 내려오는 playersInfos용
        public int gameRound { get; set; }
    }

    // /game/start 응답 DTO
    public class GameStartResponse
    {
        public string roomId { get; set; }
        public string inviteCode { get; set; }
        public string[] players { get; set; }
        public string[] turnOrder { get; set; }
        public string firstPlayer { get; set; }
        public string currentTurn { get; set; }
        public PlayerInfo[] playerInfos { get; set; }
    }

    // /room/leave 응답 DTO
    public class LeaveRoomResponse
    {
        public string roomId { get; set; }
        public string playerId { get; set; }
        public string[] players { get; set; }
        public string currentTurn { get; set; }
        public bool isOwnerChanged { get; set; }
        public string newOwnerId { get; set; }
        public PlayerInfo[] playerInfos { get; set; }
    }

    // /choice 프로토콜 DTO
    public class ChoiceRequest
    {
        public string roomId { get; set; }
        public string playerId { get; set; }
        public bool isHorizontal { get; set; }
        public int row { get; set; }
        public int col { get; set; }
    }

    public class ChoiceMove
    {
        public string playerId { get; set; }
        public bool isHorizontal { get; set; }
        public int row { get; set; }
        public int col { get; set; }
    }

    public class ChoiceResponse
    {
        public string status { get; set; }
        public string roomId { get; set; }
        public long moveSeq { get; set; }
        public ChoiceMove move { get; set; }
        public List<object> madeBoxes { get; set; }
        public string nextTurnPlayerId { get; set; }
        public bool boardCompleted { get; set; }

        // 에러용 필드
        public string errorCode { get; set; }
        public string currentTurnPlayerId { get; set; }
    }


    // /draw 이벤트/응답 DTO
    public class DrawEvent
    {
        public long seq { get; set; }
        public string playerId { get; set; }
        public bool isHorizontal { get; set; }
        public int row { get; set; }
        public int col { get; set; }
        public List<object> madeBoxes { get; set; }
    }

    public class DrawResponse
    {
        public string roomId { get; set; }
        public int gameRound { get; set; }
        public List<DrawEvent> events { get; set; }
        public long lastSeq { get; set; }
    }


    /// HTTP 서버와의 통신을 전담하는 정적 클래스
    public static class ServerApi
    {
        // 모든 통신에서 재사용할 HttpClient
        private static readonly HttpClient httpClient = new HttpClient
        {
            BaseAddress = new Uri("http://43.201.40.98:8080")
            //BaseAddress = new Uri("http://localhost:5217")
        };

        /// /connect 호출해서 플레이어 세션 발급
        public static async Task<ConnectResponse> ConnectAsync(string nickname)
        {
            var requestObj = new
            {
                playerName = nickname
            };

            string json = JsonConvert.SerializeObject(requestObj);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response;
            string body;

            try
            {
                // ★ static httpClient + 상대 경로 사용
                response = await httpClient.PostAsync("/connect", content);
                body = await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"서버에 연결할 수 없습니다. ({ex.Message})");
            }

            if (!response.IsSuccessStatusCode)
            {
                TryThrowError(body); // 이미 만들어둔 에러 파싱 함수 있지?
            }

            try
            {
                var connectRes = JsonConvert.DeserializeObject<ConnectResponse>(body);
                if (connectRes == null || string.IsNullOrEmpty(connectRes.playerId))
                {
                    throw new Exception("Connect 응답이 올바르지 않습니다.");
                }

                return connectRes;
            }
            catch (JsonException)
            {
                throw new Exception("Connect 응답 파싱 중 오류가 발생했습니다.");
            }
        }

        /// /room/create : 방 생성
        public static async Task<CreateRoomResponse> CreateRoomAsync(string playerId, int maxPlayers)
        {
            var requestObj = new
            {
                playerId = playerId,
                maxPlayers = maxPlayers // 서버로 인원 수 전송
            };

            string json = JsonConvert.SerializeObject(requestObj);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response;
            string body;

            try
            {
                response = await httpClient.PostAsync("/room/create", content);
                body = await response.Content.ReadAsStringAsync();

                //@ 디버그 출력
                Debug.WriteLine("[ROOM_CREATE] " + body);
            }
            catch (Exception ex)
            {
                throw new Exception($"방을 생성할 수 없습니다. ({ex.Message})");
            }

            if (!response.IsSuccessStatusCode)
            {
                TryThrowError(body);
            }

            try
            {
                var roomRes = JsonConvert.DeserializeObject<CreateRoomResponse>(body);
                if (roomRes == null || string.IsNullOrEmpty(roomRes.roomId))
                {
                    throw new Exception("방 생성 응답이 올바르지 않습니다.");
                }

                return roomRes;
            }
            catch (JsonException)
            {
                throw new Exception("방 생성 응답 파싱 중 오류가 발생했습니다.");
            }
        }

        /// /room/join : 초대코드로 방 참가
        public static async Task<JoinRoomResponse> JoinRoomAsync(string playerId, string inviteCode)
        {
            var requestObj = new
            {
                playerId = playerId,
                inviteCode = inviteCode
            };

            string json = JsonConvert.SerializeObject(requestObj);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response;
            string body;

            try
            {
                response = await httpClient.PostAsync("/room/join", content);
                body = await response.Content.ReadAsStringAsync();

                // @ 디버그 출력
                Debug.WriteLine("[ROOM_JOIN] " + body);
            }
            catch (Exception ex)
            {
                throw new Exception($"방에 참가할 수 없습니다. ({ex.Message})");
            }

            if (!response.IsSuccessStatusCode)
            {
                TryThrowError(body);
            }

            try
            {
                var joinRes = JsonConvert.DeserializeObject<JoinRoomResponse>(body);
                if (joinRes == null || joinRes.status != "ok")
                {
                    throw new Exception("방 참가 응답이 올바르지 않습니다.");
                }

                return joinRes;
            }
            catch (JsonException)
            {
                throw new Exception("방 참가 응답 파싱 중 오류가 발생했습니다.");
            }
        }

        /// /room/state : 방 상태 확인
        public static async Task<RoomStateResponse> GetRoomStateAsync(string roomId)
        {
            HttpResponseMessage response;
            string body;

            try
            {
                response = await httpClient.GetAsync($"/room/state/{roomId}");
                body = await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"방 상태를 조회할 수 없습니다. ({ex.Message})");
            }

            if (!response.IsSuccessStatusCode)
            {
                TryThrowError(body);
            }

            try
            {
                var state = JsonConvert.DeserializeObject<RoomStateResponse>(body);
                if (state == null || string.IsNullOrEmpty(state.roomId))
                {
                    throw new Exception("방 상태 응답이 올바르지 않습니다.");
                }
                return state;
            }
            catch
            {
                throw new Exception("방 상태 응답 파싱 중 오류가 발생했습니다.");
            }
        }

        /// /game/start : 게임 시작
        public static async Task<GameStartResponse> GameStartAsync(string roomId, string playerId)
        {
            var requestObj = new
            {
                roomId = roomId,
                playerId = playerId
            };

            string json = JsonConvert.SerializeObject(requestObj);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response;
            string body;

            try
            {
                response = await httpClient.PostAsync("/game/start", content);
                body = await response.Content.ReadAsStringAsync();
                Debug.WriteLine("[GAME_START] " + body);
            }
            catch (Exception ex)
            {
                throw new Exception($"게임을 시작할 수 없습니다. ({ex.Message})");
            }

            if (!response.IsSuccessStatusCode)
            {
                // 400 기준 에러: Room not found, Player not in room, Need at least 2 players, Game already started
                TryThrowError(body);
            }

            try
            {
                var startRes = JsonConvert.DeserializeObject<GameStartResponse>(body);
                if (startRes == null || string.IsNullOrEmpty(startRes.roomId))
                {
                    throw new Exception("게임 시작 응답이 올바르지 않습니다.");
                }
                return startRes;
            }
            catch (JsonException)
            {
                throw new Exception("게임 시작 응답 파싱 중 오류가 발생했습니다.");
            }
        }

        // POST /choice : 선 긋기 요청
        public static async Task<ChoiceResponse> SendChoiceAsync(ChoiceRequest req)
        {
            string json = JsonConvert.SerializeObject(req);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var resp = await httpClient.PostAsync("/choice", content);

            if (!resp.IsSuccessStatusCode)
                return null;

            var body = await resp.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<ChoiceResponse>(body);

            return result;
        }

        // GET /draw : 선 이벤트 조회
        public static async Task<DrawResponse> GetDrawEventsAsync(string roomId, long afterSeq)
        {
            string url = $"/draw?roomId={roomId}&afterSeq={afterSeq}";
            var resp = await httpClient.GetAsync(url);
            if (!resp.IsSuccessStatusCode)
                return null;

            var body = await resp.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<DrawResponse>(body);

            return result;
        }

        /// /room/leave : 방 나가기
        public static async Task<LeaveRoomResponse> LeaveRoomAsync(string roomId, string playerId)
        {
            var requestObj = new
            {
                roomId = roomId,
                playerId = playerId
            };

            string json = JsonConvert.SerializeObject(requestObj);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response;
            string body;

            try
            {
                response = await httpClient.PostAsync("/room/leave", content);
                body = await response.Content.ReadAsStringAsync();

                //@ 디버그 출력
                Debug.WriteLine("[ROOM_LEAVE] " + body);
            }
            catch (Exception ex)
            {
                throw new Exception($"방에서 나갈 수 없습니다. ({ex.Message})");
            }

            if (!response.IsSuccessStatusCode)
            {
                // 400, 404 등은 여기서 공통 처리
                TryThrowError(body);
            }

            try
            {
                var leaveRes = JsonConvert.DeserializeObject<LeaveRoomResponse>(body);
                if (leaveRes == null || string.IsNullOrEmpty(leaveRes.roomId))
                {
                    throw new Exception("방 나가기 응답이 올바르지 않습니다.");
                }

                return leaveRes;
            }
            catch (JsonException)
            {
                throw new Exception("방 나가기 응답 파싱 중 오류가 발생했습니다.");
            }
        }

        /// 공통 에러 파싱: { "error": "메시지" } 형식 처리
        private static void TryThrowError(string body)
        {
            try
            {
                var errObj = JsonConvert.DeserializeObject<dynamic>(body);
                string errMsg = errObj?.error != null ? (string)errObj.error : "알 수 없는 서버 오류가 발생했습니다.";
                throw new Exception(errMsg);
            }
            catch (JsonException)
            {
                throw new Exception("서버 응답을 해석할 수 없습니다.");
            }
        }
    }
}
