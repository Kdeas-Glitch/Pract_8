using Pract_8_Demin.Models;

namespace Pract_8_Demin.Service
{
    public class DBService
    {
        private DBService()
        {
            context = new DeminDbContext();
        }
        private static DBService? instance;
        public static DBService Instance
        {
            get
            {
                if (instance == null)
                    instance = new DBService();
                return instance;
            }
        }
        private DeminDbContext context;
        public DeminDbContext Context => context;
    }
}
