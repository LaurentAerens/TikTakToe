# API Reference

Base URL (default): `http://localhost:8080`

All responses are wrapped in a standard envelope:

```json
{
  "success": true,
  "data": { ... },
  "error": null
}
```

On error the envelope becomes:

```json
{
  "success": false,
  "data": null,
  "error": "Human-readable message"
}
```

---

## Health & Version

### `GET /healthz`

Returns the application health status.

**Response `200 OK`**

```json
{ "status": "healthy" }
```

---

### `GET /version`

Returns the running application version.

**Response `200 OK`**

```json
{ "version": "1.0.0+abc123" }
```

---

## Games

### `POST /games`

Creates a new game with a rectangular board.

**Request body**

| Field       | Type       | Default | Description                                          |
| ----------- | ---------- | ------- | ---------------------------------------------------- |
| `rows`      | `integer`  | `3`     | Number of rows on the board (1 – 10 000).            |
| `cols`      | `integer`  | `3`     | Number of columns on the board (1 – 10 000).         |
| `playerIds` | `Guid[]`   | —       | Ordered list of 2 – 1 000 player GUIDs.             |

```json
{
  "rows": 3,
  "cols": 3,
  "playerIds": [
    "11111111-1111-1111-1111-111111111111",
    "22222222-2222-2222-2222-222222222222"
  ]
}
```

**Response `201 Created`**

```json
{
  "success": true,
  "data": {
    "id": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
    "board": [[0,0,0],[0,0,0],[0,0,0]],
    "players": [
      { "id": "...", "isEngine": false, "externalId": null },
      { "id": "...", "isEngine": true,  "externalId": "classical" }
    ],
    "moves": [],
    "waitingForPlayerId": "11111111-1111-1111-1111-111111111111",
    "state": {
      "isGameOver": false,
      "winnerValue": null,
      "winnerPlayerId": null
    }
  },
  "error": null
}
```

**Game response fields**

| Field              | Type      | Description                                                                 |
| ------------------ | --------- | --------------------------------------------------------------------------- |
| `id`               | `Guid`    | Game UUID.                                                                  |
| `board`            | `int[][]` | 2D array representing the board state (0 = empty, 1 = player 1, etc.).      |
| `players`          | `Player[]` | List of players in the game.                                                |
| `moves`            | `Move[]`  | List of moves made so far, ordered by move number.                          |
| `waitingForPlayerId` | `Guid?`  | ID of the player whose turn it is. `null` when the game is over.            |
| `state`            | `GameState` | Terminal-state object for game completion and winner markers.                |

**GameState fields**

| Field            | Type      | Description                                                                 |
| ---------------- | --------- | --------------------------------------------------------------------------- |
| `isGameOver`     | `boolean` | `true` when the game ended in win or draw.                                 |
| `winnerValue`    | `int?`    | Winner marker: `null` in-progress, `-1` draw, positive value (1,2,...) winner. |
| `winnerPlayerId` | `Guid?`   | Winner player id; `null` for draw or in-progress.                          |

**Error responses**

| Status | Condition                                              |
| ------ | ------------------------------------------------------ |
| `400`  | Board dimensions exceed 10 000, invalid player count, duplicate player IDs, or unknown player IDs. |

---

### `GET /games/{id}`

Returns a game by its UUID.

**Path parameters**

| Parameter | Type   | Description   |
| --------- | ------ | ------------- |
| `id`      | `Guid` | Game UUID.    |

**Response `200 OK`**

```json
{
  "success": true,
  "data": {
    "id": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
    "board": [[1,2,0],[0,1,0],[0,0,2]],
    "players": [
      { "id": "...", "isEngine": false, "externalId": null },
      { "id": "...", "isEngine": true,  "externalId": "classical" }
    ],
    "moves": [
      { "id": "...", "x": 0, "y": 0, "value": 1, "moveNumber": 1 },
      { "id": "...", "x": 2, "y": 2, "value": 2, "moveNumber": 2 },
      { "id": "...", "x": 1, "y": 1, "value": 1, "moveNumber": 3 }
    ],
    "waitingForPlayerId": "22222222-2222-2222-2222-222222222222",
    "state": {
      "isGameOver": false,
      "winnerValue": null,
      "winnerPlayerId": null
    }
  },
  "error": null
}
```

**Error responses**

| Status | Condition           |
| ------ | ------------------- |
| `404`  | Game not found.     |

---

### `POST /games/{id}/moves`

Submits a move for a human player. Engine moves are automatic and handled by the background worker.

**Path parameters**

| Parameter | Type   | Description   |
| --------- | ------ | ------------- |
| `id`      | `Guid` | Game UUID.    |

**Headers**

| Header            | Required | Description                             |
| ----------------- | -------- | --------------------------------------- |
| `X-Player-Id`     | Yes      | Player UUID making the move.            |
| `Player-Id`       | No       | Alternative header for player UUID.     |

**Request body**

| Field | Type     | Description                    |
| ----- | -------- | ------------------------------ |
| `x`   | `integer` | Row coordinate (0-indexed).   |
| `y`   | `integer` | Column coordinate (0-indexed).|

```json
{
  "x": 1,
  "y": 1
}
```

**Response `200 OK`**

Returns the updated game state with the new move recorded.

**Error responses**

| Status | Condition                                                                 |
| ------ | ------------------------------------------------------------------------- |
| `400`  | Missing player header, invalid GUID, coordinates out of bounds, cell occupied, or attempting to move during engine's turn. |
| `404`  | Game not found.                                                           |

**Automatic engine play**

When the next player (`waitingForPlayerId`) is an engine, the server automatically processes the engine move via a background worker. Clients should poll `GET /games/{id}` to observe the engine's move. Engine-vs-engine games self-complete as fast as possible without client intervention.

---

## Engines

### `GET /engines`

Lists all available engine capabilities.

**Response `200 OK`**

```json
{
  "success": true,
  "data": [
    {
      "id":           "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
      "playerId":     "cccccccc-cccc-cccc-cccc-cccccccccccc",
      "displayName":  "Classical",
      "maxBoardSizeX": 10,
      "maxBoardSizeY": 10,
      "depth": true
    }
  ],
  "error": null
}
```

**Capability fields**

| Field           | Type      | Description                                                 |
| --------------- | --------- | ----------------------------------------------------------- |
| `id`            | `Guid`    | Unique engine identifier.                                   |
| `playerId`      | `Guid`    | Player slot identifier used when creating a game.           |
| `displayName`   | `string`  | Human-readable engine name (e.g. `"Classical"`).            |
| `maxBoardSizeX` | `integer` | Maximum supported board width.                              |
| `maxBoardSizeY` | `integer` | Maximum supported board height.                             |
| `depth`         | `boolean` | Whether the engine accepts a custom `depth` parameter.      |

---

### `GET /engines/{id}`

Looks up an engine by its UUID.

**Path parameters**

| Parameter | Type   | Description     |
| --------- | ------ | --------------- |
| `id`      | `Guid` | Engine UUID.    |

**Response `200 OK`**

```json
{
  "success": true,
  "data": {
    "id":          "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
    "playerId":    "cccccccc-cccc-cccc-cccc-cccccccccccc",
    "displayName": "Classical"
  },
  "error": null
}
```

**Error responses**

| Status | Condition            |
| ------ | -------------------- |
| `404`  | Engine ID not found. |

---

### `GET /engines/{displayName}`

Looks up an engine by its display name (case-insensitive).

**Path parameters**

| Parameter     | Type     | Description                       |
| ------------- | -------- | --------------------------------- |
| `displayName` | `string` | Engine display name, e.g. `Classical`. |

**Response `200 OK`** — same shape as `GET /engines/{id}`.

**Error responses**

| Status | Condition                      |
| ------ | ------------------------------ |
| `404`  | Engine display name not found. |

---

### `GET /engines/resolve-engine-id/{playerId}`

Converts an engine player ID into the engine record ID.

This is useful when you have the `playerId` from a game's player list and want to look up the matching engine entry.

**Path parameters**

| Parameter  | Type   | Description              |
| ---------- | ------ | ------------------------ |
| `playerId` | `Guid` | Engine player UUID.      |

**Response `200 OK`** — same shape as `GET /engines/{id}`.

**Error responses**

| Status | Condition                       |
| ------ | ------------------------------- |
| `404`  | Engine player ID not found.     |

---

## OpenAPI / Scalar UI

When `Features:ExposeApiDocs` is `true` (always on in Development mode), the following routes are available:

| Route                  | Description                              |
| ---------------------- | ---------------------------------------- |
| `GET /openapi/v1.json` | OpenAPI 3 JSON specification.            |
| `GET /scalar`          | Interactive Scalar API explorer UI.      |
