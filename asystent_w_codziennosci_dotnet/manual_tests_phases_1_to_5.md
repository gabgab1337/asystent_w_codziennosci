# Scenariusze Testów Manualnych (Fazy 1 - 5)

**Narzędzia:** 
1. **Opcja A (Swagger UI):** Uruchom projekt API (`dotnet run`). Wejdź na `https://localhost:<port>/swagger`. Zaloguj się w endpointach `/login`, skopiuj `accessToken` i wklej go w kłódkę "Authorize" u góry na stronie, wpisując słowo `Bearer ` przed tokenem (np. `Bearer eyJhb...`).
2. **Opcja B (Pliki .http):** Otwórz plik `Assistant.Api.http`. Dodaj na górze zmienne `@adminToken = ...`, `@caregiverToken = ...`, `@asdToken = ...` i wywołuj requesty z poziomu edytora.

---

## PERSONA 1: Administrator (Faza 1)
*Cel: Weryfikacja logowania i zarządzania kontami z najwyższymi uprawnieniami.*

- [ ] **1. Logowanie (Admin)**
  - **POST** `/api/v1/auth/login`
  - **Body:** `{ "username": "admin_test", "password": "twoje_haslo" }` (Użyj istniejącego konta)
  - **Oczekiwane:** `200 OK`, JSON z `accessToken`, `refreshToken` i danymi użytkownika.
---

## PERSONA 2: Opiekun (Fazy 1, 2, 3, 4)
*Cel: Przygotowanie środowiska dla podopiecznego (tworzenie planu, zadań, triggerów).*

- [ ] **1. Logowanie (Opiekun)**
  - **POST** `/api/v1/auth/login`
  - Użyj konta opiekuna. Zmień token w Swaggerze.
- [ ] **2. Utworzenie Podopiecznego**
  - **POST** `/api/v1/proteges`
  - **Body:** `{ "username": "login_podopiecznego", "password": "hasło_podopiecznego" }`
  - **Oczekiwane:** `201 Created`, zapisz wygenerowane `id` (np. `15`) jako `{protegeId}`.
- [ ] **3. Ustawienie Aktywnego Podopiecznego**
  - **PUT** `/api/v1/me/selected-protege`
  - **Body:** `{ "protegeId": {protegeId} }`
  - **Oczekiwane:** `200 OK`. (Weryfikacja: wywołaj `GET /api/v1/me` – w odpowiedzi powinieneś zobaczyć zaktualizowane `selectedProtege`).
- [ ] **4. Stworzenie Triggera Czasowego**
  - **POST** `/api/v1/triggers`
  - **Body:** `{ "name": "Codziennie", "kind": "time", "time": { "subType": "default" } }`
  - **Oczekiwane:** `201 Created`, zapisz `id` triggera (np. `101`). Zwrócony JSON NIE może mieć pola z surowym starym formatem (średnikami).
- [ ] **5. Utworzenie Zadania**
  - **POST** `/api/v1/proteges/{protegeId}/tasks`
  - **Body:** `{ "name": "nazwa", "timeBegin": null, "timeEnd": null }`
  - **Oczekiwane:** `201 Created`, zapisz `id` zadania (np. `50`).
- [ ] **6. Podpięcie Triggera do Zadania**
  - **PUT** `/api/v1/tasks/{taskId}/triggers` (Użyj ID z kroków wyżej)
  - **Body:** `{ "triggerId": {triggerId}, "negation": false }`
  - **Oczekiwane:** `204 No Content` lub `200 OK`.
- [ ] **7. Dodanie Punktu (Kroku) do Zadania**
  - **POST** `/api/v1/tasks/{taskId}/points`
  - **Body:** `{ "name": "podzadanie" }`
  - **Oczekiwane:** `201 Created`, zapisz `id` punktu (np. `200`).
- [ ] **8. Test Uploadu Zdjęcia do Punktu**
  - W Swaggerze znajdź **POST** `/api/v1/points/{id}/photo`
  - Prześlij przykładowe zdjęcie `.png` lub `.jpg`.
  - **Oczekiwane:** `200 OK`, zapisz ścieżkę z odpowiedzi.
- [ ] **9. Zabezpieczenie plików (BOLA)**
  - Otwórz okno incognito (brak autoryzacji) lub podaj błędne ID w URL zdjęcia `/api/v1/files/ścieżka...`
  - **Oczekiwane:** `401 Unauthorized` lub `404 Not Found`.
- [ ] **10. Symulacja Planu Dnia (Preview)**
  - **POST** `/api/v1/proteges/{protegeId}/plan`
  - **Body (What-If z pogodą):** `{ "at": "2026-10-26T08:15:00+02:00", "weather": { "temperatureC": 15, "weatherType": "rain" } }`
  - **Oczekiwane:** `200 OK`. Lista zadań. Upewnij się, że zadanie  zwraca `state: "toDo"` i nie ma tagów HTML w nazwach/opisach.

---

## PERSONA 3: Podopieczny (Fazy 4, 5 - Runtime)
*Cel: Urządzenie mobilne pyta o to, co ma robić i realizuje plan.*

- [ ] **1. Logowanie (Podopieczny)**
  - **POST** `/api/v1/auth/login` (użyj konta podopiecznego które utworzyłeś wcześniej).
  - Ustaw nowy token w Swaggerze.
- [ ] **2. Pogoda (Samodzielna)**
  - **GET** `/api/v1/weather/current`
  - **Oczekiwane:** `200 OK`, dane o pogodzie (temperatura, wiatr itp.).
- [ ] **3. Własny Plan Dnia**
  - **GET** `/api/v1/me/plan` (Opcjonalnie dodaj `?at=2026-10-26T08:15:00` żeby przenieść się w czasie testowo).
  - **Oczekiwane:** `200 OK`. Tylko własne zadania (powinny odpowiadać krokowi 10 u opiekuna).
- [ ] **4. Aktualna Aktywność (Current Activity)**
  - **GET** `/api/v1/me/current-activity`
  - **Oczekiwane:** `200 OK`, JSON z aktywnym zadaniem i jego punktami.
- [ ] **5. Status Paska Aktywności (Summary Polling)**
  - **GET** `/api/v1/me/current-activity/summary`
  - **Oczekiwane:** `200 OK`, bardzo krótki JSON z nazwą zadania, pozostałymi minutami oraz stanem (`normal`, `lowTime`, `postTime`).
- [ ] **6. Odhaczenie Punktu (Zrealizowane)**
  - **POST** `/api/v1/me/current-activity/complete-point`
  - **Body:** `{ "pointId": 200 }` (ID punktu "Nałóż pastę").
  - **Oczekiwane:** `200 OK`.
- [ ] **7. Test Idempotentności**
  - Wywołaj DOKŁADNIE TEN SAM request odhaczenia punktu drugi raz.
  - **Oczekiwane:** `200 OK` (Nie powinno rzucić błędu serwera 500 o naruszeniu unikalności w SQL).
- [ ] **8. Zamknięcie Zadania (Postęp Runtime'u)**
  - Sprawdź stan (krok 4). Powinno przeskoczyć do następnego zadania (lub zwrócić `kind: "freeTime"` z `task: null`, jeśli nie ma już nic na dziś).
  - Jeśli `CurrentActivity` to nadal to samo zadanie, zakończ je wymuszonym zamknięciem: **POST** `/api/v1/me/tasks/{taskId}/complete`.

---

## TESTY BEZPIECZEŃSTWA (IDOR / BOLA)
- [ ] **1. Wymuszenie Cudzego Zaufania**
  - Będąc zalogowanym jako Podopieczny, spróbuj zamknąć zadanie używając `taskId` innego podopiecznego (jeśli takie masz w bazie).
  - **Oczekiwane:** `403 Forbidden` (task_not_owned).
- [ ] **2. Edycja Niewłasnego Podopiecznego**
  - Będąc Opiekunem A, wywołaj `PUT /api/v1/proteges/{id}` podając id Podopiecznego przypisanego do Opiekuna B.
  - **Oczekiwane:** `403 Forbidden`.
