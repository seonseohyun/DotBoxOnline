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

---

## 1. 서버 상태 확인 (Health Check)

### 1.1. 서버 헬스 체크 (GET /health)

서버가 현재 동작 중인지 확인합니다.

| 속성  | 내용 |
| :--- | :--- |
| **Method** | `GET` |
| **Path** | `/health` |
| **Request Body** | 없음 |

#### Request:
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