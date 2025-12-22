## DotBox 서버 프로토콜 명세서 (API Specification)

| 구분 | 내용 |
| :--- | :--- |
| **최종 수정일** | 2025년 12월 09일 |
| **작성자** | 선서현 |
| **URL** | `http://43.201.40.98:8080` |

---

## 0. 공통 규칙 (General Rules)


* **데이터 형식:** 모든 요청 및 응답은 **JSON**을 사용합니다.
* **요청 헤더:** Body가 있는 `POST` 요청 시 `Content-Type: application/json` 필수.
* **응답 형식:** 별도의 래퍼(`status`, `data` 등) 없이 바로 객체 또는 배열을 내려줍니다.
* **시간 형식:** `DateTime` 은 모두 **UTC 기준 ISO-8601 문자열**로 내려옵니다. (예: `"2025-11-27T01:23:45.678Z"`)
* **공통 에러 응답 형식:**
  * 잘못된 요청, 유효하지 않은 ID, 조건 불충족 등:
    ```json
    { "error": "에러 메세지" }
    ```
  * HTTP Status Code는 상황에 따라 `400 Bad Request`, `404 Not Found` 등을 사용.
  * **로그:** 모든 요청에 대해 `[REQ] 메서드/경로/쿼리`, 응답에 대해 `[RES] 상태코드/메서드/경로`를 서버 로그로 남깁니다.

* **공통: playerInfos 필드:**  
  방 관련 API 응답에서는 다음 두 가지 필드가 함께 내려옵니다.

  ```json
  {
    "players": [
      "player-id-1",
      "player-id-2"
    ],
    "playerInfos": [
      {
        "playerId": "player-id-1",
        "playerName": "플레이어1"
      },
      {
        "playerId": "player-id-2",
        "playerName": "플레이어2"
      }
    ]
  }
  ```
  | 필드명                      | 타입       | 설명                            |
  | ------------------------ | -------- | ----------------------------- |
  | players                  | string[] | 방에 속한 플레이어의 `playerId` 목록     |
  | playerInfos              | object[] | 각 플레이어의 ID와 이름 정보 목록          |
  | playerInfos[].playerId   | string   | 플레이어 ID (`players` 배열의 값과 동일) |
  | playerInfos[].playerName | string   | 플레이어 닉네임                      |

---

## 1. 서버 헬스 체크 (GET /health)
서버가 현재 동작 중인지 확인합니다.

| 속성        | 내용       |
| :---       | :---      |
| **Method** | `GET`     |
| **Path**   | `/health` |
| **Request Body** | 없음 |

### Request:
* HTTP Raw 예시 
  ```http
  GET /health HTTP/1.1
  Host: 43.201.40.98:8080
  Accept: */*
  ```

* curl 예시
  ```curl
  curl http://43.201.40.98:8080/health
  ```

### Response:
```json
{ "status": "ok" }
```
---

## 2. 플레이어 접속 (POST /connect)
플레이어가 서버에 접속하면 고유한 PlayerId를 발급
| 속성               | 내용                           |
| :--------------- | :----------------------------- |
| **Method**       | `POST`                         |
| **Path**         | `/connect`                     |
| **Content-Type** | `application/json`             |
| **Request Body** | `ConnectRequest JSON`          |
| **Response**     | Player 세션 정보(JSON)           |
| **성공 HTTP Status** | `200 OK`              |
| **실패 HTTP Status** | `400 Bad Request`     |

### Request Body:
```json
{
  "playerName": "seonseo"
}
```
| 필드명        | 타입     | 필수 | 설명                           |
| ---------- | ------ | -- | ------------------------         |
| playerName | string | O  | 플레이어 닉네임 (공백 또는 빈문자열 불가) |

### Response:
* 성공 (200 OK)
  ```json
  {
    "playerId": "8f8b16c9f2e44f1f9a9e4a7e4d1c2b3",
    "playerName": "seonseo",
    "connectedAt": "2025-11-27T01:23:45.678Z"
  }
  ```
    | 필드명       | 타입     | 설명                        |
    | ----------- | ------ | ------------------------- |
    | playerId    | string | 서버에서 발급한 고유 ID (문자열)|
    | playerName  | string | 요청 시 전달한 이름           |
    | connectedAt | string | 접속 시간(UTC, ISO-8601)      |

* 실패 (400)
    
  ```json
  // playerName이 공백이거나 null, 빈 문자열인 경우
  { "error": "playerName is required" }
  ```
------

## 3. 현재 접속 플레이어 조회 (GET /players)
서버 메모리에 저장된 모든 플레이어 세션 목록을 조회
| 속성                 | 내용                        |
| :----------------- | :-------------------------- |
| **Method**         | `GET`                       |
| **Path**           | `/players`                  |
| **Request Body**   | 없음                          |
| **Response**       | `PlayerSession[]` (JSON 배열) |
| **성공 HTTP Status** | `200 OK`                    |

### Request:
```curl
curl http://43.201.40.98:8080/players
```

### Response:
```json
[
  {
    "playerId": "8f8b16c9f2e44f1f9a9e4a7e4d1c2b3",
    "playerName": "seonseo",
    "connectedAt": "2025-11-27T01:23:45.678Z"
  }
]
```
| 필드명         | 타입     | 설명                   |
| ----------- | ------ | -------------------- |
| playerId    | string | 서버에서 생성한 고유 플레이어 ID  |
| playerName  | string | 플레이어 이름              |
| connectedAt | string | 접속 시각(UTC, ISO-8601) |

---

## 4. 방 생성 (POST /room/create)

플레이어가 새로운 게임 방을 만듦  
방 ID, 초대 코드가 생성되고, 방장은 자동으로 방에 입장 처리된다.  
방을 생성한 플레이어는 서버에서 `HostId`로 관리되며, 게임 시작 시 항상 첫 턴을 갖는 방장이 된다.


| 속성                 | 내용                  |
| :----------------- | :------------------ |
| **Method**         | `POST`              |
| **Path**           | `/room/create`      |
| **Content-Type**   | `application/json`  |
| **Request Body**   | `CreateRoomRequest` |
| **성공 HTTP Status** | `200 OK`            |
| **실패 HTTP Status** | `400 Bad Request`   |

### Request Body:
```json
{
  "playerId": "seonseo",
  "maxPlayers": 3,
  "boardIndex": 0
}
```
| 필드명      | 타입     | 필수|    설명                                     |
| ---------- | ------ | -- | ------------------------------------------ |
| playerId   | string | O  | 방 생성 플레이어 ID                            |
| maxPlayers | number | X  | 방 정원 (허용: `2` 또는 `3`, 기본값 `3`)         |
| boardIndex | number | X  | 보드 크기 선택 인덱스 (서버에서 유효성 검증) |

### Response:
* 성공 (200 OK)
  ```json
  {
    "roomId": "b1a2c3d4e5f6478390ab1c2d3e4f5a6b",
    "inviteCode": "AB3F9Z",
    "players": [
      "8f8b16c9f2e44f1f9a9e4a7e4d1c2b3"
    ],
    "playerInfos": [
      {
        "playerId": "8f8b16c9f2e44f1f9a9e4a7e4d1c2b3",
        "playerName": "seonseo"
      }
    ],
    "maxPlayers": 3,
    "isFull": false,
    "currentTurn": null,
    "boardIndex": 0
  }
  ```
| 필드명                      | 타입          | 설명                                |
| ------------------------ | ----------- | --------------------------------- |
| roomId                   | string      | 방 ID                              |
| inviteCode               | string      | 방 초대 코드                           |
| players                  | string[]    | 방에 속한 플레이어 ID 목록                  |
| playerInfos              | object[]    | 각 플레이어의 ID/이름 정보 목록               |
| playerInfos[].playerId   | string      | 플레이어 ID                           |
| playerInfos[].playerName | string      | 플레이어 닉네임                          |
| maxPlayers               | int         | 최대 인원 수 (기본 3명)                   |
| isFull                   | bool        | `players.length >= maxPlayers` 여부 |
| currentTurn              | string|null | 현재 턴인 플레이어 ID (게임 시작 전에는 `null`)  |

* 실패 (400)
  ```json
  // playerId가 존재하지 않을 때
  { "error": "Invalid playerId" }
  // 인원수 유효성 검사 에러
  { "error": "Invalid maxPlayers (allowed: 2 or 3)" }
  // 맵 크기 인덱스 유효성 검사 에러
  { "error": "Invalid boardIndex" }

  ```
---

## 5-1. 방 참가 (POST /room/join)
초대 코드를 이용해서 이미 만들어진 방에 들어감  
플레이어는 먼저 /connect로 playerId를 발급받아야 하고, 그 다음 이 API로 특정 방에 참가한다.  
동일한 playerId가 같은 inviteCode로 /room/join를 여러 번 호출해도  
방 인원 수(players)는 증가하지 않으며, 현재 방 상태만 반환한다.  

| 속성                | 내용                                |
| :----------------- | :--------------------------------- |
| **Method**         | `POST`                             |
| **Path**           | `/room/join`                       |
| **Content-Type**   | `application/json`                 |
| **Request Body**   | `JoinRoomByCodeRequest`            |
| **Response**       | 방 참가 결과(JSON)                    |
| **성공 HTTP Status** | `200 OK`                           |
| **실패 HTTP Status** | `400 Bad Request`, `404 Not Found` |

### Request Body: 
```json
{
  "playerId": "c3d2e1f0a9b8c7d6e5f4a3b2c1d0e9f8",
  "inviteCode": "AB3F9Z"
}
```
| 필드명       | 타입   | 필수 | 설명                                          |
| ---------- | ------ | -- | -------------------------------------        |
| playerId   | string | O  | 방에 참가할 플레이어 ID (`/connect` 응답에서 받은 값) |
| inviteCode | string | O  | 방 초대 코드 (대소문자 구분 안 함)                   |

### Response:
* 성공 (200 OK)
  ```json
  {
    "status": "ok",
    "roomId": "b1a2c3d4e5f6478390ab1c2d3e4f5a6b",
    "inviteCode": "AB3F9Z",
    "players": [
      "8f8b16c9f2e44f1f9a9e4a7e4d1c2b3",
      "c3d2e1f0a9b8c7d6e5f4a3b2c1d0e9f8"
    ],
    "playerInfos": [
      {
        "playerId": "8f8b16c9f2e44f1f9a9e4a7e4d1c2b3",
        "playerName": "seonseo"
      },
      {
        "playerId": "c3d2e1f0a9b8c7d6e5f4a3b2c1d0e9f8",
        "playerName": "friend"
      }
    ],
  "maxPlayers": 3,
  "isFull": false,
  "currentTurn": "string",
  }
  ```
| 필드명                      | 타입          | 설명                                     |
| :----------------------- | :---------- | :------------------------------------- |
| status                   | string      | 성공 시 `"ok"`                            |
| roomId                   | string      | 방 ID                                   |
| inviteCode               | string      | 방 초대 코드                                |
| players                  | string[]    | 방에 있는 플레이어 ID 목록                       |
| playerInfos              | object[]    | 플레이어 정보 목록                             |
| playerInfos[].playerId   | string      | 플레이어 ID                                |
| playerInfos[].playerName | string      | 플레이어 이름                                |
| maxPlayers               | number      | 방 최대 인원                                |
| isFull                   | bool        | `players.length >= maxPlayers` 여부      |
| currentTurn              | string|null | 현재 턴 플레이어 ID (게임 시작 전일 수 있어 `null` 가능) |

* 실패 
  ```json
  // 1) 초대 코드에 해당하는 방이 없을 때 - 404 Not Found
  { "error": "Room not found" }

  // 2) 방이 가득 찼을 때 - 400 Bad Request 
  { "error": "Room is full" }

  // 3) 잘못된 PlayerId
  { "error": "Invalid playerId" }

  ```
---

## 5-2. 방 나가기 (POST /room/leave)

플레이어가 현재 들어가 있는 방에서 나갈 때 사용하는 API.  
나간 사람이 방장이면, 남아 있는 사람 중 첫 번째 플레이어가 새 방장이 되며 아무도 남지 않으면 방은 삭제된다.

| 속성 | 내용 |
| :--- | :--- |
| **Method** | `POST` |
| **Path** | `/room/leave` |
| **Content-Type** | `application/json` |
| **Request Body** | `LeaveRoomRequest` |
| **Response** | 방 나가기 결과(JSON) |
| **성공 HTTP Status** | `200 OK` |
| **실패 HTTP Status** | `400 Bad Request`, `404 Not Found` |

### Request Body

```json
{
  "roomId": "b1a2c3d4e5f6478390ab1c2d3e4f5a6b",
  "playerId": "8f8b16c9f2e44f1f9a9e4a7e4d1c2b3"
}
```

| 필드명      | 타입     | 필수 | 설명             |
| -------- | ------ | -- | -------------- |
| roomId   | string | O  | 나갈 방 ID        |
| playerId | string | O  | 방에서 나갈 플레이어 ID |

### Response:
* 성공 (200 OK)
  ```json
  // 1) 방에 다른 사람이 남아 있는 경우
  {
    "roomId": "b1a2c3d4e5f6478390ab1c2d3e4f5a6b",
    "playerId": "8f8b16c9f2e44f1f9a9e4a7e4d1c2b3",
    "players": [
      "c3d2e1f0a9b8c7d6e5f4a3b2c1d0e9f8"
    ],
    "currentTurn": "c3d2e1f0a9b8c7d6e5f4a3b2c1d0e9f8",
    "isOwnerChanged": true,
    "newOwnerId": "c3d2e1f0a9b8c7d6e5f4a3b2c1d0e9f8"
  }

  // 2) 마지막 사람이 나가서 방이 삭제된 경우
  {
    "roomId": "b1a2c3d4e5f6478390ab1c2d3e4f5a6b",
    "playerId": "8f8b16c9f2e44f1f9a9e4a7e4d1c2b3",
    "players": [],
    "currentTurn": null,
    "isOwnerChanged": false,
    "newOwnerId": null
  }
  ```
  | 필드명            | 타입          | 설명                             |
  | -------------- | ----------- | ------------------------------ |
  | roomId         | string      | 방 ID                           |
  | playerId       | string      | 방에서 나간 플레이어 ID                 |
  | players        | string[]    | 방에 남아 있는 플레이어 ID 목록 (없으면 빈 배열) |
  | currentTurn    | string|null | 현재 턴 플레이어 ID (없으면 null)        |
  | isOwnerChanged | bool        | 방장이 변경되었는지 여부                  |
  | newOwnerId     | string|null | 새 방장 ID (없으면 null)             |

* 실패 
  ```json
  // 1) 방을 찾을 수 없는 경우 - 404 Not Found
  { "error": "Room not found" }

  // 2) 해당 플레이어가 그 방에 없는 경우 - 400 Bad Request
  { "error": "Player not in room" }
  ```

---

## 6. 방 상태 조회 (GET /room/state/{roomId})
특정 roomId에 해당하는 방의 현재 상태 조회
| 속성                 | 내용                     |
| :----------------- | :--------------------- |
| **Method**         | `GET`                  |
| **Path**           | `/room/state/{roomId}` |
| **Path Param**     | `roomId` (string, 필수!)|
| **Request Body**   | 없음                     |
| **Response**       | 방 상태(JSON)             |
| **성공 HTTP Status** | `200 OK`               |
| **실패 HTTP Status** | `404 Not Found`        |

### Path Param:
| 파라미터   | 타입     |  필수 | 설명   |
| :----- | :----- | :-: | :--- |
| roomId | string |  O  | 방 ID |


### Request:
* HTTP Raw 예시 
    ```http
    GET /room/state/b1a2c3d4e5f6478390ab1c2d3e4f5a6b HTTP/1.1
    Host: 43.201.40.98:8080
    Accept: application/json
    ```

* curl 예시
    ```curl
    curl -X GET "http://43.201.40.98:8080/room/state/b1a2c3d4e5f6478390ab1c2d3e4f5a6b"
    ```

### Response:
* 성공 (200 OK)
  ```json
  {
    "roomId": "b1a2c3d4e5f6478390ab1c2d3e4f5a6b",
    "inviteCode": "AB3F9Z",
    "players": [
      "8f8b16c9f2e44f1f9a9e4a7e4d1c2b3",
      "c3d2e1f0a9b8c7d6e5f4a3b2c1d0e9f8"
    ],
    "playersInfos": [
      {
        "playerId": "8f8b16c9f2e44f1f9a9e4a7e4d1c2b3",
        "playerName": "seonseo"
      },
      {
        "playerId": "c3d2e1f0a9b8c7d6e5f4a3b2c1d0e9f8",
        "playerName": "friend"
      }
    ],
    "maxPlayers": 3,
    "isFull": false,
    "currentTurn": "string",
    "boardIndex": 0,
    "gameRound": 1
  }
  ```
| 필드명                       | 타입          | 설명                                |
| :------------------------ | :---------- | :-------------------------------- |
| roomId                    | string      | 방 ID                              |
| inviteCode                | string      | 방 초대 코드                           |
| players                   | string[]    | 방에 있는 플레이어 ID 목록                  |
| playersInfos              | object[]    | 플레이어 정보 목록                        |
| playersInfos[].playerId   | string      | 플레이어 ID                           |
| playersInfos[].playerName | string      | 플레이어 이름                           |
| maxPlayers                | number      | 방 최대 인원                           |
| isFull                    | bool        | `players.length >= maxPlayers` 여부 |
| currentTurn               | string|null | 현재 턴 플레이어 ID (게임 시작 전 `null` 가능)  |
| gameRound                 | number      | 현재 게임 라운드 번호                      |
| boardIndex                | number      | 보드 크기 선택 인덱스 |

* 실패 (404 Not Found)
    ```json
    // 방이 존재하지 않을 때
    { "error": "Room not found" }
    ```
---

## 7. 게임 시작 (POST /game/start)

방에 최소 2명 이상이 있고, 게임이 아직 시작되지 않은 경우 게임을 시작하고 턴 순서 및 첫 플레이어 정보를 반환한다.  
턴 순서는 항상 `HostId`(방장)를 첫 번째로 두고, 나머지 플레이어는 방 `Players` 리스트의 입장 순서대로 정렬된다.  


| 속성                 | 내용                 |
| ------------------ | ------------------ |
| **Method**         | `POST`             |
| **Path**           | `/game/start`      |
| **Content-Type**   | `application/json` |
| **Request Body**   | `GameStartRequest` |
| **성공 HTTP Status** | `200 OK`           |
| **실패 HTTP Status** | `400 Bad Request`  |

### Request Body: 
```json
{
  "roomId": "b1a2c3d4e5f6478390ab1c2d3e4f5a6b",
  "playerId": "8f8b16c9f2e44f1f9a9e4a7e4d1c2b3"
}
```
| 필드명      | 타입     | 필수 | 설명                   |
| -------- | ------ | -- | -------------------- |
| roomId   | string | O  | 게임을 시작할 방 ID         |
| playerId | string | O  | 게임 시작 요청을 보낸 플레이어 ID |

### Response:
* 성공 (200 OK)
  ```json
    {
    "roomId": "b1a2c3d4e5f6478390ab1c2d3e4f5a6b",
    "inviteCode": "AB3F9Z",
    "players": [
      "8f8b16c9f2e44f1f9a9e4a7e4d1c2b3",
      "c3d2e1f0a9b8c7d6e5f4a3b2c1d0e9f8"
    ],
    "turnOrder": [
      "c3d2e1f0a9b8c7d6e5f4a3b2c1d0e9f8",
      "8f8b16c9f2e44f1f9a9e4a7e4d1c2b3"
    ],
    "firstPlayer": "c3d2e1f0a9b8c7d6e5f4a3b2c1d0e9f8",
    "currentTurn": "c3d2e1f0a9b8c7d6e5f4a3b2c1d0e9f8",
    "gameRound": 1   // ★ 추가됨
  }
  ```
    
  | 필드명         | 타입     | 설명                                     |
  | ----------- | -------- | -------------------------------------- |
  | roomId      | string   | 방 ID                                   |
  | inviteCode  | string   | 방 초대 코드                              |
  | players     | string[] | 방에 속한 플레이어 ID 목록                       |
  | turnOrder   | string[] | 이번 게임의 턴 순서                         |
  | firstPlayer | string   | 첫 턴을 진행할 플레이어 ID                       |
  | currentTurn | string   | 현재 턴인 플레이어 ID (게임 시작 직후 = firstPlayer) |
  | gameRound   | int      | 현재 게임 라운드 번호. /game/start 호출 시 증가|

* 실패 (400 Bad Request)
  ```json
  // 1) 방을 찾을 수 없는 경우
  { "error": "Room not found" }

  // 2) 요청한 플레이어가 방에 없음
  { "error": "Player not in room" }

  // 3) 플레이어 수가 2명 미만인 경우
  { "error": "Need at least 2 players to start" }

  // 4) 이미 게임이 시작 된 경우
  { "error": "Game already started" }
  ```
---

## 8-1. 선 긋기 (POST /choice)

멀티플레이시 차례에 해당하는 유저가 선택한 선 정보를 전달한다.  
| 속성                 | 내용                 |
| ------------------ | ------------------ |
| **Method**         | `POST`             |
| **Path**           | `/choice`          |
| **Content-Type**   | `application/json` |
| **Request Body**   | `ChoiceRequest`    |
| **성공 HTTP Status** | `200 OK`           |
| **실패 HTTP Status** | `400 Bad Request`  |

### Request Body: 
```json
{
  "roomId": "string",
  "playerId": "string",
  "isHorizontal": true,
  "row": 1,
  "col": 2
}
```
| 필드명          | 타입     | 필수 | 설명                      |
| ------------ | ------ | -- | ----------------------- |
| roomId       | string | O  | 방 ID                    |
| playerId     | string | O  | 선을 긋는 플레이어 ID           |
| isHorizontal | bool   | O  | true = 가로선, false = 세로선 |
| row          | int    | O  | 선의 시작 row 좌표            |
| col          | int    | O  | 선의 시작 col 좌표            |

### Response:
* 성공 (200 OK)
  ```json
  {
    "status": "ok",
    "roomId": "abc123",
    "moveSeq": 12,
    "move": {
      "playerId": "p1",
      "isHorizontal": true,
      "row": 1,
      "col": 3
    },
    "madeBoxes": [
      { "row": 0, "col": 0 }],
    "nextTurnPlayerId": "p2",
    "boardCompleted": false
  }
  ```
| 필드명               | 타입       | 설명                        |
| ----------------- | -------- | ------------------------- |
| status            | string   | 성공 시 `"ok"`               |
| roomId            | string   | 방 ID                      |
| gameRound         | int      | 현재 게임 라운드 번호              |
| moveSeq           | long     | 이번 move 이벤트 시퀀스 번호        |
| move              | object   | 그린 선 정보                   |
| move.playerId     | string   | 선을 그린 플레이어 ID             |
| move.isHorizontal | bool     | 가로선 여부                    |
| move.row          | int      | 선 위치 row                  |
| move.col          | int      | 선 위치 col                  |
| madeBoxes         | object[] | 이번 선으로 완성된 박스 목록          |
| madeBoxes[].row   | int      | 박스 좌표 row                 |
| madeBoxes[].col   | int      | 박스 좌표 col                 |
| extraTurn         | bool     | 박스를 만들었는지 여부 (true면 턴 유지) |
| nextTurnPlayerId  | string   | 다음 턴 플레이어 ID              |
| boardCompleted    | bool     | 보드가 모두 완성되었는지 여부          |

* 실패 (400 Bad Request)
  ```json
  // 1) 방을 찾을 수 없는 경우
  { "error": "Room not found" }

  // 2) 요청한 플레이어가 방에 없음
  { "error": "Player not in room" }

  // 3) 게임 시작 전인 경우
  { "status": "error", "errorCode": "Game not started" }

  // 4) 내 턴이 아닌 경우
  {
    "status": "error",
    "errorCode": "Not your turn",
    "currentTurnPlayerId": "8f8b16c9f2e44f1f9a9e4a7e4d1c2b3"
  }

  // 5) 유효하지 않거나 중복된 선
  { "status": "error", "errorCode": "INVALID_OR_DUPLICATED_LINE" }
  ```
---

## 8-2. 선 이벤트 조회 (GET /draw)
상대가 그린 선 이벤트를 조회하여 정보를 전달합니다.
| 속성                 | 내용                   |
| ------------------ | -------------------- |
| **Method**         | `GET`                |
| **Path**           | `/draw`              |
| **Query Params**   | `roomId`, `afterSeq` |
| **성공 HTTP Status** | `200 OK`             |
| **실패 HTTP Status** | `400 Bad Request`    |

### Request Body: 
```http
GET /draw?roomId=abc123&afterSeq=10
```

| 파라미터    | 타입   | 필수 | 설명                          |
| -------- | ------ | -- | --------------------------- |
| roomId   | string | O  | 방 ID                        |
| afterSeq | long   | X  | 해당 시퀀스보다 큰 이벤트만 조회 (기본값: 0) |

### Response:
* 성공 (200 OK)
  ```json
  {
    "roomId": "abc123",
    "gameRound": 2,     // ★ 추가됨
    "events": [
      {
        "seq": 11,
        "playerId": "p2",
        "isHorizontal": false,
        "row": 4,
        "col": 1,
        "madeBoxes": []
      },
      {
        "seq": 12,
        "playerId": "p1",
        "isHorizontal": true,
        "row": 1,
        "col": 3,
        "madeBoxes": [
        { "row": 0, "col": 0 }
      ]
      }
    ],
    "lastSeq": 12
  }
  ```
| 필드명                      | 타입       | 설명                               |
| ------------------------ | -------- | -------------------------------- |
| roomId                   | string   | 방 ID                             |
| gameRound                | int      | 현재 게임 라운드 번호                     |
| events                   | object[] | afterSeq 이후의 move 이벤트 목록         |
| events[].seq             | long     | 이벤트 시퀀스 번호                       |
| events[].playerId        | string   | 선을 그린 플레이어 ID                    |
| events[].isHorizontal    | bool     | 가로선 여부                           |
| events[].row             | int      | 선 위치 row                         |
| events[].col             | int      | 선 위치 col                         |
| events[].madeBoxes       | object[] | 해당 move로 완성된 박스 목록               |
| events[].madeBoxes[].row | int      | 박스 좌표 row                        |
| events[].madeBoxes[].col | int      | 박스 좌표 col                        |
| lastSeq                  | long     | 반환된 이벤트 중 마지막 seq (없으면 afterSeq) |

* 실패 (400 Bad Request)
  ```json
  // 1) 방을 찾을 수 없는 경우
  { "status": "error", "errorCode": "ROOM_NOT_FOUND" }
  ```
  ---