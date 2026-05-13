-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE Errors_Create
	-- Add the parameters for the stored procedure here
	@id uniqueidentifier,
	@errorMessage nvarchar(max),
	@stackTrace nvarchar(max),
	@date datetime2
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	insert into Errors(Id, ErrorMessage, StackTrace, Date)
	values (@id, @errorMessage, @stackTrace, @date)
	
END
