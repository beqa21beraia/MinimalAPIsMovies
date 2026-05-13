-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE Movies_Create 
	-- Add the parameters for the stored procedure here
	@title nvarchar(150),
	@inTheaters bit,
	@releaseDate datetime2,
	@poster nvarchar(max)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	insert into Movies (Title, InTheaters, ReleaseDate, Poster)
	values (@title, @inTheaters, @releaseDate, @poster)

	select SCOPE_IDENTITY()
END
