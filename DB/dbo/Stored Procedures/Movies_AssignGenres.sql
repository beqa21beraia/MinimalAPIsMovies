-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE Movies_AssignGenres 
	-- Add the parameters for the stored procedure here
	@movieId int,
	@genresIds IntegersList readonly
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	delete GenresMovies where MovieId = @movieId

	insert into GenresMovies(GenreId, MovieId)
	select Id, @movieId from @genresIds
END
