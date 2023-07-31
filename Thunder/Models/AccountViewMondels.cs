namespace Thunder.Models
{
    public class IsPassValid
    {
        public bool isValid { get; set; }   
    }

    public class UserDeets
    {
        public string FullName { get; set; }
        public string Email { get; set; }
    }
    public class UserToUpdate
    {
        public string email { get; set; }
        public string balance { get; set; }
    }
  
}
