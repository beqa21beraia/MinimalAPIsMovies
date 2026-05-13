-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[Movies_GetById]
	-- Add the parameters for the stored procedure here
	@id int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	select *
	from Movies
	where Id = @id

	select * 
	from Comments
	where MovieId = @id

	select Id, Name
	from Genres
	inner join GenresMovies
	on GenresMovies.GenreId = Id
	where MovieId = @id; 

	select Id, Name, Character
	from Actors
	inner join ActorsMovies
	on ActorsMovies.ActorId = Id
	where MovieId = @id
	order by [Order]
END
 