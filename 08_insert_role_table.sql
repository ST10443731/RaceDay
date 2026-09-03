INSERT INTO [User] (FullName, Email, PasswordHash, RoleId) VALUES
('Tau Mokwena', 'mokwenaT@mail.co.za', 'HASH_PLACEHOLDER_1', (SELECT RoleId FROM Role WHERE RoleName = 'Organizer')),
('Lindiwe Dlamini', 'liddy@mail.co.za', 'HASH_PLACEHOLDER_2', (SELECT RoleId FROM Role WHERE RoleName = 'Organizer')),
('Thabiso Sebopa', 'Lazzy@mail.co.za', 'HASH_PLACEHOLDER_3', (SELECT RoleId FROM Role WHERE RoleName = 'Participant')),
('Lesego Maputla', 'Putla@mail.co.za', 'HASH_PLACEHOLDER_4', (SELECT RoleId FROM Role WHERE RoleName = 'Participan'));

SELECT * FROM [User];