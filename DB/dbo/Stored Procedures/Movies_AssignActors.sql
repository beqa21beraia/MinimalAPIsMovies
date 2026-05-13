-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE Movies_AssignActors 
	-- Add the parameters for the stored procedure here
	@movieId int,
	@actors ActorsList readonly
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	delete ActorsMovies where MovieId = @movieId

	insert into ActorsMovies(ActorId, MovieId, [Order], [Character])
	select ActorId, @movieId, [Order], [Character] from @actors
END
