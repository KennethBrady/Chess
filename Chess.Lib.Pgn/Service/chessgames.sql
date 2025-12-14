-- --------------------------------------------------------
-- Host:                         127.0.0.1
-- Server version:               11.4.2-MariaDB - mariadb.org binary distribution
-- Server OS:                    Win64
-- HeidiSQL Version:             12.10.0.7000
-- --------------------------------------------------------

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET NAMES utf8 */;
/*!50503 SET NAMES utf8mb4 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;


-- Dumping database structure for chessgames2
DROP DATABASE IF EXISTS `chessgames2`;
CREATE DATABASE IF NOT EXISTS `chessgames2` /*!40100 DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci */;
USE `chessgames2`;

-- Dumping structure for table chessgames2.game
CREATE TABLE IF NOT EXISTS `game` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `whiteId` int(11) NOT NULL,
  `blackId` int(11) NOT NULL,
  `moves` varchar(12288) NOT NULL,
  `status` tinyint(4) NOT NULL DEFAULT 0,
  `sourceId` int(11) NOT NULL,
  `sourceIndex` int(11) NOT NULL,
  `sourcePos` int(11) NOT NULL,
  `eventDate` date NOT NULL,
  `site` varchar(64) NOT NULL,
  `result` tinyint(4) DEFAULT NULL,
  `openingId` int(11) DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `game_uni1` (`sourceId`,`sourceIndex`,`sourcePos`),
  KEY `whiteId` (`whiteId`),
  KEY `blackId` (`blackId`),
  KEY `game_ibfk_3` (`sourceId`),
  KEY `fk_op` (`openingId`),
  CONSTRAINT `fk_op` FOREIGN KEY (`openingId`) REFERENCES `opening` (`id`),
  CONSTRAINT `game_ibfk_1` FOREIGN KEY (`whiteId`) REFERENCES `player` (`id`),
  CONSTRAINT `game_ibfk_2` FOREIGN KEY (`blackId`) REFERENCES `player` (`id`),
  CONSTRAINT `game_ibfk_3` FOREIGN KEY (`sourceId`) REFERENCES `gamesource` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Data exporting was unselected.

-- Dumping structure for table chessgames2.gamesource
CREATE TABLE IF NOT EXISTS `gamesource` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(64) NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `name` (`name`),
  UNIQUE KEY `name_2` (`name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Data exporting was unselected.

-- Dumping structure for table chessgames2.gametag
CREATE TABLE IF NOT EXISTS `gametag` (
  `gameId` int(11) NOT NULL,
  `tagId` int(11) NOT NULL,
  `value` varchar(256) NOT NULL,
  PRIMARY KEY (`gameId`,`tagId`) USING BTREE,
  KEY `gametag_ibfk_2` (`tagId`) USING BTREE,
  CONSTRAINT `gametag_ibfk_1` FOREIGN KEY (`gameId`) REFERENCES `game` (`id`),
  CONSTRAINT `gametag_ibfk_2` FOREIGN KEY (`tagId`) REFERENCES `tagkey` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Data exporting was unselected.

-- Dumping structure for table chessgames2.opening
CREATE TABLE IF NOT EXISTS `opening` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `code` char(3) NOT NULL,
  `name` varchar(512) NOT NULL,
  `sequence` varchar(256) NOT NULL,
  `moveCount` int(11) NOT NULL DEFAULT 0,
  PRIMARY KEY (`id`) USING BTREE,
  UNIQUE KEY `sequence` (`sequence`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Data exporting was unselected.

-- Dumping structure for table chessgames2.player
CREATE TABLE IF NOT EXISTS `player` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(128) NOT NULL,
  `verified` tinyint(1) NOT NULL DEFAULT 0,
  `fideId` int(11) NOT NULL DEFAULT 0,
  PRIMARY KEY (`id`),
  UNIQUE KEY `name` (`name`,`fideId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Data exporting was unselected.

-- Dumping structure for table chessgames2.tagkey
CREATE TABLE IF NOT EXISTS `tagkey` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(32) NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `name` (`name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Data exporting was unselected.

-- Dumping structure for view chessgames2.duplicatefides
-- Creating temporary table to overcome VIEW dependency errors
CREATE TABLE `duplicatefides` (
	`id` INT(11) NOT NULL,
	`name` VARCHAR(1) NOT NULL COLLATE 'utf8mb4_general_ci',
	`verified` TINYINT(1) NOT NULL,
	`fideId` INT(11) NOT NULL
) ENGINE=MyISAM;

-- Dumping structure for view chessgames2.flagcounts
-- Creating temporary table to overcome VIEW dependency errors
CREATE TABLE `flagcounts` (
	`statusF` TINYINT(4) NOT NULL,
	`COUNT(id)` BIGINT(21) NOT NULL
) ENGINE=MyISAM;

-- Dumping structure for view chessgames2.gameplayers
-- Creating temporary table to overcome VIEW dependency errors
CREATE TABLE `gameplayers` (
	`id` INT(11) NOT NULL,
	`white` VARCHAR(1) NOT NULL COLLATE 'utf8mb4_general_ci',
	`black` VARCHAR(1) NOT NULL COLLATE 'utf8mb4_general_ci',
	`moves` VARCHAR(1) NOT NULL COLLATE 'utf8mb4_general_ci',
	`eventDate` DATE NOT NULL,
	`site` VARCHAR(1) NOT NULL COLLATE 'utf8mb4_general_ci',
	`result` TINYINT(4) NULL
) ENGINE=MyISAM;

-- Dumping structure for view chessgames2.opencounts
-- Creating temporary table to overcome VIEW dependency errors
CREATE TABLE `opencounts` (
	`id` INT(11) NOT NULL,
	`name` VARCHAR(1) NOT NULL COLLATE 'utf8mb4_general_ci',
	`sequence` VARCHAR(1) NOT NULL COLLATE 'utf8mb4_general_ci',
	`# Games` BIGINT(21) NOT NULL
) ENGINE=MyISAM;

-- Dumping structure for view chessgames2.openinggamecounts
-- Creating temporary table to overcome VIEW dependency errors
CREATE TABLE `openinggamecounts` (
	`id` INT(11) NOT NULL,
	`code` CHAR(3) NOT NULL COLLATE 'utf8mb4_general_ci',
	`name` VARCHAR(1) NOT NULL COLLATE 'utf8mb4_general_ci',
	`sequence` VARCHAR(1) NOT NULL COLLATE 'utf8mb4_general_ci',
	`movecount` INT(11) NOT NULL,
	`#Games` BIGINT(21) NOT NULL
) ENGINE=MyISAM;

-- Dumping structure for view chessgames2.openingmovecounts
-- Creating temporary table to overcome VIEW dependency errors
CREATE TABLE `openingmovecounts` (
	`# Moves` INT(11) NOT NULL,
	`# Openings` BIGINT(21) NOT NULL
) ENGINE=MyISAM;

-- Dumping structure for view chessgames2.resultcounts
-- Creating temporary table to overcome VIEW dependency errors
CREATE TABLE `resultcounts` (
	`result` TINYINT(4) NULL,
	`# Games` BIGINT(21) NOT NULL
) ENGINE=MyISAM;

-- Dumping structure for view chessgames2.sitecounts
-- Creating temporary table to overcome VIEW dependency errors
CREATE TABLE `sitecounts` (
	`site` VARCHAR(1) NOT NULL COLLATE 'utf8mb4_general_ci',
	`COUNT(site)` BIGINT(21) NOT NULL
) ENGINE=MyISAM;

-- Dumping structure for procedure chessgames2.mergePlayers
DELIMITER //
CREATE PROCEDURE `mergePlayers`(IN keepId INT, IN mergeId INT)
BEGIN
	DECLARE exit handler FOR sqlexception
	begin
		ROLLBACK;
	END;
	START TRANSACTION;
	UPDATE game SET whiteId=keepId WHERE whiteId=mergeId;
	UPDATE game SET blackId=keepId WHERE blackId=mergeId;
	DELETE FROM player WHERE id=mergeId;
	COMMIT;
END//
DELIMITER ;

-- Dumping structure for procedure chessgames2.playerEloRange
DELIMITER //
CREATE PROCEDURE `playerEloRange`(IN playerId INT)
BEGIN
	DECLARE eloMin INT;
	DECLARE eloMax INT;
	DECLARE bDone INT;
	DECLARE eloVal VARCHAR(8);
	DECLARE elo INT;
	DECLARE curWhite CURSOR FOR SELECT VALUE FROM gametag WHERE tagid=10 AND gameId IN (SELECT id FROM game WHERE whiteId=playerId);
	DECLARE curBlack CURSOR FOR SELECT VALUE FROM gametag WHERE tagid=2 AND gameId IN (SELECT id FROM game WHERE blackId=playerId);
	DECLARE CONTINUE HANDLER FOR NOT FOUND SET bDone = 1;
	OPEN curWhite;
	SET bDone = 0; SET eloMin=10000; SET eloMax=0;
	repeat 
		fetch curWhite INTO eloVal;
		SET elo=CONVERT(eloVal,UNSIGNED);
		if elo < eloMin then
			SET eloMin = elo;
		END if;
		if (elo > eloMax) then
			SET eloMax = elo;
		END if;
	until bDone END repeat;
	close curWhite;
	OPEN curBlack;
	SET bDone=0;
	repeat
		fetch curBlack INTO eloVal;
		#ET elo=CONVERT(eloVal,UNSIGNED);
		if (elo > eloMax) then
			set eloMax = elo;
		END if;
		if (elo < eloMin) then
			set eloMin = elo;
		END if;
	until bDone END repeat;
	close curBlack;
	SELECT eloMin, eloMax;
END//
DELIMITER ;

-- Dumping structure for procedure chessgames2.playerGames
DELIMITER //
CREATE PROCEDURE `playerGames`(IN playerId int)
BEGIN
	SELECT * FROM game WHERE whiteId=playerId OR blackId=playerId;
END//
DELIMITER ;

-- Dumping structure for procedure chessgames2.playersGames
DELIMITER //
CREATE PROCEDURE `playersGames`(
	IN `pid1` INT,
	IN `pid2` INT
)
BEGIN
	SELECT * FROM game WHERE whiteId=pid1 AND blackId=pid2 OR blackId=pid1 AND whiteId=pid2;
END//
DELIMITER ;

-- Removing temporary table and create final VIEW structure
DROP TABLE IF EXISTS `duplicatefides`;
CREATE ALGORITHM=UNDEFINED SQL SECURITY DEFINER VIEW `duplicatefides` AS select `player`.`id` AS `id`,`player`.`name` AS `name`,`player`.`verified` AS `verified`,`player`.`fideId` AS `fideId` from `player` where `player`.`fideId` in (select `player`.`fideId` from `player` where `player`.`fideId` > 0 group by `player`.`fideId` having count(`player`.`fideId`) > 1) order by `player`.`fideId` 
;

-- Removing temporary table and create final VIEW structure
DROP TABLE IF EXISTS `flagcounts`;
CREATE ALGORITHM=UNDEFINED SQL SECURITY DEFINER VIEW `flagcounts` AS select `game`.`status` AS `statusF`,count(`game`.`id`) AS `COUNT(id)` from `game` group by `game`.`status` order by 1 
;

-- Removing temporary table and create final VIEW structure
DROP TABLE IF EXISTS `gameplayers`;
CREATE ALGORITHM=UNDEFINED SQL SECURITY DEFINER VIEW `gameplayers` AS select `g`.`id` AS `id`,`pw`.`name` AS `white`,`pb`.`name` AS `black`,`g`.`moves` AS `moves`,`g`.`eventDate` AS `eventDate`,`g`.`site` AS `site`,`g`.`result` AS `result` from ((`game` `g` join `player` `pw` on(`g`.`whiteId` = `pw`.`id`)) join `player` `pb` on(`g`.`blackId` = `pb`.`id`)) where `g`.`status` = 1 order by `g`.`id` 
;

-- Removing temporary table and create final VIEW structure
DROP TABLE IF EXISTS `opencounts`;
CREATE ALGORITHM=UNDEFINED SQL SECURITY DEFINER VIEW `opencounts` AS select `o`.`id` AS `id`,`o`.`name` AS `name`,`o`.`sequence` AS `sequence`,count(`g`.`id`) AS `# Games` from (`opening` `o` join `game` `g` on(`o`.`id` = `g`.`openingId`)) group by `g`.`openingId` 
;

-- Removing temporary table and create final VIEW structure
DROP TABLE IF EXISTS `openinggamecounts`;
CREATE ALGORITHM=UNDEFINED SQL SECURITY DEFINER VIEW `openinggamecounts` AS select `o`.`id` AS `id`,`o`.`code` AS `code`,`o`.`name` AS `name`,`o`.`sequence` AS `sequence`,`o`.`moveCount` AS `movecount`,count(`g`.`openingId`) AS `#Games` from (`game` `g` join `opening` `o` on(`o`.`id` = `g`.`openingId`)) where octet_length(`o`.`sequence`) > 0 group by `o`.`id` order by `o`.`name` 
;

-- Removing temporary table and create final VIEW structure
DROP TABLE IF EXISTS `openingmovecounts`;
CREATE ALGORITHM=UNDEFINED SQL SECURITY DEFINER VIEW `openingmovecounts` AS select `opening`.`moveCount` AS `# Moves`,count(`opening`.`id`) AS `# Openings` from `opening` group by `opening`.`moveCount` order by 2 desc 
;

-- Removing temporary table and create final VIEW structure
DROP TABLE IF EXISTS `resultcounts`;
CREATE ALGORITHM=UNDEFINED SQL SECURITY DEFINER VIEW `resultcounts` AS select `game`.`result` AS `result`,count(`game`.`id`) AS `# Games` from `game` group by `game`.`result` order by 1 
;

-- Removing temporary table and create final VIEW structure
DROP TABLE IF EXISTS `sitecounts`;
CREATE ALGORITHM=UNDEFINED SQL SECURITY DEFINER VIEW `sitecounts` AS select `game`.`site` AS `site`,count(`game`.`site`) AS `COUNT(site)` from `game` group by `game`.`site` order by 2 desc 
;

/*!40103 SET TIME_ZONE=IFNULL(@OLD_TIME_ZONE, 'system') */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;
