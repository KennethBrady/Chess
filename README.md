# Chess

# Libraries

## Sql.Lib 
Provides tools for operating on SQL databases and for creating on-the-fly SQL clauses.
The DBTableAttribute associates a C# record type with a database table.  It is easiest to name record fields the same as the table fields, though the FieldMapping can be used to map between differing names.  Since MySql does not gracefully handle the unescaped path character '\' in strings, the FilePathFields tells the library which fields need to replace with the '/' character.

The current implementation of the ISqlService interface is specialized for MySql, but implementations for other providers is not difficult.

## Chess.Lib
Provides core interfaces ad functionality for representing chess games.

The *Hardware* namespace defines a Board and the Square objects that compose it. Rank and File enumerations are as expected for chess, and the RankFile struct defines a unique position on the chess board.

The *Pieces" namespace defines interfaces and implementations representing each of the chess pieces.

The *Moves" namespace defines interfaces and implementations representing a chess move or a collection thereof.

The *Moves/Parsing* namespace provides functionality for converting string representations of moves (either Algebraic notation or Engine notation) into logical chess moves that can be applied to the pieces on a board.

The *Games* namespace defines interfaces and implementations representing both read-only and interactive chess games.

## Chess.Lib.Pgn
Provides tools for parsing PGN files and storing the games in a database.

The *Parsing* namespace defines the static class PgnSourceParser which converts single-game or multi-game PGN files into a GameImport structure which readied for import into a database.
The *DataModel* namespace defines records corresponding to the database tables.
The *Service* namespace provides chessgames.sql for defining the empty database and chessgames_data.sql which includes populated opening and tagkey tables.

## Common.Lib
Provides a few interfaces, extensions and types that are used in the other projects.

## UnitTests
Unit tests are provided for both Sql.Lib and Chess.Lib.

## Examples Folder
This folder contains examples of how to use the library.
