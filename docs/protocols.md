## 🚀 DotBox 서버 프로토콜 명세서 (API Specification)

| 구분 | 내용 |
| :--- | :--- |
| **최종 수정일** | 2025년 11월 26일 |
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

#### 예시 요청
##### 1) HTTP Raw 예시 
```http
GET /health HTTP/1.1
Host: 43.201.40.98:8080
Accept: */*
```

##### 2) curl 예시
```
curl http://43.201.40.98:8080/health
```

#### Response:
```json
{ "status": "ok" }
```

## 2. 플레이어 접속 (Connect Player)
플레이어가 서버에 접속하면 고유한 PlayerId를 발급하는 API.
| 속성               | 내용                             |
| :--------------- | :----------------------------- |
| **Method**       | `POST`                         |
| **Path**         | `/connect`                     |
| **Content-Type** | `application/json`             |
| **Request Body** | JSON (아래 명세 참고)                |
| **Response**     | Player 세션 정보(JSON)             |
| **에러 코드**        | `400 Bad Request` (유효하지 않은 이름) |
