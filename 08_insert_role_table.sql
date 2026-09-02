INSERT INTO ROLE (RoleName) VALUES ('Organizer'),('Participant');
INSERT INTO [User] (FullName, Email, PasswordHash,RoleId) VALUES
('Tau Mokwena', 'mokwenaT@mail,co,za', 'HASH_PLACEHOLDER_1', (SELECT ROLEId FROM ROLE WHERE RoleName ='Organizer')),
('Lindiwe Dlamini', 'liddy@mail.co.za', 'HASH_PLACEHOLDER_2', (SELECT ROLEId FROM ROLE WHERE RoleName = 'Organizer')),
('Thabiso Sebopa', 'Lazzy@mail.co.za', 'HASH_PLACEHOLDER_3', (SELECT ROLEId FROM ROLE WHERE RoleName = 'Participant')),
('Lesego Maputla', 'Putla@mail.co.za', 'HASH_PLACEHOLDER_4', (SELECT ROLEId FROM ROLE WHERE RoleName = ' participant'))