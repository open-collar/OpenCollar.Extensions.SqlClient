CREATE PROCEDURE [dbo].[TestProc1]
    @param1 int = 0,
    @param2 int
AS
    SELECT * FROM TestTable1
    SELECT * FROM TestTable1
RETURN 0
