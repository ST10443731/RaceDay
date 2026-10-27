\# RaceDay Web Application - Part 3 (MVC Front-End)



This is the front-end MVC application for the RaceDay platform. It consumes the Part 2 API and provides a clean, role-aware interface for Organisers and Participants.



\## Features

\- \*\*Authentication\*\*: Role-based login and registration (Organiser / Participant).

\- \*\*Organiser Interface\*\*: Create and manage events, upload event banners (Azure Blob Storage), add categories, view enrolments, and enter race results.

\- \*\*Participant Interface\*\*: Browse upcoming events, enrol in categories, view personal enrolments, and view personal race results.

\- \*\*Security\*\*: Session-based authentication. All data operations are routed through the Part 2 API (no direct database access).



\## Docker Setup Instructions



The application is containerised using Docker and Docker Compose. 



\*\*Prerequisites:\*\*

\- Docker Desktop must be installed and running.



\*\*To start the application:\*\*

Open a terminal in the root directory (where `docker-compose.yml` is located) and run:



```bash

docker compose up -d --build

