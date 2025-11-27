## DotBox 서버 프로토콜 명세서 (API Specification)

| 구분 | 내용 |
| :--- | :--- |
| **최종 수정일** | 2025년 11월 28일 |
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

| 속성                | 내용                 |
| :----------------- | :------------------ |
| **Method**         | `POST`              |
| **Path**           | `/room/create`      |
| **Content-Type**   | `application/json`  |
| **Request Body**   | `CreateRoomRequest` |
| **Response**       | 방 정보(JSON)         |
| **성공 HTTP Status** | `200 OK`            |
| **실패 HTTP Status** | `400 Bad Request`   |

### Request Body:
```json
{
  "playerId": "8f8b16c9f2e44f1f9a9e4a7e4d1c2b3"
}
```
| 필드명      | 타입    | 필수 | 설명                                      |
| --------  | ------ | --  | ------------------------------------     |
| playerId  | string | O   | 방을 생성하는 플레이어 ID (`/connect` 에서 발급) |

### Response:
* 성공 (200 OK)
    ```json
  {
    "roomId": "b1a2c3d4e5f6478390ab1c2d3e4f5a6b",
    "inviteCode": "AB3F9Z",
    "players": [
      "8f8b16c9f2e44f1f9a9e4a7e4d1c2b3"
    ],
    "maxPlayers": 3,
    "isFull": false,
    "currentTurn": null
  }
  ```
  | 필드명        | 타입      | 설명                                        |
  | ----------- | -------- | -----------------------------------------  |
  | roomId      | string   | 고유한 방 ID (`Guid.NewGuid().ToString("N")`)|
  | inviteCode  | string   | 초대 코드(영문+숫자, 혼동 문자 제거된 6자리)        |
  | players     | string[] | 현재 방에 있는 playerId 목록 (생성자는 자동 입장)   |
  | maxPlayers  | int      | 최대 플레이어 수(기본값 3)                       |
  | isFull      | bool     | 현재 인원수가 `maxPlayers`에 도달했는지 여부       |
  | currentTurn | string?  | 게임 시작 전이므로 항상 `null`                   |

* 실패 (400)
  ```json
  // playerId가 존재하지 않을 때
  { "error": "Invalid playerId" }
  ```
---

## 5. 방 참가 (POST /room/join)
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
    "inviteCode": "AB3F9Z"
  }
  ```
  | 필드명       | 타입    | 설명                               |
  | ---------- | ------ | -------------------------         |
  | status     | string | 성공 시 `"ok"`                      |
  | roomId     | string | 참가에 성공한 방 ID                   |
  | inviteCode | string | 방의 초대 코드 (요청에 사용한 코드와 동일)  |

* 실패 (404)
  ```json
  // 1) playerId가 유효하지 않을 때 - 404 Bad Request
  { "error": "Invalid playerId" }
  // 2) 초대 코드에 해당하는 방이 없을 때 - 404 Not Found
  { "error": "Room not found" }
  // 3) 방이 가득 찼을 때 - 404 Bad Request 
  { "error": "Room is full" }
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
      "isFull": false
    }
    ```
    | 필드명       | 타입      | 설명                                           |
    | ---------- | -------- | -------------------------------------         |
    | roomId     | string   | 방 고유 ID                                      |
    | inviteCode | string   | 방 초대 코드                                     |
    | players    | string[] | 현재 방에 참가 중인 `playerId` 리스트               |
    | isFull     | bool     | 현재 인원수가 `MaxPlayers`(기본 3명)에 도달했는지 여부 |

* 실패 (404 Not Found)
    ```json
    // 방이 존재하지 않을 때
    { "error": "Room not found" }
    ```
---

