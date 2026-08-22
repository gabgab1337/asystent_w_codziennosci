USE [2309_Assistant]
GO

TRUNCATE TABLE [2309_Assistant].[2309_admin].[DonePoints];
TRUNCATE TABLE [2309_Assistant].[2309_admin].[DoneTasks];

TRUNCATE TABLE [2309_Assistant].[2309_admin].[TriggerPoints];
TRUNCATE TABLE [2309_Assistant].[2309_admin].[TriggerTasks];

DELETE FROM [2309_Assistant].[2309_admin].[TaskPoints];
DELETE FROM  [2309_Assistant].[2309_admin].[Tasks];
DELETE FROM  [2309_Assistant].[2309_admin].[Trigers];
DELETE FROM  [2309_Assistant].[2309_admin].[Users];
GO

SET IDENTITY_INSERT [2309_Assistant].[2309_admin].[Users] ON 
GO

INSERT INTO [2309_Assistant].[2309_admin].[Users]
           ([Id]
		   ,[Username]
           ,[Password]
           ,[IsActive]
           ,[Type]
           ,[CaregiverId]
           ,[SelectedASD]
           ,[JoiningDate]
		   ,[ColorsPalete]
		   )
     VALUES
(1,'Opiekun','e2d120f9af4b7ff454f1cf1f98072a4e4f688ddeb63e4eed5b8e8ce55fc4a873',0,1,0,1026,'2024-10-05 17:37:09.3400000', null),
(2,'Test','532eaabd9574880dbf76b9b8cc00832c20a6ec113d682299550d7a6e0f345e25',0,1,0,NULL,'2024-10-05 17:37:09.3400000', null),
(1004,'Admin','c1c224b03cd9bc7b6a86d77f5dace40191766c485cd55dc48caf9ac873335d6f',0,0,NULL,NULL,'2024-10-05 17:37:09.3400000', null),
(1005,'asd','688787d8ff144c502c7f5cffaafe2cc588d86079f9de88304c26b0cb99ce91c6',0,2,1,NULL,'2024-10-05 17:37:09.3400000', null),
(1006,'Jan2','5c5db120cb11bee138ff3143edcbedaead684de7a0ba140e12287d436c5dc487',0,0,1,NULL,'2024-10-05 17:37:09.3400000', null),
(1008,'Test1','8a863b145dc6e4ed7ac41c08f7536c476ebac7509e028ed2b49f8bd5a3562b9f',0,2,1,NULL,'2024-10-05 17:37:09.3400000', null),
(1019,'OpiekunTest','eedc394dbc9b46294a382ffd700f9c97252a6233bf53bb8476cf72ff18e920d2',0,1,NULL,1020,'2024-10-05 17:37:09.3400000', null),
(1026,'Kacper','554953124e78ce04bcca05ae17882ae59f646202cd28d1b776eb69f91309ad4e',0,2,1,NULL,'2024-10-28 11:40:34.9238437', null);


SET IDENTITY_INSERT [2309_Assistant].[2309_admin].[Users] OFF 
GO


SET IDENTITY_INSERT [2309_Assistant].[2309_admin].[Trigers] ON 
GO

INSERT INTO [2309_Assistant].[2309_admin].[Trigers] 
     ([Id]
    ,[Name]
    ,[Type]
    ,[SubType]
    ,[Data]
    ,[CaregiverId])
VALUES
	(2012,'Weekend',1,6,NULL,1),
	(2013,'Dzieñ roboczy',1,5,NULL,1),
	(2014,'Poniedzia³ek',1,3,'1',1),
	(2015,'Wtorek',1,3,'2',1),
	(3014,'Wiosenna pogoda',2,0,'14;24;0;12',1),
	(3015,'letnia pogoda',2,0,'25;40;0;12',1),
	(4012,'Œroda',1,3,'3',1),
	(4013,'Czwartek',1,3,'4',1),
	(4014,'Pi¹tek',1,3,'5',1),
	(4015,'Sobota',1,3,'6',1),
	(4016,'Niedziela',1,3,'7',1),
	(6013,'Urodziny',1,4,'11.12.2024 09:12:00;9',1),
	(6014,'Wigilia',1,4,'24.12.2024 10:26:00;9',1),
	(6015,'1 Dzieñ Bo¿ego Narodzenia',1,4,'25.12.2024 00:00:00;9',1),
	(6016,'2 Dzieñ Bo¿ego Narodzenia',1,4,'26.12.2024 00:00:00;9',1),
	(6017,'Przerwa œwi¹t Bo¿ego Narodzenia na uczelni 2024',1,2,'21.12.2024 00:00:00;06.01.2025 00:00:00',1);

SET IDENTITY_INSERT [2309_Assistant].[2309_admin].[Trigers] OFF 
GO

SET IDENTITY_INSERT [2309_Assistant].[2309_admin].[Tasks] ON 
GO

INSERT INTO [2309_Assistant].[2309_admin].[Tasks]
           ([Id]
		   ,[Name]
           ,[Description]
           ,[CaregiverId]
           ,[AsdPersonId]
           ,[AnchorBegin]
           ,[AnchorEnd]
           ,[Time]
           ,[TimeBegin]
           ,[TimeEnd]
           ,[Number])
     VALUES
	 (3026,'Pobudka',NULL,1,1026,1,0,10,420,NULL,1),
(3027,'Toaleta poranna',NULL,1,1026,0,0,10,NULL,NULL,5),
(3028,'Œniadanie',NULL,1,1026,0,0,40,NULL,NULL,9),
(3029,'Poranne obowi¹zki domowe',NULL,1,1026,0,0,15,NULL,NULL,10),
(3031,'Korepetycje,Korepetycje z programownia',NULL,1,1026,1,0,120,540,NULL,13),
(3033,'Czytanie wyk³adów',NULL,1,1026,0,0,60,NULL,NULL,15),
(3034,'Obiad',NULL,1,1026,0,0,60,NULL,NULL,17),
(3035,'Odrabianie zadañ laboratoryjnych',NULL,1,1026,0,0,120,NULL,NULL,20),
(3038,'Korepetycje',NULL,1,1026,1,1,120,1080,1200,21),
(3040,'Toaleta wieczorna',NULL,1,1026,1,0,15,1365,NULL,55),
(3041,'Koniec dnia',NULL,1,1026,0,0,2,1380,NULL,56),
(3042,'Dojazd na uczelniê',NULL,1,1026,1,1,45,480,555,11),
(3043,'Zajêcia na uczelni',NULL,1,1026,1,1,90,555,645,12),
(5024,'Pobudka',NULL,1,1026,1,0,10,540,NULL,2),
(5026,'Kolacja',NULL,1,1026,1,0,30,1140,NULL,53),
(5028,'Zakupy',NULL,1,1026,0,0,60,NULL,NULL,19),
(5029,'Obiad',NULL,1,1026,1,0,30,770,NULL,22),
(5030,'Zajêcia na uczelni',NULL,1,1026,1,1,150,825,975,23),
(5031,'Powrót do domu',NULL,1,1026,0,0,45,NULL,NULL,24),
(5032,'Dojazd do psychologa',NULL,1,1026,1,0,15,495,NULL,25),
(5033,'Wizyta u psychologa',NULL,1,1026,1,1,45,510,555,26),
(5034,'Dojazd na uczelnie',NULL,1,1026,0,0,40,NULL,NULL,27),
(5035,'Zajêcia na uczelni',NULL,1,1026,1,1,85,605,690,28),
(5036,'Powrót do domu',NULL,1,1026,0,0,40,NULL,NULL,30),
(5037,'Obiad',NULL,1,1026,0,0,60,NULL,NULL,31),
(5038,'Korepetycje',NULL,1,1026,1,1,90,840,930,34),
(5039,'Czytanie wyk³adów',NULL,1,1026,0,0,120,NULL,NULL,32),
(5041,'Dojazd na uczelniê',NULL,1,1026,1,0,45,690,NULL,35),
(5042,'Zajêcia na uczelni',NULL,1,1026,1,1,90,750,840,36),
(5043,'Obiad',NULL,1,1026,0,0,40,NULL,NULL,37),
(5044,'Zajêcia na uczelni',NULL,1,1026,1,1,250,880,1130,38),
(5045,'Powrót do domu',NULL,1,1026,0,0,60,NULL,NULL,39),
(5046,'Dojazd na uczelnie',NULL,1,1026,1,0,60,435,NULL,40),
(5047,'Zajêcia na uczelni',NULL,1,1026,1,1,315,500,815,41),
(5048,'Powrót do domu',NULL,1,1026,0,0,60,NULL,NULL,42),
(5049,'Obiad',NULL,1,1026,0,0,60,NULL,NULL,43),
(5050,'Wyjœcie do sklepu',NULL,1,1026,0,0,60,NULL,NULL,51),
(5052,'Sprz¹tanie',NULL,1,1026,0,0,60,NULL,NULL,44),
(5053,'Zakupy kie³bas na ulicy Wygonowej',NULL,1,1026,0,0,30,NULL,NULL,45),
(5054,'Korepetycje',NULL,1,1026,1,1,90,720,810,46),
(5055,'Obiad',NULL,1,1026,1,0,60,840,NULL,47),
(5056,'Zakupy',NULL,1,1026,0,0,60,NULL,NULL,52),
(5057,'Prasowanie',NULL,1,1026,0,0,60,NULL,NULL,48),
(5058,'Obiad',NULL,1,1026,1,0,60,840,NULL,49),
(5059,'Pobudka',NULL,1,1026,1,0,10,380,NULL,3),
(5060,'Pobudka',NULL,1,1026,1,0,10,540,NULL,4),
(9028,'Kolacja',NULL,1,1026,1,0,30,1200,NULL,54),
(9029,'Kolacja',NULL,1,1026,1,0,30,1050,NULL,33),
(9030,'Obiad w Wigilie',NULL,1,1026,1,0,60,900,NULL,50);

GO

SET IDENTITY_INSERT [2309_Assistant].[2309_admin].[Tasks] OFF 
GO

SET IDENTITY_INSERT [2309_Assistant].[2309_admin].[TaskPoints] ON 
GO

INSERT INTO [2309_Assistant].[2309_admin].[TaskPoints]
          ([ID]
		   ,[Name]
           ,[Description]
           ,[MinTime]
           ,[AvgTime]
           ,[Number]
           ,[TaskId]
		   ,[PhotoUrl])
     VALUES
(4032,'Ubranie',NULL,0,0,1,3026,NULL),
(4033,'Poœcielenie ³ó¿ka',NULL,0,0,2,3026,NULL),
(4034,'Mycie zêbów',NULL,0,0,1,3027,NULL),
(4035,'Higiena intymna',NULL,0,0,2,3027,NULL),
(4036,'Przyrz¹dzenie œniadania',NULL,0,0,1,3028,NULL),
(4037,'Zjedzenie posi³ku',NULL,0,0,2,3028,NULL),
(4040,'Przygotowanie obiadu',NULL,0,0,1,3034,NULL),
(4041,'Zjedzenie obiadu',NULL,0,0,2,3034,NULL),
(4042,'Mycie zêbów',NULL,0,0,1,3040,NULL),
(4043,'Przejœcie na przystanek',NULL,0,0,1,3042,NULL),
(4044,'Jazda autobusem',NULL,0,0,2,3042,NULL),
(4045,'Dojœcie na uczelniê',NULL,0,0,3,3042,NULL),
(6032,'Za¿ycie leków',NULL,0,0,3,3028,NULL),
(6033,'SprawdŸ kosz na œmieci jeœli jest pe³en, wyrzuæ',NULL,0,0,1,3029,NULL),
(6034,'Nastawienie budzika na nastêpny dzieñ na 7:00',NULL,0,0,1,3041,NULL),
(6035,'Sen',NULL,0,0,4,3041,NULL),
(6036,'Ubieranie',NULL,0,0,1,5024,NULL),
(6037,'Poœcielenie ³ó¿ka',NULL,0,0,2,5024,NULL),
(6039,'Wypakowanie naczyñ ze zmywarki',NULL,0,0,3,3029,NULL),
(6040,'Nastawienie budzika na nastêpny dzieñ na 9:00',NULL,0,0,2,3041,NULL),
(6041,'Nastawienie budzika na nastêpny dzieñ na 6:30',NULL,0,0,3,3041,NULL),
(6042,'K¹piel',NULL,0,0,2,3040,NULL),
(6043,'Uruchomienie zmywarki',NULL,0,0,2,3029,NULL),
(6044,'W³¹czenie prania',NULL,0,0,4,3029,NULL);
GO

SET IDENTITY_INSERT [2309_Assistant].[2309_admin].[TaskPoints] OFF 
GO

INSERT INTO [2309_Assistant].[2309_admin].[TriggerTasks]
           ([TaskId]
           ,[TriggerId]
		   ,[Negation])
     VALUES
(3026,2014,0),
(3031,2014,0),
(3033,2014,0),
(3034,2014,0),
(3035,2014,0),
(3038,2014,0),
(5028,2014,0),
(9028,2014,0),
(3026,2015,0),
(3042,2015,0),
(3043,2015,0),
(5026,2015,0),
(5029,2015,0),
(5030,2015,0),
(5031,2015,0),
(3026,4012,0),
(5032,4012,0),
(5033,4012,0),
(5034,4012,0),
(5035,4012,0),
(5036,4012,0),
(5037,4012,0),
(5038,4012,0),
(5039,4012,0),
(9029,4012,0),
(5024,4013,0),
(5026,4013,0),
(5041,4013,0),
(5042,4013,0),
(5043,4013,0),
(5044,4013,0),
(5045,4013,0),
(5026,4014,0),
(5046,4014,0),
(5047,4014,0),
(5048,4014,0),
(5049,4014,0),
(5050,4014,0),
(5059,4014,0),
(5026,4015,0),
(5052,4015,0),
(5053,4015,0),
(5054,4015,0),
(5055,4015,0),
(5056,4015,0),
(5060,4015,0),
(5026,4016,0),
(5057,4016,0),
(5058,4016,0),
(5060,4016,0),
(9030,6014,0),
(3042,6017,1),
(3043,6017,1),
(5029,6017,1),
(5030,6017,1),
(5031,6017,1),
(5034,6017,1),
(5035,6017,1),
(5036,6017,1),
(5041,6017,1),
(5042,6017,1),
(5044,6017,1),
(5045,6017,1),
(5046,6017,1),
(5047,6017,1),
(5048,6017,1);
GO

INSERT INTO [2309_Assistant].[2309_admin].[TriggerPoints] 
           ([PointId]
           ,[TriggerId]
		   ,[Negation])
     VALUES
(6034,2014,0),
(6043,2014,0),
(6034,2015,0),
(6039,2015,0),
(6040,4012,0),
(6043,4012,0),
(6039,4013,0),
(6041,4013,0),
(6040,4014,0),
(6040,4015,0),
(6034,4016,0);
GO

