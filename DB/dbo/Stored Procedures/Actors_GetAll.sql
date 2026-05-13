-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[Actors_GetAll]
	-- Add the parameters for the stored procedure here
	@page int,
	@recordsPerPage int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	select Id, Name, DateOfBirth, Picture
	from Actors
	order by Name
	offset ((@page - 1) * @recordsPerPage) rows
	fetch next @recordsPerPage rows only
END
