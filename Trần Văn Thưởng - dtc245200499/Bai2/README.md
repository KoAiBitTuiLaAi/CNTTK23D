# Bai 2 - RESTful API - De 20: Library and book reservations

Student: Tran Van Thuong  
Student ID: dtc245200499  
Class: CNTT K23D

This solution applies the REST API exercise to the selected library domain. It has two independent .NET 10 processes: `Submission.Api` and `Submission.Client`. The server keeps sample books and reservations in memory and exposes JSON over HTTP.

## Run

From this directory, open two PowerShell terminals.

Terminal 1, start the API on the fixed port:

```powershell
dotnet run --project src/Submission.Api --urls http://localhost:5000
```

Terminal 2, run the client:

```powershell
dotnet run --project src/Submission.Client -- http://localhost:5000
```

Build both projects with:

```powershell
dotnet build
```

Open `requests.http` in VS Code with the REST Client extension and send each request in order. The reservation readback request uses the ID returned by the preceding create request. The final POST demonstrates the framework's 415 response when the JSON content type is omitted.

## API contract

| Method | URI | Success | Errors |
|---|---|---|---|
| GET | `/` | 200 health JSON | - |
| GET | `/api/books?keyword=...` | 200 book array | - |
| GET | `/api/books/{bookId}` | 200 book | 404 `BOOK_NOT_FOUND` |
| POST | `/api/books/{bookId}/reservations` | 201 + `Location` | 400 `INVALID_RESERVATION`, 404 `BOOK_NOT_FOUND`, 409 `NO_COPIES_AVAILABLE` |
| GET | `/api/reservations/{reservationId}` | 200 reservation | 404 `RESERVATION_NOT_FOUND` |

All handler-generated errors have the same `ApiError { code, message }` shape. The server supplies the reservation ID, status and timestamp. `bookId` belongs in the URI because a reservation is a child resource of a book.

## Ten-point self-check

| # | Request or action | Expected result |
|---|---|---|
| 1 | `GET /` | 200 |
| 2 | `GET /api/books` | 200 and four sample books |
| 3 | `GET /api/books/1` | 200 |
| 4 | `GET /api/books/999` | 404 + `BOOK_NOT_FOUND` |
| 5 | `GET /api/books/abc` | framework 404 with empty body |
| 6 | POST a valid reservation | 201 + `Location` |
| 7 | POST an empty reservation | 400 + `INVALID_RESERVATION` |
| 8 | POST without `Content-Type` | framework 415 |
| 9 | Run the client | lists books, reserves, reads back from `Location`, then prints a 400 `ApiError` without crashing |
| 10 | Stop the API and run the client again | network error, distinct from an HTTP error response |

The 409 no-stock case and keyword search are additional checks. Reservations and stock counts live in memory and reset when the API restarts.

## Three short answers

1. **Why does the URI use nouns while the HTTP method carries the action?** A URI identifies a resource, such as `/api/books/1/reservations`; `GET` reads it and `POST` creates a reservation. This makes the endpoint reusable without action words like `/borrowBook`.
2. **Where is statelessness visible?** Each request carries the book ID in its URI and reservation details in its JSON body. The client does not need a server session between calls. The API does store application data in process memory; statelessness does not mean the server stores no data.
3. **Why does each side declare its own DTOs?** The client and server are separate processes and depend on the JSON contract's property names and types, not on a shared C# project. Separate DTOs keep either side independently changeable while preserving that contract.

## Experiment notes

| Case | Result observed |
|---|---|
| Valid reservation | `201 Created`; returned a GUID and `Location: /api/reservations/{guid}`. Readback returned 200 and `reserved`. |
| Invalid reservation | `400 Bad Request`; `INVALID_RESERVATION: StudentCode and StudentName are required.` |
| Missing content type | `415 Unsupported Media Type` from the framework. |
| API stopped | Client printed `Network error ... actively refused it (localhost:5000)` and exited with failure status. |

The API root, list, detail, missing detail, invalid route, valid reservation, invalid reservation and missing content-type requests returned 200, 200, 200, 404, 404, 201, 400 and 415 respectively. The client completed its four stages while the API was running and handled the stopped-server case without crashing. No screenshot is included; add genuine screenshots of the local runs here if required by the instructor. Do not commit `bin/` or `obj/`.
