INSERT INTO Event(OrganizerId, Name, Description, EventDate, RouteInfo) VALUES
(13, 'Johannesburg City Marathon', 'Annual road marathon through the johanesburg CBD.' , '2026-10-18 06:00:00' ,   'Starts at Mary Fitzegarald Squre, go through braamfontein .'),
(14, 'Soweto Fun Run', 'Community fun run supporting local charities.', '2026-09-27 07:00:00',  'Starts and ends at the Soweto Theatre.'),
(15, 'Cape Town Cycle Challange', 'cycling event along camps bay.' , '2026-06-02 06:23:00',  'Coastal Route and camps bay.')

EXEC sp_columns Event;

SELECT UserId, FullName FROM [User];