-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE Movies_GetAll
	-- Add the parameters for the stored procedure here
	@page int,
	@recordsPerPage int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	select * 
	from Movies
	order by Id
	offset ((@page - 1) * @recordsPerPage) rows
	fetch next @recordsPerPage rows only
END
