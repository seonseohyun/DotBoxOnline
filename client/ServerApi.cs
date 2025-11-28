using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Diagnostics; //debug출력

namespace DotsAndBoxes
{
    // /connect 응답 DTO
    public class ConnectResponse
    {
        public string playerId { get; set; }
        public string playerName { get; set; }
        public string connectedAt { get; set; }
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
    }

    // /room/join 응답 DTO
    public class JoinRoomResponse
    {
        public string status { get; set; }    // "ok"
        public string roomId { get; set; }
        public string inviteCode { get; set; }
    }

    // /room/state 응답 DTO
    public class RoomStateResponse
    {
        public string roomId { get; set; }
        public string inviteCode { get; set; }
        public string[] players { get; set; }
        public bool isFull { get; set; }
    }

 
    /// HTTP 서버와의 통신을 전담하는 정적 클래스
    public static class ServerApi
    {
        // 모든 통신에서 재사용할 HttpClient
        private static readonly HttpClient httpClient = new HttpClient
        {
            BaseAddress = new Uri("http://43.201.40.98:8080")
        };


        /// /connect 호출해서 플레이어 세션 발급
        /// 실패 시 Exception(error 메시지) 던짐
        public static async Task<ConnectResponse> ConnectAsync(string playerName)
        {
            var requestObj = new
            {
                playerName = playerName
            };

            string json = JsonConvert.SerializeObject(requestObj);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response;
            string body;

            try
            {
                response = await httpClient.PostAsync("/connect", content);
                body = await response.Content.ReadAsStringAsync();

                //@디버그 출력
                Debug.WriteLine("[CONNECT] " + body);
            }
            catch (Exception ex)
            {
                // 네트워크 자체 오류 (서버 다운, 타임아웃 등)
                throw new Exception($"서버에 접속할 수 없습니다. ({ex.Message})");
            }

            if (!response.IsSuccessStatusCode)
            {
                TryThrowError(body);
            }

            try
            {
                var connectRes = JsonConvert.DeserializeObject<ConnectResponse>(body);
                if (connectRes == null || string.IsNullOrEmpty(connectRes.playerId))
                {
                    throw new Exception("서버 응답이 올바르지 않습니다.");
                }

                return connectRes;
            }
            catch (JsonException)
            {
                throw new Exception("서버 응답 파싱 중 오류가 발생했습니다.");
            }
        }

        /// /room/create : 방 생성
        public static async Task<CreateRoomResponse> CreateRoomAsync(string playerId)
        {
            var requestObj = new
            {
                playerId = playerId
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
