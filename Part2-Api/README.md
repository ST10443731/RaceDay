\# RaceDay API (Part 2)



A RESTful API for managing race events, built with ASP.NET Core Web API, Entity Framework Core (Code-First) and SQL Server. Organisers create events and categories and record results. Participants browse events, enrol in categories and view their own results.



\## The two roles



| Role | What they can do |

|---|---|

| \*\*Organizer\*\* | Create, update and delete their own events and categories. View all enrolments for their own events. Capture finish times and positions. |

| \*\*Participant\*\* | View events and categories. Enrol in a category (once per category, while space remains). View and withdraw their own enrolments. View their own results. |



Both roles can register, log in, view and update their own profile, and view events, categories and event results.



\## Setup and run



\*\*Requirements:\*\* .NET 9 SDK, SQL Server (Express is fine) and Visual Studio or the `dotnet` CLI.



1\. Clone the repo and switch to the Part 2 branch:

```

&#x20;  git clone https://github.com/ST10443731/RaceDay.git

&#x20;  cd RaceDay

&#x20;  git checkout part2-api

&#x20;  cd Part2-Api/RaceDay.Api

```

2\. Open `appsettings.json` and set `DefaultConnection` to your own SQL Server instance (the default points at `LabVM2049939\\SQLEXPRESS`; change `Server=` to yours).

3\. Install the EF Core tool (once) and create the database from the migrations:

```

&#x20;  dotnet tool install --global dotnet-ef

&#x20;  dotnet ef database update

```

&#x20;  This creates `RaceDayApiDb` and seeds the two roles.

4\. Run the API:

```

&#x20;  dotnet run

```

5\. Open Swagger at `https://localhost:7040/swagger/index.html` (use the port shown in the console).



\## Authentication



Users register with a role (`Organizer` or `Participant`; any other value is rejected) and log in with email and password. Passwords are hashed and never stored in plain text. After login the API issues a \*\*session cookie\*\* that carries the user's id and role, and role rules are enforced on every request.



\*\*Changes from my Part 1 plan:\*\* login uses a session cookie instead of a JWT (as required for Part 2); `Location` and `EventType` (run, walk or cycle) were added to Event; and the `FinishTime` column name was corrected.



\## Endpoints



| Area | Endpoints |

|---|---|

| Authentication | `POST /api/auth/register`, `POST /api/auth/login` |

| User profile | `GET` and `PUT /api/users/me` |

| Events | `GET /api/events`, `GET /api/events/{id}`, `POST /api/events`, `PUT` and `DELETE /api/events/{id}` |

| Categories | `GET` and `POST /api/events/{eventId}/categories`, `PUT` and `DELETE /api/categories/{id}` |

| Enrolments | `POST /api/categories/{categoryId}/enrolments`, `GET /api/users/me/enrolments`, `GET /api/events/{eventId}/enrolments`, `DELETE /api/enrolments/{id}` |

| Results | `POST /api/enrolments/{enrolmentId}/result`, `GET /api/users/me/results`, `GET /api/events/{eventId}/results` |



Every endpoint is documented in Swagger, including the expected data and each possible response code.



\## Unit tests



The test project is `Part2-Api/RaceDay.Api.Tests` (xUnit). It starts the whole API in memory with an in-memory database and sends real HTTP requests. It has 24 tests covering registration and login, event management by Organisers, rejection of the wrong role, and enrolments. Both success and failure cases are tested.



```

cd Part2-Api/RaceDay.Api.Tests

dotnet test

```



\## CI/CD



A GitHub Actions workflow (`.github/workflows/ci.yml`) builds the project and runs all unit tests on every push.



!\[Successful green build](screenshots/green-build.png)



!\[Unit tests passing in CI](screenshots/green-build-tests.png)



\## Demo video



YOUTUBE\_LINK\_HERE

