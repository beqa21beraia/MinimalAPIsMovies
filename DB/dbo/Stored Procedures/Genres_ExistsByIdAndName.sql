-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE Genres_ExistsByIdAndName
	-- Add the parameters for the stored procedure here
	@id int,
	@name nvarchar(50)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	if exists (select 1 from Genres where Id != @id and Name = @name)
		select 1
	else
		select 0
END
