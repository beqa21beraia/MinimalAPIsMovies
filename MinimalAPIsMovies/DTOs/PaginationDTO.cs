namespace MinimalAPIsMovies.DTOs
{
    public class PaginationDTO
    {
        public int Page { get; set; }
        public int recordsPerPage = 10;
        public int recordsPerPageMax = 50;

        public int RecordsPerPage
        {
            get
            {
                return recordsPerPage;
            }
            set
            {
                if (value > recordsPerPageMax)
                {
                    recordsPerPage = recordsPerPageMax;
                }
                else
                {
                    recordsPerPage = value;
                }
            }
        }
    }
}
